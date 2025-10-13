# 📜 Certificados Digitais

## 🔐 Configuração do Certificado

### Pasta de Certificados
Esta pasta deve conter o certificado digital para integração com SEFAZ.

### Arquivo Necessário
- **certificado.pfx**: Certificado digital e-CNPJ A1

### Configuração
1. Coloque o arquivo `certificado.pfx` nesta pasta
2. Configure a senha no `appsettings.json`:
   ```json
   {
     "SefazSettings": {
       "CertificatePath": "certificates/certificado.pfx",
       "CertificatePassword": "sua_senha_aqui"
     }
   }
   ```

### ⚠️ Segurança
- **NUNCA** commite certificados no repositório Git
- Use variáveis de ambiente para senhas em produção
- Mantenha backup seguro dos certificados

### 🛒 Onde Comprar
- **Serasa Experian**: https://certificadodigital.serasaexperian.com.br
- **Certisign**: https://www.certisign.com.br
- **Valid**: https://www.valid.com
- **Soluti**: https://www.soluti.com.br
- **AC Safeweb**: https://www.safeweb.com.br

### 📋 Documentos Necessários
- Cartão CNPJ atualizado
- Contrato Social ou última alteração
- RG/CNH + CPF do representante legal
- Procuração (se necessário)

### 💰 Preços Aproximados (2025)
- **A1 (arquivo)**: R$ 140 - R$ 300
- **A3 (token)**: R$ 220 - R$ 500