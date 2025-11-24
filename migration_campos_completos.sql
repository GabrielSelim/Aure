CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE companies (
        id uuid NOT NULL DEFAULT (uuid_generate_v4()),
        name character varying(200) NOT NULL,
        cnpj character varying(18) NOT NULL,
        type integer NOT NULL,
        kyc_status integer NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_companies" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL DEFAULT (uuid_generate_v4()),
        name character varying(100) NOT NULL,
        email character varying(255) NOT NULL,
        password_hash character varying(255) NOT NULL,
        role integer NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE contracts (
        id uuid NOT NULL DEFAULT (uuid_generate_v4()),
        client_id uuid NOT NULL,
        provider_id uuid NOT NULL,
        title character varying(255) NOT NULL,
        value_total numeric(18,2) NOT NULL,
        ipfs_cid character varying(100),
        sha256_hash character varying(255) NOT NULL,
        status integer NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_contracts" PRIMARY KEY (id),
        CONSTRAINT "FK_contracts_companies_client_id" FOREIGN KEY (client_id) REFERENCES companies (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_contracts_companies_provider_id" FOREIGN KEY (provider_id) REFERENCES companies (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE kycrecords (
        id uuid NOT NULL,
        company_id uuid NOT NULL,
        document_type integer NOT NULL,
        document_hash text NOT NULL,
        verified_at timestamp with time zone,
        status integer NOT NULL,
        provider_ref text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_kycrecords" PRIMARY KEY (id),
        CONSTRAINT "FK_kycrecords_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE auditlogs (
        id uuid NOT NULL,
        entity_name text NOT NULL,
        entity_id uuid NOT NULL,
        action integer NOT NULL,
        performed_by uuid NOT NULL,
        ip_address text NOT NULL,
        timestamp timestamp with time zone NOT NULL,
        hash_chain text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_auditlogs" PRIMARY KEY (id),
        CONSTRAINT "FK_auditlogs_users_performed_by" FOREIGN KEY (performed_by) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE sessions (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        jwt_hash text NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_sessions" PRIMARY KEY (id),
        CONSTRAINT "FK_sessions_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE payments (
        id uuid NOT NULL DEFAULT (uuid_generate_v4()),
        contract_id uuid NOT NULL,
        amount numeric(18,2) NOT NULL,
        method integer NOT NULL,
        status integer NOT NULL,
        payment_date timestamp with time zone,
        company_id uuid,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_payments" PRIMARY KEY (id),
        CONSTRAINT "FK_payments_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id),
        CONSTRAINT "FK_payments_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE signatures (
        id uuid NOT NULL DEFAULT (uuid_generate_v4()),
        contract_id uuid NOT NULL,
        user_id uuid NOT NULL,
        signed_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        signature_hash character varying(255) NOT NULL,
        method integer NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_signatures" PRIMARY KEY (id),
        CONSTRAINT "FK_signatures_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_signatures_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE splitrules (
        id uuid NOT NULL,
        contract_id uuid NOT NULL,
        beneficiary_ref text NOT NULL,
        percentage numeric NOT NULL,
        fixed_fee numeric,
        priority integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_splitrules" PRIMARY KEY (id),
        CONSTRAINT "FK_splitrules_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE tokenizedassets (
        id uuid NOT NULL,
        contract_id uuid NOT NULL,
        token_address text NOT NULL,
        chain_id integer NOT NULL,
        tx_hash text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_tokenizedassets" PRIMARY KEY (id),
        CONSTRAINT "FK_tokenizedassets_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE invoices (
        id uuid NOT NULL,
        contract_id uuid NOT NULL,
        payment_id uuid,
        invoice_number text NOT NULL,
        series text NOT NULL,
        access_key text NOT NULL,
        issue_date timestamp with time zone NOT NULL,
        due_date timestamp with time zone,
        total_amount numeric NOT NULL,
        tax_amount numeric NOT NULL,
        status integer NOT NULL,
        xml_content text NOT NULL,
        pdf_url text,
        cancellation_reason text,
        sefaz_protocol text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_invoices" PRIMARY KEY (id),
        CONSTRAINT "FK_invoices_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE CASCADE,
        CONSTRAINT "FK_invoices_payments_payment_id" FOREIGN KEY (payment_id) REFERENCES payments (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE ledgerentries (
        id uuid NOT NULL,
        payment_id uuid NOT NULL,
        contract_id uuid NOT NULL,
        debit numeric NOT NULL,
        credit numeric NOT NULL,
        currency text NOT NULL,
        timestamp timestamp with time zone NOT NULL,
        note text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_ledgerentries" PRIMARY KEY (id),
        CONSTRAINT "FK_ledgerentries_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE CASCADE,
        CONSTRAINT "FK_ledgerentries_payments_payment_id" FOREIGN KEY (payment_id) REFERENCES payments (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE notifications (
        id uuid NOT NULL,
        contract_id uuid,
        payment_id uuid,
        type integer NOT NULL,
        recipient_email text NOT NULL,
        subject text NOT NULL,
        content text NOT NULL,
        sent_at timestamp with time zone,
        status integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_notifications" PRIMARY KEY (id),
        CONSTRAINT "FK_notifications_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id),
        CONSTRAINT "FK_notifications_payments_payment_id" FOREIGN KEY (payment_id) REFERENCES payments (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE splitexecutions (
        id uuid NOT NULL,
        payment_id uuid NOT NULL,
        split_id uuid NOT NULL,
        value numeric NOT NULL,
        executed_at timestamp with time zone NOT NULL,
        tx_hash text,
        status integer NOT NULL,
        split_rule_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_splitexecutions" PRIMARY KEY (id),
        CONSTRAINT "FK_splitexecutions_payments_payment_id" FOREIGN KEY (payment_id) REFERENCES payments (id) ON DELETE CASCADE,
        CONSTRAINT "FK_splitexecutions_splitrules_split_rule_id" FOREIGN KEY (split_rule_id) REFERENCES splitrules (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE invoiceitems (
        id uuid NOT NULL,
        invoice_id uuid NOT NULL,
        item_sequence integer NOT NULL,
        description text NOT NULL,
        ncm_code text NOT NULL,
        quantity numeric NOT NULL,
        unit_value numeric NOT NULL,
        total_value numeric NOT NULL,
        tax_classification text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_invoiceitems" PRIMARY KEY (id),
        CONSTRAINT "FK_invoiceitems_invoices_invoice_id" FOREIGN KEY (invoice_id) REFERENCES invoices (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE TABLE taxcalculations (
        id uuid NOT NULL,
        invoice_id uuid NOT NULL,
        tax_type integer NOT NULL,
        tax_rate numeric NOT NULL,
        tax_base numeric NOT NULL,
        tax_amount numeric NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_taxcalculations" PRIMARY KEY (id),
        CONSTRAINT "FK_taxcalculations_invoices_invoice_id" FOREIGN KEY (invoice_id) REFERENCES invoices (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_auditlogs_performed_by" ON auditlogs (performed_by);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_companies_cnpj ON companies (cnpj);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_companies_created_at ON companies (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_companies_kyc_status ON companies (kyc_status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_companies_type ON companies (type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_contracts_client_id ON contracts (client_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_contracts_created_at ON contracts (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_contracts_provider_id ON contracts (provider_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_contracts_sha256_hash ON contracts (sha256_hash);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_contracts_status ON contracts (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_invoiceitems_invoice_id" ON invoiceitems (invoice_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_invoices_contract_id" ON invoices (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_invoices_payment_id" ON invoices (payment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_kycrecords_company_id" ON kycrecords (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_ledgerentries_contract_id" ON ledgerentries (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_ledgerentries_payment_id" ON ledgerentries (payment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_notifications_contract_id" ON notifications (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_notifications_payment_id" ON notifications (payment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_payments_contract_id ON payments (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_payments_created_at ON payments (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_payments_method ON payments (method);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_payments_payment_date ON payments (payment_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_payments_status ON payments (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_payments_company_id" ON payments (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_sessions_user_id" ON sessions (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_signatures_contract_id ON signatures (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_signatures_contract_user_unique ON signatures (contract_id, user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_signatures_signed_at ON signatures (signed_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_signatures_user_id ON signatures (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_splitexecutions_payment_id" ON splitexecutions (payment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_splitexecutions_split_rule_id" ON splitexecutions (split_rule_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_splitrules_contract_id" ON splitrules (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX "IX_taxcalculations_invoice_id" ON taxcalculations (invoice_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_tokenizedassets_contract_id" ON tokenizedassets (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_users_created_at ON users (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE UNIQUE INDEX idx_users_email ON users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    CREATE INDEX idx_users_role ON users (role);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012164321_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012164321_InitialCreate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012171837_AddCompanyIdToUser') THEN
    ALTER TABLE users ADD company_id uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012171837_AddCompanyIdToUser') THEN
    CREATE INDEX "IX_users_company_id" ON users (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012171837_AddCompanyIdToUser') THEN
    ALTER TABLE users ADD CONSTRAINT "FK_users_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012171837_AddCompanyIdToUser') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012171837_AddCompanyIdToUser', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012191149_AddBusinessModelToCompany') THEN
    ALTER TABLE companies ADD business_model integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012191149_AddBusinessModelToCompany') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012191149_AddBusinessModelToCompany', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    CREATE TABLE userinvites (
        id uuid NOT NULL,
        inviter_name character varying(200) NOT NULL,
        invitee_email character varying(255) NOT NULL,
        invitee_name character varying(200) NOT NULL,
        role integer NOT NULL,
        business_model integer,
        company_name character varying(300),
        cnpj character varying(18),
        company_type integer,
        company_id uuid NOT NULL,
        invited_by_user_id uuid NOT NULL,
        token character varying(50) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        is_accepted boolean NOT NULL DEFAULT FALSE,
        invite_type integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_userinvites" PRIMARY KEY (id),
        CONSTRAINT "FK_userinvites_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_userinvites_users_invited_by_user_id" FOREIGN KEY (invited_by_user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    CREATE INDEX "IX_userinvites_company_id_is_accepted_is_deleted" ON userinvites (company_id, is_accepted, is_deleted);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    CREATE INDEX "IX_userinvites_invited_by_user_id" ON userinvites (invited_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    CREATE INDEX "IX_userinvites_invitee_email" ON userinvites (invitee_email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    CREATE UNIQUE INDEX "IX_userinvites_token" ON userinvites (token);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012193722_AddUserInvitesTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012193722_AddUserInvitesTable', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012201236_AddCompanyRelationships') THEN
    CREATE TABLE companyrelationships (
        id uuid NOT NULL,
        client_company_id uuid NOT NULL,
        provider_company_id uuid NOT NULL,
        type integer NOT NULL,
        status integer NOT NULL,
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone,
        notes text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_companyrelationships" PRIMARY KEY (id),
        CONSTRAINT "FK_companyrelationships_companies_client_company_id" FOREIGN KEY (client_company_id) REFERENCES companies (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_companyrelationships_companies_provider_company_id" FOREIGN KEY (provider_company_id) REFERENCES companies (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012201236_AddCompanyRelationships') THEN
    CREATE UNIQUE INDEX "IX_companyrelationships_client_company_id_provider_company_id_~" ON companyrelationships (client_company_id, provider_company_id, type) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012201236_AddCompanyRelationships') THEN
    CREATE INDEX "IX_companyrelationships_provider_company_id" ON companyrelationships (provider_company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251012201236_AddCompanyRelationships') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251012201236_AddCompanyRelationships', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    ALTER TABLE contracts ADD description text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    ALTER TABLE contracts ADD expiration_date timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    ALTER TABLE contracts ADD monthly_value numeric;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    ALTER TABLE contracts ADD signed_date timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    ALTER TABLE contracts ADD start_date timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027215749_AddRegenerateTokenMethod') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027215749_AddRegenerateTokenMethod', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    DROP INDEX idx_users_email;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD aceitou_politica_privacidade boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD aceitou_termos_uso boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD avatar_url character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD c_p_f_encrypted character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD cargo character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD data_aceite_politica_privacidade timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD data_aceite_termos_uso timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD data_nascimento timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_bairro character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_cep character varying(10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_cidade character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_complemento character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_estado character varying(2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_numero character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_pais character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD endereco_rua character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD r_g_encrypted character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD telefone_celular character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD telefone_fixo character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD versao_politica_privacidade_aceita character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    ALTER TABLE users ADD versao_termos_uso_aceita character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE TABLE notificationpreferences (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        user_id1 uuid NOT NULL,
        receber_email_novo_contrato boolean NOT NULL DEFAULT TRUE,
        receber_email_contrato_assinado boolean NOT NULL DEFAULT TRUE,
        receber_email_contrato_vencendo boolean NOT NULL DEFAULT TRUE,
        receber_email_pagamento_processado boolean NOT NULL DEFAULT TRUE,
        receber_email_pagamento_recebido boolean NOT NULL DEFAULT TRUE,
        receber_email_novo_funcionario boolean NOT NULL DEFAULT TRUE,
        receber_email_alertas_financeiros boolean NOT NULL DEFAULT TRUE,
        receber_email_atualizacoes_sistema boolean NOT NULL DEFAULT TRUE,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_notificationpreferences" PRIMARY KEY (id),
        CONSTRAINT "FK_notificationpreferences_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
        CONSTRAINT "FK_notificationpreferences_users_user_id1" FOREIGN KEY (user_id1) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE TABLE userinvitations (
        id uuid NOT NULL,
        name character varying(200) NOT NULL,
        email character varying(200) NOT NULL,
        role text NOT NULL,
        cargo character varying(100),
        company_id uuid NOT NULL,
        invited_by_user_id uuid NOT NULL,
        status text NOT NULL,
        invitation_token character varying(100) NOT NULL,
        expires_at timestamp with time zone NOT NULL,
        accepted_at timestamp with time zone,
        accepted_by_user_id uuid,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_userinvitations" PRIMARY KEY (id),
        CONSTRAINT "FK_userinvitations_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_userinvitations_users_accepted_by_user_id" FOREIGN KEY (accepted_by_user_id) REFERENCES users (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_userinvitations_users_invited_by_user_id" FOREIGN KEY (invited_by_user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE UNIQUE INDEX idx_users_email ON users (email) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE UNIQUE INDEX "IX_users_c_p_f_encrypted" ON users (c_p_f_encrypted) WHERE c_p_f_encrypted IS NOT NULL AND is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_users_data_nascimento" ON users (data_nascimento);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_users_is_deleted" ON users (is_deleted);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_notificationpreferences_is_deleted" ON notificationpreferences (is_deleted);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE UNIQUE INDEX "IX_notificationpreferences_user_id" ON notificationpreferences (user_id) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_notificationpreferences_user_id1" ON notificationpreferences (user_id1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_userinvitations_accepted_by_user_id" ON userinvitations (accepted_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_userinvitations_company_id_status" ON userinvitations (company_id, status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_userinvitations_email" ON userinvitations (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE UNIQUE INDEX "IX_userinvitations_invitation_token" ON userinvitations (invitation_token);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    CREATE INDEX "IX_userinvitations_invited_by_user_id" ON userinvitations (invited_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251029131523_AddUserInvitationsTable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251029131523_AddUserInvitationsTable', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193120_AdicionarTokenRecuperacaoSenha') THEN
    ALTER TABLE users ADD password_reset_token text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193120_AdicionarTokenRecuperacaoSenha') THEN
    ALTER TABLE users ADD password_reset_token_expiry timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193120_AdicionarTokenRecuperacaoSenha') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251030193120_AdicionarTokenRecuperacaoSenha', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_city text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_complement text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_country text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_neighborhood text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_number text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_state text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_street text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD address_zip_code text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD phone_landline text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    ALTER TABLE companies ADD phone_mobile text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251030193745_AdicionarContatoEnderecoEmpresa') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251030193745_AdicionarContatoEnderecoEmpresa', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ALTER COLUMN performed_by DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ALTER COLUMN ip_address DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ALTER COLUMN entity_id DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD duration double precision NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD http_method text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD path text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD performed_by_email text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD status_code integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD success boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    ALTER TABLE auditlogs ADD user_agent text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251101004055_AtualizarAuditLogComCamposExtras') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251101004055_AtualizarAuditLogComCamposExtras', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251102050705_CorrigirNomeTabelaNotificationPreferences') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251102050705_CorrigirNomeTabelaNotificationPreferences', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    ALTER TABLE notificationpreferences DROP CONSTRAINT "FK_notificationpreferences_users_user_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    ALTER TABLE notificationpreferences DROP CONSTRAINT "FK_notificationpreferences_users_user_id1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    DROP INDEX "IX_notificationpreferences_user_id1";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    ALTER TABLE notificationpreferences DROP COLUMN user_id1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE TABLE contracttemplates (
        id uuid NOT NULL,
        nome character varying(200) NOT NULL,
        descricao character varying(1000) NOT NULL,
        tipo text NOT NULL,
        conteudo_html text NOT NULL,
        conteudo_docx text,
        eh_padrao boolean NOT NULL DEFAULT FALSE,
        ativo boolean NOT NULL DEFAULT TRUE,
        company_id uuid NOT NULL,
        variaveis_disponiveis jsonb NOT NULL,
        data_desativacao timestamp with time zone,
        motivo_desativacao character varying(500),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_contracttemplates" PRIMARY KEY (id),
        CONSTRAINT "FK_contracttemplates_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE TABLE contractdocuments (
        id uuid NOT NULL,
        contract_id uuid NOT NULL,
        template_id uuid,
        conteudo_html text NOT NULL,
        conteudo_pdf text,
        conteudo_docx text,
        versao_major integer NOT NULL,
        versao_minor integer NOT NULL,
        data_geracao timestamp with time zone NOT NULL,
        gerado_por_usuario_id uuid,
        dados_preenchidos jsonb NOT NULL,
        eh_versao_final boolean NOT NULL DEFAULT FALSE,
        hash_documento character varying(100),
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_contractdocuments" PRIMARY KEY (id),
        CONSTRAINT "FK_contractdocuments_contracts_contract_id" FOREIGN KEY (contract_id) REFERENCES contracts (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_contractdocuments_contracttemplates_template_id" FOREIGN KEY (template_id) REFERENCES contracttemplates (id) ON DELETE SET NULL,
        CONSTRAINT "FK_contractdocuments_users_gerado_por_usuario_id" FOREIGN KEY (gerado_por_usuario_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contractdocuments_contract_id" ON contractdocuments (contract_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contractdocuments_contract_id_eh_versao_final" ON contractdocuments (contract_id, eh_versao_final);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contractdocuments_eh_versao_final" ON contractdocuments (eh_versao_final);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contractdocuments_gerado_por_usuario_id" ON contractdocuments (gerado_por_usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contractdocuments_template_id" ON contractdocuments (template_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contracttemplates_ativo" ON contracttemplates (ativo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contracttemplates_company_id" ON contracttemplates (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    CREATE INDEX "IX_contracttemplates_company_id_tipo_eh_padrao" ON contracttemplates (company_id, tipo, eh_padrao);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118151626_AdicionarSistemaTemplatesContratos') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251118151626_AdicionarSistemaTemplatesContratos', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118182357_AdicionarCampoEhSistemaTemplate') THEN
    ALTER TABLE contracttemplates ADD eh_sistema boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118182357_AdicionarCampoEhSistemaTemplate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251118182357_AdicionarCampoEhSistemaTemplate', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124173628_AdicionarContractTemplateConfig') THEN
    CREATE TABLE contract_template_configs (
        id uuid NOT NULL,
        company_id uuid NOT NULL,
        titulo_servico character varying(200) NOT NULL,
        descricao_servico character varying(1000) NOT NULL,
        local_prestacao_servico character varying(500) NOT NULL,
        detalhamento_servicos jsonb NOT NULL,
        clausula_ajuda_custo character varying(2000),
        obrigacoes_contratado jsonb NOT NULL,
        obrigacoes_contratante jsonb NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        created_at timestamp with time zone NOT NULL,
        is_deleted boolean NOT NULL,
        CONSTRAINT "PK_contract_template_configs" PRIMARY KEY (id),
        CONSTRAINT "FK_contract_template_configs_companies_company_id" FOREIGN KEY (company_id) REFERENCES companies (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124173628_AdicionarContractTemplateConfig') THEN
    CREATE UNIQUE INDEX "IX_contract_template_configs_company_id" ON contract_template_configs (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124173628_AdicionarContractTemplateConfig') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124173628_AdicionarContractTemplateConfig', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    DROP INDEX "IX_contract_template_configs_company_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    ALTER TABLE contract_template_configs ADD categoria character varying(50) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    ALTER TABLE contract_template_configs ADD nome_config character varying(100) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    CREATE INDEX "IX_contract_template_configs_company_id" ON contract_template_configs (company_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    CREATE UNIQUE INDEX "IX_contract_template_configs_company_id_nome_config" ON contract_template_configs (company_id, nome_config);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124183809_PermitirMultiplasConfigsPorEmpresa') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124183809_PermitirMultiplasConfigsPorEmpresa', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124185124_AdicionarCamposContratoPJ') THEN
    ALTER TABLE contracts ADD dia_pagamento integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124185124_AdicionarCamposContratoPJ') THEN
    ALTER TABLE contracts ADD dia_vencimento_n_f integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124185124_AdicionarCamposContratoPJ') THEN
    ALTER TABLE contracts ADD type integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124185124_AdicionarCamposContratoPJ') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124185124_AdicionarCamposContratoPJ', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    ALTER TABLE users ADD estado_civil text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    ALTER TABLE users ADD nacionalidade text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    ALTER TABLE users ADD orgao_expedidor_r_g text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    ALTER TABLE companies ADD nire text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    ALTER TABLE companies ADD state_registration text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais', '8.0.11');
    END IF;
END $EF$;
COMMIT;

