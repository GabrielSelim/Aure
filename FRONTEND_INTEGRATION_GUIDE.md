# 📘 Guia Completo de Integração Frontend - Sistema Aure

> **Data de Atualização**: 28 de Outubro de 2025  
> **Versão da API**: 1.0  
> **URL Base (Desenvolvimento)**: `http://localhost:5203/api`

---

## 📑 Índice

1. [Visão Geral do Sistema](#visão-geral-do-sistema)
2. [Autenticação e Autorização](#autenticação-e-autorização)
3. [Endpoints da API](#endpoints-da-api)
4. [Tipos e Enums](#tipos-e-enums)
5. [Fluxos de Negócio](#fluxos-de-negócio)
6. [Estrutura de Dados (DTOs)](#estrutura-de-dados-dtos)
7. [Tratamento de Erros](#tratamento-de-erros)
8. [Validações e Regras](#validações-e-regras)
9. [Sistema de Notificações](#sistema-de-notificações)
10. [Exemplos de Implementação](#exemplos-de-implementação)

---

## 🎯 Visão Geral do Sistema

### Arquitetura
O Sistema Aure é uma plataforma fintech para gestão de:
- **Empresas** (Clientes e Fornecedores)
- **Contratos** (CLT e PJ)
- **Pagamentos** (PIX, TED, Boleto, Cartão)
- **Notas Fiscais** (Emissão e gestão)
- **Relatórios Financeiros e Fiscais**

### Hierarquia de Usuários
```
Empresa Pai (Principal)
├── Dono da Empresa Pai (Role: 1) - Acesso Total
├── Financeiro (Role: 2) - Gestão Operacional
├── Jurídico (Role: 3) - Contratos e Documentação
└── Funcionários
    ├── CLT (Role: 4) - Funcionários com carteira
    └── PJ (Role: 5) - Prestadores de serviço
```

---

## 🔐 Autenticação e Autorização

### 1. Login
**Endpoint**: `POST /api/Auth/entrar`

**Request Body**:
```typescript
interface LoginRequest {
  email: string;        // Obrigatório, formato email
  password: string;     // Obrigatório, mínimo 6 caracteres
}
```

**Response**:
```typescript
interface LoginResponse {
  tokenAcesso: string;           // JWT Token
  tokenRenovacao: string;        // Refresh Token
  expiraEm: string;              // ISO DateTime
  usuario: {
    id: string;                  // GUID
    name: string;
    email: string;
    role: string;                // "DonoEmpresaPai" | "Financeiro" | "Juridico" | "FuncionarioCLT" | "FuncionarioPJ"
    companyId: string;           // GUID
    createdAt: string;
    updatedAt: string;
  };
}
```

**Exemplo de Uso**:
```typescript
const login = async (email: string, password: string) => {
  const response = await fetch('http://localhost:5203/api/Auth/entrar', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  
  if (!response.ok) throw new Error('Login falhou');
  
  const data = await response.json();
  
  // Salvar token no localStorage ou contexto
  localStorage.setItem('accessToken', data.tokenAcesso);
  localStorage.setItem('refreshToken', data.tokenRenovacao);
  localStorage.setItem('user', JSON.stringify(data.usuario));
  
  return data;
};
```

### 2. Renovar Token
**Endpoint**: `POST /api/Auth/renovar-token`

**Request Body**:
```typescript
interface RefreshTokenRequest {
  refreshToken: string;
}
```

**Response**: Mesmo formato do Login

### 3. Logout
**Endpoint**: `POST /api/Auth/sair`

**Headers**: 
```
Authorization: Bearer {accessToken}
```

**Response**: `204 No Content`

### 4. Header de Autorização
Para todas as requisições protegidas, incluir:
```typescript
headers: {
  'Authorization': `Bearer ${accessToken}`,
  'Content-Type': 'application/json'
}
```

---

## 🌐 Endpoints da API

### 📋 Registro e Convites

#### 1. Registrar Empresa Pai (Primeiro Acesso)
**POST** `/api/Registration/admin-empresa`

```typescript
interface RegisterAdminRequest {
  // Dados do Usuário
  nome: string;                     // Nome completo
  email: string;                    // Email válido
  senha: string;                    // Mínimo 8 caracteres
  telefoneCelular?: string;         // Opcional
  telefoneFixo?: string;            // Opcional
  
  // Dados da Empresa
  razaoSocial: string;              // Nome da empresa
  cnpj: string;                     // CNPJ válido (14 dígitos)
  
  // Endereço
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
}
```

**Response**: `LoginResponse` (já retorna autenticado)

#### 2. Convidar Usuário
**POST** `/api/Registration/convidar-usuario`

**Roles Permitidas**: DonoEmpresaPai, Financeiro, Juridico

```typescript
interface InviteUserRequest {
  // Dados do Convidado
  email: string;
  nome: string;
  role: 2 | 3 | 5;                  // 2=Financeiro, 3=Juridico, 5=FuncionarioPJ
  inviteType: 0 | 1 | 2;            // 0=Internal, 1=ContractedPJ, 2=ExternalUser
  
  // Se role = FuncionarioPJ (5) e inviteType = ContractedPJ (1), obrigatório:
  razaoSocial?: string;             // Nome da empresa PJ
  cnpj?: string;                    // CNPJ da empresa PJ
  businessModel?: 3 | 4;            // 3=ContractedPJ, 4=Freelancer
  companyType?: 1 | 2 | 3;          // 1=Client, 2=Provider, 3=Both
}
```

**Response**:
```typescript
interface InviteResponse {
  mensagem: string;
  convite: {
    id: string;
    inviterName: string;
    inviteeEmail: string;
    inviteeName: string;
    role: string;                   // Nome da role
    companyId: string;
    invitedByUserId: string;
    token: string;                  // Token do convite
    expiresAt: string;              // ISO DateTime
    isAccepted: boolean;
    inviteType: string;
    businessModel: string | null;
    companyName: string | null;
    cnpj: string | null;
    companyType: string | null;
  };
}
```

#### 3. Aceitar Convite
**POST** `/api/Registration/aceitar-convite/{token}`

**Sem Autenticação** (link enviado por email)

```typescript
interface AcceptInviteRequest {
  senha: string;                    // Mínimo 8 caracteres
  telefoneCelular?: string;
  telefoneFixo?: string;
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
}
```

**Response**: `LoginResponse`

#### 4. Listar Convites Pendentes
**GET** `/api/Registration/convites`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Response**: `InviteResponse[]`

#### 5. Cancelar Convite
**DELETE** `/api/Registration/cancelar-convite/{inviteId}`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Response**: `204 No Content`

#### 6. Reenviar Convite
**POST** `/api/Registration/reenviar-convite/{inviteId}`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Response**: `InviteResponse`

---

### 👥 Usuários

#### 1. Listar Usuários
**GET** `/api/Users`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Query Parameters**:
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `role` (opcional): filtrar por role (1-5)

**Response**:
```typescript
interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
```

#### 2. Buscar Usuário por ID
**GET** `/api/Users/{id}`

**Response**:
```typescript
interface UserResponse {
  id: string;
  name: string;
  email: string;
  role: string;                     // Nome legível da role
  companyId: string;
  createdAt: string;
  updatedAt: string;
}
```

#### 3. Atualizar Perfil
**PUT** `/api/Users/perfil`

**Permite**: Todos os usuários autenticados

```typescript
interface UpdateProfileRequest {
  nome?: string;
  email?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
  senhaAtual?: string;              // Obrigatório se mudar senha
  novaSenha?: string;               // Mínimo 8 caracteres
}
```

**Response**: `UserResponse`

#### 4. Deletar Usuário (Soft Delete)
**DELETE** `/api/Users/{id}`

**Roles**: DonoEmpresaPai

**Response**: `204 No Content`

---

### 🏢 Contratos

#### 1. Listar Contratos
**GET** `/api/Contracts`

**Roles**: DonoEmpresaPai, Financeiro, Juridico, FuncionarioPJ (vê apenas seus contratos)

**Query Parameters**:
- `pageNumber`, `pageSize`
- `status` (opcional): 1=Draft, 2=Active, 3=Completed, 4=Cancelled
- `employeeId` (opcional): filtrar por funcionário

**Response**: `PaginatedResponse<ContractResponse>`

```typescript
interface ContractResponse {
  id: string;
  companyId: string;
  employeeId: string;
  employeeName: string;             // Nome do funcionário
  valorMensal: number;
  dataInicio: string;               // ISO Date
  dataFim: string | null;           // ISO Date
  status: string;                   // "Draft" | "Active" | "Completed" | "Cancelled"
  metodoAssinatura: string;         // "Digital" | "Electronic" | "Manual"
  dataAssinatura: string | null;
  termos: string;                   // Texto do contrato
  createdAt: string;
  updatedAt: string;
}
```

#### 2. Buscar Contrato por ID
**GET** `/api/Contracts/{id}`

**Response**: `ContractResponse`

#### 3. Criar Contrato
**POST** `/api/Contracts`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

```typescript
interface CreateContractRequest {
  employeeId: string;               // GUID do funcionário PJ
  valorMensal: number;              // Valor > 0
  dataInicio: string;               // ISO Date
  dataFim?: string;                 // ISO Date (opcional)
  metodoAssinatura: 1 | 2 | 3;     // 1=Digital, 2=Electronic, 3=Manual
  termos: string;                   // Texto do contrato
}
```

**Response**: `ContractResponse`

#### 4. Assinar Contrato (Funcionário PJ)
**POST** `/api/Contracts/{id}/assinar`

**Roles**: FuncionarioPJ (apenas próprios contratos)

**Response**: `ContractResponse` (status muda para Active)

---

### 💰 Pagamentos

#### 1. Listar Pagamentos
**GET** `/api/Payments`

**Roles**: 
- DonoEmpresaPai, Financeiro: veem todos
- FuncionarioPJ: vê apenas próprios pagamentos

**Query Parameters**:
- `pageNumber`, `pageSize`
- `status` (opcional): 1=Pending, 2=Completed, 3=Failed, 4=Cancelled
- `metodo` (opcional): 1=PIX, 2=TED, 3=CreditCard, 4=Boleto

**Response**: `PaginatedResponse<PaymentResponse>`

```typescript
interface PaymentResponse {
  id: string;
  contratoId: string;
  employeeName: string;             // Nome do funcionário
  valor: number;
  metodo: string;                   // "PIX" | "TED" | "CreditCard" | "Boleto"
  status: string;                   // "Pending" | "Completed" | "Failed" | "Cancelled"
  dataPagamento: string;            // ISO DateTime
  dataProcessamento: string | null;
  referencia: string | null;        // Número do PIX, TED, etc
  observacoes: string | null;
  createdAt: string;
}
```

#### 2. Processar Pagamento
**POST** `/api/Payments/processar`

**Roles**: DonoEmpresaPai (apenas dono pode autorizar pagamentos)

```typescript
interface ProcessPaymentRequest {
  contratoId: string;               // GUID do contrato
  valor: number;                    // Valor > 0
  metodo: 1 | 2 | 3 | 4;           // 1=PIX, 2=TED, 3=CreditCard, 4=Boleto
  dataPagamento: string;            // ISO DateTime
  referencia?: string;              // Número PIX, comprovante, etc
  observacoes?: string;
}
```

**Response**: `PaymentResponse`

**Importante**: Após processar pagamento, o sistema envia **automaticamente** email para o funcionário PJ notificando o recebimento.

---

### 📄 Notas Fiscais (NFe)

#### 1. Listar Notas Fiscais
**GET** `/api/Invoices`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Query Parameters**:
- `pageNumber`, `pageSize`
- `status` (opcional): 1=Draft, 2=Issued, 3=Sent, 4=Cancelled, 5=Error

**Response**: `PaginatedResponse<InvoiceResponse>`

#### 2. Emitir Nota Fiscal
**POST** `/api/Invoices/emitir`

**Roles**: DonoEmpresaPai, Financeiro

```typescript
interface IssueInvoiceRequest {
  contratoId: string;
  serie: number;                    // Série da NFe
  numero: number;                   // Número da NFe
  naturezaOperacao: string;
  valorTotal: number;
  itens: InvoiceItemRequest[];
}

interface InvoiceItemRequest {
  descricao: string;
  quantidade: number;
  valorUnitario: number;
  ncm: string;                      // Código NCM
  cfop: string;                     // Código CFOP
}
```

**Response**: `InvoiceResponse`

---

### 🔗 Relacionamentos entre Empresas

#### 1. Listar Relacionamentos
**GET** `/api/CompanyRelationships`

**Roles**: DonoEmpresaPai, Financeiro, Juridico

**Response**:
```typescript
interface CompanyRelationshipResponse {
  id: string;
  empresaPrincipalId: string;
  empresaPrincipalNome: string;
  empresaRelacionadaId: string;
  empresaRelacionadaNome: string;
  tipoRelacionamento: string;       // "ContractedPJ" | "Partnership" | "Supplier" | "Client"
  status: string;                   // "Active" | "Inactive" | "Terminated" | "Suspended"
  dataInicio: string;
  dataFim: string | null;
  createdAt: string;
}
```

---

## 🎨 Tipos e Enums

### Enums TypeScript para Frontend

```typescript
// ============================================
// ROLES DE USUÁRIO
// ============================================
export enum UserRole {
  DonoEmpresaPai = 1,     // ✅ Todos os privilégios
  Financeiro = 2,         // ✅ Gestão operacional (sem pagamentos)
  Juridico = 3,           // ✅ Contratos e docs (sem financeiro sensível)
  FuncionarioCLT = 4,     // 👤 Funcionário com carteira
  FuncionarioPJ = 5       // 👤 Prestador de serviço PJ
}

export const UserRoleLabels: Record<UserRole, string> = {
  [UserRole.DonoEmpresaPai]: 'Dono da Empresa',
  [UserRole.Financeiro]: 'Financeiro',
  [UserRole.Juridico]: 'Jurídico',
  [UserRole.FuncionarioCLT]: 'Funcionário CLT',
  [UserRole.FuncionarioPJ]: 'Funcionário PJ'
};

// ============================================
// TIPOS DE EMPRESA
// ============================================
export enum CompanyType {
  Client = 1,       // Cliente
  Provider = 2,     // Fornecedor/Prestador
  Both = 3          // Ambos
}

export const CompanyTypeLabels: Record<CompanyType, string> = {
  [CompanyType.Client]: 'Cliente',
  [CompanyType.Provider]: 'Fornecedor',
  [CompanyType.Both]: 'Cliente e Fornecedor'
};

// ============================================
// MODELOS DE NEGÓCIO
// ============================================
export enum BusinessModel {
  Standard = 1,       // Empresa comum
  MainCompany = 2,    // Empresa principal que contrata PJs
  ContractedPJ = 3,   // PJ contratado
  Freelancer = 4      // Freelancer individual
}

// ============================================
// TIPOS DE CONVITE
// ============================================
export enum InviteType {
  Internal = 0,       // Funcionário interno (Financeiro/Jurídico)
  ContractedPJ = 1,   // PJ que terá empresa criada
  ExternalUser = 2    // Usuário externo
}

// ============================================
// STATUS DE CONTRATO
// ============================================
export enum ContractStatus {
  Draft = 1,          // Rascunho
  Active = 2,         // Ativo
  Completed = 3,      // Concluído
  Cancelled = 4       // Cancelado
}

export const ContractStatusLabels: Record<ContractStatus, string> = {
  [ContractStatus.Draft]: 'Rascunho',
  [ContractStatus.Active]: 'Ativo',
  [ContractStatus.Completed]: 'Concluído',
  [ContractStatus.Cancelled]: 'Cancelado'
};

export const ContractStatusColors: Record<ContractStatus, string> = {
  [ContractStatus.Draft]: 'gray',
  [ContractStatus.Active]: 'green',
  [ContractStatus.Completed]: 'blue',
  [ContractStatus.Cancelled]: 'red'
};

// ============================================
// STATUS DE PAGAMENTO
// ============================================
export enum PaymentStatus {
  Pending = 1,        // Pendente
  Completed = 2,      // Completado
  Failed = 3,         // Falhou
  Cancelled = 4       // Cancelado
}

export const PaymentStatusLabels: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Pendente',
  [PaymentStatus.Completed]: 'Completado',
  [PaymentStatus.Failed]: 'Falhou',
  [PaymentStatus.Cancelled]: 'Cancelado'
};

export const PaymentStatusColors: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'orange',
  [PaymentStatus.Completed]: 'green',
  [PaymentStatus.Failed]: 'red',
  [PaymentStatus.Cancelled]: 'gray'
};

// ============================================
// MÉTODOS DE PAGAMENTO
// ============================================
export enum PaymentMethod {
  PIX = 1,
  TED = 2,
  CreditCard = 3,
  Boleto = 4
}

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.PIX]: 'PIX',
  [PaymentMethod.TED]: 'TED',
  [PaymentMethod.CreditCard]: 'Cartão de Crédito',
  [PaymentMethod.Boleto]: 'Boleto'
};

// ============================================
// MÉTODOS DE ASSINATURA
// ============================================
export enum SignatureMethod {
  Digital = 1,        // Certificado digital
  Electronic = 2,     // Assinatura eletrônica simples
  Manual = 3          // Assinatura física
}

export const SignatureMethodLabels: Record<SignatureMethod, string> = {
  [SignatureMethod.Digital]: 'Digital',
  [SignatureMethod.Electronic]: 'Eletrônica',
  [SignatureMethod.Manual]: 'Manual'
};

// ============================================
// STATUS DE NOTA FISCAL
// ============================================
export enum InvoiceStatus {
  Draft = 1,
  Issued = 2,
  Sent = 3,
  Cancelled = 4,
  Error = 5
}

export const InvoiceStatusLabels: Record<InvoiceStatus, string> = {
  [InvoiceStatus.Draft]: 'Rascunho',
  [InvoiceStatus.Issued]: 'Emitida',
  [InvoiceStatus.Sent]: 'Enviada',
  [InvoiceStatus.Cancelled]: 'Cancelada',
  [InvoiceStatus.Error]: 'Erro'
};

// ============================================
// TIPO DE RELACIONAMENTO
// ============================================
export enum RelationshipType {
  ContractedPJ = 1,
  Partnership = 2,
  Supplier = 3,
  Client = 4
}

export const RelationshipTypeLabels: Record<RelationshipType, string> = {
  [RelationshipType.ContractedPJ]: 'PJ Contratado',
  [RelationshipType.Partnership]: 'Parceria',
  [RelationshipType.Supplier]: 'Fornecedor',
  [RelationshipType.Client]: 'Cliente'
};

// ============================================
// HELPERS
// ============================================

// Verificar se usuário tem permissão para ação
export const hasPermission = (userRole: UserRole, requiredRoles: UserRole[]): boolean => {
  return requiredRoles.includes(userRole);
};

// Verificar se é gestor (pode gerenciar usuários)
export const isManager = (userRole: UserRole): boolean => {
  return [UserRole.DonoEmpresaPai, UserRole.Financeiro, UserRole.Juridico].includes(userRole);
};

// Verificar se pode autorizar pagamentos
export const canAuthorizePayments = (userRole: UserRole): boolean => {
  return userRole === UserRole.DonoEmpresaPai;
};

// Verificar se pode gerenciar contratos
export const canManageContracts = (userRole: UserRole): boolean => {
  return [UserRole.DonoEmpresaPai, UserRole.Financeiro, UserRole.Juridico].includes(userRole);
};

// Verificar se é funcionário PJ
export const isContractedPJ = (userRole: UserRole): boolean => {
  return userRole === UserRole.FuncionarioPJ;
};
```

---

## 🔄 Fluxos de Negócio

### Fluxo 1: Onboarding - Primeira Empresa

```
1. Usuário acessa página de registro
2. Preenche formulário com dados pessoais + empresa
3. POST /api/Registration/admin-empresa
4. Sistema cria:
   - Empresa Pai
   - Usuário como DonoEmpresaPai
   - Retorna tokens JWT
5. Redireciona para dashboard
```

### Fluxo 2: Convidar Funcionário PJ

```
DonoEmpresaPai/Financeiro/Juridico:

1. Acessa "Convidar Usuário"
2. Preenche formulário:
   - Email: email@pj.com
   - Nome: João Silva
   - Role: FuncionarioPJ (5)
   - InviteType: ContractedPJ (1)
   - Razão Social: João Silva ME
   - CNPJ: 12345678000190
   - BusinessModel: ContractedPJ (3)
   - CompanyType: Provider (2)
3. POST /api/Registration/convidar-usuario
4. Sistema:
   - Cria convite
   - Envia email com link
5. Funcionário PJ:
   - Clica no link do email
   - Cadastra senha
   - POST /api/Registration/aceitar-convite/{token}
6. Sistema:
   - Cria empresa PJ
   - Cria usuário PJ
   - Cria relacionamento empresa principal <-> PJ
   - Retorna tokens (já logado)
7. Redireciona para dashboard PJ
```

### Fluxo 3: Criar e Assinar Contrato

```
Gestor (DonoEmpresaPai/Financeiro/Juridico):

1. Acessa "Contratos" > "Novo Contrato"
2. Seleciona funcionário PJ
3. Preenche:
   - Valor Mensal: 10000
   - Data Início: 2025-11-01
   - Data Fim: 2026-10-31 (opcional)
   - Método Assinatura: Electronic (2)
   - Termos: [texto do contrato]
4. POST /api/Contracts
5. Sistema cria contrato com status Draft

Funcionário PJ:

6. Recebe email: "Novo contrato disponível para assinatura"
7. Acessa dashboard
8. Vê contrato pendente
9. Clica em "Assinar"
10. POST /api/Contracts/{id}/assinar
11. Sistema:
    - Muda status para Active
    - Registra data de assinatura
    - Envia email de confirmação para gestores
```

### Fluxo 4: Processar Pagamento

```
DonoEmpresaPai:

1. Acessa "Pagamentos" > "Novo Pagamento"
2. Seleciona contrato PJ
3. Preenche:
   - Valor: 10000
   - Método: PIX (1)
   - Data Pagamento: 2025-11-05
   - Referência: "chave-pix-12345"
4. POST /api/Payments/processar
5. Sistema:
   - Cria pagamento com status Completed
   - Envia email AUTOMATICAMENTE para funcionário PJ:
     "Pagamento Recebido - R$ 10.000,00"
6. Funcionário PJ vê notificação no email e no dashboard
```

---

## 📦 Estrutura de Dados Completa (DTOs)

### Autenticação

```typescript
// LOGIN
interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  tokenAcesso: string;
  tokenRenovacao: string;
  expiraEm: string;
  usuario: UserResponse;
}

// REFRESH TOKEN
interface RefreshTokenRequest {
  refreshToken: string;
}
```

### Registro

```typescript
// REGISTRO ADMIN EMPRESA
interface RegisterAdminRequest {
  nome: string;
  email: string;
  senha: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  razaoSocial: string;
  cnpj: string;
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
}

// CONVIDAR USUÁRIO
interface InviteUserRequest {
  email: string;
  nome: string;
  role: 2 | 3 | 5;
  inviteType: 0 | 1 | 2;
  razaoSocial?: string;
  cnpj?: string;
  businessModel?: 3 | 4;
  companyType?: 1 | 2 | 3;
}

interface InviteResponse {
  mensagem: string;
  convite: {
    id: string;
    inviterName: string;
    inviteeEmail: string;
    inviteeName: string;
    role: string;
    companyId: string;
    invitedByUserId: string;
    token: string;
    expiresAt: string;
    isAccepted: boolean;
    inviteType: string;
    businessModel: string | null;
    companyName: string | null;
    cnpj: string | null;
    companyType: string | null;
  };
}

// ACEITAR CONVITE
interface AcceptInviteRequest {
  senha: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
}
```

### Usuários

```typescript
interface UserResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  companyId: string;
  createdAt: string;
  updatedAt: string;
}

interface UpdateProfileRequest {
  nome?: string;
  email?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  rua?: string;
  cidade?: string;
  estado?: string;
  pais?: string;
  cep?: string;
  senhaAtual?: string;
  novaSenha?: string;
}
```

### Contratos

```typescript
interface ContractResponse {
  id: string;
  companyId: string;
  employeeId: string;
  employeeName: string;
  valorMensal: number;
  dataInicio: string;
  dataFim: string | null;
  status: string;
  metodoAssinatura: string;
  dataAssinatura: string | null;
  termos: string;
  createdAt: string;
  updatedAt: string;
}

interface CreateContractRequest {
  employeeId: string;
  valorMensal: number;
  dataInicio: string;
  dataFim?: string;
  metodoAssinatura: 1 | 2 | 3;
  termos: string;
}
```

### Pagamentos

```typescript
interface PaymentResponse {
  id: string;
  contratoId: string;
  employeeName: string;
  valor: number;
  metodo: string;
  status: string;
  dataPagamento: string;
  dataProcessamento: string | null;
  referencia: string | null;
  observacoes: string | null;
  createdAt: string;
}

interface ProcessPaymentRequest {
  contratoId: string;
  valor: number;
  metodo: 1 | 2 | 3 | 4;
  dataPagamento: string;
  referencia?: string;
  observacoes?: string;
}
```

### Paginação

```typescript
interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
}
```

---

## ⚠️ Tratamento de Erros

### Estrutura de Erro Padrão

```typescript
interface ApiError {
  status: number;
  title: string;
  detail: string;
  errors?: Record<string, string[]>;
}
```

### Códigos de Status HTTP

```typescript
// 2xx - Sucesso
200 OK              // GET, PUT bem-sucedidos
201 Created         // POST bem-sucedido (recurso criado)
204 No Content      // DELETE bem-sucedido

// 4xx - Erros do Cliente
400 Bad Request     // Validação falhou
401 Unauthorized    // Não autenticado (sem token ou token inválido)
403 Forbidden       // Não autorizado (sem permissão)
404 Not Found       // Recurso não encontrado
409 Conflict        // Conflito (ex: email já existe)

// 5xx - Erros do Servidor
500 Internal Server Error    // Erro no servidor
```

### Exemplo de Tratamento

```typescript
const handleApiError = (error: any): string => {
  if (error.status === 400) {
    // Erros de validação
    if (error.errors) {
      return Object.values(error.errors).flat().join(', ');
    }
    return error.detail || 'Dados inválidos';
  }
  
  if (error.status === 401) {
    // Redirecionar para login
    window.location.href = '/login';
    return 'Sessão expirada. Faça login novamente.';
  }
  
  if (error.status === 403) {
    return 'Você não tem permissão para realizar esta ação.';
  }
  
  if (error.status === 404) {
    return 'Recurso não encontrado.';
  }
  
  if (error.status === 409) {
    return error.detail || 'Já existe um registro com estes dados.';
  }
  
  // Erro genérico
  return 'Erro inesperado. Tente novamente.';
};
```

---

## ✅ Validações e Regras

### Email
- Formato válido
- Único no sistema

### Senha
- Mínimo: 8 caracteres
- Deve conter: letra maiúscula, minúscula, número, caractere especial

### CNPJ
- Exatamente 14 dígitos numéricos
- Validação de dígitos verificadores
- Único no sistema

### CPF
- Exatamente 11 dígitos numéricos
- Validação de dígitos verificadores

### Valores Monetários
- Sempre > 0
- Máximo 2 casas decimais
- Formato: usar `number` no TypeScript, converter para centavos se necessário

### Datas
- Formato ISO 8601: `YYYY-MM-DDTHH:mm:ss.sssZ`
- Data de início < Data de fim
- Datas futuras onde aplicável

### Telefone
- Formato: `(XX) XXXXX-XXXX` ou `(XX) XXXX-XXXX`
- Apenas números (remover formatação ao enviar)

### CEP
- Formato: `XXXXX-XXX`
- 8 dígitos numéricos

---

## 🔔 Sistema de Notificações

### Eventos que Geram Notificações Automáticas

#### 1. Pagamento Processado → Funcionário PJ
```
Quando: DonoEmpresaPai processa pagamento
Para: Funcionário PJ que recebeu
Email: "Pagamento Recebido - R$ {valor}"
Conteúdo:
  - Valor
  - Data do pagamento
  - Método (PIX/TED/etc)
  - Referência
  - Nome da empresa pagadora
```

#### 2. Novo Contrato Criado → Funcionário PJ
```
Quando: Gestor cria contrato PJ
Para: Funcionário PJ
Email: "Novo Contrato Disponível para Assinatura"
Ação: Link para assinar contrato
```

#### 3. Contrato Assinado → Gestores
```
Quando: Funcionário PJ assina contrato
Para: DonoEmpresaPai, Financeiro, Jurídico
Email: "Contrato Assinado - {nomeFuncionario}"
Detalhes:
  - Nome do funcionário
  - Valor mensal
  - Data de assinatura
```

#### 4. Convite Enviado → Convidado
```
Quando: Gestor convida novo usuário
Para: Email do convidado
Email: "Convite para Sistema Aure - {nomeEmpresa}"
Ação: Link para aceitar convite (expira em 7 dias)
```

#### 5. Novo Funcionário Cadastrado → Gestores
```
Quando: Convite é aceito
Para: DonoEmpresaPai, Financeiro, Jurídico
Notificação: "{nomeFuncionario} aceitou o convite"
```

#### 6. Alerta de Vencimento de Contrato
```
Quando: 30, 15 e 7 dias antes do vencimento
Para: DonoEmpresaPai, Jurídico, Funcionário PJ
Email: "Contrato Próximo ao Vencimento"
Detalhes:
  - Nome do funcionário
  - Data de vencimento
  - Dias restantes
```

### Implementação Frontend

```typescript
// Sistema de notificações in-app (opcional)
interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  createdAt: string;
  read: boolean;
  actionUrl?: string;
}

// Buscar notificações
const fetchNotifications = async (): Promise<Notification[]> => {
  const response = await fetch('http://localhost:5203/api/Notifications', {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.json();
};

// Marcar como lida
const markAsRead = async (id: string): Promise<void> => {
  await fetch(`http://localhost:5203/api/Notifications/${id}/ler`, {
    method: 'PUT',
    headers: { Authorization: `Bearer ${token}` }
  });
};
```

---

## 💻 Exemplos de Implementação

### Serviço de API (React/TypeScript)

```typescript
// api/client.ts
const API_BASE_URL = 'http://localhost:5203/api';

class ApiClient {
  private getHeaders(includeAuth = true): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json'
    };
    
    if (includeAuth) {
      const token = localStorage.getItem('accessToken');
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }
    
    return headers;
  }
  
  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const error = await response.json();
      throw error;
    }
    
    if (response.status === 204) {
      return {} as T;
    }
    
    return response.json();
  }
  
  async get<T>(endpoint: string, params?: Record<string, any>): Promise<T> {
    const url = new URL(`${API_BASE_URL}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }
    
    const response = await fetch(url.toString(), {
      method: 'GET',
      headers: this.getHeaders()
    });
    
    return this.handleResponse<T>(response);
  }
  
  async post<T>(endpoint: string, data: any, includeAuth = true): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: this.getHeaders(includeAuth),
      body: JSON.stringify(data)
    });
    
    return this.handleResponse<T>(response);
  }
  
  async put<T>(endpoint: string, data: any): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: this.getHeaders(),
      body: JSON.stringify(data)
    });
    
    return this.handleResponse<T>(response);
  }
  
  async delete(endpoint: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'DELETE',
      headers: this.getHeaders()
    });
    
    await this.handleResponse<void>(response);
  }
}

export const apiClient = new ApiClient();
```

### Serviço de Autenticação

```typescript
// services/authService.ts
import { apiClient } from '../api/client';
import { LoginRequest, LoginResponse, RegisterAdminRequest } from '../types';

export class AuthService {
  async login(email: string, password: string): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      '/Auth/entrar',
      { email, password } as LoginRequest,
      false // Não incluir auth header no login
    );
    
    // Salvar tokens
    localStorage.setItem('accessToken', response.tokenAcesso);
    localStorage.setItem('refreshToken', response.tokenRenovacao);
    localStorage.setItem('user', JSON.stringify(response.usuario));
    
    return response;
  }
  
  async register(data: RegisterAdminRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      '/Registration/admin-empresa',
      data,
      false
    );
    
    // Salvar tokens
    localStorage.setItem('accessToken', response.tokenAcesso);
    localStorage.setItem('refreshToken', response.tokenRenovacao);
    localStorage.setItem('user', JSON.stringify(response.usuario));
    
    return response;
  }
  
  async logout(): Promise<void> {
    try {
      await apiClient.post('/Auth/sair', {});
    } finally {
      // Limpar tokens mesmo se a requisição falhar
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  }
  
  async refreshToken(): Promise<LoginResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }
    
    const response = await apiClient.post<LoginResponse>(
      '/Auth/renovar-token',
      { refreshToken },
      false
    );
    
    localStorage.setItem('accessToken', response.tokenAcesso);
    localStorage.setItem('refreshToken', response.tokenRenovacao);
    localStorage.setItem('user', JSON.stringify(response.usuario));
    
    return response;
  }
  
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }
  
  isAuthenticated(): boolean {
    return !!localStorage.getItem('accessToken');
  }
  
  getUserRole(): UserRole | null {
    const user = this.getCurrentUser();
    if (!user) return null;
    
    // Converter string da role para número
    const roleMap: Record<string, UserRole> = {
      'DonoEmpresaPai': UserRole.DonoEmpresaPai,
      'Financeiro': UserRole.Financeiro,
      'Juridico': UserRole.Juridico,
      'FuncionarioCLT': UserRole.FuncionarioCLT,
      'FuncionarioPJ': UserRole.FuncionarioPJ
    };
    
    return roleMap[user.role] || null;
  }
}

export const authService = new AuthService();
```

### Serviço de Contratos

```typescript
// services/contractService.ts
import { apiClient } from '../api/client';
import {
  ContractResponse,
  CreateContractRequest,
  PaginatedResponse,
  PaginationParams
} from '../types';

export class ContractService {
  async getContracts(params?: PaginationParams & { status?: number; employeeId?: string }) {
    return apiClient.get<PaginatedResponse<ContractResponse>>('/Contracts', params);
  }
  
  async getContract(id: string) {
    return apiClient.get<ContractResponse>(`/Contracts/${id}`);
  }
  
  async createContract(data: CreateContractRequest) {
    return apiClient.post<ContractResponse>('/Contracts', data);
  }
  
  async signContract(id: string) {
    return apiClient.post<ContractResponse>(`/Contracts/${id}/assinar`, {});
  }
}

export const contractService = new ContractService();
```

### Hook de Autenticação (React)

```typescript
// hooks/useAuth.tsx
import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authService } from '../services/authService';
import { UserRole } from '../types/enums';

interface AuthContextData {
  user: any | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  userRole: UserRole | null;
  hasPermission: (roles: UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextData>({} as AuthContextData);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    // Carregar usuário do localStorage
    const loadUser = () => {
      const currentUser = authService.getCurrentUser();
      setUser(currentUser);
      setLoading(false);
    };
    
    loadUser();
  }, []);
  
  const login = async (email: string, password: string) => {
    const response = await authService.login(email, password);
    setUser(response.usuario);
  };
  
  const logout = async () => {
    await authService.logout();
    setUser(null);
  };
  
  const hasPermission = (roles: UserRole[]): boolean => {
    const userRole = authService.getUserRole();
    if (!userRole) return false;
    return roles.includes(userRole);
  };
  
  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        logout,
        isAuthenticated: authService.isAuthenticated(),
        userRole: authService.getUserRole(),
        hasPermission
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
```

### Componente de Rota Protegida

```typescript
// components/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { UserRole } from '../types/enums';

interface ProtectedRouteProps {
  children: React.ReactNode;
  roles?: UserRole[];
}

export const ProtectedRoute = ({ children, roles }: ProtectedRouteProps) => {
  const { isAuthenticated, hasPermission, loading } = useAuth();
  
  if (loading) {
    return <div>Carregando...</div>;
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  if (roles && !hasPermission(roles)) {
    return <Navigate to="/acesso-negado" replace />;
  }
  
  return <>{children}</>;
};
```

### Exemplo de Página de Login

```typescript
// pages/Login.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    
    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err: any) {
      setError(err.detail || 'Erro ao fazer login. Verifique suas credenciais.');
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <div className="login-container">
      <h1>Login - Sistema Aure</h1>
      
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={loading}
          />
        </div>
        
        <div>
          <label htmlFor="password">Senha</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={6}
            disabled={loading}
          />
        </div>
        
        {error && <div className="error">{error}</div>}
        
        <button type="submit" disabled={loading}>
          {loading ? 'Entrando...' : 'Entrar'}
        </button>
      </form>
      
      <p>
        Primeira vez? <a href="/registro">Criar conta</a>
      </p>
    </div>
  );
};
```

### Exemplo de Listagem de Contratos

```typescript
// pages/Contracts.tsx
import { useState, useEffect } from 'react';
import { contractService } from '../services/contractService';
import { ContractResponse } from '../types';
import { ContractStatusLabels, ContractStatusColors } from '../types/enums';

export const ContractsPage = () => {
  const [contracts, setContracts] = useState<ContractResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  
  useEffect(() => {
    loadContracts();
  }, [page]);
  
  const loadContracts = async () => {
    try {
      setLoading(true);
      const response = await contractService.getContracts({
        pageNumber: page,
        pageSize: 10
      });
      
      setContracts(response.items);
      setTotalPages(response.totalPages);
    } catch (err: any) {
      setError('Erro ao carregar contratos');
    } finally {
      setLoading(false);
    }
  };
  
  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };
  
  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('pt-BR');
  };
  
  if (loading) return <div>Carregando...</div>;
  if (error) return <div className="error">{error}</div>;
  
  return (
    <div className="contracts-page">
      <h1>Contratos</h1>
      
      <table>
        <thead>
          <tr>
            <th>Funcionário</th>
            <th>Valor Mensal</th>
            <th>Data Início</th>
            <th>Data Fim</th>
            <th>Status</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {contracts.map(contract => (
            <tr key={contract.id}>
              <td>{contract.employeeName}</td>
              <td>{formatCurrency(contract.valorMensal)}</td>
              <td>{formatDate(contract.dataInicio)}</td>
              <td>{contract.dataFim ? formatDate(contract.dataFim) : 'Indeterminado'}</td>
              <td>
                <span
                  className={`status-badge status-${ContractStatusColors[contract.status]}`}
                >
                  {ContractStatusLabels[contract.status]}
                </span>
              </td>
              <td>
                <button onClick={() => handleViewContract(contract.id)}>
                  Ver
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      <div className="pagination">
        <button
          onClick={() => setPage(p => p - 1)}
          disabled={page === 1}
        >
          Anterior
        </button>
        <span>Página {page} de {totalPages}</span>
        <button
          onClick={() => setPage(p => p + 1)}
          disabled={page === totalPages}
        >
          Próxima
        </button>
      </div>
    </div>
  );
  
  function handleViewContract(id: string) {
    // Navegar para página de detalhes
    window.location.href = `/contratos/${id}`;
  }
};
```

---

## 📝 Checklist de Implementação

### Configuração Inicial
- [ ] Configurar variável de ambiente `REACT_APP_API_URL=http://localhost:5203/api`
- [ ] Instalar dependências: `axios` ou `fetch` (nativo)
- [ ] Configurar React Router
- [ ] Criar estrutura de pastas (services, hooks, types, components)

### Autenticação
- [ ] Implementar serviço de autenticação (`authService.ts`)
- [ ] Criar contexto de autenticação (`useAuth`)
- [ ] Implementar página de login
- [ ] Implementar página de registro (admin empresa)
- [ ] Criar componente de rota protegida
- [ ] Implementar interceptor para refresh token automático

### Enums e Tipos
- [ ] Criar arquivo com todos os enums TypeScript
- [ ] Criar labels e cores para cada enum
- [ ] Criar interfaces para todos os DTOs
- [ ] Criar helpers de permissão

### Módulos Principais
- [ ] Dashboard (visão geral conforme role)
- [ ] Gestão de Usuários (convidar, listar, perfil)
- [ ] Gestão de Contratos (criar, listar, assinar)
- [ ] Gestão de Pagamentos (processar, listar, histórico)
- [ ] Notificações (in-app opcional)

### Componentes Reutilizáveis
- [ ] Tabela paginada
- [ ] Formulários com validação
- [ ] Badges de status (contratos, pagamentos)
- [ ] Modal de confirmação
- [ ] Toasts/Alerts
- [ ] Loading states

### Testes
- [ ] Testar fluxo de registro
- [ ] Testar fluxo de convite PJ
- [ ] Testar criação e assinatura de contrato
- [ ] Testar processamento de pagamento
- [ ] Testar permissões por role

---

## 🎨 Sugestões de UI/UX

### Dashboard por Role

**DonoEmpresaPai**:
- Resumo financeiro (total pago este mês, próximos pagamentos)
- Gráfico de pagamentos por mês
- Contratos ativos vs vencendo
- Ações rápidas: Processar Pagamento, Novo Contrato, Convidar Usuário

**Financeiro**:
- Resumo de funcionários (total CLT, total PJ)
- Contratos pendentes de assinatura
- Relatórios recentes
- Ações rápidas: Novo Contrato, Ver Pagamentos

**Jurídico**:
- Contratos vencendo nos próximos 30 dias
- Contratos aguardando assinatura
- Histórico de contratos
- Ações rápidas: Novo Contrato, Ver Contratos

**Funcionário PJ**:
- Próximo pagamento (data prevista)
- Histórico de pagamentos (últimos 6 meses)
- Meu contrato (status, vigência)
- Contratos pendentes de assinatura (se houver)

### Cores e Status

```css
/* Contratos */
.status-draft { background: #718096; }      /* Cinza */
.status-active { background: #48BB78; }     /* Verde */
.status-completed { background: #4299E1; }  /* Azul */
.status-cancelled { background: #F56565; }  /* Vermelho */

/* Pagamentos */
.status-pending { background: #ED8936; }    /* Laranja */
.status-completed { background: #48BB78; }  /* Verde */
.status-failed { background: #F56565; }     /* Vermelho */
.status-cancelled { background: #718096; }  /* Cinza */
```

---

## 🔗 Links Úteis

- **Swagger UI**: http://localhost:5203 (quando API estiver rodando)
- **Adminer (DB)**: http://localhost:8080
- **Hangfire Dashboard**: http://localhost:5203/hangfire

---

## 📞 Suporte

Para dúvidas ou problemas:
1. Verificar logs da API: `docker logs aure-api`
2. Verificar Swagger para estrutura exata dos endpoints
3. Consultar este guia para referência de DTOs e enums

---

**Última Atualização**: 28 de Outubro de 2025
