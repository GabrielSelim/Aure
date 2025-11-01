# 🧪 Roteiro Completo de Testes - API Aure

**Data**: 31/10/2025  
**Ambiente**: Production - https://aureapi.gabrielsanztech.com.br

---

## 📋 Pré-requisitos

### Ferramentas:
- Postman ou Insomnia
- Editor de texto
- Acesso ao servidor SSH (para verificações)

### Variáveis de Ambiente (Postman):
```json
{
  "base_url": "https://aureapi.gabrielsanztech.com.br",
  "token_dono": "",
  "token_financeiro": "",
  "token_pj": "",
  "userId_dono": "",
  "userId_financeiro": "",
  "userId_pj": "",
  "companyId": "",
  "inviteToken_financeiro": "",
  "inviteToken_pj": ""
}
```

---

## 🎯 FASE 1: Testes Críticos (Prioritários)

### ✅ 1.1. Registro de Empresa e Dono

**Endpoint**: `POST {{base_url}}/api/registration/admin-empresa`

**Body**:
```json
{
  "companyName": "Empresa Teste Automação Ltda",
  "companyCnpj": "88899900011122",
  "companyType": "Client",
  "businessModel": "MainCompany",
  "name": "Maria Dona Teste",
  "email": "maria.teste.{{$timestamp}}@empresateste.com",
  "password": "SenhaTeste@123",
  "cpf": "12345678901",
  "dataNascimento": "1990-05-15",
  "telefoneCelular": "11987654321",
  "rua": "Rua Teste",
  "numero": "100",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "01310000",
  "aceitouTermosUso": true,
  "versaoTermosUsoAceita": "1.0",
  "aceitouPoliticaPrivacidade": true,
  "versaoPoliticaPrivacidadeAceita": "1.0"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Response contém: `id`, `name`, `email`, `role: "DonoEmpresaPai"`, `companyId`, `isActive: true`
- ✅ Armazenar em variáveis: `userId_dono`, `companyId`

**Script Postman (Tests)**:
```javascript
pm.test("Status 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Dono criado com sucesso", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.role).to.eql("DonoEmpresaPai");
    pm.expect(jsonData.isActive).to.be.true;
    pm.environment.set("userId_dono", jsonData.id);
    pm.environment.set("companyId", jsonData.companyId);
});
```

---

### ✅ 1.2. Login do Dono

**Endpoint**: `POST {{base_url}}/api/auth/login`

**Body**:
```json
{
  "email": "{{email_dono}}",
  "password": "SenhaTeste@123"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Response contém: `token`, `refreshToken`, `user`
- ✅ Armazenar token em: `token_dono`

**Script Postman**:
```javascript
pm.test("Login bem-sucedido", function () {
    pm.response.to.have.status(200);
    var jsonData = pm.response.json();
    pm.expect(jsonData.token).to.be.a('string');
    pm.environment.set("token_dono", jsonData.token);
});
```

---

### ✅ 1.3. Convidar Funcionário Financeiro

**Endpoint**: `POST {{base_url}}/api/registration/convidar-usuario`

**Headers**:
```
Authorization: Bearer {{token_dono}}
Content-Type: application/json
```

**Body**:
```json
{
  "name": "João Financeiro",
  "email": "joao.financeiro.{{$timestamp}}@empresateste.com",
  "role": "Financeiro",
  "inviteType": "Internal",
  "cargo": "Gerente Financeiro"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Response contém: `id`, `token`, `inviteeEmail`, `role: "Financeiro"`
- ✅ Armazenar: `inviteToken_financeiro`

**Script Postman**:
```javascript
pm.test("Convite criado", function () {
    pm.response.to.have.status(200);
    var jsonData = pm.response.json();
    pm.expect(jsonData.role).to.eql("Financeiro");
    pm.environment.set("inviteToken_financeiro", jsonData.token);
});
```

---

### ✅ 1.4. Convidar Funcionário PJ (TESTE CRÍTICO)

**Endpoint**: `POST {{base_url}}/api/registration/convidar-usuario`

**Headers**:
```
Authorization: Bearer {{token_dono}}
Content-Type: application/json
```

**Body**:
```json
{
  "name": "Ana PJ Consultoria",
  "email": "ana.pj.{{$timestamp}}@consultoria.com",
  "role": "FuncionarioPJ",
  "inviteType": "ContractedPJ",
  "companyName": "Ana Consultoria ME",
  "cnpj": "12345678000190",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Response contém: `id`, `token`, `role: "FuncionarioPJ"`, `companyName`, `cnpj`
- ✅ Armazenar: `inviteToken_pj`
- ✅ **VERIFICAR NO BANCO**: CompanyRelationship criado com status "Pending"

**Script Postman**:
```javascript
pm.test("Convite PJ criado", function () {
    pm.response.to.have.status(200);
    var jsonData = pm.response.json();
    pm.expect(jsonData.role).to.eql("FuncionarioPJ");
    pm.expect(jsonData.inviteType).to.eql("ContractedPJ");
    pm.expect(jsonData.companyName).to.exist;
    pm.environment.set("inviteToken_pj", jsonData.token);
});
```

**Verificação Manual no Banco**:
```sql
-- No servidor
docker exec -it aure-postgres-aure-gabriel psql -U aure_user -d aure_production

-- Verificar convite criado
SELECT * FROM "UserInvites" 
WHERE "InviteeEmail" LIKE 'ana.pj%' 
ORDER BY "CreatedAt" DESC 
LIMIT 1;

-- Verificar empresa PJ NÃO foi criada ainda (só cria ao aceitar)
SELECT * FROM "Companies" WHERE "Name" LIKE 'Ana Consultoria%';

-- Deve retornar vazio ✅
```

---

### ✅ 1.5. Aceitar Convite Financeiro

**Endpoint**: `POST {{base_url}}/api/registration/aceitar-convite/{{inviteToken_financeiro}}`

**Body**:
```json
{
  "password": "SenhaFinanceiro@123",
  "cpf": "98765432100",
  "dataNascimento": "1992-08-20",
  "telefoneCelular": "11987654322",
  "rua": "Rua Financeiro",
  "numero": "200",
  "bairro": "Jardim",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "02020000",
  "aceitouTermosUso": true,
  "versaoTermosUsoAceita": "1.0",
  "aceitouPoliticaPrivacidade": true,
  "versaoPoliticaPrivacidadeAceita": "1.0"
}
```

**Validações**:
- ✅ Status: 200
- ✅ `isActive: true`, `role: "Financeiro"`
- ✅ Armazenar: `userId_financeiro`

---

### ✅ 1.6. Aceitar Convite PJ (TESTE CRÍTICO)

**Endpoint**: `POST {{base_url}}/api/registration/aceitar-convite/{{inviteToken_pj}}`

**Body**:
```json
{
  "password": "SenhaPJ@123",
  "cpf": "11122233344",
  "dataNascimento": "1988-03-10",
  "telefoneCelular": "11987654323",
  "rua": "Rua PJ",
  "numero": "300",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "03030000",
  "aceitouTermosUso": true,
  "versaoTermosUsoAceita": "1.0",
  "aceitouPoliticaPrivacidade": true,
  "versaoPoliticaPrivacidadeAceita": "1.0"
}
```

**Validações**:
- ✅ Status: 200
- ✅ `isActive: true`, `role: "FuncionarioPJ"`
- ✅ Armazenar: `userId_pj`

**Verificação Manual no Banco** (CRÍTICA):
```sql
-- Verificar empresa PJ foi criada
SELECT * FROM "Companies" WHERE "Name" = 'Ana Consultoria ME';

-- Copiar Id da empresa PJ
-- Exemplo: pj_company_id = '123e4567-e89b-12d3-a456-426614174000'

-- Verificar relacionamento foi criado
SELECT * FROM "CompanyRelationships" 
WHERE "ProviderCompanyId" = 'pj_company_id';

-- Deve retornar:
-- ClientCompanyId = {companyId do dono}
-- ProviderCompanyId = {pj_company_id}
-- Type = 'ContractedPJ'
-- Status = 'Active' ou 'Pending'
```

---

### ✅ 1.7. Login PJ

**Endpoint**: `POST {{base_url}}/api/auth/login`

**Body**:
```json
{
  "email": "{{email_pj}}",
  "password": "SenhaPJ@123"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Armazenar: `token_pj`

---

### ✅ 1.8. Verificar Relacionamentos (Como Dono)

**Endpoint**: `GET {{base_url}}/api/CompanyRelationships`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Array contém pelo menos 1 relacionamento
- ✅ Relacionamento tem: `type: "ContractedPJ"`, `providerCompany.name: "Ana Consultoria ME"`

**Script Postman**:
```javascript
pm.test("Relacionamentos encontrados", function () {
    pm.response.to.have.status(200);
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('array');
    pm.expect(jsonData.length).to.be.at.least(1);
    
    var pjRelationship = jsonData.find(r => r.type === "ContractedPJ");
    pm.expect(pjRelationship).to.exist;
});
```

---

### ✅ 1.9. Upload de Avatar (TESTE CRÍTICO)

**Endpoint**: `POST {{base_url}}/api/UserProfile/avatar`

**Headers**:
```
Authorization: Bearer {{token_dono}}
Content-Type: multipart/form-data
```

**Body**:
- Key: `file` (tipo: file)
- Value: Selecionar imagem JPG ou PNG (< 5MB)

**Validações**:
- ✅ Status: 200
- ✅ Response contém: `avatarUrl`, `thumbnailUrl`
- ✅ URLs começam com `/uploads/avatars/`

**Verificação Manual**:
```bash
# No servidor
ssh root@5.189.174.61

docker exec -it aure-api-aure-gabriel ls -la /app/wwwroot/uploads/avatars/

# Deve listar:
# {userId_dono}.jpg
# {userId_dono}_thumb.jpg
```

**Teste de Acesso à Imagem**:
```
GET https://aureapi.gabrielsanztech.com.br/uploads/avatars/{{userId_dono}}.jpg
```
**Esperado**: Imagem exibida no navegador

---

## 🧪 FASE 2: Testes de Rede e Permissões

### ✅ 2.1. Rede Completa (Como Dono)

**Endpoint**: `GET {{base_url}}/api/UsersExtended/rede`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ `totalUsuarios >= 3` (Dono, Financeiro, PJ)
- ✅ Array de usuários contém todos os 3

---

### ✅ 2.2. Rede Limitada (Como PJ)

**Endpoint**: `GET {{base_url}}/api/UsersExtended/rede`

**Headers**:
```
Authorization: Bearer {{token_pj}}
```

**Validações**:
- ✅ Status: 200
- ✅ PJ vê apenas: Próprio usuário + Usuários da empresa contratante (Dono e Financeiro)
- ✅ `totalUsuarios = 3`

---

### ✅ 2.3. PJs Contratados (Como Dono)

**Endpoint**: `GET {{base_url}}/api/UsersExtended/pjs-contratados`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Array contém: Ana PJ
- ✅ Dados incluem: `companyName: "Ana Consultoria ME"`

---

### ✅ 2.4. Quem Me Contratou (Como PJ)

**Endpoint**: `GET {{base_url}}/api/UsersExtended/contratado-por`

**Headers**:
```
Authorization: Bearer {{token_pj}}
```

**Validações**:
- ✅ Status: 200
- ✅ Array contém: Empresa do Dono
- ✅ `companyName: "Empresa Teste Automação Ltda"`

---

### ✅ 2.5. Dados da Empresa (Todos podem ver)

**Endpoint**: `GET {{base_url}}/api/UserProfile/empresa`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Contém: `nome`, `cnpj`, `cnpjFormatado`, `tipo`, `modeloNegocio`, `endereco`

**Repetir com**:
- ✅ `{{token_financeiro}}` → Deve funcionar (200)
- ✅ `{{token_pj}}` → Deve retornar dados da própria empresa PJ (200)

---

### ✅ 2.6. Atualizar Empresa (Apenas Dono)

**Endpoint**: `PUT {{base_url}}/api/UserProfile/empresa`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Body**:
```json
{
  "nome": "Empresa Teste Automação Atualizada Ltda",
  "telefoneCelular": "11999999999",
  "rua": "Rua Atualizada",
  "numero": "500",
  "bairro": "Vila Nova",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "05050000"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Campos atualizados corretamente

**Teste de Segurança**:
```
# Repetir com token_financeiro
PUT {{base_url}}/api/UserProfile/empresa
Authorization: Bearer {{token_financeiro}}
Body: { ... }

Esperado: 403 Forbidden ✅
```

---

### ✅ 2.7. Alterar Cargo de Funcionário (Apenas Dono)

**Endpoint**: `PUT {{base_url}}/api/Users/{{userId_financeiro}}/cargo`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Body**:
```json
{
  "cargo": "Diretor Financeiro"
}
```

**Validações**:
- ✅ Status: 200
- ✅ Cargo atualizado

**Teste de Segurança**:
```
PUT {{base_url}}/api/Users/{{userId_financeiro}}/cargo
Authorization: Bearer {{token_financeiro}}

Esperado: 403 Forbidden ✅
```

---

## 🔐 FASE 3: Testes de Segurança e Rejeições

### ❌ 3.1. PJ Tenta Convidar Usuário

**Endpoint**: `POST {{base_url}}/api/registration/convidar-usuario`

**Headers**:
```
Authorization: Bearer {{token_pj}}
```

**Body**: (qualquer)

**Esperado**:
- ❌ Status: 403 Forbidden

---

### ❌ 3.2. PJ Tenta Ver Logs de Auditoria

**Endpoint**: `GET {{base_url}}/api/Audit/logs`

**Headers**:
```
Authorization: Bearer {{token_pj}}
```

**Esperado**:
- ❌ Status: 403 Forbidden

---

### ❌ 3.3. Financeiro Tenta Alterar Cargo

**Endpoint**: `PUT {{base_url}}/api/Users/{{userId_pj}}/cargo`

**Headers**:
```
Authorization: Bearer {{token_financeiro}}
```

**Esperado**:
- ❌ Status: 403 Forbidden

---

### ❌ 3.4. Financeiro Tenta Atualizar Empresa

**Endpoint**: `PUT {{base_url}}/api/UserProfile/empresa`

**Headers**:
```
Authorization: Bearer {{token_financeiro}}
```

**Esperado**:
- ❌ Status: 403 Forbidden

---

## 📊 FASE 4: Testes de Listagem e Filtros

### ✅ 4.1. Listar Funcionários (Com Paginação)

**Endpoint**: `GET {{base_url}}/api/Users/funcionarios?pageNumber=1&pageSize=10`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Contém: `data`, `pageNumber`, `pageSize`, `totalRecords`, `hasNextPage`
- ✅ `totalRecords >= 2` (Financeiro + PJ)

---

### ✅ 4.2. Filtrar por Role

**Endpoint**: `GET {{base_url}}/api/Users/funcionarios?role=FuncionarioPJ`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ `totalRecords = 1`
- ✅ Único usuário retornado é PJ

---

### ✅ 4.3. Buscar por Nome

**Endpoint**: `GET {{base_url}}/api/Users/funcionarios?busca=Ana`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Retorna apenas Ana PJ

---

### ✅ 4.4. Listar Convites Pendentes

**Endpoint**: `GET {{base_url}}/api/registration/convites`

**Headers**:
```
Authorization: Bearer {{token_dono}}
```

**Validações**:
- ✅ Status: 200
- ✅ Array vazio (todos os convites foram aceitos)

---

## 📈 Resumo de Resultados Esperados

### Matriz de Testes:

| # | Teste | Status Esperado | Prioridade |
|---|-------|-----------------|------------|
| 1.1 | Registrar Dono | ✅ 200 | CRÍTICA |
| 1.2 | Login Dono | ✅ 200 | CRÍTICA |
| 1.3 | Convidar Financeiro | ✅ 200 | CRÍTICA |
| 1.4 | Convidar PJ | ✅ 200 | **CRÍTICA** |
| 1.5 | Aceitar Convite Financeiro | ✅ 200 | ALTA |
| 1.6 | Aceitar Convite PJ | ✅ 200 | **CRÍTICA** |
| 1.7 | Login PJ | ✅ 200 | ALTA |
| 1.8 | Verificar Relacionamentos | ✅ 200 | **CRÍTICA** |
| 1.9 | Upload Avatar | ✅ 200 | **CRÍTICA** |
| 2.1 | Rede Completa (Dono) | ✅ 200 | MÉDIA |
| 2.2 | Rede Limitada (PJ) | ✅ 200 | MÉDIA |
| 2.3 | PJs Contratados | ✅ 200 | MÉDIA |
| 2.4 | Quem Me Contratou | ✅ 200 | MÉDIA |
| 2.5 | Dados da Empresa | ✅ 200 | ALTA |
| 2.6 | Atualizar Empresa | ✅ 200 | ALTA |
| 2.7 | Alterar Cargo | ✅ 200 | MÉDIA |
| 3.1 | PJ Convidar (Segurança) | ❌ 403 | ALTA |
| 3.2 | PJ Ver Auditoria (Segurança) | ❌ 403 | ALTA |
| 3.3 | Financeiro Alterar Cargo (Segurança) | ❌ 403 | ALTA |
| 3.4 | Financeiro Atualizar Empresa (Segurança) | ❌ 403 | ALTA |
| 4.1 | Listar Funcionários | ✅ 200 | MÉDIA |
| 4.2 | Filtrar por Role | ✅ 200 | BAIXA |
| 4.3 | Buscar por Nome | ✅ 200 | BAIXA |
| 4.4 | Listar Convites | ✅ 200 | BAIXA |

---

## 📝 Checklist de Execução

### Antes de Começar:
- [ ] Certificar que API está rodando (health check)
- [ ] Limpar banco de dados de teste (opcional)
- [ ] Configurar variáveis de ambiente no Postman
- [ ] Ter imagem de teste para avatar (< 5MB, JPG/PNG)

### Durante os Testes:
- [ ] Executar testes na ordem (1.1 → 4.4)
- [ ] Armazenar tokens e IDs nas variáveis
- [ ] Anotar qualquer erro ou comportamento inesperado
- [ ] Verificar banco de dados quando solicitado

### Após os Testes:
- [ ] Compilar resultados (✅ Passou / ❌ Falhou)
- [ ] Documentar erros encontrados
- [ ] Verificar logs no servidor se necessário
- [ ] Limpar dados de teste (opcional)

---

## 🚨 Erros Conhecidos a Verificar

1. **Avatar Upload**: Se falhar com "Value cannot be null (Parameter 'path1')" → Aplicar fix do documento FIX.AvatarUpload.instructions.md
2. **Convite PJ**: Se falhar ou não criar CompanyRelationship → Verificar código do AcceptInviteAsync
3. **CompanyRelationships vazio**: Se endpoint retornar [], verificar se convites PJ foram aceitos

---

**Tempo Estimado**: 45-60 minutos para execução completa  
**Dificuldade**: Média  
**Requer**: Postman + Acesso SSH ao servidor
