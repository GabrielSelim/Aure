# 📋 Sistema de Templates Genéricos - Implementação

## ✅ O que foi implementado

### 1. Estrutura de Dados

#### **Entidade: ContractTemplateConfig**
```
📁 src/Aure.Domain/Entities/ContractTemplateConfig.cs
```
- Armazena configuração de template **por empresa** (1:1 com Company)
- Campos customizáveis:
  - `TituloServico` - Ex: "Serviços de Vendas"
  - `DescricaoServico` - Descrição completa
  - `LocalPrestacaoServico` - Onde será prestado
  - `DetalhamentoServicos` (List<string>) - Itens do serviço
  - `ObrigacoesContratado` (List<string>) - Obrigações do PJ
  - `ObrigacoesContratante` (List<string>) - Obrigações da empresa
  - `ClausulaAjudaCusto` (opcional) - HTML customizado

#### **Template HTML Genérico**
```
📁 src/Aure.Infrastructure/Templates/ContratoPrestacaoServicosGenerico.html
```
- Template flexível com variáveis substituíveis
- Suporta **qualquer tipo de negócio** (não só software)
- Variáveis principais:
  - `{{TITULO_SERVICO}}`
  - `{{DESCRICAO_SERVICO}}`
  - `{{DETALHAMENTO_SERVICOS}}` (HTML gerado dinamicamente)
  - `{{OBRIGACOES_CONTRATADO}}` (HTML gerado dinamicamente)
  - `{{OBRIGACOES_CONTRATANTE}}` (HTML gerado dinamicamente)
  - `{{CLAUSULA_AJUDA_CUSTO}}` (opcional)

### 2. Presets Pré-Configurados

#### **ContractTemplatePresetService**
```
📁 src/Aure.Application/Services/ContractTemplatePresetService.cs
```

**5 Modelos Prontos**:

1. **Software** - Gestão e Análise de Negócios
   - Para empresas de TI/desenvolvimento
   - 10 serviços específicos de software

2. **Vendas** - Vendas e Representação Comercial  
   - Para empresas de comércio/vendas
   - Prospecção, negociação, pós-venda
   - Inclui cláusula de ajuda de custo (R$ 500)

3. **Consultoria** - Consultoria Empresarial
   - Para consultores especializados
   - Análise, planejamento estratégico, treinamentos

4. **Marketing** - Marketing e Comunicação
   - Para agências/freelancers de marketing
   - Redes sociais, campanhas, conteúdo

5. **Logística** - Logística e Transporte
   - Para motoristas/transportadores
   - Coleta, entrega, rastreamento
   - Inclui cláusula de ajuda de custo (R$ 800)

### 3. Banco de Dados

#### **Migration Criada**
```
📁 src/Aure.Infrastructure/Migrations/20251124173628_AdicionarContractTemplateConfig.cs
```

**Tabela:** `contract_template_configs`
```sql
CREATE TABLE contract_template_configs (
    id UUID PRIMARY KEY,
    company_id UUID UNIQUE NOT NULL REFERENCES companies(id),
    titulo_servico VARCHAR(200) NOT NULL,
    descricao_servico VARCHAR(1000) NOT NULL,
    local_prestacao_servico VARCHAR(500) NOT NULL,
    detalhamento_servicos JSONB NOT NULL,
    clausula_ajuda_custo VARCHAR(2000),
    obrigacoes_contratado JSONB NOT NULL,
    obrigacoes_contratante JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);
```

**Relacionamento**: 1 empresa = 1 configuração única

### 4. Repositórios

#### **ContractTemplateConfigRepository**
```
📁 src/Aure.Infrastructure/Repositories/ContractTemplateConfigRepository.cs
```
- `GetByCompanyIdAsync()` - Busca config da empresa
- `AddAsync()` - Cria nova config
- `UpdateAsync()` - Atualiza config existente
- `DeleteAsync()` - Remove config

#### **Registros Atualizados**
- ✅ `IUnitOfWork` - Adicionado `ContractTemplateConfigs`
- ✅ `UnitOfWork` - Inicializa repositório
- ✅ `ServiceCollectionExtensions` - **PRECISA ADICIONAR SERVICE**
- ✅ `AureDbContext` - DbSet criado

### 5. DTOs

#### **ContractTemplateDTOs.cs**
```
📁 src/Aure.Application/DTOs/Contract/ContractTemplateDTOs.cs
```

**Novos DTOs**:
- `ContractTemplateConfigRequest` - Criar/atualizar config
- `ContractTemplateConfigResponse` - Resposta com dados completos
- `PreviewTemplateRequest` - Preview antes de gerar contrato
- `ContractTemplatePresetResponse` - Modelo pronto

---

## ❌ O que FALTA implementar

### 1. **Service Layer** (CRÍTICO)

Precisa criar:

```
📁 src/Aure.Application/Interfaces/IContractTemplateConfigService.cs
📁 src/Aure.Application/Services/ContractTemplateConfigService.cs
```

**Métodos necessários**:
```csharp
Task<Result<List<ContractTemplatePresetResponse>>> GetPresetsAsync();
Task<Result<ContractTemplatePresetResponse?>> GetPresetByTipoAsync(string tipo);
Task<Result<ContractTemplateConfigResponse?>> GetCompanyConfigAsync(Guid userId);
Task<Result<ContractTemplateConfigResponse>> CreateOrUpdateConfigAsync(Guid userId, ContractTemplateConfigRequest request);
Task<Result<string>> PreviewContractHtmlAsync(Guid userId, PreviewTemplateRequest request);
Task<Result<bool>> DeleteCompanyConfigAsync(Guid userId);
```

**Regras de validação**:
- ✅ Apenas `DonoEmpresaPai` pode criar/editar/deletar config
- ✅ Todos os usuários podem ver presets
- ✅ Todos os usuários podem ver config da própria empresa
- ✅ Config vinculada ao `CompanyId` (isolamento por empresa)
- ✅ Preview gera HTML completo com dados reais antes de salvar contrato

**Importante no Service**:
- User usa campos `CPFEncrypted`, `RGEncrypted`, `DataNascimento`, `Cargo`
- User usa campos `EnderecoRua`, `EnderecoNumero`, `EnderecoCidade`, etc (não `Address*`)
- Retornar `Result.Failure<T>()` (não `Result.Failure()`)
- `Result.Success<T>()` com tipo genérico explícito

### 2. **Controller** (CRÍTICO)

Precisa criar:

```
📁 src/Aure.API/Controllers/ContractTemplateConfigController.cs
```

**Endpoints necessários**:

```csharp
GET    /api/ContractTemplateConfig/presets
       → Retorna lista de 5 presets disponíveis

GET    /api/ContractTemplateConfig/presets/{tipo}
       → Retorna preset específico (software, vendas, consultoria, marketing, logistica)

GET    /api/ContractTemplateConfig/config
       → Retorna config da empresa do usuário logado
       → 404 se não tiver config ainda

POST   /api/ContractTemplateConfig/config
       → [Authorize(Roles = "DonoEmpresaPai")]
       → Cria ou atualiza config da empresa
       → Body: ContractTemplateConfigRequest

POST   /api/ContractTemplateConfig/preview
       → [Authorize(Roles = "DonoEmpresaPai")]
       → Gera preview do HTML completo
       → Body: PreviewTemplateRequest
       → Retorna: text/html

DELETE /api/ContractTemplateConfig/config
       → [Authorize(Roles = "DonoEmpresaPai")]
       → Deleta config da empresa
```

### 3. **Registrar Service** (CRÍTICO)

Em `ServiceCollectionExtensions.cs`, adicionar:

```csharp
services.AddScoped<IContractTemplateConfigService, ContractTemplateConfigService>();
```

### 4. **Migração no Servidor** (OBRIGATÓRIO)

Migration criada localmente, mas **NÃO EXECUTADA** no banco de produção.

**Passos necessários**:
```bash
# 1. Commit e push
git add .
git commit -m "feat: adicionar sistema templates genéricos"
git push origin main

# 2. No servidor
ssh root@5.189.174.61
cd /root/Aure
git pull
docker-compose down
docker-compose up -d --build

# 3. Executar migration (dentro do container ou via EF)
dotnet ef database update --project src/Aure.Infrastructure --startup-project src/Aure.API
```

### 5. **Integração com Geração de Contratos** (FUTURO)

Quando for gerar contrato PJ, modificar para:

```csharp
// Buscar config da empresa
var config = await _unitOfWork.ContractTemplateConfigs.GetByCompanyIdAsync(companyId);

// Se não tiver config, usar template antigo ou exigir config
if (config == null)
{
    throw new BusinessException("Empresa precisa configurar template de contratos");
}

// Usar template genérico
var templatePath = "ContratoPrestacaoServicosGenerico.html";
var html = await File.ReadAllTextAsync(templatePath);

// Preencher variáveis customizadas
html = html.Replace("{{TITULO_SERVICO}}", config.TituloServico);
html = html.Replace("{{DESCRICAO_SERVICO}}", config.DescricaoServico);
html = html.Replace("{{DETALHAMENTO_SERVICOS}}", config.GerarDetalhamentoServicosHtml());
html = html.Replace("{{OBRIGACOES_CONTRATADO}}", config.GerarObrigacoesContratadoHtml());
html = html.Replace("{{OBRIGACOES_CONTRATANTE}}", config.GerarObrigacoesContratanteHtml());
html = html.Replace("{{CLAUSULA_AJUDA_CUSTO}}", config.ClausulaAjudaCusto ?? "");

// ... preencher outras variáveis (dados da empresa, PJ, valores, etc)
```

---

## 🎯 Fluxo Completo de Uso

### Cenário: Empresa de Carros

**1. Dono acessa sistema**
```
GET /api/ContractTemplateConfig/presets
```
Vê 5 opções: Software, Vendas, Consultoria, Marketing, Logística

**2. Dono escolhe "Vendas"**
```
GET /api/ContractTemplateConfig/presets/vendas
```
Retorna config pronta com:
- Título: "Serviços de Vendas e Representação Comercial"
- 10 itens de serviços de vendas
- Obrigações específicas
- Cláusula de ajuda de custo

**3. Dono customiza (opcional) e salva**
```
POST /api/ContractTemplateConfig/config
Body: {
  "tituloServico": "Serviços de Vendas de Veículos",
  "descricaoServico": "vendas e representação comercial de veículos",
  "detalhamentoServicos": [
    "Prospecção de clientes para compra de veículos",
    "Apresentação de modelos disponíveis",
    "Negociação de financiamentos",
    ... (customizado para carros)
  ],
  ...
}
```

**4. Config salva na empresa dele**
- Tabela `contract_template_configs`
- `company_id` = ID da empresa do dono
- Outras empresas NÃO veem essa config

**5. Ao criar contrato PJ (vendedor)**
- Sistema busca config da empresa
- Usa template genérico
- Preenche com dados customizados
- Gera contrato personalizado para vendedor de carros

**6. Preview antes de salvar**
```
POST /api/ContractTemplateConfig/preview
Body: {
  "funcionarioPJId": "guid-do-vendedor",
  "templateConfig": { ... },
  "valorMensal": 3000,
  "prazoMeses": 12,
  ...
}
```
Retorna HTML completo para revisão

---

## 🔒 Segurança e Isolamento

### Isolamento por Empresa
- ✅ Cada empresa tem **1 configuração única**
- ✅ Config vinculada ao `company_id` (UNIQUE constraint)
- ✅ Service valida que usuário pertence à empresa
- ✅ Empresa A **nunca** acessa config da Empresa B

### Controle de Acesso
- ✅ **Criar/Editar/Deletar**: Apenas `DonoEmpresaPai`
- ✅ **Ver config da empresa**: Todos os usuários da empresa
- ✅ **Ver presets**: Todos os usuários autenticados
- ✅ **Preview**: Apenas `DonoEmpresaPai` (antes de gerar contrato)

### Validações
- ✅ Título: máximo 200 caracteres
- ✅ Descrição: máximo 1000 caracteres
- ✅ Local: máximo 500 caracteres
- ✅ Listas não podem ser vazias
- ✅ Validação no construtor da entidade

---

## 📊 Estrutura de Arquivos Criados

```
src/
├── Aure.Domain/
│   ├── Entities/
│   │   └── ContractTemplateConfig.cs ✅
│   └── Interfaces/
│       ├── IContractTemplateConfigRepository.cs ✅
│       └── IUnitOfWork.cs (atualizado) ✅
│
├── Aure.Infrastructure/
│   ├── Configuration/
│   │   └── ContractTemplateConfigConfiguration.cs ✅
│   ├── Data/
│   │   └── AureDbContext.cs (atualizado) ✅
│   ├── Migrations/
│   │   └── 20251124173628_AdicionarContractTemplateConfig.cs ✅
│   ├── Repositories/
│   │   ├── ContractTemplateConfigRepository.cs ✅
│   │   └── UnitOfWork.cs (atualizado) ✅
│   └── Templates/
│       └── ContratoPrestacaoServicosGenerico.html ✅
│
├── Aure.Application/
│   ├── DTOs/
│   │   └── Contract/
│   │       └── ContractTemplateDTOs.cs (atualizado) ✅
│   ├── Interfaces/
│   │   └── IContractTemplateConfigService.cs ❌ FALTA
│   └── Services/
│       ├── ContractTemplatePresetService.cs ✅
│       └── ContractTemplateConfigService.cs ❌ FALTA
│
└── Aure.API/
    ├── Controllers/
    │   └── ContractTemplateConfigController.cs ❌ FALTA
    └── Extensions/
        └── ServiceCollectionExtensions.cs (precisa registrar service) ⚠️
```

---

## 🚀 Próximos Passos (EM ORDEM)

1. ✅ **Criar Service e Controller** (arquivos faltantes)
2. ✅ **Registrar service** no DI
3. ✅ **Compilar** sem erros
4. ✅ **Commit** e push para repositório
5. ✅ **Deploy** no servidor
6. ✅ **Executar migration** no banco de produção
7. ✅ **Testar endpoints** via Postman/Swagger
8. 🔄 **Integrar com geração de contratos** (futuro)

---

## 📝 Exemplo de Teste

### 1. Ver presets disponíveis
```http
GET https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig/presets
Authorization: Bearer {token}
```

### 2. Configurar empresa com preset "Vendas"
```http
POST https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig/config
Authorization: Bearer {token_dono}
Content-Type: application/json

{
  "tituloServico": "Serviços de Vendas e Representação Comercial",
  "descricaoServico": "a prestação de serviços de vendas...",
  "localPrestacaoServico": "na sede da CONTRATANTE ou em visitas comerciais",
  "detalhamentoServicos": [
    "Prospecção ativa de novos clientes",
    "Apresentação de produtos",
    "Negociação de propostas comerciais",
    ...
  ],
  "obrigacoesContratado": [
    "Cumprir integralmente as obrigações",
    ...
  ],
  "obrigacoesContratante": [
    "Efetuar o pagamento",
    ...
  ],
  "clausulaAjudaCusto": "<p>Ajuda de custo de R$ 500...</p>"
}
```

### 3. Ver config da empresa
```http
GET https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig/config
Authorization: Bearer {token}
```

### 4. Preview de contrato
```http
POST https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig/preview
Authorization: Bearer {token_dono}
Content-Type: application/json

{
  "funcionarioPJId": "guid-do-pj",
  "templateConfig": { ... },
  "valorMensal": 5000,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 5,
  "diaPagamento": 10
}
```

Retorna HTML completo do contrato

---

**Status Atual**: Estrutura criada, falta implementar Service e Controller para funcionamento completo.

**Criado em**: 24/11/2025  
**Última atualização**: 24/11/2025
