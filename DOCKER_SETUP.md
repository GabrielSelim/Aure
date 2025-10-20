# 🚀 CONFIGURAÇÃO DOCKER COMPOSE - SISTEMA AURE
# Data: 20/10/2025

## 📋 ARQUIVOS CRIADOS

### ✅ **docker-compose.yml** - Configuração Principal
- PostgreSQL 15 com persistência
- Redis para cache e sessions
- API .NET 8 com todas as configurações
- Adminer para administração do banco
- Network isolada para segurança
- Health checks configurados
- Volumes persistentes

### ✅ **docker-compose.dev.yml** - Desenvolvimento Local
- Configuração simplificada para desenvolvimento
- PostgreSQL sem senha Redis
- MailHog para testes de email
- Portas expostas para debug

### ✅ **docker-compose.prod.yml** - Produção
- Configuração com secrets
- Nginx reverse proxy
- Monitoring (Prometheus + Grafana)
- Recursos limitados
- Bind apenas localhost
- Logs otimizados

## 🚀 COMANDOS PARA USO

### **Desenvolvimento Local (Recomendado)**
```bash
# Subir apenas PostgreSQL para desenvolvimento
docker-compose -f docker-compose.dev.yml up postgres-dev -d

# Subir PostgreSQL + Redis + Adminer
docker-compose -f docker-compose.dev.yml up -d

# Parar serviços
docker-compose -f docker-compose.dev.yml down
```

### **Ambiente Completo**
```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs da API
docker-compose logs -f api

# Parar todos os serviços
docker-compose down

# Limpar volumes (CUIDADO!)
docker-compose down -v
```

### **Produção**
```bash
# Criar secrets primeiro (ver seção abaixo)
# Depois subir produção
docker-compose -f docker-compose.prod.yml up -d
```

## 🔗 ACESSOS

### **Desenvolvimento**
- **API Backend**: http://localhost:5203
- **Swagger**: http://localhost:5203/swagger
- **Adminer**: http://localhost:8080
  - Server: postgres-dev
  - Username: aure_user
  - Password: aure_password
  - Database: aure_db
- **MailHog** (Email): http://localhost:8025
- **Redis**: localhost:6379

### **Produção**
- **Frontend**: https://aure.com.br
- **API**: https://api.aure.com.br
- **Monitoring**: http://localhost:3001 (Grafana)

## 🔐 CONFIGURAÇÃO DE SECRETS (PRODUÇÃO)

### Criar diretório de secrets:
```bash
mkdir secrets
```

### Criar arquivos de secrets:
```bash
# Senha do PostgreSQL
echo "sua_senha_postgres_segura" > secrets/postgres_password.txt

# Senha do Redis
echo "sua_senha_redis_segura" > secrets/redis_password.txt

# JWT Secret
echo "sua_chave_jwt_super_secreta_256_bits" > secrets/jwt_secret.txt

# Email configurações
echo "seu-email@gmail.com" > secrets/email_username.txt
echo "sua-senha-de-app-gmail" > secrets/email_password.txt

# Grafana admin
echo "admin_password_grafana" > secrets/grafana_admin_password.txt

# Permissões
chmod 600 secrets/*
```

## 📊 MONITORAMENTO

### **Health Checks Configurados:**
- PostgreSQL: `pg_isready`
- Redis: `ping`
- API: `/swagger` endpoint
- Tempo de start: 60-90s

### **Logs Centralizados:**
- API logs: `./logs/aure-YYYYMMDD.log`
- Nginx logs: `./nginx/logs/`
- Rotação automática: 30/90 dias

## 🔧 CONFIGURAÇÕES PERSONALIZADAS

### **Variáveis de Ambiente API:**
```bash
# Banco de dados
ConnectionStrings__DefaultConnection

# JWT
JwtSettings__SecretKey
JwtSettings__ExpirationMinutes

# Email SMTP
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__Username
EmailSettings__Password

# Logging
Serilog__MinimumLevel__Default
```

### **Recursos Limitados (Produção):**
- PostgreSQL: 1GB RAM, 0.5 CPU
- Redis: 512MB RAM, 0.25 CPU
- API: 2GB RAM, 1 CPU
- Frontend: 1GB RAM, 0.5 CPU

## 🛠️ TROUBLESHOOTING

### **Problema: Container não inicia**
```bash
# Ver logs detalhados
docker-compose logs [service-name]

# Recriar containers
docker-compose up --force-recreate
```

### **Problema: Banco não conecta**
```bash
# Verificar se PostgreSQL está rodando
docker-compose ps

# Testar conexão
docker-compose exec postgres psql -U aure_user -d aure_db
```

### **Problema: API não responde**
```bash
# Verificar health check
docker-compose exec api curl http://localhost:5203/swagger

# Ver logs da aplicação
docker-compose logs api
```

### **Reset Completo:**
```bash
# Parar tudo
docker-compose down

# Remover volumes
docker-compose down -v

# Limpar imagens
docker system prune -a

# Recriar tudo
docker-compose up --build -d
```

## 📈 PRÓXIMOS PASSOS

1. **✅ CONCLUÍDO**: Docker Compose básico
2. **⏳ PENDENTE**: Configurar Nginx
3. **⏳ PENDENTE**: Setup de monitoring
4. **⏳ PENDENTE**: CI/CD pipeline
5. **⏳ PENDENTE**: Backup automático
6. **⏳ PENDENTE**: SSL/TLS certificates

## 🎯 RECOMENDAÇÕES

### **Para Desenvolvimento:**
- Use `docker-compose.dev.yml`
- API rodando localmente (`dotnet run`)
- Apenas PostgreSQL no Docker

### **Para Testes:**
- Use `docker-compose.yml` completo
- Todos os serviços no Docker
- Environment variáveis de teste

### **Para Produção:**
- Use `docker-compose.prod.yml`
- Configure secrets adequadamente
- Monitor resources com Grafana

**Configuração Docker completa criada com sucesso! 🚀**