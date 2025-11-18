$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deploy Contract Templates System" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/6] Fazendo commit das alterações..." -ForegroundColor Yellow
git add .
git commit -m "feat: adicionar template de contrato de prestacao de servicos de gestao"
git push origin main

Write-Host ""
Write-Host "[2/6] Conectando no servidor e fazendo pull..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && git pull origin main"

Write-Host ""
Write-Host "[3/6] Parando containers..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && docker-compose down"

Write-Host ""
Write-Host "[4/6] Reconstruindo e iniciando containers..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && docker-compose up -d --build"

Write-Host ""
Write-Host "[5/6] Aguardando containers iniciarem (30s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host ""
Write-Host "[6/6] Aplicando migration no banco de dados..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && docker exec aure-api-aure-gabriel dotnet ef database update --no-build"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Deploy concluído com sucesso!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Verificar logs: docker logs -f aure-api-aure-gabriel" -ForegroundColor White
Write-Host "2. Testar health: curl https://aureapi.gabrielsanztech.com.br/health" -ForegroundColor White
Write-Host "3. Criar template via API POST /api/ContractTemplates" -ForegroundColor White
Write-Host ""
Write-Host "Template HTML disponível em:" -ForegroundColor Cyan
Write-Host "src/Aure.Infrastructure/Templates/ContratoPrestacaoServicosGestao.html" -ForegroundColor White
Write-Host ""
Write-Host "Variáveis disponíveis em:" -ForegroundColor Cyan
Write-Host "src/Aure.Infrastructure/Templates/VariaveisContratoPrestacaoServicos.json" -ForegroundColor White
