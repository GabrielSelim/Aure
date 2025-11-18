$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Inserir Template de Contrato" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$API_URL = "https://aureapi.gabrielsanztech.com.br"
$EMAIL = "gsystemster@gmail.com"
$PASSWORD = "Ga123456"

Write-Host "[1/4] Fazendo login..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = $EMAIL
        password = $PASSWORD
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$API_URL/api/Auth/entrar" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "Login bem-sucedido!" -ForegroundColor Green
} catch {
    Write-Host "Erro no login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[2/4] Lendo template HTML..." -ForegroundColor Yellow
$templatePath = "src\Aure.Infrastructure\Templates\ContratoPrestacaoServicosGestao.html"
if (-not (Test-Path $templatePath)) {
    Write-Host "Arquivo nao encontrado: $templatePath" -ForegroundColor Red
    exit 1
}
$templateHtml = Get-Content -Path $templatePath -Raw -Encoding UTF8
Write-Host "Template lido ($($templateHtml.Length) caracteres)" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] Montando dados do template..." -ForegroundColor Yellow
$templateData = @{
    nome = "Contrato de Prestacao de Servicos de Gestao - SISTEMA"
    descricao = "Template oficial protegido do sistema para contratos PJ. Baseado no modelo de prestacao de servicos de gestao e analise de negocios. Este template nao pode ser editado ou excluido."
    tipo = "PrestacaoServicoPJ"
    conteudoHtml = $templateHtml
    variaveisDisponiveis = @(
        "NOME_EMPRESA_CONTRATANTE","CNPJ_CONTRATANTE","ENDERECO_CONTRATANTE","NUMERO_CONTRATANTE",
        "BAIRRO_CONTRATANTE","CIDADE_CONTRATANTE","UF_CONTRATANTE","CEP_CONTRATANTE",
        "ESTADO_REGISTRO_CONTRATANTE","NIRE_CONTRATANTE","NOME_REPRESENTANTE_CONTRATANTE",
        "NACIONALIDADE_REPRESENTANTE","ESTADO_CIVIL_REPRESENTANTE","DATA_NASCIMENTO_REPRESENTANTE",
        "PROFISSAO_REPRESENTANTE","CPF_REPRESENTANTE","RG_REPRESENTANTE","ORGAO_EXPEDIDOR_REPRESENTANTE",
        "ENDERECO_RESIDENCIAL_REPRESENTANTE","NUMERO_RESIDENCIAL_REPRESENTANTE","BAIRRO_RESIDENCIAL_REPRESENTANTE",
        "CIDADE_RESIDENCIAL_REPRESENTANTE","UF_RESIDENCIAL_REPRESENTANTE","CEP_RESIDENCIAL_REPRESENTANTE",
        "RAZAO_SOCIAL_CONTRATADO","CNPJ_CONTRATADO","NOME_CONTRATADO","CPF_CONTRATADO",
        "ENDERECO_CONTRATADO","NUMERO_CONTRATADO","BAIRRO_CONTRATADO","CIDADE_CONTRATADO","ESTADO_CONTRATADO",
        "NACIONALIDADE_CONTRATADO","ESTADO_CIVIL_CONTRATADO","DATA_NASCIMENTO_CONTRATADO","PROFISSAO_CONTRATADO",
        "ENDERECO_RESIDENCIAL_CONTRATADO","NUMERO_RESIDENCIAL_CONTRATADO","BAIRRO_RESIDENCIAL_CONTRATADO",
        "CIDADE_RESIDENCIAL_CONTRATADO","ESTADO_RESIDENCIAL_CONTRATADO","CEP_RESIDENCIAL_CONTRATADO",
        "DATA_PROPOSTA","PRAZO_VIGENCIA","PRAZO_VIGENCIA_EXTENSO","VALOR_MENSAL","VALOR_MENSAL_EXTENSO",
        "VALOR_AJUDA_CUSTO","VALOR_TOTAL_MENSAL","DIA_VENCIMENTO_NF","DIA_VENCIMENTO_NF_EXTENSO",
        "DIA_PAGAMENTO","CIDADE_FORO","ESTADO_FORO","CIDADE_ASSINATURA","DATA_ASSINATURA"
    )
    definirComoPadrao = $true
}

$jsonBody = $templateData | ConvertTo-Json -Depth 5 -Compress

Write-Host ""
Write-Host "[4/4] Enviando para API..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json; charset=utf-8"
    }
    
    $response = Invoke-RestMethod -Uri "$API_URL/api/ContractTemplates" -Method Post -Body $jsonBody -Headers $headers -ContentType "application/json; charset=utf-8"
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "Template criado com sucesso!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "ID: $($response.id)" -ForegroundColor Cyan
    Write-Host "Nome: $($response.nome)" -ForegroundColor White
    Write-Host "Tipo: $($response.tipo)" -ForegroundColor White
    Write-Host "Padrao: $($response.ehPadrao)" -ForegroundColor $(if($response.ehPadrao){"Green"}else{"Yellow"})
    Write-Host "Sistema: $($response.ehSistema)" -ForegroundColor $(if($response.ehSistema){"Green"}else{"Yellow"})
    Write-Host "Ativo: $($response.ativo)" -ForegroundColor $(if($response.ativo){"Green"}else{"Red"})
    Write-Host "Variaveis: $($response.variaveisDisponiveis.Count)" -ForegroundColor White
    Write-Host ""
    Write-Host "Agora marque como template do sistema executando:" -ForegroundColor Cyan
    Write-Host ".\marcar-como-sistema.ps1 -TemplateId '$($response.id)'" -ForegroundColor Yellow
    
} catch {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "Erro ao criar template!" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Yellow
    
    if ($_.ErrorDetails.Message) {
        $errorObj = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($errorObj) {
            Write-Host "Detalhes: $($errorObj | ConvertTo-Json -Depth 3)" -ForegroundColor Yellow
        } else {
            Write-Host "Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
        }
    }
    
    exit 1
}
