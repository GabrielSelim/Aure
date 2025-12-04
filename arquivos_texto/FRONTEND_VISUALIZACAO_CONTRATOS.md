# Frontend - Visualização de Contratos com Preview Salvo

## 📋 O que mudou no Backend

### ✅ Preview Automático Salvo
Agora, quando um contrato é criado, o sistema **automaticamente gera e salva** o HTML de preview no banco de dados. Isso significa que você pode visualizar contratos Draft a qualquer momento!

**Antes:**
- Contratos Draft não tinham visualização
- Precisava usar Preview temporário antes de gerar

**Agora:**
- Todo contrato criado tem preview HTML salvo automaticamente
- Endpoint `/api/Contracts/{id}/visualizar` retorna o HTML completo
- Funciona para contratos Draft, Active, Expired e Cancelled

---

## 🆕 Novo Endpoint de Visualização

### `GET /api/Contracts/{id}/visualizar`

**Headers:**
```http
Authorization: Bearer {token}
```

**Response (Success 200):**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Contrato de Prestação de Serviços</title>
    <style>
        /* Estilos do contrato */
    </style>
</head>
<body>
    <!-- HTML completo do contrato renderizado -->
    <h1>CONTRATO DE PRESTAÇÃO DE SERVIÇOS</h1>
    <p>Entre CONTRATANTE e CONTRATADO...</p>
    <!-- ... resto do documento -->
</body>
</html>
```

**Response (Preview não disponível):**
```html
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; text-align: center; }
        h1 { color: #dc2626; }
        p { color: #666; margin: 20px 0; }
    </style>
</head>
<body>
    <h1>⚠️ Preview não disponível</h1>
    <p>Este contrato não possui preview salvo.</p>
    <p>O preview é gerado automaticamente ao criar o contrato.</p>
    <p>Se você acabou de criar o contrato, tente recarregar a página.</p>
</body>
</html>
```

**Response (404):**
```json
{
  "message": "Contrato não encontrado"
}
```

**Response (403):**
```
Forbidden (usuário não pertence à empresa do contrato)
```

---

## 💻 Implementação Frontend

### 1. Atualizar Service de Contratos

**Arquivo: `src/services/api/contracts.ts`**

Adicionar novo método:

```typescript
export const contractsService = {
  // ... métodos existentes (listar, buscarPorId, deletar)

  /**
   * Obter URL para visualizar HTML do contrato
   * @param id ID do contrato
   * @returns URL completa para abrir o HTML
   */
  obterUrlVisualizacao: (id: string): string => {
    const baseURL = process.env.NEXT_PUBLIC_API_URL || 'https://aureapi.gabrielsanztech.com.br/api';
    return `${baseURL}/Contracts/${id}/visualizar`;
  },

  /**
   * Visualizar contrato em nova aba (abre o HTML diretamente)
   * @param id ID do contrato
   * @param token Token de autenticação
   */
  visualizarEmNovaAba: (id: string, token: string) => {
    const url = contractsService.obterUrlVisualizacao(id);
    
    // Criar form temporário para enviar token via POST (mais seguro)
    const form = document.createElement('form');
    form.method = 'GET';
    form.action = url;
    form.target = '_blank';
    
    // Adicionar token como header (navegador adiciona automaticamente)
    // Alternativamente, pode abrir URL diretamente se API aceitar token na query
    window.open(url, '_blank');
  },

  /**
   * Buscar HTML do contrato para exibir em iframe
   * @param id ID do contrato
   * @returns HTML completo do contrato
   */
  buscarHtmlContrato: async (id: string): Promise<string> => {
    const response = await api.get(`/Contracts/${id}/visualizar`, {
      responseType: 'text', // Importante: retorna string HTML ao invés de JSON
    });
    return response.data;
  },
};
```

---

### 2. Componente de Visualização de Contrato

**Arquivo: `app/contratos/[id]/page.tsx`**

```typescript
'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { contractsService } from '@/services/api/contracts';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Download, ExternalLink } from 'lucide-react';
import { toast } from 'sonner';

export default function VisualizarContratoPage() {
  const params = useParams();
  const contratoId = params.id as string;
  const [htmlContrato, setHtmlContrato] = useState<string>('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    carregarContrato();
  }, [contratoId]);

  const carregarContrato = async () => {
    try {
      setLoading(true);
      const html = await contractsService.buscarHtmlContrato(contratoId);
      setHtmlContrato(html);
    } catch (error: any) {
      console.error('Erro ao carregar contrato:', error);
      toast.error('Erro ao carregar contrato');
    } finally {
      setLoading(false);
    }
  };

  const abrirEmNovaAba = () => {
    const url = contractsService.obterUrlVisualizacao(contratoId);
    window.open(url, '_blank');
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen">
      {/* Header */}
      <div className="border-b bg-background p-4">
        <div className="container mx-auto flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => window.history.back()}
            >
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
            <h1 className="text-xl font-bold">Visualização do Contrato</h1>
          </div>

          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={abrirEmNovaAba}
            >
              <ExternalLink className="w-4 h-4 mr-2" />
              Abrir em Nova Aba
            </Button>
            <Button
              variant="default"
              size="sm"
              onClick={() => toast.info('Funcionalidade de download em desenvolvimento')}
            >
              <Download className="w-4 h-4 mr-2" />
              Baixar PDF
            </Button>
          </div>
        </div>
      </div>

      {/* Iframe com HTML do contrato */}
      <div className="flex-1 overflow-hidden">
        <iframe
          srcDoc={htmlContrato}
          className="w-full h-full border-0"
          title="Visualização do Contrato"
          sandbox="allow-same-origin"
        />
      </div>
    </div>
  );
}
```

---

### 3. Atualizar Página de Listagem

**Arquivo: `app/contratos/page.tsx`**

Modificar o botão "Visualizar":

```typescript
// Trocar de:
<Button
  variant="outline"
  size="sm"
  onClick={() => window.open(`/contratos/${contrato.id}`, '_blank')}
>
  <Eye className="w-4 h-4 mr-2" />
  Visualizar
</Button>

// Para:
<Button
  variant="outline"
  size="sm"
  onClick={() => {
    // Opção 1: Abrir rota interna (com iframe)
    window.open(`/contratos/${contrato.id}`, '_blank');
    
    // Opção 2: Abrir diretamente o HTML da API
    // const url = contractsService.obterUrlVisualizacao(contrato.id);
    // window.open(url, '_blank');
  }}
>
  <Eye className="w-4 h-4 mr-2" />
  Visualizar
</Button>
```

---

### 4. Modal de Visualização Rápida (Opcional)

**Arquivo: `app/contratos/components/ModalVisualizacaoContrato.tsx`**

```typescript
'use client';

import { useEffect, useState } from 'react';
import { contractsService } from '@/services/api/contracts';
import { Button } from '@/components/ui/button';
import { X, ExternalLink } from 'lucide-react';
import { toast } from 'sonner';

interface ModalVisualizacaoContratoProps {
  contratoId: string;
  onClose: () => void;
}

export function ModalVisualizacaoContrato({ contratoId, onClose }: ModalVisualizacaoContratoProps) {
  const [htmlContrato, setHtmlContrato] = useState<string>('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    carregarContrato();
  }, [contratoId]);

  const carregarContrato = async () => {
    try {
      setLoading(true);
      const html = await contractsService.buscarHtmlContrato(contratoId);
      setHtmlContrato(html);
    } catch (error: any) {
      console.error('Erro ao carregar contrato:', error);
      toast.error('Erro ao carregar contrato');
      onClose();
    } finally {
      setLoading(false);
    }
  };

  const abrirEmNovaAba = () => {
    const url = contractsService.obterUrlVisualizacao(contratoId);
    window.open(url, '_blank');
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-background rounded-lg w-full max-w-6xl h-[90vh] flex flex-col shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
          <h2 className="text-lg font-bold">Preview do Contrato</h2>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={abrirEmNovaAba}
            >
              <ExternalLink className="w-4 h-4 mr-2" />
              Abrir em Nova Aba
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
            >
              <X className="w-4 h-4" />
            </Button>
          </div>
        </div>

        {/* Conteúdo */}
        <div className="flex-1 overflow-hidden p-4">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
            </div>
          ) : (
            <iframe
              srcDoc={htmlContrato}
              className="w-full h-full border rounded"
              title="Preview do Contrato"
              sandbox="allow-same-origin"
            />
          )}
        </div>
      </div>
    </div>
  );
}
```

**Uso no componente de listagem:**

```typescript
const [contratoParaVisualizar, setContratoParaVisualizar] = useState<string | null>(null);

// No botão:
<Button
  variant="outline"
  size="sm"
  onClick={() => setContratoParaVisualizar(contrato.id)}
>
  <Eye className="w-4 h-4 mr-2" />
  Visualizar
</Button>

// No final do componente:
{contratoParaVisualizar && (
  <ModalVisualizacaoContrato
    contratoId={contratoParaVisualizar}
    onClose={() => setContratoParaVisualizar(null)}
  />
)}
```

---

## 🎨 Estilos do Contrato

O HTML retornado pela API já vem estilizado com CSS interno. Você não precisa adicionar estilos extras, mas pode customizar se necessário:

```css
/* Estilos já incluídos no HTML da API: */
- Fonte: Arial, sans-serif
- Margens e padding profissionais
- Cabeçalhos formatados
- Listas numeradas e com marcadores
- Assinaturas com espaço para nome
- Layout responsivo
```

---

## 🧪 Testes

### Casos de Teste

1. **Visualizar contrato Draft recém-criado**
   - Criar contrato
   - Abrir visualização imediatamente
   - ✅ HTML deve carregar com todos os dados preenchidos

2. **Visualizar contrato Active**
   - Contrato assinado
   - Abrir visualização
   - ✅ Deve mostrar documento final completo

3. **Contrato sem preview**
   - Contrato criado antes da implementação
   - Abrir visualização
   - ✅ Deve mostrar mensagem "Preview não disponível"

4. **Abrir em nova aba**
   - Clicar botão "Abrir em Nova Aba"
   - ✅ Deve abrir HTML diretamente no navegador

5. **Modal de visualização rápida**
   - Abrir modal de preview
   - Fechar modal
   - ✅ Não deve travar página

---

## ⚠️ Notas Importantes

### Segurança
- Endpoint valida que usuário pertence à empresa (cliente ou fornecedor)
- Token JWT obrigatório no header
- Usuários de outras empresas recebem 403 Forbidden

### Performance
- HTML é retornado como texto puro (não JSON)
- Use `responseType: 'text'` no axios
- Cache no frontend pode melhorar UX

### Compatibilidade
- Funciona em todos os navegadores modernos
- Iframe com `sandbox="allow-same-origin"` por segurança
- Mobile-friendly (HTML da API é responsivo)

---

## 🚀 Deploy

### Variáveis de Ambiente

Certifique-se de ter configurado:

```env
# .env.local
NEXT_PUBLIC_API_URL=https://aureapi.gabrielsanztech.com.br/api
```

### Ordem de Implementação

1. ✅ Atualizar `contracts.ts` service
2. ✅ Criar página `/contratos/[id]/page.tsx`
3. ✅ Atualizar botão na listagem
4. ⚙️ (Opcional) Implementar modal de preview rápido
5. 🧪 Testar todos os cenários

---

## 📚 Exemplos de Uso

### Exemplo 1: Visualização Simples
```typescript
// Abrir contrato em nova aba
const visualizarContrato = (contratoId: string) => {
  const url = contractsService.obterUrlVisualizacao(contratoId);
  window.open(url, '_blank');
};
```

### Exemplo 2: Preview em Modal
```typescript
// Carregar HTML e exibir em modal
const [html, setHtml] = useState('');

const carregarPreview = async (contratoId: string) => {
  const htmlContrato = await contractsService.buscarHtmlContrato(contratoId);
  setHtml(htmlContrato);
  setModalAberto(true);
};
```

### Exemplo 3: Download (Futuro)
```typescript
// Converter HTML para PDF no frontend
import html2pdf from 'html2pdf.js';

const baixarPdf = async (contratoId: string) => {
  const html = await contractsService.buscarHtmlContrato(contratoId);
  const opt = {
    margin: 1,
    filename: `contrato-${contratoId}.pdf`,
    image: { type: 'jpeg', quality: 0.98 },
    html2canvas: { scale: 2 },
    jsPDF: { unit: 'in', format: 'a4', orientation: 'portrait' }
  };
  html2pdf().set(opt).from(html).save();
};
```

---

## ✅ Checklist de Implementação

### Obrigatório
- [ ] Adicionar método `obterUrlVisualizacao` ao service
- [ ] Adicionar método `buscarHtmlContrato` ao service
- [ ] Criar página `app/contratos/[id]/page.tsx`
- [ ] Atualizar botão "Visualizar" na listagem
- [ ] Testar visualização de contratos Draft
- [ ] Testar visualização de contratos Active
- [ ] Testar mensagem de erro (preview indisponível)

### Opcional
- [ ] Implementar modal de visualização rápida
- [ ] Adicionar botão de download PDF
- [ ] Adicionar opção de imprimir
- [ ] Cache de HTML no localStorage
- [ ] Loading skeleton personalizado

---

**Data de Criação**: 04/12/2025  
**Última Atualização**: 04/12/2025  
**Status**: ✅ Backend Deployado - Frontend Pendente
**Endpoint Produção**: `https://aureapi.gabrielsanztech.com.br/api/Contracts/{id}/visualizar`
