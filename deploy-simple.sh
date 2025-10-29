#!/bin/bash

# 🚀 Deploy Simples - API Aure (Seguro para outros containers)
# Verifica conflitos antes de subir

echo "🚀 Deploy da API Aure..."

# Verificar se script de verificação existe
if [ -f "./check-ports.sh" ]; then
    echo "🔍 Verificando portas em uso..."
    chmod +x check-ports.sh
    ./check-ports.sh
    echo ""
    read -p "Continuar com o deploy? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Deploy cancelado. Ajuste as portas no .env se necessário."
        exit 1
    fi
fi

# Parar apenas containers Aure (não afeta outros)
echo "⏸️  Parando containers Aure existentes..."
docker-compose down 2>/dev/null || true

# Criar .env se não existir
if [ ! -f .env ]; then
    echo "📋 Criando arquivo .env..."
    cp .env.example .env
    echo "⚠️  IMPORTANTE: Edite o arquivo .env com suas configurações!"
    echo "   - Altere a senha do banco (POSTGRES_PASSWORD)"
    echo "   - Altere a chave JWT (JWT_SECRET_KEY)"
    echo "   - Configure seu domínio (API_BASE_URL)"
    echo ""
    echo "Depois rode novamente: ./deploy-simple.sh"
    exit 0
fi

# Criar diretórios necessários
echo "📁 Criando diretórios..."
mkdir -p logs certificates uploads

# Build e start
echo "🔨 Building e iniciando containers..."
docker-compose up -d --build

# Aguardar API ficar pronta
echo "⏳ Aguardando API ficar pronta..."
API_PORT=$(grep "API_PORT=" .env 2>/dev/null | cut -d'=' -f2 || echo "5000")
for i in {1..30}; do
    if curl -f -s http://localhost:${API_PORT}/health > /dev/null 2>&1; then
        echo "✅ API está funcionando!"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ API não ficou pronta após 5 minutos"
        echo "📋 Verificando logs..."
        docker-compose logs --tail=20 api
        exit 1
    fi
    sleep 10
done

echo ""
echo "🎉 Deploy concluído!"
echo "📍 API: http://localhost:${API_PORT}"
echo "📊 Swagger: http://localhost:${API_PORT}/swagger"
echo "🗄️  Adminer (dev): docker-compose --profile dev up -d adminer"
echo ""
echo "🔧 Comandos úteis:"
echo "   Logs: docker-compose logs -f api"
echo "   Parar apenas Aure: docker-compose down"
echo "   Reiniciar: docker-compose restart"
echo "   Status: docker-compose ps"
echo ""
echo "🛡️  Containers isolados com prefixo: aure-*-${COMPOSE_PROJECT_NAME:-aure}"