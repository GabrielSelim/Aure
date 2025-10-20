# 🚀 PROMPT COMPLETO - FRONTEND REACT/NEXT.JS
# Sistema Aure - Interface de Usuário Completa
# Data: 20/10/2025

## 📋 INSTRUÇÕES PARA O COPILOT

Preciso que você crie um **frontend completo em React com Next.js** para o Sistema Aure, seguindo rigorosamente as especificações abaixo:

---

## 🎯 ESPECIFICAÇÕES OBRIGATÓRIAS

### **Estrutura do Projeto:**
- **Framework:** Next.js 14+ (App Router)
- **Linguagem:** TypeScript
- **Estilização:** Tailwind CSS
- **Componentes UI:** Shadcn/ui
- **Gerenciamento de Estado:** Zustand
- **Validação:** Zod + React Hook Form
- **HTTP Client:** Axios
- **Autenticação:** JWT com Context API
- **Nomenclatura:** **100% em português** (sem termos em inglês)

### **Princípios de Desenvolvimento:**
- ✅ **Componentização total** - cada elemento deve ser um componente reutilizável
- ✅ **Código limpo** - sem console.log, sem mocks desnecessários
- ✅ **Nomenclatura portuguesa** - arquivos, variáveis, funções, componentes
- ✅ **Responsividade completa** - mobile-first
- ✅ **Acessibilidade** - ARIA labels e navegação por teclado
- ✅ **Performance** - lazy loading e otimizações

---

## 🏗️ ESTRUTURA DE PASTAS OBRIGATÓRIA

```
aure-frontend/
├── src/
│   ├── app/                          # App Router Next.js
│   │   ├── (autenticado)/            # Rotas protegidas
│   │   │   ├── painel/              # Dashboard principal
│   │   │   ├── empresas/            # Gestão de empresas
│   │   │   ├── usuarios/            # Gestão de usuários
│   │   │   ├── convites/            # Sistema de convites
│   │   │   ├── contratos/           # Gestão de contratos
│   │   │   ├── pagamentos/          # Sistema de pagamentos
│   │   │   ├── relatorios/          # Relatórios e análises
│   │   │   └── layout.tsx
│   │   ├── (publico)/               # Rotas públicas
│   │   │   ├── entrar/              # Login
│   │   │   ├── registrar/           # Registro
│   │   │   ├── aceitar-convite/     # Aceitar convites
│   │   │   └── layout.tsx
│   │   ├── layout.tsx               # Layout raiz
│   │   ├── page.tsx                 # Página inicial
│   │   ├── loading.tsx              # Loading global
│   │   ├── error.tsx                # Error boundary
│   │   └── not-found.tsx            # 404
│   ├── componentes/                 # Componentes reutilizáveis
│   │   ├── ui/                      # Componentes base (shadcn)
│   │   ├── layout/                  # Componentes de layout
│   │   │   ├── CabecalhoNavegacao.tsx
│   │   │   ├── BarraLateral.tsx
│   │   │   ├── RodapePagina.tsx
│   │   │   └── LayoutAutenticado.tsx
│   │   ├── formularios/             # Componentes de formulário
│   │   │   ├── FormularioLogin.tsx
│   │   │   ├── FormularioRegistro.tsx
│   │   │   ├── FormularioConvite.tsx
│   │   │   └── FormularioEmpresa.tsx
│   │   ├── tabelas/                 # Componentes de tabela
│   │   │   ├── TabelaUsuarios.tsx
│   │   │   ├── TabelaEmpresas.tsx
│   │   │   ├── TabelaConvites.tsx
│   │   │   └── TabelaContratos.tsx
│   │   ├── cartoes/                 # Componentes de card
│   │   │   ├── CartaoEstatistica.tsx
│   │   │   ├── CartaoEmpresa.tsx
│   │   │   ├── CartaoUsuario.tsx
│   │   │   └── CartaoConvite.tsx
│   │   ├── modais/                  # Componentes de modal
│   │   │   ├── ModalConfirmacao.tsx
│   │   │   ├── ModalDetalhes.tsx
│   │   │   └── ModalEdicao.tsx
│   │   └── comum/                   # Componentes comuns
│   │       ├── Carregando.tsx
│   │       ├── MensagemErro.tsx
│   │       ├── MensagemSucesso.tsx
│   │       ├── BotaoAcao.tsx
│   │       └── IndicadorStatus.tsx
│   ├── servicos/                    # Serviços de API
│   │   ├── api.ts                   # Configuração Axios
│   │   ├── autenticacao.ts          # Serviços de auth
│   │   ├── usuarios.ts              # Serviços de usuários
│   │   ├── empresas.ts              # Serviços de empresas
│   │   ├── convites.ts              # Serviços de convites
│   │   ├── contratos.ts             # Serviços de contratos
│   │   └── pagamentos.ts            # Serviços de pagamentos
│   ├── contextos/                   # Context providers
│   │   ├── ContextoAutenticacao.tsx # Context de autenticação
│   │   ├── ContextoNotificacao.tsx  # Context de notificações
│   │   └── ContextoTema.tsx         # Context de tema
│   ├── armazenamento/               # Zustand stores
│   │   ├── loja-usuario.ts          # Store do usuário
│   │   ├── loja-empresa.ts          # Store da empresa
│   │   ├── loja-convites.ts         # Store de convites
│   │   └── loja-notificacao.ts      # Store de notificações
│   ├── validacoes/                  # Schemas Zod
│   │   ├── esquema-login.ts         # Schema de login
│   │   ├── esquema-registro.ts      # Schema de registro
│   │   ├── esquema-convite.ts       # Schema de convite
│   │   └── esquema-empresa.ts       # Schema de empresa
│   ├── utilidades/                  # Funções utilitárias
│   │   ├── formatadores.ts          # Formatação de dados
│   │   ├── validadores.ts           # Validações customizadas
│   │   ├── constantes.ts            # Constantes da aplicação
│   │   └── ajudantes.ts             # Funções auxiliares
│   ├── tipos/                       # Definições de tipos
│   │   ├── usuario.ts               # Tipos de usuário
│   │   ├── empresa.ts               # Tipos de empresa
│   │   ├── convite.ts               # Tipos de convite
│   │   ├── contrato.ts              # Tipos de contrato
│   │   └── api.ts                   # Tipos de API
│   └── estilos/                     # Estilos globais
│       └── globals.css              # CSS global com Tailwind
```

---

## 🎨 DESIGN SYSTEM OBRIGATÓRIO

### **Paleta de Cores (Usar em Tailwind):**
```typescript
// tailwind.config.js
const cores = {
  primaria: {
    50: '#eff6ff',
    100: '#dbeafe', 
    500: '#3b82f6',
    600: '#2563eb',
    900: '#1e3a8a'
  },
  secundaria: {
    50: '#f0f9ff',
    500: '#06b6d4',
    600: '#0891b2'
  },
  sucesso: {
    50: '#f0fdf4',
    500: '#22c55e',
    600: '#16a34a'
  },
  erro: {
    50: '#fef2f2',
    500: '#ef4444',
    600: '#dc2626'
  },
  alerta: {
    50: '#fffbeb',
    500: '#f59e0b',
    600: '#d97706'
  },
  neutro: {
    50: '#f9fafb',
    100: '#f3f4f6',
    200: '#e5e7eb',
    500: '#6b7280',
    700: '#374151',
    900: '#111827'
  }
}
```

### **Tipografia:**
- **Títulos:** Inter ou similar (font-bold)
- **Corpo:** Inter ou similar (font-normal)
- **Código:** JetBrains Mono ou similar

---

## 📱 PÁGINAS OBRIGATÓRIAS

### **1. Páginas Públicas**

#### **Página de Login (/entrar)**
```typescript
// Componentes necessários:
- FormularioLogin
- LinkEsqueceuSenha
- LinkRegistrar
- LogoEmpresa

// Funcionalidades:
- Validação em tempo real
- Lembrar usuário
- Redirecionamento após login
- Mensagens de erro claras
```

#### **Página de Registro (/registrar)**
```typescript
// Tipos de registro:
- Administrador de Empresa
- Usuário Individual
- Aceitar Convite

// Validações obrigatórias:
- CNPJ em tempo real (BrasilAPI)
- Email único
- Senha forte
- Confirmação de senha
```

#### **Página Aceitar Convite (/aceitar-convite/[token])**
```typescript
// Funcionalidades:
- Validação de token
- Exibir dados do convite
- Formulário de definição de senha
- Confirmação de aceite
```

### **2. Páginas Autenticadas**

#### **Dashboard Principal (/painel)**
```typescript
// Componentes obrigatórios:
- ResumoEstatisticas (4 cards principais)
- GraficoAtividades (últimos 30 dias)
- ListaConvitesPendentes
- ListaContratosAtivos
- NotificacaoesRecentes

// Estatísticas por tipo de usuário:
// Admin Empresa: PJs contratados, usuários, contratos, faturamento
// PJ: Contratos ativos, pagamentos, projetos, clientes
```

#### **Gestão de Usuários (/usuarios)**
```typescript
// Funcionalidades:
- TabelaUsuarios com filtros
- ModalAdicionarUsuario
- ModalEditarUsuario
- ModalDetalhesUsuario
- BotaoConvidarUsuario
- FiltrosPorTipo (Admin, PJ, Funcionário)
- PaginacaoTabela
```

#### **Sistema de Convites (/convites)**
```typescript
// Abas obrigatórias:
- ConvitesPendentes
- ConvitesEnviados
- ConvitesAceitos
- ConvitesExpirados

// Funcionalidades:
- FormularioNovoConvite
- BotaoReenviarConvite
- BotaoCancelarConvite
- FiltrosPorTipo
- HistoricoConvites
```

#### **Gestão de Empresas (/empresas)**
```typescript
// Para Admin do Sistema:
- TabelaTodasEmpresas
- ModalDetalhesEmpresa
- FiltrosPorTipo
- RelatorioEmpresas

// Para Admin de Empresa:
- DetalhesMinhaEmpresa
- ListaPJsContratados
- RelacionamentosEmpresa
- ConfiguracoesEmpresa
```

---

## 🔧 FUNCIONALIDADES TÉCNICAS OBRIGATÓRIAS

### **Autenticação JWT:**
```typescript
// Context de Autenticação deve incluir:
interface ContextoAutenticacao {
  usuario: Usuario | null;
  empresa: Empresa | null;
  token: string | null;
  estaLogado: boolean;
  estaCarregando: boolean;
  fazerLogin: (email: string, senha: string) => Promise<void>;
  fazerLogout: () => void;
  atualizarUsuario: (dados: Usuario) => void;
  verificarToken: () => Promise<boolean>;
}
```

### **Interceptadores HTTP:**
```typescript
// Axios deve ter interceptadores para:
- Adicionar token automaticamente
- Refresh token automático
- Tratamento de erros 401/403
- Loading states globais
- Retry automático para falhas de rede
```

### **Validações em Tempo Real:**
```typescript
// Todas as validações devem ser em tempo real:
- CNPJ (integração com BrasilAPI)
- Email (verificação de formato e disponibilidade)
- CPF (validação de formato)
- CEP (busca automática de endereço)
- Telefone (formatação automática)
```

### **Estados de Loading:**
```typescript
// Implementar estados para:
- Carregamento de páginas
- Carregamento de formulários
- Carregamento de tabelas
- Carregamento de componentes
- Skeleton screens para melhor UX
```

---

## 🚀 INTEGRAÇÃO COM API BACKEND

### **Base URL de Desenvolvimento:**
```typescript
const API_BASE_URL = 'http://localhost:5203/api';
```

### **Endpoints Principais:**
```typescript
// Autenticação
POST /Auth/entrar
POST /Auth/sair
POST /Auth/renovar-token

// Usuários
GET /Users
GET /Users/{id}
PUT /Users/{id}
DELETE /Users/{id}

// Registro e Convites
POST /Registration/admin-empresa
POST /Registration/convidar-usuario
POST /Registration/aceitar-convite/{token}
GET /Registration/convites

// Empresas
GET /Companies
GET /Companies/{id}
PUT /Companies/{id}

// Relacionamentos
GET /UsersExtended/pjs-contratados
GET /UsersExtended/contratado-por
GET /UsersExtended/rede-completa
GET /CompanyRelationships/como-cliente
GET /CompanyRelationships/como-fornecedor
```

### **Tipos TypeScript para API:**
```typescript
// Todos os tipos devem corresponder exatamente às DTOs do backend:
interface Usuario {
  id: string;
  nome: string;
  email: string;
  papel: 'Admin' | 'Provider' | 'Employee';
  empresaId: string;
  criadoEm: string;
  atualizadoEm: string;
}

interface Empresa {
  id: string;
  nome: string;
  cnpj: string;
  tipo: 'Client' | 'Provider';
  modeloNegocio: 'MainCompany' | 'ContractedPJ';
  statusKyc: 'Pending' | 'Approved' | 'Rejected';
  criadaEm: string;
  atualizadaEm: string;
}

interface Convite {
  id: string;
  nomeConvidado: string;
  emailConvidado: string;
  tipoConvite: 'ContractedPJ' | 'Employee';
  nomeEmpresa: string;
  cnpj: string;
  tipoEmpresa: 'Client' | 'Provider';
  modeloNegocio: 'MainCompany' | 'ContractedPJ';
  token: string;
  expiraEm: string;
  estaExpirado: boolean;
  foiAceito: boolean;
}
```

---

## 🎯 FLUXOS DE USUÁRIO OBRIGATÓRIOS

### **Fluxo 1: Empresa Contratando PJ**
1. Admin faz login
2. Acessa "Convites" → "Novo Convite"
3. Preenche dados do PJ (nome, email, CNPJ da empresa PJ)
4. Sistema valida CNPJ em tempo real
5. Envia convite
6. PJ recebe email e aceita convite
7. Sistema cria usuário PJ + empresa PJ + relacionamento
8. Admin visualiza PJ na lista "PJs Contratados"

### **Fluxo 2: Visualização de Relacionamentos**
1. Admin acessa "Usuários" → "Rede Completa"
2. Vê todos os usuários da própria empresa
3. Vê todos os PJs contratados
4. Pode filtrar por tipo de relacionamento
5. Pode acessar detalhes de cada relacionamento

### **Fluxo 3: PJ Visualizando Contratos**
1. PJ faz login
2. Acessa "Contratos" → "Meus Contratos"
3. Vê empresas que o contrataram
4. Pode visualizar detalhes de cada contrato
5. Pode acessar informações da empresa contratante

---

## 🔒 SEGURANÇA OBRIGATÓRIA

### **Proteção de Rotas:**
```typescript
// Implementar middleware para:
- Verificar autenticação em rotas protegidas
- Verificar permissões por tipo de usuário
- Redirecionar usuários não autenticados
- Proteger rotas administrativas
```

### **Validação de Permissões:**
```typescript
// Por tipo de usuário:
Admin: Acesso total a sua empresa + convites + usuários
PJ: Acesso aos próprios contratos + empresas que o contrataram
Funcionário: Acesso limitado conforme permissões
```

---

## 🎨 UX/UI OBRIGATÓRIAS

### **Responsividade:**
- ✅ Design mobile-first
- ✅ Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- ✅ Menu lateral colapsa em mobile
- ✅ Tabelas com scroll horizontal em mobile
- ✅ Modais adaptados para mobile

### **Acessibilidade:**
- ✅ ARIA labels em todos os componentes
- ✅ Navegação por teclado
- ✅ Contraste de cores adequado
- ✅ Textos alternativos em imagens
- ✅ Focus indicators visíveis

### **Feedback Visual:**
- ✅ Loading states em todas as ações
- ✅ Mensagens de sucesso/erro claras
- ✅ Confirmações para ações destrutivas
- ✅ Indicadores de status visuais
- ✅ Tooltips informativos

---

## 📦 DEPENDÊNCIAS OBRIGATÓRIAS

```json
{
  "dependencies": {
    "next": "^14.0.0",
    "react": "^18.0.0",
    "react-dom": "^18.0.0",
    "typescript": "^5.0.0",
    "@radix-ui/react-dialog": "^1.0.5",
    "@radix-ui/react-dropdown-menu": "^2.0.6",
    "@radix-ui/react-label": "^2.0.2",
    "@radix-ui/react-select": "^2.0.0",
    "@radix-ui/react-slot": "^1.0.2",
    "@radix-ui/react-toast": "^1.1.5",
    "react-hook-form": "^7.47.0",
    "@hookform/resolvers": "^3.3.2",
    "zod": "^3.22.4",
    "zustand": "^4.4.6",
    "axios": "^1.6.0",
    "tailwindcss": "^3.3.5",
    "@tailwindcss/forms": "^0.5.7",
    "class-variance-authority": "^0.7.0",
    "clsx": "^2.0.0",
    "tailwind-merge": "^2.0.0",
    "lucide-react": "^0.292.0",
    "date-fns": "^2.30.0"
  },
  "devDependencies": {
    "@types/node": "^20.9.0",
    "@types/react": "^18.2.37",
    "@types/react-dom": "^18.2.15",
    "eslint": "^8.53.0",
    "eslint-config-next": "^14.0.0",
    "prettier": "^3.1.0",
    "prettier-plugin-tailwindcss": "^0.5.7"
  }
}
```

---

## 🚀 COMANDOS DE CONFIGURAÇÃO

```bash
# Criar projeto
npx create-next-app@latest aure-frontend --typescript --tailwind --eslint --app

# Instalar dependências adicionais
npm install @radix-ui/react-dialog @radix-ui/react-dropdown-menu @radix-ui/react-label @radix-ui/react-select @radix-ui/react-slot @radix-ui/react-toast react-hook-form @hookform/resolvers zod zustand axios @tailwindcss/forms class-variance-authority clsx tailwind-merge lucide-react date-fns

# Configurar Shadcn/ui
npx shadcn-ui@latest init
npx shadcn-ui@latest add button input label select dialog dropdown-menu toast card table
```

---

## ✅ CHECKLIST DE ENTREGA

### **Estrutura:**
- [ ] Todas as pastas criadas conforme especificação
- [ ] Nomenclatura 100% em português
- [ ] Componentes organizados por categoria
- [ ] Tipos TypeScript definidos

### **Funcionalidades Core:**
- [ ] Sistema de autenticação JWT completo
- [ ] Proteção de rotas implementada
- [ ] Validações em tempo real funcionando
- [ ] Integração com API backend

### **UI/UX:**
- [ ] Design system implementado
- [ ] Responsividade completa
- [ ] Acessibilidade básica
- [ ] Loading states em todas as ações

### **Páginas Principais:**
- [ ] Login/Registro funcionando
- [ ] Dashboard com estatísticas
- [ ] Sistema de convites completo
- [ ] Gestão de usuários
- [ ] Visualização de relacionamentos

### **Qualidade:**
- [ ] Código limpo (sem console.log)
- [ ] Tratamento de erros
- [ ] Validações robustas
- [ ] Performance otimizada

---

## 🎯 RESULTADO FINAL ESPERADO

Um frontend **completo, profissional e totalmente funcional** que:

1. **Integre perfeitamente** com a API backend existente
2. **Implemente todos os fluxos** do sistema Aure
3. **Seja componentizado** e reutilizável
4. **Use nomenclatura portuguesa** em todo o código
5. **Seja responsivo** e acessível
6. **Tenha alta qualidade** de código
7. **Seja fácil de manter** e expandir

**Este projeto deve estar pronto para produção imediatamente após a criação!**

---

## 🚨 OBSERVAÇÕES FINAIS

- **NÃO usar mocks** - integrar diretamente com a API
- **NÃO usar console.log** - usar logging adequado se necessário
- **NÃO usar termos em inglês** - toda nomenclatura em português
- **SIM componentizar tudo** - cada elemento deve ser um componente
- **SIM seguir padrões** - código limpo e organizado
- **SIM ser completo** - todos os fluxos implementados

**Agora crie este frontend completo seguindo exatamente estas especificações!**
- **UI Framework**: Tailwind CSS + shadcn/ui ou Mantine
- **Icons**: Lucide React ou React Icons
- **Tabelas**: TanStack Table v8
- **Gráficos**: Recharts ou Chart.js
- **Calendários**: React Day Picker
- **Notificações**: React Hot Toast
- **Datas**: date-fns ou day.js
- **Upload**: React Dropzone
- **PDF**: React-PDF
- **Testes**: Vitest + Testing Library

### Estrutura de Pastas
```
src/
├── components/
│   ├── ui/              # Componentes base (Button, Input, etc.)
│   ├── forms/           # Formulários específicos
│   ├── tables/          # Tabelas de dados
│   ├── charts/          # Gráficos e visualizações
│   ├── modals/          # Modais e dialogs
│   └── layout/          # Layout components
├── pages/               # Páginas da aplicação
├── hooks/               # Custom hooks
├── services/            # API calls e HTTP client
├── stores/              # Estado global (Zustand/Redux)
├── types/               # TypeScript types/interfaces
├── utils/               # Funções utilitárias
├── constants/           # Constantes da aplicação
├── config/              # Configurações
└── assets/              # Imagens, fonts, etc.
```

## 🔌 INTEGRAÇÃO COM API BACKEND

### Base URL e Autenticação
```typescript
const API_BASE_URL = 'http://localhost:5203/api'
// Headers: Authorization: Bearer {token}
// Content-Type: application/json
```

### 📊 TODOS OS ENDPOINTS DISPONÍVEIS

#### **1. 🔐 Autenticação - /api/auth**
```
POST /api/auth/entrar                    # Login
POST /api/auth/sair                      # Logout  
GET  /api/auth/perfil                    # Perfil do usuário
POST /api/auth/renovar-token             # Renovar token JWT
```

#### **2. 👥 Registro - /api/registration**
```
POST /api/registration/company-admin     # Registro empresa + admin
POST /api/registration/invite           # Convidar usuário
POST /api/registration/accept-invite/{token} # Aceitar convite
GET  /api/registration/invites          # Listar convites pendentes
```

#### **3. 👤 Usuários - /api/users**
```
GET  /api/users/{id}                    # Buscar usuário específico
PUT  /api/users/perfil                  # Atualizar perfil próprio
PATCH /api/users/senha                  # Trocar senha própria
```

#### **4. 🏢 Relacionamentos Empresariais - /api/companyrelationships**
```
GET  /api/companyrelationships          # Listar relacionamentos
POST /api/companyrelationships          # Criar relacionamento
GET  /api/companyrelationships/pjs      # Listar PJs contratados
PUT  /api/companyrelationships/{id}/activate   # Ativar PJ
PUT  /api/companyrelationships/{id}/deactivate # Desativar PJ
DELETE /api/companyrelationships/{id}   # Encerrar relacionamento
```

#### **5. 📄 Contratos - /api/contracts**
```
GET  /api/contracts                     # Listar contratos da empresa
GET  /api/contracts/dashboard          # Dashboard de contratos
GET  /api/contracts/receitas-mensais   # Receitas mensais
GET  /api/contracts/{id}               # Detalhes do contrato
POST /api/contracts                    # Criar contrato
POST /api/contracts/{id}/assinar       # Assinar contrato
```

#### **6. 💰 Pagamentos - /api/payments**
```
GET  /api/payments                     # Listar pagamentos
GET  /api/payments/dashboard          # Dashboard de pagamentos
POST /api/payments                    # Criar pagamento
GET  /api/payments/{id}               # Detalhes do pagamento
PUT  /api/payments/{id}/processar     # Processar pagamento
```

#### **7. 📊 Razão Contábil - /api/ledger**
```
GET  /api/ledger/extratos             # Extratos contábeis
GET  /api/ledger/balanco              # Balanço/saldo da empresa
GET  /api/ledger/relatorio-financeiro # Relatório financeiro detalhado
```

#### **8. 🪙 Ativos Tokenizados - /api/tokenizedassets**
```
GET  /api/tokenizedassets             # Listar ativos tokenizados
POST /api/tokenizedassets             # Tokenizar contrato
GET  /api/tokenizedassets/{id}        # Detalhes do ativo
PUT  /api/tokenizedassets/{id}/sync   # Sincronizar com blockchain
```

#### **9. 📋 Notas Fiscais - /api/invoices**
```
GET  /api/invoices                    # Listar notas fiscais
GET  /api/invoices/{id}               # Detalhes da nota fiscal
POST /api/invoices                    # Criar nota fiscal
PUT  /api/invoices/{id}/emitir        # Emitir nota fiscal
POST /api/invoices/{id}/cancelar      # Cancelar nota fiscal
GET  /api/invoices/{id}/xml           # Download XML da NFe
GET  /api/invoices/{id}/pdf           # Download PDF (DANFE)

# 🔥 SEFAZ Integration (NOVO!)
POST /api/invoices/{id}/emitir-sefaz      # Emitir via SEFAZ
POST /api/invoices/{id}/cancelar-sefaz    # Cancelar via SEFAZ  
GET  /api/invoices/{id}/status-sefaz      # Status na SEFAZ
GET  /api/invoices/validar-certificado-sefaz # Validar certificado
```

#### **10. 📈 Relatórios Fiscais - /api/taxreports**
```
GET  /api/taxreports/impostos         # Relatório de impostos
GET  /api/taxreports/livro-saidas     # Livro de registro de saídas
GET  /api/taxreports/sped-fiscal      # Dados para SPED Fiscal
GET  /api/taxreports/conciliacao-contabil # Conciliação contábil
```

#### **11. 🔍 Auditoria - /api/audit**
```
GET  /api/audit/logs                  # Logs de auditoria (Admin)
GET  /api/audit/kyc                   # Registros KYC da empresa
GET  /api/audit/relatorio-compliance  # Relatório de compliance (Admin)
GET  /api/audit/notificacoes         # Notificações da empresa
```

## 🎨 DESIGN SYSTEM E COMPONENTES

### Paleta de Cores (Sugestão)
```css
:root {
  /* Primary */
  --primary-50: #eff6ff;
  --primary-500: #3b82f6;
  --primary-600: #2563eb;
  --primary-700: #1d4ed8;
  
  /* Secondary */
  --secondary-500: #10b981;
  --secondary-600: #059669;
  
  /* Neutral */
  --gray-50: #f9fafb;
  --gray-100: #f3f4f6;
  --gray-500: #6b7280;
  --gray-900: #111827;
  
  /* Status */
  --success: #10b981;
  --warning: #f59e0b;
  --error: #ef4444;
  --info: #3b82f6;
}
```

### 🧩 Componentes Base Necessários

#### Formulários
- `InputField` - Input com label, erro e validação
- `SelectField` - Select customizado com search
- `DatePicker` - Seletor de data
- `FileUploader` - Upload de arquivos com preview
- `FormProvider` - Context para formulários
- `PasswordInput` - Input de senha com toggle

#### Navegação
- `Sidebar` - Menu lateral responsivo
- `TopBar` - Barra superior com notificações
- `Breadcrumb` - Navegação estrutural
- `Pagination` - Paginação de tabelas

#### Feedback
- `LoadingSpinner` - Indicador de carregamento
- `Toast` - Notificações temporárias
- `Modal` - Modais reutilizáveis
- `ConfirmDialog` - Confirmação de ações
- `EmptyState` - Estados vazios

#### Dados
- `DataTable` - Tabela com filtros, ordenação, paginação
- `StatsCard` - Cards de estatísticas
- `Chart` - Gráficos responsivos
- `Badge` - Status badges
- `StatusIndicator` - Indicadores de status

## 📱 PÁGINAS E FUNCIONALIDADES

### 🔐 **1. Autenticação**
```
/login                    # Página de login
/register                 # Registro de empresa
/accept-invite/:token     # Aceitar convite
/forgot-password          # Recuperar senha
```

**Componentes necessários:**
- `LoginForm` - Formulário de login com validação
- `RegisterForm` - Registro completo empresa + admin
- `InviteAcceptForm` - Aceitar convite e definir senha
- `ForgotPasswordForm` - Recuperação de senha

### 🏠 **2. Dashboard**
```
/dashboard               # Dashboard principal
```

**Métricas principais:**
- Total de contratos (ativos, draft, concluídos)
- Receita total e mensal
- Pagamentos pendentes/processados
- Notas fiscais emitidas
- Gráficos de receita por período
- Contratos próximos do vencimento
- Notificações importantes

### 👥 **3. Usuários e Relacionamentos**
```
/users                   # Listagem de usuários da rede
/users/profile          # Perfil do usuário atual
/relationships          # Relacionamentos empresariais
/relationships/pjs      # PJs contratados
/invites               # Convites pendentes
```

**Funcionalidades:**
- Listar usuários da rede de relacionamentos
- Editar perfil próprio
- Trocar senha
- Gerenciar relacionamentos empresa-PJ
- Ativar/desativar PJs
- Enviar e gerenciar convites

### 📄 **4. Contratos**
```
/contracts              # Listagem de contratos
/contracts/new         # Criar novo contrato
/contracts/:id         # Detalhes do contrato
/contracts/:id/edit    # Editar contrato (se draft)
```

**Funcionalidades:**
- CRUD completo de contratos
- Assinatura digital de contratos
- Filtros por status, cliente, fornecedor
- Upload de documentos
- Histórico de alterações
- Tokenização blockchain

### 💰 **5. Pagamentos**
```
/payments              # Listagem de pagamentos
/payments/new         # Criar pagamento
/payments/:id         # Detalhes do pagamento
```

**Funcionalidades:**
- Criar pagamentos vinculados a contratos
- Processar pagamentos (PIX, TED, etc.)
- Regras de split automático
- Histórico de transações
- Conciliação bancária
- Relatórios financeiros

### 📋 **6. Notas Fiscais (COM SEFAZ!)**
```
/invoices              # Listagem de notas fiscais
/invoices/new         # Criar nota fiscal
/invoices/:id         # Detalhes da nota fiscal
/invoices/:id/sefaz   # Status SEFAZ da nota
```

**Funcionalidades COMPLETAS:**
- CRUD de notas fiscais
- **Emissão via SEFAZ** (integração real!)
- **Cancelamento via SEFAZ**
- **Consulta de status na SEFAZ**
- Download de XML (NFe)
- Download de PDF (DANFE)
- Validação de certificado digital
- Filtros por status, período, contrato
- Itens da nota fiscal com NCM
- Cálculos automáticos de impostos

### 📊 **7. Razão Contábil**
```
/ledger/statements     # Extratos contábeis
/ledger/balance       # Balanço patrimonial
/ledger/reports       # Relatórios financeiros
```

**Funcionalidades:**
- Extratos contábeis detalhados
- Balanço patrimonial
- DRE (Demonstração de Resultado)
- Conciliação contábil
- Filtros por período, contrato
- Export para Excel/PDF

### 🪙 **8. Ativos Tokenizados**
```
/tokenized-assets      # Listagem de ativos
/tokenized-assets/:id  # Detalhes do ativo
```

**Funcionalidades:**
- Tokenizar contratos na blockchain
- Acompanhar status de tokenização
- Sincronização com blockchain
- Histórico de transações on-chain
- Metadados dos tokens

### 📈 **9. Relatórios Fiscais**
```
/tax-reports/taxes     # Relatório de impostos
/tax-reports/sped     # SPED Fiscal
/tax-reports/books    # Livros fiscais
```

**Funcionalidades:**
- Relatórios de impostos por período
- Livro de registro de saídas
- Dados para SPED Fiscal
- Conciliação contábil
- Export para TXT/XML (SPED)

### 🔍 **10. Auditoria e Compliance**
```
/audit/logs           # Logs de auditoria (Admin)
/audit/kyc           # Status KYC da empresa
/audit/compliance    # Relatórios de compliance
/notifications       # Central de notificações
```

**Funcionalidades:**
- Trilha completa de auditoria
- Gestão de KYC empresarial
- Relatórios de compliance
- Central de notificações
- Filtros avançados por ação, usuário, período

## 🛠️ FUNCIONALIDADES TÉCNICAS ESPECÍFICAS

### Autenticação JWT
```typescript
// Interceptor Axios
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Refresh token automático
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Redirecionar para login
    }
    return Promise.reject(error);
  }
);
```

### Estado Global (Exemplo com Zustand)
```typescript
interface AuthStore {
  user: User | null;
  token: string | null;
  login: (credentials: LoginData) => Promise<void>;
  logout: () => void;
  updateProfile: (data: UpdateProfileData) => Promise<void>;
}

interface CompanyStore {
  currentCompany: Company | null;
  relationships: CompanyRelationship[];
  fetchRelationships: () => Promise<void>;
}
```

### Validação de Formulários (Zod + React Hook Form)
```typescript
const createContractSchema = z.object({
  title: z.string().min(1, 'Título é obrigatório'),
  clientId: z.string().uuid('Cliente é obrigatório'),
  providerId: z.string().uuid('Fornecedor é obrigatório'),
  valueTotal: z.number().positive('Valor deve ser positivo'),
  expirationDate: z.date().min(new Date(), 'Data deve ser futura'),
});
```

### Componentes de Loading States
```typescript
interface LoadingProps {
  isLoading: boolean;
  error?: string;
  children: React.ReactNode;
}

const LoadingWrapper: React.FC<LoadingProps> = ({ isLoading, error, children }) => {
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  return <>{children}</>;
};
```

## 🎯 FUNCIONALIDADES ESPECÍFICAS POR ÁREA

### Dashboard Inteligente
- **KPIs em tempo real**: Contratos, pagamentos, receitas
- **Gráficos interativos**: Receita mensal, distribuição de contratos
- **Alertas**: Contratos vencendo, pagamentos pendentes
- **Quick actions**: Criar contrato, emitir nota fiscal
- **Timeline**: Atividades recentes

### Gestão de Contratos Avançada
- **Editor de contratos**: Rich text editor para conteúdo
- **Versionamento**: Controle de versões dos contratos
- **Assinatura digital**: Fluxo completo de assinatura
- **Notificações**: Alertas de vencimento
- **Templates**: Templates pré-definidos

### Sistema de Pagamentos Completo
- **Split automático**: Configuração de regras de divisão
- **Múltiplos métodos**: PIX, TED, cartão, boleto
- **Conciliação**: Matching automático com extratos
- **Relatórios**: Análises financeiras detalhadas
- **Compliance**: Controles anti-lavagem

### SEFAZ Integration (DESTAQUE!)
- **Emissão real**: Integração direta com SEFAZ
- **Certificado digital**: Validação e status
- **Status em tempo real**: Consulta na SEFAZ
- **XML/PDF**: Download automático
- **Cancelamento**: Fluxo completo de cancelamento
- **Múltiplos estados**: SP, RJ, MG, etc.

### Auditoria e Compliance
- **Trilha completa**: Todos os eventos auditados
- **KYC empresarial**: Verificação de documentos
- **Relatórios**: Compliance automático
- **Alertas**: Não conformidades

## 🚀 TECNOLOGIAS E BIBLIOTECAS ESPECÍFICAS

### Core
```json
{
  "react": "^18.2.0",
  "typescript": "^5.0.0",
  "vite": "^4.4.0",
  "react-router-dom": "^6.15.0"
}
```

### Estado e HTTP
```json
{
  "zustand": "^4.4.0",
  "axios": "^1.5.0",
  "@tanstack/react-query": "^4.32.0"
}
```

### UI e Styling
```json
{
  "tailwindcss": "^3.3.0",
  "@headlessui/react": "^1.7.0",
  "clsx": "^2.0.0",
  "lucide-react": "^0.274.0"
}
```

### Formulários e Validação
```json
{
  "react-hook-form": "^7.45.0",
  "zod": "^3.22.0",
  "@hookform/resolvers": "^3.3.0"
}
```

### Tabelas e Visualização
```json
{
  "@tanstack/react-table": "^8.9.0",
  "recharts": "^2.8.0",
  "react-day-picker": "^8.8.0"
}
```

### Utilitários
```json
{
  "date-fns": "^2.30.0",
  "react-hot-toast": "^2.4.0",
  "react-dropzone": "^14.2.0",
  "react-pdf": "^7.3.0"
}
```

## 📋 TIPOS TYPESCRIPT PRINCIPAIS

### Entidades Base
```typescript
interface User {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'Company' | 'Provider';
  companyId?: string;
  createdAt: string;
  updatedAt: string;
}

interface Company {
  id: string;
  name: string;
  cnpj: string;
  type: 'Client' | 'Provider' | 'Both';
  businessModel: 'Standard' | 'MainCompany' | 'ContractedPJ' | 'Freelancer';
  kycStatus: 'Pending' | 'Approved' | 'Rejected';
  createdAt: string;
  updatedAt: string;
}

interface Contract {
  id: string;
  clientId: string;
  providerId: string;
  title: string;
  valueTotal: number;
  status: 'Draft' | 'Active' | 'Completed' | 'Cancelled';
  expirationDate?: string;
  signedDate?: string;
  ipfsCid?: string;
  sha256Hash?: string;
  createdAt: string;
  updatedAt: string;
  client: Company;
  provider: Company;
}

interface Payment {
  id: string;
  contractId: string;
  amount: number;
  method: 'PIX' | 'TED' | 'CreditCard' | 'Boleto';
  status: 'Pending' | 'Completed' | 'Failed' | 'Cancelled';
  paymentDate?: string;
  createdAt: string;
  contract: Contract;
}

interface Invoice {
  id: string;
  contractId: string;
  paymentId?: string;
  invoiceNumber: string;
  series: string;
  accessKey: string;
  issueDate: string;
  dueDate?: string;
  totalAmount: number;
  taxAmount: number;
  status: 'Draft' | 'Issued' | 'Sent' | 'Cancelled' | 'Error';
  xmlContent?: string;
  pdfUrl?: string;
  cancellationReason?: string;
  sefazProtocol?: string;
  createdAt: string;
  updatedAt: string;
  contract: Contract;
}
```

### Requests/Responses
```typescript
interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
  user: User;
  company?: Company;
}

interface CreateContractRequest {
  clientId: string;
  providerId: string;
  title: string;
  valueTotal: number;
  expirationDate?: string;
}

interface SefazResponse {
  success: boolean;
  protocol?: string;
  accessKey?: string;
  message?: string;
  errorCode?: string;
  processedAt: string;
}
```

## 🎨 EXEMPLOS DE COMPONENTES

### Dashboard Card
```typescript
interface StatsCardProps {
  title: string;
  value: string | number;
  change?: number;
  icon: React.ComponentType<{ className?: string }>;
  color?: 'blue' | 'green' | 'yellow' | 'red';
}

const StatsCard: React.FC<StatsCardProps> = ({ title, value, change, icon: Icon, color = 'blue' }) => {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <p className="text-2xl font-bold text-gray-900">{value}</p>
          {change && (
            <p className={`text-sm ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
              {change >= 0 ? '+' : ''}{change}%
            </p>
          )}
        </div>
        <Icon className={`h-8 w-8 text-${color}-600`} />
      </div>
    </div>
  );
};
```

### Data Table com Filtros
```typescript
interface DataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
  searchable?: boolean;
  filterable?: boolean;
  onRowClick?: (row: T) => void;
}

const DataTable = <T,>({ data, columns, searchable, filterable, onRowClick }: DataTableProps<T>) => {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [filtering, setFiltering] = useState('');
  
  const table = useReactTable({
    data,
    columns,
    state: { sorting, globalFilter: filtering },
    onSortingChange: setSorting,
    onGlobalFilterChange: setFiltering,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
  });

  return (
    <div className="space-y-4">
      {searchable && (
        <Input
          placeholder="Buscar..."
          value={filtering}
          onChange={(e) => setFiltering(e.target.value)}
        />
      )}
      
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          {/* Header */}
          <thead className="bg-gray-50">
            {table.getHeaderGroups().map(headerGroup => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map(header => (
                  <th key={header.id} className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    {flexRender(header.column.columnDef.header, header.getContext())}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          
          {/* Body */}
          <tbody className="bg-white divide-y divide-gray-200">
            {table.getRowModel().rows.map(row => (
              <tr 
                key={row.id} 
                className={onRowClick ? 'cursor-pointer hover:bg-gray-50' : ''}
                onClick={() => onRowClick?.(row.original)}
              >
                {row.getVisibleCells().map(cell => (
                  <td key={cell.id} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
```

## 🔥 FUNCIONALIDADES CRÍTICAS

### 1. **Sistema de Notificações em Tempo Real**
- WebSocket ou Server-Sent Events
- Notificações push no browser
- Central de notificações persistente
- Filtros por tipo e importância

### 2. **Upload e Gestão de Arquivos**
- Drag & drop para contratos
- Preview de PDFs
- Versionamento de documentos
- Compressão automática

### 3. **Exportação de Relatórios**
- Excel, PDF, CSV
- Agendamento de relatórios
- Templates customizáveis
- Envio por email

### 4. **Busca Global Inteligente**
- Busca fuzzy across entities
- Filtros avançados
- Histórico de buscas
- Sugestões automáticas

### 5. **Tema Escuro/Claro**
- Toggle de tema
- Persistência da preferência
- Smooth transitions
- Variáveis CSS dinâmicas

## 🔐 SEGURANÇA E VALIDAÇÕES

### Client-Side Security
- Sanitização de inputs (DOMPurify)
- Validação rigorosa com Zod
- Rate limiting visual feedback
- XSS protection
- CSRF tokens em formulários

### Validações Específicas
- CNPJ validation
- Email format
- Password strength
- File type validation
- Max file sizes

## 📱 RESPONSIVIDADE E ACESSIBILIDADE

### Mobile-First Design
- Breakpoints: 320px, 768px, 1024px, 1280px
- Touch-friendly buttons (min 44px)
- Swipe gestures para tabelas
- Modais full-screen em mobile

### Acessibilidade (WCAG 2.1)
- Semantic HTML
- ARIA labels completos
- Keyboard navigation
- Screen reader support
- Alto contraste
- Focus management

## 🎯 OBJETIVOS DE PERFORMANCE

### Core Web Vitals
- LCP < 2.5s (Lazy loading)
- FID < 100ms (Code splitting)
- CLS < 0.1 (Skeleton loaders)

### Otimizações
- React.memo para componentes pesados
- useMemo/useCallback estratégicos
- Virtualização para listas grandes
- Prefetch de dados críticos
- Service Worker para cache

## 📦 ESTRUTURA DE DEPLOY

### Build & Deploy
- Vite build otimizado
- Environment variables
- CI/CD pipeline ready
- Docker containerization
- CDN para assets estáticos

---

## 🚀 RESUMO EXECUTIVO

Este prompt cria um **sistema fintech completo** com:

✅ **16+ páginas funcionais** integradas com todos os endpoints
✅ **Integração SEFAZ real** para emissão de notas fiscais
✅ **Dashboard inteligente** com KPIs e gráficos
✅ **Sistema de autenticação** JWT completo
✅ **CRUD completo** para todas as entidades
✅ **Design system** profissional e responsivo
✅ **TypeScript** tipado end-to-end
✅ **Performance otimizada** com lazy loading
✅ **Acessibilidade WCAG** 2.1 compliant
✅ **Mobile-first** responsive design
✅ **Testes automatizados** com Vitest
✅ **Arquitetura escalável** com clean code

### 🎯 **Principais Diferenciais:**
1. **SEFAZ Integration** - Emissão real de notas fiscais
2. **Blockchain Support** - Tokenização de contratos
3. **Split Payments** - Divisão automática de pagamentos
4. **Audit Trail** - Trilha completa de auditoria
5. **KYC Management** - Verificação empresarial
6. **Advanced Reporting** - SPED, DRE, Balanço
7. **Real-time Notifications** - WebSocket integration
8. **Professional UI** - Design system completo

**O resultado será um sistema fintech de nível empresarial, pronto para produção!**