# 🔧 Implementação: Sistema de Logs Automáticos (Auditoria)

**Data**: 31/10/2025  
**Prioridade**: MÉDIA  
**Impacto**: Adiciona auditoria automática em todas as operações críticas

---

## 📋 O que foi implementado

### 1️⃣ **AuditMiddleware** - Captura automática de requisições
**Arquivo**: `src/Aure.API/Middleware/AuditMiddleware.cs`

**Funcionalidade:**
- Intercepta todas as requisições HTTP
- Captura dados da requisição (método, path, usuário, IP, etc.)
- Cria registro automático no banco de dados
- Não bloqueia requisições GET de leitura

**Eventos Auditados:**
- ✅ POST, PUT, PATCH, DELETE (todas as modificações)
- ✅ GET com erro (4xx, 5xx)
- ❌ GET com sucesso (200) - não registrado para não poluir logs

**Endpoints Auditados:**
- `/api/registration/*` - Registros e convites
- `/api/auth/login` - Tentativas de login
- `/api/contracts/*` - Contratos
- `/api/payments/*` - Pagamentos
- `/api/users/*` - Usuários
- `/api/userprofile/*` - Perfil de usuário
- `/api/companyrelationships/*` - Relacionamentos entre empresas

**Dados Capturados:**
- Usuário que executou (ID + email)
- IP e User-Agent
- Endpoint acessado
- Método HTTP (POST, PUT, DELETE, etc.)
- Status da resposta (200, 400, 500, etc.)
- Tempo de execução (em milissegundos)
- Sucesso ou falha

---

### 2️⃣ **Entidade AuditLog Atualizada**
**Arquivo**: `src/Aure.Domain/Entities/AuditLog.cs`

**Campos Adicionados:**
```csharp
public string? PerformedByEmail { get; set; }      // Email do usuário
public string? UserAgent { get; set; }             // Navegador/App usado
public string? HttpMethod { get; set; }            // POST, PUT, DELETE
public string? Path { get; set; }                  // /api/users/123
public int StatusCode { get; set; }                // 200, 400, 500
public double Duration { get; set; }               // Tempo em ms
public bool Success { get; set; }                  // true/false
```

**Campos tornados opcionais (nullable):**
- `EntityId` - Algumas operações não têm ID específico (ex: listagens)
- `PerformedBy` - Requisições não autenticadas (ex: login, registro)

---

### 3️⃣ **Program.cs Atualizado**

**Mudanças:**
1. Criação automática de `wwwroot` (para avatares)
2. `app.UseStaticFiles()` habilitado
3. `app.UseAuditMiddleware()` adicionado

**Ordem do Pipeline:**
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();           // ← Servir arquivos estáticos (avatares)
app.UseCors(...);
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditMiddleware();       // ← Auditoria APÓS autenticação
app.MapControllers();
```

---

## 🚀 Comandos de Implantação

### Passo 1: Criar Migration

```powershell
cd C:\Users\gabriel\source\repos\GabrielSelim\Aure

dotnet ef migrations add AtualizarAuditLogComCamposExtras --project src/Aure.Infrastructure --startup-project src/Aure.API
```

**Migration esperada:**
```csharp
migrationBuilder.AddColumn<string>(
    name: "PerformedByEmail",
    table: "AuditLogs",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "UserAgent",
    table: "AuditLogs",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "HttpMethod",
    table: "AuditLogs",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "Path",
    table: "AuditLogs",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<int>(
    name: "StatusCode",
    table: "AuditLogs",
    type: "integer",
    nullable: false,
    defaultValue: 0);

migrationBuilder.AddColumn<double>(
    name: "Duration",
    table: "AuditLogs",
    type: "double precision",
    nullable: false,
    defaultValue: 0.0);

migrationBuilder.AddColumn<bool>(
    name: "Success",
    table: "AuditLogs",
    type: "boolean",
    nullable: false,
    defaultValue: false);

migrationBuilder.AlterColumn<Guid>(
    name: "EntityId",
    table: "AuditLogs",
    type: "uuid",
    nullable: true,
    oldClrType: typeof(Guid),
    oldType: "uuid",
    oldNullable: false);

migrationBuilder.AlterColumn<Guid>(
    name: "PerformedBy",
    table: "AuditLogs",
    type: "uuid",
    nullable: true,
    oldClrType: typeof(Guid),
    oldType: "uuid",
    oldNullable: false);
```

### Passo 2: Commit e Push

```powershell
git add .
git commit -m "feat: implementar auditoria automática com middleware + fix avatar upload"
git push origin main
```

### Passo 3: Deploy em Produção

```powershell
ssh root@5.189.174.61

cd /root/Aure
git pull

docker-compose down

docker-compose up -d --build

docker logs -f aure-api-aure-gabriel
```

### Passo 4: Aplicar Migration

```powershell
# Dentro do servidor SSH
docker exec -it aure-api-aure-gabriel dotnet ef database update --project /app/Aure.Infrastructure.dll

# OU gerar script SQL e executar manualmente
docker exec -it aure-api-aure-gabriel dotnet ef migrations script --project /app/Aure.Infrastructure.dll --output /app/migration.sql

# Executar script
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production < /app/migration.sql
```

### Passo 5: Verificar Funcionamento

```bash
# 1. Fazer uma requisição (ex: login)
POST https://aureapi.gabrielsanztech.com.br/api/auth/login
Body: { "email": "teste@test.com", "password": "senha" }

# 2. Verificar se log foi criado
docker exec -it aure-postgres-aure-gabriel psql -U aure_user -d aure_production

SELECT * FROM "AuditLogs" ORDER BY "Timestamp" DESC LIMIT 10;

# Esperado: Ver registro com:
# - EntityName = "Authentication"
# - Action = "Create"
# - HttpMethod = "POST"
# - Path = "/api/auth/login"
# - StatusCode = 200 ou 401
# - PerformedByEmail = NULL (pois ainda não está logado)
```

---

## 📊 Exemplos de Logs Gerados

### Login Bem-Sucedido:
```json
{
  "id": "...",
  "entityName": "Authentication",
  "entityId": null,
  "action": "Create",
  "performedBy": null,
  "performedByEmail": null,
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "timestamp": "2025-10-31T14:30:00Z",
  "httpMethod": "POST",
  "path": "/api/auth/login",
  "statusCode": 200,
  "duration": 245.5,
  "success": true
}
```

### Convite de Usuário:
```json
{
  "id": "...",
  "entityName": "User",
  "entityId": null,
  "action": "Create",
  "performedBy": "123e4567-e89b-12d3-a456-426614174000",
  "performedByEmail": "dono@empresa.com",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "timestamp": "2025-10-31T15:00:00Z",
  "httpMethod": "POST",
  "path": "/api/registration/convidar-usuario",
  "statusCode": 200,
  "duration": 1523.2,
  "success": true
}
```

### Tentativa de Acesso Negada:
```json
{
  "id": "...",
  "entityName": "Payment",
  "entityId": "456e7890-b12c-34d5-e678-901234567890",
  "action": "Update",
  "performedBy": "789e0123-c45d-67e8-f901-234567890abc",
  "performedByEmail": "financeiro@empresa.com",
  "ipAddress": "192.168.1.101",
  "timestamp": "2025-10-31T15:30:00Z",
  "httpMethod": "PUT",
  "path": "/api/payments/456e7890-b12c-34d5-e678-901234567890",
  "statusCode": 403,
  "duration": 45.8,
  "success": false
}
```

---

## 🔍 Como Consultar Logs

### Via Endpoint (GET /api/Audit/logs):

```bash
GET https://aureapi.gabrielsanztech.com.br/api/audit/logs?startDate=2025-10-01&endDate=2025-10-31
Headers: { Authorization: "Bearer {token_dono}" }
```

### Via Banco de Dados:

```sql
-- Logs das últimas 24 horas
SELECT 
    "Timestamp",
    "EntityName",
    "Action",
    "PerformedByEmail",
    "HttpMethod",
    "Path",
    "StatusCode",
    "Duration",
    "Success"
FROM "AuditLogs"
WHERE "Timestamp" >= NOW() - INTERVAL '24 hours'
ORDER BY "Timestamp" DESC;

-- Logs de um usuário específico
SELECT * FROM "AuditLogs"
WHERE "PerformedBy" = '123e4567-e89b-12d3-a456-426614174000'
ORDER BY "Timestamp" DESC;

-- Logs de tentativas falhadas
SELECT * FROM "AuditLogs"
WHERE "Success" = false
ORDER BY "Timestamp" DESC;

-- Operações mais lentas
SELECT 
    "Path",
    AVG("Duration") as "TempoMedio",
    COUNT(*) as "Quantidade"
FROM "AuditLogs"
WHERE "Timestamp" >= NOW() - INTERVAL '7 days'
GROUP BY "Path"
ORDER BY "TempoMedio" DESC
LIMIT 10;
```

---

## ⚙️ Configurações Avançadas

### Desabilitar Auditoria em Desenvolvimento:

```csharp
// Em Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseAuditMiddleware();
}
```

### Auditar apenas endpoints específicos:

```csharp
// Em AuditMiddleware.cs, método ShouldAudit
var criticalPaths = new[]
{
    "/api/payments",      // Apenas pagamentos
    "/api/contracts",     // Apenas contratos
    "/api/auth/login"     // Apenas login
};
```

### Adicionar campos customizados:

```csharp
// Em AuditMiddleware.cs
var auditLog = new AuditLog
{
    // ... campos existentes
    CompanyId = user.CompanyId,  // ← Adicionar ID da empresa
    DeviceType = GetDeviceType(context.Request.Headers["User-Agent"])
};
```

---

## 📋 Checklist de Testes

### Após Deploy:

- [ ] Fazer login → Verificar log criado
- [ ] Convidar usuário → Verificar log criado
- [ ] Tentar acessar endpoint sem permissão → Verificar log com status 403
- [ ] Fazer requisição GET → Verificar que NÃO criou log (otimização)
- [ ] Consultar GET /api/audit/logs → Ver logs listados
- [ ] Verificar performance (logs não devem impactar tempo de resposta significativamente)

### Queries de Validação:

```sql
-- Deve retornar > 0
SELECT COUNT(*) FROM "AuditLogs" WHERE "Timestamp" >= NOW() - INTERVAL '1 hour';

-- Verificar estrutura da tabela
\d "AuditLogs"
```

---

## 🚨 Troubleshooting

### Erro: "Coluna não existe"
**Causa**: Migration não foi aplicada  
**Solução**: Executar `dotnet ef database update`

### Logs não aparecem
**Causa**: Middleware não está sendo executado  
**Solução**: Verificar ordem no Program.cs (deve estar APÓS Authentication)

### Performance lenta
**Causa**: Muitos logs sendo criados  
**Solução**: 
1. Aumentar filtros no `ShouldAudit`
2. Criar índices no banco:
```sql
CREATE INDEX idx_auditlogs_timestamp ON "AuditLogs"("Timestamp" DESC);
CREATE INDEX idx_auditlogs_performedby ON "AuditLogs"("PerformedBy");
CREATE INDEX idx_auditlogs_entityname ON "AuditLogs"("EntityName");
```

---

## ✅ Vantagens da Implementação

1. **Automático**: Não precisa adicionar código em cada endpoint
2. **Completo**: Captura TODAS as operações críticas
3. **Não-intrusivo**: Não afeta código de negócio
4. **Performance**: Assíncrono, não bloqueia requisições
5. **Compliance**: Atende LGPD e requisitos de auditoria
6. **Debugging**: Facilita investigação de problemas

---

## 📊 Relatórios Disponíveis

Com os logs automáticos, você pode criar:

1. **Relatório de Atividades por Usuário**
2. **Relatório de Tentativas de Acesso Negado** (segurança)
3. **Relatório de Performance** (endpoints mais lentos)
4. **Relatório de Uso** (endpoints mais acessados)
5. **Timeline de Ações** (para investigação)

---

**Status**: ✅ Implementado e pronto para deploy  
**Tempo Estimado**: 30 minutos (migration + deploy + testes)  
**Impacto**: Médio - Adiciona auditoria sem afetar funcionalidades existentes
