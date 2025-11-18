-- Migration: Adicionar campo eh_sistema
ALTER TABLE contracttemplates ADD COLUMN IF NOT EXISTS eh_sistema boolean NOT NULL DEFAULT false;

-- Atualizar Migration History
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251118182356_AdicionarCampoEhSistemaTemplate', '8.0.0')
ON CONFLICT DO NOTHING;

-- Verificar estrutura
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'contracttemplates'
ORDER BY ordinal_position;
