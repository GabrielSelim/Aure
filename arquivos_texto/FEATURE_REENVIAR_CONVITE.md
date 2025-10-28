# Feature: Reenviar Convite

## 📋 Descrição
Funcionalidade para reenviar emails de convite que não foram recebidos ou cujo token expirou. Permite regenerar o token mantendo todas as informações do convite original.

## 🎯 Objetivo
Resolver casos onde:
- Email não foi recebido
- Link expirou antes do usuário aceitar
- Email foi perdido/deletado

## 🔧 Implementação

### Endpoint
```
POST /api/registration/reenviar-convite/{inviteId}
```

**Autenticação**: Requer token JWT
**Autorização**: DonoEmpresaPai, Financeiro, Juridico

### Request
```json
Headers:
  Authorization: Bearer {token}

URL Parameters:
  inviteId: GUID do convite a ser reenviado
```

### Response Success (200)
```json
{
  "mensagem": "Convite reenviado com sucesso",
  "convite": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "inviterName": "João Silva",
    "inviteeEmail": "maria@example.com",
    "inviteeName": "Maria Santos",
    "role": "FuncionarioPJ",
    "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "token": "abc123def456",
    "expiresAt": "2025-11-03T17:00:00Z",
    "isAccepted": false,
    "inviteType": "ContractedPJ"
  }
}
```

### Response Errors
```json
// 400 - Convite já aceito
{
  "erro": "Não é possível reenviar convite já aceito"
}

// 401 - Não autorizado
{
  "erro": "Usuário não autenticado"
}

// 403 - Convite de outra empresa
{
  "erro": "Não autorizado a reenviar este convite"
}

// 404 - Convite não encontrado
{
  "erro": "Convite não encontrado"
}
```

## 📦 Mudanças no Código

### 1. UserInvite Entity (`Aure.Domain/Entities/UserInvite.cs`)
```csharp
public void RegenerateToken(int expirationHours = 168)
{
    Token = GenerateInviteToken();
    ExpiresAt = DateTime.UtcNow.AddHours(expirationHours);
}
```

**Funcionalidade**: 
- Gera novo token aleatório
- Estende validade por 7 dias (padrão)
- Mantém imutabilidade da entidade (private setters)

### 2. IUserService Interface (`Aure.Application/Interfaces/IUserService.cs`)
```csharp
Task<Result<InviteResponse>> ResendInviteEmailAsync(Guid inviteId, Guid currentUserId);
```

### 3. UserService Implementation (`Aure.Application/Services/UserService.cs`)
```csharp
public async Task<Result<InviteResponse>> ResendInviteEmailAsync(Guid inviteId, Guid currentUserId)
{
    // 1. Busca e valida convite
    // 2. Verifica se já foi aceito
    // 3. Verifica autorização (mesma empresa)
    // 4. Regenera token
    // 5. Salva no banco
    // 6. Busca dados da empresa
    // 7. Envia email
    // 8. Retorna resposta
}
```

**Validações Implementadas**:
- ✅ Convite existe
- ✅ Convite não foi aceito
- ✅ Usuário pertence à mesma empresa do convite
- ✅ Token regenerado com sucesso
- ✅ Email enviado

### 4. RegistrationController (`Aure.API/Controllers/RegistrationController.cs`)
```csharp
[HttpPost("reenviar-convite/{inviteId}")]
[Authorize(Roles = "DonoEmpresaPai,Financeiro,Juridico")]
public async Task<IActionResult> ReenviarConvite(Guid inviteId)
{
    // Extrai userId do token JWT
    // Chama serviço ResendInviteEmailAsync
    // Retorna resultado
}
```

## 📊 Migration
```
Migration: 20251027215749_AddRegenerateTokenMethod
Status: Aplicada com sucesso
```

**Nota**: Como `RegenerateToken()` é apenas um método (não altera schema), a migration não teve mudanças de schema específicas para UserInvite.

## 🔐 Segurança

### Autorização
- Apenas usuários autenticados podem reenviar convites
- Apenas usuários das roles: DonoEmpresaPai, Financeiro, Juridico
- Usuário deve pertencer à mesma empresa do convite

### Validações de Negócio
- Não permite reenviar convites já aceitos
- Não permite reenviar convites de outras empresas
- Gera token criptograficamente seguro
- Estende validade automaticamente

## 📧 Email
O email utiliza o mesmo template do convite original (`invite-template.html`) com:
- Nome do destinatário
- Nome de quem convidou
- Nome da empresa
- Link com novo token
- Data de expiração atualizada

## 🧪 Testes

### Teste Manual com Postman
```bash
# 1. Login como DonoEmpresaPai
POST /api/auth/login
{
  "email": "admin@empresa.com",
  "senha": "Senha@123"
}

# 2. Criar convite
POST /api/registration/convidar-usuario
{
  "nomeConvidado": "Teste PJ",
  "emailConvidado": "testepj@email.com",
  "role": "FuncionarioPJ",
  "tipoConvite": "ContractedPJ",
  "modeloNegocio": "Freelancer",
  "nomeEmpresaPJ": "PJ Teste LTDA",
  "cnpj": "12345678000199",
  "tipoEmpresaPJ": "Provider"
}

# 3. Copiar inviteId da resposta

# 4. Reenviar convite
POST /api/registration/reenviar-convite/{inviteId}
Headers:
  Authorization: Bearer {token}

# Verificar:
# - Status 200
# - Novo token gerado
# - ExpiresAt atualizado
# - Email recebido
```

### Casos de Teste
1. ✅ Reenviar convite pendente válido
2. ✅ Tentar reenviar convite já aceito (deve falhar)
3. ✅ Usuário Financeiro pode reenviar
4. ✅ Usuário Juridico pode reenviar
5. ✅ FuncionarioPJ não pode reenviar (não tem permissão)
6. ✅ Não pode reenviar convite de outra empresa
7. ✅ Convite não encontrado retorna 404

## 🎨 Swagger Documentation
```yaml
Summary: Reenviar convite pendente
Description: Reenvia o email de convite para um convite pendente que ainda não foi aceito. Gera novo token e estende validade por 7 dias.

Responses:
  200:
    description: Convite reenviado com sucesso
    schema: InviteResponse
  400:
    description: Convite já aceito ou inválido
  401:
    description: Não autenticado
  403:
    description: Acesso negado
  404:
    description: Convite não encontrado
```

## 📝 Próximos Passos
1. ✅ Implementação concluída
2. ⏳ Testar com servidor SMTP real
3. ⏳ Adicionar limite de reenvios (ex: máximo 3 por convite)
4. ⏳ Adicionar log de auditoria para rastreamento
5. ⏳ Implementar notificação para admin quando convite é reenviado

## 🐛 Troubleshooting

### Email não está sendo enviado
- Verificar configuração SMTP em `appsettings.Docker.json`
- Verificar se senha do Gmail está sem espaços
- Verificar logs em `./logs/aure-*.log`
- Testar com endpoint de teste de email primeiro

### Token inválido ao aceitar
- Verificar se token foi copiado corretamente (sem espaços)
- Verificar se convite não expirou
- Token tem validade de 7 dias

### Unauthorized ao tentar reenviar
- Verificar se usuário tem role permitida
- Verificar se usuário pertence à mesma empresa do convite
- Verificar se token JWT está válido

## 📚 Referências
- Issue: [#001] - Sistema de convites precisa suportar reenvio
- Documento: `FLUXO_TESTE_NOTIFICACOES.md` - Testes de notificações
- Documento: `Permissoes.instructions.md` - Matriz de permissões
- Template: `src/Aure.Infrastructure/Templates/invite-template.html`
