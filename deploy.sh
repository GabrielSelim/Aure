#!/bin/bash

# 🚀 Script de Deploy - API Aure no Servidor Contabo
# Autor: Sistema Aure
# Versão: 1.0

set -e  # Para se houver erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para log com cores
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] ⚠️  $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ❌ $1${NC}"
}

# Variáveis
DOMAIN="aureapi.gabrielsanztech.com.br"
APP_DIR="/opt/aure"
BACKUP_DIR="/opt/aure-backup"
COMPOSE_FILE="docker-compose.prod.yml"

log "🚀 Iniciando deploy da API Aure no Contabo..."

# Verificar se está rodando como root ou com sudo
if [ "$EUID" -ne 0 ]; then
    error "Este script precisa ser executado como root ou com sudo"
    exit 1
fi

# Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    error "Docker não está instalado! Execute: apt install -y docker.io docker-compose"
    exit 1
fi

# Verificar se está no diretório correto
if [ ! -f "Dockerfile.prod" ]; then
    error "Arquivo Dockerfile.prod não encontrado! Execute este script na raiz do projeto."
    exit 1
fi

# Verificar se .env.production existe
if [ ! -f ".env.production" ]; then
    error "Arquivo .env.production não encontrado!"
    warn "Copie o arquivo .env.production e configure as variáveis:"
    warn "cp .env.production .env"
    warn "nano .env"
    exit 1
fi

# Criar backup se aplicação já existir
if [ -d "$APP_DIR" ] && [ -f "$APP_DIR/docker-compose.prod.yml" ]; then
    log "📦 Criando backup da versão anterior..."
    BACKUP_NAME="$BACKUP_DIR-$(date +%Y%m%d_%H%M%S)"
    cp -r "$APP_DIR" "$BACKUP_NAME"
    log "✅ Backup criado em: $BACKUP_NAME"
fi

# Parar serviços existentes
log "⏸️  Parando serviços existentes..."
if [ -f "$COMPOSE_FILE" ]; then
    docker-compose -f "$COMPOSE_FILE" down --remove-orphans || true
fi

# Limpar containers órfãos
log "🧹 Limpando containers órfãos..."
docker system prune -f

# Verificar se git está instalado e atualizar código
if command -v git &> /dev/null; then
    log "📥 Atualizando código do repositório..."
    git pull origin main || warn "Falha ao atualizar código via git"
fi

# Copiar .env.production para .env
log "📋 Configurando variáveis de ambiente..."
cp .env.production .env

# Verificar se variáveis críticas estão configuradas
source .env
if [ -z "$POSTGRES_PASSWORD" ] || [ "$POSTGRES_PASSWORD" = "sua_senha_postgres_muito_segura_aqui_123" ]; then
    error "Configure a variável POSTGRES_PASSWORD no arquivo .env!"
    exit 1
fi

if [ -z "$JWT_SECRET_KEY" ] || [ "$JWT_SECRET_KEY" = "sua_chave_jwt_de_256_bits_muito_segura_aqui_gere_uma_aleatoria" ]; then
    error "Configure a variável JWT_SECRET_KEY no arquivo .env!"
    exit 1
fi

# Criar diretórios necessários
log "📁 Criando diretórios necessários..."
mkdir -p logs certificates uploads

# Build da aplicação
log "🔨 Fazendo build da aplicação (isso pode demorar alguns minutos)..."
docker-compose -f "$COMPOSE_FILE" build --no-cache

# Subir serviços
log "🚀 Subindo serviços em produção..."
docker-compose -f "$COMPOSE_FILE" up -d

# Aguardar serviços ficarem prontos
log "⏳ Aguardando serviços ficarem prontos..."
sleep 30

# Verificar se PostgreSQL está pronto
log "🗄️  Verificando PostgreSQL..."
for i in {1..30}; do
    if docker-compose -f "$COMPOSE_FILE" exec -T postgres pg_isready -U aure_user -d aure_production &>/dev/null; then
        log "✅ PostgreSQL está pronto!"
        break
    fi
    if [ $i -eq 30 ]; then
        error "PostgreSQL não ficou pronto após 5 minutos"
        exit 1
    fi
    sleep 10
done

# Verificar se Redis está pronto
log "🔄 Verificando Redis..."
for i in {1..10}; do
    if docker-compose -f "$COMPOSE_FILE" exec -T redis redis-cli ping &>/dev/null; then
        log "✅ Redis está pronto!"
        break
    fi
    if [ $i -eq 10 ]; then
        error "Redis não ficou pronto"
        exit 1
    fi
    sleep 5
done

# Verificar saúde da API
log "🔍 Verificando saúde da API..."
for i in {1..20}; do
    if curl -f -s http://localhost:5000/health > /dev/null 2>&1; then
        log "✅ API está respondendo!"
        break
    fi
    if [ $i -eq 20 ]; then
        error "API não está respondendo após 10 minutos!"
        log "📋 Verificando logs da API..."
        docker-compose -f "$COMPOSE_FILE" logs --tail=50 aure-api
        exit 1
    fi
    sleep 30
done

# Verificar se Swagger está acessível
log "📊 Verificando Swagger..."
if curl -f -s http://localhost:5000/swagger > /dev/null 2>&1; then
    log "✅ Swagger está acessível!"
else
    warn "⚠️  Swagger pode não estar acessível externamente"
fi

# Mostrar status dos containers
log "📋 Status dos containers:"
docker-compose -f "$COMPOSE_FILE" ps

# Mostrar logs recentes
log "📝 Logs recentes da API:"
docker-compose -f "$COMPOSE_FILE" logs --tail=20 aure-api

# Informações finais
log "🎉 Deploy concluído com sucesso!"
echo
log "📍 Informações do Deploy:"
echo -e "   🌐 Domínio: ${BLUE}https://$DOMAIN${NC}"
echo -e "   📊 Swagger: ${BLUE}https://$DOMAIN/swagger${NC}"
echo -e "   🗄️  Database: PostgreSQL na porta 5432"
echo -e "   🔄 Cache: Redis na porta 6379"
echo -e "   📁 Logs: /opt/aure/logs/"
echo
log "🔧 Comandos úteis:"
echo -e "   Ver logs: ${BLUE}docker-compose -f $COMPOSE_FILE logs -f aure-api${NC}"
echo -e "   Reiniciar: ${BLUE}docker-compose -f $COMPOSE_FILE restart${NC}"
echo -e "   Parar: ${BLUE}docker-compose -f $COMPOSE_FILE down${NC}"
echo -e "   Status: ${BLUE}docker-compose -f $COMPOSE_FILE ps${NC}"
echo
warn "🔒 Não esqueça de configurar SSL/HTTPS com Nginx!"
warn "📧 Teste o envio de emails após o deploy!"

log "✅ Deploy finalizado!"