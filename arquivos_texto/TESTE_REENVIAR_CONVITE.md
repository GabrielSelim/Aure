# 🧪 Teste Completo - Reenviar Convite

## 📋 Pré-requisitos
- API rodando em `http://localhost:5203`
- Postman instalado
- Database limpo ou com dados de teste

## 🔄 Fluxo de Teste

### 1️⃣ Registrar Empresa Principal (DonoEmpresaPai)

```http
POST http://localhost:5203/api/registration/admin-empresa
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@empresateste.com",
  "senha": "Senha@123456",
  "confirmaSenha": "Senha@123456",
  "nomeEmpresa": "Empresa Teste LTDA",
  "cnpj": "12345678000190",
  "tipoEmpresa": "Client",
  "modeloNegocio": "MainCompany"
}
```

**Resposta Esperada**: 201 Created
```json
{
  "mensagem": "Cadastro realizado com sucesso",
  "usuario": {
    "id": "guid-usuario",
    "nome": "João Silva",
    "email": "joao.silva@empresateste.com",
    "role": "DonoEmpresaPai",
    "empresaId": "guid-empresa"
  }
}
```

**✅ Copiar**: `id` do usuário e `empresaId`

---

### 2️⃣ Login como DonoEmpresaPai

```http
POST http://localhost:5203/api/auth/login
Content-Type: application/json

{
  "email": "joao.silva@empresateste.com",
  "senha": "Senha@123456"
}
```

**Resposta Esperada**: 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": "guid-usuario",
    "nome": "João Silva",
    "email": "joao.silva@empresateste.com",
    "role": "DonoEmpresaPai"
  }
}
```

**✅ Copiar**: `token` (usar em todos os requests seguintes)

---

### 3️⃣ Convidar Funcionário PJ

```http
POST http://localhost:5203/api/registration/convidar-usuario
Content-Type: application/json
Authorization: Bearer {SEU_TOKEN_AQUI}

{
  "nomeConvidado": "Maria Santos",
  "emailConvidado": "maria.santos@pjteste.com",
  "role": "FuncionarioPJ",
  "tipoConvite": "ContractedPJ",
  "modeloNegocio": "Freelancer",
  "nomeEmpresaPJ": "Maria Santos PJ LTDA",
  "cnpj": "98765432000111",
  "tipoEmpresaPJ": "Provider"
}
```

**Resposta Esperada**: 200 OK
```json
{
  "mensagem": "Convite enviado com sucesso",
  "convite": {
    "id": "guid-convite-aqui",
    "inviterName": "João Silva",
    "inviteeEmail": "maria.santos@pjteste.com",
    "inviteeName": "Maria Santos",
    "role": "FuncionarioPJ",
    "companyId": "guid-empresa",
    "invitedByUserId": "guid-usuario",
    "token": "token-original",
    "expiresAt": "2025-11-03T17:00:00Z",
    "isAccepted": false,
    "inviteType": "ContractedPJ"
  }
}
```

**✅ Copiar**: `id` do convite (guid-convite-aqui)

---

### 4️⃣ Verificar Convites Pendentes

```http
GET http://localhost:5203/api/registration/convites
Authorization: Bearer {SEU_TOKEN_AQUI}
```

**Resposta Esperada**: 200 OK
```json
[
  {
    "id": "guid-convite",
    "inviterName": "João Silva",
    "inviteeEmail": "maria.santos@pjteste.com",
    "inviteeName": "Maria Santos",
    "role": "FuncionarioPJ",
    "token": "token-original",
    "expiresAt": "2025-11-03T17:00:00Z",
    "isAccepted": false
  }
]
```

---

### 5️⃣ **TESTAR: Reenviar Convite** ⭐

```http
POST http://localhost:5203/api/registration/reenviar-convite/{guid-convite-aqui}
Authorization: Bearer {SEU_TOKEN_AQUI}
```

**Resposta Esperada**: 200 OK
```json
{
  "mensagem": "Convite reenviado com sucesso",
  "convite": {
    "id": "guid-convite",
    "inviterName": "João Silva",
    "inviteeEmail": "maria.santos@pjteste.com",
    "inviteeName": "Maria Santos",
    "role": "FuncionarioPJ",
    "companyId": "guid-empresa",
    "invitedByUserId": "guid-usuario",
    "token": "NOVO-TOKEN-DIFERENTE",
    "expiresAt": "2025-11-04T01:00:00Z",
    "isAccepted": false,
    "inviteType": "ContractedPJ"
  }
}
```

**✅ Validar**:
- `token` mudou (diferente do original)
- `expiresAt` foi atualizado (7 dias a partir de agora)
- `isAccepted` continua `false`
- Outros campos permaneceram iguais

---

### 6️⃣ Verificar Email Foi Enviado

**Verificar Logs**:
```bash
docker logs aure-api --tail 100 | grep -i "email"
```

**Procurar por**:
- `[INF] Enviando email para maria.santos@pjteste.com`
- `[INF] Email enviado com sucesso`

**Se falhar**:
- Verificar configuração SMTP em `appsettings.Docker.json`
- Gmail pode estar bloqueando (verificar "Acesso de apps menos seguros")
- Senha do Gmail deve ser App Password (sem espaços)

---

## 🔒 Testes de Segurança

### Teste 7: Tentar reenviar convite já aceito

```http
# Primeiro aceitar o convite
POST http://localhost:5203/api/registration/aceitar-convite/{token}
Content-Type: application/json

{
  "senha": "SenhaPJ@123",
  "confirmaSenha": "SenhaPJ@123"
}

# Depois tentar reenviar
POST http://localhost:5203/api/registration/reenviar-convite/{guid-convite}
Authorization: Bearer {SEU_TOKEN_AQUI}
```

**Resposta Esperada**: 400 Bad Request
```json
{
  "erro": "Não é possível reenviar convite já aceito"
}
```

---

### Teste 8: Tentar reenviar sem autenticação

```http
POST http://localhost:5203/api/registration/reenviar-convite/{guid-convite}
```

**Resposta Esperada**: 401 Unauthorized

---

### Teste 9: FuncionarioPJ tentar reenviar convite

```http
# Login como FuncionarioPJ
POST http://localhost:5203/api/auth/login
{
  "email": "maria.santos@pjteste.com",
  "senha": "SenhaPJ@123"
}

# Tentar reenviar
POST http://localhost:5203/api/registration/reenviar-convite/{guid-convite}
Authorization: Bearer {TOKEN_DO_PJ}
```

**Resposta Esperada**: 403 Forbidden

---

### Teste 10: Tentar reenviar convite de outra empresa

```http
# Criar segunda empresa
POST http://localhost:5203/api/registration/admin-empresa
{
  "nome": "Carlos Souza",
  "email": "carlos@outraempresa.com",
  "senha": "Senha@123456",
  "confirmaSenha": "Senha@123456",
  "nomeEmpresa": "Outra Empresa LTDA",
  "cnpj": "11223344000155",
  "tipoEmpresa": "Client",
  "modeloNegocio": "MainCompany"
}

# Login como Carlos
POST http://localhost:5203/api/auth/login
{
  "email": "carlos@outraempresa.com",
  "senha": "Senha@123456"
}

# Tentar reenviar convite da primeira empresa
POST http://localhost:5203/api/registration/reenviar-convite/{guid-convite-empresa-1}
Authorization: Bearer {TOKEN_CARLOS}
```

**Resposta Esperada**: 400 Bad Request
```json
{
  "erro": "Não autorizado a reenviar este convite"
}
```

---

### Teste 11: Convite inexistente

```http
POST http://localhost:5203/api/registration/reenviar-convite/00000000-0000-0000-0000-000000000000
Authorization: Bearer {SEU_TOKEN_AQUI}
```

**Resposta Esperada**: 400 Bad Request
```json
{
  "erro": "Convite não encontrado"
}
```

---

## ✅ Checklist de Validação

### Funcionalidade
- [ ] Endpoint responde em `/api/registration/reenviar-convite/{id}`
- [ ] Token é regenerado (valor diferente)
- [ ] Data de expiração é atualizada (7 dias)
- [ ] Email é enviado com novo token
- [ ] Dados do convite permanecem iguais (nome, email, role, etc)

### Segurança
- [ ] Requer autenticação (Bearer token)
- [ ] Apenas DonoEmpresaPai, Financeiro, Juridico podem usar
- [ ] FuncionarioPJ não tem acesso
- [ ] Não permite reenviar convites de outras empresas
- [ ] Não permite reenviar convites já aceitos

### Validações
- [ ] Convite não encontrado retorna 400
- [ ] Convite já aceito retorna 400
- [ ] Sem autenticação retorna 401
- [ ] Sem permissão retorna 403
- [ ] Token regenerado é válido para aceitar convite

### Swagger
- [ ] Endpoint aparece no Swagger UI
- [ ] Documentação está completa
- [ ] Exemplos de request/response corretos
- [ ] Códigos de status documentados

---

## 🐛 Possíveis Problemas

### Email não enviado
**Sintoma**: Status 200 mas email não chega

**Soluções**:
1. Verificar logs: `docker logs aure-api | grep -i email`
2. Verificar configuração SMTP:
   ```bash
   docker exec aure-api cat /app/appsettings.Docker.json | grep -A 10 EmailSettings
   ```
3. Testar credenciais Gmail manualmente
4. Verificar se Gmail App Password está sem espaços

### Token não funciona ao aceitar
**Sintoma**: Após reenviar, aceitar retorna "Token inválido"

**Soluções**:
1. Verificar se token foi copiado corretamente
2. Verificar se não há espaços no token
3. Verificar data de expiração

### Unauthorized ao reenviar
**Sintoma**: 401 ou 403 ao tentar reenviar

**Soluções**:
1. Verificar se token JWT está válido
2. Verificar role do usuário (deve ser Dono, Financeiro ou Juridico)
3. Refazer login para obter novo token

---

## 📊 Resultados Esperados

### ✅ Sucesso Total
- Todos os 11 testes passam
- Email chega com novo link
- Novo token aceita convite normalmente
- Logs mostram todas operações sem erros

### ⚠️ Sucesso Parcial
- Funcionalidade funciona mas email não envia
- Verificar configuração SMTP
- Pode testar funcionalidade mesmo sem email

### ❌ Falha
- Endpoint retorna 500
- Verificar logs: `docker logs aure-api`
- Verificar database: conexão e migrations
- Verificar build: `docker-compose build --no-cache api`

---

## 📝 Relatório de Teste

```
Data: __/__/____
Testador: _______________

Teste 1 - Criar Empresa:        [ ] Pass [ ] Fail
Teste 2 - Login:                [ ] Pass [ ] Fail
Teste 3 - Criar Convite:        [ ] Pass [ ] Fail
Teste 4 - Listar Convites:      [ ] Pass [ ] Fail
Teste 5 - Reenviar Convite:     [ ] Pass [ ] Fail
Teste 6 - Email Enviado:        [ ] Pass [ ] Fail
Teste 7 - Convite Aceito:       [ ] Pass [ ] Fail
Teste 8 - Sem Autenticação:     [ ] Pass [ ] Fail
Teste 9 - FuncionarioPJ:        [ ] Pass [ ] Fail
Teste 10 - Outra Empresa:       [ ] Pass [ ] Fail
Teste 11 - Convite Inexistente: [ ] Pass [ ] Fail

Observações:
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________
```

---

## 🎯 Próximos Passos

Após validar todos os testes:
1. Testar com Gmail real (não apenas logs)
2. Testar com múltiplos reenvios do mesmo convite
3. Adicionar limite de reenvios (máximo 3 por convite)
4. Adicionar notificação de auditoria quando convite é reenviado
5. Documentar no Postman Collection oficial
