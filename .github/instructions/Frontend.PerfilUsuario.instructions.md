# 🎨 Frontend - Sistema de Perfil de Usuário e Configurações da Empresa

## 📋 Visão Geral

Este documento contém todas as instruções para implementar no **frontend** o sistema completo de perfil de usuário, configurações pessoais, informações empresariais e preferências de notificação no sistema Aure.

**Status do Backend**: ✅ Totalmente implementado e funcional
**API Base URL**: `https://aureapi.gabrielsanztech.com.br`
**Swagger**: Disponível em `https://aureapi.gabrielsanztech.com.br`

---

## 🏗️ Estrutura de Telas a Implementar

### 1. Dashboards Personalizados por Role

#### 📊 Dashboard DonoEmpresaPai
**Rota**: `/dashboard`

```tsx
// Componentes necessários:
<FinancialOverviewWidget />      // Valores, economia, próximos pagamentos
<ContractsStatsWidget />         // Ativos, vencendo, aguardando assinatura
<EmployeesStatsWidget />         // CLT, PJ, Financeiro, Jurídico
<BirthdaysWidget />              // Aniversariantes do mês
<QuickActionsWidget />           // Botões de ações rápidas

// Layout sugerido: Grid 2x3
```

#### 📊 Dashboard Financeiro
**Rota**: `/dashboard`

```tsx
// Componentes necessários:
<OperationalManagementWidget />  // Funcionários, contratos
<PendingTasksWidget />          // Tarefas pendentes
<BirthdaysWidget />             // Aniversariantes do mês
<QuickActionsWidget />          // Ações contextuais

// Layout sugerido: Grid 2x2
```

#### 📊 Dashboard Jurídico
**Rota**: `/dashboard`

```tsx
// Componentes necessários:
<LegalManagementWidget />       // Contratos, documentos
<QuickActionsWidget />          // Ações jurídicas

// Layout sugerido: Grid 1x2
```

#### 📊 Dashboard FuncionarioPJ
**Rota**: `/dashboard`

```tsx
// Componentes necessários:
<MyPaymentWidget />             // Próximo pagamento
<MyContractWidget />            // Status do contrato
<PaymentHistoryChart />         // Gráfico últimos 6 meses
<PendingActionsWidget />        // Contratos para assinar

// Layout sugerido: Grid 2x2
```

#### 📊 Dashboard FuncionarioCLT
**Rota**: `/dashboard`

```tsx
// Componentes necessários:
<MyCLTContractWidget />         // Contrato CLT
<DocumentsWidget />             // Documentos disponíveis
<InfoWidget />                  // Informações para RH

// Layout sugerido: Grid 1x3
```

---

### 2. Tela: Configurações (Perfil Pessoal)

**Rota**: `/configuracoes`

#### Estrutura em Abas

```tsx
<Tabs defaultValue="dados-pessoais">
  <TabsList>
    <TabsTrigger value="dados-pessoais">👤 Dados Pessoais</TabsTrigger>
    {role === 'FuncionarioPJ' && (
      <TabsTrigger value="empresa-pj">🏢 Minha Empresa PJ</TabsTrigger>
    )}
    <TabsTrigger value="notificacoes">🔔 Notificações</TabsTrigger>
    <TabsTrigger value="termos">📜 Termos e Privacidade</TabsTrigger>
  </TabsList>
  
  <TabsContent value="dados-pessoais">
    <PersonalDataForm />
  </TabsContent>
  
  {role === 'FuncionarioPJ' && (
    <TabsContent value="empresa-pj">
      <CompanyPJForm />
    </TabsContent>
  )}
  
  <TabsContent value="notificacoes">
    <NotificationPreferencesForm />
  </TabsContent>
  
  <TabsContent value="termos">
    <TermsAndPrivacyView />
  </TabsContent>
</Tabs>
```

#### Aba 1: Dados Pessoais - `<PersonalDataForm />`

```tsx
// Campos do formulário:
const PersonalDataForm = () => {
  return (
    <form onSubmit={handleSubmit}>
      {/* Avatar */}
      <AvatarUpload 
        currentAvatarUrl={user.avatarUrl}
        userName={user.nome}
        onUploadSuccess={handleAvatarUpdate}
      />
      
      {/* Dados Básicos */}
      <Input name="nome" label="Nome Completo" required />
      <Input name="email" label="Email" type="email" required />
      <InputMask name="cpf" label="CPF" mask="999.999.999-99" 
                 disabled={role !== 'DonoEmpresaPai' && !isOwnProfile} />
      <Input name="rg" label="RG" optional />
      <DatePicker name="dataNascimento" label="Data de Nascimento" optional />
      
      {/* Cargo - apenas para CLT e PJ */}
      {(role === 'FuncionarioCLT' || role === 'FuncionarioPJ') && (
        <CargoSelect name="cargo" label="Cargo" />
      )}
      
      {/* Contatos */}
      <InputMask name="telefoneCelular" label="Celular" mask="(99) 99999-9999" />
      <InputMask name="telefoneFixo" label="Telefone Fixo" mask="(99) 9999-9999" optional />
      
      {/* Endereço */}
      <AddressForm 
        isDonoEmpresaPai={role === 'DonoEmpresaPai'}
        companyAddress={companyAddress}
      />
      
      {/* Segurança */}
      <PasswordChangeForm />
      
      <div className="form-actions">
        <Button type="button" variant="outline">Cancelar</Button>
        <Button type="submit">Salvar</Button>
      </div>
    </form>
  );
};
```

#### Aba 2: Empresa PJ - `<CompanyPJForm />` (Apenas FuncionarioPJ)

```tsx
const CompanyPJForm = () => {
  return (
    <form onSubmit={handleSubmit}>
      <Input name="razaoSocial" label="Razão Social" required />
      <CnpjInput 
        name="cnpj" 
        label="CNPJ" 
        required
        onValidation={handleCnpjValidation}
      />
      
      {/* Validação CNPJ x Razão Social */}
      {cnpjValidation.divergencia && (
        <CnpjDivergenceModal 
          razaoSocialInformada={cnpjValidation.razaoSocialInformada}
          razaoSocialReceita={cnpjValidation.razaoSocialReceita}
          onCorrect={handleCorrectRazaoSocial}
          onConfirmDivergence={handleConfirmDivergence}
        />
      )}
      
      <AddressForm prefix="empresa" label="Endereço da Empresa" />
      
      <Select name="companyType" label="Tipo">
        <option value="Cliente">Cliente</option>
        <option value="Fornecedor">Fornecedor</option>
      </Select>
      
      <Select name="businessModel" label="Modelo">
        <option value="PJContratado">PJ Contratado</option>
      </Select>
      
      <div className="form-actions">
        <Button type="button" variant="outline">Cancelar</Button>
        <Button type="submit">Salvar</Button>
      </div>
    </form>
  );
};
```

#### Aba 3: Notificações - `<NotificationPreferencesForm />`

```tsx
// Layout dinâmico por role
const NotificationPreferencesForm = () => {
  const getNotificationOptions = (role: UserRole) => {
    switch (role) {
      case 'DonoEmpresaPai':
        return [
          { section: '📋 Contratos', options: [
            { key: 'receberEmailNovoContrato', label: 'Novo contrato criado' },
            { key: 'receberEmailContratoAssinado', label: 'Contrato assinado' },
            { key: 'receberEmailContratoVencendo', label: 'Contrato vencendo (30, 15, 7 dias)' }
          ]},
          { section: '💰 Pagamentos', options: [
            { key: 'receberEmailPagamentoProcessado', label: 'Pagamento processado com sucesso' },
            { key: 'receberEmailAlertasFinanceiros', label: 'Alertas financeiros (valores altos, etc.)' }
          ]},
          { section: '👥 Operações', options: [
            { key: 'receberEmailNovoFuncionario', label: 'Novo funcionário cadastrado' },
          ]},
          { section: '⚙️ Sistema', options: [
            { key: 'receberEmailAtualizacoesSistema', label: 'Atualizações do sistema' }
          ]}
        ];
      
      case 'Financeiro':
        return [
          { section: '📋 Contratos', options: [
            { key: 'receberEmailNovoContrato', label: 'Novo contrato criado' },
            { key: 'receberEmailContratoAssinado', label: 'Contrato assinado' },
            { key: 'receberEmailContratoVencendo', label: 'Contrato vencendo' }
          ]},
          { section: '💰 Pagamentos', options: [
            { key: 'receberEmailPagamentoProcessado', label: 'Pagamento processado (notificação interna)' }
          ]},
          { section: '👥 Operações', options: [
            { key: 'receberEmailNovoFuncionario', label: 'Novo funcionário cadastrado' }
          ]},
          { section: '⚙️ Sistema', options: [
            { key: 'receberEmailAtualizacoesSistema', label: 'Atualizações do sistema' }
          ]}
        ];
      
      case 'Juridico':
        return [
          { section: '📋 Contratos', options: [
            { key: 'receberEmailNovoContrato', label: 'Novo contrato criado' },
            { key: 'receberEmailContratoAssinado', label: 'Contrato assinado' },
            { key: 'receberEmailContratoVencendo', label: 'Contrato vencendo' }
          ]},
          { section: '👥 Operações', options: [
            { key: 'receberEmailNovoFuncionario', label: 'Novo funcionário cadastrado' }
          ]},
          { section: '⚙️ Sistema', options: [
            { key: 'receberEmailAtualizacoesSistema', label: 'Atualizações do sistema' }
          ]}
        ];
      
      case 'FuncionarioPJ':
        return [
          { section: '📋 Contratos', options: [
            { key: 'receberEmailNovoContrato', label: 'Novo contrato para assinar' },
            { key: 'receberEmailContratoVencendo', label: 'Contrato vencendo' }
          ]},
          { section: '💰 Pagamentos', options: [
            { key: 'receberEmailPagamentoRecebido', label: 'Pagamento recebido' }
          ]},
          { section: '⚙️ Sistema', options: [
            { key: 'receberEmailAtualizacoesSistema', label: 'Atualizações do sistema' }
          ]}
        ];
      
      case 'FuncionarioCLT':
        return [
          { section: '⚙️ Sistema', options: [
            { key: 'receberEmailAtualizacoesSistema', label: 'Atualizações do sistema' }
          ]},
          { section: 'ℹ️ Info', options: [
            { key: 'info', label: 'Para questões de pagamento, consulte o RH', disabled: true }
          ]}
        ];
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {getNotificationOptions(userRole).map(section => (
        <div key={section.section} className="notification-section">
          <h3>{section.section}</h3>
          {section.options.map(option => (
            <Checkbox
              key={option.key}
              name={option.key}
              label={option.label}
              disabled={option.disabled}
            />
          ))}
        </div>
      ))}
      
      <Button type="submit">Salvar</Button>
    </form>
  );
};
```

#### Aba 4: Termos e Privacidade - `<TermsAndPrivacyView />`

```tsx
const TermsAndPrivacyView = () => {
  return (
    <div className="terms-privacy-view">
      <div className="terms-section">
        <div className="status-card">
          <div className="status-header">
            <CheckIcon className="text-green-500" />
            <h3>Termos de Uso</h3>
          </div>
          <p>Status: Aceito</p>
          <p>Data de Aceite: {formatDate(user.dataAceiteTermosUso)}</p>
          <p>Versão Aceita: {user.versaoTermosUsoAceita}</p>
          <Button variant="outline" onClick={() => openTermsModal('termos')}>
            📄 Ver Termos de Uso Completos
          </Button>
        </div>
      </div>

      <div className="privacy-section">
        <div className="status-card">
          <div className="status-header">
            <CheckIcon className="text-green-500" />
            <h3>Política de Privacidade</h3>
          </div>
          <p>Status: Aceito</p>
          <p>Data de Aceite: {formatDate(user.dataAceitePoliticaPrivacidade)}</p>
          <p>Versão Aceita: {user.versaoPoliticaPrivacidadeAceita}</p>
          <Button variant="outline" onClick={() => openTermsModal('privacidade')}>
            📄 Ver Política de Privacidade Completa
          </Button>
        </div>
      </div>

      <div className="lgpd-section">
        <h3>📊 Dados e Privacidade (LGPD)</h3>
        <div className="lgpd-actions">
          <Button variant="outline" onClick={handleExportData}>
            Exportar Meus Dados
          </Button>
          <Button variant="destructive" onClick={handleRequestAccountDeletion}>
            Solicitar Exclusão de Conta
          </Button>
        </div>
      </div>
    </div>
  );
};
```

---

### 3. Tela: Funcionários (Lista Completa)

**Rota**: `/funcionarios`
**Acesso**: DonoEmpresaPai, Financeiro, Jurídico

```tsx
const EmployeesPage = () => {
  return (
    <div className="employees-page">
      <div className="page-header">
        <h1>👥 Funcionários</h1>
        <Button onClick={() => navigate('/funcionarios/convidar')}>
          + Convidar Usuário
        </Button>
      </div>

      {/* Filtros */}
      <div className="filters-section">
        <Select name="roleFilter" placeholder="Todos">
          <option value="">Todos</option>
          <option value="FuncionarioCLT">CLT</option>
          <option value="FuncionarioPJ">PJ</option>
          <option value="Financeiro">Financeiro</option>
          <option value="Juridico">Jurídico</option>
        </Select>
        
        <Select name="statusFilter" placeholder="Status">
          <option value="">Todos</option>
          <option value="Ativo">Ativo</option>
          <option value="Pendente">Pendente</option>
          <option value="Inativo">Inativo</option>
        </Select>
        
        <Input 
          name="search" 
          placeholder="Buscar por nome ou email..." 
          icon={<SearchIcon />}
        />
        
        <Button variant="outline" onClick={handleExport}>
          📊 Exportar
        </Button>
      </div>

      {/* Tabela */}
      <DataTable
        data={employees}
        columns={[
          { key: 'nome', label: 'Nome', sortable: true },
          { key: 'cargo', label: 'Cargo' },
          { key: 'role', label: 'Tipo', 
            render: (role) => <RoleBadge role={role} /> },
          { key: 'status', label: 'Status',
            render: (status) => <StatusBadge status={status} /> },
          { key: 'dataEntrada', label: 'Data Entrada', 
            render: (date) => formatDate(date) },
          { key: 'email', label: 'Email' },
          { key: 'telefoneCelular', label: 'Telefone' },
          { key: 'actions', label: 'Ações',
            render: (_, employee) => (
              <Button variant="ghost" onClick={() => viewEmployee(employee.id)}>
                👁️
              </Button>
            )}
        ]}
        pagination
        loading={loading}
      />
    </div>
  );
};
```

---

### 4. Tela: Empresa (Informações da Empresa Pai)

**Rota**: `/empresa`
**Acesso**: DonoEmpresaPai (edição), Financeiro/Jurídico (leitura)

```tsx
const CompanyPage = () => {
  const canEdit = userRole === 'DonoEmpresaPai';
  
  return (
    <div className="company-page">
      <div className="page-header">
        <h1>🏢 Informações da Empresa</h1>
      </div>

      <form onSubmit={canEdit ? handleSubmit : undefined}>
        <Input 
          name="razaoSocial" 
          label="Razão Social" 
          disabled={!canEdit}
          value={company.razaoSocial}
        />
        
        <Input 
          name="cnpj" 
          label="CNPJ" 
          disabled={true}
          value={company.cnpj}
          helperText="CNPJ não pode ser alterado"
        />

        <AddressForm 
          prefix="endereco"
          label="Endereço da Empresa"
          disabled={!canEdit}
          values={company.endereco}
        />

        <Select 
          name="companyType" 
          label="Tipo" 
          disabled={!canEdit}
          value={company.companyType}
        >
          <option value="Cliente">Cliente</option>
          <option value="Fornecedor">Fornecedor</option>
          <option value="Ambos">Ambos</option>
        </Select>

        <Select 
          name="businessModel" 
          label="Modelo" 
          disabled={!canEdit}
          value={company.businessModel}
        >
          <option value="EmpresaPrincipal">Empresa Principal</option>
        </Select>

        {/* Estatísticas */}
        <div className="company-stats">
          <h3>📊 Estatísticas</h3>
          <StatCard label="Funcionários Totais" value={company.totalFuncionarios} />
          <StatCard label="Contratos Ativos" value={company.contratosAtivos} />
          <StatCard label="Data de Cadastro" value={formatDate(company.dataCadastro)} />
        </div>

        {canEdit && (
          <div className="form-actions">
            <Button type="button" variant="outline">Cancelar</Button>
            <Button type="submit">Salvar</Button>
          </div>
        )}
      </form>
    </div>
  );
};
```

---

## 🔧 Componentes Reutilizáveis

### 1. Avatar Upload - `<AvatarUpload />`

```tsx
interface AvatarUploadProps {
  currentAvatarUrl?: string;
  userName: string;
  onUploadSuccess: (avatarUrl: string) => void;
}

const AvatarUpload = ({ currentAvatarUrl, userName, onUploadSuccess }: AvatarUploadProps) => {
  const [uploading, setUploading] = useState(false);
  const [preview, setPreview] = useState<string | null>(null);
  
  const handleFileSelect = async (file: File) => {
    // Validações
    if (!file.type.startsWith('image/')) {
      toast.error('Apenas imagens são permitidas');
      return;
    }
    
    if (file.size > 5 * 1024 * 1024) { // 5MB
      toast.error('Imagem deve ter no máximo 5MB');
      return;
    }
    
    // Preview
    const reader = new FileReader();
    reader.onload = (e) => setPreview(e.target?.result as string);
    reader.readAsDataURL(file);
    
    // Upload
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await api.post('/api/Users/avatar', formData);
      onUploadSuccess(response.data.avatarUrl);
      toast.success('Avatar atualizado com sucesso!');
    } catch (error) {
      toast.error('Erro ao fazer upload do avatar');
    } finally {
      setUploading(false);
      setPreview(null);
    }
  };
  
  const handleRemoveAvatar = async () => {
    try {
      await api.delete('/api/Users/avatar');
      onUploadSuccess(''); // Avatar removido
      toast.success('Avatar removido com sucesso!');
    } catch (error) {
      toast.error('Erro ao remover avatar');
    }
  };
  
  return (
    <div className="avatar-upload">
      <div className="avatar-preview">
        {preview ? (
          <img src={preview} alt="Preview" className="avatar-image" />
        ) : currentAvatarUrl ? (
          <img src={currentAvatarUrl} alt={userName} className="avatar-image" />
        ) : (
          <div className="avatar-initials">
            {getInitials(userName)}
          </div>
        )}
      </div>
      
      <div className="avatar-actions">
        <input
          type="file"
          accept="image/*"
          onChange={(e) => e.target.files?.[0] && handleFileSelect(e.target.files[0])}
          style={{ display: 'none' }}
          ref={fileInputRef}
        />
        
        <Button 
          type="button" 
          variant="outline" 
          onClick={() => fileInputRef.current?.click()}
          disabled={uploading}
        >
          {uploading ? 'Enviando...' : 'Alterar Foto'}
        </Button>
        
        {currentAvatarUrl && (
          <Button 
            type="button" 
            variant="ghost" 
            onClick={handleRemoveAvatar}
          >
            Remover
          </Button>
        )}
      </div>
    </div>
  );
};
```

### 2. CNPJ Input com Validação - `<CnpjInput />`

```tsx
interface CnpjInputProps {
  name: string;
  label: string;
  required?: boolean;
  onValidation: (validation: CnpjValidationResult) => void;
}

const CnpjInput = ({ name, label, required, onValidation }: CnpjInputProps) => {
  const [validating, setValidating] = useState(false);
  const [cnpjValue, setCnpjValue] = useState('');
  
  const validateCnpj = async (cnpj: string) => {
    if (!cnpj || cnpj.length < 18) return; // CNPJ completo: 99.999.999/9999-99
    
    setValidating(true);
    try {
      const response = await api.post('/api/Users/empresa-pj/validate-cnpj', { cnpj });
      onValidation(response.data);
    } catch (error) {
      toast.error('Erro ao validar CNPJ');
    } finally {
      setValidating(false);
    }
  };
  
  const handleCnpjChange = (value: string) => {
    setCnpjValue(value);
    
    // Debounce validation
    const timeoutId = setTimeout(() => {
      validateCnpj(value);
    }, 1000);
    
    return () => clearTimeout(timeoutId);
  };
  
  return (
    <div className="cnpj-input">
      <InputMask
        name={name}
        label={label}
        mask="99.999.999/9999-99"
        required={required}
        value={cnpjValue}
        onChange={handleCnpjChange}
        rightIcon={validating ? <SpinnerIcon /> : undefined}
      />
    </div>
  );
};
```

### 3. Modal de Divergência CNPJ - `<CnpjDivergenceModal />`

```tsx
interface CnpjDivergenceModalProps {
  razaoSocialInformada: string;
  razaoSocialReceita: string;
  onCorrect: (razaoSocialCorreta: string) => void;
  onConfirmDivergence: () => void;
  onCancel: () => void;
}

const CnpjDivergenceModal = ({ 
  razaoSocialInformada, 
  razaoSocialReceita, 
  onCorrect, 
  onConfirmDivergence,
  onCancel 
}: CnpjDivergenceModalProps) => {
  return (
    <Modal open={true} onClose={onCancel}>
      <div className="cnpj-divergence-modal">
        <div className="modal-header">
          <WarningIcon className="text-yellow-500" />
          <h2>⚠️ Divergência de Razão Social</h2>
        </div>
        
        <div className="modal-content">
          <p>A Razão Social informada não corresponde aos dados da Receita Federal.</p>
          
          <div className="comparison">
            <div className="informed">
              <label>Informado por você:</label>
              <p className="value">{razaoSocialInformada}</p>
            </div>
            
            <div className="official">
              <label>Consta na Receita:</label>
              <p className="value official-value">{razaoSocialReceita}</p>
            </div>
          </div>
          
          <p>Deseja corrigir ou confirmar a divergência?</p>
        </div>
        
        <div className="modal-actions">
          <Button 
            variant="outline" 
            onClick={onCancel}
          >
            Cancelar
          </Button>
          
          <Button 
            variant="destructive" 
            onClick={onConfirmDivergence}
          >
            Manter Minha Versão
          </Button>
          
          <Button 
            variant="primary" 
            onClick={() => onCorrect(razaoSocialReceita)}
          >
            Corrigir para Razão Oficial
          </Button>
        </div>
      </div>
    </Modal>
  );
};
```

### 4. Cargo Select - `<CargoSelect />`

```tsx
const CARGOS_PREDEFINIDOS = [
  'Desenvolvedor Full Stack',
  'Desenvolvedor Frontend', 
  'Desenvolvedor Backend',
  'Designer UI/UX',
  'Analista de Sistemas',
  'Analista Financeiro',
  'Gerente de Projetos',
  'Consultor',
  'Recepcionista',
  'Assistente Administrativo',
  'Contador',
  'Advogado',
  'Outro'
];

const CargoSelect = ({ name, label, value, onChange }: CargoSelectProps) => {
  const [showCustomField, setShowCustomField] = useState(false);
  const [selectedCargo, setSelectedCargo] = useState(value || '');
  
  const handleCargoChange = (cargo: string) => {
    setSelectedCargo(cargo);
    
    if (cargo === 'Outro') {
      setShowCustomField(true);
      onChange(''); // Reset value for custom input
    } else {
      setShowCustomField(false);
      onChange(cargo);
    }
  };
  
  return (
    <div className="cargo-select">
      <Select 
        name={showCustomField ? undefined : name}
        label={label}
        value={selectedCargo}
        onChange={handleCargoChange}
      >
        <option value="">Selecione um cargo</option>
        {CARGOS_PREDEFINIDOS.map(cargo => (
          <option key={cargo} value={cargo}>{cargo}</option>
        ))}
      </Select>
      
      {showCustomField && (
        <Input
          name={name}
          label="Especifique o cargo"
          placeholder="Digite o cargo..."
          required
          onChange={onChange}
        />
      )}
    </div>
  );
};
```

### 5. Widget de Aniversariantes - `<BirthdaysWidget />`

```tsx
const BirthdaysWidget = () => {
  const [birthdays, setBirthdays] = useState<BirthdayItem[]>([]);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    fetchBirthdays();
  }, []);
  
  const fetchBirthdays = async () => {
    try {
      const response = await api.get('/api/Users/aniversariantes-mes');
      setBirthdays(response.data);
    } catch (error) {
      console.error('Erro ao buscar aniversariantes:', error);
    } finally {
      setLoading(false);
    }
  };
  
  if (loading) return <WidgetSkeleton />;
  
  return (
    <div className="birthdays-widget">
      <div className="widget-header">
        <h3>🎂 Aniversariantes do Mês</h3>
      </div>
      
      <div className="widget-content">
        {birthdays.length === 0 ? (
          <p className="no-birthdays">Nenhum aniversariante este mês</p>
        ) : (
          <div className="birthdays-list">
            {birthdays.map(birthday => (
              <div key={birthday.userId} className="birthday-item">
                <Avatar 
                  src={birthday.avatarUrl} 
                  name={birthday.nome}
                  size="sm"
                />
                <div className="birthday-info">
                  <p className="name">{birthday.nome}</p>
                  <p className="role-date">
                    {birthday.cargo} - {formatDate(birthday.dataAniversario, 'dd/MM')}
                  </p>
                </div>
                <div className="birthday-date">
                  {getDaysUntilBirthday(birthday.dataAniversario) === 0 ? (
                    <span className="today">Hoje! 🎉</span>
                  ) : (
                    <span>{formatDate(birthday.dataAniversario, 'dd/MM')}</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
```

### 6. Modal de Aceite de Termos - `<TermsAcceptanceModal />`

```tsx
interface TermsAcceptanceModalProps {
  open: boolean;
  versaoTermosUso: string;
  versaoPoliticaPrivacidade: string;
  onAccept: (acceptedTerms: AcceptedTerms) => void;
  onReject: () => void;
}

const TermsAcceptanceModal = ({ 
  open, 
  versaoTermosUso, 
  versaoPoliticaPrivacidade, 
  onAccept, 
  onReject 
}: TermsAcceptanceModalProps) => {
  const [activeTab, setActiveTab] = useState<'termos' | 'privacidade'>('termos');
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  const [acceptedPrivacy, setAcceptedPrivacy] = useState(false);
  
  const canProceed = acceptedTerms && acceptedPrivacy;
  
  const handleAccept = () => {
    if (canProceed) {
      onAccept({
        versaoTermosUso,
        versaoPoliticaPrivacidade,
        aceitouTermosUso: true,
        aceitouPoliticaPrivacidade: true
      });
    }
  };
  
  return (
    <Modal open={open} onClose={onReject} size="lg">
      <div className="terms-acceptance-modal">
        <div className="modal-header">
          <WarningIcon className="text-yellow-500" />
          <h2>⚠️ Termos de Uso e Política de Privacidade</h2>
        </div>
        
        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList>
            <TabsTrigger value="termos">Termos de Uso</TabsTrigger>
            <TabsTrigger value="privacidade">Política de Privacidade</TabsTrigger>
          </TabsList>
          
          <TabsContent value="termos">
            <div className="terms-content">
              <div className="document-header">
                <h3>TERMOS DE USO - Versão {versaoTermosUso}</h3>
                <p>Última atualização: {formatDate(new Date())}</p>
              </div>
              
              <div className="scrollable-content">
                <TermsOfUseContent />
              </div>
            </div>
          </TabsContent>
          
          <TabsContent value="privacidade">
            <div className="privacy-content">
              <div className="document-header">
                <h3>POLÍTICA DE PRIVACIDADE - Versão {versaoPoliticaPrivacidade}</h3>
                <p>Última atualização: {formatDate(new Date())}</p>
              </div>
              
              <div className="scrollable-content">
                <PrivacyPolicyContent />
              </div>
            </div>
          </TabsContent>
        </Tabs>
        
        <div className="acceptance-checkboxes">
          <Checkbox
            checked={acceptedTerms}
            onChange={setAcceptedTerms}
            label={`Li e aceito os Termos de Uso (v${versaoTermosUso})`}
          />
          
          <Checkbox
            checked={acceptedPrivacy}
            onChange={setAcceptedPrivacy}
            label={`Li e aceito a Política de Privacidade (v${versaoPoliticaPrivacidade})`}
          />
        </div>
        
        <div className="acceptance-warning">
          <WarningIcon className="text-yellow-500" />
          <p>⚠️ Ambos os documentos devem ser aceitos</p>
          
          <div className="agreement-details">
            <p>Ao aceitar, você concorda com:</p>
            <ul>
              <li>• Uso de dados conforme política LGPD</li>
              <li>• Normas de conduta da plataforma</li>
              <li>• Responsabilidades contratuais</li>
              <li>• Coleta e armazenamento de CPF/RG</li>
            </ul>
          </div>
        </div>
        
        <div className="modal-actions">
          <Button variant="outline" onClick={onReject}>
            Recusar
          </Button>
          
          <Button 
            variant="primary" 
            onClick={handleAccept}
            disabled={!canProceed}
          >
            Aceitar e Continuar
          </Button>
        </div>
      </div>
    </Modal>
  );
};
```

---

## 🌐 Integração com API

### Configuração da API

**URL Base da API**: `https://aureapi.gabrielsanztech.com.br`
**URL Base do Frontend**: `https://aure.gabrielsanztech.com.br`

#### Configuração no Frontend

```typescript
// config/api.ts
export const API_CONFIG = {
  baseURL: 'https://aureapi.gabrielsanztech.com.br',
  frontendURL: 'https://aure.gabrielsanztech.com.br',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  }
};

// Configuração do Axios
import axios from 'axios';

export const api = axios.create(API_CONFIG);

// Interceptor para adicionar token de autenticação
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para tratamento de erros
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado, redirecionar para login
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

#### Variáveis de Ambiente

```bash
# .env.local (para desenvolvimento)
NEXT_PUBLIC_API_URL=https://aureapi.gabrielsanztech.com.br
NEXT_PUBLIC_APP_URL=http://localhost:3000

# .env.production (para produção)
NEXT_PUBLIC_API_URL=https://aureapi.gabrielsanztech.com.br
NEXT_PUBLIC_APP_URL=https://aure.gabrielsanztech.com.br
```

#### Importante: CORS e HTTPS

🔒 **A API está configurada com HTTPS e CORS apropriado para produção:**
- ✅ SSL/TLS habilitado
- ✅ CORS configurado para origens específicas em produção
- ✅ AllowedHosts configurado para aceitar requisições do domínio
- ✅ Swagger disponível diretamente na raiz da API
- ✅ Emails com links apontam para o frontend (não para API)

**Domínios permitidos no CORS (produção):**
- `https://aure.gabrielsanztech.com.br` (Frontend principal)
- `https://app.gabrielsanztech.com.br` (Alternativo)
- `https://admin.gabrielsanztech.com.br` (Painel admin)

**Configurações de Email:**
- ✅ Links de convite: `https://aure.gabrielsanztech.com.br/aceitar-convite?token={token}`
- ✅ Links de login: `https://aure.gabrielsanztech.com.br/login`
- ✅ Email de boas-vindas: Enviado automaticamente ao criar conta de Dono da Empresa

📝 **Para desenvolvimento local**, certifique-se de que seu frontend esteja rodando em `http://localhost:3000` ou configure o CORS no backend se necessário.

### Endpoints Disponíveis

#### 0. Autenticação e Registro

```typescript
// Registrar primeiro usuário (Dono da Empresa Pai)
POST /api/registration/admin-empresa
Body: {
  companyName: string;
  companyCnpj: string; // 14 dígitos, apenas números
  companyType: "Client" | "Provider" | "Both";
  businessModel: "MainCompany" | "Standard" | "ContractedPJ" | "Freelancer";
  name: string;
  email: string;
  password: string;
  telefoneCelular: string; // 10-11 dígitos
  rua: string;
  cidade: string;
  estado: string; // 2 letras (ex: SP)
  pais: string;
  cep: string; // 8 dígitos
  telefoneFixo?: string; // Opcional, 10 dígitos
}
Response: UserResponse
// ✅ Email de boas-vindas enviado automaticamente

// Login
POST /api/auth/login
Body: { email: string; password: string }
Response: LoginResponse

// Convidar usuário (Financeiro, Jurídico ou PJ)
POST /api/registration/convidar-usuario
Headers: { Authorization: "Bearer {token}" }
Body: InviteUserRequest
Response: UserResponse
// ✅ Email de convite enviado automaticamente para: https://aure.gabrielsanztech.com.br/aceitar-convite?token={token}

// Aceitar convite
POST /api/registration/aceitar-convite/{inviteToken}
Body: {
  password: string;
  telefoneCelular: string;
  rua: string;
  cidade: string;
  estado: string;
  pais: string;
  cep: string;
  telefoneFixo?: string;
}
Response: UserResponse
```

#### 1. Perfil Completo
```typescript
// Buscar perfil do usuário logado
GET /api/Users/perfil-completo
Response: UserProfileResponse

// Atualizar perfil completo
PUT /api/Users/perfil-completo
Body: UpdateFullProfileRequest
Response: UserProfileResponse
```

#### 2. Avatar
```typescript
// Upload de avatar
POST /api/Users/avatar
Body: FormData (file: File)
Response: { avatarUrl: string, thumbnailUrl: string }

// Remover avatar
DELETE /api/Users/avatar
Response: { success: boolean }
```

#### 3. Empresa PJ (Apenas FuncionarioPJ)
```typescript
// Buscar dados da empresa PJ
GET /api/Users/empresa-pj
Response: CompanyPJResponse

// Atualizar empresa PJ
PUT /api/Users/empresa-pj
Body: UpdateCompanyPJRequest
Response: UpdateCompanyPJResponse

// Validar CNPJ (utilizar para validação em tempo real)
POST /api/Users/empresa-pj/validate-cnpj
Body: { cnpj: string }
Response: CnpjValidationResponse
```

#### 4. Preferências de Notificação
```typescript
// Buscar preferências
GET /api/Users/notificacoes/preferencias
Response: NotificationPreferencesDTO

// Atualizar preferências
PUT /api/Users/notificacoes/preferencias
Body: NotificationPreferencesDTO
Response: NotificationPreferencesDTO
```

#### 5. Termos e Privacidade
```typescript
// Buscar versões atuais dos termos
GET /api/Users/termos/versoes
Response: TermsVersionsResponse

// Aceitar termos
POST /api/Users/aceitar-termos
Body: AcceptTermsRequest
Response: { success: boolean }
```

#### 6. Empresa (Informações da Empresa Pai)
```typescript
// Buscar informações da empresa pai
GET /api/Companies/empresa-pai
Response: CompanyInfoResponse

// Atualizar empresa pai (apenas DonoEmpresaPai)
PUT /api/Companies/empresa-pai
Body: UpdateCompanyRequest
Response: CompanyInfoResponse
```

#### 7. Funcionários
```typescript
// Listar funcionários com filtros
GET /api/Users/funcionarios?pageNumber=1&pageSize=20&role=FuncionarioPJ&status=Ativo&busca=joão
Response: PagedResponse<EmployeeListItemResponse>

// Exportar funcionários
GET /api/Users/funcionarios/exportar?format=excel&role=FuncionarioPJ
Response: File (Excel ou PDF)

// Buscar aniversariantes do mês
GET /api/Users/aniversariantes-mes
Response: BirthdayItem[]
```

#### 8. LGPD
```typescript
// Exportar dados do usuário
GET /api/Users/exportar-dados
Response: File (JSON ou PDF)

// Solicitar exclusão de conta
DELETE /api/Users/solicitar-exclusao
Body: { confirmacao: string }
Response: { success: boolean, message: string }
```

### Tipos TypeScript

```typescript
// User Profile
interface UserProfileResponse {
  id: string;
  nome: string;
  email: string;
  avatarUrl?: string;
  dataNascimento?: string;
  cpfMascarado?: string; // ***.***.123-45
  cpf?: string; // Apenas para DonoEmpresaPai
  rg?: string;
  cargo?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  enderecoCompleto?: string;
  // ... demais campos
}

interface UpdateFullProfileRequest {
  nome?: string;
  email?: string;
  dataNascimento?: string;
  cpf?: string;
  rg?: string;
  cargo?: string;
  telefoneCelular?: string;
  telefoneFixo?: string;
  enderecoRua?: string;
  enderecoNumero?: string;
  enderecoComplemento?: string;
  enderecoBairro?: string;
  enderecoCidade?: string;
  enderecoEstado?: string;
  enderecoPais?: string;
  enderecoCep?: string;
  senhaAtual?: string;
  novaSenha?: string;
}

// Company PJ
interface UpdateCompanyPJRequest {
  razaoSocial?: string;
  cnpj?: string;
  enderecoRua?: string;
  enderecoNumero?: string;
  enderecoComplemento?: string;
  enderecoBairro?: string;
  enderecoCidade?: string;
  enderecoEstado?: string;
  enderecoPais?: string;
  enderecoCep?: string;
  companyType?: 'Cliente' | 'Fornecedor';
  businessModel?: 'PJContratado';
  confirmarDivergenciaRazaoSocial?: boolean;
}

interface CnpjValidationResponse {
  sucesso: boolean;
  mensagem?: string;
  empresa?: CompanyPJData;
  divergenciaRazaoSocial: boolean;
  razaoSocialReceita?: string;
  razaoSocialInformada?: string;
  requerConfirmacao: boolean;
}

// Notifications
interface NotificationPreferencesDTO {
  receberEmailNovoContrato: boolean;
  receberEmailContratoAssinado: boolean;
  receberEmailContratoVencendo: boolean;
  receberEmailPagamentoProcessado: boolean;
  receberEmailPagamentoRecebido: boolean;
  receberEmailNovoFuncionario: boolean;
  receberEmailAlertasFinanceiros: boolean;
  receberEmailAtualizacoesSistema: boolean;
}

// Terms
interface AcceptTermsRequest {
  versaoTermosUso: string;
  versaoPoliticaPrivacidade: string;
  aceitouTermosUso: boolean;
  aceitouPoliticaPrivacidade: boolean;
}

interface TermsVersionsResponse {
  versaoTermosUsoAtual: string;
  versaoPoliticaPrivacidadeAtual: string;
  usuarioPrecisaAceitar: boolean;
}

// Company
interface CompanyInfoResponse {
  id: string;
  razaoSocial: string;
  cnpj: string;
  companyType: 'Cliente' | 'Fornecedor' | 'Ambos';
  businessModel: 'EmpresaPrincipal';
  enderecoCompleto?: string;
  totalFuncionarios: number;
  contratosAtivos: number;
  dataCadastro: string;
}

// Employees
interface EmployeeListItemResponse {
  id: string;
  nome: string;
  email: string;
  role: string;
  cargo?: string;
  status: 'Ativo' | 'Pendente' | 'Inativo';
  dataEntrada: string;
  telefoneCelular?: string;
}

interface BirthdayItem {
  userId: string;
  nome: string;
  cargo?: string;
  avatarUrl?: string;
  dataAniversario: string;
}

// Pagination
interface PagedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

---

## 🎨 Estilo e Design System

### Cores por Role

```css
/* DonoEmpresaPai */
.role-dono { color: #8B5CF6; } /* Purple */

/* Financeiro */
.role-financeiro { color: #10B981; } /* Green */

/* Jurídico */
.role-juridico { color: #F59E0B; } /* Yellow */

/* FuncionarioPJ */
.role-pj { color: #3B82F6; } /* Blue */

/* FuncionarioCLT */
.role-clt { color: #6B7280; } /* Gray */
```

### Status Badges

```css
/* Status Ativo */
.status-ativo {
  background: #DCFCE7;
  color: #16A34A;
  border: 1px solid #BBF7D0;
}

/* Status Pendente */
.status-pendente {
  background: #FEF3C7;
  color: #D97706;
  border: 1px solid #FDE68A;
}

/* Status Inativo */
.status-inativo {
  background: #FEE2E2;
  color: #DC2626;
  border: 1px solid #FECACA;
}
```

### Avatar com Iniciais

```css
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.avatar-lg {
  width: 80px;
  height: 80px;
  font-size: 24px;
}

.avatar-sm {
  width: 32px;
  height: 32px;
  font-size: 12px;
}
```

---

## ⚡ Funcionalidades Especiais

### 1. Sincronização de Endereço DonoEmpresaPai

```typescript
// Quando DonoEmpresaPai atualiza perfil, endereço da empresa deve ser sincronizado
const handleDonoProfileUpdate = async (profileData: UpdateFullProfileRequest) => {
  try {
    // Atualizar perfil
    await api.put('/api/Users/perfil-completo', profileData);
    
    // Se endereço foi alterado, atualizar empresa também
    if (profileData.enderecoRua || profileData.enderecoCidade) {
      const companyData = {
        enderecoRua: profileData.enderecoRua,
        enderecoNumero: profileData.enderecoNumero,
        enderecoComplemento: profileData.enderecoComplemento,
        enderecoBairro: profileData.enderecoBairro,
        enderecoCidade: profileData.enderecoCidade,
        enderecoEstado: profileData.enderecoEstado,
        enderecoPais: profileData.enderecoPais,
        enderecoCep: profileData.enderecoCep,
      };
      
      await api.put('/api/Companies/empresa-pai', companyData);
    }
    
    toast.success('Perfil e empresa atualizados com sucesso!');
  } catch (error) {
    toast.error('Erro ao atualizar perfil');
  }
};
```

### 2. Validação CNPJ em Tempo Real

```typescript
// Debounced CNPJ validation
const useCnpjValidation = () => {
  const [validation, setValidation] = useState<CnpjValidationResponse | null>(null);
  const [loading, setLoading] = useState(false);
  
  const validateCnpj = useCallback(
    debounce(async (cnpj: string) => {
      if (!cnpj || cnpj.length < 18) return;
      
      setLoading(true);
      try {
        const response = await api.post('/api/Users/empresa-pj/validate-cnpj', { cnpj });
        setValidation(response.data);
      } catch (error) {
        setValidation(null);
      } finally {
        setLoading(false);
      }
    }, 1000),
    []
  );
  
  return { validation, loading, validateCnpj };
};
```

### 3. Exportação de Dados LGPD

```typescript
const handleExportUserData = async () => {
  try {
    const response = await api.get('/api/Users/exportar-dados', {
      responseType: 'blob'
    });
    
    const blob = new Blob([response.data], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `dados_usuario_${new Date().toISOString().split('T')[0]}.json`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
    
    toast.success('Dados exportados com sucesso!');
  } catch (error) {
    toast.error('Erro ao exportar dados');
  }
};
```

### 4. Solicitação de Exclusão de Conta

```typescript
const handleRequestAccountDeletion = async () => {
  const confirmed = await showConfirmDialog({
    title: '⚠️ Excluir Conta',
    message: `
      Tem certeza que deseja excluir sua conta?
      
      ⚠️ ATENÇÃO:
      • Seus dados pessoais serão anonimizados
      • Documentos fiscais serão mantidos por 5 anos (legislação brasileira)
      • Esta ação NÃO pode ser desfeita
      
      Digite "CONFIRMAR EXCLUSÃO" para prosseguir:
    `,
    confirmText: 'CONFIRMAR EXCLUSÃO',
    destructive: true
  });
  
  if (confirmed) {
    try {
      await api.delete('/api/Users/solicitar-exclusao', {
        data: { confirmacao: 'CONFIRMAR EXCLUSÃO' }
      });
      
      toast.success('Solicitação de exclusão processada. Você será desconectado.');
      
      // Logout automático
      setTimeout(() => {
        logout();
        navigate('/login');
      }, 2000);
    } catch (error) {
      toast.error('Erro ao processar solicitação de exclusão');
    }
  }
};
```

---

## 📱 Responsividade

### Breakpoints

```css
/* Mobile First */
.dashboard-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: 1fr;
}

/* Tablet */
@media (min-width: 768px) {
  .dashboard-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* Large Desktop */
@media (min-width: 1280px) {
  .dashboard-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}
```

### Configurações Mobile

```tsx
// Configurações em mobile - usar drawer/bottom sheet
const ConfigMobile = () => {
  return (
    <BottomSheet>
      <div className="config-mobile">
        <div className="config-section">
          <h3>👤 Perfil</h3>
          <ConfigItem icon="📝" label="Editar Dados" onClick={() => navigate('/perfil/editar')} />
          <ConfigItem icon="🔔" label="Notificações" onClick={() => navigate('/perfil/notificacoes')} />
        </div>
        
        {role === 'FuncionarioPJ' && (
          <div className="config-section">
            <h3>🏢 Empresa PJ</h3>
            <ConfigItem icon="✏️" label="Editar Empresa" onClick={() => navigate('/empresa-pj')} />
          </div>
        )}
        
        <div className="config-section">
          <h3>📜 Legal</h3>
          <ConfigItem icon="📄" label="Termos e Privacidade" onClick={() => navigate('/termos')} />
          <ConfigItem icon="📊" label="Exportar Dados" onClick={handleExportData} />
        </div>
      </div>
    </BottomSheet>
  );
};
```

---

## 🚀 Checklist de Implementação

### ✅ Fase 1: Estrutura Base
- [ ] Configurar rotas (`/dashboard`, `/configuracoes`, `/funcionarios`, `/empresa`)
- [ ] Criar componentes base (Avatar, InputMask, DatePicker, etc.)
- [ ] Implementar sistema de tipos TypeScript
- [ ] Configurar integração com API (axios/fetch)

### ✅ Fase 2: Dashboards
- [ ] Dashboard DonoEmpresaPai com 4 widgets
- [ ] Dashboard Financeiro com 3 widgets
- [ ] Dashboard Jurídico com 2 widgets
- [ ] Dashboard FuncionarioPJ com 4 widgets
- [ ] Dashboard FuncionarioCLT com 3 widgets
- [ ] Widget de aniversariantes funcional

### ✅ Fase 3: Configurações
- [ ] Aba Dados Pessoais com todos os campos
- [ ] Upload de avatar funcional
- [ ] Aba Empresa PJ (apenas FuncionarioPJ)
- [ ] Validação CNPJ em tempo real
- [ ] Modal de divergência CNPJ
- [ ] Aba Notificações dinâmica por role
- [ ] Aba Termos e Privacidade

### ✅ Fase 4: Listagem de Funcionários
- [ ] Tabela com filtros e busca
- [ ] Exportação Excel/PDF
- [ ] Badges de status e role
- [ ] Paginação

### ✅ Fase 5: Empresa
- [ ] Tela de informações da empresa
- [ ] Edição apenas para DonoEmpresaPai
- [ ] Estatísticas visuais

### ✅ Fase 6: Funcionalidades Especiais
- [ ] Modal de aceite de termos
- [ ] Exportação LGPD
- [ ] Solicitação de exclusão de conta
- [ ] Sincronização de endereço DonoEmpresaPai

### ✅ Fase 7: Testes e Polimento
- [ ] Testes de responsividade
- [ ] Validação de formulários
- [ ] Loading states
- [ ] Error handling
- [ ] Acessibilidade (a11y)

---

## 🔧 Ferramentas e Bibliotecas Sugeridas

### React/Next.js
```bash
# Componentes UI
npm install @radix-ui/react-tabs
npm install @radix-ui/react-dialog
npm install @radix-ui/react-select

# Máscaras e validação
npm install react-input-mask
npm install react-hook-form
npm install zod

# Utilitários
npm install date-fns
npm install clsx
npm install tailwind-merge

# Gráficos (para dashboard)
npm install recharts

# Toast/Notificações
npm install react-hot-toast
```

### Utilitários
```typescript
// Formatação
export const formatCpf = (cpf: string) => {
  return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
};

export const maskCpf = (cpf: string) => {
  if (!cpf) return '';
  return `***.***.${cpf.slice(-5)}`;
};

export const formatCnpj = (cnpj: string) => {
  return cnpj.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
};

export const getInitials = (name: string) => {
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
};

// Validações
export const validateCpf = (cpf: string): boolean => {
  // Implementar validação de CPF
  return true; // Simplificado
};

export const validateEmail = (email: string): boolean => {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email);
};
```

---

## 📋 Observações Finais

### 🎯 Prioridades
1. **Alta**: Dashboards e configurações básicas
2. **Média**: Funcionalidades especiais (CNPJ, LGPD)
3. **Baixa**: Polimentos e animações

### 🔒 Segurança
- Sempre validar permissões no frontend
- Mascarar dados sensíveis (CPF)
- Implementar rate limiting em uploads
- Validar arquivos no upload de avatar

### 📱 UX/UI
- Loading states em todas as operações
- Feedback visual para ações
- Confirmações para ações destrutivas
- Responsividade em todos os componentes

### 🚀 Performance
- Lazy loading de componentes pesados
- Debounce em validações
- Cache de dados frequentes
- Otimização de imagens (avatar)

---

**📋 DOCUMENTO COMPLETO PARA IMPLEMENTAÇÃO FRONTEND!** 

Este documento contém todas as especificações, componentes, endpoints e exemplos de código necessários para implementar completamente o sistema de perfil de usuário no frontend. 

**🎯 Próximo passo: Começar pela Fase 1 (Estrutura Base) e seguir o checklist sequencialmente.**