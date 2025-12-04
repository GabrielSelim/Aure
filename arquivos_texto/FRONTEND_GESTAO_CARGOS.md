# 🎯 Gestão de Cargos - Instruções Frontend

## 📋 Resumo
O Proprietário pode definir cargos personalizados para **TODOS os usuários** da empresa (Financeiro, Jurídico, FuncionarioCLT, FuncionarioPJ). O cargo representa a função/profissão do usuário.

---

## 🔐 Permissões

| Role | Pode Alterar Cargo? | Pode Alterar Cargo de Quem? |
|------|-------------------|----------------------------|
| **DonoEmpresaPai** | ✅ Sim | Financeiro, Jurídico, FuncionarioCLT, FuncionarioPJ |
| **Juridico** | ✅ Sim | Financeiro, Jurídico (outros), FuncionarioCLT, FuncionarioPJ |
| **Financeiro** | ❌ Não | Ninguém |
| **FuncionarioCLT** | ❌ Não | Ninguém |
| **FuncionarioPJ** | ❌ Não | Ninguém |

**Regra Crítica:** Ninguém pode alterar o cargo do **Proprietário**.

---

## 🔌 Endpoint da API

### **Atualizar Cargo de Usuário**

```
PUT /api/Users/{userId}/cargo
```

**Headers:**
```json
{
  "Authorization": "Bearer {token}",
  "Content-Type": "application/json"
}
```

**Body:**
```json
{
  "cargo": "Advogado Contratual"
}
```

**Response 200 OK:**
```json
{
  "id": "uuid-do-usuario",
  "name": "Maria Silva",
  "email": "maria@empresa.com",
  "role": "Juridico",
  "cargo": "Advogado Contratual",
  "companyId": "uuid-da-empresa",
  "isActive": true,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

**Erros:**
- `400` - Cargo vazio ou inválido
- `401` - Usuário não autenticado ou sem permissão
- `404` - Usuário não encontrado

---

## 💡 Sugestões de Cargos por Role

### **Financeiro (Role: Financeiro)**
```typescript
const cargosSugeridosFinanceiro = [
  "Gerente Financeiro",
  "Analista Contábil",
  "Controller",
  "CFO",
  "Analista de Custos",
  "Contador",
  "Assistente Financeiro",
  "Coordenador Financeiro"
];
```

### **Jurídico (Role: Juridico)**
```typescript
const cargosSugeridosJuridico = [
  "Advogado Contratual",
  "Advogado Corporativo",
  "Consultor Jurídico",
  "Gerente Jurídico",
  "Advogado Trabalhista",
  "Compliance Officer",
  "Assessor Jurídico",
  "Coordenador Jurídico"
];
```

### **Funcionário CLT (Role: FuncionarioCLT)**
```typescript
const cargosSugeridosCLT = [
  "Analista de TI",
  "Gerente de Vendas",
  "Coordenador de Marketing",
  "Assistente Administrativo",
  "Analista de RH",
  "Supervisor de Produção",
  "Engenheiro de Software",
  "Designer Gráfico"
];
```

### **Funcionário PJ (Role: FuncionarioPJ)**
```typescript
const cargosSugeridosPJ = [
  "Desenvolvedor Full Stack",
  "Consultor de Negócios",
  "Designer UX/UI",
  "Arquiteto de Software",
  "Analista de Dados",
  "Gerente de Projetos",
  "Especialista em Marketing Digital",
  "Consultor Financeiro"
];
```

---

## 🎨 Implementação no Frontend

### **1. Interface/Type TypeScript**

```typescript
interface UpdateCargoRequest {
  cargo: string;
}

interface UpdateCargoResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  cargo: string;
  companyId: string;
  isActive: boolean;
  createdAt: string;
}
```

### **2. Função para Atualizar Cargo**

```typescript
async function atualizarCargoUsuario(
  userId: string, 
  cargo: string
): Promise<UpdateCargoResponse> {
  const response = await api.put<UpdateCargoResponse>(
    `/api/Users/${userId}/cargo`,
    { cargo }
  );
  return response.data;
}
```

### **3. Componente de Edição de Cargo**

```tsx
import { useState } from 'react';

interface EditarCargoProps {
  usuario: {
    id: string;
    name: string;
    role: string;
    cargo?: string;
  };
  onSave: () => void;
}

export function EditarCargo({ usuario, onSave }: EditarCargoProps) {
  const [cargo, setCargo] = useState(usuario.cargo || '');
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');

  const sugestoesCargo = getSugestoesPorRole(usuario.role);

  async function handleSalvar() {
    if (!cargo.trim()) {
      setErro('Cargo não pode ser vazio');
      return;
    }

    if (cargo.length > 100) {
      setErro('Cargo deve ter no máximo 100 caracteres');
      return;
    }

    setSalvando(true);
    setErro('');

    try {
      await atualizarCargoUsuario(usuario.id, cargo);
      onSave();
    } catch (error: any) {
      setErro(error.response?.data?.message || 'Erro ao atualizar cargo');
    } finally {
      setSalvando(false);
    }
  }

  return (
    <div>
      <label htmlFor="cargo">Cargo/Profissão</label>
      
      <input
        id="cargo"
        type="text"
        value={cargo}
        onChange={(e) => setCargo(e.target.value)}
        maxLength={100}
        placeholder="Ex: Advogado Contratual"
      />

      {/* Sugestões */}
      <div className="sugestoes">
        <p>Sugestões:</p>
        {sugestoesCargo.map((sugestao) => (
          <button
            key={sugestao}
            type="button"
            onClick={() => setCargo(sugestao)}
          >
            {sugestao}
          </button>
        ))}
      </div>

      {erro && <p className="erro">{erro}</p>}

      <button onClick={handleSalvar} disabled={salvando}>
        {salvando ? 'Salvando...' : 'Salvar Cargo'}
      </button>
    </div>
  );
}
```

### **4. Função Auxiliar - Sugestões por Role**

```typescript
function getSugestoesPorRole(role: string): string[] {
  const sugestoes: Record<string, string[]> = {
    Financeiro: [
      "Gerente Financeiro",
      "Analista Contábil",
      "Controller",
      "CFO",
      "Contador"
    ],
    Juridico: [
      "Advogado Contratual",
      "Advogado Corporativo",
      "Consultor Jurídico",
      "Gerente Jurídico",
      "Compliance Officer"
    ],
    FuncionarioCLT: [
      "Analista de TI",
      "Gerente de Vendas",
      "Coordenador",
      "Assistente Administrativo",
      "Supervisor"
    ],
    FuncionarioPJ: [
      "Desenvolvedor Full Stack",
      "Consultor",
      "Designer UX/UI",
      "Arquiteto de Software",
      "Gerente de Projetos"
    ]
  };

  return sugestoes[role] || [];
}
```

### **5. Validação no Frontend**

```typescript
function validarCargo(cargo: string): string | null {
  if (!cargo.trim()) {
    return 'Cargo não pode ser vazio';
  }

  if (cargo.length > 100) {
    return 'Cargo deve ter no máximo 100 caracteres';
  }

  return null; // Válido
}
```

---

## 🚀 Fluxo de Uso

### **Cenário 1: Proprietário Define Cargo do Jurídico**

1. Proprietário acessa lista de usuários
2. Clica em "Editar" no usuário Jurídico
3. Vê sugestões: "Advogado Contratual", "Advogado Corporativo", etc.
4. Seleciona "Advogado Contratual" ou digita cargo customizado
5. Clica em "Salvar"
6. Sistema valida e atualiza via `PUT /api/Users/{id}/cargo`

### **Cenário 2: Proprietário Define Cargo do Financeiro**

1. Proprietário acessa lista de usuários
2. Clica em "Editar" no usuário Financeiro
3. Vê sugestões: "Gerente Financeiro", "Controller", "CFO", etc.
4. Seleciona "Controller" ou digita cargo customizado
5. Clica em "Salvar"
6. Sistema valida e atualiza

### **Cenário 3: Funcionário PJ Precisa de Cargo para Contrato**

1. Proprietário tenta gerar contrato PJ
2. Sistema retorna erro: "Campos faltando: Profissão"
3. Proprietário acessa perfil do funcionário PJ
4. Define cargo: "Desenvolvedor Full Stack"
5. Salva
6. Tenta gerar contrato novamente
7. ✅ Contrato gerado com sucesso

---

## 📍 Onde Implementar no Frontend

### **Tela 1: Lista de Funcionários**
```
/funcionarios
```

**Adicionar coluna "Cargo":**
```tsx
<table>
  <thead>
    <tr>
      <th>Nome</th>
      <th>Email</th>
      <th>Role</th>
      <th>Cargo</th> {/* NOVA COLUNA */}
      <th>Ações</th>
    </tr>
  </thead>
  <tbody>
    {funcionarios.map(func => (
      <tr key={func.id}>
        <td>{func.name}</td>
        <td>{func.email}</td>
        <td>{func.role}</td>
        <td>{func.cargo || '(Não definido)'}</td>
        <td>
          <button onClick={() => editarCargo(func)}>
            Editar Cargo
          </button>
        </td>
      </tr>
    ))}
  </tbody>
</table>
```

### **Tela 2: Perfil de Usuário**
```
/usuarios/{id}/perfil
```

**Adicionar campo "Cargo":**
```tsx
<div className="perfil-usuario">
  <h2>Dados do Usuário</h2>
  
  <div className="campo-somente-leitura">
    <label>Nome:</label>
    <span>{usuario.name}</span>
  </div>

  <div className="campo-somente-leitura">
    <label>Email:</label>
    <span>{usuario.email}</span>
  </div>

  <div className="campo-somente-leitura">
    <label>Tipo:</label>
    <span>{usuario.role}</span>
  </div>

  {/* NOVO CAMPO EDITÁVEL (apenas para Proprietário/Jurídico) */}
  {podeEditarCargo && (
    <div className="campo-editavel">
      <label>Cargo/Profissão:</label>
      <EditarCargo usuario={usuario} onSave={recarregarDados} />
    </div>
  )}

  {!podeEditarCargo && (
    <div className="campo-somente-leitura">
      <label>Cargo:</label>
      <span>{usuario.cargo || '(Não definido)'}</span>
    </div>
  )}
</div>
```

### **Tela 3: Modal de Edição Rápida**
```tsx
<Modal isOpen={modalAberto} onClose={() => setModalAberto(false)}>
  <h2>Editar Cargo - {usuario.name}</h2>
  <EditarCargo usuario={usuario} onSave={handleSalvar} />
</Modal>
```

---

## 🔍 Verificação de Permissão

```typescript
function podeEditarCargo(userRole: string): boolean {
  return userRole === 'DonoEmpresaPai' || userRole === 'Juridico';
}

// No componente:
const podeEditar = podeEditarCargo(usuarioLogado.role);

return (
  <div>
    {podeEditar ? (
      <button onClick={abrirModalEdicao}>Editar Cargo</button>
    ) : (
      <span>{usuario.cargo || '(Não definido)'}</span>
    )}
  </div>
);
```

---

## ⚠️ Validações Frontend

```typescript
// Validar antes de enviar
function validarAntesDeSalvar(cargo: string): boolean {
  if (!cargo.trim()) {
    alert('Cargo não pode ser vazio');
    return false;
  }

  if (cargo.length > 100) {
    alert('Cargo deve ter no máximo 100 caracteres');
    return false;
  }

  return true;
}

// No submit:
async function handleSubmit(e: FormEvent) {
  e.preventDefault();
  
  if (!validarAntesDeSalvar(cargo)) {
    return;
  }

  // Enviar para API...
}
```

---

## 📊 Exemplo Completo - Tela de Gestão

```tsx
'use client';
import { useState, useEffect } from 'react';

interface Usuario {
  id: string;
  name: string;
  email: string;
  role: string;
  cargo?: string;
}

export default function GestaoUsuarios() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [usuarioEditando, setUsuarioEditando] = useState<Usuario | null>(null);
  const [cargoEditando, setCargoEditando] = useState('');

  useEffect(() => {
    carregarUsuarios();
  }, []);

  async function carregarUsuarios() {
    const response = await api.get('/api/Users/funcionarios?pageSize=100');
    setUsuarios(response.data.items);
  }

  function abrirEdicao(usuario: Usuario) {
    setUsuarioEditando(usuario);
    setCargoEditando(usuario.cargo || '');
  }

  async function salvarCargo() {
    if (!usuarioEditando) return;

    if (!cargoEditando.trim()) {
      alert('Cargo não pode ser vazio');
      return;
    }

    try {
      await api.put(`/api/Users/${usuarioEditando.id}/cargo`, {
        cargo: cargoEditando
      });
      
      alert('Cargo atualizado com sucesso!');
      setUsuarioEditando(null);
      carregarUsuarios();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Erro ao atualizar cargo');
    }
  }

  return (
    <div>
      <h1>Gestão de Usuários</h1>

      <table>
        <thead>
          <tr>
            <th>Nome</th>
            <th>Email</th>
            <th>Tipo</th>
            <th>Cargo</th>
            <th>Ações</th>
          </tr>
        </thead>
        <tbody>
          {usuarios.map(usuario => (
            <tr key={usuario.id}>
              <td>{usuario.name}</td>
              <td>{usuario.email}</td>
              <td>{usuario.role}</td>
              <td>{usuario.cargo || '(Não definido)'}</td>
              <td>
                <button onClick={() => abrirEdicao(usuario)}>
                  Editar Cargo
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Modal de Edição */}
      {usuarioEditando && (
        <div className="modal">
          <h2>Editar Cargo - {usuarioEditando.name}</h2>
          
          <label>Cargo/Profissão:</label>
          <input
            type="text"
            value={cargoEditando}
            onChange={(e) => setCargoEditando(e.target.value)}
            maxLength={100}
          />

          <div className="sugestoes">
            <p>Sugestões:</p>
            {getSugestoesPorRole(usuarioEditando.role).map(sugestao => (
              <button
                key={sugestao}
                onClick={() => setCargoEditando(sugestao)}
              >
                {sugestao}
              </button>
            ))}
          </div>

          <button onClick={salvarCargo}>Salvar</button>
          <button onClick={() => setUsuarioEditando(null)}>Cancelar</button>
        </div>
      )}
    </div>
  );
}
```

---

## ✅ Checklist de Implementação

- [ ] Adicionar coluna "Cargo" na tabela de usuários
- [ ] Criar botão "Editar Cargo" (visível apenas para Proprietário/Jurídico)
- [ ] Implementar modal/formulário de edição de cargo
- [ ] Adicionar sugestões de cargo por role
- [ ] Implementar validação frontend (não vazio, max 100 chars)
- [ ] Integrar com endpoint `PUT /api/Users/{id}/cargo`
- [ ] Exibir mensagem de sucesso/erro
- [ ] Recarregar lista após salvar
- [ ] Adicionar campo cargo no perfil do usuário
- [ ] Testar com Proprietário alterando cargo de Financeiro
- [ ] Testar com Proprietário alterando cargo de Jurídico
- [ ] Testar com Proprietário alterando cargo de FuncionarioPJ
- [ ] Validar que Financeiro/CLT/PJ não conseguem editar cargos

---

**Resumo:** Backend já está pronto. Proprietário e Jurídico podem alterar cargos de qualquer usuário (exceto Proprietário) usando o endpoint `PUT /api/Users/{id}/cargo`. Implementar interface no frontend conforme exemplos acima.
