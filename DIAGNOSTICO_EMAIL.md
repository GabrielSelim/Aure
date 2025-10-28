# 🔍 Diagnóstico de Problemas com Email SMTP Gmail

## ❌ Erro Atual
```
535: 5.7.8 Username and Password not accepted
```

## 📋 Checklist de Verificação

### 1. Verificar App Password do Gmail

#### ✅ Passos para Criar App Password Correta:

1. **Acesse sua conta Google**: https://myaccount.google.com/
2. **Verificação em 2 Etapas**: 
   - Vá para "Segurança" → "Verificação em duas etapas"
   - **DEVE estar ATIVADA** (se não estiver, ative primeiro)
3. **Criar App Password**:
   - Acesse: https://myaccount.google.com/apppasswords
   - Se não aparecer, é porque:
     - Verificação em 2 etapas não está ativa
     - Você está usando conta do Workspace (não pessoal)
     - Conta tem restrições de segurança

4. **Gerar Nova Senha**:
   - Selecione "Aplicativo": **Email**
   - Selecione "Dispositivo": **Outro (nome personalizado)**
   - Digite: **Aure API**
   - Clique em **Gerar**

5. **Copiar Senha**:
   - Google mostra: `abcd efgh ijkl mnop` (COM espaços)
   - **IMPORTANTE**: Remover TODOS os espaços
   - Use: `abcdefghijklmnop` (SEM espaços)

---

### 2. Problemas Comuns

#### ❌ Senha Normal vs App Password
- **NÃO funciona**: Senha normal da conta Gmail
- **FUNCIONA**: App Password gerada especificamente

#### ❌ Verificação em 2 Etapas Desativada
- App Password **só funciona** se 2FA estiver ativo
- Ativar em: https://myaccount.google.com/signinoptions/two-step-verification

#### ❌ Conta Google Workspace
- Algumas contas corporativas não permitem App Passwords
- Solução: Usar conta Gmail pessoal

#### ❌ Senha com Espaços
- Google mostra: `dezj dsaf deiw veai`
- Correto: `dezjdsafdeiwveai`
- **Sem espaços, sem traços, sem nada!**

---

### 3. Alternativas de Teste

Se continuar falhando, você pode:

#### Opção A: Usar Mailtrap (Desenvolvimento)
```json
"EmailSettings": {
  "SmtpHost": "smtp.mailtrap.io",
  "SmtpPort": 587,
  "Username": "seu-username-mailtrap",
  "Password": "seu-password-mailtrap",
  "FromEmail": "noreply@aure.com",
  "FromName": "Sistema Aure"
}
```
- Criar conta grátis: https://mailtrap.io
- Emails ficam em inbox virtual (não envia de verdade)
- Perfeito para testes!

#### Opção B: Usar SendGrid (Produção)
```json
"EmailSettings": {
  "SmtpHost": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "Username": "apikey",
  "Password": "SG.sua-api-key-aqui",
  "FromEmail": "noreply@aure.com",
  "FromName": "Sistema Aure"
}
```
- 100 emails grátis por dia
- Mais confiável que Gmail
- Criar conta: https://sendgrid.com

#### Opção C: Usar Outlook/Hotmail
```json
"EmailSettings": {
  "SmtpHost": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "Username": "seu-email@outlook.com",
  "Password": "sua-senha-outlook",
  "FromEmail": "seu-email@outlook.com",
  "FromName": "Sistema Aure"
}
```

---

### 4. Teste Manual da Senha

Para verificar se a senha está correta, você pode testar manualmente:

```powershell
# Instalar ferramenta de teste SMTP
Install-Package -Name MailKit

# Ou usar telnet (mais complexo)
```

Ou use um site de teste: https://www.gmass.co/smtp-test

---

### 5. Verificar Logs do Google

1. Acesse: https://myaccount.google.com/notifications
2. Procure por tentativas de login bloqueadas
3. Se houver, permita o acesso

---

## 🔧 Solução Rápida

### Passo 1: Deletar App Password Antiga
1. Acesse: https://myaccount.google.com/apppasswords
2. Delete a senha "aure" existente

### Passo 2: Criar Nova App Password
1. Clique em "Criar senha de app"
2. Escolha "Email" e "Outro"
3. Digite: "Aure API Nova"
4. **COPIE A SENHA SEM ESPAÇOS**

### Passo 3: Atualizar Configuração
Me informe a nova senha e eu atualizo os arquivos!

---

## 📊 Status Atual

**Conta**: aurecontroll@gmail.com  
**Senha Atual no Sistema**: `dezjdsafdeiwveai`  
**Status**: ❌ Autenticação falhando  

**Possíveis Causas**:
1. ❓ Verificação em 2 etapas não está ativa
2. ❓ App Password foi deletada/revogada no Google
3. ❓ Senha foi copiada incorretamente (com espaços invisíveis)
4. ❓ Conta tem restrições de segurança

---

## 🎯 Próximos Passos

1. **Verificar se 2FA está ativo**: https://myaccount.google.com/security
2. **Criar nova App Password**: https://myaccount.google.com/apppasswords
3. **Me informar a nova senha** (sem espaços!)
4. Eu atualizo e testo novamente

Ou

**Usar Mailtrap** para testes (mais fácil e rápido!)
