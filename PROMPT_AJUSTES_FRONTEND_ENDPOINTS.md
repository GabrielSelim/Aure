# 🔄 Ajustes Necessários no Frontend - Endpoints de Empresa

## 📋 Contexto
Foram realizadas alterações nos endpoints de gerenciamento de dados da empresa para:
1. Remover campos de endereço duplicados nas respostas
2. Centralizar edição de NIRE e Inscrição Estadual no endpoint do dono
3. Ajustar threshold de validação de Razão Social (0.85 → 0.8)

---

## 🎯 Alterações por Endpoint

### 1️⃣ GET `/api/UserProfile/empresa`
**Status**: ✅ Estrutura de Response Alterada

#### Antes:
```json
{
  "id": "uuid",
  "nome": "Empresa LTDA",
  "cnpj": "12345678000199",
  "cnpjFormatado": "12.345.678/0001-99",
  "tipo": "Client",
  "modeloNegocio": "MainCompany",
  "rua": "Rua Exemplo",
  "numero": "100",
  "complemento": "Sala 5",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "01310000",
  "enderecoCompleto": "Rua Exemplo, 100, Sala 5 - Centro, São Paulo/SP, Brasil - CEP: 01310000",
  "endereco": {
    "rua": "Rua Exemplo",
    "numero": "100",
    "complemento": "Sala 5",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil",
    "cep": "01310000",
    "enderecoCompleto": "Rua Exemplo, 100, Sala 5 - Centro, São Paulo/SP, Brasil - CEP: 01310000"
  },
  "telefoneFixo": "1133334444",
  "telefoneCelular": "11987654321",
  "nire": "35123456789",
  "inscricaoEstadual": "123.456.789.012"
}
```

#### Agora:
```json
{
  "id": "uuid",
  "nome": "Empresa LTDA",
  "cnpj": "12345678000199",
  "cnpjFormatado": "12.345.678/0001-99",
  "tipo": "Client",
  "modeloNegocio": "MainCompany",
  "enderecoCompleto": "Rua Exemplo, 100, Sala 5 - Centro, São Paulo/SP, Brasil - CEP: 01310000",
  "endereco": {
    "rua": "Rua Exemplo",
    "numero": "100",
    "complemento": "Sala 5",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil",
    "cep": "01310000",
    "enderecoCompleto": "Rua Exemplo, 100, Sala 5 - Centro, São Paulo/SP, Brasil - CEP: 01310000"
  },
  "telefoneFixo": "1133334444",
  "telefoneCelular": "11987654321",
  "nire": "35123456789",
  "inscricaoEstadual": "123.456.789.012"
}
```

#### 🔧 Ações Necessárias:
- ❌ **REMOVER** acesso aos campos: `rua`, `numero`, `complemento`, `bairro`, `cidade`, `estado`, `pais`, `cep` do nível raiz
- ✅ **USAR** sempre `endereco.rua`, `endereco.numero`, etc. para acessar dados individuais
- ✅ **USAR** `enderecoCompleto` para exibir endereço formatado

#### Exemplo de Código (TypeScript/React):
```typescript
// ❌ ANTES (NÃO FUNCIONA MAIS)
const rua = empresa.rua;
const cidade = empresa.cidade;

// ✅ AGORA (CORRETO)
const rua = empresa.endereco?.rua;
const cidade = empresa.endereco?.cidade;
const enderecoFormatado = empresa.enderecoCompleto;
```

---

### 2️⃣ PUT `/api/UserProfile/empresa`
**Status**: ✅ Endpoint Mantido (sem alterações)

**Uso**: Atualização de dados da empresa pelo dono (nome, telefones, endereço, NIRE, inscrição estadual)

#### Request Body:
```json
{
  "nome": "Nova Razão Social LTDA",
  "telefoneCelular": "11987654321",
  "telefoneFixo": "1133334444",
  "rua": "Rua Nova",
  "numero": "200",
  "complemento": "Andar 3",
  "bairro": "Jardins",
  "cidade": "São Paulo",
  "estado": "SP",
  "pais": "Brasil",
  "cep": "01310000",
  "nire": "35123456789",
  "inscricaoEstadual": "123.456.789.012"
}
```

#### 🔧 Ações Necessárias:
- ✅ **Nenhuma alteração necessária** - endpoint continua funcionando normalmente
- ℹ️ Este endpoint **JÁ** permite editar NIRE e Inscrição Estadual

---

### 3️⃣ PUT `/api/Companies/empresa-pai`
**Status**: ✅ Novos Campos Adicionados

**Uso**: Atualização de dados críticos da empresa pai pelo dono (CNPJ, Razão Social, NIRE, Inscrição Estadual, Endereço)

#### Request Body Atualizado:
```json
{
  "razaoSocial": "Nova Razão Social LTDA",
  "cnpj": "12345678000199",
  "confirmarDivergenciaRazaoSocial": false,
  "nire": "35123456789",
  "inscricaoEstadual": "123.456.789.012",
  "enderecoRua": "Rua Atualizada",
  "enderecoNumero": "300",
  "enderecoComplemento": "Sala 10",
  "enderecoBairro": "Centro",
  "enderecoCidade": "São Paulo",
  "enderecoEstado": "SP",
  "enderecoPais": "Brasil",
  "enderecoCep": "01310000"
}
```

#### Response (em caso de divergência):
```json
{
  "sucesso": false,
  "mensagem": "A Razão Social informada (Nova Razão Social LTDA) difere da registrada na Receita Federal (Nova Razao Social LTDA). Confirme para prosseguir.",
  "empresa": null,
  "divergenciaRazaoSocial": true,
  "razaoSocialReceita": "Nova Razao Social LTDA",
  "razaoSocialInformada": "Nova Razão Social LTDA",
  "requerConfirmacao": true
}
```

#### 🔧 Ações Necessárias:
- ✅ **ADICIONAR** campos `nire` e `inscricaoEstadual` ao formulário de edição da empresa pai
- ⚠️ **IMPORTANTE**: Threshold de divergência foi ajustado de 85% para 80%
  - Isso significa que diferenças menores entre a Razão Social informada e a da Receita Federal agora **exigem confirmação**
  - Exemplo: "EMPRESA LTDA" vs "EMPRESA LIMITADA" agora pode pedir confirmação (antes passava direto)

#### Exemplo de Fluxo de Validação:
```typescript
const handleUpdateEmpresa = async (data: UpdateEmpresaPaiRequest) => {
  try {
    const response = await api.put('/api/Companies/empresa-pai', data);
    
    if (!response.data.sucesso && response.data.divergenciaRazaoSocial) {
      // Mostrar modal de confirmação
      const confirmar = await showConfirmDialog({
        title: 'Divergência de Razão Social',
        message: response.data.mensagem,
        details: `
          Receita Federal: ${response.data.razaoSocialReceita}
          Informado: ${response.data.razaoSocialInformada}
        `
      });
      
      if (confirmar) {
        // Reenviar com confirmação
        data.confirmarDivergenciaRazaoSocial = true;
        await api.put('/api/Companies/empresa-pai', data);
      }
    }
  } catch (error) {
    console.error('Erro ao atualizar empresa:', error);
  }
};
```

---

## 🎨 Sugestões de UX

### 1. Formulário de Edição da Empresa Pai
Adicionar campos NIRE e Inscrição Estadual:

```tsx
<FormField>
  <Label>NIRE (opcional)</Label>
  <Input 
    name="nire" 
    placeholder="35123456789"
    maxLength={20}
  />
  <Helper>Número de Identificação do Registro de Empresa</Helper>
</FormField>

<FormField>
  <Label>Inscrição Estadual (opcional)</Label>
  <Input 
    name="inscricaoEstadual" 
    placeholder="123.456.789.012"
    maxLength={50}
  />
</FormField>
```

### 2. Exibição de Endereço
Sempre usar o objeto `endereco` ou `enderecoCompleto`:

```tsx
// ✅ Opção 1: Exibir formatado
<div className="address-display">
  {empresa.enderecoCompleto || 'Endereço não cadastrado'}
</div>

// ✅ Opção 2: Exibir campos individuais
<div className="address-detailed">
  <p>{empresa.endereco?.rua}, {empresa.endereco?.numero}</p>
  {empresa.endereco?.complemento && <p>{empresa.endereco.complemento}</p>}
  <p>{empresa.endereco?.bairro} - {empresa.endereco?.cidade}/{empresa.endereco?.estado}</p>
  <p>CEP: {empresa.endereco?.cep}</p>
</div>
```

### 3. Modal de Divergência de Razão Social
Com o threshold mais sensível (80%), é importante ter um modal claro:

```tsx
<Modal isOpen={showDivergenciaModal}>
  <ModalHeader>⚠️ Divergência de Razão Social</ModalHeader>
  <ModalBody>
    <Alert variant="warning">
      A Razão Social informada difere da registrada na Receita Federal.
    </Alert>
    
    <ComparisonTable>
      <tr>
        <th>Receita Federal:</th>
        <td><strong>{razaoSocialReceita}</strong></td>
      </tr>
      <tr>
        <th>Informado:</th>
        <td><strong>{razaoSocialInformada}</strong></td>
      </tr>
    </ComparisonTable>
    
    <p>Deseja prosseguir com a atualização mesmo assim?</p>
  </ModalBody>
  <ModalFooter>
    <Button variant="secondary" onClick={handleCancel}>
      Cancelar
    </Button>
    <Button variant="primary" onClick={handleConfirm}>
      Confirmar e Prosseguir
    </Button>
  </ModalFooter>
</Modal>
```

---

## 📝 Checklist de Implementação

### GET `/api/UserProfile/empresa`
- [ ] Atualizar interface TypeScript do response
- [ ] Remover referências a campos de endereço no nível raiz
- [ ] Usar `empresa.endereco?.campo` para acessar dados individuais
- [ ] Usar `empresa.enderecoCompleto` para exibição formatada
- [ ] Testar componentes que exibem dados da empresa

### PUT `/api/UserProfile/empresa`
- [ ] Nenhuma alteração necessária (já funciona)

### PUT `/api/Companies/empresa-pai`
- [ ] Adicionar campos `nire` e `inscricaoEstadual` ao formulário
- [ ] Implementar/atualizar modal de confirmação de divergência
- [ ] Ajustar mensagens para refletir threshold de 80%
- [ ] Testar fluxo completo de validação de CNPJ + divergência
- [ ] Testar sincronização bidirecional de endereço (user + company)

---

## 🧪 Testes Recomendados

### Cenário 1: Buscar Dados da Empresa
```bash
GET /api/UserProfile/empresa
Authorization: Bearer {token}

# Verificar:
# - Campos de endereço duplicados NÃO aparecem no nível raiz
# - Objeto "endereco" está presente e preenchido
# - Campo "enderecoCompleto" está formatado corretamente
```

### Cenário 2: Atualizar NIRE e Inscrição Estadual
```bash
PUT /api/Companies/empresa-pai
Authorization: Bearer {token}
Content-Type: application/json

{
  "razaoSocial": "Empresa Teste LTDA",
  "cnpj": "12345678000199",
  "nire": "35123456789",
  "inscricaoEstadual": "123.456.789.012"
}

# Verificar:
# - Campos NIRE e Inscrição Estadual são salvos
# - Response retorna empresa com dados atualizados
```

### Cenário 3: Divergência de Razão Social (Threshold 80%)
```bash
PUT /api/Companies/empresa-pai
Authorization: Bearer {token}
Content-Type: application/json

{
  "razaoSocial": "EMPRESA LIMITADA",
  "cnpj": "12345678000199",
  "confirmarDivergenciaRazaoSocial": false
}

# Se a Receita retornar "EMPRESA LTDA":
# - Backend deve retornar sucesso=false
# - divergenciaRazaoSocial=true
# - requerConfirmacao=true
# - Frontend deve mostrar modal de confirmação

# Reenviar com confirmação:
{
  "razaoSocial": "EMPRESA LIMITADA",
  "cnpj": "12345678000199",
  "confirmarDivergenciaRazaoSocial": true
}

# - Backend deve aceitar e salvar
# - Response com sucesso=true
```

---

## 🔗 Endpoints Relacionados

| Método | Endpoint | Autenticação | Descrição |
|--------|----------|--------------|-----------|
| GET | `/api/UserProfile/empresa` | Bearer Token | Busca dados da empresa do usuário logado |
| PUT | `/api/UserProfile/empresa` | Bearer Token (Dono) | Atualiza dados gerais da empresa |
| GET | `/api/Companies/empresa-pai` | Bearer Token (Dono) | Busca dados completos da empresa pai |
| PUT | `/api/Companies/empresa-pai` | Bearer Token (Dono) | Atualiza dados críticos (CNPJ, Razão Social, NIRE, IE) |

---

## 📞 Suporte

Caso tenha dúvidas sobre a implementação:
1. Verifique a documentação Swagger: `https://aureapi.gabrielsanztech.com.br/swagger`
2. Consulte o arquivo `ALTERACOES_API_FRONTEND.md` para referência completa
3. Entre em contato com o time de backend

---

**Data da Alteração**: 25/11/2025  
**Versão da API**: 1.0  
**Breaking Changes**: Sim (estrutura do response do GET `/api/UserProfile/empresa`)
