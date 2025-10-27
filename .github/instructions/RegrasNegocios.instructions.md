---
applyTo: '**'
---

# Regras de Negócio e Automações - Sistema Aure

## 🔔 Sistema de Notificações Automáticas

### Pagamentos de Funcionários PJ
**Trigger**: Quando um pagamento é processado para um funcionário PJ
**Destinatário**: Funcionário PJ que recebeu o pagamento
**Conteúdo da Notificação**:
```
Título: "Pagamento Recebido"
Mensagem: "Seu pagamento no valor de R$ {valor} foi processado com sucesso."
Detalhes:
- Valor: R$ {valorPagamento}
- Data do Pagamento: {dataPagamento}
- Referência: {numeroContrato}
- Empresa Pagadora: {nomeEmpresaCliente}
```

### Notificações de Contratos

#### Novo Contrato PJ Criado
**Trigger**: Quando um contrato PJ é criado
**Destinatários**: 
- Funcionário PJ (para assinatura)
- DonoEmpresaPai e Financeiro (para acompanhamento)

**Para Funcionário PJ**:
```
Título: "Novo Contrato Disponível"
Mensagem: "Um novo contrato está disponível para sua assinatura."
Ação: Link para visualizar e assinar o contrato
```

**Para DonoEmpresaPai/Financeiro**:
```
Título: "Contrato PJ Criado"
Mensagem: "Contrato criado para {nomeFuncionario} aguardando assinatura."
```

#### Contrato Assinado
**Trigger**: Quando um funcionário PJ assina um contrato
**Destinatários**: DonoEmpresaPai, Financeiro, Jurídico

```
Título: "Contrato Assinado"
Mensagem: "{nomeFuncionario} assinou o contrato {numeroContrato}."
Detalhes:
- Funcionário: {nomeFuncionario}
- Valor Mensal: R$ {valorMensal}
- Data de Assinatura: {dataAssinatura}
```

### Notificações Financeiras

#### Pagamento Processado (Para Gestores)
**Trigger**: Quando DonoEmpresaPai processa um pagamento
**Destinatários**: Financeiro
**Conteúdo**:
```
Título: "Pagamento Processado"
Mensagem: "Pagamento de R$ {valor} processado para {nomeFuncionario}."
Detalhes:
- Valor: R$ {valor}
- Funcionário: {nomeFuncionario}
- Tipo: {tipoFuncionario} (CLT/PJ)
- Processado por: {nomeDonoEmpresa}
```

#### Alerta de Vencimento de Contrato
**Trigger**: 30, 15 e 7 dias antes do vencimento
**Destinatários**: DonoEmpresaPai, Jurídico, Funcionário PJ
**Conteúdo**:
```
Título: "Contrato Próximo ao Vencimento"
Mensagem: "O contrato de {nomeFuncionario} vence em {diasRestantes} dias."
Detalhes:
- Data de Vencimento: {dataVencimento}
- Funcionário: {nomeFuncionario}
- Valor Mensal: R$ {valorMensal}
```

### Notificações de Auditoria

#### Novo Funcionário Cadastrado
**Trigger**: Quando um novo funcionário é convidado/cadastrado
**Destinatários**: Financeiro, Jurídico (se aplicável)
**Conteúdo**:
```
Título: "Novo Funcionário Cadastrado"
Mensagem: "{nomeFuncionario} foi adicionado como {tipoFuncionario}."
Detalhes:
- Nome: {nomeFuncionario}
- Email: {emailFuncionario}
- Tipo: {tipoFuncionario}
- Cadastrado por: {nomeGestor}
```

## 📋 Automações de Contratos

### Contrato CLT Automático
**Trigger**: Quando um funcionário CLT é convidado e aceita o convite
**Ação Automática**:
1. Gerar contrato CLT padrão baseado em template
2. Definir como "ativo" automaticamente (não precisa assinatura)
3. Notificar Jurídico sobre novo contrato ativo
4. Enviar cópia do contrato para o funcionário CLT

### Renovação Automática de Contratos PJ
**Trigger**: 60 dias antes do vencimento
**Ação Automática**:
1. Verificar se há interesse em renovação
2. Gerar novo contrato com mesmo valor (ou atualizado)
3. Enviar para assinatura do funcionário PJ
4. Notificar gestores sobre processo de renovação

## 💰 Regras de Pagamento

### Validações Antes do Pagamento
**Para Funcionários PJ**:
- Contrato deve estar ativo
- Não pode haver pagamentos pendentes para o mesmo período
- Valor não pode exceder o valor contratual mensal

**Para Funcionários CLT**:
- Deve ter contrato CLT ativo
- Calcular descontos obrigatórios (INSS, IR, etc.)
- Verificar se é dia útil para pagamento

### Histórico de Pagamentos
**Ação Automática**: Ao processar qualquer pagamento
1. Registrar no histórico financeiro
2. Atualizar status do contrato (se aplicável)
3. Gerar comprovante de pagamento
4. Enviar notificação de confirmação

## 🚨 Alertas e Validações

### Alertas Financeiros (Para DonoEmpresaPai)
- Pagamentos acima de R$ 10.000
- Mais de 5 pagamentos no mesmo dia
- Tentativas de pagamento fora do horário comercial

### Alertas de Segurança
- Múltiplas tentativas de login falhadas
- Acesso de funcionário PJ fora do horário esperado
- Alterações em contratos por usuários não autorizados

### Validações de Conformidade
- Verificar se CNPJ está regular antes de processar pagamentos
- Validar se funcionário PJ tem empresa ativa
- Verificar limites mensais de pagamento

## 📊 Relatórios Automáticos

### Relatório Mensal de Pagamentos
**Trigger**: Todo dia 1º do mês
**Destinatários**: DonoEmpresaPai, Financeiro
**Conteúdo**: Resumo de todos os pagamentos do mês anterior

### Relatório de Contratos Vencendo
**Trigger**: Toda segunda-feira
**Destinatários**: DonoEmpresaPai, Jurídico
**Conteúdo**: Lista de contratos que vencem nos próximos 30 dias

## 🔄 Fluxos de Aprovação

### Pagamentos Acima de Threshold
**Regra**: Pagamentos acima de R$ 5.000 podem precisar de aprovação adicional
**Fluxo**:
1. DonoEmpresaPai solicita pagamento
2. Sistema envia notificação para aprovação (se configurado)
3. Após aprovação, processa pagamento
4. Notifica todas as partes envolvidas

### Criação de Usuários Críticos
**Regra**: Criação de usuários Financeiro ou Jurídico
**Fluxo**:
1. Apenas DonoEmpresaPai pode criar
2. Enviar email de confirmação
3. Exigir aceite dos termos específicos da função
4. Notificar outros gestores sobre novo usuário

## 🎯 Configurações por Empresa

### Limites Personalizáveis
- Valor máximo de pagamento sem aprovação adicional
- Número máximo de funcionários PJ
- Prazo padrão para contratos PJ
- Horários permitidos para operações críticas

### Templates de Notificação
- Personalizar mensagens por empresa
- Definir canais preferenciais (email, sistema, SMS)
- Configurar frequência de lembretes

---

## 📝 Implementação Técnica

### Prioridade de Implementação
1. **Alta**: Notificações de pagamento para funcionários PJ
2. **Alta**: Alertas de vencimento de contrato
3. **Média**: Contratos CLT automáticos
4. **Média**: Relatórios automáticos
5. **Baixa**: Fluxos de aprovação avançados

### Tecnologias Sugeridas
- **Background Jobs**: Hangfire ou similar
- **Notificações**: SignalR para tempo real
- **Email**: SendGrid ou AWS SES
- **Templates**: Razor ou similar
- **Agendamento**: Cron jobs para relatórios

### Estrutura de Dados
```csharp
// Exemplo de entidade para notificações
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
```

---

**Objetivo**: Criar um sistema robusto de automações que melhore a experiência do usuário, garanta compliance e reduza trabalho manual para os gestores.