# 🚀 TESTE COMPLETO - FLUXO PJ CONTRATADO
# Sistema Aure - Relacionamentos entre Empresas
# Data: 12/10/2025

## 📋 RESUMO DO TESTE
Este arquivo contém o fluxo completo para testar:
1. Criação de empresa contratante
2. Convite de PJ
3. Aceite do convite (cria empresa PJ + relacionamento)
4. Verificação de vinculações via endpoints

---

## 🎯 CENÁRIO DE TESTE

**Empresa Contratante:** HARALD WEINERT (ACAI DO ALEMAO)
**CNPJ Contratante:** 24.345.700/0001-86
**PJ a ser contratado:** João Silva - Consultoria
**CNPJ PJ:** (Usar CNPJ válido da Receita Federal)
**Email PJ:** joao.silva@consultoria.com

⚠️ **IMPORTANTE - VALIDAÇÃO DE CNPJ:**
- O sistema valida CNPJs em tempo real com a Receita Federal (BrasilAPI/ReceitaWS)
- O nome da empresa DEVE corresponder aos dados oficiais
- Use CNPJs e nomes reais para testes
- Exemplo testado: CNPJ 24.345.700/0001-86 = "HARALD WEINERT"

---

## 🔧 PRÉ-REQUISITOS

1. ✅ Aplicação rodando em http://localhost:5203
2. ✅ PostgreSQL conectado
3. ✅ Tabela CompanyRelationships criada (migration aplicada)
4. ✅ Swagger UI disponível
5. ✅ **Internet ativa** (para validação de CNPJ com APIs externas)

## 🏢 VALIDAÇÃO DE CNPJ - COMO FUNCIONA

### Sistema de Validação Rigoroso
- **API Principal:** BrasilAPI (https://brasilapi.com.br)
- **API Backup:** ReceitaWS (https://receitaws.com.br)
- **Validação:** Nome da empresa DEVE corresponder aos dados oficiais
- **Tempo Real:** Consulta direta na Receita Federal

### Exemplos de CNPJs Válidos para Teste
```
CNPJ: 24.345.700/0001-86
Nome Oficial: HARALD WEINERT
Nome Fantasia: ACAI DO ALEMAO
Status: ATIVA
```

### ❌ O que NÃO Funciona
- CNPJs fictícios ou inventados
- Nomes que não correspondem aos dados oficiais
- CNPJs de empresas baixadas/inativas

---

## 📝 FLUXO COMPLETO DE TESTE

### **PASSO 1: LOGIN COMO EMPRESA CONTRATANTE**

```bash
POST http://localhost:5203/api/Registration/company-admin
{
  "companyName": "HARALD WEINERT",
  "companyCnpj": "24345700000186",
  "companyType": "Client",
  "businessModel": "MainCompany",
  "name": "Harald",
  "email": "contato@acaidoalemao.com.br",
  "password": "123456"
}

POST http://localhost:5203/api/Auth/login
Content-Type: application/json

{
  "email": "contato@acaidoalemao.com.br",
  "password": "123456"
}
```

**Resposta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5ZTlhZTJiNy00ZjU2LTQwYTktYTc2NC01N2ZhMDA1NTY3MjUiLCJlbWFpbCI6ImNvbnRhdG9AYWNhaWRvYWxlbWFvLmNvbS5iciIsInVuaXF1ZV9uYW1lIjoiSGFyYWxkIiwiQ29tcGFueUlkIjoiM2QxYmY4MzAtYjZlNS00ZmFhLTliNzYtMmM5ZmE5ODU3YjA5Iiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzYwMzAyNzkxLCJleHAiOjE3NjAzMDYzOTEsImlhdCI6MTc2MDMwMjc5MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.86LEoDuy_WFnTf0wzZxJrd0wTWkXsn9GPYUuEKQ0gPs",
  "user": {
    "id": "g9e9ae2b7-4f56-40a9-a764-57fa00556725",
    "name": "Herald",
    "email": "contato@acaidoalemao.com.br",
    "role": "Admin",
    "companyId": "3d1bf830-b6e5-4faa-9b76-2c9fa9857b09"
  }
}
```

**📋 ANOTAR:**
- `token`: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5ZTlhZTJiNy00ZjU2LTQwYTktYTc2NC01N2ZhMDA1NTY3MjUiLCJlbWFpbCI6ImNvbnRhdG9AYWNhaWRvYWxlbWFvLmNvbS5iciIsInVuaXF1ZV9uYW1lIjoiSGFyYWxkIiwiQ29tcGFueUlkIjoiM2QxYmY4MzAtYjZlNS00ZmFhLTliNzYtMmM5ZmE5ODU3YjA5Iiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzYwMzAyNzkxLCJleHAiOjE3NjAzMDYzOTEsImlhdCI6MTc2MDMwMjc5MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.86LEoDuy_WFnTf0wzZxJrd0wTWkXsn9GPYUuEKQ0gPs
- `user.companyId`: 3d1bf830-b6e5-4faa-9b76-2c9fa9857b09

---

### **PASSO 2: VERIFICAR USUÁRIOS ATUAIS DA EMPRESA**

```bash
GET http://localhost:5203/api/Users
Authorization: Bearer [SEU_TOKEN_AQUI]
```

**Resposta esperada:**
```json
[
  {
    "id": "guid-usuario",
    "name": "Empresa Teste",
    "email": "admin@empresa.com",
    "role": "Company",
    "companyId": "guid-empresa"
  }
]
```

---

### **PASSO 3: CONVIDAR PJ**

```bash
POST http://localhost:5203/api/Registration/invite-user
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5ZTlhZTJiNy00ZjU2LTQwYTktYTc2NC01N2ZhMDA1NTY3MjUiLCJlbWFpbCI6ImNvbnRhdG9AYWNhaWRvYWxlbWFvLmNvbS5iciIsInVuaXF1ZV9uYW1lIjoiSGFyYWxkIiwiQ29tcGFueUlkIjoiM2QxYmY4MzAtYjZlNS00ZmFhLTliNzYtMmM5ZmE5ODU3YjA5Iiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzYwMzAyNzkxLCJleHAiOjE3NjAzMDYzOTEsImlhdCI6MTc2MDMwMjc5MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.86LEoDuy_WFnTf0wzZxJrd0wTWkXsn9GPYUuEKQ0gPs
Content-Type: application/json

{
  "name": "MTX LABS SUPLEMENTOS", 
  "email": "rh@elisaecalebedocessalgadosme.com.br",
  "inviteType": "ContractedPJ",
  "businessModel": "ContractedPJ",
  "companyName": "MTX LABS SUPLEMENTOS LTDA",
  "cnpj": "49644431000180",
  "companyType": "Provider"
}
{
  "name": "CIDADE NOVA SOLUCOES S.A.", 
  "email": "sistema@patriciaeianinformaticame.com.br",
  "inviteType": "ContractedPJ",
  "businessModel": "ContractedPJ",
  "companyName": "VAKINHA.COM INSTITUICAO DE PAGAMENTO LTDA VAKINHA",
  "cnpj": "22831673000126",
  "companyType": "Provider"
}
```

**Resposta esperada:**
```json
{
  "inviteId": "49901cab-5b90-4836-a743-0bfe7b975e4c",
  "message": "Convite enviado para o email: rh@elisaecalebedocessalgadosme.com.br",
  "inviteToken": "1Ug_UFtfEEumSDF4pEuMlg",
  "expiresAt": "2025-10-19T21:12:14.4070622Z",
  "inviteType": "ContractedPJ"
}
{
  "inviteId": "af7ddb1c-1494-42d9-a60e-af19160d1719",
  "message": "Convite enviado para o email: sistema@patriciaeianinformaticame.com.br",
  "inviteToken": "TZTbIRcHA06xvUIB-XJWow",
  "expiresAt": "2025-10-19T21:41:55.6908139Z",
  "inviteType": "ContractedPJ"
}
```

**📋 ANOTAR:**
- `inviteToken`: 1Ug_UFtfEEumSDF4pEuMlg
- `inviteToken`: TZTbIRcHA06xvUIB-XJWow

---

### **PASSO 4: LISTAR CONVITES PENDENTES**

```bash
GET http://localhost:5203/api/Registration/invites
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5ZTlhZTJiNy00ZjU2LTQwYTktYTc2NC01N2ZhMDA1NTY3MjUiLCJlbWFpbCI6ImNvbnRhdG9AYWNhaWRvYWxlbWFvLmNvbS5iciIsInVuaXF1ZV9uYW1lIjoiSGFyYWxkIiwiQ29tcGFueUlkIjoiM2QxYmY4MzAtYjZlNS00ZmFhLTliNzYtMmM5ZmE5ODU3YjA5Iiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzYwMzAyNzkxLCJleHAiOjE3NjAzMDYzOTEsImlhdCI6MTc2MDMwMjc5MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.86LEoDuy_WFnTf0wzZxJrd0wTWkXsn9GPYUuEKQ0gPs
```

**Resposta esperada:**
```json
[
  {
    "id": "49901cab-5b90-4836-a743-0bfe7b975e4c",
    "inviterName": "Harald",
    "inviteeEmail": "rh@elisaecalebedocessalgadosme.com.br",
    "inviteeName": "MTX LABS SUPLEMENTOS",
    "role": "Provider",
    "inviteType": "ContractedPJ",
    "companyName": "MTX LABS SUPLEMENTOS LTDA",
    "cnpj": "49644431000180",
    "companyType": "Provider",
    "businessModel": "ContractedPJ",
    "token": "1Ug_UFtfEEumSDF4pEuMlg",
    "expiresAt": "2025-10-19T21:12:14.407062Z",
    "createdAt": "2025-10-12T21:12:14.406885Z",
    "isExpired": false
  }
]
[
  {
    "id": "af7ddb1c-1494-42d9-a60e-af19160d1719",
    "inviterName": "Harald",
    "inviteeEmail": "sistema@patriciaeianinformaticame.com.br",
    "inviteeName": "CIDADE NOVA SOLUCOES S.A.",
    "role": "Provider",
    "inviteType": "ContractedPJ",
    "companyName": "VAKINHA.COM INSTITUICAO DE PAGAMENTO LTDA VAKINHA",
    "cnpj": "22831673000126",
    "companyType": "Provider",
    "businessModel": "ContractedPJ",
    "token": "TZTbIRcHA06xvUIB-XJWow",
    "expiresAt": "2025-10-19T21:41:55.690813Z",
    "createdAt": "2025-10-12T21:41:55.690614Z",
    "isExpired": false
  }
]
```

---

### **PASSO 5: ACEITAR CONVITE (SIMULAR PJ ACEITANDO)**

```bash
POST http://localhost:5203/api/Registration/accept-invite/[TOKEN_DO_CONVITE]
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI5ZTlhZTJiNy00ZjU2LTQwYTktYTc2NC01N2ZhMDA1NTY3MjUiLCJlbWFpbCI6ImNvbnRhdG9AYWNhaWRvYWxlbWFvLmNvbS5iciIsInVuaXF1ZV9uYW1lIjoiSGFyYWxkIiwiQ29tcGFueUlkIjoiM2QxYmY4MzAtYjZlNS00ZmFhLTliNzYtMmM5ZmE5ODU3YjA5Iiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzYwMzAyNzkxLCJleHAiOjE3NjAzMDYzOTEsImlhdCI6MTc2MDMwMjc5MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.86LEoDuy_WFnTf0wzZxJrd0wTWkXsn9GPYUuEKQ0gPs
Content-Type: application/json

{
  "password": "123456"
}
{
  "password": "123456"
}
```

**Resposta esperada:**
```json
{
  "id": "f3b4e188-abf9-4a81-a402-599a7221ce95",
  "name": "MTX LABS SUPLEMENTOS",
  "email": "rh@elisaecalebedocessalgadosme.com.br",
  "role": "Provider",
  "companyId": "3f78c688-03b6-4c15-be5f-8c691768bd85",
  "createdAt": "2025-10-12T21:15:27.700211Z",
  "updatedAt": "2025-10-12T21:15:27.7002113Z"
}
{
  "id": "783f3450-e589-4133-8184-257aea1e09ba",
  "name": "CIDADE NOVA SOLUCOES S.A.",
  "email": "sistema@patriciaeianinformaticame.com.br",
  "role": "Provider",
  "companyId": "e52b498a-2f01-4cc3-a51e-c2cca325786a",
  "createdAt": "2025-10-12T21:43:13.9264055Z",
  "updatedAt": "2025-10-12T21:43:13.9264059Z"
}
```

**📋 ANOTAR:**
- `id`: f3b4e188-abf9-4a81-a402-599a7221ce95
- `companyId`: 3f78c688-03b6-4c15-be5f-8c691768bd85

- `id`: 783f3450-e589-4133-8184-257aea1e09ba
- `companyId`: e52b498a-2f01-4cc3-a51e-c2cca325786a
---

### **PASSO 6: LOGIN COMO USUÁRIO PJ**

```bash
POST http://localhost:5203/api/Auth/login
Content-Type: application/json

{
  "email": "rh@elisaecalebedocessalgadosme.com.br",
  "password": "123456"
}
```

**Resposta esperada:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmM2I0ZTE4OC1hYmY5LTRhODEtYTQwMi01OTlhNzIyMWNlOTUiLCJlbWFpbCI6InJoQGVsaXNhZWNhbGViZWRvY2Vzc2FsZ2Fkb3NtZS5jb20uYnIiLCJ1bmlxdWVfbmFtZSI6Ik1UWCBMQUJTIFNVUExFTUVOVE9TIiwiQ29tcGFueUlkIjoiM2Y3OGM2ODgtMDNiNi00YzE1LWJlNWYtOGM2OTE3NjhiZDg1Iiwicm9sZSI6IlByb3ZpZGVyIiwibmJmIjoxNzYwMzAzODQxLCJleHAiOjE3NjAzMDc0NDEsImlhdCI6MTc2MDMwMzg0MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.5zD1RhzLmbQurxyuAzk59-MucvaHNOhcSD-H1yc7neA",
  "refreshToken": "y4XP0822WLKzRqrDSPGZoDvHipfbu7p+/UGJI4ahunfhpcCfq4creRNBkfvvolESZkbZhNnamU85a3yvqW/kSw==",
  "expiresAt": "2025-10-12T22:17:21.9174377Z",
  "user": {
    "id": "f3b4e188-abf9-4a81-a402-599a7221ce95",
    "name": "MTX LABS SUPLEMENTOS",
    "email": "rh@elisaecalebedocessalgadosme.com.br",
    "role": "Provider",
    "companyId": "3f78c688-03b6-4c15-be5f-8c691768bd85",
    "createdAt": "2025-10-12T21:15:27.700211Z",
    "updatedAt": "2025-10-12T21:15:27.700211Z"
  }
}
```

**📋 ANOTAR:**
- `token`: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmM2I0ZTE4OC1hYmY5LTRhODEtYTQwMi01OTlhNzIyMWNlOTUiLCJlbWFpbCI6InJoQGVsaXNhZWNhbGViZWRvY2Vzc2FsZ2Fkb3NtZS5jb20uYnIiLCJ1bmlxdWVfbmFtZSI6Ik1UWCBMQUJTIFNVUExFTUVOVE9TIiwiQ29tcGFueUlkIjoiM2Y3OGM2ODgtMDNiNi00YzE1LWJlNWYtOGM2OTE3NjhiZDg1Iiwicm9sZSI6IlByb3ZpZGVyIiwibmJmIjoxNzYwMzAzODQxLCJleHAiOjE3NjAzMDc0NDEsImlhdCI6MTc2MDMwMzg0MSwiaXNzIjoiQXVyZS5BUEkiLCJhdWQiOiJBdXJlLldlYkFwcCJ9.5zD1RhzLmbQurxyuAzk59-MucvaHNOhcSD-H1yc7neA

---

## 🔍 VERIFICAÇÃO DOS RELACIONAMENTOS

### **TESTE 1: EMPRESA CONTRATANTE VISUALIZA PJs**

**Login como Empresa Teste** e execute:

```bash
GET http://localhost:5203/api/UsersExtended/contracted-pjs
Authorization: Bearer [TOKEN_EMPRESA_TESTE]
```

**Resultado esperado:**
```json
{
  "totalContractedPJs": 1,
  "activeContracts": 1,
  "contractedPJs": [
    {
      "userId": "guid-usuario-pj",
      "name": "João Silva",
      "email": "joao.silva@consultoria.com",
      "role": "Provider",
      "pjCompany": {
        "id": "guid-empresa-pj",
        "name": "João Silva Consultoria ME",
        "cnpj": "12345678000190",
        "businessModel": "ContractedPJ"
      },
      "contractInfo": {
        "relationshipId": "guid-relacionamento",
        "startDate": "2025-10-12T...",
        "status": "Active",
        "notes": "PJ contratado via convite - Usuário: João Silva"
      }
    }
  ]
}
```

---

### **TESTE 2: EMPRESA CONTRATANTE VÊ REDE COMPLETA**

```bash
GET http://localhost:5203/api/UsersExtended/network
Authorization: Bearer [TOKEN_EMPRESA_TESTE]
```

**Resultado esperado:**
```json
{
  "totalUsers": 2,
  "ownCompanyUsers": 1,
  "relatedCompanyUsers": 1,
  "users": [
    {
      "userId": "guid-admin-empresa",
      "name": "Empresa Teste",
      "email": "admin@empresa.com",
      "role": "Company",
      "companyName": "Minha Empresa",
      "relationship": "OwnCompany",
      "isDirectEmployee": true
    },
    {
      "userId": "guid-usuario-pj", 
      "name": "João Silva",
      "email": "joao.silva@consultoria.com",
      "role": "Provider",
      "companyName": "João Silva Consultoria ME",
      "companyCnpj": "12345678000190",
      "businessModel": "ContractedPJ",
      "relationship": "ContractedPJ",
      "relationshipStatus": "Active",
      "isDirectEmployee": false,
      "relationshipNotes": "PJ contratado via convite - Usuário: João Silva"
    }
  ]
}
```

---

### **TESTE 3: PJ VISUALIZA QUEM O CONTRATOU**

**Login como João Silva** e execute:

```bash
GET http://localhost:5203/api/UsersExtended/contracted-by
Authorization: Bearer [TOKEN_PJ]
```

**Resultado esperado:**
```json
{
  "totalContracts": 1,
  "contractedBy": [
    {
      "relationshipId": "guid-relacionamento",
      "clientCompany": {
        "id": "guid-empresa-teste",
        "name": "Empresa Teste",
        "cnpj": "cnpj-empresa-teste",
        "businessModel": "MainCompany"
      },
      "contractInfo": {
        "startDate": "2025-10-12T...",
        "status": "Active",
        "notes": "PJ contratado via convite - Usuário: João Silva"
      }
    }
  ]
}
```

---

### **TESTE 4: RELACIONAMENTOS BIDIRECIONAIS**

```bash
GET http://localhost:5203/api/CompanyRelationships/as-client
Authorization: Bearer [TOKEN_EMPRESA_TESTE]
```

**Resultado esperado:**
```json
[
  {
    "id": "guid-relacionamento",
    "type": "ContractedPJ",
    "status": "Active", 
    "startDate": "2025-10-12T...",
    "contractedCompany": {
      "id": "guid-empresa-pj",
      "name": "João Silva Consultoria ME",
      "cnpj": "12345678000190",
      "businessModel": "ContractedPJ"
    }
  }
]
```

---

## ✅ CHECKLIST DE VALIDAÇÃO

### **Funcionalidades a verificar:**

- [ ] **Convite criado** - POST /api/Registration/invite-user
- [ ] **Convite aceito** - POST /api/Registration/accept-invite/{token}
- [ ] **Usuário PJ criado** - Nova conta funcional
- [ ] **Empresa PJ criada** - Nova empresa no sistema
- [ ] **Relacionamento criado** - Registro na tabela CompanyRelationships
- [ ] **Empresa vê PJs contratados** - GET /api/UsersExtended/contracted-pjs
- [ ] **PJ vê quem o contratou** - GET /api/UsersExtended/contracted-by
- [ ] **Rede completa visível** - GET /api/UsersExtended/network
- [ ] **Relacionamentos bidirecionais** - GET /api/CompanyRelationships/*

### **Dados a conferir no banco:**

```sql
-- Verificar usuário PJ criado
SELECT * FROM users WHERE email = 'joao.silva@consultoria.com';

-- Verificar empresa PJ criada  
SELECT * FROM companies WHERE cnpj = '12345678000190';

-- Verificar relacionamento criado
SELECT cr.*, 
       c1.name as client_name, 
       c2.name as provider_name
FROM companyrelationships cr
JOIN companies c1 ON cr.client_company_id = c1.id
JOIN companies c2 ON cr.provider_company_id = c2.id
WHERE cr.type = 1; -- ContractedPJ
```

---

## 🐛 POSSÍVEIS PROBLEMAS E SOLUÇÕES

### **Erro 1: "Invalid invite token"**
**Causa:** Token expirado ou incorreto
**Solução:** Verificar se o token foi copiado corretamente e não expirou

### **Erro 2: "User not associated with any company"**
**Causa:** Token inválido ou usuário sem empresa
**Solução:** Refazer login e verificar token JWT

### **Erro 3: "CNPJ validation failed"**
**Causa:** CNPJ em formato incorreto
**Solução:** Usar apenas números: "12345678000190"

### **Erro 4: Relacionamento não criado**
**Causa:** Erro na transação do AcceptInvite
**Solução:** Verificar logs da aplicação e banco de dados

---

## 📊 RESULTADOS ESPERADOS

Após executar todo o fluxo, você deve ter:

1. **2 empresas no sistema:**
   - Empresa Teste (contratante)
   - João Silva Consultoria ME (PJ contratado)

2. **2 usuários no sistema:**
   - Admin da Empresa Teste
   - João Silva (usuário PJ)

3. **1 relacionamento ativo:**
   - Empresa Teste → contrata → João Silva Consultoria ME
   - Tipo: ContractedPJ
   - Status: Active

4. **Visibilidade completa:**
   - Empresa Teste vê João Silva como PJ contratado
   - João Silva vê Empresa Teste como contratante
   - Ambos têm acesso à rede de relacionamentos

---

## 🎯 CONCLUSÃO

Se todos os testes passarem, o sistema está funcionando corretamente e a **vinculação PJ ↔ Empresa está 100% clara e funcional!**

A questão que você levantou ("como eu faço para saber que esse usuario pj está vinculado com a empresa que o convidou?") está **completamente resolvida** através dos novos endpoints e da tabela CompanyRelationships.

**🚀 Agora teste e me informe os resultados!**