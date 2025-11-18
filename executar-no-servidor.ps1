$ErrorActionPreference = "Stop"

Write-Host "====================================="
Write-Host "Upload e Execucao do Script no Servidor"
Write-Host "====================================="
Write-Host ""

Write-Host "[1/3] Fazendo upload do script..." -ForegroundColor Yellow
scp criar-template-servidor.sh root@5.189.174.61:/root/criar-template-servidor.sh

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao fazer upload" -ForegroundColor Red
    exit 1
}

Write-Host "Upload concluido!" -ForegroundColor Green

Write-Host ""
Write-Host "[2/3] Dando permissao de execucao..." -ForegroundColor Yellow
ssh root@5.189.174.61 "chmod +x /root/criar-template-servidor.sh"

Write-Host ""
Write-Host "[3/3] Executando script no servidor..." -ForegroundColor Yellow
ssh root@5.189.174.61 "/root/criar-template-servidor.sh"
