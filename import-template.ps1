$ErrorActionPreference = "Stop"

$API_URL = "https://aureapi.gabrielsanztech.com.br"
$EMAIL = "gsystemster@gmail.com"
$PASSWORD = "Ga123456"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Importar Template de Contrato" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/3] Fazendo login..." -ForegroundColor Yellow
$loginBody = @{
    email = $EMAIL
    password = $PASSWORD
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$API_URL/api/Auth/entrar" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token

Write-Host "Login bem-sucedido! Token obtido." -ForegroundColor Green
Write-Host ""

Write-Host "[2/3] Lendo template HTML..." -ForegroundColor Yellow
$templateHtml = Get-Content -Path "src/Aure.Infrastructure/Templates/ContratoPrestacaoServicosGestao.html" -Raw

Write-Host "[3/3] Criando template no sistema..." -ForegroundColor Yellow

$templateBody = @{
    nome = "Contrato de Prestação de Serviços de Gestão"
    descricao = "Template padrão para contratos PJ de prestação de serviços de gestão e análise de negócios. Baseado no modelo Saul."
    tipo = "PrestacaoServicoPJ"
    conteudoHtml = $templateHtml
    variaveisDisponiveis = @(
        "NOME_EMPRESA_CONTRATANTE",
        "CNPJ_CONTRATANTE",
        "ENDERECO_CONTRATANTE",
        "NUMERO_CONTRATANTE",
        "BAIRRO_CONTRATANTE",
        "CIDADE_CONTRATANTE",
        "UF_CONTRATANTE",
        "CEP_CONTRATANTE",
        "ESTADO_REGISTRO_CONTRATANTE",
        "NIRE_CONTRATANTE",
        "NOME_REPRESENTANTE_CONTRATANTE",
        "NACIONALIDADE_REPRESENTANTE",
        "ESTADO_CIVIL_REPRESENTANTE",
        "DATA_NASCIMENTO_REPRESENTANTE",
        "PROFISSAO_REPRESENTANTE",
        "CPF_REPRESENTANTE",
        "RG_REPRESENTANTE",
        "ORGAO_EXPEDIDOR_REPRESENTANTE",
        "ENDERECO_RESIDENCIAL_REPRESENTANTE",
        "NUMERO_RESIDENCIAL_REPRESENTANTE",
        "BAIRRO_RESIDENCIAL_REPRESENTANTE",
        "CIDADE_RESIDENCIAL_REPRESENTANTE",
        "UF_RESIDENCIAL_REPRESENTANTE",
        "CEP_RESIDENCIAL_REPRESENTANTE",
        "RAZAO_SOCIAL_CONTRATADO",
        "CNPJ_CONTRATADO",
        "NOME_CONTRATADO",
        "CPF_CONTRATADO",
        "ENDERECO_CONTRATADO",
        "NUMERO_CONTRATADO",
        "BAIRRO_CONTRATADO",
        "CIDADE_CONTRATADO",
        "ESTADO_CONTRATADO",
        "NACIONALIDADE_CONTRATADO",
        "ESTADO_CIVIL_CONTRATADO",
        "DATA_NASCIMENTO_CONTRATADO",
        "PROFISSAO_CONTRATADO",
        "ENDERECO_RESIDENCIAL_CONTRATADO",
        "NUMERO_RESIDENCIAL_CONTRATADO",
        "BAIRRO_RESIDENCIAL_CONTRATADO",
        "CIDADE_RESIDENCIAL_CONTRATADO",
        "ESTADO_RESIDENCIAL_CONTRATADO",
        "CEP_RESIDENCIAL_CONTRATADO",
        "DATA_PROPOSTA",
        "PRAZO_VIGENCIA",
        "PRAZO_VIGENCIA_EXTENSO",
        "VALOR_MENSAL",
        "VALOR_MENSAL_EXTENSO",
        "VALOR_AJUDA_CUSTO",
        "VALOR_TOTAL_MENSAL",
        "DIA_VENCIMENTO_NF",
        "DIA_VENCIMENTO_NF_EXTENSO",
        "DIA_PAGAMENTO",
        "CIDADE_FORO",
        "ESTADO_FORO",
        "CIDADE_ASSINATURA",
        "DATA_ASSINATURA"
    )
    definirComoPadrao = $true
} | ConvertTo-Json -Depth 10

try {
    $headers = @{
        Authorization = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    $templateResponse = Invoke-RestMethod -Uri "$API_URL/api/ContractTemplates" -Method Post -Body $templateBody -Headers $headers
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "Template criado com sucesso!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Template ID: $($templateResponse.id)" -ForegroundColor Cyan
    Write-Host "Nome: $($templateResponse.nome)" -ForegroundColor White
    Write-Host "Tipo: $($templateResponse.tipo)" -ForegroundColor White
    Write-Host "Padrão: $($templateResponse.ehPadrao)" -ForegroundColor White
    Write-Host "Ativo: $($templateResponse.ativo)" -ForegroundColor White
    Write-Host "Variáveis: $($templateResponse.variaveisDisponiveis.Count)" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "Erro ao criar template!" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Yellow
    
    if ($_.ErrorDetails.Message) {
        Write-Host "Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
    }
    
    exit 1
}
