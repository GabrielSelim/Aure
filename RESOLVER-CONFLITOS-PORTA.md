# 🔧 Resolvendo Conflitos de Porta - API Aure

Se alguma porta estiver ocupada, siga estas instruções:

## 🔍 **1. Verificar Portas em Uso**

```bash
# Executar script de verificação (recomendado)
./check-ports.sh

# Se faltar ferramentas, instalar primeiro:
sudo apt update
sudo apt install net-tools lsof  # Para netstat e lsof

# Ou manualmente (substitua PORTA pelo número real):
# Método 1 - ss (mais moderno, já vem instalado) ✅ FUNCIONA
sudo ss -tlnp | grep LISTEN | grep ":5000"

# Método 2 - netstat (se disponível)
sudo netstat -tlnp | grep LISTEN | grep ":5000" 2>/dev/null

# Método 3 - lsof (substitua 5000 pela porta desejada)
sudo lsof -i :5000

# Método 4 - verificar todas as portas da Aure ✅ TESTADO
sudo ss -tlnp | grep ":5000\|:5432\|:6379\|:8080"
```

## ⚙️ **2. Configurar Portas Alternativas**

Edite o arquivo `.env`:

```bash
nano .env
```

### Exemplos de Configurações Alternativas:

#### **Se porta 5000 estiver ocupada:**
```env
API_PORT=5001  # ou 5100, 8000, etc
```

#### **Se porta 5432 estiver ocupada (PostgreSQL):**
```env
POSTGRES_PORT=5433  # ou 5434, 5435, etc
```

#### **Se porta 6379 estiver ocupada (Redis):**
```env
REDIS_PORT=6380  # ou 6381, 6400, etc
```

#### **Se porta 8080 estiver ocupada (Adminer):**
```env
ADMINER_PORT=8081  # ou 8090, 9080, etc
```

#### **Se subnet estiver em conflito:**
```env
NETWORK_SUBNET=172.21.0.0/16  # ou 172.22.0.0/16
```

#### **Nome do projeto único:**
```env
COMPOSE_PROJECT_NAME=aure-prod  # ou aure-gabriel, etc
```

## 📝 **3. Exemplo Completo de .env com Portas Alternativas**

```env
# 🏷️ Nome único do projeto
COMPOSE_PROJECT_NAME=aure-prod

# 🌐 Portas alternativas (sem conflito)
API_PORT=5100
POSTGRES_PORT=5433
REDIS_PORT=6380
ADMINER_PORT=8090

# 🌐 Subnet alternativa
NETWORK_SUBNET=172.21.0.0/16

# 🗄️ Database
POSTGRES_DB=aure_production
POSTGRES_USER=aure_user
POSTGRES_PASSWORD=minha_senha_super_segura_123

# 🔐 JWT (IMPORTANTE: Mude!)
JWT_SECRET_KEY=sua_chave_jwt_256_bits_muito_segura_aqui

# 📧 Email
EMAIL_USERNAME=aurecontroll@gmail.com
EMAIL_PASSWORD=wotorcxxfepwxvvu

# 🌐 URLs
API_BASE_URL=https://aureapi.gabrielsanztech.com.br

# 🔒 Encryption
ENCRYPTION_KEY=YXVyZS1lbmNyeXB0aW9uLWtleS0yMDI1LXNlY3VyZQ==
ENCRYPTION_IV=YXVyZS1pdi0yMDI1
```

## 🚀 **4. Deploy com Novas Configurações**

```bash
# Após editar .env, fazer deploy
./deploy-simple.sh

# Acessar API na nova porta
curl http://localhost:5100/health

# Swagger na nova porta
# http://localhost:5100/swagger
```

## 🔧 **5. Nginx com Porta Alternativa**

Se usar porta alternativa, atualize o nginx:

```nginx
# No arquivo nginx-aureapi.conf
location / {
    proxy_pass http://127.0.0.1:5100;  # Usar sua porta
    # ... resto da configuração
}
```

## 🛡️ **6. Segurança e Isolamento**

Com essas configurações:
- ✅ **Containers isolados** com nome único
- ✅ **Network separada** com subnet própria  
- ✅ **Volumes únicos** não conflitam
- ✅ **Portas configuráveis** evitam conflitos
- ✅ **Bind localhost** para segurança (PostgreSQL, Redis, Adminer)

## 📋 **7. Comandos para Verificar**

```bash
# Ver containers Aure ativos
docker ps | grep aure

# Ver todas as redes Docker
docker network ls

# Ver volumes Docker
docker volume ls | grep aure

# Parar apenas containers Aure
docker-compose down

# Ver logs de container específico
docker logs aure-api-prod  # usar seu COMPOSE_PROJECT_NAME
```

## 🆘 **8. Troubleshooting**

### **Porta ainda ocupada após mudança:**
```bash
# Reiniciar Docker
sudo systemctl restart docker

# Limpar cache de rede
docker network prune -f
```

### **Conflito de subnet:**
```bash
# Ver redes em uso
docker network ls
docker network inspect NOME_DA_REDE

# Usar subnet bem diferente
NETWORK_SUBNET=192.168.100.0/24
```

### **Container não sobe:**
```bash
# Ver logs detalhados
docker-compose logs api
docker-compose logs postgres
```

---

**💡 Dica:** Sempre execute `./check-ports.sh` antes do deploy para verificar conflitos!