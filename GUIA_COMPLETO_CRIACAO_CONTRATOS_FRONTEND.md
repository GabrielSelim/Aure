# 📋 Guia Completo - Criação de Contratos PJ para Frontend

## 🎯 Objetivo
Este documento detalha **100% do fluxo** para implementar a funcionalidade de criação de contratos PJ no frontend, incluindo todos os endpoints, campos obrigatórios, validações e exemplos de código.

---

## 🏗️ Arquitetura do Sistema de Contratos

### Visão Geral
O sistema permite criar contratos PJ de duas formas:
1. **Com Funcionário PJ Existente**: Seleciona um funcionário PJ já cadastrado no sistema
2. **Com Dados Manuais**: Insere dados de um contratado externo manualmente

### Componentes Principais
1. **Templates Presets**: Templates pré-configurados (Vendas, TI, Consultoria, etc.)
2. **Configurações Personalizadas**: Templates customizados pela empresa
3. **Preview de Contrato**: Visualização do HTML antes de gerar
4. **Geração de Contrato**: Criação efetiva do contrato no sistema

---

## 📡 Endpoints Disponíveis

### Base URL
```
https://aureapi.gabrielsanztech.com.br/api/ContractTemplateConfig
```

### Autenticação
Todos os endpoints requerem:
```
Authorization: Bearer {seu_token_jwt}
```

---

## 1️⃣ Listar Templates Presets Disponíveis

### Endpoint
```http
GET /api/ContractTemplateConfig/presets
```

### Resposta
```json
[
  {
    "tipo": "Vendas",
    "nome": "Contrato de Prestação de Serviços - Vendas",
    "descricao": "Template para contratos de prestação de serviços na área de vendas",
    "configuracao": {
      "nomeConfig": "Vendas",
      "categoria": "Comercial",
      "tituloServico": "PRESTAÇÃO DE SERVIÇOS DE VENDAS",
      "descricaoServico": "Prestação de serviços de consultoria comercial e vendas...",
      "localPrestacaoServico": "São Paulo/SP ou Home Office",
      "detalhamentoServicos": [
        "Prospecção e qualificação de leads",
        "Apresentação de propostas comerciais",
        "Negociação e fechamento de vendas"
      ],
      "clausulaAjudaCusto": null,
      "obrigacoesContratado": [
        "Cumprir as metas de vendas estabelecidas",
        "Manter sigilo sobre informações confidenciais"
      ],
      "obrigacoesContratante": [
        "Fornecer todo material necessário",
        "Efetuar pagamento na data acordada"
      ]
    }
  },
  {
    "tipo": "TI",
    "nome": "Contrato de Prestação de Serviços - TI",
    "descricao": "Template para contratos de prestação de serviços na área de TI",
    "configuracao": { ... }
  }
]
```

### Presets Disponíveis
- `Vendas`
- `TI`
- `Consultoria`
- `Marketing`
- `RH`

---

## 2️⃣ Buscar Preset Específico

### Endpoint
```http
GET /api/ContractTemplateConfig/presets/{tipo}
```

### Exemplo
```http
GET /api/ContractTemplateConfig/presets/TI
```

---

## 3️⃣ Listar Configurações Personalizadas da Empresa

### Endpoint
```http
GET /api/ContractTemplateConfig/config
```

### Resposta
```json
[
  {
    "id": "uuid",
    "companyId": "uuid",
    "nomeEmpresa": "Minha Empresa LTDA",
    "nomeConfig": "Desenvolvimento Web",
    "categoria": "TI",
    "tituloServico": "DESENVOLVIMENTO DE APLICAÇÕES WEB",
    "descricaoServico": "Desenvolvimento de sistemas web personalizados",
    "localPrestacaoServico": "Remoto",
    "detalhamentoServicos": [
      "Análise de requisitos",
      "Desenvolvimento backend e frontend",
      "Testes e deploy"
    ],
    "clausulaAjudaCusto": null,
    "obrigacoesContratado": ["..."],
    "obrigacoesContratante": ["..."],
    "createdAt": "2025-11-20T10:00:00Z",
    "updatedAt": "2025-11-20T10:00:00Z"
  }
]
```

---

## 4️⃣ Buscar Configuração Específica da Empresa

### Endpoint
```http
GET /api/ContractTemplateConfig/config/{nomeConfig}
```

### Exemplo
```http
GET /api/ContractTemplateConfig/config/Desenvolvimento%20Web
```

---

## 5️⃣ Criar ou Atualizar Configuração Personalizada

### Endpoint
```http
POST /api/ContractTemplateConfig/config
```

### Permissão
**Apenas DonoEmpresaPai**

### Request Body
```json
{
  "nomeConfig": "Suporte Técnico",
  "categoria": "TI",
  "tituloServico": "SUPORTE TÉCNICO ESPECIALIZADO",
  "descricaoServico": "Prestação de serviços de suporte técnico em infraestrutura de TI",
  "localPrestacaoServico": "São Paulo/SP ou Remoto",
  "detalhamentoServicos": [
    "Atendimento a chamados técnicos",
    "Manutenção preventiva de servidores",
    "Configuração de redes e segurança",
    "Backup e recuperação de dados"
  ],
  "clausulaAjudaCusto": "O CONTRATANTE fornecerá auxílio transporte no valor de R$ 500,00 mensais quando houver atendimento presencial.",
  "obrigacoesContratado": [
    "Atender chamados em até 2 horas úteis",
    "Manter sigilo sobre dados da empresa",
    "Fornecer relatórios mensais de atividades",
    "Cumprir as normas de segurança da informação"
  ],
  "obrigacoesContratante": [
    "Fornecer acesso aos sistemas necessários",
    "Efetuar pagamento até o 5º dia útil",
    "Comunicar problemas com antecedência mínima de 24h",
    "Fornecer ambiente adequado para trabalho presencial"
  ]
}
```

### Campos Obrigatórios
| Campo | Tipo | Validação | Descrição |
|-------|------|-----------|-----------|
| `nomeConfig` | string | Max 100 chars | Nome único da configuração |
| `categoria` | string | Max 50 chars | Categoria (TI, Vendas, etc) |
| `tituloServico` | string | Max 200 chars | Título do serviço no contrato |
| `descricaoServico` | string | Max 1000 chars | Descrição detalhada |
| `localPrestacaoServico` | string | Max 500 chars | Local de prestação |
| `detalhamentoServicos` | array | Mínimo 1 item | Lista de serviços |
| `obrigacoesContratado` | array | Mínimo 1 item | Obrigações do PJ |
| `obrigacoesContratante` | array | Mínimo 1 item | Obrigações da empresa |

### Campos Opcionais
- `clausulaAjudaCusto`: Cláusula de ajuda de custo (transporte, alimentação, etc)

---

## 6️⃣ Clonar Preset como Configuração Personalizada

### Endpoint
```http
POST /api/ContractTemplateConfig/clonar-preset/{tipoPreset}
```

### Permissão
**Apenas DonoEmpresaPai**

### Exemplo
```http
POST /api/ContractTemplateConfig/clonar-preset/TI
Content-Type: application/json

{
  "nomeConfig": "TI - Minha Empresa"
}
```

### Response
Retorna a configuração criada (mesmo formato do endpoint de criar config)

---

## 7️⃣ Validar Dados para Gerar Contrato

### Endpoint
```http
GET /api/ContractTemplateConfig/validar-dados
```

### Permissão
**DonoEmpresaPai ou Jurídico**

### Resposta
```json
{
  "perfilCompleto": true,
  "empresaCompleta": true,
  "camposEmpresaFaltando": [],
  "camposRepresentanteFaltando": [],
  "nomeRepresentante": "João Silva",
  "cargoRepresentante": "Proprietário",
  "nomeEmpresa": "Minha Empresa LTDA",
  "podeGerarContrato": true
}
```

### Exemplo com Dados Incompletos
```json
{
  "perfilCompleto": false,
  "empresaCompleta": false,
  "camposEmpresaFaltando": [
    "Endereço da empresa (Rua)",
    "Endereço da empresa (Número)",
    "Endereço da empresa (Cidade)",
    "Endereço da empresa (Estado)",
    "NIRE"
  ],
  "camposRepresentanteFaltando": [
    "CPF",
    "Data de Nascimento",
    "Endereço Residencial (Rua)",
    "Endereço Residencial (Cidade)"
  ],
  "nomeRepresentante": "João Silva",
  "cargoRepresentante": "Proprietário",
  "nomeEmpresa": "Minha Empresa LTDA",
  "podeGerarContrato": false
}
```

### ⚠️ Campos Obrigatórios da Empresa
- Nome/Razão Social
- CNPJ
- Endereço completo (Rua, Número, Bairro, Cidade, Estado, CEP)
- NIRE (Número de Identificação do Registro de Empresa)

### ⚠️ Campos Obrigatórios do Representante (Dono)
- Nome completo
- CPF
- RG
- Órgão Expedidor do RG
- Data de Nascimento
- Nacionalidade
- Estado Civil
- Endereço Residencial completo (Rua, Número, Bairro, Cidade, Estado, CEP)

---

## 8️⃣ Preview do Contrato (HTML)

### Endpoint
```http
POST /api/ContractTemplateConfig/preview
```

### Permissão
**Apenas DonoEmpresaPai**

### Request Body - Com Funcionário PJ
```json
{
  "funcionarioPJId": "uuid-do-funcionario-pj",
  "dadosContratadoManual": null,
  "templateConfig": {
    "nomeConfig": "TI - Backend",
    "categoria": "TI",
    "tituloServico": "DESENVOLVIMENTO BACKEND",
    "descricaoServico": "Desenvolvimento de APIs e microsserviços",
    "localPrestacaoServico": "Remoto",
    "detalhamentoServicos": [
      "Desenvolvimento de APIs RESTful",
      "Criação de microsserviços",
      "Integração com banco de dados"
    ],
    "clausulaAjudaCusto": null,
    "obrigacoesContratado": [
      "Entregar código documentado",
      "Cumprir prazos acordados"
    ],
    "obrigacoesContratante": [
      "Fornecer acesso aos sistemas",
      "Efetuar pagamento no prazo"
    ]
  },
  "valorMensal": 8500.00,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 5,
  "diaPagamento": 10
}
```

### Request Body - Com Dados Manuais
```json
{
  "funcionarioPJId": null,
  "dadosContratadoManual": {
    "nomeCompleto": "Maria Santos",
    "razaoSocial": "Maria Santos ME",
    "cnpj": "12345678000199",
    "cpf": "12345678901",
    "rg": "123456789",
    "dataNascimento": "1990-05-15",
    "profissao": "Desenvolvedora",
    "email": "maria@exemplo.com",
    "telefoneCelular": "11987654321",
    "telefoneFixo": "1133334444",
    "rua": "Rua das Flores",
    "numero": "100",
    "complemento": "Sala 5",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil",
    "cep": "01310000"
  },
  "templateConfig": { ... },
  "valorMensal": 8500.00,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 5,
  "diaPagamento": 10
}
```

### Campos do Contrato
| Campo | Tipo | Validação | Descrição |
|-------|------|-----------|-----------|
| `funcionarioPJId` | uuid? | Opcional | ID do funcionário PJ (se usar existente) |
| `dadosContratadoManual` | object? | Opcional | Dados manuais (se não usar funcionário) |
| `templateConfig` | object | Obrigatório | Configuração do template |
| `valorMensal` | decimal | > 0 | Valor mensal do contrato |
| `prazoVigenciaMeses` | int | 1-120 | Prazo de vigência em meses |
| `diaVencimentoNF` | int | 1-31 | Dia do vencimento da NF |
| `diaPagamento` | int | 1-31 | Dia do pagamento |

### ⚠️ Regra Importante
**Você DEVE informar `funcionarioPJId` OU `dadosContratadoManual`, nunca ambos.**

### Response
Retorna HTML completo do contrato para visualização

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Contrato de Prestação de Serviços</title>
    ...
</head>
<body>
    <h1>CONTRATO DE PRESTAÇÃO DE SERVIÇOS</h1>
    <p><strong>CONTRATANTE:</strong> MINHA EMPRESA LTDA, pessoa jurídica...</p>
    ...
</body>
</html>
```

---

## 9️⃣ Gerar Contrato Definitivo

### Endpoint
```http
POST /api/ContractTemplateConfig/gerar-contrato
```

### Permissão
**DonoEmpresaPai ou Jurídico**

### Request Body - Com Funcionário PJ
```json
{
  "nomeConfig": "TI - Backend",
  "funcionarioPJId": "uuid-do-funcionario-pj",
  "dadosContratadoManual": null,
  "valorMensal": 8500.00,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 5,
  "diaPagamento": 10,
  "dataInicioVigencia": "2025-12-01"
}
```

### Request Body - Com Dados Manuais
```json
{
  "nomeConfig": "TI - Backend",
  "funcionarioPJId": null,
  "dadosContratadoManual": {
    "nomeCompleto": "Maria Santos",
    "razaoSocial": "Maria Santos ME",
    "cnpj": "12345678000199",
    "cpf": "12345678901",
    "rg": "123456789",
    "dataNascimento": "1990-05-15",
    "profissao": "Desenvolvedora",
    "email": "maria@exemplo.com",
    "telefoneCelular": "11987654321",
    "telefoneFixo": "1133334444",
    "rua": "Rua das Flores",
    "numero": "100",
    "complemento": "Sala 5",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil",
    "cep": "01310000"
  },
  "valorMensal": 8500.00,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 5,
  "diaPagamento": 10,
  "dataInicioVigencia": "2025-12-01"
}
```

### Campos Obrigatórios
| Campo | Tipo | Validação | Descrição |
|-------|------|-----------|-----------|
| `nomeConfig` | string | Max 100 chars | Nome da configuração a usar |
| `valorMensal` | decimal | > 0 | Valor mensal |
| `prazoVigenciaMeses` | int | 1-120 | Prazo em meses |
| `diaVencimentoNF` | int | 1-31 | Dia vencimento NF |
| `diaPagamento` | int | 1-31 | Dia pagamento |

### Campo Opcional
- `dataInicioVigencia`: Se não informado, usa data atual

### Response (Sucesso)
```json
{
  "contractId": "uuid-do-contrato-criado",
  "message": "Contrato criado com sucesso"
}
```

Status: `201 Created`

### Comportamento
1. **Com Funcionário PJ**: 
   - Valida se o funcionário existe
   - Valida se é realmente PJ
   - Verifica se já não tem contrato ativo
   - Cria contrato vinculado ao funcionário

2. **Com Dados Manuais**:
   - Cria uma nova Company (tipo Provider)
   - Cria um novo User PJ vinculado
   - Cria contrato com esse novo usuário

---

## 🔟 Deletar Configuração Personalizada

### Endpoint
```http
DELETE /api/ContractTemplateConfig/config/{nomeConfig}
```

### Permissão
**Apenas DonoEmpresaPai**

### Exemplo
```http
DELETE /api/ContractTemplateConfig/config/TI%20-%20Backend
```

---

## 🎨 Fluxo Completo no Frontend

### Passo 1: Verificar Dados do Usuário/Empresa
```typescript
const verificarDadosCompletos = async () => {
  try {
    const response = await api.get('/api/ContractTemplateConfig/validar-dados');
    
    if (!response.data.podeGerarContrato) {
      // Mostrar modal com campos faltando
      showModalDadosIncompletos({
        empresa: response.data.camposEmpresaFaltando,
        representante: response.data.camposRepresentanteFaltando
      });
      return false;
    }
    
    return true;
  } catch (error) {
    console.error('Erro ao validar dados:', error);
    return false;
  }
};
```

### Passo 2: Escolher Template
```typescript
const carregarPresets = async () => {
  const response = await api.get('/api/ContractTemplateConfig/presets');
  setPresets(response.data);
};

const carregarConfigsPersonalizadas = async () => {
  const response = await api.get('/api/ContractTemplateConfig/config');
  setConfigsPersonalizadas(response.data);
};
```

### Passo 3: Configurar Template (se necessário)
```typescript
const criarConfigPersonalizada = async (config: ContractTemplateConfigRequest) => {
  try {
    const response = await api.post('/api/ContractTemplateConfig/config', config);
    toast.success('Configuração criada com sucesso!');
    return response.data;
  } catch (error) {
    toast.error('Erro ao criar configuração');
  }
};
```

### Passo 4: Preview do Contrato
```typescript
const gerarPreview = async (dados: PreviewTemplateRequest) => {
  try {
    const response = await api.post('/api/ContractTemplateConfig/preview', dados, {
      responseType: 'text',
      headers: { 'Accept': 'text/html' }
    });
    
    // Exibir HTML em iframe ou modal
    setHtmlPreview(response.data);
    setShowPreviewModal(true);
  } catch (error: any) {
    const mensagemErro = error.response?.data?.message || 'Erro ao gerar preview';
    toast.error(mensagemErro);
  }
};
```

### Passo 5: Gerar Contrato Definitivo
```typescript
const gerarContrato = async (dados: GerarContratoComConfigRequest) => {
  try {
    const response = await api.post('/api/ContractTemplateConfig/gerar-contrato', dados);
    
    toast.success('Contrato criado com sucesso!');
    
    // Redirecionar para detalhes do contrato
    navigate(`/contratos/${response.data.contractId}`);
  } catch (error: any) {
    const mensagemErro = error.response?.data?.message || 'Erro ao gerar contrato';
    toast.error(mensagemErro);
  }
};
```

---

## 📋 Exemplo de Interface: Formulário de Criação

### Estrutura Sugerida
```tsx
interface CriarContratoForm {
  // Passo 1: Escolher Template
  tipoTemplate: 'preset' | 'personalizado';
  templateSelecionado: string; // Nome do preset ou config
  
  // Passo 2: Escolher Contratado
  tipoContratado: 'funcionario' | 'manual';
  funcionarioPJId?: string;
  dadosManual?: DadosContratadoManual;
  
  // Passo 3: Dados do Contrato
  valorMensal: number;
  prazoVigenciaMeses: number;
  diaVencimentoNF: number;
  diaPagamento: number;
  dataInicioVigencia?: Date;
}
```

### Validações Frontend
```typescript
const validarFormulario = (form: CriarContratoForm): string[] => {
  const erros: string[] = [];
  
  if (!form.templateSelecionado) {
    erros.push('Selecione um template');
  }
  
  if (form.tipoContratado === 'funcionario' && !form.funcionarioPJId) {
    erros.push('Selecione um funcionário PJ');
  }
  
  if (form.tipoContratado === 'manual' && !form.dadosManual) {
    erros.push('Preencha os dados do contratado');
  }
  
  if (form.valorMensal <= 0) {
    erros.push('Valor mensal deve ser maior que zero');
  }
  
  if (form.prazoVigenciaMeses < 1 || form.prazoVigenciaMeses > 120) {
    erros.push('Prazo deve ser entre 1 e 120 meses');
  }
  
  if (form.diaVencimentoNF < 1 || form.diaVencimentoNF > 31) {
    erros.push('Dia de vencimento da NF inválido');
  }
  
  if (form.diaPagamento < 1 || form.diaPagamento > 31) {
    erros.push('Dia de pagamento inválido');
  }
  
  return erros;
};
```

---

## 🛠️ Componentes React Sugeridos

### 1. SelecionarTemplate.tsx
```tsx
const SelecionarTemplate: React.FC<Props> = ({ onSelect }) => {
  const [presets, setPresets] = useState([]);
  const [configs, setConfigs] = useState([]);
  
  useEffect(() => {
    carregarPresets();
    carregarConfigsPersonalizadas();
  }, []);
  
  return (
    <div>
      <h3>Templates Predefinidos</h3>
      <TemplateGrid items={presets} onSelect={onSelect} />
      
      <h3>Meus Templates Personalizados</h3>
      <TemplateGrid items={configs} onSelect={onSelect} />
      
      <Button onClick={abrirModalCriarTemplate}>
        + Criar Template Personalizado
      </Button>
    </div>
  );
};
```

### 2. SelecionarContratado.tsx
```tsx
const SelecionarContratado: React.FC<Props> = ({ onChange }) => {
  const [tipo, setTipo] = useState<'funcionario' | 'manual'>('funcionario');
  const [funcionarios, setFuncionarios] = useState([]);
  
  return (
    <div>
      <RadioGroup value={tipo} onChange={setTipo}>
        <Radio value="funcionario">Funcionário PJ Existente</Radio>
        <Radio value="manual">Dados Manuais</Radio>
      </RadioGroup>
      
      {tipo === 'funcionario' ? (
        <Select
          options={funcionarios}
          onChange={(f) => onChange({ funcionarioPJId: f.id })}
        />
      ) : (
        <FormularioDadosManuais onChange={(dados) => onChange({ dadosManual: dados })} />
      )}
    </div>
  );
};
```

### 3. DadosContrato.tsx
```tsx
const DadosContrato: React.FC<Props> = ({ onChange }) => {
  return (
    <Form>
      <FormField>
        <Label>Valor Mensal (R$)</Label>
        <Input
          type="number"
          min="0.01"
          step="0.01"
          placeholder="8500.00"
          onChange={(e) => onChange({ valorMensal: parseFloat(e.target.value) })}
        />
      </FormField>
      
      <FormField>
        <Label>Prazo de Vigência (meses)</Label>
        <Input
          type="number"
          min="1"
          max="120"
          placeholder="12"
          onChange={(e) => onChange({ prazoVigenciaMeses: parseInt(e.target.value) })}
        />
      </FormField>
      
      <FormField>
        <Label>Dia de Vencimento da NF</Label>
        <Input
          type="number"
          min="1"
          max="31"
          placeholder="5"
          onChange={(e) => onChange({ diaVencimentoNF: parseInt(e.target.value) })}
        />
        <Helper>Dia do mês em que a NF deve ser emitida</Helper>
      </FormField>
      
      <FormField>
        <Label>Dia de Pagamento</Label>
        <Input
          type="number"
          min="1"
          max="31"
          placeholder="10"
          onChange={(e) => onChange({ diaPagamento: parseInt(e.target.value) })}
        />
        <Helper>Dia do mês em que o pagamento será efetuado</Helper>
      </FormField>
      
      <FormField>
        <Label>Data de Início da Vigência (opcional)</Label>
        <DatePicker
          onChange={(date) => onChange({ dataInicioVigencia: date })}
        />
        <Helper>Se não informado, será usada a data atual</Helper>
      </FormField>
    </Form>
  );
};
```

### 4. PreviewContrato.tsx
```tsx
const PreviewContrato: React.FC<Props> = ({ dados, onConfirm }) => {
  const [html, setHtml] = useState('');
  const [loading, setLoading] = useState(false);
  
  const gerarPreview = async () => {
    setLoading(true);
    try {
      const response = await api.post('/api/ContractTemplateConfig/preview', dados);
      setHtml(response.data);
    } catch (error) {
      toast.error('Erro ao gerar preview');
    } finally {
      setLoading(false);
    }
  };
  
  useEffect(() => {
    gerarPreview();
  }, []);
  
  return (
    <Modal size="fullscreen">
      <ModalHeader>Preview do Contrato</ModalHeader>
      <ModalBody>
        {loading ? (
          <Spinner />
        ) : (
          <iframe
            srcDoc={html}
            style={{ width: '100%', height: '80vh', border: 'none' }}
          />
        )}
      </ModalBody>
      <ModalFooter>
        <Button variant="secondary" onClick={onClose}>
          Voltar
        </Button>
        <Button variant="primary" onClick={onConfirm}>
          Confirmar e Gerar Contrato
        </Button>
      </ModalFooter>
    </Modal>
  );
};
```

---

## ⚠️ Validações e Tratamento de Erros

### Erros Comuns

#### 1. Dados Incompletos da Empresa
```json
{
  "message": "Dados de endereço da empresa contratante estão incompletos. Por favor, complete o cadastro da empresa."
}
```
**Solução**: Redirecionar para edição de dados da empresa

#### 2. Dados Incompletos do Representante
```json
{
  "message": "CPF do representante não cadastrado. Por favor, complete seu perfil."
}
```
**Solução**: Redirecionar para edição de perfil

#### 3. Funcionário PJ Não Encontrado
```json
{
  "message": "Funcionário PJ não encontrado"
}
```
**Solução**: Validar ID antes de enviar

#### 4. Contrato Duplicado
```json
{
  "message": "Funcionário PJ já possui um contrato ativo"
}
```
**Solução**: Mostrar alerta e sugerir visualizar contrato existente

#### 5. Configuração Não Encontrada
```json
{
  "message": "Configuração de template não encontrada"
}
```
**Solução**: Validar se configuração existe antes de gerar contrato

---

## 📊 Estados da Interface

### Estado do Formulário
```typescript
type EstadoFormulario = 
  | 'selecionar-template'
  | 'configurar-template'
  | 'selecionar-contratado'
  | 'preencher-dados'
  | 'preview'
  | 'gerando';

const [estado, setEstado] = useState<EstadoFormulario>('selecionar-template');
```

### Fluxo de Estados
```
selecionar-template
    ↓
configurar-template (opcional, se criar novo)
    ↓
selecionar-contratado
    ↓
preencher-dados
    ↓
preview
    ↓
gerando
    ↓
sucesso / erro
```

---

## 🎯 Checklist de Implementação

### Backend (Já Implementado ✅)
- [x] Endpoints de presets
- [x] Endpoints de configurações
- [x] Endpoint de preview
- [x] Endpoint de geração
- [x] Validação de dados
- [x] Suporte a funcionário PJ e dados manuais

### Frontend (A Implementar)
- [ ] Serviço de API para contratos
- [ ] Tela de listagem de templates
- [ ] Formulário de criação de template personalizado
- [ ] Formulário de seleção de contratado
- [ ] Formulário de dados do contrato
- [ ] Modal de preview com iframe
- [ ] Integração com gestão de funcionários PJ
- [ ] Validação de dados completos (empresa + representante)
- [ ] Tratamento de erros
- [ ] Loading states
- [ ] Confirmações e feedbacks

---

## 🔗 Endpoints Relacionados

### Funcionários PJ
```
GET /api/Users/funcionarios-pj
```
Lista todos os funcionários PJ da empresa para seleção

### Dados da Empresa
```
GET /api/UserProfile/empresa
PUT /api/UserProfile/empresa
GET /api/Companies/empresa-pai
PUT /api/Companies/empresa-pai
```

### Perfil do Usuário
```
GET /api/UserProfile/perfil-completo
PUT /api/UserProfile/perfil-completo
```

---

## 📝 Notas Importantes

### 1. Campos NIRE e Inscrição Estadual
Esses campos foram recentemente adicionados e devem ser preenchidos via:
- `PUT /api/Companies/empresa-pai` (preferencial - para dono)
- `PUT /api/UserProfile/empresa` (também aceita)

### 2. Threshold de Validação
O threshold de validação de Razão Social foi reduzido para 80%. Divergências menores agora exigem confirmação.

### 3. Sincronização Bidirecional
Ao atualizar endereço via `PUT /api/Companies/empresa-pai`, o sistema atualiza tanto o User quanto a Company automaticamente.

### 4. Modo Manual vs Funcionário PJ
- **Manual**: Cria novo PJ no sistema (pode ser usado para contratados externos)
- **Funcionário PJ**: Usa PJ já cadastrado (recomendado para colaboradores recorrentes)

### 5. Preview é Opcional mas Recomendado
Sempre gere um preview antes de confirmar para o usuário revisar o contrato.

---

## 🚀 Próximos Passos

1. **Implementar validação prévia** antes de abrir formulário
2. **Criar wizard step-by-step** para guiar o usuário
3. **Adicionar tooltips** explicando cada campo
4. **Implementar salvamento de rascunho** (localStorage)
5. **Adicionar histórico** de contratos criados
6. **Integrar com sistema de assinaturas** (futuro)

---

## 📞 Suporte

Caso tenha dúvidas:
1. Consulte a documentação Swagger: `https://aureapi.gabrielsanztech.com.br/swagger`
2. Revise este documento
3. Entre em contato com o time de backend

---

**Data de Criação**: 01/12/2025  
**Versão**: 1.0  
**Autor**: Backend Team - Aure Platform
