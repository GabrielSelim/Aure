START TRANSACTION;

DROP INDEX "IX_contract_template_configs_company_id";

ALTER TABLE contract_template_configs ADD categoria character varying(50) NOT NULL DEFAULT '';

ALTER TABLE contract_template_configs ADD nome_config character varying(100) NOT NULL DEFAULT '';

CREATE INDEX "IX_contract_template_configs_company_id" ON contract_template_configs (company_id);

CREATE UNIQUE INDEX "IX_contract_template_configs_company_id_nome_config" ON contract_template_configs (company_id, nome_config);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124183809_PermitirMultiplasConfigsPorEmpresa', '8.0.11');

COMMIT;

