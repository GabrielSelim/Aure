# 🚀 Aure - Fintech Backend System

Sistema backend completo para fintech desenvolvido em **C# .NET 8** com **Domain-Driven Design**, **Clean Architecture** e **PostgreSQL**.

## 📋 Índice
- [Arquitetura](#-arquitetura)
- [Stack Tecnológica](#-stack-tecnológica)
- [Quick Start](#-quick-start)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [APIs Disponíveis](#-apis-disponíveis)
- [Docker](#-docker)
- [Desenvolvimento](#-desenvolvimento)

## 🏗️ Arquitetura

### Domain-Driven Design (DDD)
```
src/
├── Aure.Domain/        # Entidades, Value Objects, Interfaces
├── Aure.Application/   # Use Cases, DTOs, Services
├── Aure.Infrastructure/ # Repositórios, EF Core, Dados
└── Aure.API/          # Controllers, Middlewares, Extensions
```

### 📊 Entidades Principais
- **User** - Gerenciamento de usuários e autenticação
- **Company** - Empresas (clientes/fornecedores) com KYC
- **Contract** - Contratos com tokenização blockchain
- **Payment** - Pagamentos com regras de split
- **Invoice** - Notas fiscais (NFe/NFSe)
- **AuditLog** - Trilha de auditoria completa

## 🛠️ Stack Tecnológica

### Backend
- **.NET 8** (ASP.NET Core Web API)
- **PostgreSQL 15+** (com Npgsql)
- **Entity Framework Core** (ORM)
- **Serilog** (Structured Logging)
- **FluentValidation** (Validação)
- **AutoMapper** (Mapeamento)
- **BCrypt** (Hash de senhas)

### Infraestrutura
- **Docker** + **Docker Compose**
- **Redis** (Cache)
- **Swagger/OpenAPI** (Documentação)
- **Health Checks** (Monitoramento)

## 🚀 Quick Start

### 1. Pré-requisitos
- .NET 8 SDK
- Docker Desktop
- Git

### 2. Clonar e Executar
```bash
# Clonar o repositório
git clone <repository-url>
cd Aure

# Subir PostgreSQL e Redis
docker-compose up -d postgres redis

# Executar a API localmente
dotnet run --project src/Aure.API

# OU executar tudo no Docker
docker-compose up --build
```

### 3. Acessar
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080 (página inicial)
- **Health Check**: http://localhost:8080/health

## 📁 Estrutura do Projeto

```
Aure/
├── src/
│   ├── Aure.Domain/
│   │   ├── Entities/         # 16+ entidades de domínio
│   │   ├── Enums/           # Enumerações do sistema
│   │   ├── Interfaces/      # Contratos de repositório
│   │   └── Common/          # BaseEntity, Result Pattern
│   ├── Aure.Application/
│   │   ├── DTOs/            # Data Transfer Objects
│   │   ├── Services/        # Serviços de aplicação
│   │   ├── Interfaces/      # Contratos de serviços
│   │   ├── Validators/      # FluentValidation
│   │   └── Mappings/        # AutoMapper profiles
│   ├── Aure.Infrastructure/
│   │   ├── Data/            # DbContext, Configurations
│   │   └── Repositories/    # Implementações
│   └── Aure.API/
│       ├── Controllers/     # API Controllers
│       └── Extensions/      # DI Configuration
├── scripts/
│   └── init.sql            # Scripts PostgreSQL
├── docker-compose.yml      # Orquestração de containers
├── Dockerfile             # Container da API
└── README.md              # Este arquivo
```

## 🔌 APIs Disponíveis

### Autenticação
- `POST /api/auth/login` - Login de usuário
- `POST /api/auth/logout` - Logout

### Usuários
- `GET /api/users` - Listar usuários
- `GET /api/users/{id}` - Obter usuário por ID
- `POST /api/users` - Criar usuário
- `PUT /api/users/{id}` - Atualizar usuário
- `DELETE /api/users/{id}` - Excluir usuário

### Health Check
- `GET /health` - Status da aplicação

## � Docker

### Executar com Docker Compose
```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs da API
docker-compose logs -f api

# Parar todos os serviços
docker-compose down
```

### Serviços
- **API**: Porta 8080 (HTTP) / 8081 (HTTPS)
- **PostgreSQL**: Porta 5432
- **Redis**: Porta 6379

## 💻 Desenvolvimento

### Configurações de Ambiente
- **Development**: `appsettings.Development.json`
- **Docker**: `appsettings.Docker.json`
- **Production**: `appsettings.json`

### Migrations (Entity Framework)
```bash
# Criar migration
dotnet ef migrations add InitialCreate --project src/Aure.Infrastructure --startup-project src/Aure.API

# Aplicar migrations
dotnet ef database update --project src/Aure.Infrastructure --startup-project src/Aure.API
```

### Comandos Úteis
```bash
# Compilar solução
dotnet build

# Restaurar pacotes
dotnet restore

# Executar testes
dotnet test

# Executar API
dotnet run --project src/Aure.API
```

## � Padrões de Código

### ✅ Obrigatório
- **Serilog** para logging (ZERO Console.WriteLine)
- **Async/await** para operações I/O
- **Nomes autoexplicativos** em inglês
- **Result Pattern** para tratamento de erros
- **FluentValidation** para validações

### ❌ Proibido
- Console.WriteLine() ou Debug.WriteLine()
- Comentários explicativos desnecessários
- Comentários TODO/FIXME/HACK no código final

## 🤖 Para IA/GitHub Copilot

### Arquivos de Configuração
- **`.copilot-instructions.md`** - Diretrizes para IA
- **`CHAT_TEMPLATE.md`** - Template para novos chats
- **`.editorconfig`** - Formatação automática

### Template Rápido para Novos Chats
```
Projeto Aure: C# .NET 8 fintech + PostgreSQL + DDD
Regras: Sem Console.WriteLine, sem comentários desnecessários, use Serilog
Stack: EF Core, FluentValidation, AutoMapper, Clean Architecture
```

## 📝 Notas Importantes

- Sistema preparado para **alta escalabilidade**
- **Multi-tenant** ready
- **Blockchain tokenization** preparado
- **Compliance fiscal brasileiro** (NFe/NFSe)
- **Auditoria completa** com hash chains
- **Performance otimizada** PostgreSQL

---

**Desenvolvido com ❤️ seguindo as melhores práticas de Clean Code e DDD**