# 🚀 Script de Deploy em Produção - Aure API
# Data: 25/11/2025
# Alterações: Centralizar NIRE/InscricaoEstadual, remover campos duplicados, threshold 0.8

Write-Host "🚀 Iniciando deploy em produção..." -ForegroundColor Cyan
Write-Host ""

# 1. Conectar ao servidor
Write-Host "📡 Conectando ao servidor de produção..." -ForegroundColor Yellow
ssh root@5.189.174.61

# ============================================
# COMANDOS A EXECUTAR NO SERVIDOR (após SSH)
# ============================================

# 2. Navegar para o diretório do projeto
cd /root/Aure

# 3. Fazer pull das alteraçõe
git pull origin main

# 4. Parar os containers
docker-compose down

# 5. Rebuild e restart dos containers
docker-compose up -d --build

# 6. Verificar logs da API
docker logs -f aure-api-aure-gabriel --tail=100

# (Pressione Ctrl+C para sair dos logs)

# 7. Verificar health check
curl https://aureapi.gabrielsanztech.com.br/health

# 8. Testar endpoint GET /api/UserProfile/empresa (verificar se campos duplicados foram removidos)
# curl -X GET https://aureapi.gabrielsanztech.com.br/api/UserProfile/empresa \
#   -H "Authorization: Bearer SEU_TOKEN_AQUI"

# 9. Testar endpoint PUT /api/Companies/empresa-pai (verificar se NIRE/InscricaoEstadual são aceitos)
# curl -X PUT https://aureapi.gabrielsanztech.com.br/api/Companies/empresa-pai \
#   -H "Authorization: Bearer SEU_TOKEN_AQUI" \
#   -H "Content-Type: application/json" \
#   -d '{"razaoSocial":"Teste LTDA","cnpj":"12345678000199","nire":"35123456789","inscricaoEstadual":"123456789012"}'

Write-Host ""
Write-Host "✅ Deploy concluído!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Verificações necessárias:" -ForegroundColor Yellow
Write-Host "   - Health check retornou 200" -ForegroundColor White
Write-Host "   - Logs da API sem erros críticos" -ForegroundColor White
Write-Host "   - GET /api/UserProfile/empresa sem campos de endereço duplicados" -ForegroundColor White
Write-Host "   - PUT /api/Companies/empresa-pai aceita NIRE e InscricaoEstadual" -ForegroundColor White
Write-Host "   - Threshold de divergência ajustado para 0.8 (80%)" -ForegroundColor White
