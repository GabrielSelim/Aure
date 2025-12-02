# 🔒 Validações de Contrato - Guia Frontend

## 📋 Objetivo
Este documento descreve todas as validações implementadas no backend para geração e preview de contratos PJ. O frontend deve validar os mesmos campos antes de enviar a requisição para evitar erros e melhorar a experiência do usuário.

---

## 🚨 Regras Gerais

### 1. Validação de Dados da Empresa Contratante
Todos os campos abaixo são **obrigatórios** antes de gerar ou visualizar um contrato:

| Campo | Validação | Mensagem de Erro |
|-------|-----------|------------------|
| Rua (endereço empresa) | Não pode ser vazio | "Campo Rua é obrigatório" |
| Número (endereço empresa) | Não pode ser vazio | "Campo Número é obrigatório" |
| Bairro (endereço empresa) | Não pode ser vazio | "Campo Bairro é obrigatório" |
| Cidade (endereço empresa) | Não pode ser vazio | "Campo Cidade é obrigatório" |
| Estado (endereço empresa) | Não pode ser vazio | "Campo Estado é obrigatório" |
| CEP (endereço empresa) | Não pode ser vazio | "Campo CEP é obrigatório" |

**Observação:** NIRE e Inscrição Estadual são campos **opcionais** e não bloqueiam a geração de contratos.

**Mensagem backend quando campos faltam:**
```
"Dados da empresa contratante estão incompletos. Campos faltando: [lista de campos]"
```

**Ação no Frontend:**
- Antes de permitir acesso à tela de criar contrato, verificar se todos os campos da empresa estão preenchidos
- Se faltarem campos, exibir modal/banner alertando o usuário para completar o cadastro da empresa
- Criar botão "Completar Cadastro da Empresa" que redireciona para `/api/UserProfile/empresa` (PUT)

---

### 2. Validação de Dados do Representante (Usuário Logado)
Todos os campos abaixo são **obrigatórios** antes de gerar ou visualizar um contrato:

| Campo | Validação | Mensagem de Erro |
|-------|-----------|------------------|
| CPF | Não pode ser vazio | "Campo CPF é obrigatório" |
| RG | Não pode ser vazio | "Campo RG é obrigatório" |
| Data de Nascimento | Não pode ser nulo | "Campo Data de Nascimento é obrigatório" |
| Nacionalidade | Não pode ser vazio | "Campo Nacionalidade é obrigatório" |
| Estado Civil | Não pode ser vazio | "Campo Estado Civil é obrigatório" |
| Rua (endereço residencial) | Não pode ser vazio | "Campo Rua (endereço residencial) é obrigatório" |
| Número (endereço residencial) | Não pode ser vazio | "Campo Número (endereço residencial) é obrigatório" |
| Bairro (endereço residencial) | Não pode ser vazio | "Campo Bairro (endereço residencial) é obrigatório" |
| Cidade (endereço residencial) | Não pode ser vazio | "Campo Cidade (endereço residencial) é obrigatório" |
| Estado (endereço residencial) | Não pode ser vazio | "Campo Estado (endereço residencial) é obrigatório" |
| CEP (endereço residencial) | Não pode ser vazio | "Campo CEP (endereço residencial) é obrigatório" |

**Mensagem backend quando campos faltam:**
```
"Dados do representante estão incompletos. Campos faltando: [lista de campos]"
```

**Ação no Frontend:**
- Antes de permitir acesso à tela de criar contrato, verificar se todos os campos do perfil do representante estão preenchidos
- Se faltarem campos, exibir modal/banner alertando o usuário para completar seu perfil
- Criar botão "Completar Meu Perfil" que redireciona para `/api/UserProfile/perfil-completo` (PUT)

---

### 3. Validação de Dados do Contratado - Modo Funcionário PJ Cadastrado

Quando `funcionarioPJId` é informado, todos os campos abaixo do funcionário PJ são **obrigatórios**:

| Campo | Validação | Mensagem de Erro |
|-------|-----------|------------------|
| CPF | Não pode ser vazio | "Campo CPF é obrigatório" |
| RG | Não pode ser vazio | "Campo RG é obrigatório" |
| Data de Nascimento | Não pode ser nulo | "Campo Data de Nascimento é obrigatório" |
| Nacionalidade | Não pode ser vazio | "Campo Nacionalidade é obrigatório" |
| Estado Civil | Não pode ser vazio | "Campo Estado Civil é obrigatório" |
| Profissão | Não pode ser vazio | "Campo Profissão é obrigatório" |
| Rua (endereço residencial) | Não pode ser vazio | "Campo Rua (endereço residencial) é obrigatório" |
| Número (endereço residencial) | Não pode ser vazio | "Campo Número (endereço residencial) é obrigatório" |
| Bairro (endereço residencial) | Não pode ser vazio | "Campo Bairro (endereço residencial) é obrigatório" |
| Cidade (endereço residencial) | Não pode ser vazio | "Campo Cidade (endereço residencial) é obrigatório" |
| Estado (endereço residencial) | Não pode ser vazio | "Campo Estado (endereço residencial) é obrigatório" |
| CEP (endereço residencial) | Não pode ser vazio | "Campo CEP (endereço residencial) é obrigatório" |

**Mensagem backend quando campos faltam:**
```
"Dados do contratado (funcionário PJ) estão incompletos. Campos faltando: [lista de campos]"
```

**Ação no Frontend:**
- Ao selecionar um funcionário PJ na lista, fazer uma requisição GET para buscar os dados dele
- Validar se todos os campos obrigatórios estão preenchidos
- Se faltarem campos, exibir mensagem: "O funcionário [nome] não tem todos os dados cadastrados. Complete o perfil dele antes de gerar o contrato."
- Desabilitar botão de "Gerar Contrato" e "Visualizar Preview" até que todos os campos estejam completos

#### Validação Adicional: Dados da Empresa PJ

Se o funcionário PJ tiver uma empresa associada (CompanyId não nulo), validar também:

| Campo | Validação | Mensagem de Erro |
|-------|-----------|------------------|
| Rua (empresa PJ) | Não pode ser vazio | "Campo Rua (empresa PJ) é obrigatório" |
| Número (empresa PJ) | Não pode ser vazio | "Campo Número (empresa PJ) é obrigatório" |
| Bairro (empresa PJ) | Não pode ser vazio | "Campo Bairro (empresa PJ) é obrigatório" |
| Cidade (empresa PJ) | Não pode ser vazio | "Campo Cidade (empresa PJ) é obrigatório" |
| Estado (empresa PJ) | Não pode ser vazio | "Campo Estado (empresa PJ) é obrigatório" |

**Mensagem backend quando campos faltam:**
```
"Dados da empresa do contratado (PJ) estão incompletos. Campos faltando: [lista de campos]"
```

---

### 4. Validação de Dados do Contratado - Modo Manual

Quando `dadosContratadoManual` é informado, **TODOS** os campos abaixo são **obrigatórios**:

| Campo | Tipo | Validação | Mensagem de Erro |
|-------|------|-----------|------------------|
| nomeCompleto | string | Obrigatório, max 200 caracteres | "Campo Nome Completo é obrigatório" |
| razaoSocial | string | Obrigatório, max 200 caracteres | "Campo Razão Social é obrigatório" |
| cnpj | string | Obrigatório, 14 dígitos | "Campo CNPJ é obrigatório" |
| cpf | string | Obrigatório, 11 dígitos | "Campo CPF é obrigatório" |
| rg | string | **Obrigatório** (campo atualizado), max 20 caracteres | "Campo RG é obrigatório" |
| dataNascimento | DateTime | **Obrigatório** (campo atualizado) | "Campo Data de Nascimento é obrigatório" |
| nacionalidade | string | **Obrigatório** (campo novo), max 50 caracteres | "Campo Nacionalidade é obrigatório" |
| estadoCivil | string | **Obrigatório** (campo novo), max 50 caracteres | "Campo Estado Civil é obrigatório" |
| profissao | string | **Obrigatório** (campo atualizado), max 100 caracteres | "Campo Profissão é obrigatório" |
| email | string | Obrigatório, formato email válido | "Campo Email é obrigatório" |
| telefoneCelular | string | Obrigatório, 10-11 dígitos | "Campo Telefone Celular é obrigatório" |
| telefoneFixo | string | Opcional, 10 dígitos | - |
| rua | string | Obrigatório | "Campo Rua é obrigatório" |
| numero | string | Obrigatório | "Campo Número é obrigatório" |
| complemento | string | Opcional | - |
| bairro | string | Obrigatório | "Campo Bairro é obrigatório" |
| cidade | string | Obrigatório | "Campo Cidade é obrigatório" |
| estado | string | Obrigatório, 2 caracteres | "Campo Estado é obrigatório" |
| pais | string | Obrigatório | "Campo País é obrigatório" |
| cep | string | Obrigatório, 8 dígitos | "Campo CEP é obrigatório" |

**Mensagem backend quando campos faltam:**
```
"Dados do contratado estão incompletos. Campos faltando: [lista de campos]"
```

**Ação no Frontend:**
- Criar validação em tempo real (onChange) para cada campo obrigatório
- Marcar campos obrigatórios com asterisco vermelho (*)
- Exibir mensagem de erro abaixo do campo quando vazio e usuário sair do campo (onBlur)
- Desabilitar botão "Gerar Contrato" e "Visualizar Preview" até que todos os campos obrigatórios estejam preenchidos
- Exibir contador de campos faltantes: "X campos obrigatórios faltam ser preenchidos"

---

## 🔍 Fluxo de Validação no Frontend

### Passo 1: Ao Carregar Tela de Criar Contrato

```typescript
async function validarDadosIniciais() {
  // 1. Buscar dados da empresa logada
  const empresaResponse = await fetch('/api/UserProfile/empresa', {
    headers: { Authorization: `Bearer ${token}` }
  });
  const empresa = await empresaResponse.json();

  // 2. Buscar perfil do usuário logado
  const perfilResponse = await fetch('/api/UserProfile/perfil-completo', {
    headers: { Authorization: `Bearer ${token}` }
  });
  const perfil = await perfilResponse.json();

  // 3. Validar campos da empresa
  const camposEmpresaFaltando = [];
  if (!empresa.rua) camposEmpresaFaltando.push('Rua');
  if (!empresa.numero) camposEmpresaFaltando.push('Número');
  if (!empresa.bairro) camposEmpresaFaltando.push('Bairro');
  if (!empresa.cidade) camposEmpresaFaltando.push('Cidade');
  if (!empresa.estado) camposEmpresaFaltando.push('Estado');
  if (!empresa.cep) camposEmpresaFaltando.push('CEP');

  // 4. Validar campos do representante
  const camposRepresentanteFaltando = [];
  if (!perfil.cpf) camposRepresentanteFaltando.push('CPF');
  if (!perfil.rg) camposRepresentanteFaltando.push('RG');
  if (!perfil.dataNascimento) camposRepresentanteFaltando.push('Data de Nascimento');
  if (!perfil.nacionalidade) camposRepresentanteFaltando.push('Nacionalidade');
  if (!perfil.estadoCivil) camposRepresentanteFaltando.push('Estado Civil');
  if (!perfil.enderecoRua) camposRepresentanteFaltando.push('Rua (endereço residencial)');
  if (!perfil.enderecoNumero) camposRepresentanteFaltando.push('Número (endereço residencial)');
  if (!perfil.enderecoBairro) camposRepresentanteFaltando.push('Bairro (endereço residencial)');
  if (!perfil.enderecoCidade) camposRepresentanteFaltando.push('Cidade (endereço residencial)');
  if (!perfil.enderecoEstado) camposRepresentanteFaltando.push('Estado (endereço residencial)');
  if (!perfil.enderecoCep) camposRepresentanteFaltando.push('CEP (endereço residencial)');

  // 5. Exibir alertas se houver campos faltando
  if (camposEmpresaFaltando.length > 0) {
    mostrarAlerta({
      tipo: 'warning',
      titulo: 'Dados da Empresa Incompletos',
      mensagem: `Complete os seguintes campos da empresa antes de gerar contratos: ${camposEmpresaFaltando.join(', ')}`,
      botaoAcao: 'Completar Cadastro da Empresa',
      urlAcao: '/empresa/editar'
    });
    return false;
  }

  if (camposRepresentanteFaltando.length > 0) {
    mostrarAlerta({
      tipo: 'warning',
      titulo: 'Seu Perfil Está Incompleto',
      mensagem: `Complete os seguintes campos do seu perfil: ${camposRepresentanteFaltando.join(', ')}`,
      botaoAcao: 'Completar Meu Perfil',
      urlAcao: '/perfil/editar'
    });
    return false;
  }

  return true;
}
```

### Passo 2: Ao Selecionar Funcionário PJ

```typescript
async function validarFuncionarioPJ(funcionarioId: string) {
  // 1. Buscar dados do funcionário
  const response = await fetch(`/api/Users/${funcionarioId}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  const funcionario = await response.json();

  // 2. Validar campos obrigatórios
  const camposFaltando = [];
  if (!funcionario.cpf) camposFaltando.push('CPF');
  if (!funcionario.rg) camposFaltando.push('RG');
  if (!funcionario.dataNascimento) camposFaltando.push('Data de Nascimento');
  if (!funcionario.nacionalidade) camposFaltando.push('Nacionalidade');
  if (!funcionario.estadoCivil) camposFaltando.push('Estado Civil');
  if (!funcionario.profissao) camposFaltando.push('Profissão');
  if (!funcionario.enderecoRua) camposFaltando.push('Rua');
  if (!funcionario.enderecoNumero) camposFaltando.push('Número');
  if (!funcionario.enderecoBairro) camposFaltando.push('Bairro');
  if (!funcionario.enderecoCidade) camposFaltando.push('Cidade');
  if (!funcionario.enderecoEstado) camposFaltando.push('Estado');
  if (!funcionario.enderecoCep) camposFaltando.push('CEP');

  // 3. Se tiver empresa, validar dados da empresa PJ
  if (funcionario.companyId) {
    const empresaResponse = await fetch(`/api/Companies/${funcionario.companyId}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const empresaPJ = await empresaResponse.json();

    if (!empresaPJ.rua) camposFaltando.push('Rua (empresa PJ)');
    if (!empresaPJ.numero) camposFaltando.push('Número (empresa PJ)');
    if (!empresaPJ.bairro) camposFaltando.push('Bairro (empresa PJ)');
    if (!empresaPJ.cidade) camposFaltando.push('Cidade (empresa PJ)');
    if (!empresaPJ.estado) camposFaltando.push('Estado (empresa PJ)');
  }

  // 4. Retornar resultado
  if (camposFaltando.length > 0) {
    return {
      valido: false,
      camposFaltando,
      mensagem: `O funcionário ${funcionario.name} não tem todos os dados cadastrados. Campos faltando: ${camposFaltando.join(', ')}`
    };
  }

  return { valido: true };
}
```

### Passo 3: Validação de Dados Manuais

```typescript
function validarDadosManual(dadosManual: DadosContratadoManual): ValidationResult {
  const camposFaltando = [];

  if (!dadosManual.nomeCompleto?.trim()) camposFaltando.push('Nome Completo');
  if (!dadosManual.razaoSocial?.trim()) camposFaltando.push('Razão Social');
  if (!dadosManual.cnpj?.trim() || dadosManual.cnpj.length !== 14) camposFaltando.push('CNPJ (14 dígitos)');
  if (!dadosManual.cpf?.trim() || dadosManual.cpf.length !== 11) camposFaltando.push('CPF (11 dígitos)');
  if (!dadosManual.rg?.trim()) camposFaltando.push('RG');
  if (!dadosManual.dataNascimento) camposFaltando.push('Data de Nascimento');
  if (!dadosManual.nacionalidade?.trim()) camposFaltando.push('Nacionalidade');
  if (!dadosManual.estadoCivil?.trim()) camposFaltando.push('Estado Civil');
  if (!dadosManual.profissao?.trim()) camposFaltando.push('Profissão');
  if (!dadosManual.email?.trim() || !isValidEmail(dadosManual.email)) camposFaltando.push('Email válido');
  if (!dadosManual.telefoneCelular?.trim() || dadosManual.telefoneCelular.length < 10) camposFaltando.push('Telefone Celular (10-11 dígitos)');
  if (!dadosManual.rua?.trim()) camposFaltando.push('Rua');
  if (!dadosManual.numero?.trim()) camposFaltando.push('Número');
  if (!dadosManual.bairro?.trim()) camposFaltando.push('Bairro');
  if (!dadosManual.cidade?.trim()) camposFaltando.push('Cidade');
  if (!dadosManual.estado?.trim() || dadosManual.estado.length !== 2) camposFaltando.push('Estado (2 caracteres)');
  if (!dadosManual.pais?.trim()) camposFaltando.push('País');
  if (!dadosManual.cep?.trim() || dadosManual.cep.length !== 8) camposFaltando.push('CEP (8 dígitos)');

  return {
    valido: camposFaltando.length === 0,
    camposFaltando,
    mensagem: camposFaltando.length > 0 
      ? `Preencha os seguintes campos obrigatórios: ${camposFaltando.join(', ')}`
      : ''
  };
}
```

---

## 🎨 Componentes React Sugeridos

### 1. Alert de Dados Incompletos

```tsx
interface DadosIncompletosAlertProps {
  tipo: 'empresa' | 'representante' | 'funcionario' | 'manual';
  camposFaltando: string[];
  onCompletarCadastro: () => void;
}

export function DadosIncompletosAlert({ tipo, camposFaltando, onCompletarCadastro }: DadosIncompletosAlertProps) {
  const titulos = {
    empresa: 'Dados da Empresa Incompletos',
    representante: 'Seu Perfil Está Incompleto',
    funcionario: 'Dados do Funcionário Incompletos',
    manual: 'Dados do Contratado Incompletos'
  };

  const mensagens = {
    empresa: 'Complete os dados da empresa antes de gerar contratos.',
    representante: 'Complete seu perfil antes de gerar contratos.',
    funcionario: 'Complete os dados do funcionário antes de gerar o contrato.',
    manual: 'Preencha todos os campos obrigatórios do contratado.'
  };

  return (
    <div className="bg-yellow-50 border border-yellow-400 rounded-lg p-4 mb-4">
      <div className="flex items-start">
        <AlertTriangle className="h-5 w-5 text-yellow-600 mr-3 mt-0.5" />
        <div className="flex-1">
          <h3 className="text-sm font-semibold text-yellow-800">{titulos[tipo]}</h3>
          <p className="text-sm text-yellow-700 mt-1">{mensagens[tipo]}</p>
          
          <div className="mt-2 bg-white rounded p-3">
            <p className="text-xs font-semibold text-gray-700 mb-1">Campos faltando:</p>
            <ul className="list-disc list-inside text-xs text-gray-600 space-y-1">
              {camposFaltando.map((campo, index) => (
                <li key={index}>{campo}</li>
              ))}
            </ul>
          </div>

          {tipo !== 'manual' && (
            <button
              onClick={onCompletarCadastro}
              className="mt-3 bg-yellow-600 hover:bg-yellow-700 text-white text-sm font-medium py-2 px-4 rounded transition"
            >
              Completar Cadastro
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
```

### 2. Validação de Formulário Manual

```tsx
interface FormFieldProps {
  label: string;
  obrigatorio?: boolean;
  error?: string;
  children: React.ReactNode;
}

export function FormField({ label, obrigatorio = false, error, children }: FormFieldProps) {
  return (
    <div className="mb-4">
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label}
        {obrigatorio && <span className="text-red-500 ml-1">*</span>}
      </label>
      {children}
      {error && (
        <p className="text-xs text-red-600 mt-1 flex items-center">
          <AlertCircle className="h-3 w-3 mr-1" />
          {error}
        </p>
      )}
    </div>
  );
}
```

### 3. Contador de Campos Faltantes

```tsx
interface CamposFaltantesCounterProps {
  total: number;
  preenchidos: number;
}

export function CamposFaltantesCounter({ total, preenchidos }: CamposFaltantesCounterProps) {
  const faltantes = total - preenchidos;
  const porcentagem = (preenchidos / total) * 100;

  return (
    <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-medium text-blue-900">
          Progresso do Formulário
        </span>
        <span className="text-sm font-bold text-blue-900">
          {preenchidos}/{total}
        </span>
      </div>
      
      <div className="w-full bg-blue-200 rounded-full h-2 mb-2">
        <div
          className="bg-blue-600 h-2 rounded-full transition-all duration-300"
          style={{ width: `${porcentagem}%` }}
        />
      </div>
      
      {faltantes > 0 ? (
        <p className="text-xs text-blue-700">
          {faltantes} campo{faltantes > 1 ? 's' : ''} obrigatório{faltantes > 1 ? 's' : ''} faltando
        </p>
      ) : (
        <p className="text-xs text-green-700 flex items-center">
          <CheckCircle className="h-3 w-3 mr-1" />
          Todos os campos obrigatórios preenchidos
        </p>
      )}
    </div>
  );
}
```

---

## 📌 Checklist de Implementação Frontend

### Validações Iniciais (Tela de Criação de Contrato)
- [ ] Validar dados da empresa contratante ao carregar a tela
- [ ] Validar dados do representante ao carregar a tela
- [ ] Exibir alertas se houver campos faltando
- [ ] Implementar botões de redirecionamento para completar cadastros
- [ ] Bloquear acesso à criação de contrato se dados estiverem incompletos

### Modo Funcionário PJ Cadastrado
- [ ] Validar dados do funcionário ao selecioná-lo na lista
- [ ] Validar dados da empresa PJ (se existir)
- [ ] Exibir mensagem clara de campos faltantes
- [ ] Desabilitar botões de "Gerar Contrato" e "Preview" se dados incompletos
- [ ] Implementar indicador visual (badge/ícone) nos funcionários com dados incompletos

### Modo Dados Manuais
- [ ] Adicionar campos `nacionalidade` e `estadoCivil` ao formulário
- [ ] Tornar `rg`, `dataNascimento` e `profissao` obrigatórios
- [ ] Implementar validação em tempo real (onChange)
- [ ] Exibir mensagens de erro individuais por campo (onBlur)
- [ ] Implementar contador de campos faltantes
- [ ] Desabilitar botões se formulário inválido
- [ ] Aplicar máscaras de formatação (CPF, CNPJ, CEP, telefone)

### UX/UI
- [ ] Marcar campos obrigatórios com asterisco vermelho (*)
- [ ] Exibir tooltip explicativo em campos sensíveis
- [ ] Implementar feedback visual de validação (verde quando válido, vermelho quando inválido)
- [ ] Criar animações suaves para alertas e transições
- [ ] Implementar scroll automático para primeiro campo com erro

---

## 🔄 Fluxo Completo Recomendado

```
1. Usuário acessa tela "Criar Contrato"
   ↓
2. Sistema valida dados da empresa e representante
   ↓
3a. Se incompleto → Exibir alerta e botão "Completar Cadastro"
3b. Se completo → Permitir criar contrato
   ↓
4. Usuário escolhe modo: Funcionário PJ ou Manual
   ↓
5a. Se Funcionário PJ:
    - Buscar lista de funcionários PJ
    - Exibir badge de "completo" ou "incompleto" em cada um
    - Ao selecionar, validar dados
    - Se incompleto, exibir alerta e desabilitar botões
   ↓
5b. Se Manual:
    - Exibir formulário completo com todos os campos
    - Validar em tempo real
    - Exibir contador de campos faltantes
    - Habilitar botões apenas quando tudo estiver válido
   ↓
6. Usuário preenche configurações do contrato (valor, prazo, etc.)
   ↓
7. Botão "Visualizar Preview" fica habilitado
   ↓
8. Ao clicar "Gerar Contrato", enviar requisição
   ↓
9a. Se backend retornar erro de validação → Exibir mensagem clara
9b. Se sucesso → Redirecionar para visualização do contrato gerado
```

---

## 🚀 Endpoints de Suporte

### Buscar Dados para Validação

```typescript
// Dados da empresa logada
GET /api/UserProfile/empresa
Authorization: Bearer {token}

Response:
{
  "id": "uuid",
  "nome": "string",
  "cnpj": "string",
  "rua": "string",
  "numero": "string",
  "bairro": "string",
  "cidade": "string",
  "estado": "string",
  "cep": "string",
  "nire": "string",
  "inscricaoEstadual": "string"
}
```

```typescript
// Perfil completo do usuário logado
GET /api/UserProfile/perfil-completo
Authorization: Bearer {token}

Response:
{
  "id": "uuid",
  "name": "string",
  "email": "string",
  "cpf": "string",
  "rg": "string",
  "dataNascimento": "2025-01-01",
  "nacionalidade": "string",
  "estadoCivil": "string",
  "enderecoRua": "string",
  "enderecoNumero": "string",
  "enderecoBairro": "string",
  "enderecoCidade": "string",
  "enderecoEstado": "string",
  "enderecoCep": "string"
}
```

```typescript
// Dados de um funcionário PJ específico
GET /api/Users/{funcionarioId}
Authorization: Bearer {token}

Response: (mesmo formato do perfil completo)
```

### Atualizar Dados

```typescript
// Atualizar dados da empresa
PUT /api/UserProfile/empresa
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "nome": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string?",
  "rua": "string",
  "numero": "string",
  "complemento": "string?",
  "bairro": "string",
  "cidade": "string",
  "estado": "string",
  "pais": "string",
  "cep": "string"
}
```

```typescript
// Atualizar perfil completo do usuário
PUT /api/UserProfile/perfil-completo
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "name": "string",
  "cpf": "string",
  "rg": "string",
  "dataNascimento": "2025-01-01",
  "nacionalidade": "string",
  "estadoCivil": "string",
  "profissao": "string",
  "telefoneCelular": "string",
  "telefoneFixo": "string?",
  "enderecoRua": "string",
  "enderecoNumero": "string",
  "enderecoComplemento": "string?",
  "enderecoBairro": "string",
  "enderecoCidade": "string",
  "enderecoEstado": "string",
  "enderecoPais": "string",
  "enderecoCep": "string"
}
```

---

## ⚠️ Mensagens de Erro do Backend

O backend agora retorna mensagens detalhadas informando **exatamente quais campos estão faltando**:

### Exemplos de Respostas de Erro:

```json
{
  "message": "Dados da empresa contratante estão incompletos. Campos faltando: NIRE, Inscrição Estadual"
}
```

```json
{
  "message": "Dados do representante estão incompletos. Campos faltando: RG, Nacionalidade, Estado Civil"
}
```

```json
{
  "message": "Dados do contratado (funcionário PJ) estão incompletos. Campos faltando: RG, Profissão, CEP (endereço residencial)"
}
```

```json
{
  "message": "Dados da empresa do contratado (PJ) estão incompletos. Campos faltando: Número (empresa PJ), Bairro (empresa PJ)"
}
```

```json
{
  "message": "Dados do contratado estão incompletos. Campos faltando: RG, Data de Nascimento, Nacionalidade, Estado Civil, CEP"
}
```

**Use essas mensagens para feedback direto ao usuário no frontend!**

---

## 📚 Resumo dos Novos Campos

### Campos Adicionados ao DTO `DadosContratadoManualRequest`:

| Campo | Tipo | Obrigatório | Observação |
|-------|------|-------------|------------|
| `nacionalidade` | string? | Sim (tornou-se obrigatório) | Max 50 caracteres |
| `estadoCivil` | string? | Sim (tornou-se obrigatório) | Max 50 caracteres |

### Campos Tornados Obrigatórios:

- `rg` → Antes opcional, agora **obrigatório**
- `dataNascimento` → Antes opcional, agora **obrigatório**
- `profissao` → Antes opcional, agora **obrigatório**

---

## ✅ Validações Implementadas no Backend

- ✅ Validação completa de dados da empresa contratante
- ✅ Validação completa de dados do representante
- ✅ Validação completa de dados do contratado (modo PJ cadastrado)
- ✅ Validação completa de dados da empresa PJ (se existir)
- ✅ Validação completa de dados do contratado (modo manual)
- ✅ Limpeza automática de vírgulas duplicadas no HTML (`, ,` → `,`)
- ✅ Limpeza de espaços em branco duplicados
- ✅ Mensagens de erro detalhadas com lista de campos faltando

---

**Data de Criação:** 02/12/2024  
**Última Atualização:** 02/12/2024  
**Versão:** 1.0
