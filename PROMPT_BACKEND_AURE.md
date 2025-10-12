# PROMPT PARA CRIAÇÃO DO BACK-END AURE

Preciso que você crie um sistema back-end completo em **C# .NET 8** utilizando **Domain-Driven Design (DDD)** e arquitetura **MVC** com separação de camadas bem definida. O projeto deve seguir as melhores práticas de desenvolvimento e ser preparado para escalabilidade.

## 🎯 IMPORTANTE PARA IA/COPILOT:
- **Leia**: `.copilot-instructions.md` para diretrizes de código limpo
- **Template para novos chats**: Use `CHAT_TEMPLATE.md`
- **❌ PROIBIDO**: Console.WriteLine(), comentários desnecessários
- **✅ OBRIGATÓRIO**: Serilog logging, nomes autoexplicativos, async/await

## ESPECIFICAÇÕES TÉCNICAS

### Stack Tecnológica
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core
- **Banco de Dados**: PostgreSQL 15+ (com Npgsql provider)
- **Autenticação**: JWT Bearer Token
- **Documentação**: Swagger/OpenAPI
- **Logging**: Serilog
- **Validação**: FluentValidation
- **Mapeamento**: AutoMapper
- **Testes**: xUnit, FluentAssertions, Moq, TestContainers
- **Cache**: Redis (opcional para produção)
- **Containerização**: Docker + Docker Compose
- **Observabilidade**: OpenTelemetry, Prometheus, Jaeger
- **Mensageria**: RabbitMQ ou Apache Kafka
- **Versionamento**: API Versioning
- **Segurança**: Rate Limiting, CORS, Security Headers

### Estrutura de Camadas (DDD)
```
Aure.Domain/
├── Entities/
├── ValueObjects/
├── Enums/
├── Interfaces/
└── Services/

Aure.Application/
├── DTOs/
├── Commands/
├── Queries/
├── Services/
├── Interfaces/
└── Validators/

Aure.Infrastructure/
├── Data/
├── Repositories/
├── Services/
└── Configurations/

Aure.API/
├── Controllers/
├── Middlewares/
├── Filters/
└── Extensions/
```

## MODELO DE DOMÍNIO

### DOMÍNIO DE IDENTIDADE

**User** (Usuário do sistema)
- id_user (Guid, PK)
- name (string, obrigatório)
- email (string, único, obrigatório)
- password_hash (string, obrigatório)
- role (enum: Admin, Company, Provider)
- created_at (DateTime)
- updated_at (DateTime)
- Relacionamentos: 1:N com Session, Signature, AuditLog

**Company** (Empresas registradas)
- id_company (Guid, PK)
- name (string, obrigatório)
- cnpj (string, único, obrigatório)
- type (enum: Client, Provider, Both)
- kyc_status (enum: Pending, Approved, Rejected)
- created_at (DateTime)
- updated_at (DateTime)
- Relacionamentos: 1:N com Contract, KYCRecord, Payment

**Session** (Gerenciamento de autenticação)
- id_session (Guid, PK)
- id_user (Guid, FK)
- jwt_hash (string)
- expires_at (DateTime)
- created_at (DateTime)
- Relacionamento: N:1 com User

### DOMÍNIO DE CONTRATOS

**Contract** (Núcleo do sistema)
- id_contract (Guid, PK)
- client_id (Guid, FK para Company)
- provider_id (Guid, FK para Company)
- title (string, obrigatório)
- value_total (decimal, obrigatório)
- ipfs_cid (string, opcional)
- sha256_hash (string)
- status (enum: Draft, Active, Completed, Cancelled)
- created_at (DateTime)
- updated_at (DateTime)
- Relacionamentos: 1:N com Signature, Payment, SplitRule, Notification; 1:1 com TokenizedAsset

**Signature** (Assinatura digital)
- id_signature (Guid, PK)
- id_contract (Guid, FK)
- id_user (Guid, FK)
- signed_at (DateTime)
- signature_hash (string)
- method (enum: Digital, Electronic, Manual)
- Relacionamentos: N:1 com Contract, User

**TokenizedAsset** (Tokenização blockchain)
- id_token (Guid, PK)
- id_contract (Guid, FK, único)
- token_address (string)
- chain_id (int)
- tx_hash (string)
- created_at (DateTime)
- Relacionamento: 1:1 com Contract

### DOMÍNIO FINANCEIRO

**Payment** (Pagamentos)
- id_payment (Guid, PK)
- id_contract (Guid, FK)
- amount (decimal, obrigatório)
- method (enum: PIX, TED, CreditCard, Boleto)
- status (enum: Pending, Completed, Failed, Cancelled)
- payment_date (DateTime)
- created_at (DateTime)
- Relacionamentos: N:1 com Contract; 1:N com SplitExecution, LedgerEntry, Notification

**SplitRule** (Regras de divisão)
- id_split (Guid, PK)
- id_contract (Guid, FK)
- beneficiary_ref (string, obrigatório)
- percentage (decimal, 0-100)
- fixed_fee (decimal, opcional)
- priority (int)
- created_at (DateTime)
- Relacionamentos: N:1 com Contract; 1:N com SplitExecution

**SplitExecution** (Execução da divisão)
- id_execution (Guid, PK)
- id_payment (Guid, FK)
- id_split (Guid, FK)
- value (decimal)
- executed_at (DateTime)
- tx_hash (string, opcional)
- status (enum: Pending, Completed, Failed)
- Relacionamentos: N:1 com Payment, SplitRule

**LedgerEntry** (Registro contábil)
- id_entry (Guid, PK)
- id_payment (Guid, FK)
- id_contract (Guid, FK)
- debit (decimal)
- credit (decimal)
- currency (string, default "BRL")
- timestamp (DateTime)
- note (string, opcional)
- Relacionamentos: N:1 com Payment, Contract

### DOMÍNIO DE COMPLIANCE E AUDITORIA

**KYCRecord** (Verificação de identidade)
- id_kyc (Guid, PK)
- id_company (Guid, FK)
- document_type (enum: CNPJ, CPF, RG, Passport)
- document_hash (string)
- verified_at (DateTime, opcional)
- status (enum: Pending, Verified, Rejected)
- provider_ref (string, opcional)
- created_at (DateTime)
- Relacionamento: N:1 com Company

**AuditLog** (Trilha de auditoria)
- id_log (Guid, PK)
- entity_name (string, obrigatório)
- entity_id (Guid, obrigatório)
- action (enum: Create, Update, Delete, Read)
- performed_by (Guid, FK para User)
- ip_address (string)
- timestamp (DateTime)
- hash_chain (string)
- Relacionamentos: N:1 com User

**Notification** (Notificações)
- id_notification (Guid, PK)
- id_contract (Guid, FK, opcional)
- id_payment (Guid, FK, opcional)
- type (enum: Email, SMS, Push, InApp)
- recipient_email (string, obrigatório)
- subject (string)
- content (string)
- sent_at (DateTime, opcional)
- status (enum: Pending, Sent, Failed)
- created_at (DateTime)
- Relacionamentos: N:1 com Contract, Payment

### DOMÍNIO FISCAL

**Invoice** (Nota Fiscal)
- id_invoice (Guid, PK)
- id_contract (Guid, FK)
- id_payment (Guid, FK, opcional)
- invoice_number (string, único)
- series (string)
- access_key (string, 44 caracteres, único)
- issue_date (DateTime)
- due_date (DateTime, opcional)
- total_amount (decimal)
- tax_amount (decimal)
- status (enum: Draft, Issued, Sent, Cancelled, Error)
- xml_content (string, XML da NFe)
- pdf_url (string, opcional)
- cancellation_reason (string, opcional)
- sefaz_protocol (string, opcional)
- created_at (DateTime)
- updated_at (DateTime)
- Relacionamentos: N:1 com Contract, Payment; 1:N com InvoiceItem, TaxCalculation

**InvoiceItem** (Itens da Nota Fiscal)
- id_item (Guid, PK)
- id_invoice (Guid, FK)
- item_sequence (int)
- description (string, obrigatório)
- ncm_code (string, 8 dígitos)
- quantity (decimal)
- unit_value (decimal)
- total_value (decimal)
- tax_classification (string, CFOP)
- created_at (DateTime)
- Relacionamento: N:1 com Invoice

**TaxCalculation** (Cálculos de Impostos)
- id_tax (Guid, PK)
- id_invoice (Guid, FK)
- tax_type (enum: ICMS, ISS, PIS, COFINS, IPI, II)
- tax_rate (decimal, percentual)
- tax_base (decimal, base de cálculo)
- tax_amount (decimal, valor do imposto)
- created_at (DateTime)
- Relacionamento: N:1 com Invoice

## REQUISITOS FUNCIONAIS

### APIs Obrigatórias
1. **Autenticação e Autorização**
   - POST /api/auth/login
   - POST /api/auth/logout
   - POST /api/auth/refresh-token
   - GET /api/auth/profile

2. **Gestão de Usuários**
   - GET /api/users
   - GET /api/users/{id}
   - POST /api/users
   - PUT /api/users/{id}
   - DELETE /api/users/{id}

3. **Gestão de Empresas**
   - GET /api/companies
   - GET /api/companies/{id}
   - POST /api/companies
   - PUT /api/companies/{id}
   - DELETE /api/companies/{id}

4. **Gestão de Contratos**
   - GET /api/contracts
   - GET /api/contracts/{id}
   - POST /api/contracts
   - PUT /api/contracts/{id}
   - DELETE /api/contracts/{id}
   - POST /api/contracts/{id}/sign

5. **Gestão de Pagamentos**
   - GET /api/payments
   - GET /api/payments/{id}
   - POST /api/payments
   - PUT /api/payments/{id}/status

6. **Gestão Fiscal**
   - GET /api/invoices
   - GET /api/invoices/{id}
   - POST /api/invoices
   - PUT /api/invoices/{id}/issue
   - POST /api/invoices/{id}/cancel
   - GET /api/invoices/{id}/xml
   - GET /api/invoices/{id}/pdf

7. **Relatórios e Auditoria**
   - GET /api/audit-logs
   - GET /api/reports/financial
   - GET /api/reports/contracts
   - GET /api/reports/fiscal

## CONFIGURAÇÕES POSTGRESQL

### Pacotes NuGet Obrigatórios
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL.Design" Version="1.1.0" />
```

### Configurações Específicas PostgreSQL
- **UUID Extension**: Ativar `uuid-ossp` para geração automática de UUIDs
- **Schemas Multi-tenant**: Criar schema por empresa para isolamento
- **Índices Especializados**: 
  - GIN para campos JSONB (metadados de contratos)
  - B-tree para consultas de data/valor
  - Hash para lookups de CNPJ/email
- **Particionamento**: Tabelas de auditoria particionadas por data
- **Connection Pool**: Configurar adequadamente para alta concorrência

### Tipos PostgreSQL Utilizados
- **UUID**: Para todas as chaves primárias
- **JSONB**: Para metadados flexíveis em contratos
- **DECIMAL(18,2)**: Para valores monetários
- **TIMESTAMP WITH TIME ZONE**: Para todas as datas
- **TEXT**: Para campos de texto variável
- **BYTEA**: Para hashes e dados binários

## REQUISITOS TÉCNICOS

### Implementações Obrigatórias
- **Soft Delete**: Implementar para todas as entidades
- **Multi-tenant**: Isolamento por company_id onde aplicável
- **Audit Trail**: Log automático de todas as operações CRUD
- **Validação**: Validação robusta em todas as camadas
- **Exception Handling**: Middleware global para tratamento de erros
- **CORS**: Configuração adequada para frontend
- **Rate Limiting**: Proteção contra abuso de APIs
- **Health Checks**: Endpoints para monitoramento detalhado
- **Caching**: Implementar cache distribuído com Redis
- **Event Sourcing**: Para entidades críticas (Payment, Contract)
- **Outbox Pattern**: Garantir consistência eventual
- **API Versioning**: Suporte a múltiplas versões
- **Feature Flags**: Deploy seguro de novas funcionalidades
- **Observabilidade**: Tracing distribuído e métricas
- **Background Services**: Processamento assíncrono robusto

### Segurança
- Hash de senhas com BCrypt
- JWT com refresh token
- Validação de CNPJ/CPF
- Sanitização de inputs
- SQL Injection protection via EF Core
- HTTPS obrigatório

### Performance
- Paginação em listas com OFFSET/LIMIT otimizado
- Lazy loading configurado adequadamente
- Índices PostgreSQL específicos:
  - `CREATE INDEX idx_contracts_status ON contracts USING btree(status)`
  - `CREATE INDEX idx_payments_date ON payments USING btree(payment_date)`
  - `CREATE INDEX idx_audit_timestamp ON audit_logs USING btree(timestamp)`
  - `CREATE INDEX idx_companies_cnpj ON companies USING hash(cnpj)`
- Connection pooling com Npgsql
- Async/await em todas as operações I/O

## ESTRUTURA DE RESPOSTA

Crie o projeto com:
1. **Solução completa** com todos os projetos separados por camada
2. **Migrations** do Entity Framework configuradas para PostgreSQL
3. **Scripts SQL** para criação de extensões e schemas
4. **Seed data** básico para testes
5. **Documentação Swagger** completa
6. **Docker Compose** completo com PostgreSQL + Redis + API
7. **Dockerfile** otimizado para produção
8. **appsettings** para ambientes (Development, Docker, Production)
9. **Scripts de inicialização** PostgreSQL
10. **README.md** com instruções Docker e execução

### Configurações PostgreSQL Essenciais
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=aure_db;Username=aure_user;Password=your_password;Include Error Detail=true",
    "DockerConnection": "Host=postgres;Database=aure_db;Username=aure_user;Password=aure_password;Include Error Detail=true"
  },
  "DatabaseSettings": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "MaxRetryCount": 3
  }
}
```

## CONFIGURAÇÃO DOCKER

### Dockerfile da API
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Aure.API/Aure.API.csproj", "Aure.API/"]
COPY ["Aure.Application/Aure.Application.csproj", "Aure.Application/"]
COPY ["Aure.Domain/Aure.Domain.csproj", "Aure.Domain/"]
COPY ["Aure.Infrastructure/Aure.Infrastructure.csproj", "Aure.Infrastructure/"]
RUN dotnet restore "Aure.API/Aure.API.csproj"
COPY . .
WORKDIR "/src/Aure.API"
RUN dotnet build "Aure.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Aure.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aure.API.dll"]
```

### Docker Compose Completo
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: aure-postgres
    environment:
      POSTGRES_DB: aure_db
      POSTGRES_USER: aure_user
      POSTGRES_PASSWORD: aure_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - aure-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U aure_user -d aure_db"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: aure-redis
    ports:
      - "6379:6379"
    networks:
      - aure-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: aure-api
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ASPNETCORE_URLS=http://+:8080;https://+:8081
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=aure_db;Username=aure_user;Password=aure_password;Include Error Detail=true
      - Redis__ConnectionString=redis:6379
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - aure-network
    volumes:
      - ./logs:/app/logs
    restart: unless-stopped

volumes:
  postgres_data:

networks:
  aure-network:
    driver: bridge
```

### Scripts de Inicialização
**scripts/init.sql**
```sql
-- Extensões PostgreSQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Schema para multi-tenancy
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS reports;

-- Índices especializados serão criados via migrations
```

### Configurações de Ambiente
**appsettings.Docker.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=aure_db;Username=aure_user;Password=aure_password;Include Error Detail=true"
  },
  "Redis": {
    "ConnectionString": "redis:6379"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-jwt-key-here-256-bits",
    "Issuer": "AureAPI",
    "Audience": "AureClients",
    "ExpirationInMinutes": 60
  }
}
```

## OBSERVAÇÕES IMPORTANTES

- Utilize **UUID** (PostgreSQL type) como chave primária em todas as entidades
- Implemente **Repository Pattern** e **Unit of Work**
- Use **CQRS** para separar comandos de consultas
- Aplique **FluentValidation** em todos os DTOs
- Configure **AutoMapper** para mapeamento entre entidades e DTOs
- Implemente **logging estruturado** com Serilog
- Configure **Npgsql** com connection pooling adequado
- Use **schemas PostgreSQL** para multi-tenancy quando necessário
- Implemente **soft delete** com índices parciais PostgreSQL
- Prepare o código para futuras integrações com **blockchain** e **Open Banking**

### Scripts PostgreSQL Iniciais
Inclua scripts para:
- Criação do banco e usuário
- Ativação de extensões (uuid-ossp, pgcrypto)
- Criação de índices especializados
- Configuração de schemas multi-tenant

### Comandos Docker Essenciais
```bash
# Executar o ambiente completo
docker-compose up -d

# Executar apenas o banco para desenvolvimento local
docker-compose up -d postgres redis

# Rebuild da API
docker-compose up --build api

# Logs da aplicação
docker-compose logs -f api

# Acesso ao PostgreSQL
docker-compose exec postgres psql -U aure_user -d aure_db
```

### Health Checks e Monitoramento
- **PostgreSQL**: Verificação de conectividade
- **Redis**: Ping de status
- **API**: Endpoint `/health` para container orchestration
- **Logs estruturados**: Mapeamento para volume host

## ARQUITETURA FUTURE-PROOF

### 🚀 **Implementações Essenciais para Escalabilidade**

#### **1. Event-Driven Architecture**
```csharp
// Implementar padrão de eventos de domínio
public abstract class DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public class ContractSignedEvent : DomainEvent
{
    public Guid ContractId { get; set; }
    public Guid SignedBy { get; set; }
}
```

#### **2. Outbox Pattern (Essencial para Fintech)**
- Garantir consistência eventual entre serviços
- Evitar perda de eventos críticos (pagamentos, contratos)
- Implementar na mesma transação do banco

#### **3. API Versioning Estratégico**
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/contracts")]
public class ContractsController : ControllerBase
```

#### **4. Observabilidade Completa**
```csharp
// OpenTelemetry para tracing distribuído
services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsqlInstrumentation())
    .WithMetrics(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation());
```

#### **5. Background Services Robustos**
```csharp
// Para processamento de pagamentos assíncronos
public class PaymentProcessingService : BackgroundService
{
    // Implementar com retry, circuit breaker, dead letter queue
}
```

#### **6. Feature Flags**
```csharp
// Para deploys seguros e A/B testing
services.AddFeatureManagement();

if (await _featureManager.IsEnabledAsync("NewPaymentEngine"))
{
    // Nova implementação
}
```

#### **7. Configuração para Múltiplos Ambientes**
```json
{
  "Environments": {
    "Development": { "EnableDetailedErrors": true },
    "Staging": { "EnableSwagger": true },
    "Production": { "EnableDetailedErrors": false }
  }
}
```

### 🔒 **Segurança Avançada**

#### **8. Implementações de Segurança**
- **Encryption at Rest**: Campos sensíveis criptografados no banco
- **PII Tokenization**: Dados pessoais tokenizados
- **GDPR Compliance**: Right to be forgotten implementado
- **2FA Support**: Preparado para autenticação multifator

#### **9. Rate Limiting Inteligente**
```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});
```

### 📊 **Monitoramento e Alertas**

#### **10. Health Checks Detalhados**
```csharp
services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection)
    .AddCheck<PaymentServiceHealthCheck>("payment-service")
    .AddCheck<BlockchainHealthCheck>("blockchain-service");
```

#### **11. Métricas de Negócio**
- Contratos criados/assinados por período
- Volume financeiro processado
- Tempo médio de processamento de pagamentos
- Taxa de falha por operação

### 🔄 **Integração e Messaging**

#### **12. Message Broker (RabbitMQ/Kafka)**
```yaml
# docker-compose.yml
rabbitmq:
  image: rabbitmq:3-management
  environment:
    RABBITMQ_DEFAULT_USER: aure
    RABBITMQ_DEFAULT_PASS: password
  ports:
    - "5672:5672"
    - "15672:15672"
```

#### **13. Webhook System**
```csharp
public interface IWebhookService
{
    Task SendWebhookAsync(string url, object payload, string secret);
}
```

### 🧪 **Testes Abrangentes**

#### **14. Estratégia de Testes**
- **Unit Tests**: 80%+ cobertura em Domain/Application
- **Integration Tests**: TestContainers para PostgreSQL
- **Contract Tests**: Pact para APIs
- **Load Tests**: NBomber para performance
- **Security Tests**: OWASP ZAP integration

### 📋 **Compliance e Auditoria**

#### **15. Compliance Preparado**
```csharp
public class ComplianceService
{
    // LGPD/GDPR compliance
    Task<bool> ForgetUserDataAsync(Guid userId);
    
    // Audit trail completo
    Task<AuditReport> GenerateAuditReportAsync(DateTime from, DateTime to);
    
    // Regulatory reporting
    Task<ComplianceReport> GenerateComplianceReportAsync();
}
```

#### **16. Backup e Disaster Recovery**
- Backup automático PostgreSQL
- Point-in-time recovery
- Cross-region replication preparado
- Plano de contingência documentado

### 🔮 **Preparação para Futuro**

#### **17. Microservices Ready**
- Bounded contexts bem definidos
- Database per service preparado
- Service mesh ready (Istio)
- API Gateway preparado

#### **18. Cloud Native**
- 12-Factor App compliance
- Kubernetes manifests
- Helm charts
- CI/CD pipelines

#### **19. AI/ML Ready**
- Data pipeline para analytics
- Feature store preparado
- Model serving infrastructure
- A/B testing framework

## 📄 INTEGRAÇÃO FISCAL BRASILEIRA

### **Estruturas para Nota Fiscal Eletrônica (NFe/NFSe)**

#### **1. Integração com SEFAZ**
```csharp
// Serviço para comunicação com SEFAZ
public interface ISefazService
{
    Task<InvoiceResponse> IssueInvoiceAsync(Invoice invoice);
    Task<CancellationResponse> CancelInvoiceAsync(string accessKey, string reason);
    Task<InvoiceStatus> CheckStatusAsync(string accessKey);
    Task<string> GenerateXmlAsync(Invoice invoice);
}
```

#### **2. Bibliotecas Recomendadas**
- **ACBr.Net.NFe**: Componente gratuito para NFe/NFCe
- **Zeus.Net.NFe.NFCe**: Biblioteca .NET para emissão
- **NFe.Core**: Implementação moderna em .NET Core

#### **3. Fluxo de Emissão**
1. **Gerar XML** da nota fiscal
2. **Assinar digitalmente** com certificado A1/A3
3. **Enviar para SEFAZ** (webservice)
4. **Processar retorno** (autorização/rejeição)
5. **Gerar PDF** (DANFE)
6. **Enviar por email** ao destinatário

#### **4. Configurações Necessárias**
```json
{
  "SefazSettings": {
    "Environment": "Homologacao", // ou "Producao"
    "State": "SP", // Estado do emitente
    "CertificatePath": "certificado.pfx",
    "CertificatePassword": "senha_certificado",
    "WebServiceTimeout": 30000
  },
  "InvoiceSettings": {
    "DefaultSeries": "1",
    "InvoiceNumberSequence": "auto",
    "DefaultCfop": "5102", // Venda de mercadoria
    "CompanyLogo": "logo.png"
  }
}
```

#### **5. Campos NFe Obrigatórios**
- **Emitente**: CNPJ, Razão Social, Endereço
- **Destinatário**: CPF/CNPJ, Nome, Endereço
- **Produtos/Serviços**: Descrição, NCM, Valor, Impostos
- **Impostos**: ICMS, PIS, COFINS, ISS (conforme regime)
- **Transporte**: Modalidade de frete
- **Pagamento**: Forma e condições

#### **6. Service Implementation**
```csharp
public class InvoiceService : IInvoiceService
{
    public async Task<Invoice> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        // 1. Validar dados obrigatórios
        // 2. Calcular impostos automaticamente
        // 3. Gerar número sequencial
        // 4. Salvar como Draft
        // 5. Preparar para emissão
    }
    
    public async Task<InvoiceResult> IssueInvoiceAsync(Guid invoiceId)
    {
        // 1. Buscar invoice no banco
        // 2. Gerar XML NFe
        // 3. Assinar digitalmente
        // 4. Enviar para SEFAZ
        // 5. Processar retorno
        // 6. Atualizar status
        // 7. Gerar PDF (DANFE)
    }
}
```

#### **7. Validações Fiscais**
- **CNPJ/CPF**: Validação de dígitos verificadores
- **CEP**: Validação e preenchimento automático de endereço
- **NCM**: Classificação fiscal de produtos
- **CFOP**: Código Fiscal de Operações
- **CST/CSOSN**: Código de Situação Tributária

#### **8. Relatórios Fiscais**
- **Livro de Registro de Saídas**
- **SPED Fiscal** (preparação de arquivos)
- **Relatório de impostos** por período
- **Conciliação contábil**

#### **9. Compliance Fiscal**
- **Backup XML**: Armazenamento obrigatório por 5 anos
- **Numeração sequencial**: Sem lacunas permitidas
- **Certificado digital**: Renovação automática
- **Contingência**: Emissão offline quando SEFAZ indisponível

### **Alternativas de Implementação:**

#### **Opção 1: Integração Direta**
- Implementar comunicação direta com SEFAZ
- Maior controle, mais complexidade
- Ideal para alto volume

#### **Opção 2: API de Terceiros**
- Usar serviços como **eNotas**, **NF.io**, **Focus NFe**
- Mais simples, custo mensal
- Ideal para MVP e médio volume

#### **Opção 3: Híbrida**
- Começar com API terceirizada
- Migrar para solução própria conforme escala

O sistema deve ser robusto, escalável, containerizado, observável, fiscalmente compliant e preparado para crescimento exponencial seguindo as melhores práticas de desenvolvimento em C# e .NET Core.