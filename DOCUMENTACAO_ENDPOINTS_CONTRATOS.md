# Documentação - Sistema de Templates e Documentos de Contratos

## Visão Geral

O sistema permite gerenciar **templates de contratos** que podem ser usados para gerar **documentos de contratos** preenchidos. Existem dois tipos de templates:

1. **Templates da Empresa**: Criados e gerenciados por cada empresa individualmente
2. **Templates do Sistema**: Templates oficiais compartilhados entre todas as empresas (marcados com `ehSistema: true`)

## Arquitetura

```
Templates de Contratos
├── Templates da Empresa (company_id específico)
│   ├── Podem ser editados
│   ├── Podem ser definidos como padrão
│   └── Podem ser desativados
│
└── Templates do Sistema (company_id = NULL)
    ├── NÃO podem ser editados (ehSistema: true)
    ├── NÃO podem ser deletados
    ├── Acessíveis por TODAS as empresas
    └── Apenas visualização e uso para gerar contratos
```

## Fluxo de Uso

### 1. Listar Templates Disponíveis
Usuário acessa a tela de contratos e vê:
- Templates da própria empresa
- Templates do sistema (compartilhados)

### 2. Criar Contrato a partir de Template
Usuário escolhe um template e preenche as variáveis dinâmicas para gerar o contrato final

### 3. Gerenciar Templates Próprios
Empresa pode criar, editar e desativar seus próprios templates (mas não os do sistema)

---

## Endpoints Disponíveis

### 📋 Templates de Contratos

#### 1. **GET** `/api/ContractTemplates`
**Descrição**: Lista todos os templates disponíveis (da empresa + do sistema)

**Autenticação**: Obrigatória

**Query Parameters**:
- `apenasAtivos` (bool, opcional): Filtra apenas templates ativos. Default: `true`

**Response 200**:
```json
[
  {
    "id": "uuid",
    "nome": "Contrato de Prestacao de Servicos de Gestao - SISTEMA",
    "descricao": "Template oficial do sistema disponivel para todas as empresas",
    "tipo": "PrestacaoServicoPJ",
    "ehPadrao": false,
    "ehSistema": true,
    "ativo": true,
    "quantidadeVariaveis": 56,
    "createdAt": "2025-11-18T10:30:00Z"
  },
  {
    "id": "uuid",
    "nome": "Meu Template Personalizado",
    "descricao": "Template customizado da minha empresa",
    "tipo": "PrestacaoServicoPJ",
    "ehPadrao": true,
    "ehSistema": false,
    "ativo": true,
    "quantidadeVariaveis": 30,
    "createdAt": "2025-11-10T15:20:00Z"
  }
]
```

**Comportamento**:
- Retorna templates da empresa do usuário logado
- Retorna também templates do sistema (ehSistema: true)
- Templates do sistema aparecem para todas as empresas

---

#### 2. **GET** `/api/ContractTemplates/{id}`
**Descrição**: Obtém detalhes completos de um template específico

**Autenticação**: Obrigatória

**Response 200**:
```json
{
  "id": "uuid",
  "nome": "Contrato de Prestacao de Servicos de Gestao - SISTEMA",
  "descricao": "Template oficial do sistema...",
  "tipo": "PrestacaoServicoPJ",
  "conteudoHtml": "<html>...{{NOME_EMPRESA_CONTRATANTE}}...</html>",
  "variaveisDisponiveis": [
    "NOME_EMPRESA_CONTRATANTE",
    "CNPJ_CONTRATANTE",
    "ENDERECO_CONTRATANTE",
    "NUMERO_CONTRATANTE",
    "...56 variáveis no total"
  ],
  "ehPadrao": false,
  "ehSistema": true,
  "podeEditar": false,
  "podeDeletar": false,
  "ativo": true,
  "createdAt": "2025-11-18T10:30:00Z",
  "updatedAt": "2025-11-18T10:30:00Z"
}
```

**Response 401**: Se tentar acessar template de outra empresa (e não for template do sistema)

**Response 404**: Template não encontrado

---

#### 3. **POST** `/api/ContractTemplates`
**Descrição**: Cria um novo template personalizado da empresa

**Autenticação**: Obrigatória (Roles: DonoEmpresaPai, Juridico)

**Request Body**:
```json
{
  "nome": "Meu Template Customizado",
  "descricao": "Template para contratos específicos da empresa",
  "tipo": "PrestacaoServicoPJ",
  "conteudoHtml": "<html>Contrato entre {{CONTRATANTE}} e {{CONTRATADO}}...</html>",
  "variaveisDisponiveis": [
    "CONTRATANTE",
    "CONTRATADO",
    "VALOR",
    "DATA"
  ],
  "definirComoPadrao": false,
  "conteudoDocxBase64": null
}
```

**Tipos disponíveis**:
- `PrestacaoServicoPJ`
- `PrestacaoServicoCLT`
- `Confidencialidade`
- `Consultoria`
- `Outros`

**Response 201**: Template criado com sucesso
**Response 400**: Validação falhou
**Response 401**: Não autorizado

---

#### 4. **PUT** `/api/ContractTemplates/{id}`
**Descrição**: Atualiza um template existente da empresa

**Autenticação**: Obrigatória (Roles: DonoEmpresaPai, Juridico)

**Request Body**:
```json
{
  "nome": "Nome Atualizado",
  "descricao": "Nova descrição",
  "conteudoHtml": "<html>Novo conteúdo...</html>",
  "variaveisDisponiveis": ["VAR1", "VAR2"]
}
```

**Response 200**: Template atualizado
**Response 400**: Se tentar editar template do sistema (ehSistema: true)
**Response 404**: Template não encontrado

**⚠️ IMPORTANTE**: Templates do sistema (ehSistema: true) **NÃO podem ser editados**

---

#### 5. **GET** `/api/ContractTemplates/padrao/{tipo}`
**Descrição**: Obtém o template padrão da empresa para um tipo específico

**Autenticação**: Obrigatória

**Path Parameter**:
- `tipo`: PrestacaoServicoPJ, PrestacaoServicoCLT, etc.

**Response 200**: Template padrão encontrado
**Response 404**: Nenhum template padrão para este tipo

---

#### 6. **POST** `/api/ContractTemplates/{id}/definir-padrao`
**Descrição**: Define um template como padrão para seu tipo

**Autenticação**: Obrigatória (Role: DonoEmpresaPai)

**Response 200**: Template definido como padrão
**Response 400**: Já existe outro template padrão para este tipo

---

#### 7. **POST** `/api/ContractTemplates/{id}/remover-padrao`
**Descrição**: Remove a marcação de padrão de um template

**Autenticação**: Obrigatória (Role: DonoEmpresaPai)

**Response 200**: Padrão removido com sucesso

---

#### 8. **POST** `/api/ContractTemplates/{id}/ativar`
**Descrição**: Ativa um template desativado

**Autenticação**: Obrigatória (Roles: DonoEmpresaPai, Juridico)

**Response 200**: Template ativado

---

#### 9. **POST** `/api/ContractTemplates/{id}/desativar`
**Descrição**: Desativa um template

**Autenticação**: Obrigatória (Roles: DonoEmpresaPai, Juridico)

**Request Body**: `"Motivo da desativação"`

**Response 200**: Template desativado
**Response 400**: Se tentar desativar template do sistema

---

#### 10. **GET** `/api/ContractTemplates/variaveis-disponiveis`
**Descrição**: Lista todas as variáveis disponíveis para uso em templates

**Autenticação**: Obrigatória

**Response 200**:
```json
{
  "variaveis": [
    {
      "nome": "{{NOME_CONTRATANTE}}",
      "descricao": "Nome da empresa contratante",
      "exemplo": "Tech Solutions LTDA",
      "categoria": "Empresa"
    },
    {
      "nome": "{{CNPJ_CONTRATANTE}}",
      "descricao": "CNPJ da empresa contratante",
      "exemplo": "12.345.678/0001-90",
      "categoria": "Empresa"
    }
  ]
}
```

---

#### 11. **POST** `/api/ContractTemplates/gerar-contrato`
**Descrição**: Gera um documento de contrato preenchido a partir de um template

**Autenticação**: Obrigatória (Roles: DonoEmpresaPai, Juridico)

**Request Body**:
```json
{
  "templateId": "uuid-do-template",
  "contractId": "uuid-do-contrato-pj",
  "dadosPreenchimento": {
    "NOME_EMPRESA_CONTRATANTE": "EXBE TECNOLOGIA E SERVICOS LTDA",
    "CNPJ_CONTRATANTE": "47.700.845/0001-53",
    "ENDERECO_CONTRATANTE": "Rua das Flores",
    "NUMERO_CONTRATANTE": "123",
    "NOME_CONTRATADO": "SAUL VICTOR FRANCO DE SOUZA",
    "CPF_CONTRATADO": "024.110.262-69",
    "VALOR_MENSAL": "R$ 9.950,00",
    "VALOR_MENSAL_EXTENSO": "nove mil, novecentos e cinquenta Reais",
    "DATA_ASSINATURA": "18 de novembro de 2025"
  },
  "gerarPdf": true
}
```

**Response 201**:
```json
{
  "id": "uuid-do-documento",
  "contractId": "uuid-do-contrato",
  "templateId": "uuid-do-template",
  "templateNome": "Contrato de Prestacao de Servicos...",
  "conteudoHtmlPreenchido": "<html>Contrato preenchido...</html>",
  "conteudoPdfBase64": "JVBERi0xLjQKJeLjz9MK...",
  "versao": "1.0",
  "assinadoPor": null,
  "assinadoEm": null,
  "createdAt": "2025-11-18T12:00:00Z"
}
```

**Fluxo**:
1. Sistema busca o template (pode ser da empresa ou do sistema)
2. Substitui todas as variáveis `{{VARIAVEL}}` pelos valores fornecidos
3. Gera HTML preenchido
4. Se `gerarPdf: true`, converte para PDF
5. Salva documento vinculado ao contrato PJ
6. Retorna documento criado

---

### 📄 Documentos de Contratos Gerados

#### 1. **GET** `/api/ContractDocuments`
**Descrição**: Lista todos os documentos de contratos gerados

**Autenticação**: Obrigatória

**Query Parameters**:
- `contractId` (uuid, opcional): Filtra por contrato específico

**Response 200**: Lista de documentos

---

#### 2. **GET** `/api/ContractDocuments/{id}`
**Descrição**: Obtém detalhes de um documento específico

**Response 200**: Documento completo com HTML e PDF (se houver)

---

#### 3. **POST** `/api/ContractDocuments/{id}/assinar`
**Descrição**: Marca documento como assinado

**Request Body**:
```json
{
  "assinadoPor": "Nome do Assinante",
  "dataAssinatura": "2025-11-18T14:30:00Z"
}
```

---

#### 4. **GET** `/api/ContractDocuments/{id}/download-pdf`
**Descrição**: Download do PDF do documento

**Response 200**: Arquivo PDF
**Response 404**: PDF não disponível

---

## Casos de Uso Práticos

### Caso 1: Gerar Contrato PJ do Zero

**Passo 1**: Listar templates disponíveis
```
GET /api/ContractTemplates
```

**Passo 2**: Escolher template (pode ser do sistema ou da empresa)
```
GET /api/ContractTemplates/{id-do-template}
```

**Passo 3**: Ver variáveis necessárias
```
GET /api/ContractTemplates/variaveis-disponiveis
```

**Passo 4**: Gerar contrato preenchido
```
POST /api/ContractTemplates/gerar-contrato
{
  "templateId": "...",
  "contractId": "...",
  "dadosPreenchimento": { ... },
  "gerarPdf": true
}
```

**Passo 5**: Assinar documento
```
POST /api/ContractDocuments/{id}/assinar
```

---

### Caso 2: Criar Template Personalizado

**Passo 1**: Ver variáveis disponíveis
```
GET /api/ContractTemplates/variaveis-disponiveis
```

**Passo 2**: Criar template com HTML customizado
```
POST /api/ContractTemplates
{
  "nome": "Meu Template",
  "tipo": "PrestacaoServicoPJ",
  "conteudoHtml": "<html>...{{VARIAVEIS}}...</html>",
  "variaveisDisponiveis": ["VAR1", "VAR2"]
}
```

**Passo 3**: Definir como padrão (opcional)
```
POST /api/ContractTemplates/{id}/definir-padrao
```

---

### Caso 3: Usar Template do Sistema

**Diferença**: Templates do sistema (ehSistema: true) aparecem automaticamente para todas as empresas. Eles:
- ✅ Podem ser listados
- ✅ Podem ser visualizados
- ✅ Podem ser usados para gerar contratos
- ❌ NÃO podem ser editados
- ❌ NÃO podem ser deletados

---

## Validações Importantes

### Permissões por Role

| Endpoint | DonoEmpresaPai | Financeiro | Juridico | FuncionarioPJ |
|----------|----------------|------------|----------|---------------|
| Listar templates | ✅ | ✅ | ✅ | ❌ |
| Ver detalhes | ✅ | ✅ | ✅ | ❌ |
| Criar template | ✅ | ❌ | ✅ | ❌ |
| Editar template | ✅ | ❌ | ✅ | ❌ |
| Definir padrão | ✅ | ❌ | ❌ | ❌ |
| Gerar contrato | ✅ | ❌ | ✅ | ❌ |
| Assinar documento | ✅ | ❌ | ✅ | ❌ |

### Regras de Negócio

1. **Templates do Sistema**:
   - Campo `ehSistema: true`
   - Campo `company_id: NULL` no banco
   - Visíveis para todas as empresas
   - Não podem ser editados
   - Aparecem na listagem junto com templates da empresa

2. **Templates da Empresa**:
   - Campo `ehSistema: false`
   - Campo `company_id` preenchido
   - Visíveis apenas para a própria empresa
   - Podem ser editados e deletados

3. **Template Padrão**:
   - Apenas 1 template pode ser padrão por tipo por empresa
   - Templates do sistema não podem ser padrão

4. **Geração de Contrato**:
   - Todas as variáveis do template devem ser fornecidas
   - Sistema valida se todas as `{{VARIAVEIS}}` foram substituídas
   - PDF é gerado automaticamente se `gerarPdf: true`

---

## Estrutura de Dados

### ContractTemplate
```json
{
  "id": "uuid",
  "companyId": "uuid ou NULL (sistema)",
  "nome": "string",
  "descricao": "string",
  "tipo": "enum",
  "conteudoHtml": "string",
  "variaveisDisponiveis": ["array"],
  "ehPadrao": "boolean",
  "ehSistema": "boolean",
  "podeEditar": "boolean",
  "podeDeletar": "boolean",
  "ativo": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### ContractDocument
```json
{
  "id": "uuid",
  "contractId": "uuid",
  "templateId": "uuid",
  "conteudoHtmlPreenchido": "string",
  "conteudoPdfBase64": "string?",
  "versao": "string",
  "assinadoPor": "string?",
  "assinadoEm": "datetime?",
  "createdAt": "datetime"
}
```

---

## Próximos Passos para Frontend

1. **Tela de Listagem de Templates**
   - Mostrar templates do sistema com badge "SISTEMA"
   - Mostrar templates da empresa com botões de editar/deletar
   - Filtro por tipo
   - Ação: "Usar este template"

2. **Tela de Criação de Template**
   - Editor HTML com suporte a variáveis
   - Lista de variáveis disponíveis (drag & drop)
   - Preview do template

3. **Tela de Geração de Contrato**
   - Formulário dinâmico baseado nas variáveis do template
   - Preview do contrato preenchido
   - Botão "Gerar PDF"
   - Botão "Assinar"

4. **Tela de Documentos Gerados**
   - Lista de contratos gerados
   - Status: Pendente / Assinado
   - Download PDF
   - Visualizar HTML

---

## Base URL

**Produção**: `https://aureapi.gabrielsanztech.com.br`

**Autenticação**: Bearer Token via header `Authorization: Bearer {token}`

---

**Versão**: 1.0  
**Data**: 18 de Novembro de 2025  
**Status**: ✅ Implementado e Testado
