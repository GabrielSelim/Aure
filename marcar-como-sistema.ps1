param(
    [Parameter(Mandatory=$true)]
    [string]$TemplateId
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Marcar Template como SISTEMA" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Conectando ao banco de dados..." -ForegroundColor Yellow
$sql = "UPDATE contracttemplates SET eh_sistema = true WHERE id = '$TemplateId'; SELECT id, nome, eh_sistema, eh_padrao FROM contracttemplates WHERE id = '$TemplateId';"

$result = $sql | ssh root@5.189.174.61 "docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "✓ Template marcado como SISTEMA!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host $result
Write-Host ""
Write-Host "Agora o template está protegido e não pode ser editado ou excluído!" -ForegroundColor Cyan
