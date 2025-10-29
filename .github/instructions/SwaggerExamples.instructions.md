---
applyTo: '**/*Controller.cs'
---

# Exemplos de Swagger para Todos os Endpoints - Sistema Aure

## 🎯 Objetivo

Garantir que **TODOS os endpoints** tenham documentação Swagger completa e clara, especialmente mostrando:
- **Valores aceitos para TODOS os enums** (com números e descrições)
- **Exemplos JSON completos** para requests
- **Todos os códigos HTTP possíveis** (200, 201, 400, 401, 403, 404)
- **Descrição de regras de negócio** importantes
- **Descrição de notificações automáticas** (quando aplicável)

---

## 📝 Template Padrão para Swagger

### Estrutura Completa

```csharp
[HttpPost("endpoint")]
[Authorize(Roles = "DonoEmpresaPai,Financeiro")]
[SwaggerOperation(
    Summary = "Título curto e claro",
    Description = @"Descrição detalhada do endpoint.

**Valores Aceitos para NomeEnum:**
- `1` = Valor1 (Descrição do que significa)
- `2` = Valor2 (Descrição do que significa)
- `3` = Valor3 (Descrição do que significa)

**Regras de Negócio:**
- Validação 1
- Validação 2
- Restrição de permissão

**Notificações Automáticas:**
- Email enviado para X quando Y acontece"
)]
[SwaggerResponse(200, "Descrição do sucesso", typeof(ResponseType))]
[SwaggerResponse(400, "Descrição do erro de validação")]
[SwaggerResponse(401, "Não autenticado (token ausente ou inválido)")]
[SwaggerResponse(403, "Sem permissão para esta operação")]
[ProducesResponseType(typeof(ResponseType), StatusCodes.Status200OK)]
public async Task<IActionResult> NomeEndpoint(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Cenário A**
```json
{
  ""campo1"": ""valor1"",
  ""enumField"": 1,
  ""campo2"": 123.45
}
```

**Exemplo 2: Cenário B**
```json
{
  ""campo1"": ""valor2"",
  ""enumField"": 2,
  ""campo2"": 678.90
}
```", Required = true)]
    RequestType request)
{
    // Implementação
}
```

---

## 🔢 Enums do Sistema - Valores Completos

### UserRole (Role)

```
**Valores Aceitos para Role (UserRole):**
- `1` = DonoEmpresaPai (Proprietário da empresa com todos os privilégios - único que autoriza pagamentos)
- `2` = Financeiro (Gestão operacional e visualização financeira - não autoriza pagamentos)
- `3` = Juridico (Contratos e documentação legal)
- `4` = FuncionarioCLT (Funcionário com carteira assinada - contrato automático)
- `5` = FuncionarioPJ (Prestador de serviço PJ - precisa assinar contrato)
```

### InviteType

```
**Valores Aceitos para InviteType:**
- `0` = Internal (Usuário interno - Financeiro ou Juridico - não cria empresa)
- `1` = ContractedPJ (Funcionário PJ - cria empresa PJ automaticamente)
- `2` = ExternalUser (Usuário externo - para futuras integrações)
```

### CompanyType

```
**Valores Aceitos para CompanyType:**
- `1` = Client (Empresa cliente - recebe serviços)
- `2` = Provider (Empresa fornecedora/prestadora - fornece serviços)
- `3` = Both (Atua como cliente E fornecedor)
```

### BusinessModel

```
**Valores Aceitos para BusinessModel:**
- `1` = Standard (Modelo de negócio padrão - empresa comum)
- `2` = MainCompany (Empresa principal que contrata PJs)
- `3` = ContractedPJ (PJ contratado por empresa principal)
- `4` = Freelancer (Freelancer individual)
```

### ContractStatus

```
**Valores Aceitos para ContractStatus:**
- `1` = Draft (Rascunho - em criação, ainda não enviado)
- `2` = Active (Ativo - assinado e vigente)
- `3` = PendingSignature (Pendente de assinatura - aguardando assinatura do funcionário)
- `4` = Expired (Expirado - data de término atingida)
- `5` = Terminated (Rescindido - encerrado antes do prazo)
- `6` = Cancelled (Cancelado - nunca chegou a ser ativado)
```

### SignatureMethod

```
**Valores Aceitos para SignatureMethod (Método de Assinatura):**
- `1` = Digital (Certificado digital ICP-Brasil - A1/A3)
- `2` = Electronic (Assinatura eletrônica simples - email/SMS - RECOMENDADO)
- `3` = Manual (Assinatura física - papel assinado e digitalizado)
```

### PaymentMethod

```
**Valores Aceitos para PaymentMethod:**
- `1` = PIX (Pagamento instantâneo - RECOMENDADO)
- `2` = TED (Transferência bancária tradicional - D+1)
- `3` = CreditCard (Cartão de crédito)
- `4` = Boleto (Boleto bancário - vencimento em dias)
```

### PaymentStatus

```
**Valores Aceitos para PaymentStatus:**
- `1` = Pending (Pendente - criado mas não processado)
- `2` = Processing (Processando - em andamento no banco)
- `3` = Completed (Concluído - confirmado e finalizado)
- `4` = Failed (Falhou - erro no processamento)
- `5` = Cancelled (Cancelado - cancelado antes de processar)
```

---

## ✅ Exemplos Completos por Endpoint

### 1. Convidar Usuário (Internal ou PJ)

```csharp
[HttpPost("convidar-usuario")]
[Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
[SwaggerOperation(
    Summary = "Convidar novo usuário (interno ou PJ)",
    Description = @"Permite convidar usuários internos (Financeiro/Juridico) ou funcionários PJ.

**Valores Aceitos para Role (UserRole):**
- `2` = Financeiro (Gestão operacional)
- `3` = Juridico (Contratos e documentação)
- `5` = FuncionarioPJ (Prestador de serviço PJ - obrigatório com inviteType = 1)

**Valores Aceitos para InviteType:**
- `0` = Internal (Usuário interno - apenas Financeiro ou Juridico)
- `1` = ContractedPJ (Funcionário PJ - cria empresa PJ automaticamente)

**Valores Aceitos para CompanyType (apenas para ContractedPJ):**
- `1` = Client (Cliente)
- `2` = Provider (Fornecedor/Prestador - RECOMENDADO para PJ)
- `3` = Both (Cliente e Fornecedor)

**Valores Aceitos para BusinessModel (apenas para ContractedPJ):**
- `1` = Standard (Empresa padrão)
- `2` = MainCompany (Empresa principal)
- `3` = ContractedPJ (PJ contratado - RECOMENDADO)
- `4` = Freelancer (Freelancer individual)

**Regras de Negócio:**
1. **Internal (inviteType: 0)**:
   - Role: DEVE ser `2` (Financeiro) ou `3` (Juridico)
   - Campos empresa: NÃO devem ser informados
   
2. **ContractedPJ (inviteType: 1)**:
   - Role: DEVE ser `5` (FuncionarioPJ)
   - Campos obrigatórios: razaoSocial, cnpj, companyType, businessModel
   - Sistema cria empresa PJ automaticamente

**Validações:**
- Email único no sistema
- CNPJ único e válido (consulta Receita Federal)
- Razão Social vs CNPJ (similaridade > 85%)

**Notificações Automáticas:**
- Email de convite enviado para o convidado
- Email de notificação para gestores (DonoEmpresaPai e Financeiro)"
)]
[SwaggerResponse(200, "Convite enviado com sucesso. Email enviado para o convidado.", typeof(InviteResponse))]
[SwaggerResponse(400, "Dados inválidos: email/CNPJ duplicado, CNPJ inativo, ou razão social divergente")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[SwaggerResponse(403, "Sem permissão para convidar este tipo de usuário")]
[ProducesResponseType(typeof(InviteResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> ConvidarUsuario(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Convidar Funcionário PJ**
```json
{
  ""email"": ""joao.silva@consultor.com"",
  ""nome"": ""João Silva"",
  ""role"": 5,
  ""inviteType"": 1,
  ""razaoSocial"": ""João Silva Consultoria ME"",
  ""cnpj"": ""12345678000190"",
  ""businessModel"": 3,
  ""companyType"": 2
}
```

**Exemplo 2: Convidar Financeiro (Interno)**
```json
{
  ""email"": ""maria.santos@empresa.com"",
  ""nome"": ""Maria Santos"",
  ""role"": 2,
  ""inviteType"": 0
}
```

**Exemplo 3: Convidar Jurídico (Interno)**
```json
{
  ""email"": ""pedro.advogado@empresa.com"",
  ""nome"": ""Pedro Costa"",
  ""role"": 3,
  ""inviteType"": 0
}
```", Required = true)]
    InviteUserRequest request)
{
    // Implementação
}
```

### 2. Criar Contrato PJ

```csharp
[HttpPost]
[Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
[SwaggerOperation(
    Summary = "Criar novo contrato PJ",
    Description = @"Cria um contrato para funcionário PJ.

**Valores Aceitos para metodoAssinatura (SignatureMethod):**
- `1` = Digital (Certificado digital ICP-Brasil A1/A3)
- `2` = Electronic (Assinatura eletrônica simples - RECOMENDADO)
- `3` = Manual (Assinatura física em papel)

**Regras de Negócio:**
- Employee DEVE ser FuncionarioPJ (role 5)
- Valor mensal deve ser > 0
- Data início não pode ser no passado
- Data fim deve ser posterior à data início (se informada)
- Contrato criado com status Draft (1)
- Após assinatura, status muda para Active (2)

**Validações:**
- employeeId deve existir e ser FuncionarioPJ
- Valor mensal obrigatório e positivo
- Datas válidas e consistentes

**Notificações Automáticas:**
- Email enviado ao funcionário PJ para assinar
- Email de notificação aos gestores (DonoEmpresaPai, Financeiro, Juridico)"
)]
[SwaggerResponse(201, "Contrato criado com sucesso. Email enviado para assinatura.", typeof(ContractResponse))]
[SwaggerResponse(400, "Dados inválidos: funcionário não é PJ, valor <= 0, ou datas inconsistentes")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[SwaggerResponse(403, "Sem permissão para criar contratos")]
[SwaggerResponse(404, "Funcionário não encontrado")]
[ProducesResponseType(typeof(ContractResponse), StatusCodes.Status201Created)]
public async Task<IActionResult> CriarContrato(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Contrato com Data Fim Definida**
```json
{
  ""employeeId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""valorMensal"": 8000.00,
  ""dataInicio"": ""2025-11-01T00:00:00Z"",
  ""dataFim"": ""2026-10-31T23:59:59Z"",
  ""metodoAssinatura"": 2,
  ""termos"": ""Contrato de prestação de serviços de desenvolvimento de software...\n\nCLÁUSULA 1: DO OBJETO\nO contratado prestará serviços de desenvolvimento full-stack...""
}
```

**Exemplo 2: Contrato Indeterminado (sem data fim)**
```json
{
  ""employeeId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""valorMensal"": 12000.00,
  ""dataInicio"": ""2025-11-01T00:00:00Z"",
  ""metodoAssinatura"": 2,
  ""termos"": ""Contrato de prestação de serviços de consultoria especializada...""
}
```", Required = true)]
    CreateContractRequest request)
{
    // Implementação
}
```

### 3. Processar Pagamento (SOMENTE DonoEmpresaPai)

```csharp
[HttpPost("processar")]
[Authorize(Roles = "DonoEmpresaPai")]
[SwaggerOperation(
    Summary = "Processar pagamento (SOMENTE Dono da Empresa Pai)",
    Description = @"Processa pagamento para funcionário PJ ou CLT.

**Valores Aceitos para metodo (PaymentMethod):**
- `1` = PIX (Instantâneo - RECOMENDADO)
- `2` = TED (Transferência bancária D+1)
- `3` = CreditCard (Cartão de crédito)
- `4` = Boleto (Boleto bancário)

**⚠️ IMPORTANTE - PERMISSÃO EXCLUSIVA:**
- **APENAS DonoEmpresaPai** pode autorizar e processar pagamentos
- Financeiro/Juridico NÃO têm permissão
- Esta é uma regra crítica de segurança financeira

**Regras de Negócio:**
- Contrato deve estar ativo (status 2)
- Valor deve ser > 0 e <= valor contratual mensal
- Não pode haver pagamentos duplicados para o mesmo período
- DataPagamento não pode ser no futuro

**Validações:**
- contratoId deve existir e estar ativo
- Valor dentro do limite contratual
- Método de pagamento válido
- Referência obrigatória para PIX e TED

**Notificações Automáticas:**
- ✅ **Email enviado AUTOMATICAMENTE ao funcionário PJ** com:
  - Valor recebido
  - Data do pagamento
  - Método utilizado
  - Referência (chave PIX, número TED, etc.)
  - Nome da empresa pagadora
- Email de confirmação ao DonoEmpresaPai
- Notificação interna para Financeiro (sem autorizar, apenas informativa)"
)]
[SwaggerResponse(200, "Pagamento processado com sucesso. Notificação enviada ao funcionário.", typeof(PaymentResponse))]
[SwaggerResponse(400, "Dados inválidos: contrato inativo, valor excede limite, ou pagamento duplicado")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[SwaggerResponse(403, "APENAS Dono da Empresa Pai pode autorizar pagamentos")]
[SwaggerResponse(404, "Contrato não encontrado")]
[ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> ProcessarPagamento(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Pagamento via PIX**
```json
{
  ""contratoId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""valor"": 8000.00,
  ""metodo"": 1,
  ""dataPagamento"": ""2025-11-05T10:30:00Z"",
  ""referencia"": ""chave-pix-123456789"",
  ""observacoes"": ""Pagamento referente ao mês de outubro/2025""
}
```

**Exemplo 2: Pagamento via TED**
```json
{
  ""contratoId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""valor"": 8000.00,
  ""metodo"": 2,
  ""dataPagamento"": ""2025-11-05T10:30:00Z"",
  ""referencia"": ""TED-20251105-001"",
  ""observacoes"": ""Banco do Brasil - Agência 1234-5 - Conta 12345-6""
}
```

**Exemplo 3: Pagamento Parcial (50%)**
```json
{
  ""contratoId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
  ""valor"": 4000.00,
  ""metodo"": 1,
  ""dataPagamento"": ""2025-11-05T10:30:00Z"",
  ""referencia"": ""chave-pix-987654321"",
  ""observacoes"": ""Pagamento parcial (50%) - Primeira quinzena""
}
```", Required = true)]
    ProcessPaymentRequest request)
{
    // Implementação
}
```

### 4. Atualizar Perfil Completo

```csharp
[HttpPut("perfil-completo")]
[Authorize]
[SwaggerOperation(
    Summary = "Atualizar perfil completo do usuário autenticado",
    Description = @"Permite usuário atualizar seu próprio perfil com todos os campos disponíveis.

**Campos Editáveis por TODOS os usuários:**
- Nome, email, telefones (celular e fixo)
- Data de nascimento
- Endereço completo (8 campos)
- Senha (requer senhaAtual para validação)

**Campos Específicos por Role:**

**Cargo (apenas para FuncionarioCLT e FuncionarioPJ):**
- Dropdown pré-definido:
  - ""Desenvolvedor Full Stack""
  - ""Desenvolvedor Backend""
  - ""Desenvolvedor Frontend""
  - ""Designer UI/UX""
  - ""Analista de Dados""
  - ""Gerente de Projetos""
  - ""DevOps""
  - ""QA/Tester""
  - ""Analista Financeiro""
  - ""Contador""
  - ""Advogado""
  - ""Recursos Humanos""
  - ""Marketing""
- Ou campo livre (opção ""Outro"")

**CPF/RG:**
- Todos podem editar
- Serão **criptografados automaticamente** no backend (AES-256)
- CPF **único** no sistema (não pode duplicar)
- Visualização:
  - DonoEmpresaPai: Vê CPF completo de todos
  - Outros: Veem apenas próprio CPF completo, demais mascarados (**\*.\*\*.123-45)

**Endereço Especial para DonoEmpresaPai:**
- Endereço do Dono = Endereço da Empresa Pai
- Ao atualizar perfil do Dono, **atualiza também endereço da Empresa Pai**
- Sincronização bidirecional

**Validações:**
- Email único no sistema
- CPF único no sistema (formato: 11 dígitos numéricos)
- Data nascimento: Idade entre 16 e 100 anos
- Estado: Sigla de 2 letras (SP, RJ, etc.)
- CEP: Formato válido (8 dígitos com ou sem hífen)
- Senha: Requer senhaAtual correta para alteração"
)]
[SwaggerResponse(200, "Perfil atualizado com sucesso", typeof(UserProfileResponse))]
[SwaggerResponse(400, "Dados inválidos: email/CPF duplicado, senha atual incorreta, ou data nascimento inválida")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> AtualizarPerfilCompleto(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Perfil Completo de Funcionário PJ**
```json
{
  ""nome"": ""João Silva"",
  ""email"": ""joao.silva@consultor.com"",
  ""dataNascimento"": ""1990-05-15"",
  ""cpf"": ""12345678901"",
  ""rg"": ""123456789"",
  ""cargo"": ""Desenvolvedor Full Stack"",
  ""telefoneCelular"": ""11987654321"",
  ""telefoneFixo"": ""1133334444"",
  ""enderecoRua"": ""Rua das Flores"",
  ""enderecoNumero"": ""123"",
  ""enderecoComplemento"": ""Apto 45"",
  ""enderecoBairro"": ""Centro"",
  ""enderecoCidade"": ""São Paulo"",
  ""enderecoEstado"": ""SP"",
  ""enderecoPais"": ""Brasil"",
  ""enderecoCep"": ""01234-567""
}
```

**Exemplo 2: Apenas Alterar Telefone**
```json
{
  ""telefoneCelular"": ""11999998888""
}
```

**Exemplo 3: Alterar Senha**
```json
{
  ""senhaAtual"": ""SenhaAtual123!"",
  ""novaSenha"": ""NovaSenhaSegura456!""
}
```", Required = true)]
    UpdateFullProfileRequest request)
{
    // Implementação
}
```

### 5. Atualizar Preferências de Notificação

```csharp
[HttpPut("notificacoes/preferencias")]
[Authorize]
[SwaggerOperation(
    Summary = "Atualizar preferências de notificação por email",
    Description = @"Define quais tipos de email o usuário deseja receber.

**Preferências Disponíveis por Role:**

**DonoEmpresaPai (role 1):**
- receberEmailNovoContrato: Novo contrato criado
- receberEmailContratoAssinado: Funcionário assinou contrato
- receberEmailContratoVencendo: Contrato próximo ao vencimento (30/15/7 dias)
- receberEmailPagamentoProcessado: Pagamento processado por você
- receberEmailAlertasFinanceiros: Alertas críticos (pagamentos altos, múltiplos pagamentos)
- receberEmailNovoFuncionario: Novo funcionário convidado
- receberEmailAtualizacoesSistema: Atualizações e manutenções do sistema

**Financeiro (role 2):**
- receberEmailNovoContrato: Novo contrato criado
- receberEmailContratoAssinado: Funcionário assinou contrato
- receberEmailContratoVencendo: Contrato próximo ao vencimento
- receberEmailPagamentoProcessado: Notificação interna (não autoriza)
- receberEmailNovoFuncionario: Novo funcionário convidado
- receberEmailAtualizacoesSistema: Atualizações e manutenções

**Juridico (role 3):**
- receberEmailNovoContrato: Novo contrato criado
- receberEmailContratoAssinado: Funcionário assinou contrato
- receberEmailContratoVencendo: Contrato próximo ao vencimento
- receberEmailNovoFuncionario: Novo funcionário convidado
- receberEmailAtualizacoesSistema: Atualizações e manutenções

**FuncionarioPJ (role 5):**
- receberEmailContratoVencendo: Seu contrato próximo ao vencimento
- receberEmailPagamentoRecebido: Você recebeu um pagamento
- receberEmailAtualizacoesSistema: Atualizações e manutenções

**FuncionarioCLT (role 4):**
- receberEmailAtualizacoesSistema: Atualizações e manutenções

**Padrão:**
- Todas as preferências iniciam como `true` (receber todos os emails)
- Usuário pode desabilitar individualmente"
)]
[SwaggerResponse(200, "Preferências atualizadas com sucesso", typeof(NotificationPreferencesDTO))]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[ProducesResponseType(typeof(NotificationPreferencesDTO), StatusCodes.Status200OK)]
public async Task<IActionResult> AtualizarPreferenciasNotificacao(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: DonoEmpresaPai desabilita alertas financeiros**
```json
{
  ""receberEmailNovoContrato"": true,
  ""receberEmailContratoAssinado"": true,
  ""receberEmailContratoVencendo"": true,
  ""receberEmailPagamentoProcessado"": true,
  ""receberEmailAlertasFinanceiros"": false,
  ""receberEmailNovoFuncionario"": true,
  ""receberEmailAtualizacoesSistema"": true
}
```

**Exemplo 2: FuncionarioPJ (apenas contratos e pagamentos)**
```json
{
  ""receberEmailContratoVencendo"": true,
  ""receberEmailPagamentoRecebido"": true,
  ""receberEmailAtualizacoesSistema"": false
}
```

**Exemplo 3: Desabilitar TODOS os emails**
```json
{
  ""receberEmailNovoContrato"": false,
  ""receberEmailContratoAssinado"": false,
  ""receberEmailContratoVencendo"": false,
  ""receberEmailPagamentoProcessado"": false,
  ""receberEmailPagamentoRecebido"": false,
  ""receberEmailAlertasFinanceiros"": false,
  ""receberEmailNovoFuncionario"": false,
  ""receberEmailAtualizacoesSistema"": false
}
```", Required = true)]
    NotificationPreferencesDTO request)
{
    // Implementação
}
```

### 6. Atualizar Empresa PJ (com Validação CNPJ)

```csharp
[HttpPut("empresa-pj")]
[Authorize(Roles = "FuncionarioPJ")]
[SwaggerOperation(
    Summary = "Atualizar empresa PJ (SOMENTE FuncionarioPJ)",
    Description = @"Permite funcionário PJ atualizar dados da sua empresa.

**Valores Aceitos para companyType:**
- `1` = Client (Cliente - empresa contrata serviços)
- `2` = Provider (Fornecedor/Prestador - RECOMENDADO para PJ)
- `3` = Both (Atua como cliente E fornecedor)

**⚠️ VALIDAÇÃO AUTOMÁTICA DE CNPJ - FLUXO COMPLETO:**

Ao informar ou alterar CNPJ, o sistema executa:

1. **Validação de Formato**: 14 dígitos numéricos
2. **Validação de Dígitos Verificadores**: Algoritmo oficial da Receita Federal
3. **Verificação de Unicidade**: CNPJ único no sistema
4. **Consulta na Receita Federal**:
   - API Principal: Brasil API (https://brasilapi.com.br)
   - API Fallback: ReceitaWS (https://receitaws.com.br)
5. **Verificação de Status**: Empresa deve estar ATIVA
6. **Comparação de Razão Social**:
   - Sistema compara Razão Social informada vs Razão Social oficial
   - Normaliza strings (remove acentos, maiúsculas, pontuação)
   - Calcula similaridade (algoritmo de Levenshtein)

**Resultado da Comparação:**

✅ **Similaridade > 85%**: Aceita automaticamente
- Exemplo: ""João Silva Consultoria ME"" vs ""JOAO SILVA CONSULTORIA ME"" = 95% similar

⚠️ **Similaridade < 85%**: Retorna divergência e requer confirmação
- Exemplo: ""João Silva Consultoria"" vs ""JOAO SILVA CONSULTORIA E DESENVOLVIMENTO LTDA"" = 60% similar

**Fluxo de Divergência (Frontend):**
1. Backend retorna `divergenciaRazaoSocial: true` + `razaoSocialReceita`
2. Frontend mostra MODAL com:
   ```
   ⚠️ Divergência Detectada
   
   Razão Social Informada: João Silva Consultoria
   Razão Social na Receita Federal: JOAO SILVA CONSULTORIA E DESENVOLVIMENTO LTDA
   
   [Botão: Usar Razão Social Oficial] [Botão: Confirmar Minha Versão]
   ```
3. Se usuário escolher ""Confirmar Minha Versão"":
   - Reenviar request com `confirmarDivergenciaRazaoSocial: true`
   - Sistema grava divergência em AuditLog para análise posterior

**Regras de Negócio:**
- Apenas FuncionarioPJ pode atualizar própria empresa PJ
- CNPJ deve estar ATIVO na Receita Federal
- Endereço completo é obrigatório
- CEP deve ser válido (8 dígitos)
- Estado deve ser sigla de 2 letras

**Notificações Automáticas:**
- Email de confirmação para o FuncionarioPJ
- Notificação para DonoEmpresaPai/Financeiro sobre alteração (apenas se CNPJ mudou)"
)]
[SwaggerResponse(200, "Empresa PJ atualizada com sucesso", typeof(UpdateCompanyPJResponse))]
[SwaggerResponse(400, "CNPJ inválido, duplicado, inativo, ou divergência de razão social não confirmada")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[SwaggerResponse(403, "Apenas FuncionarioPJ pode atualizar empresa PJ")]
[ProducesResponseType(typeof(UpdateCompanyPJResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> AtualizarEmpresaPJ(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo 1: Atualização Normal (Razão Social OK)**
```json
{
  ""razaoSocial"": ""João Silva Consultoria ME"",
  ""cnpj"": ""12345678000190"",
  ""enderecoRua"": ""Av. Paulista"",
  ""enderecoNumero"": ""1000"",
  ""enderecoComplemento"": ""Sala 500"",
  ""enderecoBairro"": ""Bela Vista"",
  ""enderecoCidade"": ""São Paulo"",
  ""enderecoEstado"": ""SP"",
  ""enderecoPais"": ""Brasil"",
  ""enderecoCep"": ""01310-100"",
  ""companyType"": 2
}
```

**Exemplo 2: Confirmando Divergência de Razão Social**
```json
{
  ""razaoSocial"": ""Joao Silva Consultoria"",
  ""cnpj"": ""12345678000190"",
  ""confirmarDivergenciaRazaoSocial"": true,
  ""enderecoRua"": ""Av. Paulista"",
  ""enderecoNumero"": ""1000"",
  ""enderecoCidade"": ""São Paulo"",
  ""enderecoEstado"": ""SP"",
  ""enderecoCep"": ""01310-100"",
  ""companyType"": 2
}
```

**Response com Divergência (Status 400):**
```json
{
  ""sucesso"": false,
  ""mensagem"": ""Divergência de Razão Social detectada"",
  ""divergenciaRazaoSocial"": true,
  ""razaoSocialReceita"": ""JOAO SILVA CONSULTORIA E DESENVOLVIMENTO LTDA"",
  ""razaoSocialInformada"": ""Joao Silva Consultoria"",
  ""similaridadePercentual"": 65.5,
  ""requerConfirmacao"": true
}
```", Required = true)]
    UpdateCompanyPJRequest request)
{
    // Implementação
}
```

### 7. Listar Funcionários (com Filtros)

```csharp
[HttpGet("funcionarios")]
[Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
[SwaggerOperation(
    Summary = "Listar funcionários com filtros (gestores)",
    Description = @"Lista todos os funcionários da empresa pai com opções de filtro.

**Valores Aceitos para role (filtro):**
- `2` = Financeiro
- `3` = Juridico
- `4` = FuncionarioCLT
- `5` = FuncionarioPJ

**Valores Aceitos para tipoContrato (filtro):**
- `CLT` = Funcionários CLT
- `PJ` = Funcionários PJ
- `Todos` = CLT + PJ

**Filtros Disponíveis:**
- **role**: Filtrar por tipo de usuário (enum UserRole)
- **tipoContrato**: Filtrar por CLT ou PJ
- **nome**: Busca parcial no nome (case-insensitive)
- **email**: Busca parcial no email (case-insensitive)
- **cpf**: Busca por CPF (apenas para DonoEmpresaPai)
- **ativo**: true/false/null (null = todos)

**Paginação:**
- **pageNumber**: Número da página (default: 1)
- **pageSize**: Itens por página (default: 20, max: 100)

**Ordenação:**
- **sortBy**: Campo para ordenar (nome, email, dataCriacao)
- **sortDirection**: asc ou desc (default: asc)

**Dados Retornados:**
- Nome, email, role, cargo
- CPF **mascarado** (***.***.123-45) para todos
  - DonoEmpresaPai: Pode clicar em ""Ver Detalhes"" para ver CPF completo
- Data de nascimento
- Telefones (celular e fixo)
- Status ativo/inativo
- Data de cadastro
- Para PJ: Nome da empresa PJ, CNPJ

**Permissões:**
- DonoEmpresaPai: Vê TODOS (inclusive Financeiro e Juridico)
- Financeiro: Vê apenas FuncionarioCLT e FuncionarioPJ
- Juridico: Vê apenas FuncionarioCLT e FuncionarioPJ"
)]
[SwaggerResponse(200, "Lista de funcionários retornada com sucesso", typeof(PagedResult<EmployeeListItemDTO>))]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[SwaggerResponse(403, "Sem permissão para listar funcionários")]
[ProducesResponseType(typeof(PagedResult<EmployeeListItemDTO>), StatusCodes.Status200OK)]
public async Task<IActionResult> ListarFuncionarios(
    [FromQuery] [SwaggerParameter("Filtrar por role (2=Financeiro, 3=Juridico, 4=CLT, 5=PJ)")] int? role,
    [FromQuery] [SwaggerParameter("Filtrar por tipo (CLT, PJ, Todos)")] string? tipoContrato,
    [FromQuery] [SwaggerParameter("Busca parcial no nome")] string? nome,
    [FromQuery] [SwaggerParameter("Busca parcial no email")] string? email,
    [FromQuery] [SwaggerParameter("Filtrar por status ativo/inativo")] bool? ativo,
    [FromQuery] [SwaggerParameter("Número da página (default: 1)")] int pageNumber = 1,
    [FromQuery] [SwaggerParameter("Itens por página (default: 20, max: 100)")] int pageSize = 20,
    [FromQuery] [SwaggerParameter("Campo para ordenar (nome, email, dataCriacao)")] string sortBy = "nome",
    [FromQuery] [SwaggerParameter("Direção da ordenação (asc ou desc)")] string sortDirection = "asc")
{
    // Implementação
}

// Exemplo de Request:
// GET /api/Users/funcionarios?role=5&ativo=true&pageNumber=1&pageSize=20&sortBy=nome&sortDirection=asc

// Exemplo de Response:
/*
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "totalItems": 45,
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "João Silva",
      "email": "joao.silva@consultor.com",
      "role": 5,
      "roleDescricao": "FuncionarioPJ",
      "cargo": "Desenvolvedor Full Stack",
      "cpfMascarado": "***.***.901-01",
      "telefoneCelular": "11987654321",
      "ativo": true,
      "dataCriacao": "2025-10-15T10:30:00Z",
      "empresaPJ": {
        "nome": "João Silva Consultoria ME",
        "cnpj": "12.345.678/0001-90"
      }
    }
  ]
}
*/
```

### 8. Exportar Dados LGPD (Dados Pessoais)

```csharp
[HttpGet("exportar-dados")]
[Authorize]
[SwaggerOperation(
    Summary = "Exportar dados pessoais (LGPD - Art. 18, IV)",
    Description = @"Permite usuário exportar TODOS os seus dados pessoais armazenados no sistema.

**Dados Incluídos na Exportação:**

1. **Dados Pessoais Básicos**:
   - Nome, email, telefones
   - CPF, RG (criptografados)
   - Data de nascimento
   - Endereço completo
   - Cargo

2. **Dados da Empresa PJ** (se FuncionarioPJ):
   - Razão Social, CNPJ
   - Endereço da empresa
   - Tipo de empresa (companyType)

3. **Histórico de Contratos**:
   - Todos os contratos (ativos, expirados, cancelados)
   - Valores, datas, termos, status

4. **Histórico de Pagamentos** (apenas FuncionarioPJ):
   - Todos os pagamentos recebidos
   - Valores, datas, métodos, referências

5. **Preferências de Notificação**:
   - Todas as preferências de email

6. **Logs de Acesso** (últimos 90 dias):
   - Datas/horários de login
   - IPs de acesso
   - Ações realizadas

7. **Termos Aceitos**:
   - Versões dos termos aceitos
   - Datas de aceite

**Formato da Exportação:**
- **JSON**: Estruturado e legível por máquinas
- **PDF**: Formatado e legível por humanos

**Regras de Negócio:**
- Cada usuário pode exportar apenas seus próprios dados
- DonoEmpresaPai pode exportar dados de qualquer usuário (auditoria)
- Exportação é registrada em AuditLog (rastreabilidade LGPD)
- Arquivo gerado expira em 24 horas

**Conformidade LGPD:**
- Art. 18, IV: Direito de portabilidade dos dados"
)]
[SwaggerResponse(200, "Arquivo de exportação gerado com sucesso", typeof(ExportDataResponse))]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[ProducesResponseType(typeof(ExportDataResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> ExportarDados(
    [FromQuery] [SwaggerParameter("Formato da exportação (json ou pdf)")] string formato = "json")
{
    // Implementação
}

// Exemplo de Request:
// GET /api/Users/exportar-dados?formato=pdf

// Exemplo de Response:
/*
{
  "sucesso": true,
  "mensagem": "Dados exportados com sucesso",
  "downloadUrl": "https://storage.aure.com/exports/3fa85f64-export-20251028.pdf",
  "expiresAt": "2025-10-29T10:30:00Z",
  "tamanhoArquivo": "2.5 MB"
}
*/
```

### 9. Solicitar Exclusão de Conta (LGPD + Legislação Fiscal)

```csharp
[HttpDelete("solicitar-exclusao")]
[Authorize]
[SwaggerOperation(
    Summary = "Solicitar exclusão de conta (LGPD - Art. 18, VI)",
    Description = @"Solicita a exclusão/anonimização da conta do usuário.

**⚠️ IMPORTANTE - Conformidade Legal:**

Este endpoint implementa **ANONIMIZAÇÃO** (não deleção completa) para cumprir:
- **LGPD**: Direito ao esquecimento (Art. 18, VI)
- **Legislação Fiscal Brasileira**: Documentos fiscais devem ser mantidos por 5 anos

**O que SERÁ ANONIMIZADO:**
- ✅ Nome: ""Usuário Removido {GUID}""
- ✅ Email: ""removed_{GUID}@aure.deleted""
- ✅ Telefones: Removidos (null)
- ✅ Endereço: Removido (null)
- ✅ Cargo: Removido (null)
- ✅ Avatar: Deletado do storage
- ✅ Data de Nascimento: Removida (null)
- ✅ Preferências: Resetadas
- ✅ Tokens/Sessões: Invalidados
- ✅ IsDeleted: true

**O que SERÁ MANTIDO (encriptado):**
- ⚠️ CPF/RG: Mantidos **criptografados** para auditoria fiscal (5 anos)
- ⚠️ Contratos: Mantidos para conformidade legal (5 anos)
- ⚠️ NFe: Mantidas para conformidade fiscal (5 anos)
- ⚠️ Pagamentos: Mantidos para auditoria contábil (5 anos)
- ⚠️ Logs Críticos: Login/Logout, Pagamentos, Alterações de CPF/CNPJ

**Fluxo de Exclusão:**

1. **Validações**:
   - Não pode ter contratos ativos
   - Não pode ter pagamentos pendentes
   - Não pode ter pendências financeiras

2. **Anonimização Imediata**:
   - Dados pessoais anonimizados
   - Conta bloqueada (não pode mais fazer login)

3. **Retenção de Documentos Fiscais** (5 anos):
   - Contratos, NFe, Pagamentos mantidos
   - Vinculados ao usuário anonimizado
   - Após 5 anos: Deleção física automática

4. **Notificação**:
   - Email de confirmação enviado
   - Informações sobre dados mantidos e prazo

**Conformidade LGPD:**
- Art. 18, VI: Direito ao esquecimento
- Art. 16: Eliminação de dados após término do tratamento
- Base Legal para Retenção: Cumprimento de obrigação legal (Art. 7º, II)

**Validações:**
- Usuário não pode ter contratos ativos
- Usuário não pode ter pagamentos pendentes
- FuncionarioPJ não pode ter empresa PJ com contratos ativos de outros usuários

**Período de Arrependimento:**
- 7 dias para cancelar a solicitação (conta fica bloqueada mas dados não são anonimizados)
- Após 7 dias: Anonimização é irreversível"
)]
[SwaggerResponse(200, "Solicitação de exclusão processada. Conta anonimizada.", typeof(DeleteAccountResponse))]
[SwaggerResponse(400, "Não é possível excluir: contratos ativos, pagamentos pendentes, ou outras restrições")]
[SwaggerResponse(401, "Não autenticado (token JWT ausente ou inválido)")]
[ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status200OK)]
public async Task<IActionResult> SolicitarExclusao(
    [FromBody]
    [SwaggerRequestBody(@"**Exemplo de Request:**
```json
{
  ""confirmarExclusao"": true,
  ""motivoExclusao"": ""Não utilizo mais o sistema"",
  ""senhaAtual"": ""SenhaAtual123!""
}
```", Required = true)]
    DeleteAccountRequest request)
{
    // Implementação
}

// Exemplo de Response (Sucesso):
/*
{
  ""sucesso"": true,
  ""mensagem"": ""Sua conta foi anonimizada com sucesso. Você não poderá mais fazer login."",
  ""dadosAnonimizados"": [
    ""Nome"", ""Email"", ""Telefones"", ""Endereço"", ""Avatar"", ""Data de Nascimento""
  ],
  ""dadosMantidos"": [
    ""CPF/RG (criptografados)"",
    ""Contratos (conformidade legal - 5 anos)"",
    ""NFe (conformidade fiscal - 5 anos)"",
    ""Pagamentos (auditoria contábil - 5 anos)""
  ],
  ""prazoRetencao"": ""5 anos"",
  ""dataExclusaoFinal"": ""2030-10-28T00:00:00Z""
}
*/

// Exemplo de Response (Erro - Contrato Ativo):
/*
{
  ""sucesso"": false,
  ""mensagem"": ""Não é possível excluir conta com contratos ativos"",
  ""contratosAtivos"": [
    {
      ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
      ""empresa"": ""Empresa XYZ Ltda"",
      ""dataInicio"": ""2025-01-01T00:00:00Z"",
      ""dataFim"": ""2026-01-01T00:00:00Z"",
      ""status"": ""Active""
    }
  ],
  ""acaoNecessaria"": ""Solicite o encerramento dos contratos ativos antes de excluir sua conta""
}
*/
```

---

## 🎯 Checklist de Implementação Swagger

Para **CADA endpoint** implementado, garantir:

- [ ] **SwaggerOperation** com Summary (curto) e Description (detalhada)
- [ ] **TODOS os valores de enums** listados com números E significados
- [ ] **SwaggerResponse** para TODOS os status HTTP possíveis:
  - [ ] 200/201: Sucesso
  - [ ] 400: Validação/dados inválidos
  - [ ] 401: Não autenticado
  - [ ] 403: Sem permissão
  - [ ] 404: Não encontrado (se aplicável)
- [ ] **SwaggerRequestBody** com pelo menos 2 exemplos JSON completos
- [ ] **ProducesResponseType** com tipo de retorno correto
- [ ] **Descrição de regras de negócio** relevantes
- [ ] **Descrição de validações** que serão aplicadas
- [ ] **Descrição de permissões** (quais roles podem acessar)
- [ ] **Descrição de notificações automáticas** (se aplicável)
- [ ] **Descrição de side-effects** (email enviado, auditoria, etc.)
- [ ] **Exemplos de Response** (sucesso E erro) quando útil

---

## 📌 Notas Finais

### Benefícios da Documentação Completa:

1. **Frontend Developers**: Sabem exatamente quais valores passar para cada enum
2. **QA/Testers**: Podem testar todos os cenários documentados
3. **Auditoria**: Regras de negócio documentadas facilitam compliance
4. **Manutenção**: Novo desenvolvedor entende o sistema rapidamente
5. **API Consumers**: Integração facilitada com exemplos claros

### Padrão de Qualidade:

- ✅ **Clareza**: Descrições claras e sem ambiguidade
- ✅ **Completude**: Todos os cenários documentados
- ✅ **Exemplos Reais**: JSON com valores reais (não placeholders)
- ✅ **Erros Documentados**: Todos os status HTTP possíveis
- ✅ **Regras Explícitas**: Validações e permissões claras

---

**Última Atualização**: 28 de Outubro de 2025
**Status**: ✅ Pronto para implementação
**Próximo Passo**: Aplicar este template em TODOS os controllers do sistema
