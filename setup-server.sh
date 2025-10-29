#!/bin/bash

# 🔧 Script de Configuração do Servidor - API Aure
# Execute este script PRIMEIRO no servidor Contabo

set -e

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] ⚠️  $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ❌ $1${NC}"
}

# Verificar se está rodando como root
if [ "$EUID" -ne 0 ]; then
    error "Execute este script como root: sudo ./setup-server.sh"
    exit 1
fi

log "🚀 Configurando servidor Contabo para API Aure..."

# Atualizar sistema
log "📦 Atualizando sistema..."
apt update
apt upgrade -y

# Instalar dependências essenciais
log "🔧 Instalando dependências..."
apt install -y \
    curl \
    wget \
    git \
    htop \
    nano \
    unzip \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release \
    ufw \
    fail2ban \
    logrotate

# Instalar Docker
log "🐳 Instalando Docker..."
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
apt update
apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Instalar Docker Compose (versão standalone)
log "🔗 Instalando Docker Compose..."
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Habilitar e iniciar Docker
systemctl enable docker
systemctl start docker

# Adicionar usuário atual ao grupo docker
if [ -n "$SUDO_USER" ]; then
    usermod -aG docker $SUDO_USER
    log "✅ Usuário $SUDO_USER adicionado ao grupo docker"
fi

# Instalar Nginx
log "🌐 Instalando Nginx..."
apt install -y nginx

# Instalar Certbot para SSL
log "🔐 Instalando Certbot (SSL)..."
apt install -y certbot python3-certbot-nginx

# Configurar Firewall
log "🛡️  Configurando firewall..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing
ufw allow ssh
ufw allow 80/tcp
ufw allow 443/tcp
# Portas do Docker (apenas localhost)
ufw allow from 127.0.0.1 to any port 5000
ufw allow from 127.0.0.1 to any port 5432
ufw allow from 127.0.0.1 to any port 6379
ufw --force enable

# Configurar Fail2Ban
log "🔒 Configurando Fail2Ban..."
cat > /etc/fail2ban/jail.local << 'EOF'
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5
destemail = admin@gabrielsanztech.com.br
sendername = Fail2Ban-Contabo
mta = sendmail

[sshd]
enabled = true
port = ssh
logpath = /var/log/auth.log
maxretry = 3
bantime = 7200

[nginx-http-auth]
enabled = true
filter = nginx-http-auth
port = http,https
logpath = /var/log/nginx/aureapi_error.log

[nginx-limit-req]
enabled = true
filter = nginx-limit-req
port = http,https
logpath = /var/log/nginx/aureapi_error.log
maxretry = 10
EOF

systemctl restart fail2ban
systemctl enable fail2ban

# Criar diretórios da aplicação
log "📁 Criando estrutura de diretórios..."
mkdir -p /opt/aure
mkdir -p /opt/aure-backup
mkdir -p /var/log/aure

# Configurar logrotate para aplicação
log "📋 Configurando rotação de logs..."
cat > /etc/logrotate.d/aure << 'EOF'
/opt/aure/logs/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 0644 www-data www-data
    sharedscripts
    postrotate
        docker-compose -f /opt/aure/docker-compose.prod.yml restart aure-api >/dev/null 2>&1 || true
    endscript
}

/var/log/nginx/aureapi_*.log {
    daily
    missingok
    rotate 52
    compress
    delaycompress
    notifempty
    sharedscripts
    postrotate
        if [ -f /var/run/nginx.pid ]; then
            kill -USR1 `cat /var/run/nginx.pid`
        fi
    endscript
}
EOF

# Configurar timezone
log "🕐 Configurando timezone..."
timedatectl set-timezone America/Sao_Paulo

# Configurar swap se não existir
if [ ! -f /swapfile ]; then
    log "💾 Criando arquivo de swap (2GB)..."
    fallocate -l 2G /swapfile
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

# Otimizações do sistema
log "⚡ Aplicando otimizações do sistema..."
cat >> /etc/sysctl.conf << 'EOF'

# Otimizações para API web
net.core.rmem_max = 134217728
net.core.wmem_max = 134217728
net.core.netdev_max_backlog = 5000
net.core.somaxconn = 65535
net.ipv4.tcp_congestion_control = bbr
net.ipv4.tcp_window_scaling = 1
net.ipv4.tcp_rmem = 4096 87380 134217728
net.ipv4.tcp_wmem = 4096 65536 134217728
EOF

sysctl -p

# Configurar limites do sistema
cat >> /etc/security/limits.conf << 'EOF'
* soft nofile 65535
* hard nofile 65535
* soft nproc 65535
* hard nproc 65535
EOF

# Instalar ferramentas de monitoramento
log "📊 Instalando ferramentas de monitoramento..."
apt install -y htop iotop nethogs ncdu tree

# Criar script de monitoramento
cat > /opt/monitor-aure.sh << 'EOF'
#!/bin/bash
echo "🖥️  Status do Sistema Aure - $(date)"
echo "=================================================="
echo "💾 Uso de Disco:"
df -h | grep -E '^/dev|^Filesystem'
echo ""
echo "🧠 Uso de Memória:"
free -h
echo ""
echo "🚀 Load Average:"
uptime
echo ""
echo "🐳 Status dos Containers:"
cd /opt/aure && docker-compose -f docker-compose.prod.yml ps
echo ""
echo "📊 API Health:"
curl -s http://localhost:5000/health | jq . 2>/dev/null || echo "API não está respondendo"
echo ""
echo "📋 Últimos logs da API (5 linhas):"
cd /opt/aure && docker-compose -f docker-compose.prod.yml logs --tail=5 aure-api
EOF

chmod +x /opt/monitor-aure.sh

# Criar cron job para limpeza automática
cat > /etc/cron.d/aure-maintenance << 'EOF'
# Limpeza automática do sistema Aure
0 2 * * * root docker system prune -f >/dev/null 2>&1
0 3 * * 0 root find /opt/aure-backup* -mtime +30 -delete >/dev/null 2>&1
EOF

# Informações finais
log "🎉 Configuração do servidor concluída!"
echo
log "📋 Próximos passos:"
echo -e "   1. ${BLUE}Configure o DNS do domínio aureapi.gabrielsanztech.com.br${NC}"
echo -e "   2. ${BLUE}Obtenha certificado SSL:${NC}"
echo -e "      sudo certbot --nginx -d aureapi.gabrielsanztech.com.br"
echo -e "   3. ${BLUE}Clone o repositório:${NC}"
echo -e "      cd /opt/aure && git clone https://github.com/GabrielSelim/Aure.git ."
echo -e "   4. ${BLUE}Configure o Nginx:${NC}"
echo -e "      sudo cp nginx-aureapi.conf /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br"
echo -e "      sudo ln -s /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br /etc/nginx/sites-enabled/"
echo -e "      sudo nginx -t && sudo systemctl reload nginx"
echo -e "   5. ${BLUE}Execute o deploy:${NC}"
echo -e "      chmod +x deploy.sh && sudo ./deploy.sh"
echo
log "🔧 Comandos úteis instalados:"
echo -e "   Monitor: ${BLUE}/opt/monitor-aure.sh${NC}"
echo -e "   Logs: ${BLUE}tail -f /var/log/nginx/aureapi_error.log${NC}"
echo -e "   Docker: ${BLUE}docker-compose -f docker-compose.prod.yml logs -f${NC}"
echo
warn "🔄 Reinicie o servidor para aplicar todas as configurações:"
warn "sudo reboot"
echo
log "✅ Servidor configurado e pronto para deploy!"