# 🚀 Script de Gerenciamento Docker - Sistema Aure (PowerShell)
# Data: 20/10/2025

param(
    [Parameter(Position=0)]
    [string]$Command
)

# Cores para output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"
$Cyan = "Cyan"

# Função para logs coloridos
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

# Função para exibir ajuda
function Show-Help {
    Write-Host "🚀 Sistema Aure - Gerenciamento Docker" -ForegroundColor $Cyan
    Write-Host ""
    Write-Host "Uso: .\docker.ps1 [COMANDO]"
    Write-Host ""
    Write-Host "Comandos disponíveis:" -ForegroundColor $Green
    Write-Host ""
    Write-Host "📦 DESENVOLVIMENTO:" -ForegroundColor $Green
    Write-Host "  dev-up        - Subir PostgreSQL + Redis + Adminer para desenvolvimento"
    Write-Host "  dev-db        - Subir apenas PostgreSQL para desenvolvimento"
    Write-Host "  dev-down      - Parar serviços de desenvolvimento"
    Write-Host "  dev-logs      - Ver logs dos serviços de desenvolvimento"
    Write-Host ""
    Write-Host "🏗️ AMBIENTE COMPLETO:" -ForegroundColor $Green
    Write-Host "  up            - Subir todos os serviços (API + Banco + Redis)"
    Write-Host "  down          - Parar todos os serviços"
    Write-Host "  restart       - Reiniciar todos os serviços"
    Write-Host "  logs          - Ver logs de todos os serviços"
    Write-Host "  logs-api      - Ver logs apenas da API"
    Write-Host ""
    Write-Host "🚀 PRODUÇÃO:" -ForegroundColor $Green
    Write-Host "  prod-up       - Subir ambiente de produção"
    Write-Host "  prod-down     - Parar ambiente de produção"
    Write-Host "  prod-logs     - Ver logs de produção"
    Write-Host ""
    Write-Host "🛠️ MANUTENÇÃO:" -ForegroundColor $Green
    Write-Host "  build         - Rebuild das imagens Docker"
    Write-Host "  clean         - Limpar containers e volumes (CUIDADO!)"
    Write-Host "  status        - Status dos containers"
    Write-Host "  db-migrate    - Aplicar migrações Entity Framework"
    Write-Host "  db-reset      - Reset completo do banco (CUIDADO!)"
    Write-Host ""
    Write-Host "📊 MONITORAMENTO:" -ForegroundColor $Green
    Write-Host "  adminer       - Abrir Adminer (admin banco)"
    Write-Host "  mailhog       - Abrir MailHog (teste emails)"
    Write-Host ""
    Write-Host "Exemplos:"
    Write-Host "  .\docker.ps1 dev-up        # Para desenvolvimento"
    Write-Host "  .\docker.ps1 up            # Ambiente completo"
    Write-Host "  .\docker.ps1 logs-api      # Ver logs da API"
    Write-Host ""
}

# Verificar se Docker está rodando
function Test-Docker {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        Write-Error "Docker não está rodando. Inicie o Docker primeiro."
        exit 1
    }
}

# Comando principal
switch ($Command) {
    "dev-up" {
        Test-Docker
        Write-Info "Subindo serviços de desenvolvimento..."
        docker-compose -f docker-compose.dev.yml up -d
        Write-Success "Serviços de desenvolvimento iniciados!"
        Write-Info "Adminer: http://localhost:8080"
        Write-Info "MailHog: http://localhost:8025"
    }
    
    "dev-db" {
        Test-Docker
        Write-Info "Subindo apenas PostgreSQL para desenvolvimento..."
        docker-compose -f docker-compose.dev.yml up postgres-dev -d
        Write-Success "PostgreSQL de desenvolvimento iniciado!"
        Write-Info "Conexão: localhost:5432 - DB: aure_db - User: aure_user - Pass: aure_password"
    }
    
    "dev-down" {
        Write-Info "Parando serviços de desenvolvimento..."
        docker-compose -f docker-compose.dev.yml down
        Write-Success "Serviços de desenvolvimento parados!"
    }
    
    "dev-logs" {
        docker-compose -f docker-compose.dev.yml logs -f
    }
    
    "up" {
        Test-Docker
        Write-Info "Subindo todos os serviços..."
        docker-compose up -d
        Write-Success "Todos os serviços iniciados!"
        Write-Info "API: http://localhost:5203"
        Write-Info "Swagger: http://localhost:5203/swagger"
        Write-Info "Adminer: http://localhost:8080"
    }
    
    "down" {
        Write-Info "Parando todos os serviços..."
        docker-compose down
        Write-Success "Todos os serviços parados!"
    }
    
    "restart" {
        Write-Info "Reiniciando todos os serviços..."
        docker-compose down
        docker-compose up -d
        Write-Success "Todos os serviços reiniciados!"
    }
    
    "logs" {
        docker-compose logs -f
    }
    
    "logs-api" {
        docker-compose logs -f api
    }
    
    "prod-up" {
        Test-Docker
        Write-Warning "Iniciando ambiente de produção..."
        if (-not (Test-Path "secrets")) {
            Write-Error "Diretório 'secrets' não encontrado. Configure os secrets primeiro!"
            exit 1
        }
        docker-compose -f docker-compose.prod.yml up -d
        Write-Success "Ambiente de produção iniciado!"
    }
    
    "prod-down" {
        Write-Info "Parando ambiente de produção..."
        docker-compose -f docker-compose.prod.yml down
        Write-Success "Ambiente de produção parado!"
    }
    
    "prod-logs" {
        docker-compose -f docker-compose.prod.yml logs -f
    }
    
    "build" {
        Test-Docker
        Write-Info "Fazendo rebuild das imagens..."
        docker-compose build --no-cache
        Write-Success "Rebuild concluído!"
    }
    
    "clean" {
        Test-Docker
        Write-Warning "ATENÇÃO: Isso irá remover TODOS os containers e volumes!"
        $response = Read-Host "Tem certeza? (y/N)"
        if ($response -eq "y" -or $response -eq "Y") {
            Write-Info "Limpando containers e volumes..."
            docker-compose down -v
            docker system prune -a -f
            Write-Success "Limpeza concluída!"
        }
        else {
            Write-Info "Operação cancelada."
        }
    }
    
    "status" {
        Test-Docker
        Write-Info "Status dos containers:"
        docker-compose ps
    }
    
    "db-migrate" {
        Write-Info "Aplicando migrações Entity Framework..."
        dotnet ef database update --project ".\src\Aure.Infrastructure" --startup-project ".\src\Aure.API"
        Write-Success "Migrações aplicadas!"
    }
    
    "db-reset" {
        Write-Warning "ATENÇÃO: Isso irá RESETAR completamente o banco de dados!"
        $response = Read-Host "Tem certeza? (y/N)"
        if ($response -eq "y" -or $response -eq "Y") {
            Write-Info "Resetando banco de dados..."
            docker-compose down
            try { docker volume rm aure_postgres_data } catch { }
            docker-compose up postgres -d
            Start-Sleep 10
            dotnet ef database update --project ".\src\Aure.Infrastructure" --startup-project ".\src\Aure.API"
            Write-Success "Banco de dados resetado!"
        }
        else {
            Write-Info "Operação cancelada."
        }
    }
    
    "adminer" {
        Write-Info "Abrindo Adminer no navegador..."
        Start-Process "http://localhost:8080"
    }
    
    "mailhog" {
        Write-Info "Abrindo MailHog no navegador..."
        Start-Process "http://localhost:8025"
    }
    
    { $_ -in @("help", "--help", "-h", "") } {
        Show-Help
    }
    
    default {
        Write-Error "Comando desconhecido: $Command"
        Write-Host "Use '.\docker.ps1 help' para ver os comandos disponíveis."
        exit 1
    }
}