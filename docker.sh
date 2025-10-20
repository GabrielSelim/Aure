#!/bin/bash
# 🚀 Script de Gerenciamento Docker - Sistema Aure
# Data: 20/10/2025

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para exibir ajuda
show_help() {
    echo -e "${BLUE}🚀 Sistema Aure - Gerenciamento Docker${NC}"
    echo ""
    echo "Uso: ./docker.sh [COMANDO] [OPÇÕES]"
    echo ""
    echo "Comandos disponíveis:"
    echo ""
    echo -e "${GREEN}📦 DESENVOLVIMENTO:${NC}"
    echo "  dev-up        - Subir PostgreSQL + Redis + Adminer para desenvolvimento"
    echo "  dev-db        - Subir apenas PostgreSQL para desenvolvimento"
    echo "  dev-down      - Parar serviços de desenvolvimento"
    echo "  dev-logs      - Ver logs dos serviços de desenvolvimento"
    echo ""
    echo -e "${GREEN}🏗️ AMBIENTE COMPLETO:${NC}"
    echo "  up            - Subir todos os serviços (API + Banco + Redis)"
    echo "  down          - Parar todos os serviços"
    echo "  restart       - Reiniciar todos os serviços"
    echo "  logs          - Ver logs de todos os serviços"
    echo "  logs-api      - Ver logs apenas da API"
    echo ""
    echo -e "${GREEN}🚀 PRODUÇÃO:${NC}"
    echo "  prod-up       - Subir ambiente de produção"
    echo "  prod-down     - Parar ambiente de produção"
    echo "  prod-logs     - Ver logs de produção"
    echo ""
    echo -e "${GREEN}🛠️ MANUTENÇÃO:${NC}"
    echo "  build         - Rebuild das imagens Docker"
    echo "  clean         - Limpar containers e volumes (CUIDADO!)"
    echo "  status        - Status dos containers"
    echo "  db-migrate    - Aplicar migrações Entity Framework"
    echo "  db-reset      - Reset completo do banco (CUIDADO!)"
    echo ""
    echo -e "${GREEN}📊 MONITORAMENTO:${NC}"
    echo "  adminer       - Abrir Adminer (admin banco)"
    echo "  mailhog       - Abrir MailHog (teste emails)"
    echo ""
    echo "Exemplos:"
    echo "  ./docker.sh dev-up        # Para desenvolvimento"
    echo "  ./docker.sh up            # Ambiente completo"
    echo "  ./docker.sh logs-api      # Ver logs da API"
    echo ""
}

# Função para logs coloridos
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Verificar se Docker está rodando
check_docker() {
    if ! docker info >/dev/null 2>&1; then
        log_error "Docker não está rodando. Inicie o Docker primeiro."
        exit 1
    fi
}

# Comando principal
case "$1" in
    "dev-up")
        check_docker
        log_info "Subindo serviços de desenvolvimento..."
        docker-compose -f docker-compose.dev.yml up -d
        log_success "Serviços de desenvolvimento iniciados!"
        log_info "Adminer: http://localhost:8080"
        log_info "MailHog: http://localhost:8025"
        ;;
    
    "dev-db")
        check_docker
        log_info "Subindo apenas PostgreSQL para desenvolvimento..."
        docker-compose -f docker-compose.dev.yml up postgres-dev -d
        log_success "PostgreSQL de desenvolvimento iniciado!"
        log_info "Conexão: localhost:5432 - DB: aure_db - User: aure_user - Pass: aure_password"
        ;;
    
    "dev-down")
        log_info "Parando serviços de desenvolvimento..."
        docker-compose -f docker-compose.dev.yml down
        log_success "Serviços de desenvolvimento parados!"
        ;;
    
    "dev-logs")
        docker-compose -f docker-compose.dev.yml logs -f
        ;;
    
    "up")
        check_docker
        log_info "Subindo todos os serviços..."
        docker-compose up -d
        log_success "Todos os serviços iniciados!"
        log_info "API: http://localhost:5203"
        log_info "Swagger: http://localhost:5203/swagger"
        log_info "Adminer: http://localhost:8080"
        ;;
    
    "down")
        log_info "Parando todos os serviços..."
        docker-compose down
        log_success "Todos os serviços parados!"
        ;;
    
    "restart")
        log_info "Reiniciando todos os serviços..."
        docker-compose down
        docker-compose up -d
        log_success "Todos os serviços reiniciados!"
        ;;
    
    "logs")
        docker-compose logs -f
        ;;
    
    "logs-api")
        docker-compose logs -f api
        ;;
    
    "prod-up")
        check_docker
        log_warning "Iniciando ambiente de produção..."
        if [ ! -d "secrets" ]; then
            log_error "Diretório 'secrets' não encontrado. Configure os secrets primeiro!"
            exit 1
        fi
        docker-compose -f docker-compose.prod.yml up -d
        log_success "Ambiente de produção iniciado!"
        ;;
    
    "prod-down")
        log_info "Parando ambiente de produção..."
        docker-compose -f docker-compose.prod.yml down
        log_success "Ambiente de produção parado!"
        ;;
    
    "prod-logs")
        docker-compose -f docker-compose.prod.yml logs -f
        ;;
    
    "build")
        check_docker
        log_info "Fazendo rebuild das imagens..."
        docker-compose build --no-cache
        log_success "Rebuild concluído!"
        ;;
    
    "clean")
        check_docker
        log_warning "ATENÇÃO: Isso irá remover TODOS os containers e volumes!"
        read -p "Tem certeza? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            log_info "Limpando containers e volumes..."
            docker-compose down -v
            docker system prune -a -f
            log_success "Limpeza concluída!"
        else
            log_info "Operação cancelada."
        fi
        ;;
    
    "status")
        check_docker
        log_info "Status dos containers:"
        docker-compose ps
        ;;
    
    "db-migrate")
        log_info "Aplicando migrações Entity Framework..."
        dotnet ef database update --project "./src/Aure.Infrastructure" --startup-project "./src/Aure.API"
        log_success "Migrações aplicadas!"
        ;;
    
    "db-reset")
        log_warning "ATENÇÃO: Isso irá RESETAR completamente o banco de dados!"
        read -p "Tem certeza? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            log_info "Resetando banco de dados..."
            docker-compose down
            docker volume rm aure_postgres_data 2>/dev/null || true
            docker-compose up postgres -d
            sleep 10
            dotnet ef database update --project "./src/Aure.Infrastructure" --startup-project "./src/Aure.API"
            log_success "Banco de dados resetado!"
        else
            log_info "Operação cancelada."
        fi
        ;;
    
    "adminer")
        log_info "Abrindo Adminer no navegador..."
        if command -v start >/dev/null 2>&1; then
            start http://localhost:8080
        elif command -v open >/dev/null 2>&1; then
            open http://localhost:8080
        elif command -v xdg-open >/dev/null 2>&1; then
            xdg-open http://localhost:8080
        else
            log_info "Acesse manualmente: http://localhost:8080"
        fi
        ;;
    
    "mailhog")
        log_info "Abrindo MailHog no navegador..."
        if command -v start >/dev/null 2>&1; then
            start http://localhost:8025
        elif command -v open >/dev/null 2>&1; then
            open http://localhost:8025
        elif command -v xdg-open >/dev/null 2>&1; then
            xdg-open http://localhost:8025
        else
            log_info "Acesse manualmente: http://localhost:8025"
        fi
        ;;
    
    "help"|"--help"|"-h"|"")
        show_help
        ;;
    
    *)
        log_error "Comando desconhecido: $1"
        echo "Use './docker.sh help' para ver os comandos disponíveis."
        exit 1
        ;;
esac