$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DEPLOY - Endpoints Empresa/Funcionários" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/4] Conectando ao servidor..." -ForegroundColor Yellow

ssh root@5.189.174.61 @'
cd /root/Aure

echo ""
echo "========================================" 
echo "  [2/4] Atualizando código..."
echo "========================================"
git pull origin main

echo ""
echo "========================================" 
echo "  [3/4] Reconstruindo containers..."
echo "========================================"
docker-compose down
docker-compose up -d --build

echo ""
echo "========================================" 
echo "  [4/4] Verificando status..."
echo "========================================"
sleep 10
docker ps | grep aure

echo ""
echo "========================================" 
echo "  ✅ DEPLOY CONCLUÍDO"
echo "========================================"
echo ""
echo "Testar endpoints:"
echo "GET https://aureapi.gabrielsanztech.com.br/api/Companies/empresa-pai"
echo "GET https://aureapi.gabrielsanztech.com.br/api/Users/funcionarios"
echo ""
'@

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ DEPLOY CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Teste o endpoint de empresa:" -ForegroundColor White
Write-Host "   GET https://aureapi.gabrielsanztech.com.br/api/Companies/empresa-pai" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Teste o endpoint de funcionários:" -ForegroundColor White
Write-Host "   GET https://aureapi.gabrielsanztech.com.br/api/Users/funcionarios" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Verifique se os novos campos aparecem:" -ForegroundColor White
Write-Host "   - Empresa: rua, numero, bairro, cidade, estado, cep" -ForegroundColor Gray
Write-Host "   - Funcionários: cpf, rg, endereço completo, dados PJ" -ForegroundColor Gray
Write-Host ""
