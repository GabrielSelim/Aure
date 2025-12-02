# 📋 Endpoints de Gestão de Funcionários - Sistema Aure

## 🎯 Objetivo
Este documento descreve os endpoints disponíveis para que **Proprietários** e **Jurídico** possam gerenciar informações dos funcionários, respeitando a hierarquia de permissões do sistema.

---

## 🔐 Matriz de Permissões

### Quem Pode Alterar O Quê?

| Ação | Proprietário | Jurídico | Financeiro | Próprio Funcionário |
|------|--------------|----------|------------|-------------------|
| **Alterar Cargo** | ✅ | ✅ | ❌ | ❌ |
| **Desativar Funcionário** | ✅ | ❌ | ❌ | ❌ |
| **Ver Dados Completos** | ✅ | ✅ | ✅ | ❌ (apenas próprios) |
| **Alterar Dados Pessoais** | ❌ | ❌ | ❌ | ✅ (apenas próprios) |
| **Alterar Senha** | ❌ | ❌ | ❌ | ✅ (apenas própria) |

**Regras Críticas:**
- ❌ Ninguém pode alterar o cargo do **Proprietário**
- ❌ Funcionários **não podem alterar** seu próprio cargo
- ✅ Funcionários podem atualizar apenas seus **dados pessoais e perfil**
- ✅ Proprietário e Jurídico podem **visualizar** dados de todos os funcionários
- ✅ Proprietário e Jurídico podem **alterar cargos** de funcionários (exceto do proprietário)

---

## 📍 Endpoints Disponíveis

### 1️⃣ **Alterar Cargo de Funcionário**

**Endpoint:** `PUT /api/Users/{employeeId}/cargo`

**Permissões:** 
- ✅ DonoEmpresaPai
- ✅ Juridico (precisa implementar)
- ❌ Financeiro
- ❌ FuncionarioCLT
- ❌ FuncionarioPJ

**Request:**
```json
{
  "cargo": "Gerente de Vendas"
}
```

**Response 200 OK:**
```json
{
  "id": "uuid-do-funcionario",
  "name": "João Silva",
  "email": "joao@empresa.com",
  "role": "FuncionarioCLT",
  "cargo": "Gerente de Vendas",
  "companyId": "uuid-da-empresa",
  "isActive": true,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

**Validações:**
- Cargo não pode ser vazio
- Cargo deve ter no máximo 100 caracteres
- Funcionário deve pertencer à mesma empresa
- Não é possível alterar cargo do proprietário

**Exemplo cURL:**
```bash
curl -X PUT "https://aureapi.gabrielsanztech.com.br/api/Users/uuid-funcionario/cargo" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{"cargo": "Gerente de Vendas"}'
```

**Códigos de Erro:**
- `400` - Cargo inválido ou vazio
- `401` - Não autenticado ou sem permissão
- `404` - Funcionário não encontrado

---

### 2️⃣ **Visualizar Funcionário por ID**

**Endpoint:** `GET /api/Users/{userId}`

**Permissões:** 
- ✅ DonoEmpresaPai (todos os funcionários da empresa)
- ✅ Juridico (todos os funcionários da empresa)
- ✅ Financeiro (todos os funcionários da empresa)
- ✅ FuncionarioCLT (apenas próprio perfil)
- ✅ FuncionarioPJ (apenas próprio perfil)

**Response 200 OK:**
```json
{
  "id": "uuid-do-funcionario",
  "name": "João Silva",
  "email": "joao@empresa.com",
  "role": "FuncionarioCLT",
  "cargo": "Analista de TI",
  "cpf": "***456***12",
  "telefoneCelular": "11987654321",
  "telefoneFixo": "1133334444",
  "endereco": {
    "rua": "Rua Exemplo",
    "numero": "100",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "cep": "01310000"
  },
  "dataNascimento": "1990-01-01",
  "companyId": "uuid-da-empresa",
  "isActive": true,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

**Exemplo cURL:**
```bash
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Users/uuid-funcionario" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

---

### 3️⃣ **Listar Todos os Funcionários**

**Endpoint:** `GET /api/Users`

**Query Parameters:**
- `role` (opcional): Filtrar por tipo (FuncionarioCLT, FuncionarioPJ, etc.)
- `isActive` (opcional): Filtrar por status ativo/inativo

**Permissões:** 
- ✅ DonoEmpresaPai
- ✅ Juridico
- ✅ Financeiro

**Response 200 OK:**
```json
{
  "totalUsuarios": 15,
  "usuarios": [
    {
      "id": "uuid1",
      "name": "João Silva",
      "email": "joao@empresa.com",
      "role": "FuncionarioCLT",
      "cargo": "Analista de TI",
      "isActive": true
    },
    {
      "id": "uuid2",
      "name": "Maria Santos",
      "email": "maria@empresa.com",
      "role": "FuncionarioPJ",
      "cargo": "Consultora Jurídica",
      "isActive": true
    }
  ]
}
```

**Exemplo cURL:**
```bash
# Listar todos
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Users" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"

# Filtrar por CLT ativos
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Users?role=FuncionarioCLT&isActive=true" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

---

### 4️⃣ **Desativar Funcionário**

**Endpoint:** `DELETE /api/Users/{userId}`

**Permissões:** 
- ✅ DonoEmpresaPai (apenas proprietário pode desativar)
- ❌ Juridico
- ❌ Financeiro

**Response 200 OK:**
```json
{
  "message": "Usuário desativado com sucesso"
}
```

**Validações:**
- Não é possível desativar o próprio proprietário
- Não é possível desativar usuário de outra empresa
- Desativação é lógica (soft delete), não remove do banco

**Exemplo cURL:**
```bash
curl -X DELETE "https://aureapi.gabrielsanztech.com.br/api/Users/uuid-funcionario" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Códigos de Erro:**
- `400` - Tentativa de desativar o proprietário
- `401` - Não autenticado ou sem permissão
- `404` - Funcionário não encontrado

---

## 🚫 O Que Funcionários **NÃO PODEM** Fazer

### ❌ Funcionário Não Pode Alterar Próprio Cargo

**Endpoint Bloqueado:** `PUT /api/Users/{employeeId}/cargo`

**Se tentar:**
```json
{
  "erro": "Apenas o dono da empresa pode alterar cargos"
}
```

### ✅ Funcionário Pode Alterar Próprios Dados Pessoais

**Endpoint Permitido:** `PUT /api/UserProfile/perfil-completo`

**Campos que funcionário pode alterar:**
- Nome
- Email
- Telefone celular e fixo
- Data de nascimento
- Endereço completo
- Senha (com confirmação da senha atual)
- CPF/RG

**Campos que funcionário NÃO pode alterar:**
- **Cargo** (somente Proprietário/Jurídico)
- **Role** (tipo de usuário)
- **CompanyId** (empresa vinculada)
- **IsActive** (status ativo/inativo)

---

## 🛠️ Implementações Necessárias

### ⚠️ AJUSTE CRÍTICO: Permitir Jurídico Alterar Cargos

**Arquivo:** `src/Aure.API/Controllers/UsersController.cs`

**Linha 305:** Alterar de:
```csharp
[Authorize(Roles = "DonoEmpresaPai")]
```

**Para:**
```csharp
[Authorize(Roles = "DonoEmpresaPai,Juridico")]
```

**Arquivo:** `src/Aure.Application/Services/UserService.cs`

**Linha 1334:** Alterar de:
```csharp
if (requestingUser.Role != UserRole.DonoEmpresaPai)
    return Result.Failure<UserResponse>("Apenas o dono da empresa pode alterar cargos");
```

**Para:**
```csharp
if (requestingUser.Role != UserRole.DonoEmpresaPai && requestingUser.Role != UserRole.Juridico)
    return Result.Failure<UserResponse>("Apenas o dono da empresa ou jurídico podem alterar cargos");
```

---

## 📝 Fluxo Recomendado no Frontend

### Tela de Gestão de Funcionários (Proprietário/Jurídico)

```typescript
// 1. Listar funcionários
const funcionarios = await api.get('/api/Users');

// 2. Ver detalhes do funcionário
const detalhe = await api.get(`/api/Users/${funcionarioId}`);

// 3. Alterar cargo (se Proprietário ou Jurídico)
if (userRole === 'DonoEmpresaPai' || userRole === 'Juridico') {
  await api.put(`/api/Users/${funcionarioId}/cargo`, {
    cargo: 'Novo Cargo'
  });
}

// 4. Desativar funcionário (somente Proprietário)
if (userRole === 'DonoEmpresaPai') {
  await api.delete(`/api/Users/${funcionarioId}`);
}
```

### Tela de Perfil (Funcionário)

```typescript
// Funcionário atualiza APENAS próprios dados
await api.put('/api/UserProfile/perfil-completo', {
  name: 'Novo Nome',
  email: 'novoemail@empresa.com',
  telefoneCelular: '11987654321',
  // NÃO incluir campo "cargo" - será ignorado ou dará erro
  // cargo: 'Gerente' ❌ PROIBIDO
});
```

---

## 🔍 Validações de Segurança Backend

### No Controller (UsersController.cs)

```csharp
// Linha 305: Autorização via atributo
[Authorize(Roles = "DonoEmpresaPai,Juridico")]
```

### No Service (UserService.cs)

```csharp
// Linha 1334: Validação adicional
if (requestingUser.Role != UserRole.DonoEmpresaPai && 
    requestingUser.Role != UserRole.Juridico)
{
    return Result.Failure<UserResponse>(
        "Apenas o dono da empresa ou jurídico podem alterar cargos"
    );
}

// Linha 1342: Impedir alteração do próprio cargo do proprietário
if (employee.Role == UserRole.DonoEmpresaPai)
{
    return Result.Failure<UserResponse>(
        "Não é possível alterar o cargo do proprietário"
    );
}

// Linha 1339: Validar empresa
if (employee.CompanyId != requestingUser.CompanyId)
{
    return Result.Failure<UserResponse>(
        "Você só pode alterar cargos de funcionários da sua empresa"
    );
}
```

---

## 📊 Resumo de Endpoints

| Endpoint | Método | Quem Pode Usar | Finalidade |
|----------|--------|----------------|------------|
| `/api/Users` | GET | Dono, Jurídico, Financeiro | Listar funcionários |
| `/api/Users/{id}` | GET | Dono, Jurídico, Financeiro, Próprio | Ver detalhes |
| `/api/Users/{id}/cargo` | PUT | Dono, Jurídico | **Alterar cargo** |
| `/api/Users/{id}` | DELETE | Dono | Desativar funcionário |
| `/api/UserProfile/perfil-completo` | PUT | Todos | Atualizar próprio perfil |

---

## ✅ Checklist de Implementação

- [ ] Ajustar `[Authorize(Roles = "DonoEmpresaPai,Juridico")]` em `UsersController.cs` linha 305
- [ ] Ajustar validação `Role != Juridico` em `UserService.cs` linha 1334
- [ ] Testar endpoint PUT `/api/Users/{id}/cargo` com usuário Jurídico
- [ ] Validar que funcionário NÃO consegue alterar próprio cargo
- [ ] Testar endpoint PUT `/api/UserProfile/perfil-completo` (funcionário altera próprios dados)
- [ ] Documentar no frontend quais campos são editáveis por cada role

---

## 🚀 Comandos de Teste

### Teste 1: Proprietário Altera Cargo
```bash
# Login como Proprietário
TOKEN=$(curl -s -X POST "https://aureapi.gabrielsanztech.com.br/api/Auth/entrar" \
  -H "Content-Type: application/json" \
  -d '{"email":"gabrielsanz2002@gmail.com","password":"SuaSenha123"}' \
  | jq -r '.token')

# Alterar cargo
curl -X PUT "https://aureapi.gabrielsanztech.com.br/api/Users/uuid-funcionario/cargo" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"cargo":"Gerente Comercial"}'
```

### Teste 2: Jurídico Altera Cargo (Após Implementação)
```bash
# Login como Jurídico
TOKEN_JURIDICO=$(curl -s -X POST "https://aureapi.gabrielsanztech.com.br/api/Auth/entrar" \
  -H "Content-Type: application/json" \
  -d '{"email":"juridico@petrobras.com","password":"Senha123"}' \
  | jq -r '.token')

# Alterar cargo
curl -X PUT "https://aureapi.gabrielsanztech.com.br/api/Users/uuid-funcionario/cargo" \
  -H "Authorization: Bearer $TOKEN_JURIDICO" \
  -H "Content-Type: application/json" \
  -d '{"cargo":"Analista Jurídico"}'
```

### Teste 3: Funcionário Tenta Alterar Próprio Cargo (Deve Falhar)
```bash
# Login como Funcionário
TOKEN_FUNC=$(curl -s -X POST "https://aureapi.gabrielsanztech.com.br/api/Auth/entrar" \
  -H "Content-Type: application/json" \
  -d '{"email":"funcionario@petrobras.com","password":"Senha123"}' \
  | jq -r '.token')

# Tentar alterar próprio cargo (DEVE RETORNAR 401/403)
curl -X PUT "https://aureapi.gabrielsanztech.com.br/api/Users/seu-proprio-id/cargo" \
  -H "Authorization: Bearer $TOKEN_FUNC" \
  -H "Content-Type: application/json" \
  -d '{"cargo":"CEO"}' # ❌ BLOQUEADO
```

---

**Data de Criação:** 02/12/2025  
**Última Atualização:** 02/12/2025  
**Status:** ✅ Documentado | ⚠️ Ajuste pendente (permitir Jurídico alterar cargos)
