$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Deploy Final - Sistema de Templates" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Conectando no servidor..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && git pull origin main"

Write-Host ""
Write-Host "[2/5] Reconstruindo containers..." -ForegroundColor Yellow
ssh root@5.189.174.61 "cd /root/Aure && docker-compose up -d --build"

Write-Host ""
Write-Host "[3/5] Aguardando API iniciar (30s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host ""
Write-Host "[4/5] Aplicando migration do campo eh_sistema..." -ForegroundColor Yellow
$sql1 = "ALTER TABLE contracttemplates ADD COLUMN IF NOT EXISTS eh_sistema boolean NOT NULL DEFAULT false;"
$sql2 = "INSERT INTO \`"__EFMigrationsHistory\`" (\`"MigrationId\`", \`"ProductVersion\`") VALUES ('20251118182357_AdicionarCampoEhSistemaTemplate', '8.0.0') ON CONFLICT DO NOTHING;"

$sql1 | ssh root@5.189.174.61 "docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production"
$sql2 | ssh root@5.189.174.61 "docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production"

Write-Host ""
Write-Host "[5/5] Verificando API..." -ForegroundColor Yellow
$healthCheck = Invoke-WebRequest -Uri "https://aureapi.gabrielsanztech.com.br/health" -UseBasicParsing
if ($healthCheck.StatusCode -eq 200) {
    Write-Host "API esta funcionando!" -ForegroundColor Green
} else {
    Write-Host "Erro ao verificar API" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Deploy concluido!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Proximo passo: executar .\inserir-template-simples.ps1" -ForegroundColor Cyan
