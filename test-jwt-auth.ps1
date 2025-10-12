# Script para testar autenticação JWT na API Aure

Write-Host "🔐 Testando Autenticação JWT na API Aure" -ForegroundColor Cyan
Write-Host ("=" * 50)

$baseUrl = "http://localhost:5203"

# 1. Tentar acessar endpoint protegido sem token
Write-Host "`n1️⃣  Tentando acessar endpoint protegido SEM autenticação..."
try {
    $unauthorizedResponse = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method GET -TimeoutSec 5
    Write-Host "❌ ERRO: Endpoint deveria estar protegido mas retornou: $($unauthorizedResponse.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ Perfeito! Endpoint protegido retornou 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Erro inesperado: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# 2. Fazer login para obter token
Write-Host "`n2️⃣  Fazendo login para obter token JWT..."
$loginBody = @{
    email = "gabriel@teste.com"
    password = "MinhaSenh@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody -TimeoutSec 10
    $loginData = $loginResponse.Content | ConvertFrom-Json
    
    Write-Host "✅ Login realizado com sucesso!" -ForegroundColor Green
    Write-Host "   Token: $($loginData.accessToken.Substring(0, 50))..." -ForegroundColor Gray
    Write-Host "   Usuário: $($loginData.user.name) ($($loginData.user.email))" -ForegroundColor Gray
    Write-Host "   Expira em: $($loginData.expiresAt)" -ForegroundColor Gray
    
    $jwtToken = $loginData.accessToken
    
} catch {
    Write-Host "❌ Erro no login: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Detalhes: $errorContent" -ForegroundColor Yellow
    }
    exit 1
}

# 3. Tentar acessar endpoint protegido COM token
Write-Host "`n3️⃣  Tentando acessar endpoint protegido COM token JWT..."
try {
    $headers = @{
        'Authorization' = "Bearer $jwtToken"
        'Content-Type' = 'application/json'
    }
    
    $authorizedResponse = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method GET -Headers $headers -TimeoutSec 10
    $usersData = $authorizedResponse.Content | ConvertFrom-Json
    
    Write-Host "✅ Acesso realizado com sucesso! Status: $($authorizedResponse.StatusCode)" -ForegroundColor Green
    Write-Host "   Usuários encontrados: $($usersData.Count)" -ForegroundColor Gray
    
} catch {
    Write-Host "❌ Erro ao acessar endpoint protegido: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
    }
}

# 4. Testar endpoint público (criação de usuário)
Write-Host "`n4️⃣  Testando endpoint público (criação de usuário)..."
$newUserBody = @{
    name = "Teste Usuario"
    email = "teste@exemplo.com"
    password = "MinhaSenh@456"
    role = 1
} | ConvertTo-Json

try {
    $createResponse = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method POST -ContentType "application/json" -Body $newUserBody -TimeoutSec 10
    Write-Host "✅ Usuário criado com sucesso sem necessidade de autenticação!" -ForegroundColor Green
    $newUser = $createResponse.Content | ConvertFrom-Json
    Write-Host "   Novo usuário: $($newUser.name) ($($newUser.email))" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 409 -or $_.Exception.Message -like "*already exists*") {
        Write-Host "⚠️  Email já existe - isso é esperado se o usuário já foi criado antes" -ForegroundColor Yellow
    } else {
        Write-Host "❌ Erro ao criar usuário: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n🎉 Teste de autenticação JWT concluído!" -ForegroundColor Cyan
Write-Host ("=" * 50)