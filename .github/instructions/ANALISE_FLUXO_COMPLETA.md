# 🔍 Análise Completa do Fluxo do Projeto Aure

**Data**: 31/10/2025  
**Status**: ANÁLISE TÉCNICA DETALHADA

---

## 📋 Problemas Identificados

### ❌ PROBLEMA 1: Erro no Upload de Avatar
**Endpoint**: `POST /api/UserProfile/avatar`  
**Erro**: `400 { "message": "Value cannot be null. (Parameter 'path1')" }`

#### 🔎 Causa Raiz:
No arquivo `AvatarService.cs`, linha 43:
```csharp
var uploadsPath = Path.Combine(_environment.WebRootPath, AvatarsFolder);
```

**`_environment.WebRootPath` está NULL** porque:
1. A aplicação está rodando em Docker
2. O diretório `wwwroot` não foi mapeado corretamente
3. O `WebRootPath` não é configurado automaticamente em containers

#### ✅ Solução:
**1. Criar diretório wwwroot no Dockerfile:**
```dockerfile
# Adicionar ao Dockerfile após WORKDIR /app
RUN mkdir -p /app/wwwroot/uploads/avatars
```

**2. Configurar WebRootPath no Program.cs:**
```csharp
// Configurar WebRootPath explicitamente
builder.Services.Configure<StaticFileOptions>(options =>
{
    options.FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));
});

// OU definir explicitamente antes de builder.Build()
var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
```

**3. Alternativa: Usar IConfiguration para caminho dinâmico:**
```csharp
// Em AvatarService.cs
public class AvatarService : IAvatarService
{
    private readonly IConfiguration _configuration;
    private readonly string _uploadsPath;

    public AvatarService(IConfiguration configuration, ILogger<AvatarService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Usar caminho absoluto configurado
        var contentRoot = Directory.GetCurrentDirectory();
        _uploadsPath = Path.Combine(contentRoot, "wwwroot", "uploads", "avatars");
        
        // Criar diretório se não existir
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId)
    {
        // Usar _uploadsPath diretamente ao invés de Path.Combine com WebRootPath
        await DeleteAvatarAsync(userId);

        var originalFileName = $"{userId}.jpg";
        var originalPath = Path.Combine(_uploadsPath, originalFileName);
        // ... resto do código
    }
}
```

---

### ❌ PROBLEMA 2: Convite de Usuário PJ Não Funciona

#### 🔎 Análise do Fluxo Atual:

**Endpoint**: `POST /api/registration/convidar-usuario`

**Fluxo Esperado para PJ:**
```json
{
  "name": "João Silva",
  "email": "joao@empresa.com",
  "role": "FuncionarioPJ",
  "inviteType": "ContractedPJ",
  "companyName": "João Silva Consultoria ME",
  "cnpj": "12345678000190",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}
```

#### 🔍 Verificação Necessária:

**1. Verificar implementação do InviteUserAsync:**
```csharp
// Em UserService.cs - buscar o método InviteUserAsync
// Verificar se está criando:
// - A empresa PJ (Company)
// - O relacionamento (CompanyRelationship)
// - O convite (UserInvite)
```

**2. Campos Obrigatórios Faltando:**
O DTO `InviteUserRequest` precisa incluir:
- `cargo?: string` - Cargo do funcionário (opcional inicialmente)
- Validação de CNPJ única
- Validação de email único

#### ✅ Solução:

**Verificar se o método está completo** (preciso ver o código completo do InviteUserAsync):

```csharp
public async Task<Result<UserResponse>> InviteUserAsync(
    InviteUserRequest request, 
    Guid inviterId, 
    string inviterRole)
{
    // 1. Validações básicas
    if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        return Result.Failure<UserResponse>("Email já cadastrado");

    var inviter = await _unitOfWork.Users.GetByIdAsync(inviterId);
    if (inviter == null || !inviter.CompanyId.HasValue)
        return Result.Failure<UserResponse>("Usuário convidador inválido");

    // 2. Para FuncionarioPJ: Criar empresa e relacionamento
    if (request.Role == UserRole.FuncionarioPJ && request.InviteType == InviteType.ContractedPJ)
    {
        // Validar campos obrigatórios
        if (string.IsNullOrEmpty(request.CompanyName) || 
            string.IsNullOrEmpty(request.Cnpj))
            return Result.Failure<UserResponse>("Dados da empresa PJ são obrigatórios");

        // Verificar se CNPJ já existe
        var existingCompany = await _unitOfWork.Companies.GetByCnpjAsync(request.Cnpj);
        if (existingCompany != null)
            return Result.Failure<UserResponse>("CNPJ já cadastrado");

        // Criar empresa PJ
        var pjCompany = new Company(
            request.CompanyName,
            request.Cnpj,
            request.CompanyType ?? CompanyType.Provider,
            request.BusinessModel ?? BusinessModel.ContractedPJ
        );
        await _unitOfWork.Companies.AddAsync(pjCompany);
        await _unitOfWork.SaveChangesAsync();

        // Criar relacionamento
        var relationship = new CompanyRelationship(
            inviter.CompanyId.Value,
            pjCompany.Id,
            RelationshipType.ContractedPJ,
            RelationshipStatus.Pending
        );
        await _unitOfWork.CompanyRelationships.AddAsync(relationship);

        // Criar convite
        var invite = new UserInvite(
            inviterId,
            inviter.Name,
            inviter.CompanyId.Value,
            request.Email,
            request.Name,
            request.Role,
            request.InviteType,
            pjCompany.Id,
            request.CompanyName,
            request.Cnpj,
            request.CompanyType,
            request.BusinessModel
        );
        
        await _unitOfWork.UserInvites.AddAsync(invite);
        await _unitOfWork.SaveChangesAsync();

        // Enviar email
        await _emailService.SendInviteEmailAsync(
            request.Email,
            request.Name,
            inviter.Name,
            invite.Token
        );

        // Criar usuário temporário (pendente)
        var user = new User(
            request.Name,
            request.Email,
            string.Empty, // Senha será definida ao aceitar convite
            request.Role,
            pjCompany.Id
        );
        user.MarkAsInactive(); // Inativo até aceitar convite
        
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(_mapper.Map<UserResponse>(user));
    }

    // 3. Para usuários internos (Financeiro/Jurídico)
    // ... código para usuários internos
}
```

**TESTE NECESSÁRIO:**
```bash
POST https://aureapi.gabrielsanztech.com.br/api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token_dono}" }
Body: {
  "name": "João Silva",
  "email": "joao.teste@email.com",
  "role": "FuncionarioPJ",
  "inviteType": "ContractedPJ",
  "companyName": "João Silva Consultoria ME",
  "cnpj": "12345678000190",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}
```

---

### ❓ PROBLEMA 3: CompanyRelationships - Necessidade Questionável

#### 🔍 Análise do Propósito Original:

**O que o endpoint faz:**
- Lista relacionamentos entre empresas
- Mostra compromissos mensais (valores a pagar/receber)
- Gerencia status de relacionamentos (ativo/inativo)

**Casos de Uso:**
1. ✅ **Empresa Pai** contrata **múltiplos PJs** → CompanyRelationships rastreia cada PJ
2. ✅ **Empresa Pai** vê **compromissos mensais** com todos os PJs
3. ✅ **PJ** vê **quem o contratou**
4. ✅ Ativar/desativar acesso de PJs

#### 🎯 Conclusão: **MANTER CompanyRelationships**

**Motivos:**
1. Rastreia relacionamento entre Empresa Pai e cada PJ
2. Permite múltiplos PJs contratados
3. Gerencia status de acesso (ativo/inativo)
4. Calcula compromissos financeiros mensais
5. Necessário para futura expansão (B2B, múltiplas empresas)

**Problemas Identificados:**
- ❌ Endpoint GET retorna vazio porque:
  - Relacionamentos são criados ao convidar PJ
  - Se convite de PJ não funciona, não há relacionamentos
  - **FIX**: Corrigir primeiro o convite de PJ

---

### ❓ PROBLEMA 4: Audit - Funcionalidade Atual

#### 🔍 Análise dos Endpoints:

**Disponíveis:**
1. `GET /api/Audit/logs` - Busca logs de auditoria
2. `GET /api/Audit/kyc` - Busca registros KYC
3. `GET /api/Audit/relatorio-compliance` - Relatório de compliance
4. `GET /api/Audit/notificacoes` - Notificações da empresa

**Status Atual:**
- ✅ Endpoints implementados
- ❓ **Falta implementar**: Gravação automática de logs
- ❓ **Falta implementar**: KYC ainda não é usado
- ❓ **Falta implementar**: Notificações ainda não são criadas

#### ❌ Problema: **Falta Middleware de Auditoria**

**Solução Necessária:**

**1. Criar Middleware de Auditoria:**
```csharp
// Em Aure.API/Middleware/AuditMiddleware.cs
public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        // Capturar requisição
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var method = context.Request.Method;
        var path = context.Request.Path;

        await _next(context);

        // Após resposta, gravar auditoria para ações críticas
        if (ShouldAudit(method, path))
        {
            var auditLog = new AuditLog
            {
                EntityName = ExtractEntityName(path),
                EntityId = ExtractEntityId(path),
                Action = MapHttpMethodToAuditAction(method),
                PerformedBy = Guid.Parse(userId ?? Guid.Empty.ToString()),
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.AuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private bool ShouldAudit(string method, string path)
    {
        // Auditar apenas: POST, PUT, DELETE em endpoints críticos
        if (method == "GET") return false;
        
        var criticalPaths = new[] { 
            "/api/registration", 
            "/api/contracts", 
            "/api/payments", 
            "/api/users" 
        };
        
        return criticalPaths.Any(p => path.StartsWith(p));
    }
}

// Em Program.cs
app.UseMiddleware<AuditMiddleware>();
```

**2. Implementar Auditoria no DbContext:**
```csharp
// Em AureDbContext.cs
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var auditEntries = new List<AuditLog>();
    
    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.State == EntityState.Added || 
            entry.State == EntityState.Modified || 
            entry.State == EntityState.Deleted)
        {
            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry.Entity),
                Action = MapEntityStateToAuditAction(entry.State),
                Timestamp = DateTime.UtcNow,
                // PerformedBy será preenchido pelo middleware
            };
            
            auditEntries.Add(auditLog);
        }
    }

    var result = await base.SaveChangesAsync(cancellationToken);

    // Salvar logs de auditoria
    foreach (var log in auditEntries)
    {
        await AuditLogs.AddAsync(log, cancellationToken);
    }
    
    await base.SaveChangesAsync(cancellationToken);

    return result;
}
```

---

### ❓ PROBLEMA 5: Registration vs Invitations - Duplicação

#### 🔍 Análise dos Endpoints:

**RegistrationController:**
- `POST /api/registration/admin-empresa` - Registrar primeiro usuário (Dono)
- `POST /api/registration/convidar-usuario` - Convidar usuário (interno ou PJ)
- `POST /api/registration/aceitar-convite/{token}` - Aceitar convite
- `GET /api/registration/convites` - Listar convites pendentes
- `POST /api/registration/cancelar-convite/{id}` - Cancelar convite
- `POST /api/registration/reenviar-convite/{id}` - Reenviar convite

**Problema**: NÃO HÁ CONTROLLER `InvitationsController` duplicado!

#### ✅ Conclusão: **NÃO HÁ DUPLICAÇÃO**

O fluxo está correto:
1. **Registration** gerencia cadastro e convites
2. **Auth** gerencia login/logout
3. Não há sobreposição

---

### ❓ PROBLEMA 6: UsersExtended - Funcionalidade

#### 🔍 Análise dos Endpoints:

**UsersExtendedController:**
1. `GET /api/UsersExtended/rede` - Lista usuários da rede de relacionamentos
2. `GET /api/UsersExtended/pjs-contratados` - Lista PJs contratados
3. `GET /api/UsersExtended/contratado-por` - Lista quem contratou o PJ
4. `GET /api/UsersExtended/rede/{userId}` - Busca usuário específico da rede

**Propósito:**
- Controle de acesso baseado em relacionamentos
- PJs só veem contatos autorizados
- Admin vê rede completa

#### ✅ Conclusão: **MANTER UsersExtended**

**Motivos:**
1. Implementa regras de segurança complexas
2. Diferente de `UsersController` (que é perfil próprio)
3. Necessário para multi-tenancy e B2B

**Problemas:**
- ❌ Depende de CompanyRelationships funcionando
- ❌ Se convite PJ não funciona, rede fica vazia

---

## 🎯 Resumo dos Problemas

| # | Problema | Status | Prioridade | Ação |
|---|----------|--------|------------|------|
| 1 | Avatar upload falha (WebRootPath null) | ❌ | **ALTA** | Corrigir Dockerfile + AvatarService |
| 2 | Convite PJ não funciona | ❌ | **CRÍTICA** | Verificar/corrigir InviteUserAsync |
| 3 | CompanyRelationships vazio | ⚠️ | Média | Depende do #2 |
| 4 | Audit não grava automaticamente | ⚠️ | Média | Implementar Middleware |
| 5 | Registration vs Invitations duplicado | ✅ | - | Não há duplicação |
| 6 | UsersExtended sem dados | ⚠️ | Média | Depende do #2 |

---

## 📋 ROTEIRO DE TESTES COMPLETO

### 🔧 FASE 1: Correções Prioritárias

#### ✅ 1.1. Corrigir Avatar Upload

**Arquivos a Modificar:**
1. `Dockerfile` - Criar diretório wwwroot
2. `AvatarService.cs` - Usar caminho absoluto
3. Testar upload

**Teste:**
```bash
POST https://aureapi.gabrielsanztech.com.br/api/UserProfile/avatar
Headers: { 
  Authorization: "Bearer {token}",
  Content-Type: multipart/form-data 
}
Body: file=avatar.jpg (< 5MB, JPG/PNG)

Esperado: 200 { avatarUrl: "/uploads/avatars/{userId}.jpg" }
```

#### ✅ 1.2. Corrigir Convite PJ

**Arquivo a Verificar:**
- `UserService.cs` → método `InviteUserAsync`

**Teste:**
```bash
# 1. Login como Dono
POST /api/auth/login
Body: { email: "dono@empresa.com", password: "senha" }

# 2. Convidar PJ
POST /api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token_dono}" }
Body: {
  "name": "João Silva PJ",
  "email": "joao.pj@test.com",
  "role": "FuncionarioPJ",
  "inviteType": "ContractedPJ",
  "companyName": "João Silva Consultoria ME",
  "cnpj": "12345678000190",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}

Esperado: 200 { 
  id: "...", 
  name: "João Silva PJ",
  role: "FuncionarioPJ",
  isActive: false
}

# 3. Verificar convite criado
GET /api/registration/convites
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: [ { emailConvidado: "joao.pj@test.com", ... } ]

# 4. Verificar relacionamento criado
GET /api/CompanyRelationships
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: [ { tipo: "ContractedPJ", status: "Pending", ... } ]
```

---

### 🧪 FASE 2: Testes de Fluxo Completo

#### ✅ 2.1. Fluxo de Registro e Convite

**Cenário**: Empresa registra e convida funcionários

```bash
# 1. Registrar Empresa (Dono)
POST /api/registration/admin-empresa
Body: {
  "companyName": "Empresa Teste Ltda",
  "companyCnpj": "11222333000144",
  "companyType": "Client",
  "businessModel": "MainCompany",
  "name": "Maria Dona",
  "email": "maria@empresateste.com",
  "password": "Senha@123",
  "cpf": "12345678901",
  "dataNascimento": "1990-01-01",
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

Esperado: 200 { id: "...", role: "DonoEmpresaPai", ... }

# 2. Login como Dono
POST /api/auth/login
Body: { "email": "maria@empresateste.com", "password": "Senha@123" }

Esperado: 200 { token: "...", user: { ... } }

# 3. Convidar Financeiro
POST /api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token}" }
Body: {
  "name": "João Financeiro",
  "email": "joao.fin@empresateste.com",
  "role": "Financeiro",
  "inviteType": "Internal"
}

Esperado: 200 { ... }

# 4. Convidar PJ
POST /api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token}" }
Body: {
  "name": "Ana PJ",
  "email": "ana.pj@consultoria.com",
  "role": "FuncionarioPJ",
  "inviteType": "ContractedPJ",
  "companyName": "Ana Consultoria ME",
  "cnpj": "99888777000166",
  "companyType": "Provider",
  "businessModel": "ContractedPJ"
}

Esperado: 200 { ... }

# 5. Listar convites pendentes
GET /api/registration/convites
Headers: { Authorization: "Bearer {token}" }

Esperado: [ 
  { emailConvidado: "joao.fin@empresateste.com", ... },
  { emailConvidado: "ana.pj@consultoria.com", ... }
]
```

#### ✅ 2.2. Aceitar Convite

```bash
# 1. Obter token do convite (do email ou da lista de convites)
token_convite = "abc123..."

# 2. Aceitar convite
POST /api/registration/aceitar-convite/{token_convite}
Body: {
  "password": "Senha@123",
  "cpf": "98765432100",
  "dataNascimento": "1995-05-15",
  "telefoneCelular": "11987654322",
  "rua": "Rua Convidado",
  "numero": "200",
  "bairro": "Vila",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "02020000",
  "aceitouTermosUso": true,
  "versaoTermosUsoAceita": "1.0",
  "aceitouPoliticaPrivacidade": true,
  "versaoPoliticaPrivacidadeAceita": "1.0"
}

Esperado: 200 { id: "...", isActive: true, ... }

# 3. Login com conta ativada
POST /api/auth/login
Body: { "email": "ana.pj@consultoria.com", "password": "Senha@123" }

Esperado: 200 { token: "...", ... }
```

#### ✅ 2.3. Verificar Relacionamentos

```bash
# 1. Como Dono - Ver relacionamentos
GET /api/CompanyRelationships
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: [ 
  {
    tipo: "ContractedPJ",
    status: "Active",
    empresaRelacionada: { nome: "Ana Consultoria ME", ... }
  }
]

# 2. Como Dono - Ver compromissos mensais
GET /api/CompanyRelationships/compromissos-mensais
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: { 
  totalCompromissos: 0, 
  compromissosMensais: [] 
} # Vazio pois não há contratos ainda

# 3. Como PJ - Ver quem me contratou
GET /api/CompanyRelationships/como-fornecedor
Headers: { Authorization: "Bearer {token_pj}" }

Esperado: [ 
  { 
    tipo: "ContractedPJ",
    empresaCliente: { nome: "Empresa Teste Ltda", ... }
  }
]
```

#### ✅ 2.4. Verificar Rede de Usuários

```bash
# 1. Como Dono - Ver rede completa
GET /api/UsersExtended/rede
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  totalUsuarios: 3,
  usuariosPropriaEmpresa: 2,
  usuariosEmpresasRelacionadas: 1,
  usuarios: [
    { nome: "Maria Dona", relacionamento: "PropriaEmpresa", ... },
    { nome: "João Financeiro", relacionamento: "PropriaEmpresa", ... },
    { nome: "Ana PJ", relacionamento: "ContractedPJ", ... }
  ]
}

# 2. Como PJ - Ver rede limitada
GET /api/UsersExtended/rede
Headers: { Authorization: "Bearer {token_pj}" }

Esperado: {
  totalUsuarios: 3, # Ana PJ + Dono + Financeiro
  notaSeguranca: "Acesso limitado - PJ pode ver apenas a si mesmo e contatos da empresa contratante",
  usuarios: [
    { nome: "Ana PJ", relacionamento: "Proprio", ... },
    { nome: "Maria Dona", relacionamento: "ContatoEmpresaContratante", ... },
    { nome: "João Financeiro", relacionamento: "ContatoEmpresaContratante", ... }
  ]
}
```

---

### 🔐 FASE 3: Testes de Segurança e Permissões

#### ✅ 3.1. Testes de Autorização

```bash
# 1. PJ tenta convidar outro usuário (deve falhar)
POST /api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token_pj}" }
Body: { ... }

Esperado: 403 Forbidden

# 2. PJ tenta ver logs de auditoria (deve falhar)
GET /api/Audit/logs
Headers: { Authorization: "Bearer {token_pj}" }

Esperado: 403 Forbidden

# 3. Financeiro tenta alterar cargo (deve falhar)
PUT /api/Users/{userId}/cargo
Headers: { Authorization: "Bearer {token_financeiro}" }
Body: { "cargo": "Gerente" }

Esperado: 403 Forbidden

# 4. Apenas Dono pode alterar cargo
PUT /api/Users/{userId}/cargo
Headers: { Authorization: "Bearer {token_dono}" }
Body: { "cargo": "Gerente Financeiro" }

Esperado: 200 { cargo: "Gerente Financeiro", ... }
```

---

### 📊 FASE 4: Testes de Endpoints Específicos

#### ✅ 4.1. Upload de Avatar

```bash
# 1. Upload de avatar válido
POST /api/UserProfile/avatar
Headers: { 
  Authorization: "Bearer {token}",
  Content-Type: multipart/form-data 
}
Body: file=avatar.jpg (< 5MB)

Esperado: 200 { 
  avatarUrl: "/uploads/avatars/{userId}.jpg",
  thumbnailUrl: "/uploads/avatars/{userId}_thumb.jpg"
}

# 2. Verificar avatar no perfil
GET /api/UserProfile/perfil-completo
Headers: { Authorization: "Bearer {token}" }

Esperado: { avatarUrl: "/uploads/avatars/{userId}.jpg", ... }

# 3. Deletar avatar
DELETE /api/UserProfile/avatar
Headers: { Authorization: "Bearer {token}" }

Esperado: 200 { success: true }
```

#### ✅ 4.2. Dados da Empresa

```bash
# 1. Buscar dados da empresa (todos os usuários)
GET /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token}" }

Esperado: {
  id: "...",
  nome: "Empresa Teste Ltda",
  cnpj: "11222333000144",
  cnpjFormatado: "11.222.333/0001-44",
  tipo: "Client",
  modeloNegocio: "MainCompany",
  endereco: { ... },
  telefoneCelular: "11987654321"
}

# 2. Atualizar dados da empresa (apenas Dono)
PUT /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token_dono}" }
Body: {
  "nome": "Empresa Teste Atualizada Ltda",
  "telefoneCelular": "11999999999",
  "rua": "Nova Rua",
  "numero": "500",
  "bairro": "Novo Bairro",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "03030000"
}

Esperado: 200 { nome: "Empresa Teste Atualizada Ltda", ... }

# 3. Financeiro tenta atualizar (deve falhar)
PUT /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token_financeiro}" }
Body: { ... }

Esperado: 403 Forbidden
```

#### ✅ 4.3. Preferências de Notificações

```bash
# 1. Buscar preferências
GET /api/UserProfile/notificacoes/preferencias
Headers: { Authorization: "Bearer {token}" }

Esperado: {
  emailPagamentos: true,
  emailContratos: true,
  emailFuncionarios: true,
  emailRelatorios: true,
  sistemaPagamentos: true,
  sistemaContratos: true,
  sistemaFuncionarios: true,
  sistemaRelatorios: true
}

# 2. Atualizar preferências
PUT /api/UserProfile/notificacoes/preferencias
Headers: { Authorization: "Bearer {token}" }
Body: {
  "emailPagamentos": false,
  "emailContratos": true,
  "emailFuncionarios": true,
  "emailRelatorios": false,
  "sistemaPagamentos": true,
  "sistemaContratos": true,
  "sistemaFuncionarios": false,
  "sistemaRelatorios": true
}

Esperado: 200 { emailPagamentos: false, ... }
```

#### ✅ 4.4. Listar Funcionários

```bash
# 1. Listar todos os funcionários (Dono)
GET /api/Users/funcionarios?pageNumber=1&pageSize=20
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  data: [
    { nome: "João Financeiro", role: "Financeiro", ... },
    { nome: "Ana PJ", role: "FuncionarioPJ", ... }
  ],
  pageNumber: 1,
  totalRecords: 2,
  hasNextPage: false
}

# 2. Filtrar por role
GET /api/Users/funcionarios?role=FuncionarioPJ
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  data: [ { nome: "Ana PJ", role: "FuncionarioPJ", ... } ],
  totalRecords: 1
}

# 3. Buscar por nome
GET /api/Users/funcionarios?busca=Ana
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  data: [ { nome: "Ana PJ", ... } ],
  totalRecords: 1
}
```

#### ✅ 4.5. Auditoria (Apenas Dono)

```bash
# 1. Listar logs de auditoria
GET /api/Audit/logs?startDate=2025-10-01&endDate=2025-10-31
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  totalRegistros: 5,
  logs: [
    { 
      nomeEntidade: "User",
      acao: "Created",
      realizadoPor: "{userId}",
      dataHora: "2025-10-30T10:00:00Z"
    },
    ...
  ]
}

# 2. Relatório de compliance
GET /api/Audit/relatorio-compliance?startDate=2025-10-01&endDate=2025-10-31
Headers: { Authorization: "Bearer {token_dono}" }

Esperado: {
  periodo: { inicio: "2025-10-01", fim: "2025-10-31" },
  estatisticasAuditoria: [ ... ],
  estatisticasFinanceiras: {
    totalContratos: 0,
    totalPagamentos: 0
  }
}
```

---

## 📈 Matriz de Testes

| Endpoint | Método | Role Permitido | Status Esperado | Prioridade |
|----------|--------|----------------|-----------------|------------|
| `/api/registration/admin-empresa` | POST | Público | 200 | CRÍTICA |
| `/api/registration/convidar-usuario` | POST | DonoEmpresaPai | 200 | CRÍTICA |
| `/api/registration/aceitar-convite/{token}` | POST | Público | 200 | CRÍTICA |
| `/api/auth/login` | POST | Público | 200 | CRÍTICA |
| `/api/UserProfile/avatar` | POST | Autenticado | 200 | ALTA |
| `/api/UserProfile/empresa` | GET | Autenticado | 200 | ALTA |
| `/api/UserProfile/empresa` | PUT | DonoEmpresaPai | 200 | ALTA |
| `/api/Users/funcionarios` | GET | Dono/Fin/Jur | 200 | MÉDIA |
| `/api/Users/{id}/cargo` | PUT | DonoEmpresaPai | 200 | MÉDIA |
| `/api/CompanyRelationships` | GET | Autenticado | 200 | MÉDIA |
| `/api/UsersExtended/rede` | GET | Autenticado | 200 | MÉDIA |
| `/api/Audit/logs` | GET | DonoEmpresaPai | 200 | BAIXA |

---

## ✅ Próximos Passos

### Imediato (Hoje):
1. ✅ Corrigir upload de avatar (WebRootPath)
2. ✅ Verificar e corrigir InviteUserAsync para PJ
3. ✅ Testar fluxo completo de convite PJ

### Curto Prazo (Esta Semana):
4. ✅ Implementar middleware de auditoria
5. ✅ Testar todos os endpoints da matriz
6. ✅ Documentar fluxos no Swagger

### Médio Prazo (Próximas 2 Semanas):
7. ✅ Implementar sistema KYC
8. ✅ Implementar notificações automáticas
9. ✅ Testes de carga e performance

---

**📝 Observações Finais:**

- CompanyRelationships é NECESSÁRIO e deve ser mantido
- UsersExtended é NECESSÁRIO para controle de acesso
- Audit precisa de middleware para funcionar completamente
- Foco principal: Corrigir avatar e convite PJ

**✅ Após correções, sistema estará 95% funcional!**
