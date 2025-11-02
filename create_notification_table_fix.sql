-- Criar tabela notificationpreferences exatamente como EF Core espera

CREATE TABLE IF NOT EXISTS notificationpreferences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    receber_email_novo_contrato BOOLEAN NOT NULL DEFAULT true,
    receber_email_contrato_assinado BOOLEAN NOT NULL DEFAULT true,
    receber_email_contrato_vencendo BOOLEAN NOT NULL DEFAULT true,
    receber_email_pagamento_processado BOOLEAN NOT NULL DEFAULT true,
    receber_email_pagamento_recebido BOOLEAN NOT NULL DEFAULT true,
    receber_email_novo_funcionario BOOLEAN NOT NULL DEFAULT true,
    receber_email_alertas_financeiros BOOLEAN NOT NULL DEFAULT true,
    receber_email_atualizacoes_sistema BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    
    CONSTRAINT fk_notificationpreferences_users_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES users(id) 
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_notificationpreferences_user_id 
    ON notificationpreferences(user_id) 
    WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS ix_notificationpreferences_is_deleted 
    ON notificationpreferences(is_deleted);

-- Criar preferências padrão para usuários existentes
INSERT INTO notificationpreferences (id, user_id, created_at, updated_at, is_deleted)
SELECT gen_random_uuid(), id, NOW(), NOW(), false
FROM users
WHERE is_deleted = false
  AND NOT EXISTS (
    SELECT 1 FROM notificationpreferences np WHERE np.user_id = users.id
  );

-- Verificar resultado
SELECT COUNT(*) as total FROM notificationpreferences;
