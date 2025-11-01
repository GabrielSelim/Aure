---
applyTo: '**'
---

# Melhorias e Ajustes - Backend API Aure

## 📝 Objetivo
Implementar melhorias críticas no backend da API Aure, incluindo recuperação de senha, CORS, campos de perfil completo, notificações e endpoints de empresa.

---

## 1️⃣ Recuperação de Senha

### Objetivo
Implementar fluxo completo de recuperação de senha com envio de email contendo link seguro.

### Implementação Necessária

#### 1.1 DTOs (Aure.Application/DTOs/Auth/)

**Criar arquivo `PasswordRecoveryDTOs.cs`:**

```csharp
namespace Aure.Application.DTOs.Auth
{
    public class RequestPasswordResetRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
            ErrorMessage = "Senha deve conter maiúscula, minúscula, número e caractere especial")]
        public string NovaSenha { get; set; }

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "Senhas não conferem")]
        public string ConfirmacaoSenha { get; set; }
    }

    public class PasswordResetResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
    }
}
```

#### 1.2 Entidade (Aure.Domain/Entities/)

**Adicionar à entidade `User.cs`:**

```csharp
public string? PasswordResetToken { get; set; }
public DateTime? PasswordResetTokenExpiry { get; set; }

public bool IsPasswordResetTokenValid()
{
    return !string.IsNullOrEmpty(PasswordResetToken) 
        && PasswordResetTokenExpiry.HasValue 
        && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
}

public void GeneratePasswordResetToken()
{
    PasswordResetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) 
        + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(2);
}

public void ClearPasswordResetToken()
{
    PasswordResetToken = null;
    PasswordResetTokenExpiry = null;
}
```

#### 1.3 Migration

**Criar migration:**
```bash
dotnet ef migrations add AdicionarTokenRecuperacaoSenha --project src/Aure.Infrastructure --startup-project src/Aure.API
```

**Migration esperada:**
```csharp
migrationBuilder.AddColumn<string>(
    name: "PasswordResetToken",
    table: "Users",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<DateTime>(
    name: "PasswordResetTokenExpiry",
    table: "Users",
    type: "timestamp with time zone",
    nullable: true);

migrationBuilder.CreateIndex(
    name: "IX_Users_PasswordResetToken",
    table: "Users",
    column: "PasswordResetToken");
```

#### 1.4 Service (Aure.Application/Services/)

**Adicionar ao `IUserService.cs`:**
```csharp
Task<bool> RequestPasswordResetAsync(string email);
Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordRequest request);
```

**Implementar no `UserService.cs`:**
```csharp
public async Task<bool> RequestPasswordResetAsync(string email)
{
    var user = await _userRepository.GetByEmailAsync(email);
    if (user == null)
    {
        return true;
    }

    user.GeneratePasswordResetToken();
    await _userRepository.UpdateAsync(user);

    var resetLink = $"{_emailSettings.FrontendUrl}/recuperar-senha?token={Uri.EscapeDataString(user.PasswordResetToken)}";
    
    await Task.Run(async () =>
    {
        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            user.Name,
            resetLink
        );
    });

    return true;
}

public async Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordRequest request)
{
    var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token);
    
    if (user == null || !user.IsPasswordResetTokenValid())
    {
        return new PasswordResetResponse
        {
            Sucesso = false,
            Mensagem = "Token inválido ou expirado"
        };
    }

    user.SetPassword(request.NovaSenha);
    user.ClearPasswordResetToken();
    await _userRepository.UpdateAsync(user);

    return new PasswordResetResponse
    {
        Sucesso = true,
        Mensagem = "Senha alterada com sucesso"
    };
}
```

#### 1.5 Repository (Aure.Domain/Interfaces/)

**Adicionar ao `IUserRepository.cs`:**
```csharp
Task<User?> GetByPasswordResetTokenAsync(string token);
```

**Implementar em `Aure.Infrastructure/Repositories/UserRepository.cs`:**
```csharp
public async Task<User?> GetByPasswordResetTokenAsync(string token)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.PasswordResetToken == token);
}
```

#### 1.6 Email Service (Aure.Infrastructure/Services/)

**Adicionar ao `IEmailService.cs`:**
```csharp
Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink);
```

**Implementar no `EmailService.cs`:**
```csharp
public async Task<bool> SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetLink)
{
    try
    {
        var subject = "🔐 Recuperação de Senha - Aure";
        
        var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
                .warning {{ background: #FEF3C7; border-left: 4px solid #F59E0B; padding: 15px; margin: 20px 0; }}
                .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🔐 Recuperação de Senha</h1>
                </div>
                <div class='content'>
                    <p>Olá, <strong>{recipientName}</strong>!</p>
                    
                    <p>Recebemos uma solicitação para redefinir a senha da sua conta na plataforma Aure.</p>
                    
                    <p>Clique no botão abaixo para criar uma nova senha:</p>
                    
                    <div style='text-align: center;'>
                        <a href='{resetLink}' class='button'>Redefinir Senha</a>
                    </div>
                    
                    <div class='warning'>
                        <strong>⚠️ Atenção:</strong>
                        <ul>
                            <li>Este link expira em 2 horas</li>
                            <li>Se você não solicitou esta recuperação, ignore este email</li>
                            <li>Nunca compartilhe este link com outras pessoas</li>
                        </ul>
                    </div>
                    
                    <p>Se o botão não funcionar, copie e cole este link no navegador:</p>
                    <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>
                    
                    <p>Atenciosamente,<br><strong>Equipe Aure</strong></p>
                </div>
                <div class='footer'>
                    <p>Este é um email automático. Por favor, não responda.</p>
                </div>
            </div>
        </body>
        </html>";

        return await SendEmailAsync(recipientEmail, subject, body);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao enviar email de recuperação de senha para {Email}", recipientEmail);
        return false;
    }
}
```

#### 1.7 Controller (Aure.API/Controllers/)

**Adicionar ao `AuthController.cs`:**

```csharp
[HttpPost("solicitar-recuperacao-senha")]
[AllowAnonymous]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult> SolicitarRecuperacaoSenha([FromBody] RequestPasswordResetRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    await _userService.RequestPasswordResetAsync(request.Email);
    
    return Ok(new { mensagem = "Se o email existir no sistema, você receberá instruções para recuperação de senha" });
}

[HttpPost("redefinir-senha")]
[AllowAnonymous]
[ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<PasswordResetResponse>> RedefinirSenha([FromBody] ResetPasswordRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _userService.ResetPasswordAsync(request);
    
    if (!result.Sucesso)
        return BadRequest(result);
    
    return Ok(result);
}
```

---

## 2️⃣ Configuração CORS

### Objetivo
Permitir requisições do frontend em desenvolvimento (localhost:3000) e produção (aure.gabrielsanztech.com.br).

### Implementação

**Editar `Program.cs`:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "https://aure.gabrielsanztech.com.br"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
```

**Posição correta no pipeline:**
```csharp
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## 3️⃣ Campos Completos no Perfil do Usuário

### Objetivo
Garantir que ao criar a conta do dono, todos os campos do perfil sejam preenchidos corretamente, incluindo cargo automático.

### Implementação

#### 3.1 Adicionar campos faltantes ao DTO

**Editar `RegisterCompanyAdminRequest` em `UserDTOs.cs`:**

```csharp
public class RegisterCompanyAdminRequest
{
    [Required(ErrorMessage = "Nome da empresa é obrigatório")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "CNPJ é obrigatório")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "CNPJ deve ter 14 dígitos")]
    public string CompanyCnpj { get; set; }

    [Required(ErrorMessage = "Tipo da empresa é obrigatório")]
    public CompanyType CompanyType { get; set; }

    [Required(ErrorMessage = "Modelo de negócio é obrigatório")]
    public BusinessModel BusinessModel { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Password { get; set; }

    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string Cpf { get; set; }

    public string? Rg { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "Telefone celular é obrigatório")]
    [StringLength(11, MinimumLength = 10, ErrorMessage = "Telefone celular deve ter 10 ou 11 dígitos")]
    public string TelefoneCelular { get; set; }

    [StringLength(10, MinimumLength = 10, ErrorMessage = "Telefone fixo deve ter 10 dígitos")]
    public string? TelefoneFixo { get; set; }

    [Required(ErrorMessage = "Rua é obrigatória")]
    public string Rua { get; set; }

    [Required(ErrorMessage = "Número é obrigatório")]
    public string Numero { get; set; }

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; }

    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; }

    [Required(ErrorMessage = "Estado é obrigatório")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    public string Estado { get; set; }

    [Required(ErrorMessage = "País é obrigatório")]
    public string Pais { get; set; }

    [Required(ErrorMessage = "CEP é obrigatório")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "CEP deve ter 8 dígitos")]
    public string Cep { get; set; }

    [Required(ErrorMessage = "É necessário aceitar os termos de uso")]
    public bool AceitouTermosUso { get; set; }

    [Required(ErrorMessage = "Versão dos termos de uso é obrigatória")]
    public string VersaoTermosUsoAceita { get; set; }

    [Required(ErrorMessage = "É necessário aceitar a política de privacidade")]
    public bool AceitouPoliticaPrivacidade { get; set; }

    [Required(ErrorMessage = "Versão da política de privacidade é obrigatória")]
    public string VersaoPoliticaPrivacidadeAceita { get; set; }
}
```

#### 3.2 Atualizar UserService.RegisterCompanyAdminAsync

**Editar em `UserService.cs`:**

```csharp
public async Task<UserResponse> RegisterCompanyAdminAsync(RegisterCompanyAdminRequest request)
{
    var existingUser = await _userRepository.GetByEmailAsync(request.Email);
    if (existingUser != null)
        throw new BusinessException("Email já cadastrado");

    var existingCompany = await _companyRepository.GetByCnpjAsync(request.CompanyCnpj);
    if (existingCompany != null)
        throw new BusinessException("CNPJ já cadastrado");

    var company = new Company(
        request.CompanyName,
        request.CompanyCnpj,
        request.CompanyType,
        request.BusinessModel
    );

    await _companyRepository.AddAsync(company);

    var user = new User(
        request.Name,
        request.Email,
        request.Password,
        company.Id,
        UserRole.DonoEmpresaPai
    );

    user.SetCpf(request.Cpf);
    
    if (!string.IsNullOrWhiteSpace(request.Rg))
        user.SetRg(request.Rg);

    user.SetBirthDate(request.DataNascimento);
    user.SetPosition("Proprietário");

    user.UpdateProfile(request.TelefoneCelular, request.TelefoneFixo);
    
    user.UpdateAddress(
        request.Rua,
        request.Numero,
        request.Complemento,
        request.Bairro,
        request.Cidade,
        request.Estado,
        request.Pais,
        request.Cep
    );

    user.AcceptTermsOfUse(request.VersaoTermosUsoAceita);
    user.AcceptPrivacyPolicy(request.VersaoPoliticaPrivacidadeAceita);

    await _userRepository.AddAsync(user);

    await Task.Run(async () =>
    {
        await _emailService.SendWelcomeEmailAsync(
            user.Email,
            user.Name,
            company.Name
        );
    });

    return new UserResponse
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        CompanyId = user.CompanyId,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
```

#### 3.3 Adicionar métodos faltantes na entidade User

**Editar `User.cs`:**

```csharp
public void SetCpf(string cpf)
{
    if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11)
        throw new DomainException("CPF inválido");
    
    Cpf = cpf;
}

public void SetRg(string rg)
{
    if (string.IsNullOrWhiteSpace(rg))
        throw new DomainException("RG inválido");
    
    Rg = rg;
}

public void SetBirthDate(DateTime birthDate)
{
    if (birthDate >= DateTime.UtcNow)
        throw new DomainException("Data de nascimento inválida");
    
    if (birthDate < DateTime.UtcNow.AddYears(-120))
        throw new DomainException("Data de nascimento inválida");
    
    BirthDate = birthDate;
}

public void SetPosition(string position)
{
    if (string.IsNullOrWhiteSpace(position))
        throw new DomainException("Cargo inválido");
    
    Position = position;
}

public void AcceptTermsOfUse(string version)
{
    AcceptedTermsOfUse = true;
    TermsOfUseAcceptedAt = DateTime.UtcNow;
    AcceptedTermsOfUseVersion = version;
}

public void AcceptPrivacyPolicy(string version)
{
    AcceptedPrivacyPolicy = true;
    PrivacyPolicyAcceptedAt = DateTime.UtcNow;
    AcceptedPrivacyPolicyVersion = version;
}

public string GetMaskedCpf()
{
    if (string.IsNullOrEmpty(Cpf) || Cpf.Length != 11)
        return string.Empty;
    
    return $"***{Cpf.Substring(3, 3)}***{Cpf.Substring(9, 2)}";
}

public void UpdateAddress(string street, string number, string? complement, string neighborhood, 
    string city, string state, string country, string zipCode)
{
    if (string.IsNullOrWhiteSpace(street))
        throw new DomainException("Rua é obrigatória");
    
    if (string.IsNullOrWhiteSpace(number))
        throw new DomainException("Número é obrigatório");
    
    if (string.IsNullOrWhiteSpace(neighborhood))
        throw new DomainException("Bairro é obrigatório");
    
    if (string.IsNullOrWhiteSpace(city))
        throw new DomainException("Cidade é obrigatória");
    
    if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
        throw new DomainException("Estado inválido");
    
    if (string.IsNullOrWhiteSpace(country))
        throw new DomainException("País é obrigatório");
    
    if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 8)
        throw new DomainException("CEP inválido");
    
    AddressStreet = street;
    AddressNumber = number;
    AddressComplement = complement;
    AddressNeighborhood = neighborhood;
    AddressCity = city;
    AddressState = state;
    AddressCountry = country;
    AddressZipCode = zipCode;
}
```

---

## 4️⃣ Endpoint para Alterar Cargo de Funcionários

### Objetivo
Permitir que apenas o Dono da empresa altere o cargo de funcionários.

### Implementação

#### 4.1 DTO

**Criar em `UserDTOs.cs`:**

```csharp
public class UpdateEmployeePositionRequest
{
    [Required(ErrorMessage = "Cargo é obrigatório")]
    [StringLength(100, ErrorMessage = "Cargo deve ter no máximo 100 caracteres")]
    public string Cargo { get; set; }
}
```

#### 4.2 Service

**Adicionar ao `IUserService.cs`:**
```csharp
Task<UserResponse> UpdateEmployeePositionAsync(Guid employeeId, string newPosition, Guid requestingUserId);
```

**Implementar no `UserService.cs`:**
```csharp
public async Task<UserResponse> UpdateEmployeePositionAsync(Guid employeeId, string newPosition, Guid requestingUserId)
{
    var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
    if (requestingUser == null || requestingUser.Role != UserRole.DonoEmpresaPai)
        throw new UnauthorizedException("Apenas o dono da empresa pode alterar cargos");

    var employee = await _userRepository.GetByIdAsync(employeeId);
    if (employee == null)
        throw new NotFoundException("Funcionário não encontrado");

    if (employee.CompanyId != requestingUser.CompanyId)
        throw new UnauthorizedException("Você só pode alterar cargos de funcionários da sua empresa");

    if (employee.Role == UserRole.DonoEmpresaPai)
        throw new BusinessException("Não é possível alterar o cargo do proprietário");

    employee.SetPosition(newPosition);
    await _userRepository.UpdateAsync(employee);

    return new UserResponse
    {
        Id = employee.Id,
        Name = employee.Name,
        Email = employee.Email,
        Role = employee.Role,
        CompanyId = employee.CompanyId,
        IsActive = employee.IsActive,
        CreatedAt = employee.CreatedAt
    };
}
```

#### 4.3 Controller

**Adicionar ao `UsersController.cs`:**

```csharp
[HttpPut("{employeeId}/cargo")]
[Authorize(Roles = "DonoEmpresaPai")]
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<UserResponse>> AtualizarCargoFuncionario(
    Guid employeeId,
    [FromBody] UpdateEmployeePositionRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    var result = await _userService.UpdateEmployeePositionAsync(employeeId, request.Cargo, userId);
    
    return Ok(result);
}
```

---

## 5️⃣ Erro 500 em Preferências de Notificações

### Diagnóstico

Verificar se:
1. Tabela `NotificationPreferences` existe no banco
2. Relacionamento com `Users` está correto
3. Migration foi executada

### Investigação

**Verificar migration:**
```bash
dotnet ef migrations list --project src/Aure.Infrastructure --startup-project src/Aure.API
```

**Se não existir, criar:**
```bash
dotnet ef migrations add AdicionarPreferenciasNotificacoes --project src/Aure.Infrastructure --startup-project src/Aure.API
```

### Correção Esperada

**Verificar `NotificationPreferences.cs`:**
```csharp
public class NotificationPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public bool EmailPagamentos { get; set; } = true;
    public bool EmailContratos { get; set; } = true;
    public bool EmailFuncionarios { get; set; } = true;
    public bool EmailRelatorios { get; set; } = true;
    
    public bool SistemaPagamentos { get; set; } = true;
    public bool SistemaContratos { get; set; } = true;
    public bool SistemaFuncionarios { get; set; } = true;
    public bool SistemaRelatorios { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Migration deve ter:**
```csharp
migrationBuilder.CreateTable(
    name: "NotificationPreferences",
    columns: table => new
    {
        Id = table.Column<Guid>(nullable: false),
        UserId = table.Column<Guid>(nullable: false),
        EmailPagamentos = table.Column<bool>(nullable: false, defaultValue: true),
        EmailContratos = table.Column<bool>(nullable: false, defaultValue: true),
        EmailFuncionarios = table.Column<bool>(nullable: false, defaultValue: true),
        EmailRelatorios = table.Column<bool>(nullable: false, defaultValue: true),
        SistemaPagamentos = table.Column<bool>(nullable: false, defaultValue: true),
        SistemaContratos = table.Column<bool>(nullable: false, defaultValue: true),
        SistemaFuncionarios = table.Column<bool>(nullable: false, defaultValue: true),
        SistemaRelatorios = table.Column<bool>(nullable: false, defaultValue: true),
        CreatedAt = table.Column<DateTime>(nullable: false),
        UpdatedAt = table.Column<DateTime>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
        table.ForeignKey(
            name: "FK_NotificationPreferences_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateIndex(
    name: "IX_NotificationPreferences_UserId",
    table: "NotificationPreferences",
    column: "UserId",
    unique: true);
```

---

## 6️⃣ Remover Endpoint PUT /api/Users/perfil

### Objetivo
Remover endpoint duplicado, mantendo apenas o do UserProfile.

### Implementação

**Remover do `UsersController.cs`:**
```csharp
// REMOVER ESTE MÉTODO:
[HttpPut("perfil")]
public async Task<ActionResult<UserResponse>> AtualizarPerfil([FromBody] UpdateProfileRequest request)
{
    // ... código a ser removido
}
```

**Motivo:** Endpoint duplicado com `/api/UserProfile/perfil-completo` que já existe.

---

## 7️⃣ Endpoint para Dados da Empresa do Funcionário

### Objetivo
Criar endpoint para funcionários visualizarem dados da empresa em que trabalham.

### Implementação

#### 7.1 DTO

**Criar `CompanyDTOs.cs` em `Aure.Application/DTOs/Company/`:**

```csharp
namespace Aure.Application.DTOs.Company
{
    public class CompanyInfoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Cnpj { get; set; }
        public string CnpjFormatado { get; set; }
        public string Tipo { get; set; }
        public string ModeloNegocio { get; set; }
        public EnderecoEmpresaDto? Endereco { get; set; }
        public string? TelefoneFixo { get; set; }
        public string? TelefoneCelular { get; set; }
    }

    public class EnderecoEmpresaDto
    {
        public string Rua { get; set; }
        public string Numero { get; set; }
        public string? Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
        public string Cep { get; set; }
        public string EnderecoCompleto { get; set; }
    }

    public class UpdateCompanyInfoRequest
    {
        [Required(ErrorMessage = "Nome da empresa é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Telefone celular é obrigatório")]
        [StringLength(11, MinimumLength = 10)]
        public string TelefoneCelular { get; set; }

        [StringLength(10, MinimumLength = 10)]
        public string? TelefoneFixo { get; set; }

        [Required(ErrorMessage = "Rua é obrigatória")]
        public string Rua { get; set; }

        [Required(ErrorMessage = "Número é obrigatório")]
        public string Numero { get; set; }

        public string? Complemento { get; set; }

        [Required(ErrorMessage = "Bairro é obrigatório")]
        public string Bairro { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatória")]
        public string Cidade { get; set; }

        [Required(ErrorMessage = "Estado é obrigatório")]
        [StringLength(2, MinimumLength = 2)]
        public string Estado { get; set; }

        [Required(ErrorMessage = "País é obrigatório")]
        public string Pais { get; set; }

        [Required(ErrorMessage = "CEP é obrigatório")]
        [StringLength(8, MinimumLength = 8)]
        public string Cep { get; set; }
    }
}
```

#### 7.2 Adicionar campos à entidade Company

**Editar `Company.cs`:**

```csharp
public string? PhoneMobile { get; set; }
public string? PhoneLandline { get; set; }
public string? AddressStreet { get; set; }
public string? AddressNumber { get; set; }
public string? AddressComplement { get; set; }
public string? AddressNeighborhood { get; set; }
public string? AddressCity { get; set; }
public string? AddressState { get; set; }
public string? AddressCountry { get; set; }
public string? AddressZipCode { get; set; }

public void UpdateContactInfo(string phoneMobile, string? phoneLandline)
{
    if (string.IsNullOrWhiteSpace(phoneMobile))
        throw new DomainException("Telefone celular é obrigatório");
    
    PhoneMobile = phoneMobile;
    PhoneLandline = phoneLandline;
}

public void UpdateAddress(string street, string number, string? complement, string neighborhood,
    string city, string state, string country, string zipCode)
{
    if (string.IsNullOrWhiteSpace(street))
        throw new DomainException("Rua é obrigatória");
    
    if (string.IsNullOrWhiteSpace(number))
        throw new DomainException("Número é obrigatório");
    
    if (string.IsNullOrWhiteSpace(neighborhood))
        throw new DomainException("Bairro é obrigatório");
    
    if (string.IsNullOrWhiteSpace(city))
        throw new DomainException("Cidade é obrigatória");
    
    if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
        throw new DomainException("Estado inválido");
    
    if (string.IsNullOrWhiteSpace(country))
        throw new DomainException("País é obrigatório");
    
    if (string.IsNullOrWhiteSpace(zipCode) || zipCode.Length != 8)
        throw new DomainException("CEP inválido");
    
    AddressStreet = street;
    AddressNumber = number;
    AddressComplement = complement;
    AddressNeighborhood = neighborhood;
    AddressCity = city;
    AddressState = state;
    AddressCountry = country;
    AddressZipCode = zipCode;
}

public string GetFormattedCnpj()
{
    if (string.IsNullOrEmpty(Cnpj) || Cnpj.Length != 14)
        return Cnpj ?? string.Empty;
    
    return $"{Cnpj.Substring(0, 2)}.{Cnpj.Substring(2, 3)}.{Cnpj.Substring(5, 3)}/{Cnpj.Substring(8, 4)}-{Cnpj.Substring(12, 2)}";
}

public string GetFullAddress()
{
    if (string.IsNullOrWhiteSpace(AddressStreet))
        return string.Empty;
    
    var address = $"{AddressStreet}, {AddressNumber}";
    
    if (!string.IsNullOrWhiteSpace(AddressComplement))
        address += $", {AddressComplement}";
    
    address += $" - {AddressNeighborhood}, {AddressCity}/{AddressState}, {AddressCountry} - CEP: {AddressZipCode}";
    
    return address;
}
```

#### 7.3 Migration para Company

```bash
dotnet ef migrations add AdicionarContatoEnderecoEmpresa --project src/Aure.Infrastructure --startup-project src/Aure.API
```

#### 7.4 Service

**Adicionar ao `ICompanyService.cs`:**
```csharp
Task<CompanyInfoResponse> GetCompanyInfoByUserIdAsync(Guid userId);
Task<CompanyInfoResponse> UpdateCompanyInfoAsync(Guid userId, UpdateCompanyInfoRequest request);
```

**Implementar no `CompanyService.cs`:**
```csharp
public async Task<CompanyInfoResponse> GetCompanyInfoByUserIdAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
        throw new NotFoundException("Usuário não encontrado");

    var company = await _companyRepository.GetByIdAsync(user.CompanyId);
    if (company == null)
        throw new NotFoundException("Empresa não encontrada");

    return new CompanyInfoResponse
    {
        Id = company.Id,
        Nome = company.Name,
        Cnpj = company.Cnpj,
        CnpjFormatado = company.GetFormattedCnpj(),
        Tipo = company.Type.ToString(),
        ModeloNegocio = company.BusinessModel.ToString(),
        TelefoneCelular = company.PhoneMobile,
        TelefoneFixo = company.PhoneLandline,
        Endereco = string.IsNullOrWhiteSpace(company.AddressStreet) ? null : new EnderecoEmpresaDto
        {
            Rua = company.AddressStreet,
            Numero = company.AddressNumber!,
            Complemento = company.AddressComplement,
            Bairro = company.AddressNeighborhood!,
            Cidade = company.AddressCity!,
            Estado = company.AddressState!,
            Pais = company.AddressCountry!,
            Cep = company.AddressZipCode!,
            EnderecoCompleto = company.GetFullAddress()
        }
    };
}

public async Task<CompanyInfoResponse> UpdateCompanyInfoAsync(Guid userId, UpdateCompanyInfoRequest request)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null || user.Role != UserRole.DonoEmpresaPai)
        throw new UnauthorizedException("Apenas o dono da empresa pode alterar os dados da empresa");

    var company = await _companyRepository.GetByIdAsync(user.CompanyId);
    if (company == null)
        throw new NotFoundException("Empresa não encontrada");

    company.UpdateName(request.Nome);
    company.UpdateContactInfo(request.TelefoneCelular, request.TelefoneFixo);
    company.UpdateAddress(
        request.Rua,
        request.Numero,
        request.Complemento,
        request.Bairro,
        request.Cidade,
        request.Estado,
        request.Pais,
        request.Cep
    );

    await _companyRepository.UpdateAsync(company);

    return await GetCompanyInfoByUserIdAsync(userId);
}
```

#### 7.5 Controller

**Criar/Adicionar ao `UserProfileController.cs`:**

```csharp
[HttpGet("empresa")]
[Authorize]
[ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CompanyInfoResponse>> ObterDadosEmpresa()
{
    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    var result = await _companyService.GetCompanyInfoByUserIdAsync(userId);
    return Ok(result);
}

[HttpPut("empresa")]
[Authorize(Roles = "DonoEmpresaPai")]
[ProducesResponseType(typeof(CompanyInfoResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CompanyInfoResponse>> AtualizarDadosEmpresa([FromBody] UpdateCompanyInfoRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
    var result = await _companyService.UpdateCompanyInfoAsync(userId, request);
    return Ok(result);
}
```

---

## 8️⃣ Ajustar AcceptInviteRequest

### Objetivo
Incluir todos os campos necessários ao aceitar convite, similar ao registro do dono.

### Implementação

**Editar `AcceptInviteRequest` em `UserDTOs.cs`:**

```csharp
public class AcceptInviteRequest
{
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
    public string Password { get; set; }

    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string Cpf { get; set; }

    public string? Rg { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [Required(ErrorMessage = "Telefone celular é obrigatório")]
    [StringLength(11, MinimumLength = 10, ErrorMessage = "Telefone celular deve ter 10 ou 11 dígitos")]
    public string TelefoneCelular { get; set; }

    [StringLength(10, MinimumLength = 10, ErrorMessage = "Telefone fixo deve ter 10 dígitos")]
    public string? TelefoneFixo { get; set; }

    [Required(ErrorMessage = "Rua é obrigatória")]
    public string Rua { get; set; }

    [Required(ErrorMessage = "Número é obrigatório")]
    public string Numero { get; set; }

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; }

    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; }

    [Required(ErrorMessage = "Estado é obrigatório")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
    public string Estado { get; set; }

    [Required(ErrorMessage = "País é obrigatório")]
    public string Pais { get; set; }

    [Required(ErrorMessage = "CEP é obrigatório")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "CEP deve ter 8 dígitos")]
    public string Cep { get; set; }

    [Required(ErrorMessage = "É necessário aceitar os termos de uso")]
    public bool AceitouTermosUso { get; set; }

    [Required(ErrorMessage = "Versão dos termos de uso é obrigatória")]
    public string VersaoTermosUsoAceita { get; set; }

    [Required(ErrorMessage = "É necessário aceitar a política de privacidade")]
    public bool AceitouPoliticaPrivacidade { get; set; }

    [Required(ErrorMessage = "Versão da política de privacidade é obrigatória")]
    public string VersaoPoliticaPrivacidadeAceita { get; set; }
}
```

**Atualizar `UserService.AcceptInviteAsync`:**

```csharp
public async Task<UserResponse> AcceptInviteAsync(string inviteToken, AcceptInviteRequest request)
{
    var user = await _userRepository.GetByInviteTokenAsync(inviteToken);
    
    if (user == null || !user.IsInviteTokenValid())
        throw new BusinessException("Token de convite inválido ou expirado");

    user.SetPassword(request.Password);
    user.SetCpf(request.Cpf);
    
    if (!string.IsNullOrWhiteSpace(request.Rg))
        user.SetRg(request.Rg);
    
    user.SetBirthDate(request.DataNascimento);
    user.UpdateProfile(request.TelefoneCelular, request.TelefoneFixo);
    user.UpdateAddress(
        request.Rua,
        request.Numero,
        request.Complemento,
        request.Bairro,
        request.Cidade,
        request.Estado,
        request.Pais,
        request.Cep
    );
    
    user.AcceptTermsOfUse(request.VersaoTermosUsoAceita);
    user.AcceptPrivacyPolicy(request.VersaoPoliticaPrivacidadeAceita);
    user.AcceptInvite();

    await _userRepository.UpdateAsync(user);

    return new UserResponse
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        CompanyId = user.CompanyId,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
```

---

## 📋 Checklist de Implementação

### Prioridade Alta
- [ ] 1. Implementar recuperação de senha com email
- [ ] 2. Configurar CORS para localhost:3000 e aure.gabrielsanztech.com.br
- [ ] 3. Adicionar campos completos ao registro do dono
- [ ] 4. Adicionar campos completos ao aceitar convite
- [ ] 5. Criar migration para novos campos

### Prioridade Média
- [ ] 6. Criar endpoint para alterar cargo de funcionários
- [ ] 7. Investigar erro 500 em preferências de notificações
- [ ] 8. Criar endpoints de dados da empresa (GET e PUT)
- [ ] 9. Criar migration para campos da empresa

### Prioridade Baixa
- [ ] 10. Remover endpoint duplicado PUT /api/Users/perfil
- [ ] 11. Testar todos os novos endpoints
- [ ] 12. Atualizar documentação do Swagger
- [ ] 13. Validar fluxos completos (registro, convite, recuperação de senha)

---

## 🧪 Testes Necessários

### Recuperação de Senha
```bash
# Solicitar recuperação
POST /api/auth/solicitar-recuperacao-senha
Body: { "email": "teste@empresa.com" }

# Redefinir senha
POST /api/auth/redefinir-senha
Body: {
  "token": "...",
  "novaSenha": "NovaSenha@123",
  "confirmacaoSenha": "NovaSenha@123"
}
```

### Registro Completo
```bash
POST /api/registration/admin-empresa
Body: {
  "companyName": "Empresa Teste",
  "companyCnpj": "12345678000199",
  "companyType": "Client",
  "businessModel": "MainCompany",
  "name": "João Silva",
  "email": "joao@teste.com",
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
```

### Alterar Cargo
```bash
PUT /api/Users/{employeeId}/cargo
Headers: { Authorization: "Bearer {token}" }
Body: { "cargo": "Gerente de Vendas" }
```

### Dados da Empresa
```bash
# Buscar dados da empresa
GET /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token}" }

# Atualizar dados da empresa (apenas dono)
PUT /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token}" }
Body: {
  "nome": "Nova Razão Social",
  "telefoneCelular": "11987654321",
  "rua": "Rua Nova",
  "numero": "200",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "01310000"
}
```

---

## 🚀 Comandos para Produção

```powershell
# 1. Criar migrations
dotnet ef migrations add AdicionarTokenRecuperacaoSenha --project src/Aure.Infrastructure --startup-project src/Aure.API
dotnet ef migrations add AdicionarContatoEnderecoEmpresa --project src/Aure.Infrastructure --startup-project src/Aure.API
dotnet ef migrations add AdicionarCamposPerfilCompleto --project src/Aure.Infrastructure --startup-project src/Aure.API

# 2. Gerar script SQL
dotnet ef migrations script --project src/Aure.Infrastructure --startup-project src/Aure.API --output migrations_melhorias.sql

# 3. Build local
dotnet build

# 4. Commit e push
git add .
git commit -m "feat: implementar melhorias backend - recuperação senha, CORS, perfil completo, cargo e dados empresa"
git push origin main

# 5. Deploy em produção
ssh root@5.189.174.61
cd /root/Aure
git pull
docker-compose down
docker-compose up -d --build

# 6. Executar migrations
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production < migrations_melhorias.sql

# 7. Verificar logs
docker logs -f aure-api-aure-gabriel

# 8. Testar health
curl https://aureapi.gabrielsanztech.com.br/health
```

---

## ⚠️ Atenções Especiais

1. **Segurança**: Token de recuperação de senha expira em 2 horas
2. **CORS**: Verificar se as URLs estão corretas (com/sem barra final)
3. **Cargo Proprietário**: Deve ser definido automaticamente ao criar conta do dono
4. **Validação CPF**: Implementar validação de CPF válido (dígitos verificadores)
5. **Migration em Produção**: Sempre fazer backup antes de executar migrations
6. **Email Template**: Templates HTML devem ser testados em diferentes clientes de email
7. **Rate Limiting**: Considerar limite de tentativas para recuperação de senha

---

**Data de Criação**: 30/10/2025  
**Última Atualização**: 30/10/2025  
**Status**: Pendente de Implementação
