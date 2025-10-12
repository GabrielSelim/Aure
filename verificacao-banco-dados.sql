-- 🗃️ QUERIES DE VERIFICAÇÃO - FLUXO PJ COMPLETO
-- Sistema Aure - Relacionamentos entre Empresas
-- Data: 12/10/2025

-- ==============================================
-- 📊 VERIFICAÇÕES NO BANCO DE DADOS
-- ==============================================

-- 🔍 1. VERIFICAR EMPRESAS CADASTRADAS
SELECT 
    id,
    name,
    cnpj,
    type,
    business_model,
    created_at,
    CASE 
        WHEN business_model = 1 THEN 'Standard'
        WHEN business_model = 2 THEN 'MainCompany' 
        WHEN business_model = 3 THEN 'ContractedPJ'
        WHEN business_model = 4 THEN 'Freelancer'
    END as business_model_name
FROM companies 
WHERE is_deleted = false
ORDER BY created_at DESC;

-- 🔍 2. VERIFICAR USUÁRIOS CADASTRADOS  
SELECT 
    u.id,
    u.name,
    u.email,
    u.role,
    u.company_id,
    c.name as company_name,
    c.cnpj as company_cnpj,
    u.created_at,
    CASE 
        WHEN u.role = 1 THEN 'Admin'
        WHEN u.role = 2 THEN 'Company'
        WHEN u.role = 3 THEN 'Provider'
    END as role_name
FROM users u
LEFT JOIN companies c ON u.company_id = c.id
WHERE u.is_deleted = false
ORDER BY u.created_at DESC;

-- 🔍 3. VERIFICAR CONVITES (HISTÓRICO)
SELECT 
    id,
    inviter_name,
    invitee_name,
    invitee_email,
    role,
    invite_type,
    company_name,
    cnpj,
    business_model,
    is_accepted,
    expires_at,
    created_at,
    CASE 
        WHEN invite_type = 0 THEN 'Employee'
        WHEN invite_type = 1 THEN 'ContractedPJ'
        WHEN invite_type = 2 THEN 'ExternalUser'
    END as invite_type_name
FROM userinvites
WHERE is_deleted = false
ORDER BY created_at DESC;

-- 🔍 4. VERIFICAR RELACIONAMENTOS ENTRE EMPRESAS
SELECT 
    cr.id as relationship_id,
    cr.type,
    cr.status,
    cr.start_date,
    cr.end_date,
    cr.notes,
    c_client.name as client_company,
    c_client.cnpj as client_cnpj,
    c_provider.name as provider_company,
    c_provider.cnpj as provider_cnpj,
    cr.created_at,
    CASE 
        WHEN cr.type = 1 THEN 'ContractedPJ'
        WHEN cr.type = 2 THEN 'Partnership'
        WHEN cr.type = 3 THEN 'Supplier'
        WHEN cr.type = 4 THEN 'Client'
    END as relationship_type_name,
    CASE 
        WHEN cr.status = 1 THEN 'Active'
        WHEN cr.status = 2 THEN 'Inactive'
        WHEN cr.status = 3 THEN 'Terminated'
        WHEN cr.status = 4 THEN 'Suspended'
    END as status_name
FROM companyrelationships cr
JOIN companies c_client ON cr.client_company_id = c_client.id
JOIN companies c_provider ON cr.provider_company_id = c_provider.id
WHERE cr.is_deleted = false
ORDER BY cr.created_at DESC;

-- ==============================================
-- 📋 CONSULTAS ESPECÍFICAS PARA O TESTE
-- ==============================================

-- 🎯 5. BUSCAR DADOS DO TESTE ESPECÍFICO

-- Buscar empresa contratante (TechCorp/Empresa Teste)
SELECT 
    id as empresa_id,
    name,
    cnpj,
    business_model
FROM companies 
WHERE name ILIKE '%teste%' 
   OR name ILIKE '%techcorp%'
   AND is_deleted = false;

-- Buscar empresa PJ (João Silva Consultoria)  
SELECT 
    id as empresa_pj_id,
    name,
    cnpj,
    business_model
FROM companies 
WHERE name ILIKE '%joão%' 
   OR name ILIKE '%silva%'
   OR cnpj = '12345678000190'
   AND is_deleted = false;

-- Buscar usuário PJ
SELECT 
    u.id as usuario_pj_id,
    u.name,
    u.email,
    u.company_id,
    c.name as empresa_pj
FROM users u
JOIN companies c ON u.company_id = c.id
WHERE u.email = 'joao.silva@consultoria.com'
   AND u.is_deleted = false;

-- 🔗 6. VERIFICAR VINCULAÇÃO ESPECÍFICA DO TESTE
SELECT 
    'RELACIONAMENTO ENCONTRADO!' as status,
    cr.id as relationship_id,
    c_client.name as empresa_contratante,
    c_provider.name as empresa_pj,
    cr.notes,
    cr.created_at
FROM companyrelationships cr
JOIN companies c_client ON cr.client_company_id = c_client.id  
JOIN companies c_provider ON cr.provider_company_id = c_provider.id
WHERE c_provider.cnpj = '12345678000190'
   AND cr.type = 1  -- ContractedPJ
   AND cr.status = 1 -- Active
   AND cr.is_deleted = false;

-- ==============================================
-- 🧹 QUERIES DE LIMPEZA (USAR SE NECESSÁRIO)
-- ==============================================

-- ⚠️ CUIDADO: Estas queries removem dados do teste
-- Execute apenas se quiser limpar e repetir o teste

/*
-- Remover relacionamento de teste
UPDATE companyrelationships 
SET is_deleted = true 
WHERE id IN (
    SELECT cr.id 
    FROM companyrelationships cr
    JOIN companies c ON cr.provider_company_id = c.id
    WHERE c.cnpj = '12345678000190'
);

-- Remover usuário PJ de teste  
UPDATE users 
SET is_deleted = true 
WHERE email = 'joao.silva@consultoria.com';

-- Remover empresa PJ de teste
UPDATE companies 
SET is_deleted = true 
WHERE cnpj = '12345678000190';

-- Remover convites de teste
UPDATE userinvites 
SET is_deleted = true 
WHERE invitee_email = 'joao.silva@consultoria.com';
*/

-- ==============================================
-- 📈 QUERIES DE ANÁLISE E RELATÓRIOS
-- ==============================================

-- 📊 7. RELATÓRIO GERAL DO SISTEMA
SELECT 
    'RESUMO GERAL' as categoria,
    (SELECT COUNT(*) FROM companies WHERE is_deleted = false) as total_empresas,
    (SELECT COUNT(*) FROM users WHERE is_deleted = false) as total_usuarios,
    (SELECT COUNT(*) FROM companyrelationships WHERE is_deleted = false) as total_relacionamentos,
    (SELECT COUNT(*) FROM userinvites WHERE is_deleted = false AND is_accepted = true) as convites_aceitos,
    (SELECT COUNT(*) FROM userinvites WHERE is_deleted = false AND is_accepted = false) as convites_pendentes;

-- 📊 8. ANÁLISE DE RELACIONAMENTOS POR TIPO
SELECT 
    CASE 
        WHEN type = 1 THEN 'ContractedPJ'
        WHEN type = 2 THEN 'Partnership'
        WHEN type = 3 THEN 'Supplier'
        WHEN type = 4 THEN 'Client'
    END as tipo_relacionamento,
    COUNT(*) as quantidade,
    COUNT(CASE WHEN status = 1 THEN 1 END) as ativos,
    COUNT(CASE WHEN status != 1 THEN 1 END) as inativos
FROM companyrelationships 
WHERE is_deleted = false
GROUP BY type
ORDER BY quantidade DESC;

-- 📊 9. TOP EMPRESAS POR RELACIONAMENTOS
SELECT 
    c.name as empresa,
    c.cnpj,
    c.business_model,
    COUNT(DISTINCT CASE WHEN cr.client_company_id = c.id THEN cr.id END) as como_cliente,
    COUNT(DISTINCT CASE WHEN cr.provider_company_id = c.id THEN cr.id END) as como_fornecedor,
    COUNT(DISTINCT cr.id) as total_relacionamentos
FROM companies c
LEFT JOIN companyrelationships cr ON (c.id = cr.client_company_id OR c.id = cr.provider_company_id)
WHERE c.is_deleted = false
GROUP BY c.id, c.name, c.cnpj, c.business_model
HAVING COUNT(DISTINCT cr.id) > 0
ORDER BY total_relacionamentos DESC;

-- ==============================================
-- ✅ QUERY DE VALIDAÇÃO FINAL DO TESTE
-- ==============================================

-- 🎯 10. VALIDAÇÃO COMPLETA DO FLUXO PJ
SELECT 
    '✅ TESTE VALIDADO!' as resultado,
    COUNT(DISTINCT c_client.id) as empresas_contratantes,
    COUNT(DISTINCT c_provider.id) as empresas_pj,
    COUNT(DISTINCT u_pj.id) as usuarios_pj,
    COUNT(DISTINCT cr.id) as relacionamentos_ativos,
    string_agg(DISTINCT c_provider.name, ', ') as empresas_pj_nomes
FROM companyrelationships cr
JOIN companies c_client ON cr.client_company_id = c_client.id
JOIN companies c_provider ON cr.provider_company_id = c_provider.id  
JOIN users u_pj ON u_pj.company_id = c_provider.id
WHERE cr.type = 1  -- ContractedPJ
   AND cr.status = 1 -- Active
   AND cr.is_deleted = false
   AND c_provider.business_model = 3; -- ContractedPJ

-- Se esta query retornar dados, o teste foi bem-sucedido! 🎉