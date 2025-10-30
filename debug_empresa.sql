-- Script para debugar problema do endpoint /api/UserProfile/empresa

-- 1. Verificar estrutura da tabela companies (novos campos)
SELECT column_name 
FROM information_schema.columns 
WHERE table_name = 'companies' 
  AND column_name LIKE 'phone%' OR column_name LIKE 'address%'
ORDER BY column_name;

-- 2. Ver dados da empresa
SELECT id, name, cnpj, phone_mobile, phone_landline, address_city, address_state
FROM companies
LIMIT 5;

-- 3. Ver usuários e seus company_id
SELECT id, name, email, role, company_id
FROM users
WHERE is_deleted = false
ORDER BY created_at DESC
LIMIT 5;

-- 4. Verificar se existe relacionamento user->company
SELECT u.id as user_id, u.name as user_name, u.email, u.role, 
       c.id as company_id, c.name as company_name
FROM users u
LEFT JOIN companies c ON u.company_id = c.id
WHERE u.is_deleted = false
ORDER BY u.created_at DESC
LIMIT 5;
