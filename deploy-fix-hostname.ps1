# 🚀 Deploy da Correção de Hostname - Servidor Contabo
# IP: 5.189.174.61
# Domínio: aureapi.gabrielsanztech.com.br

$SERVER_IP = "5.189.174.61"
$SERVER_USER = "root"
$REMOTE_DIR = "/root/Aure"

Write-Host "🚀 Aplicando correção de hostname no servidor $SERVER_IP..." -ForegroundColor Green

# 1. Parar containers existentes
Write-Host "⏹️  Parando containers existentes..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; docker-compose down"

# 2. Fazer backup das configurações atuais
Write-Host "💾 Fazendo backup das configurações..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; cp src/Aure.API/appsettings.Production.json src/Aure.API/appsettings.Production.json.backup"
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; cp docker-compose.yml docker-compose.yml.backup"

# 3. Aplicar correção no appsettings.Production.json
Write-Host "🔧 Corrigindo AllowedHosts no appsettings..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; sed -i 's/\`"AllowedHosts\`": \`".*\`"/\`"AllowedHosts\`": \`"*\`"/' src/Aure.API/appsettings.Production.json"

# 4. Aplicar correção no docker-compose.yml
Write-Host "🔧 Adicionando variável de ambiente no docker-compose..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; sed -i '/- TZ=America\/Sao_Paulo/a\      - ASPNETCORE_HOSTFILTERING__ALLOWEDHOSTS__0=*' docker-compose.yml"

# 5. Verificar se as correções foram aplicadas
Write-Host "✅ Verificando correções aplicadas..." -ForegroundColor Green
Write-Host "📋 AllowedHosts:" -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; grep 'AllowedHosts' src/Aure.API/appsettings.Production.json"
Write-Host "📋 Variável de ambiente:" -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; grep 'ASPNETCORE_HOSTFILTERING' docker-compose.yml"

# 6. Rebuild da API com cache limpo
Write-Host "🔨 Rebuilding API com cache limpo..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; docker-compose build --no-cache api"

# 7. Subir containers
Write-Host "▶️  Subindo containers..." -ForegroundColor Yellow
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; docker-compose up -d"

# 8. Aguardar containers ficarem prontos
Write-Host "⏳ Aguardando containers ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# 9. Verificar status dos containers
Write-Host "📊 Status dos containers:" -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; docker-compose ps"

# 10. Testar API diretamente
Write-Host "🧪 Testando API diretamente (IP local)..." -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "curl -s http://127.0.0.1:5002/health || echo 'ERRO: API não respondeu'"

# 11. Testar API via domínio
Write-Host "🧪 Testando API via domínio..." -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "curl -s -k https://aureapi.gabrielsanztech.com.br/health || echo 'AVISO: Ainda com problema via domínio'"

# 12. Verificar logs da API se houver erro
Write-Host "📋 Últimos logs da API:" -ForegroundColor Cyan
ssh "${SERVER_USER}@${SERVER_IP}" "cd ${REMOTE_DIR}; docker-compose logs --tail=20 api"

Write-Host ""
Write-Host "✅ Deploy da correção concluído!" -ForegroundColor Green
Write-Host "🌐 Teste manual: https://aureapi.gabrielsanztech.com.br/health" -ForegroundColor Green
Write-Host "📊 Swagger: https://aureapi.gabrielsanztech.com.br/swagger" -ForegroundColor Green
Write-Host ""
Write-Host "🔍 Se ainda houver problemas, execute:" -ForegroundColor Yellow
Write-Host "   ssh root@$SERVER_IP" -ForegroundColor Yellow