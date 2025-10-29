#!/bin/bash

# 🎯 Setup Completo do Servidor - Comandos Prontos
# Execute este arquivo no servidor Contabo

echo "🚀 Setup automático do servidor para API Aure"

# Verificar se é Ubuntu/Debian
if ! command -v apt-get &> /dev/null; then
    echo "❌ Este script é para Ubuntu/Debian apenas"
    exit 1
fi

# Verificar se está como root ou com sudo
if [ "$EUID" -ne 0 ]; then
    echo "Execute com sudo: sudo ./setup-complete.sh"
    exit 1
fi

# Atualizar sistema
echo "📦 Atualizando sistema..."
apt update && apt upgrade -y

# Instalar Docker de forma mais simples
echo "🐳 Instalando Docker..."
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
usermod -aG docker $USER

# Instalar docker-compose
echo "🔗 Instalando Docker Compose..."
apt install -y docker-compose

# Instalar Nginx e Certbot
echo "🌐 Instalando Nginx e SSL..."
apt install -y nginx certbot python3-certbot-nginx

# Habilitar serviços
systemctl enable docker
systemctl enable nginx
systemctl start docker
systemctl start nginx

# Configurar firewall básico
echo "🛡️  Configurando firewall..."
ufw --force reset
ufw default deny incoming
ufw default allow outgoing  
ufw allow ssh
ufw allow 80
ufw allow 443
ufw --force enable

# Criar usuário para aplicação (opcional)
if ! id "aure" &>/dev/null; then
    echo "👤 Criando usuário aure..."
    useradd -m -s /bin/bash aure
    usermod -aG docker aure
fi

# Criar diretório da aplicação
echo "📁 Preparando diretório..."
mkdir -p /opt/aure
chown -R aure:aure /opt/aure

echo ""
echo "✅ Servidor configurado!"
echo ""
echo "🎯 Próximos passos:"
echo ""
echo "1. 📥 Clonar repositório:"
echo "   cd /opt/aure"
echo "   git clone https://github.com/GabrielSelim/Aure.git ."
echo ""
echo "2. 🚀 Deploy da aplicação:"
echo "   chmod +x deploy-simple.sh"
echo "   ./deploy-simple.sh"
echo ""  
echo "3. 🌐 Configurar domínio (opcional):"
echo "   # Configure DNS aureapi.gabrielsanztech.com.br -> IP_DO_SERVIDOR"
echo "   sudo cp nginx-aureapi.conf /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br"
echo "   sudo ln -s /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br /etc/nginx/sites-enabled/"
echo "   sudo nginx -t"
echo "   sudo systemctl reload nginx"
echo ""
echo "4. 🔐 SSL gratuito (opcional):"
echo "   sudo certbot --nginx -d aureapi.gabrielsanztech.com.br"
echo ""
echo "🔄 Reinicie o servidor: sudo reboot"
echo ""
echo "🎉 Depois disso, sua API estará rodando!"