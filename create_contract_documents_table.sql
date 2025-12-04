-- Criar tabela contract_documents
CREATE TABLE IF NOT EXISTS contract_documents (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id UUID NOT NULL,
    template_id UUID,
    conteudo_html TEXT NOT NULL,
    conteudo_pdf TEXT,
    conteudo_docx TEXT,
    versao_major INTEGER NOT NULL DEFAULT 1,
    versao_minor INTEGER NOT NULL DEFAULT 0,
    data_geracao TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    gerado_por_usuario_id UUID,
    dados_preenchidos JSONB NOT NULL DEFAULT '{}'::jsonb,
    eh_versao_final BOOLEAN NOT NULL DEFAULT FALSE,
    hash_documento VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_contract_documents_contracts 
        FOREIGN KEY (contract_id) 
        REFERENCES contracts(id) 
        ON DELETE RESTRICT,
    
    CONSTRAINT fk_contract_documents_templates 
        FOREIGN KEY (template_id) 
        REFERENCES contract_templates(id) 
        ON DELETE SET NULL,
    
    CONSTRAINT fk_contract_documents_users 
        FOREIGN KEY (gerado_por_usuario_id) 
        REFERENCES users(id) 
        ON DELETE SET NULL
);

-- Criar índices
CREATE INDEX IF NOT EXISTS ix_contract_documents_contract_id 
    ON contract_documents(contract_id);

CREATE INDEX IF NOT EXISTS ix_contract_documents_template_id 
    ON contract_documents(template_id);

CREATE INDEX IF NOT EXISTS ix_contract_documents_eh_versao_final 
    ON contract_documents(eh_versao_final);

CREATE INDEX IF NOT EXISTS ix_contract_documents_contract_id_eh_versao_final 
    ON contract_documents(contract_id, eh_versao_final);

-- Comentários
COMMENT ON TABLE contract_documents IS 'Armazena os documentos HTML/PDF/DOCX gerados para contratos';
COMMENT ON COLUMN contract_documents.data_geracao IS 'Data e hora em que o documento foi gerado';
COMMENT ON COLUMN contract_documents.eh_versao_final IS 'Indica se este é o documento final assinado';
COMMENT ON COLUMN contract_documents.dados_preenchidos IS 'JSON com os dados que foram preenchidos no template';
