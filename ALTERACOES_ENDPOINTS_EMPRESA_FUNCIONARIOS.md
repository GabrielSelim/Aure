# Alterações nos Endpoints de Empresa e Funcionários

**Data**: 19/11/2025  
**Commit**: c018013

---

## 📋 Resumo das Alterações

Foram adicionados campos completos de endereço e informações de PJ nos endpoints para permitir a geração de contratos com todos os dados necessários.

---

## 1️⃣ Endpoint: GET /api/Companies/empresa-pai

### Objetivo
Retornar dados completos da empresa, incluindo endereço detalhado para preenchimento de contratos.

### Campos Adicionados ao Response

```json
{
  "id": "guid",
  "nome": "string",
  "cnpj": "string",
  "cnpjFormatado": "string",
  "tipo": "string",
  "modeloNegocio": "string",
  
  // ✅ NOVOS CAMPOS DE ENDEREÇO
  "rua": "string",
  "numero": "string",
  "complemento": "string | null",
  "bairro": "string",
  "cidade": "string",
  "estado": "string (2 caracteres)",
  "pais": "string",
  "cep": "string (8 dígitos)",
  "enderecoCompleto": "string",
  
  // Mantido para compatibilidade
  "endereco": {
    "rua": "string",
    "numero": "string",
    "complemento": "string | null",
    "bairro": "string",
    "cidade": "string",
    "estado": "string",
    "pais": "string",
    "cep": "string",
    "enderecoCompleto": "string"
  },
  
  "telefoneFixo": "string | null",
  "telefoneCelular": "string | null"
}
```

### Exemplo de Response

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "Empresa Teste Ltda",
  "cnpj": "12345678000199",
  "cnpjFormatado": "12.345.678/0001-99",
  "tipo": "Client",
  "modeloNegocio": "MainCompany",
  "rua": "Av. Paulista",
  "numero": "1000",
  "complemento": "Sala 1501",
  "bairro": "Bela Vista",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "01310100",
  "enderecoCompleto": "Av. Paulista, 1000, Sala 1501 - Bela Vista, São Paulo/SP, Brasil - CEP: 01310100",
  "endereco": {
    "rua": "Av. Paulista",
    "numero": "1000",
    "complemento": "Sala 1501",
    "bairro": "Bela Vista",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil",
    "cep": "01310100",
    "enderecoCompleto": "Av. Paulista, 1000, Sala 1501 - Bela Vista, São Paulo/SP, Brasil - CEP: 01310100"
  },
  "telefoneFixo": "1133334444",
  "telefoneCelular": "11987654321"
}
```

---

## 2️⃣ Endpoint: GET /api/Users/funcionarios

### Objetivo
Retornar lista de funcionários com TODOS os dados necessários para preenchimento de contratos (endereço, CPF, RG, dados da empresa PJ, etc).

### Campos Adicionados ao Response

```json
{
  "items": [
    {
      "id": "guid",
      "nome": "string",
      "email": "string",
      "role": "string",
      "cargo": "string | null",
      "status": "string",
      "dataEntrada": "datetime",
      "telefoneCelular": "string | null",
      
      // ✅ NOVOS CAMPOS
      "telefoneFixo": "string | null",
      "cpf": "string | null (descriptografado)",
      "cpfMascarado": "string | null (***.***.123-45)",
      "rg": "string | null (descriptografado)",
      "dataNascimento": "datetime | null",
      
      // Endereço do funcionário
      "rua": "string | null",
      "numero": "string | null",
      "complemento": "string | null",
      "bairro": "string | null",
      "cidade": "string | null",
      "estado": "string | null",
      "pais": "string | null",
      "cep": "string | null",
      "enderecoCompleto": "string | null",
      
      // Dados da empresa PJ (apenas para FuncionarioPJ)
      "empresaPJ": "string | null",
      "cnpjPJ": "string | null (14 dígitos)",
      "cnpjPJFormatado": "string | null (12.345.678/0001-99)",
      "razaoSocialPJ": "string | null"
    }
  ],
  "totalCount": 0,
  "pageNumber": 0,
  "pageSize": 0,
  "totalPages": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Exemplo de Response - Funcionário CLT

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "João da Silva",
      "email": "joao.silva@email.com",
      "role": "FuncionarioCLT",
      "cargo": "Desenvolvedor Full Stack",
      "status": "Ativo",
      "dataEntrada": "2025-01-15T00:00:00Z",
      "telefoneCelular": "11987654321",
      "telefoneFixo": "1133334444",
      "cpf": "12345678901",
      "cpfMascarado": "***.456.789-**",
      "rg": "123456789",
      "dataNascimento": "1990-05-10T00:00:00Z",
      "rua": "Rua das Flores",
      "numero": "123",
      "complemento": "Apto 45",
      "bairro": "Centro",
      "cidade": "São Paulo",
      "estado": "SP",
      "pais": "Brasil",
      "cep": "01310000",
      "enderecoCompleto": "Rua das Flores, 123, Apto 45 - Centro, São Paulo/SP, Brasil - CEP: 01310000",
      "empresaPJ": null,
      "cnpjPJ": null,
      "cnpjPJFormatado": null,
      "razaoSocialPJ": null
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Exemplo de Response - Funcionário PJ

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nome": "Maria Santos",
      "email": "maria@prestadora.com.br",
      "role": "FuncionarioPJ",
      "cargo": "Consultora de Marketing",
      "status": "Ativo",
      "dataEntrada": "2025-03-01T00:00:00Z",
      "telefoneCelular": "11987654321",
      "telefoneFixo": null,
      "cpf": "98765432100",
      "cpfMascarado": "***.654.321-**",
      "rg": "987654321",
      "dataNascimento": "1985-08-20T00:00:00Z",
      "rua": "Av. Brasil",
      "numero": "500",
      "complemento": null,
      "bairro": "Jardim São Paulo",
      "cidade": "São Paulo",
      "estado": "SP",
      "pais": "Brasil",
      "cep": "02011000",
      "enderecoCompleto": "Av. Brasil, 500 - Jardim São Paulo, São Paulo/SP, Brasil - CEP: 02011000",
      "empresaPJ": "MS Consultoria LTDA",
      "cnpjPJ": "12345678000199",
      "cnpjPJFormatado": "12.345.678/0001-99",
      "razaoSocialPJ": "MS Consultoria LTDA"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

## 3️⃣ Uso para Geração de Contratos

### Fluxo de Geração de Contrato

1. **Buscar dados da empresa**:
   ```http
   GET /api/Companies/empresa-pai
   Authorization: Bearer {token}
   ```

2. **Buscar lista de funcionários**:
   ```http
   GET /api/Users/funcionarios?pageSize=100
   Authorization: Bearer {token}
   ```

3. **Selecionar funcionário e preencher variáveis do template**:

### Mapeamento de Variáveis - Dados da Empresa

| Variável do Template | Campo do Response | Endpoint |
|---------------------|-------------------|----------|
| `{{NOME_EMPRESA_CONTRATANTE}}` | `nome` | `/api/Companies/empresa-pai` |
| `{{CNPJ_CONTRATANTE}}` | `cnpj` | `/api/Companies/empresa-pai` |
| `{{CNPJ_CONTRATANTE_FORMATADO}}` | `cnpjFormatado` | `/api/Companies/empresa-pai` |
| `{{ENDERECO_CONTRATANTE}}` | `rua` | `/api/Companies/empresa-pai` |
| `{{NUMERO_CONTRATANTE}}` | `numero` | `/api/Companies/empresa-pai` |
| `{{COMPLEMENTO_CONTRATANTE}}` | `complemento` | `/api/Companies/empresa-pai` |
| `{{BAIRRO_CONTRATANTE}}` | `bairro` | `/api/Companies/empresa-pai` |
| `{{CIDADE_CONTRATANTE}}` | `cidade` | `/api/Companies/empresa-pai` |
| `{{UF_CONTRATANTE}}` | `estado` | `/api/Companies/empresa-pai` |
| `{{PAIS_CONTRATANTE}}` | `pais` | `/api/Companies/empresa-pai` |
| `{{CEP_CONTRATANTE}}` | `cep` | `/api/Companies/empresa-pai` |
| `{{ENDERECO_COMPLETO_CONTRATANTE}}` | `enderecoCompleto` | `/api/Companies/empresa-pai` |
| `{{TELEFONE_CONTRATANTE}}` | `telefoneCelular` | `/api/Companies/empresa-pai` |
| `{{TELEFONE_FIXO_CONTRATANTE}}` | `telefoneFixo` | `/api/Companies/empresa-pai` |

### Mapeamento de Variáveis - Dados do Funcionário (CLT)

| Variável do Template | Campo do Response | Endpoint |
|---------------------|-------------------|----------|
| `{{NOME_CONTRATADO}}` | `nome` | `/api/Users/funcionarios` |
| `{{EMAIL_CONTRATADO}}` | `email` | `/api/Users/funcionarios` |
| `{{CPF_CONTRATADO}}` | `cpf` | `/api/Users/funcionarios` |
| `{{RG_CONTRATADO}}` | `rg` | `/api/Users/funcionarios` |
| `{{DATA_NASCIMENTO_CONTRATADO}}` | `dataNascimento` | `/api/Users/funcionarios` |
| `{{TELEFONE_CONTRATADO}}` | `telefoneCelular` | `/api/Users/funcionarios` |
| `{{TELEFONE_FIXO_CONTRATADO}}` | `telefoneFixo` | `/api/Users/funcionarios` |
| `{{CARGO_CONTRATADO}}` | `cargo` | `/api/Users/funcionarios` |
| `{{ENDERECO_CONTRATADO}}` | `rua` | `/api/Users/funcionarios` |
| `{{NUMERO_CONTRATADO}}` | `numero` | `/api/Users/funcionarios` |
| `{{COMPLEMENTO_CONTRATADO}}` | `complemento` | `/api/Users/funcionarios` |
| `{{BAIRRO_CONTRATADO}}` | `bairro` | `/api/Users/funcionarios` |
| `{{CIDADE_CONTRATADO}}` | `cidade` | `/api/Users/funcionarios` |
| `{{UF_CONTRATADO}}` | `estado` | `/api/Users/funcionarios` |
| `{{PAIS_CONTRATADO}}` | `pais` | `/api/Users/funcionarios` |
| `{{CEP_CONTRATADO}}` | `cep` | `/api/Users/funcionarios` |
| `{{ENDERECO_COMPLETO_CONTRATADO}}` | `enderecoCompleto` | `/api/Users/funcionarios` |

### Mapeamento de Variáveis - Dados do Funcionário PJ

| Variável do Template | Campo do Response | Endpoint |
|---------------------|-------------------|----------|
| `{{NOME_CONTRATADO}}` | `nome` | `/api/Users/funcionarios` |
| `{{RAZAO_SOCIAL_CONTRATADO}}` | `razaoSocialPJ` | `/api/Users/funcionarios` |
| `{{NOME_EMPRESA_CONTRATADO}}` | `empresaPJ` | `/api/Users/funcionarios` |
| `{{CNPJ_CONTRATADO}}` | `cnpjPJ` | `/api/Users/funcionarios` |
| `{{CNPJ_CONTRATADO_FORMATADO}}` | `cnpjPJFormatado` | `/api/Users/funcionarios` |
| `{{CPF_REPRESENTANTE_CONTRATADO}}` | `cpf` | `/api/Users/funcionarios` |
| `{{RG_REPRESENTANTE_CONTRATADO}}` | `rg` | `/api/Users/funcionarios` |
| `{{EMAIL_CONTRATADO}}` | `email` | `/api/Users/funcionarios` |
| `{{TELEFONE_CONTRATADO}}` | `telefoneCelular` | `/api/Users/funcionarios` |
| `{{ENDERECO_CONTRATADO}}` | `rua` | `/api/Users/funcionarios` |
| `{{NUMERO_CONTRATADO}}` | `numero` | `/api/Users/funcionarios` |
| `{{COMPLEMENTO_CONTRATADO}}` | `complemento` | `/api/Users/funcionarios` |
| `{{BAIRRO_CONTRATADO}}` | `bairro` | `/api/Users/funcionarios` |
| `{{CIDADE_CONTRATADO}}` | `cidade` | `/api/Users/funcionarios` |
| `{{UF_CONTRATADO}}` | `estado` | `/api/Users/funcionarios` |
| `{{CEP_CONTRATADO}}` | `cep` | `/api/Users/funcionarios` |
| `{{ENDERECO_COMPLETO_CONTRATADO}}` | `enderecoCompleto` | `/api/Users/funcionarios` |

---

## 4️⃣ Exemplo de Código - Auto-preenchimento

### JavaScript/TypeScript

```typescript
// 1. Buscar dados da empresa
const empresaResponse = await fetch('/api/Companies/empresa-pai', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const empresa = await empresaResponse.json();

// 2. Buscar funcionários
const funcionariosResponse = await fetch('/api/Users/funcionarios?pageSize=100', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const { items: funcionarios } = await funcionariosResponse.json();

// 3. Usuário seleciona um funcionário
const funcionarioSelecionado = funcionarios.find(f => f.id === funcionarioId);

// 4. Auto-preencher variáveis do contrato
const dadosPreenchimento = {
  // Dados da Empresa Contratante
  NOME_EMPRESA_CONTRATANTE: empresa.nome,
  CNPJ_CONTRATANTE: empresa.cnpj,
  CNPJ_CONTRATANTE_FORMATADO: empresa.cnpjFormatado,
  ENDERECO_CONTRATANTE: empresa.rua,
  NUMERO_CONTRATANTE: empresa.numero,
  COMPLEMENTO_CONTRATANTE: empresa.complemento || '',
  BAIRRO_CONTRATANTE: empresa.bairro,
  CIDADE_CONTRATANTE: empresa.cidade,
  UF_CONTRATANTE: empresa.estado,
  CEP_CONTRATANTE: empresa.cep,
  ENDERECO_COMPLETO_CONTRATANTE: empresa.enderecoCompleto,
  TELEFONE_CONTRATANTE: empresa.telefoneCelular,
  
  // Dados do Contratado
  NOME_CONTRATADO: funcionarioSelecionado.nome,
  EMAIL_CONTRATADO: funcionarioSelecionado.email,
  CPF_CONTRATADO: funcionarioSelecionado.cpf,
  RG_CONTRATADO: funcionarioSelecionado.rg,
  TELEFONE_CONTRATADO: funcionarioSelecionado.telefoneCelular,
  CARGO_CONTRATADO: funcionarioSelecionado.cargo,
  ENDERECO_CONTRATADO: funcionarioSelecionado.rua,
  NUMERO_CONTRATADO: funcionarioSelecionado.numero,
  COMPLEMENTO_CONTRATADO: funcionarioSelecionado.complemento || '',
  BAIRRO_CONTRATADO: funcionarioSelecionado.bairro,
  CIDADE_CONTRATADO: funcionarioSelecionado.cidade,
  UF_CONTRATADO: funcionarioSelecionado.estado,
  CEP_CONTRATADO: funcionarioSelecionado.cep,
  ENDERECO_COMPLETO_CONTRATADO: funcionarioSelecionado.enderecoCompleto,
  
  // Se for PJ, adicionar dados da empresa PJ
  ...(funcionarioSelecionado.role === 'FuncionarioPJ' && {
    RAZAO_SOCIAL_CONTRATADO: funcionarioSelecionado.razaoSocialPJ,
    CNPJ_CONTRATADO: funcionarioSelecionado.cnpjPJ,
    CNPJ_CONTRATADO_FORMATADO: funcionarioSelecionado.cnpjPJFormatado,
  })
};

// 5. Gerar contrato
const contratoResponse = await fetch('/api/ContractTemplates/gerar-contrato', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    templateId: templateId,
    contractId: null, // ou ID do contrato PJ
    dadosPreenchimento: dadosPreenchimento,
    gerarPdf: true
  })
});
```

---

## 5️⃣ Observações Importantes

### Segurança

1. **CPF/RG Descriptografados**: 
   - Esses campos são retornados descriptografados no endpoint `/api/Users/funcionarios`
   - São necessários para geração de contratos
   - ⚠️ **Atenção**: Certifique-se de que o frontend não armazena esses dados em localStorage/sessionStorage
   - Use apenas em memória durante a geração do contrato

2. **CPF Mascarado**:
   - O campo `cpfMascarado` está disponível para exibição em listagens
   - Formato: `***.456.789-**`

### Retrocompatibilidade

1. **Endpoint `/api/Companies/empresa-pai`**:
   - Mantido o campo `endereco` como objeto para compatibilidade
   - Adicionados campos individuais (`rua`, `numero`, etc.) para facilitar o uso
   - Ambos contêm os mesmos dados

### Performance

1. **Paginação**:
   - O endpoint `/api/Users/funcionarios` continua paginado
   - Para gerar contratos, recomenda-se buscar com `pageSize=100` ou usar busca por nome

2. **Dados Sensíveis**:
   - CPF/RG são descriptografados on-demand
   - Isso pode impactar performance em listagens grandes
   - Considere adicionar cache se necessário

---

## 6️⃣ Checklist de Implementação Frontend

### Página de Geração de Contrato

- [ ] Buscar dados da empresa ao carregar página
- [ ] Buscar lista de funcionários
- [ ] Permitir seleção de funcionário (autocomplete)
- [ ] Auto-preencher campos do formulário com dados do funcionário
- [ ] Verificar se é PJ e exibir campos adicionais
- [ ] Validar que todos os campos obrigatórios estão preenchidos
- [ ] Enviar para `/api/ContractTemplates/gerar-contrato`
- [ ] Limpar dados sensíveis (CPF/RG) da memória após geração

### Listagem de Funcionários

- [ ] Exibir CPF mascarado na listagem
- [ ] Não exibir CPF/RG completos em cards/tabelas
- [ ] Adicionar botão "Gerar Contrato" por funcionário
- [ ] Filtrar por tipo (CLT/PJ)

---

## 7️⃣ Testes Necessários

### Testes Manuais

```bash
# 1. Buscar dados da empresa
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Companies/empresa-pai" \
  -H "Authorization: Bearer {seu-token}"

# 2. Listar funcionários
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Users/funcionarios?pageSize=20" \
  -H "Authorization: Bearer {seu-token}"

# 3. Listar apenas PJs
curl -X GET "https://aureapi.gabrielsanztech.com.br/api/Users/funcionarios?role=FuncionarioPJ" \
  -H "Authorization: Bearer {seu-token}"
```

### Verificações

- [ ] Endpoint `/api/Companies/empresa-pai` retorna todos os campos de endereço
- [ ] Endpoint `/api/Users/funcionarios` retorna CPF descriptografado
- [ ] Endpoint `/api/Users/funcionarios` retorna CPF mascarado
- [ ] Para PJs, campos `cnpjPJ`, `cnpjPJFormatado`, `razaoSocialPJ` estão preenchidos
- [ ] Para CLTs, campos PJ são `null`
- [ ] Endereço completo é formatado corretamente

---

## 8️⃣ Próximos Passos

1. ✅ Deploy do backend em produção
2. ⏳ Implementar frontend de geração de contratos
3. ⏳ Testar geração de contratos com dados reais
4. ⏳ Validar PDFs gerados

---

**Documentação Completa**: Consulte `DOCUMENTACAO_ENDPOINTS_CONTRATOS.md` e `PROMPT_FRONTEND_CONTRATOS.md`
