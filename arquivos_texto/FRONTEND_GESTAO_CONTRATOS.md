# Frontend - Gestão de Contratos

## 📋 Objetivo

Criar interface para listar, visualizar e deletar contratos na aplicação.

---

## 🎯 Funcionalidades

### 1. Listagem de Contratos
- Exibir todos os contratos da empresa
- Filtrar por status (Draft, Active, Expired, Cancelled)
- Mostrar informações resumidas de cada contrato
- Indicar se a empresa é Cliente ou Fornecedora

### 2. Visualização de Contrato
- Ver detalhes completos do contrato
- Baixar PDF do contrato (se disponível)
- Ver histórico de status

### 3. Deleção de Contratos Draft
- Permitir deletar apenas contratos com status **Draft**
- Confirmar deleção com modal
- Exibir mensagem de erro se tentar deletar contrato não-Draft

---

## 📡 Endpoints Disponíveis

### Listar Contratos
```typescript
GET /api/Contracts?status={status}

// Parâmetros de query (opcional)
status?: "Draft" | "Active" | "Expired" | "Cancelled"

// Response
{
  "totalContratos": 5,
  "filtradoPor": "Todos",
  "contratos": [
    {
      "id": "065fbde6-3436-4529-b8bd-306c82f12b25",
      "titulo": "Prestação de Serviços de Software",
      "valorTotal": 60000,
      "status": "Active",
      "criadoEm": "2025-12-04T17:09:23.596119Z",
      "atualizadoEm": "2025-12-04T17:09:23.596119Z",
      "empresaCliente": {
        "id": "a4af06e0-0c0c-4007-94e8-5ec43cbadda6",
        "nome": "Petrobras S.A.",
        "cnpj": "12345678000100"
      },
      "empresaFornecedora": {
        "id": "08cf3363-71a6-4d80-812f-d04b990a998a",
        "nome": "Empresa PJ LTDA",
        "cnpj": "98765432000199"
      },
      "ehCliente": true,
      "ehFornecedor": false
    }
  ]
}
```

### Deletar Contrato Draft
```typescript
DELETE /api/Contracts/{id}

// Headers
Authorization: Bearer {token}

// Response Success (200)
{
  "message": "Contrato Draft deletado com sucesso",
  "contratoId": "065fbde6-3436-4529-b8bd-306c82f12b25"
}

// Response Error - Não é Draft (400)
{
  "message": "Apenas contratos com status Draft podem ser deletados. Status atual: Active",
  "statusAtual": "Active"
}

// Response Error - Não encontrado (404)
{
  "message": "Contrato não encontrado"
}

// Response Error - Sem permissão (403)
// Sem body, apenas status 403
```

---

## 🎨 Estrutura de Componentes Sugerida

```
app/
├── contratos/
│   ├── page.tsx                    // Página principal - Listagem
│   ├── [id]/
│   │   └── page.tsx               // Página de detalhes do contrato
│   └── components/
│       ├── ListaContratos.tsx     // Tabela/Grid de contratos
│       ├── FiltroStatus.tsx       // Filtro por status
│       ├── CardContrato.tsx       // Card individual do contrato
│       ├── ModalVisualizacao.tsx  // Modal para ver detalhes
│       └── ModalConfirmarDelete.tsx // Modal de confirmação
```

---

## 💻 Implementação - Service

### `src/services/api/contracts.ts`

```typescript
import { api } from './api';

export interface Contrato {
  id: string;
  titulo: string;
  valorTotal: number;
  status: 'Draft' | 'Active' | 'Expired' | 'Cancelled';
  criadoEm: string;
  atualizadoEm: string;
  empresaCliente: {
    id: string;
    nome: string;
    cnpj: string;
  };
  empresaFornecedora: {
    id: string;
    nome: string;
    cnpj: string;
  };
  ehCliente: boolean;
  ehFornecedor: boolean;
}

export interface ListarContratosResponse {
  totalContratos: number;
  filtradoPor: string;
  contratos: Contrato[];
}

export interface DeletarContratoResponse {
  message: string;
  contratoId: string;
}

export const contractsService = {
  /**
   * Listar contratos da empresa
   */
  listar: async (status?: string): Promise<ListarContratosResponse> => {
    const params = status ? { status } : {};
    const response = await api.get<ListarContratosResponse>('/Contracts', { params });
    return response.data;
  },

  /**
   * Buscar contrato por ID
   */
  buscarPorId: async (id: string): Promise<Contrato> => {
    const response = await api.get<Contrato>(`/Contracts/${id}`);
    return response.data;
  },

  /**
   * Deletar contrato Draft
   */
  deletar: async (id: string): Promise<DeletarContratoResponse> => {
    const response = await api.delete<DeletarContratoResponse>(`/Contracts/${id}`);
    return response.data;
  },
};
```

---

## 💻 Implementação - Página de Listagem

### `app/contratos/page.tsx`

```typescript
'use client';

import { useState, useEffect } from 'react';
import { contractsService, Contrato } from '@/services/api/contracts';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Trash2, Eye, Filter } from 'lucide-react';
import { toast } from 'sonner';

type StatusFilter = 'Todos' | 'Draft' | 'Active' | 'Expired' | 'Cancelled';

export default function ContratosPage() {
  const [contratos, setContratos] = useState<Contrato[]>([]);
  const [loading, setLoading] = useState(true);
  const [filtroStatus, setFiltroStatus] = useState<StatusFilter>('Todos');
  const [contratoParaDeletar, setContratoParaDeletar] = useState<string | null>(null);

  useEffect(() => {
    carregarContratos();
  }, [filtroStatus]);

  const carregarContratos = async () => {
    try {
      setLoading(true);
      const statusParam = filtroStatus === 'Todos' ? undefined : filtroStatus;
      const response = await contractsService.listar(statusParam);
      setContratos(response.contratos);
    } catch (error) {
      console.error('Erro ao carregar contratos:', error);
      toast.error('Erro ao carregar contratos');
    } finally {
      setLoading(false);
    }
  };

  const handleDeletar = async (id: string) => {
    try {
      await contractsService.deletar(id);
      toast.success('Contrato deletado com sucesso');
      setContratoParaDeletar(null);
      carregarContratos();
    } catch (error: any) {
      const mensagem = error.response?.data?.message || 'Erro ao deletar contrato';
      toast.error(mensagem);
      setContratoParaDeletar(null);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Draft':
        return 'bg-gray-500';
      case 'Active':
        return 'bg-green-500';
      case 'Expired':
        return 'bg-yellow-500';
      case 'Cancelled':
        return 'bg-red-500';
      default:
        return 'bg-gray-500';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Draft':
        return 'Rascunho';
      case 'Active':
        return 'Ativo';
      case 'Expired':
        return 'Expirado';
      case 'Cancelled':
        return 'Cancelado';
      default:
        return status;
    }
  };

  const formatarValor = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(valor);
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold">Contratos</h1>
          <p className="text-muted-foreground mt-1">
            Gerencie os contratos da sua empresa
          </p>
        </div>
      </div>

      {/* Filtros */}
      <div className="flex gap-2 mb-6 flex-wrap">
        {(['Todos', 'Draft', 'Active', 'Expired', 'Cancelled'] as StatusFilter[]).map((status) => (
          <Button
            key={status}
            variant={filtroStatus === status ? 'default' : 'outline'}
            onClick={() => setFiltroStatus(status)}
            size="sm"
          >
            <Filter className="w-4 h-4 mr-2" />
            {status === 'Todos' ? 'Todos' : getStatusLabel(status)}
          </Button>
        ))}
      </div>

      {/* Lista de Contratos */}
      {contratos.length === 0 ? (
        <div className="text-center py-12 bg-muted/50 rounded-lg">
          <p className="text-muted-foreground">Nenhum contrato encontrado</p>
        </div>
      ) : (
        <div className="grid gap-4">
          {contratos.map((contrato) => (
            <div
              key={contrato.id}
              className="bg-card border rounded-lg p-6 hover:shadow-md transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="text-xl font-semibold">{contrato.titulo}</h3>
                    <Badge className={getStatusColor(contrato.status)}>
                      {getStatusLabel(contrato.status)}
                    </Badge>
                    {contrato.ehCliente && (
                      <Badge variant="outline" className="bg-blue-50">
                        Cliente
                      </Badge>
                    )}
                    {contrato.ehFornecedor && (
                      <Badge variant="outline" className="bg-purple-50">
                        Fornecedor
                      </Badge>
                    )}
                  </div>

                  <div className="grid grid-cols-2 gap-4 mt-4 text-sm">
                    <div>
                      <p className="text-muted-foreground">Empresa Cliente</p>
                      <p className="font-medium">{contrato.empresaCliente.nome}</p>
                      <p className="text-xs text-muted-foreground">
                        CNPJ: {contrato.empresaCliente.cnpj}
                      </p>
                    </div>
                    <div>
                      <p className="text-muted-foreground">Empresa Fornecedora</p>
                      <p className="font-medium">{contrato.empresaFornecedora.nome}</p>
                      <p className="text-xs text-muted-foreground">
                        CNPJ: {contrato.empresaFornecedora.cnpj}
                      </p>
                    </div>
                  </div>

                  <div className="flex gap-6 mt-4 text-sm">
                    <div>
                      <p className="text-muted-foreground">Valor Total</p>
                      <p className="font-bold text-lg text-primary">
                        {formatarValor(contrato.valorTotal)}
                      </p>
                    </div>
                    <div>
                      <p className="text-muted-foreground">Criado em</p>
                      <p className="font-medium">{formatarData(contrato.criadoEm)}</p>
                    </div>
                    <div>
                      <p className="text-muted-foreground">Atualizado em</p>
                      <p className="font-medium">{formatarData(contrato.atualizadoEm)}</p>
                    </div>
                  </div>
                </div>

                <div className="flex gap-2 ml-4">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => window.open(`/contratos/${contrato.id}`, '_blank')}
                  >
                    <Eye className="w-4 h-4 mr-2" />
                    Visualizar
                  </Button>

                  {contrato.status === 'Draft' && (
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => setContratoParaDeletar(contrato.id)}
                    >
                      <Trash2 className="w-4 h-4 mr-2" />
                      Deletar
                    </Button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal de Confirmação */}
      {contratoParaDeletar && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-card p-6 rounded-lg max-w-md w-full mx-4">
            <h2 className="text-xl font-bold mb-4">Confirmar Deleção</h2>
            <p className="text-muted-foreground mb-6">
              Tem certeza que deseja deletar este contrato? Esta ação não pode ser desfeita.
            </p>
            <div className="flex gap-3 justify-end">
              <Button
                variant="outline"
                onClick={() => setContratoParaDeletar(null)}
              >
                Cancelar
              </Button>
              <Button
                variant="destructive"
                onClick={() => handleDeletar(contratoParaDeletar)}
              >
                Deletar Contrato
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
```

---

## 🎨 Componentes Auxiliares (Opcional)

### Badge de Status
```typescript
interface StatusBadgeProps {
  status: 'Draft' | 'Active' | 'Expired' | 'Cancelled';
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const config = {
    Draft: { label: 'Rascunho', color: 'bg-gray-100 text-gray-800' },
    Active: { label: 'Ativo', color: 'bg-green-100 text-green-800' },
    Expired: { label: 'Expirado', color: 'bg-yellow-100 text-yellow-800' },
    Cancelled: { label: 'Cancelado', color: 'bg-red-100 text-red-800' },
  };

  const { label, color } = config[status];

  return (
    <span className={`px-2 py-1 rounded-full text-xs font-medium ${color}`}>
      {label}
    </span>
  );
}
```

---

## 📱 Responsividade

### Mobile
```typescript
// No CardContrato, usar layout em coluna
<div className="flex flex-col gap-4 md:flex-row md:justify-between">
  {/* Conteúdo */}
</div>

// Botões em linha no mobile
<div className="flex gap-2 w-full md:w-auto">
  <Button className="flex-1 md:flex-none">Visualizar</Button>
  <Button className="flex-1 md:flex-none">Deletar</Button>
</div>
```

---

## ✅ Checklist de Implementação

### Backend (Já Implementado)
- [x] Endpoint GET /api/Contracts (listar)
- [x] Endpoint DELETE /api/Contracts/{id} (deletar Draft)
- [x] Validações de permissão
- [x] Validação de status Draft
- [x] Logs de auditoria

### Frontend (A Implementar)
- [ ] Criar service `contracts.ts`
- [ ] Criar página `app/contratos/page.tsx`
- [ ] Implementar listagem com filtros
- [ ] Implementar botão de deleção
- [ ] Criar modal de confirmação
- [ ] Adicionar tratamento de erros
- [ ] Adicionar loading states
- [ ] Implementar toasts/notificações
- [ ] Testar responsividade
- [ ] Adicionar paginação (opcional)

---

## 🔒 Permissões

### Quem pode deletar contratos?
- **DonoEmpresaPai**: Sim
- **Juridico**: Sim
- **Financeiro**: Não
- **FuncionarioCLT**: Não
- **FuncionarioPJ**: Não

### Validações
1. Apenas contratos **Draft** podem ser deletados
2. Usuário deve pertencer à empresa (cliente ou fornecedora)
3. Contrato deve existir no banco de dados

---

## 🧪 Testes

### Casos de Teste

1. **Listar contratos**
   - Sem filtro (todos)
   - Com filtro por status
   - Lista vazia

2. **Deletar contrato Draft**
   - Sucesso
   - Erro: contrato não é Draft
   - Erro: contrato não encontrado
   - Erro: sem permissão

3. **Visualizar contrato**
   - Abrir em nova aba
   - Ver detalhes

4. **Filtros**
   - Trocar entre status
   - Verificar contagem

---

## 📚 Recursos Adicionais

### Melhorias Futuras
- Paginação da lista
- Busca por título/CNPJ
- Exportar lista para Excel/PDF
- Ordenação por coluna
- Download de PDF do contrato
- Histórico de alterações
- Notificações de vencimento

### Bibliotecas Úteis
- `lucide-react` - Ícones
- `sonner` - Toasts/notificações
- `shadcn/ui` - Componentes UI
- `react-query` - Cache e refetch automático
- `date-fns` - Formatação de datas

---

## 🚀 Deploy

Após implementar o frontend:

1. Testar localmente com backend em produção
2. Verificar tratamento de erros
3. Testar todos os filtros
4. Validar permissões
5. Deploy do frontend

---

**Data de Criação**: 04/12/2025  
**Última Atualização**: 04/12/2025  
**Status**: Pronto para Implementação Frontend
