START TRANSACTION;

CREATE TABLE IF NOT EXISTS notificationhistories (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    tipo_notificacao text NOT NULL,
    campos_notificados text NOT NULL,
    data_envio timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_notificationhistories" PRIMARY KEY (id),
    CONSTRAINT "FK_notificationhistories_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_notificationhistories_user_id" ON notificationhistories (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251202175341_AdicionarTabelaNotificationHistory', '8.0.11')
ON CONFLICT DO NOTHING;

COMMIT;
