START TRANSACTION;

ALTER TABLE users ADD password_reset_token text;

ALTER TABLE users ADD password_reset_token_expiry timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251030193120_AdicionarTokenRecuperacaoSenha', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE companies ADD address_city text;

ALTER TABLE companies ADD address_complement text;

ALTER TABLE companies ADD address_country text;

ALTER TABLE companies ADD address_neighborhood text;

ALTER TABLE companies ADD address_number text;

ALTER TABLE companies ADD address_state text;

ALTER TABLE companies ADD address_street text;

ALTER TABLE companies ADD address_zip_code text;

ALTER TABLE companies ADD phone_landline text;

ALTER TABLE companies ADD phone_mobile text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251030193745_AdicionarContatoEnderecoEmpresa', '8.0.11');

COMMIT;

