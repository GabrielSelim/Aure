-- Criar tabela contractdocuments
CREATE TABLE IF NOT EXISTS contractdocuments (
    id uuid NOT NULL,
    contract_id uuid NOT NULL,
    template_id uuid NULL,
    conteudo_html text NOT NULL,
    conteudo_pdf text NULL,
    conteudo_docx text NULL,
    versao_major integer NOT NULL DEFAULT 1,
    versao_minor integer NOT NULL DEFAULT 0,
    eh_versao_final boolean NOT NULL DEFAULT false,
    hash_documento character varying(256) NULL,
    dados_preenchidos jsonb NOT NULL,
    gerado_por_usuario_id uuid NULL,
    data_finalizacao timestamp with time zone NULL,
    observacoes character varying(1000) NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_contractdocuments PRIMARY KEY (id),
    CONSTRAINT fk_contractdocuments_contracts_contract_id FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
    CONSTRAINT fk_contractdocuments_contracttemplates_template_id FOREIGN KEY (template_id) REFERENCES contracttemplates(id) ON DELETE SET NULL,
    CONSTRAINT fk_contractdocuments_users_gerado_por_usuario_id FOREIGN KEY (gerado_por_usuario_id) REFERENCES users(id) ON DELETE SET NULL
);

-- Criar índices para contractdocuments
CREATE INDEX IF NOT EXISTS ix_contractdocuments_contract_id ON contractdocuments(contract_id);
CREATE INDEX IF NOT EXISTS ix_contractdocuments_template_id ON contractdocuments(template_id);
CREATE INDEX IF NOT EXISTS ix_contractdocuments_gerado_por_usuario_id ON contractdocuments(gerado_por_usuario_id);
CREATE INDEX IF NOT EXISTS ix_contractdocuments_eh_versao_final ON contractdocuments(eh_versao_final);
CREATE INDEX IF NOT EXISTS ix_contractdocuments_contract_id_eh_versao_final ON contractdocuments(contract_id, eh_versao_final);

-- Adicionar FK para contracttemplates
ALTER TABLE contracttemplates ADD CONSTRAINT fk_contracttemplates_companies_company_id FOREIGN KEY (company_id) REFERENCES companies(id) ON DELETE RESTRICT;

-- Criar índices para contracttemplates
CREATE INDEX IF NOT EXISTS ix_contracttemplates_company_id ON contracttemplates(company_id);
CREATE INDEX IF NOT EXISTS ix_contracttemplates_company_id_tipo_eh_padrao ON contracttemplates(company_id, tipo, eh_padrao);
CREATE INDEX IF NOT EXISTS ix_contracttemplates_ativo ON contracttemplates(ativo);

-- Atualizar EFMigrationsHistory
INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20251118151626_AdicionarSistemaTemplatesContratos', '8.0.0');
