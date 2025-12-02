# Requisitos para o Backend - Sistema de Notificação

## Endpoints Necessários

### 1. POST `/api/Notifications/notificar-completar-cadastro-pj`

**Descrição:** Envia email para um funcionário PJ solicitando que complete seu cadastro.
 Sabendo que o endereço da empresa do pj deve ser o mesmo do funcionario pj

**Permissões:** Roles 1 (DonoEmpresaPai), 2 (Financeiro), 3 (Juridico)
 Sabendo que o endereço da empresa do pj deve ser o mesmo do funcionario pj
**Request Body:**
```json
{
  "userId": "uuid-do-funcionario-pj",
  "camposFaltando": [
    "CPF",
    "RG",
    "Órgão Expedidor RG",
    "Data de Nascimento",
    "Nacionalidade",
    "Estado Civil",
    "Endereço - Rua",
    "Endereço - Número",
    "Endereço - Bairro",
    "Endereço - Cidade",
    "Endereço - Estado",
    "CEP",
    "Empresa PJ - Razão Social",
    "Empresa PJ - CNPJ",
    "Empresa PJ - Inscrição Estadual",
    "Empresa PJ - Endereço Rua",
    "Empresa PJ - Endereço Número",
    "Empresa PJ - Endereço Bairro",
    "Empresa PJ - Endereço Cidade",
    "Empresa PJ - Endereço Estado",
    "Empresa PJ - CEP"
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Email enviado com sucesso"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Usuário não encontrado ou não é um funcionário PJ"
}
```

**Response (403 Forbidden):**
```json
{
  "message": "Sem permissão para notificar funcionários"
}
```

---

## Template de Email - Funcionário PJ

**Assunto:** Complete seu cadastro - Dados necessários para contrato

**Corpo:**
```
Olá [Nome do Funcionário],

A empresa [Nome da Empresa Pai] está tentando gerar um contrato com você, mas seu cadastro está incompleto.

Por favor, acesse o sistema e complete os seguintes dados em Configurações > Perfil e Empresa PJ:

[Lista de campos faltando]

Após completar seu cadastro, a empresa poderá gerar o contrato.

Acesse: [URL do sistema]/configuracoes

Atenciosamente,
Sistema Aure
```

---

### 2. POST `/api/Notifications/notificar-completar-cadastro-empresa`

**Descrição:** Envia email/notificação interna para o Dono da Empresa ou Gestores alertando que dados da empresa principal estão incompletos e impedindo a geração de contratos.

**Permissões:** Qualquer usuário autenticado da empresa (usado pelo próprio sistema internamente quando detecta dados faltando)

**Request Body:**
```json
{
  "empresaId": "uuid-da-empresa-principal",
  "camposFaltando": [
    "NIRE",
    "Inscrição Estadual"
  ],
  "tentouGerarContrato": true,
  "usuarioSolicitante": "Nome do Gestor que tentou gerar"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Notificação enviada para gestores",
  "destinatarios": ["email1@empresa.com", "email2@empresa.com"]
}
```

**Comportamento:**
- Envia email para DonoEmpresaPai (role 1)
- Cria notificação interna no sistema para roles 1, 2 e 3
- Registra tentativa de geração de contrato bloqueada

---

## Template de Email - Empresa Incompleta

**Assunto:** ⚠️ Dados da Empresa Incompletos - Bloqueio de Contratos

**Corpo:**
```
Olá [Nome do Dono/Gestor],

O usuário [Nome do Gestor] tentou gerar um contrato, mas a operação foi bloqueada porque os dados da empresa estão incompletos.

Campos faltando na empresa [Nome da Empresa]:
[Lista de campos faltando]

Para gerar contratos, é obrigatório completar esses dados.

Por favor, acesse Configurações > Empresa e preencha os campos faltando.

Acesse: [URL do sistema]/configuracoes

⚠️ Enquanto esses dados não forem preenchidos, NENHUM contrato poderá ser gerado.

Atenciosamente,
Sistema Aure
```

---

## Validações Backend

### Para Funcionário PJ:
1. Verificar se `userId` existe e é um FuncionarioPJ (role 5)
2. Verificar se usuário autenticado tem permissão (roles 1, 2 ou 3)
3. Verificar se usuário autenticado pertence à mesma empresa que contratou o PJ
4. Enviar email apenas se lista `camposFaltando` não estiver vazia

### Para Empresa:
1. Verificar se `empresaId` existe
2. Buscar todos os usuários com roles 1, 2 e 3 da empresa
3. Enviar email para role 1 (DonoEmpresaPai)
4. Criar notificação interna para roles 1, 2 e 3
5. Registrar em log a tentativa bloqueada

---

## Fluxo Completo

### Cenário 1: Dados da Empresa Incompletos
1. Gestor acessa `/contratos/gerar`
2. Sistema valida dados da empresa automaticamente
3. Frontend detecta NIRE ou Inscrição Estadual faltando
4. Mostra alerta vermelho com botão "Completar Cadastro da Empresa"
5. **AUTOMATICAMENTE** chama `POST /api/Notifications/notificar-completar-cadastro-empresa`
6. Backend envia email para Dono da Empresa
7. Dono recebe email e completa cadastro
8. Gestor pode gerar contratos

### Cenário 2: Dados do Funcionário PJ Incompletos
1. Gestor acessa `/contratos/gerar`
2. Seleciona funcionário PJ
3. Frontend valida automaticamente campos do PJ
4. Se campos faltando, mostra alerta amarelo
5. Gestor clica em "Notificar Funcionário por Email"
6. Frontend chama `POST /api/Notifications/notificar-completar-cadastro-pj`
7. Backend envia email para o PJ
8. PJ recebe email e completa cadastro
9. Gestor pode gerar contrato

---

## Observações

- Email deve ser enviado assincronamente (não bloquear request)
- Registrar tentativa de notificação em log
- Não enviar mais de 1 email por dia para o mesmo usuário sobre os mesmos campos
- Incluir link direto para página de configurações no email
