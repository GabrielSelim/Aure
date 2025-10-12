# Script para testar a API Aure

# Verificar se a API está rodando
Write-Host "Testando conexão com a API..."
try {
    $healthCheck = Invoke-WebRequest -Uri "http://localhost:5203/health" -Method GET -TimeoutSec 5
    Write-Host "✅ API está online - Status: $($healthCheck.StatusCode)"
} catch {
    Write-Host "❌ API não está respondendo: $($_.Exception.Message)"
    exit 1
}

# Criar um usuário
Write-Host "`nCriando usuário..."
$createUserBody = @{
    name = "Gabriel Silva"
    email = "gabriel@teste.com"
    password = "MinhaSenh@123"
    role = 0
} | ConvertTo-Json

try {
    $createResponse = Invoke-WebRequest -Uri "http://localhost:5203/api/users" -Method POST -ContentType "application/json" -Body $createUserBody -TimeoutSec 10
    Write-Host "✅ Usuário criado com sucesso - Status: $($createResponse.StatusCode)"
    Write-Host "Response: $($createResponse.Content)"
} catch {
    Write-Host "❌ Erro ao criar usuário: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Detalhes do erro: $errorContent"
    }
}

# Fazer login
Write-Host "`nFazendo login..."
$loginBody = @{
    email = "gabriel@teste.com"
    password = "MinhaSenh@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5203/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody -TimeoutSec 10
    Write-Host "✅ Login realizado com sucesso - Status: $($loginResponse.StatusCode)"
    Write-Host "Response: $($loginResponse.Content)"
} catch {
    Write-Host "❌ Erro no login: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Detalhes do erro: $errorContent"
    }
}