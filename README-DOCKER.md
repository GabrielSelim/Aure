# 🐳 Docker Setup - Sistema Aure

## 📋 Resumo

Este projeto possui **3 configurações Docker** diferentes para atender a diferentes necessidades:

- **`docker-compose.dev.yml`** - Desenvolvimento local (apenas banco)
- **`docker-compose.yml`** - Ambiente completo de desenvolvimento/teste  
- **`docker-compose.prod.yml`** - Produção com monitoramento

## 🚀 Início Rápido

### Windows (PowerShell)
```powershell
# Subir apenas PostgreSQL para desenvolvimento
.\docker.ps1 dev-db

# Aplicar migrações
.\docker.ps1 db-migrate

# Ver status
.\docker.ps1 status
```

### Linux/Mac (Bash)
```bash
# Dar permissão de execução
chmod +x docker.sh

# Subir apenas PostgreSQL para desenvolvimento
./docker.sh dev-db

# Aplicar migrações
./docker.sh db-migrate

# Ver status
./docker.sh status
```

## 📦 Configurações Disponíveis

### 1. 🛠️ Desenvolvimento Local (`docker-compose.dev.yml`)

**Uso recomendado:** Quando você quer rodar a API localmente mas usar PostgreSQL no Docker.

**Serviços inclusos:**
- PostgreSQL 15
- Redis 7
- Adminer (admin banco)
- MailHog (teste emails)

**Comandos:**
```bash
# Subir apenas PostgreSQL
./docker.sh dev-db

# Subir todos os serviços de desenvolvimento
./docker.sh dev-up

# Parar
./docker.sh dev-down
```

**Acessos:**
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
- Adminer: http://localhost:8080
- MailHog: http://localhost:8025

### 2. 🏗️ Ambiente Completo (`docker-compose.yml`)

**Uso recomendado:** Teste completo do sistema ou demonstrações.

**Serviços inclusos:**
- PostgreSQL 15
- Redis 7
- API .NET 8
- Adminer

**Comandos:**
```bash
# Subir tudo
./docker.sh up

# Ver logs da API
./docker.sh logs-api

# Parar tudo
./docker.sh down
```

**Acessos:**
- API: http://localhost:5203
- Swagger: http://localhost:5203/swagger
- Adminer: http://localhost:8080

### 3. 🚀 Produção (`docker-compose.prod.yml`)

**Uso recomendado:** Deploy em produção.

**Serviços inclusos:**
- PostgreSQL 15 (com secrets)
- Redis 7 (com senha)
- API .NET 8
- Frontend Next.js
- Nginx (reverse proxy)
- Prometheus (monitoramento)
- Grafana (dashboards)

**Comandos:**
```bash
# Configurar secrets primeiro
mkdir secrets
echo "senha_postgres" > secrets/postgres_password.txt
echo "senha_redis" > secrets/redis_password.txt
# ... outros secrets

# Subir produção
./docker.sh prod-up
```

## 🔧 Comandos Disponíveis

### Desenvolvimento
| Comando | Descrição |
|---------|-----------|
| `dev-up` | Subir PostgreSQL + Redis + Adminer |
| `dev-db` | Subir apenas PostgreSQL |
| `dev-down` | Parar serviços de desenvolvimento |
| `dev-logs` | Ver logs |

### Ambiente Completo
| Comando | Descrição |
|---------|-----------|
| `up` | Subir todos os serviços |
| `down` | Parar todos os serviços |
| `restart` | Reiniciar serviços |
| `logs` | Ver logs de todos |
| `logs-api` | Ver logs apenas da API |

### Produção
| Comando | Descrição |
|---------|-----------|
| `prod-up` | Subir ambiente de produção |
| `prod-down` | Parar produção |
| `prod-logs` | Ver logs de produção |

### Manutenção
| Comando | Descrição |
|---------|-----------|
| `build` | Rebuild das imagens |
| `clean` | Limpar containers e volumes |
| `status` | Status dos containers |
| `db-migrate` | Aplicar migrações EF |
| `db-reset` | Reset completo do banco |

### Monitoramento
| Comando | Descrição |
|---------|-----------|
| `adminer` | Abrir Adminer no navegador |
| `mailhog` | Abrir MailHog no navegador |

## 🔗 Configuração de Banco

### Conexão PostgreSQL (Desenvolvimento)
```
Host: localhost
Port: 5432
Database: aure_db
Username: aure_user
Password: aure_password
```

### String de Conexão .NET
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aure_db;Username=aure_user;Password=aure_password;Include Error Detail=true"
  }
}
```

## 📊 Adminer (Administração de Banco)

Acesse: http://localhost:8080

**Configurações:**
- Sistema: PostgreSQL
- Servidor: postgres-dev (ou postgres)
- Usuário: aure_user
- Senha: aure_password
- Base de dados: aure_db

## 📧 MailHog (Teste de Emails)

Acesse: http://localhost:8025

Todos os emails enviados pela aplicação em desenvolvimento são capturados aqui.

**Configuração SMTP para teste:**
```json
{
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "UseSsl": false,
    "Username": "",
    "Password": ""
  }
}
```

## 🔐 Configuração de Produção

### 1. Criar diretório de secrets
```bash
mkdir secrets
chmod 700 secrets
```

### 2. Configurar secrets
```bash
# Senha do PostgreSQL
echo "sua_senha_postgres_super_segura" > secrets/postgres_password.txt

# Senha do Redis
echo "sua_senha_redis_super_segura" > secrets/redis_password.txt

# JWT Secret (256 bits)
echo "sua_chave_jwt_super_secreta_com_256_bits_minimo" > secrets/jwt_secret.txt

# Configurações de email
echo "seu-email@gmail.com" > secrets/email_username.txt
echo "sua-senha-de-app-gmail" > secrets/email_password.txt

# Senha admin do Grafana
echo "admin_password_grafana_seguro" > secrets/grafana_admin_password.txt

# Definir permissões
chmod 600 secrets/*
```

### 3. Configurar volumes de produção
```bash
# Criar diretórios para dados persistentes
sudo mkdir -p /opt/aure/data/{postgres,redis,prometheus,grafana}
sudo chown -R $(whoami):$(whoami) /opt/aure/data/
```

## 🏥 Health Checks

Todos os serviços possuem health checks configurados:

- **PostgreSQL**: `pg_isready`
- **Redis**: `ping`
- **API**: `/swagger` endpoint
- **Frontend**: Resposta HTTP 200

## 📝 Logs

### Localização dos logs:
- **API**: `./logs/aure-YYYYMMDD.log`
- **Nginx**: `./nginx/logs/`
- **Container logs**: `docker-compose logs [service]`

### Rotação de logs:
- **Desenvolvimento**: 30 dias
- **Produção**: 90 dias

## 🔧 Troubleshooting

### Problema: Porta já em uso
```bash
# Verificar o que está usando a porta
netstat -tulpn | grep :5432

# Parar container existente
docker stop aure-postgres
docker rm aure-postgres
```

### Problema: Container não inicia
```bash
# Ver logs detalhados
./docker.sh logs

# Verificar recursos do sistema
docker system df
docker system prune
```

### Problema: Banco não conecta
```bash
# Verificar se PostgreSQL está rodando
./docker.sh status

# Testar conexão
docker exec -it aure-postgres-dev psql -U aure_user -d aure_db
```

### Reset completo
```bash
# Parar tudo e limpar
./docker.sh clean

# Recriar do zero
./docker.sh dev-up
./docker.sh db-migrate
```

## 🚀 Próximos Passos

1. **✅ CONCLUÍDO**: Docker Compose básico
2. **⏳ PENDENTE**: Configurar Nginx
3. **⏳ PENDENTE**: Setup SSL/TLS
4. **⏳ PENDENTE**: CI/CD pipeline
5. **⏳ PENDENTE**: Backup automático
6. **⏳ PENDENTE**: Monitoramento avançado

## 📈 Monitoramento (Produção)

### Grafana
- **URL**: http://localhost:3001
- **Login**: admin / [secret configurado]
- **Dashboards**: Métricas da API, PostgreSQL, Redis

### Prometheus
- **URL**: http://localhost:9090
- **Métricas**: `/metrics` da API

---

**🎯 Configuração Docker completa para todos os ambientes!**