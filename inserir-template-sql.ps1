$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Inserir Template via SQL" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/3] Buscando dados do usuario..." -ForegroundColor Yellow

$getUserQuery = "SELECT id, company_id FROM users WHERE email = 'gsystemster@gmail.com' LIMIT 1;"
$userResult = ssh root@5.189.174.61 "docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production -t -c \`"$getUserQuery\`""

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao buscar usuario" -ForegroundColor Red
    exit 1
}

$userParts = $userResult.Trim() -split '\|'
$userId = $userParts[0].Trim()
$companyId = $userParts[1].Trim()

Write-Host "Usuario ID: $userId" -ForegroundColor Green
Write-Host "Company ID: $companyId" -ForegroundColor Green

Write-Host ""
Write-Host "[2/3] Lendo template HTML..." -ForegroundColor Yellow
$templateHtml = Get-Content -Path "src/Aure.Infrastructure/Templates/ContratoPrestacaoServicosGestao.html" -Raw

$templateHtml = $templateHtml -replace "'", "''"

Write-Host "Template lido ($($templateHtml.Length) caracteres)" -ForegroundColor Green

Write-Host ""
Write-Host "[3/3] Inserindo no banco de dados..." -ForegroundColor Yellow

$templateId = [Guid]::NewGuid().ToString()
$now = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")

$variaveis = @(
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

$variaveisJson = ($variaveis | ForEach-Object { "`"$_`"" }) -join ","
$variaveisJson = "[$variaveisJson]"

$insertQuery = @"
INSERT INTO contracttemplates 
(id, company_id, created_by_user_id, nome, descricao, tipo, conteudo_html, variaveis_disponiveis, eh_padrao, eh_sistema, ativo, versao_major, versao_minor, created_at, updated_at)
VALUES 
('$templateId', '$companyId', '$userId', 'Contrato de Prestacao de Servicos de Gestao', 'Template oficial protegido do sistema para contratos PJ. Baseado no modelo de prestacao de servicos de gestao e analise de negocios. Este template nao pode ser editado ou excluido.', 0, '$templateHtml', '$variaveisJson'::jsonb, true, true, true, 1, 0, '$now', '$now');
"@

$tempFile = [System.IO.Path]::GetTempFileName()
$insertQuery | Out-File -FilePath $tempFile -Encoding UTF8 -NoNewline

Get-Content $tempFile | ssh root@5.189.174.61 "cat > /tmp/insert_template.sql"
$result = ssh root@5.189.174.61 "docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production -f /tmp/insert_template.sql"

Remove-Item $tempFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao inserir template" -ForegroundColor Red
    Write-Host $result -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Template inserido com sucesso!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Template ID: $templateId" -ForegroundColor Cyan
Write-Host "Nome: Contrato de Prestacao de Servicos de Gestao" -ForegroundColor White
Write-Host "Tipo: PrestacaoServicoPJ" -ForegroundColor White
Write-Host "Padrao: True" -ForegroundColor Green
Write-Host "Sistema: True" -ForegroundColor Green
Write-Host "Ativo: True" -ForegroundColor Green
Write-Host "Variaveis: 56" -ForegroundColor White
Write-Host ""
Write-Host "Verifique via API:" -ForegroundColor Cyan
Write-Host "GET https://aureapi.gabrielsanztech.com.br/api/ContractTemplates" -ForegroundColor Yellow
