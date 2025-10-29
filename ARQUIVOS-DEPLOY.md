# 📂 Arquivos de Deploy - API Aure

Todos os arquivos necessários para deploy seguro na Contabo:

## 🎯 **Deploy Principal**
- **[DEPLOY-SIMPLES.md](./DEPLOY-SIMPLES.md)** - Guia rápido de 3 comandos
- **[deploy-simple.sh](./deploy-simple.sh)** - Script de deploy automático
- **[docker-compose.yml](./docker-compose.yml)** - Configuração principal isolada

## 🔧 **Configuração**
- **[.env.example](./.env.example)** - Template de variáveis (copie para .env)
- **[nginx-aureapi.conf](./nginx-aureapi.conf)** - Configuração Nginx com HTTPS

## 🛡️ **Segurança e Conflitos**
- **[check-ports.sh](./check-ports.sh)** - Verifica portas em uso
- **[RESOLVER-CONFLITOS-PORTA.md](./RESOLVER-CONFLITOS-PORTA.md)** - Resolver conflitos

## 📚 **Documentação Completa**
- **[DEPLOY_CONTABO.md](./DEPLOY_CONTABO.md)** - Guia detalhado avançado
- **[setup-complete.sh](./setup-complete.sh)** - Setup completo do servidor

## 🚀 **Início Rápido**

```bash
# 1. No servidor
git clone https://github.com/GabrielSelim/Aure.git
cd Aure

# 2. Verificar portas
./check-ports.sh

# 3. Deploy
./deploy-simple.sh
```

## ✅ **Garantias de Segurança**

- ✅ **Não afeta containers existentes** (nomes únicos)
- ✅ **Portas configuráveis** via .env
- ✅ **Network isolada** com subnet própria
- ✅ **Volumes únicos** não conflitam
- ✅ **Verificação automática** de conflitos

## 🌐 **URLs Finais**

Após deploy completo:
- **API**: https://aureapi.gabrielsanztech.com.br
- **Swagger**: https://aureapi.gabrielsanztech.com.br/swagger
- **Health**: https://aureapi.gabrielsanztech.com.br/health

---

**💡 Tudo pronto para deploy seguro no Contabo!**