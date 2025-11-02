-- Criar tabela notification_preferences com snake_case

CREATE TABLE IF NOT EXISTS notification_preferences (
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
    
    CONSTRAINT fk_notification_preferences_users_user_id 
        FOREIGN KEY (user_id) 
        REFERENCES users(id) 
        ON DELETE CASCADE
);

-- Criar índices
CREATE UNIQUE INDEX IF NOT EXISTS ix_notification_preferences_user_id 
    ON notification_preferences(user_id) 
    WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS ix_notification_preferences_is_deleted 
    ON notification_preferences(is_deleted);

-- Criar preferências padrão para usuários existentes
INSERT INTO notification_preferences (id, user_id, created_at, updated_at, is_deleted)
SELECT gen_random_uuid(), id, NOW(), NOW(), false
FROM users
WHERE is_deleted = false
  AND NOT EXISTS (
    SELECT 1 FROM notification_preferences np WHERE np.user_id = users.id
  );

-- Verificar resultado
SELECT COUNT(*) as total_preferences FROM notification_preferences;
