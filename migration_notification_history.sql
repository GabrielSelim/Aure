START TRANSACTION;

ALTER TABLE contracts ADD dia_pagamento integer;

ALTER TABLE contracts ADD dia_vencimento_n_f integer;

ALTER TABLE contracts ADD type integer NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124185124_AdicionarCamposContratoPJ', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE users ADD estado_civil text;

ALTER TABLE users ADD nacionalidade text;

ALTER TABLE users ADD orgao_expedidor_r_g text;

ALTER TABLE companies ADD nire text;

ALTER TABLE companies ADD state_registration text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais', '8.0.11');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124203941_AdicionarCamposPagamentoContract', '8.0.11');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251202141847_AdicionarNacionalidadeEstadoCivilDadosManual', '8.0.11');

COMMIT;

START TRANSACTION;

CREATE TABLE notificationhistories (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    tipo_notificacao text NOT NULL,
    campos_notificados text NOT NULL,
    data_envio timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_notificationhistories" PRIMARY KEY (id),
    CONSTRAINT "FK_notificationhistories_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX "IX_notificationhistories_user_id" ON notificationhistories (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251202175341_AdicionarTabelaNotificationHistory', '8.0.11');

COMMIT;

