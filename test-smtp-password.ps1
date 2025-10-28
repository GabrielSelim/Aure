# Script para testar a senha do Gmail SMTP
# Verifica se há caracteres especiais que podem causar problemas

$email = "aurecontroll@gmail.com"
$password = "1G]2i>30Ax*"

Write-Host "=== Teste de Configuração SMTP Gmail ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Email: $email" -ForegroundColor Green
Write-Host "Senha (ofuscada): $($password[0])***$($password[-1])" -ForegroundColor Yellow
Write-Host "Comprimento da senha: $($password.Length) caracteres" -ForegroundColor Yellow
Write-Host ""

# Verificar caracteres especiais
$specialChars = @('\', '>', '*', ']', '[', '<', '&', '"', "'", '$', '`')
$foundSpecialChars = @()

foreach ($char in $specialChars) {
    if ($password.Contains($char)) {
        $foundSpecialChars += $char
    }
}

if ($foundSpecialChars.Count -gt 0) {
    Write-Host "⚠️  ATENÇÃO: Senha contém caracteres especiais:" -ForegroundColor Yellow
    Write-Host "   $($foundSpecialChars -join ', ')" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Recomendações:" -ForegroundColor Cyan
    Write-Host "   1. Certifique-se de que é uma App Password do Gmail (não senha normal)" -ForegroundColor White
    Write-Host "   2. Se continuar falhando, gere uma nova App Password sem caracteres especiais" -ForegroundColor White
    Write-Host "   3. Verifique se '2-Step Verification' está ativado na conta Gmail" -ForegroundColor White
} else {
    Write-Host "✅ Senha não contém caracteres especiais problemáticos" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Links Úteis ===" -ForegroundColor Cyan
Write-Host "📧 Criar App Password: https://myaccount.google.com/apppasswords" -ForegroundColor White
Write-Host "🔐 Verificação em 2 etapas: https://myaccount.google.com/security" -ForegroundColor White
Write-Host ""
