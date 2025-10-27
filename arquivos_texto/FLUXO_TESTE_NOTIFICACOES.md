# 📋 Fluxo de Teste - Sistema de Notificações Aure

## 🎯 Objetivo
Testar completamente o sistema de notificações automáticas implementado, incluindo:
- Notificações de pagamento para funcionários PJ
- Alertas de vencimento de contratos
- Notificações de contratos assinados
- Templates HTML profissionais
- Background jobs Hangfire

---

## 🚀 1. Preparação do Ambiente

### 1.1 Verificar Serviços Docker
```bash
# Verificar se containers estão rodando
docker ps

# Se necessário, iniciar containers
docker-compose up -d

# Verificar logs da API
docker logs aure-api --tail 20
```

### 1.2 URLs Importantes
- **API**: http://localhost:5203
- **Swagger**: http://localhost:5203/swagger
- **Hangfire Dashboard**: http://localhost:5203/hangfire  
- **Adminer (BD)**: http://localhost:8080

### 1.3 Verificar Hangfire Dashboard
- URL: `http://localhost:5203/hangfire`
- Verificar se o dashboard carrega sem erros
- Confirmar que as filas estão criadas: `notificacoes`, `contratos`, `pagamentos`
- Verificar se jobs recorrentes estão agendados:
  - "Verificar Contratos Vencendo" (diário)
  - "Limpar Notificações Antigas" (semanal)

---

## 👤 2. Cadastro de Usuários e Empresa

### 2.1 Cadastrar Admin Empresa (Dono da Empresa Pai)
```json
POST /api/registration/admin-empresa
{
    "nome": "Gabriel Selim",
    "email": "gabriel@aurecontroll.com",
    "senha": "MinhaSenh@123",
    "confirmarSenha": "MinhaSenh@123",
    "telefoneCelular": "(11) 99999-9999",
    "cpf": "123.456.789-09",
    "empresa": {
        "nome": "Magazine Luiza S.A.",
        "cnpj": "47.960.950/0001-21",
        "razaoSocial": "Magazine Luiza S.A.",
        "rua": "Rua Sacadura Cabral, 102",
        "cidade": "São Paulo",
        "estado": "SP",
        "pais": "Brasil",
        "cep": "01007-907",
        "telefone": "(11) 3003-4567",
        "email": "contato@magazineluiza.com.br",
        "website": "https://www.magazineluiza.com.br"
    }
}
```

**✅ Verificações esperadas:**
- Primeiro usuário automaticamente vira **DonoEmpresaPai**
- Empresa é criada automaticamente e vinculada ao usuário
- Usuário já pode fazer login imediatamente

### 2.2 Fazer Login como Dono
```json
POST /api/auth/entrar
{
    "email": "gabriel@aurecontroll.com",
    "senha": "MinhaSenh@123"
}
```
**⚠️ IMPORTANTE**: Salvar o `tokenAcesso` retornado para usar nos próximos requests

### 2.3 Convidar Usuário Financeiro (Interno da Empresa)
```json
POST /api/registration/convidar-usuario
Headers: Authorization: Bearer {token_do_dono}
{
    "nome": "Maria Financeira",
    "email": "maria@aurecontroll.com",
    "tipoUsuario": "Financeiro",
    "telefoneCelular": "(11) 98888-8888"
}
```

**✅ Verificações esperadas:**
- Convite criado e enviado por email
- Maria receberá link para aceitar e definir senha
- Usuário será vinculado à empresa pai (não PJ)

---

## 🏢 3. Cadastro de Funcionário PJ

### 3.1 Convidar Funcionário PJ (Dono ou Jurídico)
```json
POST /api/registration/convidar-usuario
Headers: Authorization: Bearer {token_do_dono}
{
    "nome": "João Desenvolvedor",
    "email": "joao.dev@gmail.com",
    "tipoUsuario": "PJ",
    "telefoneCelular": "(11) 97777-7777",
    "empresaPj": {
        "nome": "Netflix Entretenimento Brasil Ltda.",
        "cnpj": "13.590.585/0001-00",
        "razaoSocial": "Netflix Entretenimento Brasil Ltda.",
        "rua": "Avenida das Nações Unidas, 12901",
        "cidade": "São Paulo", 
        "estado": "SP",
        "pais": "Brasil",
        "cep": "04578-910",
        "telefone": "(11) 3045-2000",
        "email": "contato@netflix.com.br"
    }
}
```

**✅ Verificações esperadas:**
- Convite PJ criado com empresa separada
- Email enviado para João com link de aceite
- Empresa PJ cadastrada no sistema
- Relacionamento criado entre empresa pai e empresa PJ

### 3.2 Funcionário PJ Aceita Convite
```json
POST /api/registration/aceitar-convite/{token_do_convite}
{
    "senha": "MinhaSenh@123",
    "confirmarSenha": "MinhaSenh@123",
    "aceitaTermos": true
}
```

### 3.3 Funcionário PJ Faz Login
```json
POST /api/auth/entrar
{
    "email": "joao.dev@gmail.com",
    "senha": "MinhaSenh@123"
}
```
**⚠️ IMPORTANTE**: Salvar o token para usar nos testes de contrato

### 3.4 Obter IDs Necessários para Próximos Passos

#### Obter ID da Empresa PJ
```json
GET /api/CompanyRelationships
Headers: Authorization: Bearer {token_do_dono}
```
**Buscar pelo providerId nos relacionamentos ativos**

#### Listar Convites Pendentes
```json
GET /api/registration/convites
Headers: Authorization: Bearer {token_do_dono}
```

#### Verificar Perfil do Usuário Logado
```json
GET /api/auth/perfil
Headers: Authorization: Bearer {token_qualquer_usuario}
```

---

## � 4. Entendendo os Tipos de Usuários

### 4.1 Hierarquia de Permissões
- **DonoEmpresaPai**: Controle total, pode processar pagamentos, criar contratos
- **Financeiro**: Visualiza dados financeiros, mas NÃO pode processar pagamentos
- **Jurídico**: Gerencia contratos, recebe notificações de vencimento
- **PJ**: Funcionário externo, só vê próprios contratos e notificações

### 4.2 Endpoints por Permissão
- **Processar Pagamentos**: Apenas `DonoEmpresaPai`
- **Convidar Usuários**: `DonoEmpresaPai` (interno) ou `DonoEmpresaPai/Jurídico` (PJ)
- **Criar Contratos**: `DonoEmpresaPai` (principalmente)
- **Assinar Contratos**: Qualquer usuário envolvido no contrato

---

##  5. Teste de Contratos

### 5.1 Criar Contrato com Funcionário PJ
```json
POST /api/contracts
Headers: Authorization: Bearer {token_do_dono}
{
    "providerId": "{id_da_empresa_pj}",
    "title": "Contrato de desenvolvimento de software - Projeto Sistema Aure",
    "description": "Desenvolvimento de sistema de notificações com entregáveis mensais obrigatórios, reuniões semanais de acompanhamento",
    "valueTotal": 90000.00,
    "monthlyValue": 15000.00,
    "startDate": "2025-11-01T00:00:00Z",
    "expirationDate": "2026-01-31T23:59:59Z"
}
```

**✅ Verificações esperadas:**
- Contrato criado entre empresa pai (cliente) e empresa PJ (provedor)
- Background job "Notificar Novo Contrato" deve aparecer no Hangfire
- Email enviado para o funcionário PJ (para assinatura)
- Email enviado para Dono e usuários Financeiro/Jurídico (para acompanhamento)
- Notificação salva na tabela `Notifications`

### 5.2 Funcionário PJ Assina Contrato
```json
POST /api/contracts/{contract_id}/assinar
Headers: Authorization: Bearer {token_funcionario_pj}
{
    "method": "Digital",
    "signatureHash": "hash_assinatura_digital_joao_dev"
}
```

**✅ Verificações esperadas:**
- Background job "Notificar Contrato Assinado" no Hangfire
- Email para Dono, Financeiro e Jurídico sobre assinatura
- Status do contrato muda para "Active" se totalmente assinado
- Sistema detecta contrato ativo para futuros pagamentos

---

## 💰 6. Teste de Pagamentos

### 6.1 Criar Pagamento para Contrato PJ
```json
POST /api/payments
Headers: Authorization: Bearer {token_do_dono}
{
    "contratoId": "{id_do_contrato}",
    "valor": 15000.00,
    "metodo": "Pix"
}
```

**✅ Verificações esperadas:**
- Pagamento criado com status "Pending"
- Validação de que contrato está ativo
- Validação de que valor não excede valor mensal contratual

### 6.2 Processar Pagamento (Marcar como Pago)
```json
PUT /api/payments/{payment_id}/processar
Headers: Authorization: Bearer {token_do_dono}
```

**✅ Verificações esperadas:**
- Background job "Notificar Pagamento PJ" no Hangfire
- **EMAIL ENVIADO PARA O FUNCIONÁRIO PJ** (principal teste!)
- Template HTML profissional com:
  - Logo da empresa
  - Valor formatado em R$ 15.000,00
  - Data do pagamento
  - Informações do contrato
  - Design responsivo
- Background job "Notificar Pagamento Processado" para gestores
- Notificação salva no banco
- Status do pagamento muda para "Completed"

### 6.3 Verificar Email do Funcionário PJ
**📧 Checklist do Email:**
- [ ] Assunto: "Pagamento Recebido - R$ 15.000,00"
- [ ] Template HTML carregou corretamente
- [ ] Logo e branding da empresa
- [ ] Valor em destaque: R$ 15.000,00
- [ ] Data formatada corretamente
- [ ] Botão de ação funcional
- [ ] Layout responsivo (testar no celular)

---

## ⏰ 7. Teste de Alertas de Vencimento

### 7.1 Criar Contrato Próximo ao Vencimento
```json
POST /api/contracts
Headers: Authorization: Bearer {token_do_dono}
{
    "providerId": "{id_da_empresa_pj}",
    "title": "Contrato teste - próximo ao vencimento",
    "description": "Contrato para testar alertas de vencimento",
    "valueTotal": 24000.00,
    "monthlyValue": 12000.00,
    "startDate": "2025-10-27T00:00:00Z",
    "expirationDate": "2025-11-15T23:59:59Z"
}
```

### 7.2 Executar Job Manual de Vencimentos
No Hangfire Dashboard:
1. Ir para "Recurring Jobs"
2. Encontrar "Verificar Contratos Vencendo"
3. Clicar em "Trigger Now"

**✅ Verificações esperadas:**
- Job executado com sucesso
- Emails enviados para Dono, Jurídico e Funcionário PJ
- Template de alerta de vencimento utilizado

---

## 🔍 8. Verificações no Hangfire Dashboard

### 8.1 Jobs Processados
- [ ] Todos os jobs aparecem como "Succeeded"
- [ ] Nenhum job em "Failed"
- [ ] Filas `notificacoes`, `contratos`, `pagamentos` funcionando

### 8.2 Jobs Recorrentes
- [ ] "Verificar Contratos Vencendo" agendado para diário
- [ ] "Limpar Notificações Antigas" agendado

### 8.3 Estatísticas
- [ ] Jobs enfileirados: 0
- [ ] Jobs processados: > 0
- [ ] Jobs falhados: 0

---

## 📊 9. Verificações no Banco de Dados

### 9.1 Tabela Notifications
```sql
SELECT * FROM "Notifications" 
ORDER BY "CreatedAt" DESC;
```

**Verificar:**
- [ ] Notificações de pagamento criadas
- [ ] Notificações de contrato criadas
- [ ] UserId correto
- [ ] Tipos de notificação corretos
- [ ] Status das notificações

### 9.2 Tabela Hangfire
```sql
SELECT * FROM hangfire.job 
WHERE statename = 'Succeeded' 
ORDER BY createdat DESC;
```

---

## 🌐 10. Dados Reais para Teste

### 10.1 CNPJs Válidos para Teste
- **Magazine Luiza**: `47.960.950/0001-21`
- **Netflix Brasil**: `13.590.585/0001-00`
- **Nubank**: `18.236.120/0001-58`
- **iFood**: `14.380.200/0001-21`
- **Mercado Livre**: `10.573.521/0001-91`

### 10.2 Empresas e Dados
**Magazine Luiza S.A.**
- CNPJ: 47.960.950/0001-21
- Endereço: Rua Sacadura Cabral, 102 - São Paulo/SP
- CEP: 01007-907

**Netflix Entretenimento Brasil Ltda.**
- CNPJ: 13.590.585/0001-00
- Endereço: Av. das Nações Unidas, 12901 - São Paulo/SP
- CEP: 04578-910

---

## 🚨 11. Possíveis Problemas e Soluções

### 11.1 Email não chega
- [ ] Verificar configurações SMTP no appsettings.json
- [ ] Conferir logs da aplicação
- [ ] Testar com email real (não @test.com)

### 11.2 Background Job falha
- [ ] Verificar dependências injetadas
- [ ] Conferir conexão com banco de dados
- [ ] Verificar logs no Hangfire Dashboard

### 11.3 Template HTML não carrega
- [ ] Verificar se arquivos estão na pasta Templates/
- [ ] Confirmar configuração do build (Copy to Output)
- [ ] Testar URLs das imagens

---

## ✅ 12. Checklist Final

### Sistema Funcionando ✅
- [ ] Dono consegue processar pagamentos
- [ ] Funcionário PJ recebe email de pagamento
- [ ] Template HTML profissional carrega
- [ ] Contratos geram notificações
- [ ] Alertas de vencimento funcionam
- [ ] Hangfire Dashboard sem erros
- [ ] Background jobs executam corretamente

### Qualidade dos Emails ✅
- [ ] Design profissional e responsivo
- [ ] Informações corretas e formatadas
- [ ] Botões funcionais
- [ ] Branding da empresa
- [ ] Compatibilidade mobile

---

## 📞 Suporte

Se algum teste falhar:
1. Verificar logs da aplicação
2. Conferir Hangfire Dashboard
3. Validar configurações de email
4. Testar com dados diferentes
5. Verificar permissões do usuário

**🎯 Objetivo Final**: Funcionário PJ deve receber email profissional sempre que um pagamento for processado, com template HTML bonito e informações completas!