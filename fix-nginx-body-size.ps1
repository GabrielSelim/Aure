$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Aumentar Limite do Nginx" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/3] Adicionando client_max_body_size ao Nginx..." -ForegroundColor Yellow

$nginxConfig = @"
server {
    listen 80;
    server_name aureapi.gabrielsanztech.com.br;
    return 301 https://\`$host\`$request_uri;
}

server {
    listen 443 ssl http2;
    server_name aureapi.gabrielsanztech.com.br;

    ssl_certificate /etc/letsencrypt/live/aureapi.gabrielsanztech.com.br/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/aureapi.gabrielsanztech.com.br/privkey.pem;

    client_max_body_size 10M;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \`$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \`$host;
        proxy_cache_bypass \`$http_upgrade;
        proxy_set_header X-Forwarded-For \`$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \`$scheme;
    }
}
"@

$tempFile = [System.IO.Path]::GetTempFileName()
$nginxConfig | Out-File -FilePath $tempFile -Encoding UTF8 -NoNewline

Write-Host "Enviando configuracao atualizada..." -ForegroundColor Yellow
Get-Content $tempFile | ssh root@5.189.174.61 "cat > /etc/nginx/sites-available/aureapi.gabrielsanztech.com.br"

Remove-Item $tempFile

Write-Host ""
Write-Host "[2/3] Testando configuracao do Nginx..." -ForegroundColor Yellow
$testResult = ssh root@5.189.174.61 "nginx -t 2>&1"
Write-Host $testResult -ForegroundColor White

if ($testResult -match "syntax is ok" -and $testResult -match "test is successful") {
    Write-Host "Configuracao valida!" -ForegroundColor Green
} else {
    Write-Host "Erro na configuracao!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[3/3] Reiniciando Nginx..." -ForegroundColor Yellow
ssh root@5.189.174.61 "systemctl reload nginx"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Nginx atualizado com sucesso!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Agora execute: .\import-template.ps1" -ForegroundColor Cyan
