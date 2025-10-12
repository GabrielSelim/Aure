# Teste 1: CNPJ válido com nome correto (deve funcionar)
$body1 = @{
    name = "João Silva"
    email = "joao@empresa.com"
    password = "MinhaSenh@123"
    companyName = "EMPRESA TESTE LTDA"
    companyCnpj = "11.222.333/0001-81"
    companyType = "Ltda"
} | ConvertTo-Json

Write-Host "=== TESTE 1: CNPJ válido com nome correto ===" -ForegroundColor Green
try {
    $response1 = Invoke-RestMethod -Uri "http://localhost:5203/api/auth/register-company-admin" -Method POST -Body $body1 -ContentType "application/json"
    Write-Host "✅ SUCESSO: " -ForegroundColor Green -NoNewline
    Write-Host ($response1 | ConvertTo-Json -Depth 3)
} catch {
    Write-Host "❌ ERRO: " -ForegroundColor Red -NoNewline
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseContent = $reader.ReadToEnd()
        Write-Host "Detalhes: $responseContent" -ForegroundColor Yellow
    }
}

Write-Host "`n" 

# Teste 2: CNPJ inválido (deve falhar)
$body2 = @{
    name = "Maria Santos"
    email = "maria@empresa.com"
    password = "MinhaSenh@123"
    companyName = "EMPRESA TESTE 2 LTDA"
    companyCnpj = "11.111.111/0001-11"
    companyType = "Ltda"
} | ConvertTo-Json

Write-Host "=== TESTE 2: CNPJ inválido (deve falhar) ===" -ForegroundColor Yellow
try {
    $response2 = Invoke-RestMethod -Uri "http://localhost:5203/api/auth/register-company-admin" -Method POST -Body $body2 -ContentType "application/json"
    Write-Host "✅ SUCESSO: " -ForegroundColor Green -NoNewline
    Write-Host ($response2 | ConvertTo-Json -Depth 3)
} catch {
    Write-Host "❌ ERRO (esperado): " -ForegroundColor Yellow -NoNewline
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseContent = $reader.ReadToEnd()
        Write-Host "Detalhes: $responseContent" -ForegroundColor Yellow
    }
}