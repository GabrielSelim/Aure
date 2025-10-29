# 🚀 Deploy Simples - API Aure

Deploy em **3 comandos** no servidor:

## 📋 No Servidor (Ubuntu/Debian)

### 1. Instalar Docker (se não tiver)
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
sudo systemctl enable docker
```

### 2. Clonar e Deploy
```bash
git clone https://github.com/GabrielSelim/Aure.git
cd Aure

# Dar permissões aos scripts (importante!)
chmod +x *.sh

# Verificar portas em uso
./check-ports.sh

# Deploy seguro
./deploy-simple.sh
```

> **⚠️ Conflito de Portas?** Consulte: **[RESOLVER-CONFLITOS-PORTA.md](./RESOLVER-CONFLITOS-PORTA.md)**

### 3. Configurar Nginx (Opcional - para HTTPS)
```bash
# Instalar Nginx
sudo apt install nginx certbot python3-certbot-nginx

# Copiar configuração
sudo cp nginx-aureapi.conf /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br
sudo ln -s /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx

# SSL com Let's Encrypt
sudo certbot --nginx -d aureapi.gabrielsanztech.com.br
```

## 🎯 Resultado

Após o deploy:
- ✅ **API**: http://localhost:5000
- ✅ **Swagger**: http://localhost:5000/swagger  
- ✅ **Health**: http://localhost:5000/health

Com Nginx:
- ✅ **API HTTPS**: https://aureapi.gabrielsanztech.com.br
- ✅ **Swagger HTTPS**: https://aureapi.gabrielsanztech.com.br/swagger

## ⚙️ Configuração Automática

O `docker-compose.yml` já vem configurado com:
- ✅ PostgreSQL com dados persistentes
- ✅ Redis para cache
- ✅ Configurações de produção
- ✅ Health checks
- ✅ Logs automáticos
- ✅ Variáveis de ambiente via `.env`

## 🔧 Personalizar Configurações

Edite o arquivo `.env`:
```bash
nano .env
```

Configure:
- `POSTGRES_PASSWORD` - Senha do banco
- `JWT_SECRET_KEY` - Chave JWT (256 bits)
- `API_BASE_URL` - Seu domínio
- `EMAIL_PASSWORD` - Senha do Gmail

## 📋 Comandos Úteis

```bash
# Ver logs
docker-compose logs -f api

# Reiniciar
docker-compose restart

# Parar tudo
docker-compose down

# Incluir Adminer (dev)
docker-compose --profile dev up -d

# Update da aplicação
git pull && docker-compose up -d --build
```

## 🛡️ Segurança Automática

- ✅ Portas bind apenas localhost (5432, 6379, 8080)
- ✅ API na porta 5000 (proxy via Nginx)
- ✅ Variáveis de ambiente seguras
- ✅ Containers com restart automático

## 🔍 Troubleshooting

### API não sobe
```bash
docker-compose logs api
```

### Banco não conecta
```bash
docker-compose logs postgres
```

### Problema de permissão
```bash
sudo chown -R $USER:$USER ./logs ./uploads
```

---

**🎉 Pronto! API rodando com 3 comandos!**