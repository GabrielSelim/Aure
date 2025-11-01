# 💡 Respostas às Suas Dúvidas - Sistema Aure

**Data**: 31/10/2025

---

## ❓ Dúvida 1: Por que aparece "Role" ao enviar convite?

### 📋 Resposta:

Sim, **é necessário enviar o campo `Role`** ao convidar um usuário, MAS ele é **calculado automaticamente** com base no `InviteType`.

### Fluxo Atual no Código:

```csharp
// Em UserService.cs, linha 377:
var userRole = request.InviteType == InviteType.ContractedPJ 
    ? UserRole.FuncionarioPJ 
    : request.Role ?? UserRole.FuncionarioCLT;
```

**O que isso significa:**
- Se `InviteType = ContractedPJ` → Role será **SEMPRE** `FuncionarioPJ` (ignora o que você mandar)
- Se `InviteType = Internal` → Usa o `Role` que você enviou (Financeiro/Jurídico)

### ✅ Solução Recomendada:

**Tornar o campo `Role` opcional quando `InviteType = ContractedPJ`**

#### Modificação no DTO `InviteUserRequest`:

**Antes:**
```csharp
[Required]
public UserRole Role { get; set; }
```

**Depois:**
```csharp
public UserRole? Role { get; set; }
```

#### Validação na lógica:

```csharp
// Em UserService.InviteUserAsync
if (request.InviteType == InviteType.Internal && request.Role == null)
{
    return Result.Failure<InviteResponse>("Role é obrigatório para convites internos");
}

var userRole = request.InviteType == InviteType.ContractedPJ 
    ? UserRole.FuncionarioPJ 
    : request.Role!.Value;
```

### 📊 Exemplos de Uso:

**Convite Interno (Financeiro):**
```json
{
  "name": "João Financeiro",
  "email": "joao@empresa.com",
  "role": "Financeiro",
  "inviteType": "Internal"
}
```

**Convite PJ (Role é ignorado):**
```json
{
  "name": "Maria PJ",
  "email": "maria@pj.com",
  "inviteType": "ContractedPJ",
  "companyName": "Maria Consultoria ME",
  "cnpj": "12345678000190",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}
```

---

## ❓ Dúvida 2: Qual a diferença entre BusinessModel e InviteType?

### 📊 Comparação:

| Campo | Propósito | Onde é Usado | Valores Possíveis |
|-------|-----------|--------------|-------------------|
| **InviteType** | Define o **tipo de convite** | Tabela `UserInvites` | `Internal`, `ContractedPJ`, `ExternalUser` |
| **BusinessModel** | Define o **modelo de negócio da empresa** | Tabela `Companies` | `Standard`, `MainCompany`, `ContractedPJ`, `Freelancer` |

### 🔍 InviteType (Tipo de Convite)

**Definição:**
```csharp
public enum InviteType
{
    Internal = 0,        // Funcionário interno (Financeiro/Jurídico)
    ContractedPJ = 1,    // PJ que terá empresa criada automaticamente
    ExternalUser = 2     // Usuário externo com acesso específico
}
```

**Propósito:** Determinar **como processar o convite**

**Comportamento:**
- `Internal`: Usuário será vinculado à **mesma empresa** do convidador (não cria nova empresa)
- `ContractedPJ`: Cria **nova empresa PJ** e **relacionamento** entre empresas
- `ExternalUser`: Para casos futuros (parceiros, auditores externos, etc.)

**Exemplo de Uso:**
```csharp
if (invite.InviteType == InviteType.ContractedPJ)
{
    // Criar empresa PJ
    var pjCompany = new Company(...)
    
    // Criar relacionamento entre empresas
    var relationship = new CompanyRelationship(
        clientCompanyId: inviterCompany.Id,
        providerCompanyId: pjCompany.Id,
        type: RelationshipType.ContractedPJ
    )
}
```

---

### 🏢 BusinessModel (Modelo de Negócio)

**Definição:**
```csharp
public enum BusinessModel
{
    Standard = 1,       // Empresa comum
    MainCompany = 2,    // Empresa principal que contrata PJs
    ContractedPJ = 3,   // PJ contratado por outra empresa
    Freelancer = 4      // Profissional autônomo individual
}
```

**Propósito:** Classificar **o tipo de operação da empresa**

**Comportamento:**
- `Standard`: Empresa normal sem funcionalidades especiais
- `MainCompany`: Empresa que **CONTRATA** PJs (pode ter múltiplos PJs contratados)
- `ContractedPJ`: Empresa PJ que **FOI CONTRATADA** por outra
- `Freelancer`: Profissional individual (pode ser usado para diferenciar de empresas)

**Exemplo de Uso:**
```csharp
// Ao registrar empresa principal
var mainCompany = new Company(
    name: "Empresa Principal Ltda",
    cnpj: "11222333000144",
    type: CompanyType.Client,
    businessModel: BusinessModel.MainCompany  // ← Indica que contrata PJs
);

// Ao criar empresa PJ via convite
var pjCompany = new Company(
    name: "João Silva ME",
    cnpj: "99888777000166",
    type: CompanyType.Provider,
    businessModel: BusinessModel.ContractedPJ  // ← Indica que é PJ contratado
);
```

---

### 🔗 Relação Entre InviteType e BusinessModel

Quando você convida um PJ:

```json
{
  "inviteType": "ContractedPJ",  ← Define COMO processar o convite
  "businessModel": "ContractedPJ" ← Define O QUE É a empresa criada
}
```

**Fluxo:**
1. `InviteType.ContractedPJ` → Sistema sabe que precisa **criar empresa + relacionamento**
2. `BusinessModel.ContractedPJ` → Empresa criada será marcada como **"PJ Contratado"**

---

### 📊 Matriz de Combinações Válidas:

| InviteType | Cria Empresa? | BusinessModel Usado | CompanyType Usado | Relacionamento Criado? |
|------------|---------------|---------------------|-------------------|------------------------|
| `Internal` | ❌ Não | N/A (usa empresa do convidador) | N/A | ❌ Não |
| `ContractedPJ` | ✅ Sim | `ContractedPJ` | `Provider` | ✅ Sim |
| `ExternalUser` | Depende | `Standard` ou `Freelancer` | `Both` | ⚠️ Opcional |

---

### 🎯 Resumo Simples:

| Campo | Responde | Exemplo |
|-------|----------|---------|
| **InviteType** | "Como processar este convite?" | "É um PJ? Então criar empresa!" |
| **BusinessModel** | "Que tipo de empresa é essa?" | "É uma empresa PJ contratada" |

**Analogia:**
- **InviteType** = "Receita de bolo" (como fazer)
- **BusinessModel** = "Tipo de bolo" (o que você está fazendo)

---

## 📋 Recomendações de Melhoria:

### 1. Simplificar o Convite PJ

**Proposta:** Tornar `Role` e `BusinessModel` opcionais para PJ:

```json
{
  "name": "Maria PJ",
  "email": "maria@pj.com",
  "inviteType": "ContractedPJ",
  "companyName": "Maria Consultoria ME",
  "cnpj": "12345678000190"
}
```

Sistema define automaticamente:
- `role = FuncionarioPJ`
- `businessModel = ContractedPJ`
- `companyType = Provider`

### 2. Validação Condicional

```csharp
// Se InviteType = Internal → Role é obrigatório
// Se InviteType = ContractedPJ → companyName, cnpj são obrigatórios
```

---

## ✅ Conclusão:

- **Role ao convidar**: Necessário para `Internal`, mas redundante para `ContractedPJ` (pode ser removido)
- **BusinessModel vs InviteType**: 
  - `InviteType` = **Como processar** o convite
  - `BusinessModel` = **O que é** a empresa criada
  - São complementares, não duplicados

**Quer que eu implemente a simplificação?** Posso tornar `Role` opcional e validar condicionalmente.
