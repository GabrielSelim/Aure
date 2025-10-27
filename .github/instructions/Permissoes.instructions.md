---
applyTo: '**'
---

# Sistema de Hierarquia e Permissões Empresariais - Aure

## 🏢 Estrutura Organizacional

### Empresa Pai (Principal)
- **Responsável/Dono**: Usuário que cria a conta inicial da empresa
- **Financeiro**: Usuários com acesso a gestão financeira e operacional
- **Jurídico**: Usuários com acesso a contratos e documentação legal

### Funcionários
- **CLT**: Funcionários com carteira assinada
- **PJ**: Prestadores de serviço (Pessoa Jurídica)

## 🔐 Matriz de Permissões

### Dono da Empresa Pai
**Permissões Financeiras:**
- ✅ Efetuar pagamentos
- ✅ Gerar relatórios financeiros
- ✅ Visualizar dados financeiros completos

**Permissões Operacionais:**
- ✅ Cadastrar usuários Financeiro e Jurídico
- ✅ Definir quais funcionários podem assinar contratos
- ✅ Gerenciar funcionários CLT e PJ
- ✅ Gerar todos os tipos de relatórios

**Notificações:**
- ✅ Pagamentos
- ✅ Vencimentos de contratos
- ✅ Notificações financeiras

### Financeiro
**Permissões Operacionais:**
- ✅ Visualizar dados da empresa pai
- ✅ Gerenciar funcionários
- ✅ Gerar relatórios de funcionários e contratos
- ✅ Visualizar dados financeiros
- ❌ Efetuar pagamentos (somente dono)
- ❌ Autorizar transferências

**Notificações:**
- ✅ Novos funcionários cadastrados
- ✅ Contratos assinados
- ✅ Alertas financeiros
- ❌ Confirmações de pagamento

### Jurídico
**Permissões Operacionais:**
- ✅ Visualizar dados da empresa pai (limitado)
- ✅ Visualizar funcionários
- ✅ Gerenciar contratos e documentação
- ✅ Gerar relatórios de contratos
- ❌ Efetuar pagamentos
- ❌ Visualizar dados financeiros sensíveis

**Notificações:**
- ✅ Novos funcionários cadastrados
- ✅ Contratos assinados
- ✅ Vencimentos contratuais
- ❌ Notificações financeiras

### Funcionários PJ
**Permissões:**
- ✅ Acessar sistema para assinar contratos
- ✅ Visualizar próprio perfil
- ✅ Atualizar dados pessoais
- ❌ Visualizar dados financeiros da empresa
- ❌ Visualizar outros funcionários

## 📋 Regras de Negócio

### Cadastro Inicial
1. Primeiro usuário cadastrado = Dono da empresa pai
2. Somente o dono pode cadastrar usuários Financeiro e Jurídico
3. Empresa pai é criada automaticamente no primeiro cadastro

### Contratos
1. **Funcionários CLT**: Contrato padrão gerado automaticamente
2. **Funcionários PJ**: Contratos personalizados que precisam ser assinados
3. **Assinatura**: Dono define quais tipos de funcionários podem assinar

### Relatórios
- **Financeiros**: Somente dono
- **Funcionários/Contratos**: Dono, Financeiro e Jurídico
- **Básicos**: Todos os níveis conforme permissão

## 🔧 Implementação Técnica

### Endpoints de Segurança
Todos os endpoints devem verificar:
1. **Autenticação**: Usuário logado
2. **Autorização**: Nível de permissão adequado
3. **Escopo**: Dados limitados à empresa pai do usuário

### Endpoint Perfil
**Campos editáveis por todos os usuários:**
- Nome
- Email
- TelefoneCelular
- TelefoneFixo
- Rua
- Cidade
- Estado
- Pais
- Cep
- Senha (com confirmação da atual)

### Filtragem de Dados
- **Financeiro/Jurídico**: Somente funcionários da empresa pai
- **Funcionários PJ**: Somente próprios dados
- **Dono**: Acesso completo aos dados da empresa

## 📊 Sistema de Notificações

### Canais por Perfil
```
Dono:
├── Pagamentos realizados
├── Vencimentos de contratos
├── Alertas financeiros
└── Todas as notificações operacionais

Financeiro:
├── Novos funcionários
├── Contratos assinados
├── Alterações em funcionários
└── Relatórios gerados

Jurídico:
├── Novos funcionários
├── Contratos assinados
├── Vencimentos contratuais
└── Relatórios de contratos

Funcionários PJ:
├── Contratos para assinatura
├── Atualizações contratuais
└── Notificações pessoais
```

## ⚠️ Validações Críticas

1. **Pagamentos**: Somente dono pode autorizar
2. **Dados Financeiros**: Isolamento total para não-donos
3. **Hierarquia**: Respeitar níveis de acesso em todas as operações
4. **Empresa Pai**: Todos os dados limitados ao escopo da empresa
5. **Contratos PJ**: Funcionários PJ só veem próprios contratos

---

**Objetivo**: Implementar sistema robusto de permissões que garanta segurança financeira e operacional, mantendo flexibilidade para diferentes níveis hierárquicos dentro da empresa.