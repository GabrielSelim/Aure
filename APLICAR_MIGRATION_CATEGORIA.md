# 🔧 Script para Corrigir Erro de Categoria em Produção

## ❌ Problema Identificado
```
Erro: column c.categoria does not exist
```

A migration `20251124183809_PermitirMultiplasConfigsPorEmpresa` não foi aplicada em produção.
Esta migration adiciona as colunas `categoria` e `nome_config` na tabela `contract_template_configs`.

---

## 📋 Comandos para Aplicar em Produção

### 1️⃣ Conectar ao servidor
```bash
ssh root@5.189.174.61
```

### 2️⃣ Fazer backup do banco (recomendado)
```bash
docker exec aure-postgres-aure-gabriel pg_dump -U aure_user aure_production > backup_antes_categoria_$(date +%Y%m%d_%H%M%S).sql
```

### 3️⃣ Aplicar a migration
```bash
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<'EOF'
START TRANSACTION;

DROP INDEX "IX_contract_template_configs_company_id";

ALTER TABLE contract_template_configs ADD categoria character varying(50) NOT NULL DEFAULT '';

ALTER TABLE contract_template_configs ADD nome_config character varying(100) NOT NULL DEFAULT '';

CREATE INDEX "IX_contract_template_configs_company_id" ON contract_template_configs (company_id);

CREATE UNIQUE INDEX "IX_contract_template_configs_company_id_nome_config" ON contract_template_configs (company_id, nome_config);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124183809_PermitirMultiplasConfigsPorEmpresa', '8.0.11');

COMMIT;
EOF
```

### 4️⃣ Verificar se foi aplicado com sucesso
```bash
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<'EOF'
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'contract_template_configs' 
  AND column_name IN ('categoria', 'nome_config');
EOF
```

**Output esperado:**
```
 column_name  |     data_type     | character_maximum_length 
--------------+-------------------+--------------------------
 categoria    | character varying |                       50
 nome_config  | character varying |                      100
```

### 5️⃣ Fazer pull do código atualizado e rebuild
```bash
cd /root/Aure
git pull origin main
docker-compose down
docker-compose up -d --build
```

### 6️⃣ Verificar logs
```bash
docker logs -f aure-api-aure-gabriel --tail=100
```

### 7️⃣ Testar o endpoint
```bash
curl -X GET https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig/config \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

**Resposta esperada (se não houver configs):**
```json
[]
```

**Resposta esperada (se houver configs):**
```json
[
  {
    "id": "uuid",
    "companyId": "uuid",
    "nomeEmpresa": "Sua Empresa",
    "nomeConfig": "Nome da Config",
    "categoria": "TI",
    ...
  }
]
```

---

## 🔍 Verificar Migrations Aplicadas

Para ver quais migrations já foram aplicadas:
```bash
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<'EOF'
SELECT "MigrationId", "ProductVersion" 
FROM "__EFMigrationsHistory" 
ORDER BY "MigrationId";
EOF
```

---

## ⚠️ Se der erro ao aplicar

### Erro: "relation already exists"
Se o índice já existir:
```sql
-- Pular a linha DROP INDEX e continuar
```

### Erro: "column already exists"
Se as colunas já existirem:
```bash
# Verificar se precisa apenas registrar a migration
docker exec -i aure-postgres-aure-gabriel psql -U aure_user -d aure_production <<'EOF'
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251124183809_PermitirMultiplasConfigsPorEmpresa', '8.0.11');
EOF
```

---

## ✅ Após Aplicar

O endpoint `GET /api/ContractTemplateConfig/config` deve funcionar corretamente.

Você poderá:
- ✅ Listar todas as configurações de templates
- ✅ Criar novas configurações
- ✅ Clonar presets
- ✅ Gerar contratos PJ

---

**Data**: 02/12/2025  
**Migration**: `20251124183809_PermitirMultiplasConfigsPorEmpresa`
