#!/bin/bash

echo "====================================="
echo "Criar Template de Contrato via SQL"
echo "====================================="
echo ""

echo "[1/4] Buscando dados do usuario..."
USER_DATA=$(docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production -t -c "SELECT id, company_id FROM users WHERE email = 'gsystemster@gmail.com' LIMIT 1;")

USER_ID=$(echo $USER_DATA | awk '{print $1}')
COMPANY_ID=$(echo $USER_DATA | awk '{print $3}')

echo "Usuario ID: $USER_ID"
echo "Company ID: $COMPANY_ID"

echo ""
echo "[2/4] Gerando ID do template..."
TEMPLATE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
echo "Template ID: $TEMPLATE_ID"

echo ""
echo "[3/4] Lendo HTML do template..."
TEMPLATE_HTML=$(cat /root/Aure/src/Aure.Infrastructure/Templates/ContratoPrestacaoServicosGestao.html | sed "s/'/''/g")
echo "Template HTML carregado (${#TEMPLATE_HTML} caracteres)"

echo ""
echo "[4/4] Inserindo no banco de dados..."

VARIAVEIS='["NOME_EMPRESA_CONTRATANTE","CNPJ_CONTRATANTE","ENDERECO_CONTRATANTE","NUMERO_CONTRATANTE","BAIRRO_CONTRATANTE","CIDADE_CONTRATANTE","UF_CONTRATANTE","CEP_CONTRATANTE","ESTADO_REGISTRO_CONTRATANTE","NIRE_CONTRATANTE","NOME_REPRESENTANTE_CONTRATANTE","NACIONALIDADE_REPRESENTANTE","ESTADO_CIVIL_REPRESENTANTE","DATA_NASCIMENTO_REPRESENTANTE","PROFISSAO_REPRESENTANTE","CPF_REPRESENTANTE","RG_REPRESENTANTE","ORGAO_EXPEDIDOR_REPRESENTANTE","ENDERECO_RESIDENCIAL_REPRESENTANTE","NUMERO_RESIDENCIAL_REPRESENTANTE","BAIRRO_RESIDENCIAL_REPRESENTANTE","CIDADE_RESIDENCIAL_REPRESENTANTE","UF_RESIDENCIAL_REPRESENTANTE","CEP_RESIDENCIAL_REPRESENTANTE","RAZAO_SOCIAL_CONTRATADO","CNPJ_CONTRATADO","NOME_CONTRATADO","CPF_CONTRATADO","ENDERECO_CONTRATADO","NUMERO_CONTRATADO","BAIRRO_CONTRATADO","CIDADE_CONTRATADO","ESTADO_CONTRATADO","NACIONALIDADE_CONTRATADO","ESTADO_CIVIL_CONTRATADO","DATA_NASCIMENTO_CONTRATADO","PROFISSAO_CONTRATADO","ENDERECO_RESIDENCIAL_CONTRATADO","NUMERO_RESIDENCIAL_CONTRATADO","BAIRRO_RESIDENCIAL_CONTRATADO","CIDADE_RESIDENCIAL_CONTRATADO","ESTADO_RESIDENCIAL_CONTRATADO","CEP_RESIDENCIAL_CONTRATADO","DATA_PROPOSTA","PRAZO_VIGENCIA","PRAZO_VIGENCIA_EXTENSO","VALOR_MENSAL","VALOR_MENSAL_EXTENSO","VALOR_AJUDA_CUSTO","VALOR_TOTAL_MENSAL","DIA_VENCIMENTO_NF","DIA_VENCIMENTO_NF_EXTENSO","DIA_PAGAMENTO","CIDADE_FORO","ESTADO_FORO","CIDADE_ASSINATURA","DATA_ASSINATURA"]'

NOW=$(date -u +"%Y-%m-%d %H:%M:%S")

docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<EOF
INSERT INTO contracttemplates 
(id, company_id, created_by_user_id, nome, descricao, tipo, conteudo_html, variaveis_disponiveis, eh_padrao, eh_sistema, ativo, versao_major, versao_minor, created_at, updated_at)
VALUES 
('$TEMPLATE_ID', '$COMPANY_ID', '$USER_ID', 
'Contrato de Prestacao de Servicos de Gestao', 
'Template oficial protegido do sistema para contratos PJ. Baseado no modelo de prestacao de servicos de gestao e analise de negocios. Este template nao pode ser editado ou excluido.', 
0, 
'$TEMPLATE_HTML', 
'$VARIAVEIS'::jsonb, 
true, true, true, 1, 0, 
'$NOW', '$NOW');
EOF

if [ $? -eq 0 ]; then
    echo ""
    echo "====================================="
    echo "Template criado com sucesso!"
    echo "====================================="
    echo ""
    echo "Template ID: $TEMPLATE_ID"
    echo "Nome: Contrato de Prestacao de Servicos de Gestao"
    echo "Tipo: PrestacaoServicoPJ"
    echo "Padrao: true"
    echo "Sistema: true"
    echo "Ativo: true"
    echo "Variaveis: 56"
    echo ""
    echo "Verifique via API:"
    echo "curl https://aureapi.gabrielsanztech.com.br/api/ContractTemplates"
else
    echo ""
    echo "====================================="
    echo "Erro ao criar template!"
    echo "====================================="
    exit 1
fi
