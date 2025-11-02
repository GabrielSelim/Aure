-- Script para corrigir tabela NotificationPreferences para snake_case

-- 1. Renomear tabela
ALTER TABLE "NotificationPreferences" RENAME TO notification_preferences;

-- 2. Renomear colunas
ALTER TABLE notification_preferences RENAME COLUMN "Id" TO id;
ALTER TABLE notification_preferences RENAME COLUMN "UserId" TO user_id;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailNovoContrato" TO receber_email_novo_contrato;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailContratoAssinado" TO receber_email_contrato_assinado;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailContratoVencendo" TO receber_email_contrato_vencendo;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailPagamentoProcessado" TO receber_email_pagamento_processado;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailPagamentoRecebido" TO receber_email_pagamento_recebido;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailNovoFuncionario" TO receber_email_novo_funcionario;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailAlertasFinanceiros" TO receber_email_alertas_financeiros;
ALTER TABLE notification_preferences RENAME COLUMN "ReceberEmailAtualizacoesSistema" TO receber_email_atualizacoes_sistema;
ALTER TABLE notification_preferences RENAME COLUMN "CreatedAt" TO created_at;
ALTER TABLE notification_preferences RENAME COLUMN "UpdatedAt" TO updated_at;
ALTER TABLE notification_preferences RENAME COLUMN "IsDeleted" TO is_deleted;

-- 3. Renomear constraints e índices
ALTER INDEX "IX_NotificationPreferences_UserId" RENAME TO ix_notification_preferences_user_id;
ALTER INDEX "IX_NotificationPreferences_IsDeleted" RENAME TO ix_notification_preferences_is_deleted;
ALTER INDEX "PK_NotificationPreferences" RENAME TO pk_notification_preferences;

-- 4. Renomear foreign key
ALTER TABLE notification_preferences 
    DROP CONSTRAINT "FK_NotificationPreferences_Users_UserId";

ALTER TABLE notification_preferences 
    ADD CONSTRAINT fk_notification_preferences_users_user_id 
    FOREIGN KEY (user_id) 
    REFERENCES users(id) 
    ON DELETE CASCADE;

-- Verificar estrutura final
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'notification_preferences'
ORDER BY ordinal_position;
