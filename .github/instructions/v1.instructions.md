````instructions
# 🎨 Frontend - Sistema de Perfil de Usuário e Configurações da Empresa (ATUALIZADO)

## 📋 Status das Implementações Backend

**Data de Atualização**: 30/10/2025
**Status do Backend**: ✅ **TOTALMENTE IMPLEMENTADO E FUNCIONAL**
**API Base URL**: `https://aureapi.gabrielsanztech.com.br`
**Swagger**: `https://aureapi.gabrielsanztech.com.br` (disponível na raiz)

---

## ✅ Melhorias Implementadas no Backend

### 1. **Recuperação de Senha** ✅
- Fluxo completo implementado com envio de email
- Token de recuperação expira em 2 horas
- Email HTML com design profissional
- Links apontam para: `https://aure.gabrielsanztech.com.br/recuperar-senha?token={token}`

**Endpoints:**
```typescript
POST /api/auth/solicitar-recuperacao-senha
Body: { email: string }
Response: { mensagem: string }

POST /api/auth/redefinir-senha
Body: {
  token: string;
  novaSenha: string;
  confirmacaoSenha: string;
}
Response: { sucesso: boolean; mensagem: string }
```

### 2. **CORS Configurado** ✅
- Ambiente de produção: `https://aure.gabrielsanztech.com.br`
- Desenvolvimento local: `http://localhost:3000`
- Credenciais permitidas
- Métodos e headers configurados

### 3. **Campos Completos no Perfil** ✅
Todos os campos implementados e validados:
- ✅ CPF (criptografado)
- ✅ RG (criptografado)
- ✅ Data de Nascimento
- ✅ Cargo (definido automaticamente como "Proprietário" para Dono)
- ✅ Telefone Celular e Fixo
- ✅ Endereço completo (8 campos)
- ✅ Termos de Uso e Política de Privacidade (versões separadas)

### 4. **Endpoint de Alterar Cargo** ✅
Apenas Dono pode alterar cargo de funcionários:

```typescript
PUT /api/Users/{employeeId}/cargo
Headers: { Authorization: "Bearer {token}" }
Body: { cargo: string }
Response: UserResponse
```

### 5. **Endpoints de Dados da Empresa** ✅
Buscar e atualizar informações da empresa do usuário:

```typescript
// Buscar dados da empresa (todos os usuários autenticados)
GET /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token}" }
Response: {
  id: string;
  nome: string;
  cnpj: string;
  cnpjFormatado: string;
  tipo: string;
  modeloNegocio: string;
  endereco: {
    rua: string;
    numero: string;
    complemento?: string;
    bairro: string;
    cidade: string;
    estado: string;
    pais: string;
    cep: string;
    enderecoCompleto: string;
  } | null;
  telefoneFixo?: string;
  telefoneCelular?: string;
}

// Atualizar dados da empresa (apenas DonoEmpresaPai)
PUT /api/UserProfile/empresa
Headers: { Authorization: "Bearer {token}" }
Body: {
  nome: string;
  telefoneCelular: string;
  telefoneFixo?: string;
  rua: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string; // 2 letras (SP, RJ, etc)
  pais: string;
  cep: string; // 8 dígitos
}
Response: CompanyInfoResponse
```

### 6. **Notificações Funcionais** ✅
Sistema de preferências de notificação implementado e testado:

```typescript
GET /api/UserProfile/notificacoes/preferencias
PUT /api/UserProfile/notificacoes/preferencias
Body: {
  emailPagamentos: boolean;
  emailContratos: boolean;
  emailFuncionarios: boolean;
  emailRelatorios: boolean;
  sistemaPagamentos: boolean;
  sistemaContratos: boolean;
  sistemaFuncionarios: boolean;
  sistemaRelatorios: boolean;
}
```

### 7. **Registro Completo do Dono** ✅
Todos os campos obrigatórios no cadastro inicial:

```typescript
POST /api/registration/admin-empresa
Body: {
  // Empresa
  companyName: string;
  companyCnpj: string; // 14 dígitos
  companyType: "Client" | "Provider" | "Both";
  businessModel: "MainCompany" | "Standard" | "ContractedPJ" | "Freelancer";
  
  // Usuário
  name: string;
  email: string;
  password: string;
  cpf: string; // 11 dígitos
  rg?: string;
  dataNascimento: Date;
  
  // Contatos
  telefoneCelular: string; // 10-11 dígitos
  telefoneFixo?: string; // 10 dígitos
  
  // Endereço
  rua: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string; // 2 letras
  pais: string;
  cep: string; // 8 dígitos
  
  // Termos
  aceitouTermosUso: boolean;
  versaoTermosUsoAceita: string;
  aceitouPoliticaPrivacidade: boolean;
  versaoPoliticaPrivacidadeAceita: string;
}
```

### 8. **Aceitar Convite Completo** ✅
Todos os campos obrigatórios ao aceitar convite:

```typescript
POST /api/registration/aceitar-convite/{inviteToken}
Body: {
  password: string;
  cpf: string;
  rg?: string;
  dataNascimento: Date;
  telefoneCelular: string;
  telefoneFixo?: string;
  rua: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
  cep: string;
  aceitouTermosUso: boolean;
  versaoTermosUsoAceita: string;
  aceitouPoliticaPrivacidade: boolean;
  versaoPoliticaPrivacidadeAceita: string;
}
```

---

## 🆕 Novas Telas a Implementar

### 1. **Tela: Recuperar Senha**

**Rota**: `/recuperar-senha` (solicitar) e `/redefinir-senha?token={token}` (redefinir)

#### Passo 1: Solicitar Recuperação

```tsx
const SolicitarRecuperacaoSenha = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [enviado, setEnviado] = useState(false);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      await api.post('/api/auth/solicitar-recuperacao-senha', { email });
      setEnviado(true);
      toast.success('Instruções enviadas para seu email!');
    } catch (error) {
      toast.error('Erro ao processar solicitação');
    } finally {
      setLoading(false);
    }
  };
  
  if (enviado) {
    return (
      <div className="recovery-success">
        <MailIcon className="success-icon" />
        <h2>📧 Verifique seu email</h2>
        <p>
          Se o email <strong>{email}</strong> estiver cadastrado, você receberá 
          instruções para recuperar sua senha.
        </p>
        <p className="helper-text">
          ⏰ O link expira em 2 horas
        </p>
        <Button onClick={() => navigate('/login')}>
          Voltar para Login
        </Button>
      </div>
    );
  }
  
  return (
    <div className="recovery-form">
      <div className="form-header">
        <LockIcon />
        <h1>🔐 Recuperar Senha</h1>
        <p>Digite seu email para receber instruções</p>
      </div>
      
      <form onSubmit={handleSubmit}>
        <Input
          type="email"
          label="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          placeholder="seu@email.com"
        />
        
        <Button type="submit" loading={loading} fullWidth>
          Enviar Instruções
        </Button>
        
        <Button 
          type="button" 
          variant="ghost" 
          onClick={() => navigate('/login')}
          fullWidth
        >
          Voltar para Login
        </Button>
      </form>
    </div>
  );
};
```

#### Passo 2: Redefinir Senha

```tsx
const RedefinirSenha = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const navigate = useNavigate();
  
  const [formData, setFormData] = useState({
    novaSenha: '',
    confirmacaoSenha: ''
  });
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  
  useEffect(() => {
    if (!token) {
      toast.error('Token inválido ou expirado');
      navigate('/recuperar-senha');
    }
  }, [token, navigate]);
  
  const validarSenha = (senha: string) => {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    return regex.test(senha);
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (formData.novaSenha !== formData.confirmacaoSenha) {
      toast.error('As senhas não conferem');
      return;
    }
    
    if (!validarSenha(formData.novaSenha)) {
      toast.error('Senha deve ter: 8+ caracteres, maiúscula, minúscula, número e caractere especial');
      return;
    }
    
    setLoading(true);
    try {
      const response = await api.post('/api/auth/redefinir-senha', {
        token,
        novaSenha: formData.novaSenha,
        confirmacaoSenha: formData.confirmacaoSenha
      });
      
      if (response.data.sucesso) {
        toast.success('Senha redefinida com sucesso!');
        setTimeout(() => navigate('/login'), 2000);
      } else {
        toast.error(response.data.mensagem);
      }
    } catch (error: any) {
      toast.error(error.response?.data?.mensagem || 'Token inválido ou expirado');
      setTimeout(() => navigate('/recuperar-senha'), 3000);
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <div className="reset-password-form">
      <div className="form-header">
        <KeyIcon />
        <h1>🔑 Nova Senha</h1>
        <p>Crie uma senha forte para sua conta</p>
      </div>
      
      <form onSubmit={handleSubmit}>
        <Input
          type={showPassword ? 'text' : 'password'}
          label="Nova Senha"
          value={formData.novaSenha}
          onChange={(e) => setFormData({ ...formData, novaSenha: e.target.value })}
          required
          rightIcon={
            <button 
              type="button" 
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? <EyeOffIcon /> : <EyeIcon />}
            </button>
          }
        />
        
        <div className="password-requirements">
          <p>A senha deve conter:</p>
          <ul>
            <li className={formData.novaSenha.length >= 8 ? 'valid' : ''}>
              {formData.novaSenha.length >= 8 ? '✓' : '○'} Mínimo 8 caracteres
            </li>
            <li className={/[A-Z]/.test(formData.novaSenha) ? 'valid' : ''}>
              {/[A-Z]/.test(formData.novaSenha) ? '✓' : '○'} Uma letra maiúscula
            </li>
            <li className={/[a-z]/.test(formData.novaSenha) ? 'valid' : ''}>
              {/[a-z]/.test(formData.novaSenha) ? '✓' : '○'} Uma letra minúscula
            </li>
            <li className={/\d/.test(formData.novaSenha) ? 'valid' : ''}>
              {/\d/.test(formData.novaSenha) ? '✓' : '○'} Um número
            </li>
            <li className={/[@$!%*?&]/.test(formData.novaSenha) ? 'valid' : ''}>
              {/[@$!%*?&]/.test(formData.novaSenha) ? '✓' : '○'} Um caractere especial (@$!%*?&)
            </li>
          </ul>
        </div>
        
        <Input
          type={showPassword ? 'text' : 'password'}
          label="Confirmar Senha"
          value={formData.confirmacaoSenha}
          onChange={(e) => setFormData({ ...formData, confirmacaoSenha: e.target.value })}
          required
        />
        
        {formData.confirmacaoSenha && formData.novaSenha !== formData.confirmacaoSenha && (
          <p className="error-message">As senhas não conferem</p>
        )}
        
        <Button type="submit" loading={loading} fullWidth>
          Redefinir Senha
        </Button>
      </form>
    </div>
  );
};
```

### 2. **Tela Atualizada: Empresa**

**Rota**: `/empresa`

```tsx
const EmpresaPage = () => {
  const { user } = useAuth();
  const canEdit = user?.role === 'DonoEmpresaPai';
  const [empresa, setEmpresa] = useState<CompanyInfoResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  
  useEffect(() => {
    fetchEmpresa();
  }, []);
  
  const fetchEmpresa = async () => {
    try {
      const response = await api.get('/api/UserProfile/empresa');
      setEmpresa(response.data);
    } catch (error) {
      toast.error('Erro ao carregar dados da empresa');
    } finally {
      setLoading(false);
    }
  };
  
  const handleSubmit = async (data: UpdateUserCompanyInfoRequest) => {
    try {
      const response = await api.put('/api/UserProfile/empresa', data);
      setEmpresa(response.data);
      setEditing(false);
      toast.success('Dados da empresa atualizados com sucesso!');
    } catch (error) {
      toast.error('Erro ao atualizar dados da empresa');
    }
  };
  
  if (loading) return <LoadingSkeleton />;
  if (!empresa) return <ErrorState />;
  
  return (
    <div className="empresa-page">
      <div className="page-header">
        <h1>🏢 {empresa.nome}</h1>
        {canEdit && !editing && (
          <Button onClick={() => setEditing(true)}>
            ✏️ Editar
          </Button>
        )}
      </div>
      
      {editing ? (
        <CompanyEditForm 
          empresa={empresa}
          onSubmit={handleSubmit}
          onCancel={() => setEditing(false)}
        />
      ) : (
        <CompanyViewMode empresa={empresa} />
      )}
    </div>
  );
};

const CompanyViewMode = ({ empresa }: { empresa: CompanyInfoResponse }) => {
  return (
    <div className="company-view">
      <div className="info-section">
        <h3>📋 Informações Básicas</h3>
        <InfoRow label="Razão Social" value={empresa.nome} />
        <InfoRow label="CNPJ" value={empresa.cnpjFormatado} />
        <InfoRow label="Tipo" value={empresa.tipo} />
        <InfoRow label="Modelo de Negócio" value={empresa.modeloNegocio} />
      </div>
      
      <div className="info-section">
        <h3>📞 Contatos</h3>
        <InfoRow 
          label="Telefone Celular" 
          value={empresa.telefoneCelular || 'Não informado'} 
        />
        <InfoRow 
          label="Telefone Fixo" 
          value={empresa.telefoneFixo || 'Não informado'} 
        />
      </div>
      
      {empresa.endereco && (
        <div className="info-section">
          <h3>📍 Endereço</h3>
          <InfoRow label="Endereço Completo" value={empresa.endereco.enderecoCompleto} />
          <div className="address-details">
            <InfoRow label="CEP" value={empresa.endereco.cep} />
            <InfoRow label="Cidade" value={empresa.endereco.cidade} />
            <InfoRow label="Estado" value={empresa.endereco.estado} />
            <InfoRow label="País" value={empresa.endereco.pais} />
          </div>
        </div>
      )}
    </div>
  );
};

const CompanyEditForm = ({ 
  empresa, 
  onSubmit, 
  onCancel 
}: { 
  empresa: CompanyInfoResponse;
  onSubmit: (data: UpdateUserCompanyInfoRequest) => void;
  onCancel: () => void;
}) => {
  const [formData, setFormData] = useState({
    nome: empresa.nome,
    telefoneCelular: empresa.telefoneCelular || '',
    telefoneFixo: empresa.telefoneFixo || '',
    rua: empresa.endereco?.rua || '',
    numero: empresa.endereco?.numero || '',
    complemento: empresa.endereco?.complemento || '',
    bairro: empresa.endereco?.bairro || '',
    cidade: empresa.endereco?.cidade || '',
    estado: empresa.endereco?.estado || '',
    pais: empresa.endereco?.pais || 'Brasil',
    cep: empresa.endereco?.cep || ''
  });
  
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };
  
  return (
    <form onSubmit={handleSubmit} className="company-edit-form">
      <div className="form-section">
        <h3>📋 Informações Básicas</h3>
        <Input
          label="Razão Social"
          value={formData.nome}
          onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
          required
        />
      </div>
      
      <div className="form-section">
        <h3>📞 Contatos</h3>
        <InputMask
          label="Telefone Celular"
          mask="(99) 99999-9999"
          value={formData.telefoneCelular}
          onChange={(e) => setFormData({ ...formData, telefoneCelular: e.target.value })}
          required
        />
        <InputMask
          label="Telefone Fixo"
          mask="(99) 9999-9999"
          value={formData.telefoneFixo}
          onChange={(e) => setFormData({ ...formData, telefoneFixo: e.target.value })}
        />
      </div>
      
      <div className="form-section">
        <h3>📍 Endereço</h3>
        <InputMask
          label="CEP"
          mask="99999-999"
          value={formData.cep}
          onChange={(e) => setFormData({ ...formData, cep: e.target.value })}
          required
        />
        <Input
          label="Rua"
          value={formData.rua}
          onChange={(e) => setFormData({ ...formData, rua: e.target.value })}
          required
        />
        <div className="form-row">
          <Input
            label="Número"
            value={formData.numero}
            onChange={(e) => setFormData({ ...formData, numero: e.target.value })}
            required
          />
          <Input
            label="Complemento"
            value={formData.complemento}
            onChange={(e) => setFormData({ ...formData, complemento: e.target.value })}
          />
        </div>
        <Input
          label="Bairro"
          value={formData.bairro}
          onChange={(e) => setFormData({ ...formData, bairro: e.target.value })}
          required
        />
        <div className="form-row">
          <Input
            label="Cidade"
            value={formData.cidade}
            onChange={(e) => setFormData({ ...formData, cidade: e.target.value })}
            required
          />
          <Select
            label="Estado"
            value={formData.estado}
            onChange={(e) => setFormData({ ...formData, estado: e.target.value })}
            required
          >
            <option value="">Selecione</option>
            {ESTADOS_BRASIL.map(estado => (
              <option key={estado.sigla} value={estado.sigla}>
                {estado.nome}
              </option>
            ))}
          </Select>
        </div>
        <Input
          label="País"
          value={formData.pais}
          onChange={(e) => setFormData({ ...formData, pais: e.target.value })}
          required
        />
      </div>
      
      <div className="form-actions">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="submit">
          Salvar Alterações
        </Button>
      </div>
    </form>
  );
};
```

### 3. **Componente Atualizado: Link de Recuperação de Senha**

Adicionar na tela de login:

```tsx
const LoginPage = () => {
  // ... código existente
  
  return (
    <div className="login-page">
      <form onSubmit={handleLogin}>
        {/* ... campos existentes ... */}
        
        <div className="form-footer">
          <Link to="/recuperar-senha" className="forgot-password-link">
            Esqueceu sua senha?
          </Link>
        </div>
        
        <Button type="submit" loading={loading} fullWidth>
          Entrar
        </Button>
      </form>
    </div>
  );
};
```

---

## 📊 Tipos TypeScript Atualizados

```typescript
// Company
interface CompanyInfoResponse {
  id: string;
  nome: string;
  cnpj: string;
  cnpjFormatado: string;
  tipo: 'Client' | 'Provider' | 'Both';
  modeloNegocio: 'MainCompany' | 'Standard' | 'ContractedPJ' | 'Freelancer';
  endereco: {
    rua: string;
    numero: string;
    complemento?: string;
    bairro: string;
    cidade: string;
    estado: string;
    pais: string;
    cep: string;
    enderecoCompleto: string;
  } | null;
  telefoneFixo?: string;
  telefoneCelular?: string;
}

interface UpdateUserCompanyInfoRequest {
  nome: string;
  telefoneCelular: string;
  telefoneFixo?: string;
  rua: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
  cep: string;
}

// Password Recovery
interface RequestPasswordResetRequest {
  email: string;
}

interface ResetPasswordRequest {
  token: string;
  novaSenha: string;
  confirmacaoSenha: string;
}

interface PasswordResetResponse {
  sucesso: boolean;
  mensagem: string;
}

// User Position
interface UpdateEmployeePositionRequest {
  cargo: string;
}

// Notification Preferences
interface NotificationPreferencesDTO {
  emailPagamentos: boolean;
  emailContratos: boolean;
  emailFuncionarios: boolean;
  emailRelatorios: boolean;
  sistemaPagamentos: boolean;
  sistemaContratos: boolean;
  sistemaFuncionarios: boolean;
  sistemaRelatorios: boolean;
}
```

---

## 🚀 Checklist de Implementação ATUALIZADO

### ✅ Novas Funcionalidades Prioritárias

#### Fase 1: Recuperação de Senha (ALTA PRIORIDADE)
- [ ] Criar rota `/recuperar-senha`
- [ ] Implementar formulário de solicitação
- [ ] Criar rota `/redefinir-senha?token={token}`
- [ ] Implementar formulário de redefinição
- [ ] Validação de senha com requisitos visuais
- [ ] Adicionar link "Esqueceu sua senha?" no login

#### Fase 2: Dados da Empresa (ALTA PRIORIDADE)
- [ ] Criar rota `/empresa`
- [ ] Implementar visualização de dados da empresa
- [ ] Implementar edição (apenas Dono)
- [ ] Formulário completo com endereço e telefones
- [ ] Validação de campos
- [ ] Estados brasileiros no select

#### Fase 3: Alterar Cargo de Funcionários (MÉDIA PRIORIDADE)
- [ ] Adicionar botão "Alterar Cargo" na lista de funcionários
- [ ] Modal de edição de cargo
- [ ] Select com cargos predefinidos + opção "Outro"
- [ ] Validação: apenas Dono pode alterar
- [ ] Toast de sucesso/erro

#### Fase 4: Notificações (MÉDIA PRIORIDADE)
- [ ] Implementar aba de notificações em Configurações
- [ ] Layout dinâmico por role
- [ ] Salvar preferências
- [ ] Feedback visual de salvamento

#### Fase 5: Campos Completos (BAIXA PRIORIDADE)
- [ ] Adicionar CPF no registro do Dono
- [ ] Adicionar RG no registro do Dono
- [ ] Adicionar Data de Nascimento no registro
- [ ] Adicionar todos os campos no aceitar convite
- [ ] Validações de CPF
- [ ] Máscaras de input

---

## 🔧 Utilitários Adicionais

### Estados Brasileiros

```typescript
export const ESTADOS_BRASIL = [
  { sigla: 'AC', nome: 'Acre' },
  { sigla: 'AL', nome: 'Alagoas' },
  { sigla: 'AP', nome: 'Amapá' },
  { sigla: 'AM', nome: 'Amazonas' },
  { sigla: 'BA', nome: 'Bahia' },
  { sigla: 'CE', nome: 'Ceará' },
  { sigla: 'DF', nome: 'Distrito Federal' },
  { sigla: 'ES', nome: 'Espírito Santo' },
  { sigla: 'GO', nome: 'Goiás' },
  { sigla: 'MA', nome: 'Maranhão' },
  { sigla: 'MT', nome: 'Mato Grosso' },
  { sigla: 'MS', nome: 'Mato Grosso do Sul' },
  { sigla: 'MG', nome: 'Minas Gerais' },
  { sigla: 'PA', nome: 'Pará' },
  { sigla: 'PB', nome: 'Paraíba' },
  { sigla: 'PR', nome: 'Paraná' },
  { sigla: 'PE', nome: 'Pernambuco' },
  { sigla: 'PI', nome: 'Piauí' },
  { sigla: 'RJ', nome: 'Rio de Janeiro' },
  { sigla: 'RN', nome: 'Rio Grande do Norte' },
  { sigla: 'RS', nome: 'Rio Grande do Sul' },
  { sigla: 'RO', nome: 'Rondônia' },
  { sigla: 'RR', nome: 'Roraima' },
  { sigla: 'SC', nome: 'Santa Catarina' },
  { sigla: 'SP', nome: 'São Paulo' },
  { sigla: 'SE', nome: 'Sergipe' },
  { sigla: 'TO', nome: 'Tocantins' }
];
```

### Validação de Senha

```typescript
export const validarSenha = (senha: string): {
  valida: boolean;
  requisitos: {
    minimoCaracteres: boolean;
    maiuscula: boolean;
    minuscula: boolean;
    numero: boolean;
    especial: boolean;
  };
} => {
  return {
    valida: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/.test(senha),
    requisitos: {
      minimoCaracteres: senha.length >= 8,
      maiuscula: /[A-Z]/.test(senha),
      minuscula: /[a-z]/.test(senha),
      numero: /\d/.test(senha),
      especial: /[@$!%*?&]/.test(senha)
    }
  };
};
```

### Validação de CPF

```typescript
export const validarCPF = (cpf: string): boolean => {
  cpf = cpf.replace(/[^\d]/g, '');
  
  if (cpf.length !== 11) return false;
  if (/^(\d)\1+$/.test(cpf)) return false;
  
  let soma = 0;
  let resto;
  
  for (let i = 1; i <= 9; i++) {
    soma += parseInt(cpf.substring(i - 1, i)) * (11 - i);
  }
  
  resto = (soma * 10) % 11;
  if (resto === 10 || resto === 11) resto = 0;
  if (resto !== parseInt(cpf.substring(9, 10))) return false;
  
  soma = 0;
  for (let i = 1; i <= 10; i++) {
    soma += parseInt(cpf.substring(i - 1, i)) * (12 - i);
  }
  
  resto = (soma * 10) % 11;
  if (resto === 10 || resto === 11) resto = 0;
  if (resto !== parseInt(cpf.substring(10, 11))) return false;
  
  return true;
};

export const formatarCPF = (cpf: string): string => {
  cpf = cpf.replace(/[^\d]/g, '');
  return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
};

export const mascaraCPF = (cpf: string): string => {
  if (!cpf) return '';
  cpf = cpf.replace(/[^\d]/g, '');
  return `***.***.${cpf.slice(-5, -2)}-${cpf.slice(-2)}`;
};
```

---

## 📋 Resumo das Mudanças

### ✅ O que está PRONTO no Backend:
1. ✅ Recuperação de senha com email
2. ✅ CORS configurado para produção
3. ✅ Campos completos no perfil (CPF, RG, Data Nascimento, Cargo, etc.)
4. ✅ Endpoint para alterar cargo de funcionários
5. ✅ Endpoints de dados da empresa (GET e PUT)
6. ✅ Notificações funcionando
7. ✅ Registro completo do Dono com todos os campos
8. ✅ Aceitar convite com todos os campos
9. ✅ Email de boas-vindas automático
10. ✅ Links de email apontam para frontend correto

### 🎯 O que PRECISA ser implementado no Frontend:
1. 🔴 Tela de recuperação de senha (solicitar)
2. 🔴 Tela de redefinir senha (com token)
3. 🔴 Tela de dados da empresa (visualização + edição)
4. 🔴 Funcionalidade de alterar cargo de funcionários
5. 🟡 Adicionar todos os campos no registro do Dono
6. 🟡 Adicionar todos os campos no aceitar convite
7. 🟡 Aba de notificações em Configurações
8. 🟢 Link "Esqueceu sua senha?" no login

### 🔗 Links Importantes:
- **API**: https://aureapi.gabrielsanztech.com.br
- **Swagger**: https://aureapi.gabrielsanztech.com.br (raiz)
- **Frontend**: https://aure.gabrielsanztech.com.br
- **Recuperar Senha**: https://aure.gabrielsanztech.com.br/recuperar-senha?token={token}
- **Aceitar Convite**: https://aure.gabrielsanztech.com.br/aceitar-convite?token={token}

---

**🎯 Próximos Passos:**
1. Implementar telas de recuperação de senha (Fase 1)
2. Implementar tela de dados da empresa (Fase 2)
3. Adicionar funcionalidade de alterar cargo (Fase 3)
4. Implementar preferências de notificações (Fase 4)
5. Completar campos nos formulários (Fase 5)

**📅 Data desta Atualização: 30/10/2025**
**✅ Todas as funcionalidades listadas já estão implementadas e testadas no backend!**
````