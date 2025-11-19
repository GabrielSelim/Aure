# Prompt para Implementação do Frontend - Sistema de Templates de Contratos

## Contexto

Você é um desenvolvedor frontend React/Next.js e precisa implementar a interface completa para o sistema de **Templates e Documentos de Contratos** da plataforma Aure. O backend já está implementado e funcionando.

## Documentação Técnica

Por favor, leia atentamente o arquivo `DOCUMENTACAO_ENDPOINTS_CONTRATOS.md` que contém:
- Todos os endpoints disponíveis
- Estrutura de dados
- Casos de uso
- Regras de negócio
- Validações e permissões

## Requisitos de Implementação

### 1. Tecnologias Obrigatórias
- **Framework**: React com Next.js 14+ (App Router)
- **Estilização**: Tailwind CSS
- **Gerenciamento de Estado**: Zustand ou Context API
- **Requisições HTTP**: Axios
- **Validação de Formulários**: React Hook Form + Zod
- **UI Components**: Shadcn/ui
- **Notificações**: React Hot Toast

### 2. Estrutura de Páginas Necessárias

#### 2.1. `/contratos/templates` - Listagem de Templates
**Funcionalidades**:
- ✅ Listar todos os templates (da empresa + do sistema)
- ✅ Badge visual diferenciando templates do sistema (não editáveis)
- ✅ Filtros: Tipo, Status (Ativo/Inativo), Origem (Minha Empresa/Sistema)
- ✅ Busca por nome
- ✅ Ações por template:
  - Ver detalhes
  - Editar (apenas se não for sistema)
  - Desativar/Ativar
  - Definir como padrão
  - Usar para gerar contrato
- ✅ Botão "Criar Novo Template"
- ✅ Indicador visual de template padrão (estrela/badge)

**Endpoint**: `GET /api/ContractTemplates`

**Layout sugerido**:
```
┌─────────────────────────────────────────────────┐
│  Templates de Contratos          [+ Novo]      │
├─────────────────────────────────────────────────┤
│  [Buscar...]  [Filtrar por tipo ▼] [Status ▼] │
├─────────────────────────────────────────────────┤
│  🏢 SISTEMA  ⭐ Contrato de Prestação...       │
│  📄 56 variáveis  •  Ativo                     │
│  [Ver] [Usar]                                   │
├─────────────────────────────────────────────────┤
│  📝 Meu Template  ⭐ (Padrão)                   │
│  📄 30 variáveis  •  Ativo                     │
│  [Ver] [Editar] [Desativar] [Usar]            │
└─────────────────────────────────────────────────┘
```

---

#### 2.2. `/contratos/templates/novo` - Criar Template
**Funcionalidades**:
- ✅ Formulário com campos:
  - Nome do template
  - Descrição
  - Tipo (dropdown)
  - Editor HTML (com syntax highlighting)
  - Lista de variáveis disponíveis (sidebar)
- ✅ Drag & drop de variáveis para o editor
- ✅ Preview em tempo real do template
- ✅ Validação: Todas as variáveis usadas devem estar na lista
- ✅ Checkbox "Definir como padrão"
- ✅ Botão "Salvar Template"

**Endpoints**:
- `GET /api/ContractTemplates/variaveis-disponiveis`
- `POST /api/ContractTemplates`

**Layout sugerido**:
```
┌──────────────────────────────────────────────────┐
│  Criar Novo Template                    [Salvar]│
├──────────────────────────────────────────────────┤
│  Nome: [_____________________________]           │
│  Tipo: [PrestacaoServicoPJ ▼]                    │
│  Descrição: [________________________]           │
├──────────────────────────────────────────────────┤
│  Editor HTML          │  Variáveis Disponíveis   │
│  ┌─────────────────┐ │  ┌──────────────────────┐│
│  │ <html>          │ │  │ 📋 Empresa           ││
│  │   {{NOME_...}}  │ │  │  • NOME_CONTRATANTE  ││
│  │ </html>         │ │  │  • CNPJ_CONTRATANTE  ││
│  └─────────────────┘ │  │                      ││
│                       │  │ 👤 Contratado        ││
│  Preview             │  │  • NOME_CONTRATADO   ││
│  ┌─────────────────┐ │  │  • CPF_CONTRATADO    ││
│  │ [Preview HTML]  │ │  │                      ││
│  └─────────────────┘ │  └──────────────────────┘│
└──────────────────────────────────────────────────┘
```

---

#### 2.3. `/contratos/templates/[id]` - Visualizar/Editar Template
**Funcionalidades**:
- ✅ Exibir todos os detalhes do template
- ✅ Se for template do sistema: Modo somente leitura
- ✅ Se for template da empresa: Permitir edição
- ✅ Mostrar lista de variáveis usadas
- ✅ Preview do HTML
- ✅ Ações:
  - Editar (se permitido)
  - Definir/Remover como padrão
  - Ativar/Desativar
  - Usar para gerar contrato

**Endpoints**:
- `GET /api/ContractTemplates/{id}`
- `PUT /api/ContractTemplates/{id}`
- `POST /api/ContractTemplates/{id}/definir-padrao`

---

#### 2.4. `/contratos/gerar` - Gerar Contrato de Template
**Funcionalidades**:
- ✅ Seleção de template (dropdown ou modal)
- ✅ Seleção de contrato PJ existente (busca com autocomplete)
- ✅ Formulário dinâmico baseado nas variáveis do template
- ✅ Validação: Todos os campos obrigatórios preenchidos
- ✅ Sugestão automática de valores (buscar do contrato PJ)
- ✅ Checkbox "Gerar PDF automaticamente"
- ✅ Preview em tempo real do contrato preenchido
- ✅ Botão "Gerar Contrato"

**Endpoints**:
- `GET /api/ContractTemplates`
- `GET /api/Contracts` (para buscar contratos PJ)
- `POST /api/ContractTemplates/gerar-contrato`

**Layout sugerido**:
```
┌──────────────────────────────────────────────────┐
│  Gerar Contrato                        [Gerar]  │
├──────────────────────────────────────────────────┤
│  Template: [Contrato de Prestação... ▼]         │
│  Contrato PJ: [Buscar contrato... ▼]            │
├──────────────────────────────────────────────────┤
│  📝 Dados do Contrato                            │
│  ┌─────────────────────────────────────────────┐│
│  │ NOME_EMPRESA_CONTRATANTE                    ││
│  │ [_____________________________________]     ││
│  │                                             ││
│  │ CNPJ_CONTRATANTE                            ││
│  │ [_____________________________________]     ││
│  │                                             ││
│  │ ... (56 campos dinâmicos)                   ││
│  └─────────────────────────────────────────────┘│
├──────────────────────────────────────────────────┤
│  Preview do Contrato                             │
│  ┌─────────────────────────────────────────────┐│
│  │ [HTML renderizado com valores preenchidos]  ││
│  └─────────────────────────────────────────────┘│
│  ☑ Gerar PDF automaticamente                     │
└──────────────────────────────────────────────────┘
```

---

#### 2.5. `/contratos/documentos` - Lista de Contratos Gerados
**Funcionalidades**:
- ✅ Listar todos os documentos de contratos gerados
- ✅ Filtros: Status (Pendente/Assinado), Contrato PJ, Template usado
- ✅ Busca por nome do template ou contratado
- ✅ Ações por documento:
  - Visualizar HTML
  - Download PDF
  - Assinar documento
  - Ver histórico de versões
- ✅ Badge de status visual

**Endpoint**: `GET /api/ContractDocuments`

**Layout sugerido**:
```
┌─────────────────────────────────────────────────┐
│  Documentos de Contratos                        │
├─────────────────────────────────────────────────┤
│  [Buscar...]  [Filtrar por status ▼]           │
├─────────────────────────────────────────────────┤
│  📄 Contrato - SAUL VICTOR FRANCO      ⏳ Pend. │
│  Template: Prestação de Serviços               │
│  Gerado em: 18/11/2025 12:00                   │
│  [Ver HTML] [Download PDF] [Assinar]           │
├─────────────────────────────────────────────────┤
│  📄 Contrato - JOÃO SILVA              ✅ Assin.│
│  Template: Consultoria                         │
│  Assinado em: 15/11/2025 por João Silva       │
│  [Ver HTML] [Download PDF]                     │
└─────────────────────────────────────────────────┘
```

---

#### 2.6. `/contratos/documentos/[id]` - Visualizar Documento
**Funcionalidades**:
- ✅ Exibir HTML renderizado do contrato
- ✅ Informações do documento (template usado, data, versão)
- ✅ Status de assinatura
- ✅ Se não assinado: Botão "Assinar Documento"
- ✅ Download do PDF
- ✅ Histórico de alterações

**Endpoints**:
- `GET /api/ContractDocuments/{id}`
- `POST /api/ContractDocuments/{id}/assinar`
- `GET /api/ContractDocuments/{id}/download-pdf`

---

### 3. Componentes Reutilizáveis Necessários

#### 3.1. `TemplateCard`
Props: `template`, `onEdit`, `onUse`, `onSetDefault`, `onToggleActive`

#### 3.2. `VariablesSidebar`
Props: `variables`, `onDragStart`, `onVariableClick`

#### 3.3. `HTMLEditor`
Props: `value`, `onChange`, `availableVariables`

#### 3.4. `ContractPreview`
Props: `htmlContent`, `isLoading`

#### 3.5. `DynamicContractForm`
Props: `variables`, `initialValues`, `onSubmit`

#### 3.6. `DocumentCard`
Props: `document`, `onView`, `onDownload`, `onSign`

#### 3.7. `StatusBadge`
Props: `status`, `type` (template | document)

---

### 4. Funcionalidades Específicas

#### 4.1. Diferenciação Visual de Templates do Sistema
```tsx
{template.ehSistema && (
  <Badge variant="secondary" className="gap-1">
    <BuildingIcon className="h-3 w-3" />
    SISTEMA
  </Badge>
)}

{!template.podeEditar && (
  <LockIcon className="h-4 w-4 text-gray-400" />
)}
```

#### 4.2. Validação de Variáveis no Editor
- Extrair todas as `{{VARIAVEL}}` do HTML
- Verificar se todas estão em `variaveisDisponiveis`
- Mostrar alerta se houver variáveis não reconhecidas

#### 4.3. Auto-preenchimento de Formulário
Quando usuário seleciona um contrato PJ:
- Buscar dados do contrato via API
- Preencher automaticamente campos correspondentes
- Permitir edição manual

#### 4.4. Preview em Tempo Real
- Substituir `{{VARIAVEL}}` por valores do formulário
- Renderizar HTML em iframe ou div
- Atualizar a cada mudança de campo

#### 4.5. Assinatura de Documento
Modal com:
- Nome do assinante (preenchido automaticamente)
- Data/hora da assinatura
- Confirmação: "Confirmo que li e concordo"
- Botão "Assinar Documento"

---

### 5. Gerenciamento de Estado

```tsx
// stores/contractTemplates.ts
interface ContractTemplatesStore {
  templates: Template[];
  selectedTemplate: Template | null;
  isLoading: boolean;
  fetchTemplates: () => Promise<void>;
  createTemplate: (data: CreateTemplateData) => Promise<void>;
  updateTemplate: (id: string, data: UpdateTemplateData) => Promise<void>;
  setAsDefault: (id: string) => Promise<void>;
}

// stores/contractDocuments.ts
interface ContractDocumentsStore {
  documents: ContractDocument[];
  selectedDocument: ContractDocument | null;
  isLoading: boolean;
  fetchDocuments: () => Promise<void>;
  generateContract: (data: GenerateContractData) => Promise<void>;
  signDocument: (id: string, data: SignData) => Promise<void>;
}
```

---

### 6. Tratamento de Erros

**Erros Esperados**:
- 400: Validação falhou (mostrar mensagens específicas)
- 401: Não autorizado (redirecionar para login)
- 403: Sem permissão (mostrar mensagem "Ação não permitida")
- 404: Recurso não encontrado
- 500: Erro do servidor (mostrar mensagem genérica)

**Exemplo**:
```tsx
try {
  await generateContract(data);
  toast.success('Contrato gerado com sucesso!');
  router.push('/contratos/documentos');
} catch (error) {
  if (error.response?.status === 400) {
    toast.error(error.response.data.message || 'Dados inválidos');
  } else if (error.response?.status === 403) {
    toast.error('Você não tem permissão para gerar contratos');
  } else {
    toast.error('Erro ao gerar contrato. Tente novamente.');
  }
}
```

---

### 7. Permissões por Role

```tsx
// hooks/usePermissions.ts
const useContractPermissions = () => {
  const { user } = useAuth();
  
  return {
    canCreateTemplate: ['DonoEmpresaPai', 'Juridico'].includes(user.role),
    canEditTemplate: ['DonoEmpresaPai', 'Juridico'].includes(user.role),
    canSetDefault: user.role === 'DonoEmpresaPai',
    canGenerateContract: ['DonoEmpresaPai', 'Juridico'].includes(user.role),
    canSignDocument: ['DonoEmpresaPai', 'Juridico'].includes(user.role),
  };
};
```

---

### 8. Testes Necessários

#### 8.1. Testes de Componentes
- TemplateCard renderiza corretamente
- Badge "SISTEMA" aparece quando ehSistema=true
- Botões de ação são habilitados/desabilitados corretamente

#### 8.2. Testes de Integração
- Listagem de templates carrega dados da API
- Criação de template envia dados corretos
- Geração de contrato substitui variáveis corretamente
- Download de PDF funciona

#### 8.3. Testes de Validação
- Formulário não permite envio com campos vazios
- Variáveis não reconhecidas são detectadas
- Formato de data/CNPJ/CPF são validados

---

### 9. Performance

#### 9.1. Otimizações
- Lazy loading de páginas com Next.js dynamic imports
- Debounce na busca de templates (500ms)
- Paginação na lista de documentos (20 por página)
- Cache de templates com SWR ou React Query
- Virtualização de lista grande de variáveis

#### 9.2. Loading States
- Skeleton loaders para listas
- Spinner para ações (salvar, gerar, assinar)
- Progress bar para upload de templates

---

### 10. Acessibilidade

- ✅ Navegação por teclado em todos os formulários
- ✅ Labels descritivos em todos os inputs
- ✅ Feedback de validação com aria-invalid
- ✅ Modais com foco automático
- ✅ Contraste de cores adequado (WCAG AA)

---

### 11. Responsividade

- ✅ Layout mobile-first
- ✅ Tabelas/listas se transformam em cards em mobile
- ✅ Editor HTML com modo simplificado em mobile
- ✅ Sidebar de variáveis colapsa em modal em mobile

---

## Estrutura de Arquivos Sugerida

```
src/
├── app/
│   ├── contratos/
│   │   ├── templates/
│   │   │   ├── page.tsx              # Lista de templates
│   │   │   ├── novo/page.tsx         # Criar template
│   │   │   └── [id]/page.tsx         # Ver/Editar template
│   │   ├── gerar/page.tsx            # Gerar contrato
│   │   └── documentos/
│   │       ├── page.tsx              # Lista de documentos
│   │       └── [id]/page.tsx         # Ver documento
├── components/
│   ├── contracts/
│   │   ├── TemplateCard.tsx
│   │   ├── TemplateForm.tsx
│   │   ├── VariablesSidebar.tsx
│   │   ├── HTMLEditor.tsx
│   │   ├── ContractPreview.tsx
│   │   ├── DynamicContractForm.tsx
│   │   ├── DocumentCard.tsx
│   │   ├── SignDocumentModal.tsx
│   │   └── StatusBadge.tsx
├── hooks/
│   ├── useContractTemplates.ts
│   ├── useContractDocuments.ts
│   ├── useContractPermissions.ts
│   └── useVariableExtractor.ts
├── lib/
│   ├── api/
│   │   ├── templates.ts
│   │   └── documents.ts
│   └── utils/
│       ├── templateParser.ts
│       └── pdfGenerator.ts
├── stores/
│   ├── contractTemplates.ts
│   └── contractDocuments.ts
└── types/
    ├── template.ts
    └── document.ts
```

---

## Prioridade de Implementação

### Sprint 1 (Essencial)
1. ✅ Listagem de templates
2. ✅ Visualizar detalhes do template
3. ✅ Gerar contrato de template
4. ✅ Listar documentos gerados

### Sprint 2 (Importante)
5. ✅ Criar template personalizado
6. ✅ Editar template
7. ✅ Assinar documento
8. ✅ Download PDF

### Sprint 3 (Complementar)
9. ✅ Definir template como padrão
10. ✅ Ativar/Desativar templates
11. ✅ Filtros avançados
12. ✅ Histórico de versões

---

## Critérios de Aceitação

### Para Templates
- [ ] Usuário consegue ver lista de templates (sistema + empresa)
- [ ] Templates do sistema têm badge "SISTEMA" e não podem ser editados
- [ ] Usuário consegue criar novo template com variáveis
- [ ] Preview do template funciona em tempo real
- [ ] Validação de variáveis funciona corretamente

### Para Geração de Contratos
- [ ] Formulário dinâmico é gerado baseado nas variáveis
- [ ] Auto-preenchimento funciona ao selecionar contrato PJ
- [ ] Preview do contrato preenchido funciona
- [ ] PDF é gerado corretamente
- [ ] Documento é salvo e listado

### Para Documentos
- [ ] Lista mostra todos os documentos gerados
- [ ] Status (Pendente/Assinado) é exibido corretamente
- [ ] Download de PDF funciona
- [ ] Assinatura de documento funciona
- [ ] HTML renderizado exibe contrato corretamente

---

## Exemplos de Código

### Exemplo 1: Fetch de Templates
```tsx
const fetchTemplates = async () => {
  try {
    setIsLoading(true);
    const response = await api.get('/api/ContractTemplates', {
      params: { apenasAtivos: true }
    });
    setTemplates(response.data);
  } catch (error) {
    toast.error('Erro ao carregar templates');
  } finally {
    setIsLoading(false);
  }
};
```

### Exemplo 2: Gerar Contrato
```tsx
const handleGenerateContract = async (data: GenerateContractData) => {
  try {
    const response = await api.post('/api/ContractTemplates/gerar-contrato', {
      templateId: selectedTemplate.id,
      contractId: selectedContract.id,
      dadosPreenchimento: formData,
      gerarPdf: true
    });
    
    toast.success('Contrato gerado com sucesso!');
    router.push(`/contratos/documentos/${response.data.id}`);
  } catch (error) {
    toast.error('Erro ao gerar contrato');
  }
};
```

### Exemplo 3: Renderizar Badge de Sistema
```tsx
const TemplateBadges = ({ template }) => (
  <div className="flex gap-2">
    {template.ehSistema && (
      <Badge variant="secondary">
        <BuildingIcon className="h-3 w-3 mr-1" />
        SISTEMA
      </Badge>
    )}
    {template.ehPadrao && (
      <Badge variant="default">
        <StarIcon className="h-3 w-3 mr-1" />
        PADRÃO
      </Badge>
    )}
    {!template.ativo && (
      <Badge variant="destructive">INATIVO</Badge>
    )}
  </div>
);
```

---

## Perguntas Frequentes

**Q: Templates do sistema podem ser editados?**
A: Não. Templates com `ehSistema: true` são somente leitura.

**Q: Como sei se posso editar um template?**
A: Verifique os campos `podeEditar` e `podeDeletar` no response.

**Q: Todas as variáveis precisam ser preenchidas?**
A: Sim. O backend valida se todas as `{{VARIAVEIS}}` do template foram fornecidas.

**Q: O PDF é gerado automaticamente?**
A: Apenas se `gerarPdf: true` for enviado na requisição.

**Q: Posso criar templates sem estar autenticado?**
A: Não. Todos os endpoints exigem autenticação com Bearer token.

---

## Base URL da API

**Produção**: `https://aureapi.gabrielsanztech.com.br`

**Autenticação**: `Authorization: Bearer {token}`

---

## Contato

Para dúvidas sobre os endpoints ou regras de negócio, consulte a documentação completa em `DOCUMENTACAO_ENDPOINTS_CONTRATOS.md`.

---

**Boa implementação! 🚀**
