# 🚀 Instruções de Deploy - Melhorias Backend Aure

**Data**: 30/10/2025  
**Branch**: main  
**Servidor**: root@5.189.174.61  
**Domínios**: aureapi.gabrielsanztech.com.br / aure.gabrielsanztech.com.br

---

## 📋 Resumo das Alterações

### Migrations a Aplicar
1. **AdicionarTokenRecuperacaoSenha** - Campos para recuperação de senha
2. **AdicionarContatoEnderecoEmpresa** - Campos de contato e endereço da empresa

### Novos Endpoints
- `POST /api/auth/solicitar-recuperacao-senha` - Solicita recuperação de senha
- `POST /api/auth/redefinir-senha` - Redefine senha com token
- `PUT /api/Users/{employeeId}/cargo` - Altera cargo de funcionário (DonoEmpresaPai)
- `GET /api/UserProfile/empresa` - Busca dados da empresa (todos os usuários)
- `PUT /api/UserProfile/empresa` - Atualiza dados da empresa (DonoEmpresaPai)

---

## 🔧 Passo a Passo do Deploy

### 1️⃣ Conectar ao Servidor

```bash
ssh root@5.189.174.61
```

### 2️⃣ Navegar para o Diretório do Projeto

```bash
cd /root/Aure
```

### 3️⃣ Fazer Pull das Alterações

```bash
git pull origin main
```

**Verificar se há conflitos:**
```bash
git status
```

### 4️⃣ Aplicar Migrations no Banco de Dados

**Opção A - Via Docker (Recomendado)**:

```bash
# Aplicar migrations usando dotnet ef dentro do container
docker exec aure-api-aure-gabriel dotnet ef database update --project /app/src/Aure.Infrastructure --startup-project /app/src/Aure.API
```

**Opção B - Via Script SQL Direto**:

O script SQL está em `deploy_migrations.sql`. Para executar:

```bash
# Copiar script para dentro do container do PostgreSQL
docker cp deploy_migrations.sql aure-postgres-aure-gabriel:/tmp/

# Executar script
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production -f /tmp/deploy_migrations.sql

# Verificar se migrations foram aplicadas
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;"
```

**Saída esperada:**
```
                  MigrationId
-----------------------------------------------
 20251030193745_AdicionarContatoEnderecoEmpresa
 20251030193120_AdicionarTokenRecuperacaoSenha
 20251029131523_AddUserInvitationsTable
 ...
```

### 5️⃣ Rebuild dos Containers Docker

```bash
# Parar containers
docker-compose down

# Rebuild com as novas alterações
docker-compose up -d --build

# Aguardar containers subirem (30-60 segundos)
sleep 30
```

### 6️⃣ Verificar Logs

```bash
# Logs da API
docker logs -f aure-api-aure-gabriel --tail 100

# Logs do PostgreSQL
docker logs aure-postgres-aure-gabriel --tail 50
```

**Buscar por erros:**
```bash
docker logs aure-api-aure-gabriel 2>&1 | grep -i "error\|exception\|fail"
```

**Logs esperados (sem erros)**:
- `Now listening on: http://[::]:8080`
- `Application started. Press Ctrl+C to shut down.`
- Sem mensagens de erro ou exception

### 7️⃣ Testar Health Check

```bash
# Health da API
curl https://aureapi.gabrielsanztech.com.br/health

# Verificar Swagger
curl https://aureapi.gabrielsanztech.com.br/swagger/index.html
```

**Respostas esperadas:**
- Health: `{"status":"Healthy"}` ou 200 OK
- Swagger: HTML da página Swagger

### 8️⃣ Verificar Estrutura do Banco

```bash
# Conectar ao PostgreSQL
docker exec -it aure-postgres-aure-gabriel psql -U aure_user -d aure_production

# Verificar colunas da tabela users
\d users

# Verificar colunas da tabela companies
\d companies

# Sair do psql
\q
```

**Colunas esperadas em `users`:**
- `password_reset_token` (text)
- `password_reset_token_expiry` (timestamp with time zone)

**Colunas esperadas em `companies`:**
- `phone_mobile` (text)
- `phone_landline` (text)
- `address_street` (text)
- `address_number` (text)
- `address_complement` (text)
- `address_neighborhood` (text)
- `address_city` (text)
- `address_state` (text)
- `address_country` (text)
- `address_zip_code` (text)

---

## 🧪 Testes Funcionais

### Teste 1: Recuperação de Senha

```bash
# Solicitar recuperação
curl -X POST https://aureapi.gabrielsanztech.com.br/api/auth/solicitar-recuperacao-senha \
  -H "Content-Type: application/json" \
  -d '{"email": "seu-email@teste.com"}'
```

**Esperado**: `200 OK` + email enviado

### Teste 2: Buscar Dados da Empresa

```bash
# Buscar empresa (com token de autenticação)
curl -X GET https://aureapi.gabrielsanztech.com.br/api/UserProfile/empresa \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Esperado**: `200 OK` + JSON com dados da empresa

### Teste 3: Swagger UI

1. Abrir no navegador: `https://aureapi.gabrielsanztech.com.br/swagger`
2. Verificar se os novos endpoints aparecem:
   - `POST /api/auth/solicitar-recuperacao-senha`
   - `POST /api/auth/redefinir-senha`
   - `PUT /api/Users/{employeeId}/cargo`
   - `GET /api/UserProfile/empresa`
   - `PUT /api/UserProfile/empresa`

---

## 🔍 Troubleshooting

### Problema: Containers não sobem

```bash
# Verificar status
docker ps -a

# Ver logs de erro
docker-compose logs

# Reiniciar containers
docker-compose restart
```

### Problema: Erro ao aplicar migrations

```bash
# Verificar conexão com banco
docker exec aure-api-aure-gabriel ping aure-postgres-aure-gabriel

# Verificar string de conexão
docker exec aure-api-aure-gabriel env | grep ConnectionStrings

# Verificar se banco existe
docker exec -it aure-postgres-aure-gabriel psql -U aure_user -l
```

### Problema: API retorna 502 Bad Gateway

```bash
# Verificar se API está rodando
docker ps | grep aure-api

# Verificar portas
docker port aure-api-aure-gabriel

# Reiniciar Nginx (se necessário)
docker restart nginx-proxy
```

### Problema: Emails não estão sendo enviados

```bash
# Verificar configuração SMTP nos logs
docker logs aure-api-aure-gabriel | grep -i "smtp\|email"

# Verificar variáveis de ambiente
docker exec aure-api-aure-gabriel env | grep Email
```

---

## 📊 Monitoramento Pós-Deploy

### Monitorar logs em tempo real:

```bash
# Terminal 1: Logs da API
docker logs -f aure-api-aure-gabriel

# Terminal 2: Logs do PostgreSQL
docker logs -f aure-postgres-aure-gabriel
```

### Verificar uso de recursos:

```bash
# CPU e Memória dos containers
docker stats --no-stream

# Espaço em disco
df -h
```

### Verificar conexões ativas:

```bash
# Conexões no PostgreSQL
docker exec -it aure-postgres-aure-gabriel psql -U aure_user -d aure_production \
  -c "SELECT count(*) FROM pg_stat_activity WHERE datname = 'aure_production';"
```

---

## ✅ Checklist Final

Após deploy, verificar:

- [ ] Migrations aplicadas com sucesso
- [ ] Containers rodando (API e PostgreSQL)
- [ ] Health check retornando 200 OK
- [ ] Swagger acessível e mostrando novos endpoints
- [ ] Logs sem erros críticos
- [ ] Colunas criadas nas tabelas `users` e `companies`
- [ ] Endpoint de recuperação de senha funcionando
- [ ] Endpoint de dados da empresa funcionando
- [ ] CORS configurado (testar do frontend em localhost:3000)

---

## 📞 Rollback (Se Necessário)

Caso algo dê errado:

```bash
# 1. Voltar para commit anterior
git reset --hard HEAD~1

# 2. Rebuild containers
docker-compose down
docker-compose up -d --build

# 3. Reverter migrations (SE NECESSÁRIO)
docker exec aure-api-aure-gabriel dotnet ef database update 20251029131523_AddUserInvitationsTable \
  --project /app/src/Aure.Infrastructure --startup-project /app/src/Aure.API
```

---

## 📝 Observações Importantes

1. **Backup**: Sempre recomendado fazer backup do banco antes de aplicar migrations em produção
2. **Downtime**: Este deploy pode ser feito sem downtime significativo (< 1 minuto)
3. **Email**: Os emails de recuperação de senha usam template HTML e apontam para `aure.gabrielsanztech.com.br`
4. **CORS**: Configurado para aceitar requisições de `localhost:3000` (desenvolvimento) e `aure.gabrielsanztech.com.br` (produção)
5. **NotificationPreferences**: A tabela já existe no banco (criada em migration anterior), então não há migration adicional para ela

---

**Desenvolvido por**: Gabriel Selim  
**Última atualização**: 30/10/2025
