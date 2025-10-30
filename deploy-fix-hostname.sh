#!/bin/bash
# 🚀 Deploy da Correção de Hostname - Servidor Contabo
# IP: 5.189.174.61
# Domínio: aureapi.gabrielsanztech.com.br

set -e

SERVER_IP="5.189.174.61"
SERVER_USER="root"
REMOTE_DIR="/root/Aure"

echo "🚀 Aplicando correção de hostname no servidor $SERVER_IP..."

# 1. Parar containers existentes
echo "⏹️  Parando containers existentes..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && docker-compose down"

# 2. Fazer backup das configurações atuais
echo "💾 Fazendo backup das configurações..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && cp src/Aure.API/appsettings.Production.json src/Aure.API/appsettings.Production.json.backup"
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && cp docker-compose.yml docker-compose.yml.backup"

# 3. Aplicar correção no appsettings.Production.json
echo "🔧 Corrigindo AllowedHosts no appsettings..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && sed -i 's/\"AllowedHosts\": \".*\"/\"AllowedHosts\": \"*\"/' src/Aure.API/appsettings.Production.json"

# 4. Aplicar correção no docker-compose.yml (adicionar variável de ambiente)
echo "🔧 Adicionando variável de ambiente no docker-compose..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && sed -i '/- TZ=America\/Sao_Paulo/a\      - ASPNETCORE_HOSTFILTERING__ALLOWEDHOSTS__0=*' docker-compose.yml"

# 5. Verificar se as correções foram aplicadas
echo "✅ Verificando correções aplicadas..."
echo "📋 AllowedHosts:"
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && grep 'AllowedHosts' src/Aure.API/appsettings.Production.json"
echo "📋 Variável de ambiente:"
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && grep 'ASPNETCORE_HOSTFILTERING' docker-compose.yml"

# 6. Rebuild da API com cache limpo
echo "🔨 Rebuilding API com cache limpo..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && docker-compose build --no-cache api"

# 7. Subir containers
echo "▶️  Subindo containers..."
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && docker-compose up -d"

# 8. Aguardar containers ficarem prontos
echo "⏳ Aguardando containers ficarem prontos..."
sleep 30

# 9. Verificar status dos containers
echo "📊 Status dos containers:"
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && docker-compose ps"

# 10. Testar API diretamente
echo "🧪 Testando API diretamente (IP local)..."
ssh $SERVER_USER@$SERVER_IP "curl -s http://127.0.0.1:5002/health || echo 'ERRO: API não respondeu'"

# 11. Testar API via domínio
echo "🧪 Testando API via domínio..."
ssh $SERVER_USER@$SERVER_IP "curl -s -k https://aureapi.gabrielsanztech.com.br/health || echo 'AVISO: Ainda com problema via domínio'"

# 12. Verificar logs da API se houver erro
echo "📋 Últimos logs da API:"
ssh $SERVER_USER@$SERVER_IP "cd $REMOTE_DIR && docker-compose logs --tail=20 api"

echo ""
echo "✅ Deploy da correção concluído!"
echo "🌐 Teste manual: https://aureapi.gabrielsanztech.com.br/health"
echo "📊 Swagger: https://aureapi.gabrielsanztech.com.br/swagger"
echo ""
echo "🔍 Se ainda houver problemas, execute:"
echo "   ssh root@$SERVER_IP 'cd $REMOTE_DIR && docker-compose logs api'"