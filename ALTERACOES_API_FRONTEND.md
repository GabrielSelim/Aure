# Alterações na API - Novos Campos Implementados

**Data**: 24/11/2025  
**Versão da API**: Atualizada com novos campos para Contratos PJ

---

## 📋 Resumo das Alterações

Foram adicionados novos campos nas entidades **Company** (Empresa) e **User** (Usuário) para suportar a geração completa de contratos PJ com todas as informações legais necessárias.

---

## 🏢 Novos Campos - COMPANY (Empresa)

### Campos Adicionados:
1. **`nire`** (string, nullable)
   - Número de Identificação do Registro de Empresa
   - Usado em contratos para identificação legal da empresa
   - Exemplo: `"35209471101"`

2. **`inscricaoEstadual`** (string, nullable)
   - Inscrição Estadual da empresa
   - Usado para informações fiscais em contratos
   - Exemplo: `"123.456.789.012"`

### Endpoints Afetados:

#### ✅ GET `/api/Companies/empresa-pai`
**Resposta atualizada:**
```json
{
  "id": "uuid",
  "razaoSocial": "string",
  "cnpj": "string",
  "companyType": 0,
  "businessModel": 0,
  "rua": "string",
  "numero": "string",
  "complemento": "string",
  "bairro": "string",
  "cidade": "string",
  "estado": "string",
  "pais": "string",
  "cep": "string",
  "enderecoCompleto": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "nire": "string",                    // ⬅️ NOVO
  "inscricaoEstadual": "string",       // ⬅️ NOVO
  "totalFuncionarios": 0,
  "contratosAtivos": 0,
  "dataCadastro": "2025-11-24T00:00:00Z"
}
```

#### ✅ GET `/api/UserProfile/empresa`
**Resposta atualizada:**
```json
{
  "id": "uuid",
  "nome": "string",
  "cnpj": "string",
  "cnpjFormatado": "string",
  "tipo": "string",
  "modeloNegocio": "string",
  "rua": "string",
  "numero": "string",
  "complemento": "string",
  "bairro": "string",
  "cidade": "string",
  "estado": "string",
  "pais": "string",
  "cep": "string",
  "enderecoCompleto": "string",
  "telefoneFixo": "string",
  "telefoneCelular": "string",
  "nire": "string",                    // ⬅️ NOVO
  "inscricaoEstadual": "string"        // ⬅️ NOVO
}
```

#### ✅ PUT `/api/UserProfile/empresa` (Apenas DonoEmpresaPai)
**Request atualizado:**
```json
{
  "nome": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "rua": "string",
  "numero": "string",
  "complemento": "string",
  "bairro": "string",
  "cidade": "string",
  "estado": "string",
  "pais": "string",
  "cep": "string",
  "nire": "string",                    // ⬅️ NOVO (opcional)
  "inscricaoEstadual": "string"        // ⬅️ NOVO (opcional)
}
```

---

## 👤 Novos Campos - USER (Usuário)

### Campos Adicionados:
1. **`orgaoExpedidorRG`** (string, nullable)
   - Órgão que expediu o RG (ex: "SSP", "SSP/SP", "Detran")
   - Usado em contratos para identificação completa
   - Exemplo: `"SSP"`

2. **`nacionalidade`** (string, nullable)
   - Nacionalidade do usuário
   - Usado em contratos para dados pessoais
   - Exemplo: `"Brasileiro(a)"`, `"Portuguesa"`

3. **`estadoCivil`** (string, nullable)
   - Estado civil do usuário
   - Usado em contratos para dados pessoais
   - Valores comuns: `"Solteiro(a)"`, `"Casado(a)"`, `"Divorciado(a)"`, `"Viúvo(a)"`, `"União Estável"`

### Endpoints Afetados:

#### ✅ GET `/api/UserProfile/perfil-completo`
**Resposta atualizada:**
```json
{
  "id": "uuid",
  "nome": "string",
  "email": "string",
  "role": 0,
  "roleDescricao": "string",
  "avatarUrl": "string",
  "dataNascimento": "2000-01-01T00:00:00Z",
  "cpfMascarado": "string",
  "cpf": "string",
  "rg": "string",
  "orgaoExpedidorRG": "string",        // ⬅️ NOVO
  "nacionalidade": "string",           // ⬅️ NOVO
  "estadoCivil": "string",             // ⬅️ NOVO
  "cargo": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "enderecoRua": "string",
  "enderecoNumero": "string",
  "enderecoComplemento": "string",
  "enderecoBairro": "string",
  "enderecoCidade": "string",
  "enderecoEstado": "string",
  "enderecoPais": "string",
  "enderecoCep": "string",
  "enderecoCompleto": "string",
  "aceitouTermosUso": true,
  "dataAceiteTermosUso": "2025-11-24T00:00:00Z",
  "versaoTermosUsoAceita": "string",
  "aceitouPoliticaPrivacidade": true,
  "dataAceitePoliticaPrivacidade": "2025-11-24T00:00:00Z",
  "versaoPoliticaPrivacidadeAceita": "string"
}
```

#### ✅ PUT `/api/UserProfile/perfil-completo`
**Request atualizado:**
```json
{
  "nome": "string",
  "email": "string",
  "dataNascimento": "2000-01-01T00:00:00Z",
  "cpf": "string",
  "rg": "string",
  "orgaoExpedidorRG": "string",        // ⬅️ NOVO (opcional)
  "nacionalidade": "string",           // ⬅️ NOVO (opcional)
  "estadoCivil": "string",             // ⬅️ NOVO (opcional)
  "cargo": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "enderecoRua": "string",
  "enderecoNumero": "string",
  "enderecoComplemento": "string",
  "enderecoBairro": "string",
  "enderecoCidade": "string",
  "enderecoEstado": "string",
  "enderecoPais": "string",
  "enderecoCep": "string",
  "senhaAtual": "string",
  "novaSenha": "string"
}
```

#### ✅ GET `/api/Contracts/funcionarios-internos`
**Resposta atualizada (cada item do array):**
```json
{
  "id": "uuid",
  "nome": "string",
  "email": "string",
  "cargo": "string",
  "role": "string",
  "cpf": "string",
  "rg": "string",
  "orgaoExpedidorRG": "string",        // ⬅️ NOVO
  "nacionalidade": "string",           // ⬅️ NOVO
  "estadoCivil": "string",             // ⬅️ NOVO
  "dataNascimento": "2000-01-01T00:00:00Z",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "endereco": {
    "rua": "string",
    "numero": "string",
    "complemento": "string",
    "bairro": "string",
    "cidade": "string",
    "estado": "string",
    "pais": "string",
    "cep": "string",
    "enderecoCompleto": "string"
  },
  "dataCadastro": "2025-11-24T00:00:00Z"
}
```

#### ✅ GET `/api/Contracts/funcionarios-pj`
**Resposta atualizada (cada item do array):**
```json
{
  "id": "uuid",
  "nome": "string",
  "email": "string",
  "cargo": "string",
  "cpf": "string",
  "rg": "string",
  "orgaoExpedidorRG": "string",        // ⬅️ NOVO
  "nacionalidade": "string",           // ⬅️ NOVO
  "estadoCivil": "string",             // ⬅️ NOVO
  "dataNascimento": "2000-01-01T00:00:00Z",
  "telefoneCelular": "string",
  "telefoneFixo": "string",
  "endereco": { /* ... */ },
  "empresaPJ": { /* ... */ },
  "dataCadastro": "2025-11-24T00:00:00Z"
}
```

---

## 🎯 Impacto nos Templates de Contratos

Os novos campos são usados automaticamente nos templates de contratos:

### Variáveis do Template - Empresa Contratante:
- `{{NIRE_CONTRATANTE}}` → Preenchido com `company.nire`
- `{{ESTADO_REGISTRO_CONTRATANTE}}` → Preenchido com `company.estado` (estado de registro)

### Variáveis do Template - Representante:
- `{{ORGAO_EXPEDIDOR_REPRESENTANTE}}` → Preenchido com `user.orgaoExpedidorRG` (padrão: "SSP")
- `{{NACIONALIDADE_REPRESENTANTE}}` → Preenchido com `user.nacionalidade` (padrão: "Brasileiro(a)")
- `{{ESTADO_CIVIL_REPRESENTANTE}}` → Preenchido com `user.estadoCivil`

### Variáveis do Template - Contratado PJ:
- `{{NACIONALIDADE_CONTRATADO}}` → Preenchido com dados do PJ ou manual
- `{{ESTADO_CIVIL_CONTRATADO}}` → Preenchido com dados do PJ ou manual

---

## ✅ Checklist de Atualização - Frontend

### Formulários a Atualizar:

#### 1. **Perfil do Usuário** (`/perfil`, `/configuracoes`)
- [ ] Adicionar campo "Órgão Expedidor RG" (text input, opcional)
  - Label: "Órgão Expedidor do RG"
  - Placeholder: "Ex: SSP, SSP/SP, Detran"
  - Máximo: 20 caracteres
  
- [ ] Adicionar campo "Nacionalidade" (text input ou select, opcional)
  - Label: "Nacionalidade"
  - Placeholder: "Ex: Brasileiro(a)"
  - Sugestões: "Brasileiro(a)", "Portuguesa", "Americana", etc.
  - Máximo: 50 caracteres
  
- [ ] Adicionar campo "Estado Civil" (select, opcional)
  - Label: "Estado Civil"
  - Opções: 
    * "Solteiro(a)"
    * "Casado(a)"
    * "Divorciado(a)"
    * "Viúvo(a)"
    * "União Estável"
  - Máximo: 30 caracteres

#### 2. **Dados da Empresa** (`/empresa`, `/configuracoes/empresa`)
- [ ] Adicionar campo "NIRE" (text input, opcional)
  - Label: "NIRE - Número de Identificação do Registro de Empresa"
  - Placeholder: "Ex: 35209471101"
  - Máximo: 20 caracteres
  - Info: "Número de registro da empresa na Junta Comercial"
  
- [ ] Adicionar campo "Inscrição Estadual" (text input, opcional)
  - Label: "Inscrição Estadual"
  - Placeholder: "Ex: 123.456.789.012"
  - Máximo: 50 caracteres
  - Info: "Número de inscrição estadual para fins fiscais"

#### 3. **Lista de Funcionários** (`/funcionarios`)
- [ ] Adicionar colunas opcionais na tabela:
  - "Nacionalidade" (exibir se preenchido)
  - "Estado Civil" (exibir se preenchido)
  
- [ ] Adicionar na visualização detalhada:
  - Órgão Expedidor do RG
  - Nacionalidade
  - Estado Civil

#### 4. **Preview/Geração de Contratos**
- [ ] Verificar se os campos são exibidos corretamente no preview
- [ ] Confirmar que os placeholders são substituídos pelos valores reais

---

## 🚨 Campos Opcionais vs Obrigatórios

**IMPORTANTE**: Todos os novos campos são **OPCIONAIS**. Não bloqueiam:
- ✅ Criação de conta
- ✅ Atualização de perfil
- ✅ Geração de contratos

**Valores Padrão no Template**:
- `orgaoExpedidorRG`: Se vazio, usa `"SSP"`
- `nacionalidade`: Se vazio, usa `"Brasileiro(a)"`
- `estadoCivil`: Se vazio, fica em branco no contrato
- `nire`: Se vazio, fica em branco no contrato
- `inscricaoEstadual`: Se vazio, fica em branco no contrato

---

## 🔧 Validações Frontend Sugeridas

### Órgão Expedidor RG:
```javascript
// Validação simples
maxLength: 20
pattern: /^[A-Z0-9\/\s]+$/  // Apenas letras maiúsculas, números, barra e espaço
```

### Nacionalidade:
```javascript
// Validação simples
maxLength: 50
pattern: /^[A-Za-zÀ-ÿ\s\(\)]+$/  // Letras, espaços e parênteses
```

### Estado Civil:
```javascript
// Select com opções fixas
options: [
  "Solteiro(a)",
  "Casado(a)",
  "Divorciado(a)",
  "Viúvo(a)",
  "União Estável"
]
```

### NIRE:
```javascript
// Validação simples
maxLength: 20
pattern: /^[0-9]+$/  // Apenas números
```

### Inscrição Estadual:
```javascript
// Validação simples
maxLength: 50
pattern: /^[0-9\.\-\/]+$/  // Números, pontos, traços e barras
```

---

## 📦 Migrações de Dados

**Dados Existentes**: Todos os registros existentes terão esses campos como `null`. Isso é **esperado e seguro**.

**Quando Popular**:
- Usuários podem preencher quando quiserem
- Ideal preencher antes de gerar contratos para ter documentos mais completos
- Sistema funciona normalmente mesmo sem esses dados

---

## 🐛 Possíveis Problemas e Soluções

### Problema: Contrato com campos vazios
**Causa**: Dados não preenchidos pelo usuário  
**Solução**: Adicionar banner/alerta sugerindo preenchimento dos dados opcionais antes de gerar contrato

### Problema: API retorna 500 em `/api/Companies/empresa-pai`
**Causa**: Migration não aplicada no banco de produção  
**Solução**: ✅ **JÁ CORRIGIDO** - Migrations aplicadas manualmente

### Problema: Campos não aparecem no frontend
**Causa**: Frontend não atualizado para os novos campos  
**Solução**: Atualizar interfaces TypeScript e componentes conforme este documento

---

## 📞 Sistema Legado - ContractTemplates

### ⚠️ Status: **AINDA EM USO** (NÃO REMOVER)

O sistema antigo `ContractTemplates` (com HTML completo no banco) **ainda está em uso** pelo `ContractDocumentsController`.

**Endpoints ativos:**
- `GET /api/ContractTemplates` - Lista templates do banco
- `GET /api/ContractDocuments/contract/{id}` - Lista documentos de contrato
- Outros endpoints relacionados

**Novo Sistema** (`ContractTemplateConfig`):
- Usa arquivo físico `ContratoPrestacaoServicosGenerico.html`
- Configurações salvas no banco (não o HTML completo)
- Endpoints: `/api/ContractTemplateConfig/*`

**Recomendação**: Manter ambos sistemas até migração completa dos contratos para o novo sistema.

---

## 🎉 Resumo para Desenvolvedores Frontend

### O que mudou?
✅ 5 novos campos opcionais adicionados (3 em User, 2 em Company)  
✅ Todos os endpoints de perfil e empresa atualizados  
✅ Templates de contratos agora preenchem esses campos automaticamente  

### O que preciso fazer?
1. Adicionar 5 campos nos formulários (3 no perfil, 2 na empresa)
2. Atualizar interfaces TypeScript para incluir os novos campos
3. Testar preview de contratos para garantir que os dados aparecem
4. Adicionar validações básicas (opcional mas recomendado)

### Prazo recomendado:
- **Crítico**: Adicionar campos nos formulários (1-2 dias)
- **Importante**: Atualizar visualizações e listas (2-3 dias)
- **Desejável**: Adicionar validações e mensagens de orientação (1 dia)

---

## 📚 Documentação Técnica Completa

**Migrations Aplicadas**:
- `20251124190947_AdicionarCamposRegistroEmpresaEDadosPessoais`
- `20251124185124_AdicionarCamposContratoPJ`

**Arquivos Modificados**:
- `User.cs` - Adicionados campos e métodos
- `Company.cs` - Adicionados campos e métodos
- `UserProfileService.cs` - Atualizado para salvar novos campos
- `CompanyService.cs` - Atualizado para retornar novos campos
- `ContractTemplateConfigService.cs` - Usa novos campos em templates
- Diversos DTOs atualizados

**Commits Relevantes**:
- `feat: adicionar campos NIRE, nacionalidade, estado civil e órgão expedidor para contratos completos`
- `feat: adicionar novos campos aos DTOs de resposta e atualização`
- `feat: adicionar novos campos aos DTOs de funcionários internos e PJ`
- `fix: corrigir busca de empresa pai por CompanyId do usuário`

---

**Última Atualização**: 24/11/2025 às 20:50  
**Versão do Documento**: 1.0  
**Status**: ✅ Pronto para implementação no Frontend
