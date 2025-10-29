# 🚀 Deploy da API Aure no Servidor Contabo

## 📋 Configurações Necessárias para Produção

### 1. Arquivo: `appsettings.Production.json`

Vou atualizar as configurações de produção para o domínio `aureapi.gabrielsanztech.com.br`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/aure-api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "AllowedHosts": "aureapi.gabrielsanztech.com.br,*.gabrielsanztech.com.br",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aure_production;Username=aure_user;Password=SUA_SENHA_SEGURA_AQUI;Include Error Detail=true;Pooling=true;MinPoolSize=10;MaxPoolSize=100;"
  },
  "JwtSettings": {
    "SecretKey": "GERE_UMA_CHAVE_MUITO_SEGURA_DE_256_BITS_AQUI",
    "Issuer": "Aure.API",
    "Audience": "Aure.WebApp",
    "ExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  },
  "SefazSettings": {
    "Environment": "Producao",
    "State": "SP",
    "CertificatePath": "/app/certificates/certificado_producao.pfx",
    "CertificatePassword": "SENHA_DO_CERTIFICADO_AQUI",
    "WebServiceTimeout": 30000
  },
  "InvoiceSettings": {
    "DefaultSeries": 1,
    "DefaultNatureOperation": "Venda de mercadoria adquirida ou recebida de terceiros",
    "EnablePdfGeneration": true,
    "PdfTemplate": "default"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": false,
    "Username": "aurecontroll@gmail.com",
    "Password": "wotorcxxfepwxvvu",
    "FromEmail": "aurecontroll@gmail.com",
    "FromName": "Sistema Aure",
    "BaseUrl": "https://aureapi.gabrielsanztech.com.br"
  },
  "Encryption": {
    "Key": "GERE_UMA_CHAVE_BASE64_AQUI",
    "IV": "GERE_UM_IV_BASE64_AQUI"
  },
  "BaseUrl": "https://aureapi.gabrielsanztech.com.br"
}
```

### 2. Configuração CORS para Produção

Preciso atualizar o CORS para aceitar apenas domínios autorizados:

**Arquivo**: `ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddCors(options =>
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environment == "Production")
        {
            options.AddPolicy("AllowSpecificOrigins", builder =>
            {
                builder
                    .WithOrigins(
                        "https://aure.gabrielsanztech.com.br",
                        "https://app.gabrielsanztech.com.br"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        }
        else
        {
            // Desenvolvimento - permite tudo
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        }
    });

    return services;
}
```

### 3. Dockerfile para Produção

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Instalar dependências do sistema
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/Aure.API/Aure.API.csproj", "src/Aure.API/"]
COPY ["src/Aure.Application/Aure.Application.csproj", "src/Aure.Application/"]
COPY ["src/Aure.Domain/Aure.Domain.csproj", "src/Aure.Domain/"]
COPY ["src/Aure.Infrastructure/Aure.Infrastructure.csproj", "src/Aure.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "src/Aure.API/Aure.API.csproj"

# Copiar código fonte
COPY . .
WORKDIR "/src/src/Aure.API"

# Build
RUN dotnet build "Aure.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aure.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Criar diretórios necessários
RUN mkdir -p /app/logs /app/certificates /app/uploads

# Copiar aplicação
COPY --from=publish /app/publish .

# Variáveis de ambiente para produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Aure.API.dll"]
```

### 4. Docker Compose para Produção

```yaml
version: '3.8'

services:
  aure-api:
    build: .
    container_name: aure-api-prod
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=aure_production;Username=aure_user;Password=${POSTGRES_PASSWORD};Include Error Detail=true;Pooling=true;MinPoolSize=10;MaxPoolSize=100;
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - EmailSettings__Password=${EMAIL_PASSWORD}
      - EmailSettings__BaseUrl=https://aureapi.gabrielsanztech.com.br
      - BaseUrl=https://aureapi.gabrielsanztech.com.br
    volumes:
      - ./logs:/app/logs
      - ./certificates:/app/certificates
      - ./uploads:/app/uploads
    depends_on:
      - postgres
      - redis
    restart: unless-stopped
    networks:
      - aure-network

  postgres:
    image: postgres:15-alpine
    container_name: aure-postgres-prod
    environment:
      - POSTGRES_DB=aure_production
      - POSTGRES_USER=aure_user
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    restart: unless-stopped
    networks:
      - aure-network

  redis:
    image: redis:7-alpine
    container_name: aure-redis-prod
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    restart: unless-stopped
    networks:
      - aure-network

volumes:
  postgres_data:
  redis_data:

networks:
  aure-network:
    driver: bridge
```

### 5. Configuração do Nginx (Proxy Reverso)

Crie o arquivo `/etc/nginx/sites-available/aureapi.gabrielsanztech.com.br`:

```nginx
server {
    listen 80;
    server_name aureapi.gabrielsanztech.com.br;
    
    # Redirecionar HTTP para HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name aureapi.gabrielsanztech.com.br;
    
    # Certificados SSL (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/aureapi.gabrielsanztech.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/aureapi.gabrielsanztech.com.br/privkey.pem;
    
    # Configurações SSL
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    
    # Headers de segurança
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;
    add_header X-XSS-Protection "1; mode=block";
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    
    # Configurações do proxy
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # Upload de arquivos (avatars)
    client_max_body_size 10M;
    
    # Logs
    access_log /var/log/nginx/aureapi_access.log;
    error_log /var/log/nginx/aureapi_error.log;
}
```

### 6. Script de Deploy

Crie o arquivo `deploy.sh`:

```bash
#!/bin/bash

echo "🚀 Iniciando deploy da API Aure no Contabo..."

# Variáveis
DOMAIN="aureapi.gabrielsanztech.com.br"
APP_DIR="/opt/aure"
BACKUP_DIR="/opt/aure-backup"

# Criar backup
echo "📦 Criando backup..."
if [ -d "$APP_DIR" ]; then
    sudo cp -r "$APP_DIR" "$BACKUP_DIR-$(date +%Y%m%d_%H%M%S)"
fi

# Parar serviços
echo "⏸️ Parando serviços..."
sudo docker-compose down

# Atualizar código
echo "📥 Atualizando código..."
git pull origin main

# Verificar se .env existe
if [ ! -f .env ]; then
    echo "❌ Arquivo .env não encontrado! Criando template..."
    cat > .env << EOF
POSTGRES_PASSWORD=sua_senha_postgres_muito_segura
JWT_SECRET_KEY=sua_chave_jwt_de_256_bits_muito_segura
EMAIL_PASSWORD=sua_senha_do_gmail_aqui
EOF
    echo "⚠️ Edite o arquivo .env com suas credenciais!"
    exit 1
fi

# Build e deploy
echo "🔨 Fazendo build da aplicação..."
sudo docker-compose build --no-cache

echo "🚀 Subindo serviços..."
sudo docker-compose up -d

# Aguardar serviços
echo "⏳ Aguardando serviços ficarem prontos..."
sleep 30

# Verificar saúde
echo "🔍 Verificando saúde da API..."
curl -f http://localhost:5000/health || {
    echo "❌ API não está respondendo!"
    exit 1
}

echo "✅ Deploy concluído com sucesso!"
echo "📍 API disponível em: https://$DOMAIN"
echo "📊 Swagger: https://$DOMAIN/swagger"
```

## 🔧 Passos para Deploy

### 1. Preparar Servidor
```bash
# Conectar ao servidor
ssh root@IP_DO_SERVIDOR

# Atualizar sistema
apt update && apt upgrade -y

# Instalar dependências
apt install -y docker.io docker-compose nginx certbot python3-certbot-nginx git

# Habilitar Docker
systemctl enable docker
systemctl start docker

# Adicionar usuário ao grupo docker
usermod -aG docker $USER
```

### 2. Configurar SSL (Let's Encrypt)
```bash
# Obter certificado SSL
certbot --nginx -d aureapi.gabrielsanztech.com.br

# Configurar renovação automática
echo "0 12 * * * /usr/bin/certbot renew --quiet" | crontab -
```

### 3. Clonar e Configurar Projeto
```bash
# Criar diretório
mkdir -p /opt/aure
cd /opt/aure

# Clonar repositório
git clone https://github.com/GabrielSelim/Aure.git .

# Configurar variáveis de ambiente
cp .env.example .env
nano .env
```

### 4. Executar Deploy
```bash
# Dar permissão ao script
chmod +x deploy.sh

# Executar deploy
./deploy.sh
```

## 🔐 Configurações de Segurança

### 1. Firewall
```bash
# Configurar UFW
ufw allow ssh
ufw allow 80
ufw allow 443
ufw --force enable
```

### 2. Fail2Ban (Proteção contra ataques)
```bash
# Instalar
apt install -y fail2ban

# Configurar
cat > /etc/fail2ban/jail.local << EOF
[DEFAULT]
bantime = 3600
findtime = 600
maxretry = 5

[sshd]
enabled = true

[nginx-http-auth]
enabled = true
[EOF]

systemctl restart fail2ban
```

### 3. Monitoramento
```bash
# Instalar htop para monitoramento
apt install -y htop

# Script de monitoramento simples
cat > /opt/monitor.sh << 'EOF'
#!/bin/bash
echo "🖥️ Status do Sistema - $(date)"
echo "💾 Uso de Disco:"
df -h
echo "🧠 Uso de Memória:"
free -h
echo "🐳 Status dos Containers:"
docker ps
echo "📊 API Health:"
curl -s http://localhost:5000/health | jq .
EOF

chmod +x /opt/monitor.sh
```

## 📧 Configurações de Email

### Opções de SMTP:

#### 1. Gmail (Atual - Funciona)
```json
{
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "aurecontroll@gmail.com",
  "Password": "wotorcxxfepwxvvu"
}
```

#### 2. SendGrid (Recomendado para produção)
```json
{
  "SmtpHost": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "Username": "apikey",
  "Password": "SUA_API_KEY_SENDGRID"
}
```

#### 3. Amazon SES (Econômico)
```json
{
  "SmtpHost": "email-smtp.us-east-1.amazonaws.com",
  "SmtpPort": 587,
  "Username": "SEU_ACCESS_KEY",
  "Password": "SEU_SECRET_KEY"
}
```

## 🔍 Testes Pós-Deploy

### 1. Teste de Conectividade
```bash
# Teste direto na API
curl https://aureapi.gabrielsanztech.com.br/health

# Teste Swagger
curl https://aureapi.gabrielsanztech.com.br/swagger
```

### 2. Teste de Email
```bash
# Usar Postman ou curl para testar envio de convite
curl -X POST "https://aureapi.gabrielsanztech.com.br/api/Users/convite" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "teste@email.com",
    "nome": "Teste",
    "role": "FuncionarioPJ"
  }'
```

### 3. Verificar Logs
```bash
# Logs da aplicação
docker logs aure-api-prod

# Logs do Nginx
tail -f /var/log/nginx/aureapi_error.log
```

## 📋 Checklist Final

- [ ] Domínio configurado (aureapi.gabrielsanztech.com.br)
- [ ] SSL/HTTPS funcionando
- [ ] Database em produção
- [ ] Variáveis de ambiente configuradas
- [ ] Email SMTP testado
- [ ] CORS configurado para produção
- [ ] Logs funcionando
- [ ] Backup configurado
- [ ] Firewall ativo
- [ ] Monitoring básico

## 🆘 Troubleshooting

### API não responde
```bash
# Verificar status dos containers
docker ps

# Verificar logs
docker logs aure-api-prod

# Reiniciar se necessário
docker-compose restart
```

### Problema de SSL
```bash
# Renovar certificado
certbot renew --dry-run

# Verificar configuração Nginx
nginx -t
```

### Problema de Email
```bash
# Testar SMTP manualmente
telnet smtp.gmail.com 587

# Verificar logs específicos
docker logs aure-api-prod | grep -i email
```

**🎯 Próximo passo: Execute os comandos de preparação do servidor e me informe se encontrar algum problema!**