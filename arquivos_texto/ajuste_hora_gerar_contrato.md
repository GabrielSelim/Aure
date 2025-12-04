# Erro 500 ao gerar contrato com funcionário PJ - Campo "profissão" não está sendo preenchido

## Problema

Estou tendo um erro 500 ao tentar gerar contratos selecionando um funcionário PJ cadastrado. O problema ocorre no endpoint:

```
POST /api/ContractTemplateConfig/gerar-contrato
```

## Payload enviado pelo frontend

```json
{
  "nomeConfig": "software",
  "funcionarioPJId": "333b9906-9817-49e5-83bf-8de502f90747",
  "dadosContratadoManual": null,
  "valorMensal": 5000,
  "prazoVigenciaMeses": 12,
  "diaVencimentoNF": 30,
  "diaPagamento": 5,
  "dataInicioVigencia": "2025-12-04"
}
```

## Resposta do backend

- Status: `500 Internal Server Error`
- Body: vazio (content-length: 0)
- Sem mensagem de erro

## Análise do problema

Quando enviamos `funcionarioPJId` (funcionário PJ selecionado), o `dadosContratadoManual` é `null` porque esperamos que o backend busque automaticamente os dados do funcionário pelo ID.

Porém, o backend está retornando erro 500, provavelmente porque:
1. Está tentando acessar `dadosContratadoManual.profissao` mas o objeto é `null`
2. Ou não está buscando o campo `cargo` do funcionário PJ no banco de dados

## Ajuste necessário

Quando `funcionarioPJId` for fornecido (e `dadosContratadoManual` for `null`), o backend deve:

1. Buscar todos os dados do funcionário PJ no banco de dados (tabela `Users`)
2. Preencher automaticamente o objeto interno com os dados, **incluindo o campo `cargo` como `profissao`**
3. Usar esses dados para gerar o contrato

## Mapeamento de campos

| Campo no banco (User) | Campo esperado no contrato (DadosContratadoManual) |
|----------------------|---------------------------------------------------|
| nome | nomeCompleto |
| razaoSocialPJ | razaoSocial |
| cnpjPJ | cnpj |
| cpf | cpf |
| rg | rg |
| dataNascimento | dataNascimento |
| nacionalidade | nacionalidade |
| estadoCivil | estadoCivil |
| **cargo** | **profissao** ← **Este campo é obrigatório** |
| email | email |
| telefoneCelular | telefoneCelular |
| telefoneFixo | telefoneFixo |
| rua | rua |
| numero | numero |
| complemento | complemento |
| bairro | bairro |
| cidade | cidade |
| estado | estado |
| cep | cep |

## Validação adicional sugerida

Antes de gerar o contrato, validar se o funcionário PJ possui o campo `cargo` preenchido. Se não tiver, retornar:

```json
{
  "erro": "O funcionário PJ não possui o campo 'Profissão/Cargo' preenchido. Complete o cadastro antes de gerar o contrato."
}
```

## Observação importante

O campo `cargo` foi adicionado recentemente ao sistema. Certifique-se de que:
- O endpoint `PUT /api/Users/{id}/cargo` está funcionando corretamente
- O campo está sendo salvo no banco de dados
- O campo é retornado ao buscar o funcionário PJ

## Para testar localmente

1. Adicione logs para ver se `funcionarioPJId` está sendo recebido
2. Verifique se está buscando os dados do funcionário no banco
3. Confirme se o campo `cargo` está sendo populado
4. Trate a exceção e retorne uma mensagem de erro descritiva (não apenas 500 vazio)

## Request completo esperado pelo Swagger

```json
{
  "nomeConfig": "string",
  "funcionarioPJId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dadosContratadoManual": {
    "nomeCompleto": "string",
    "razaoSocial": "string",
    "cnpj": "stringstringst",
    "cpf": "stringstrin",
    "rg": "string",
    "dataNascimento": "2025-12-04T15:00:12.645Z",
    "nacionalidade": "string",
    "estadoCivil": "string",
    "profissao": "string",
    "email": "user@example.com",
    "telefoneCelular": "stringstri",
    "telefoneFixo": "stringstri",
    "rua": "string",
    "numero": "string",
    "complemento": "string",
    "bairro": "string",
    "cidade": "string",
    "estado": "st",
    "pais": "string",
    "cep": "stringst"
  },
  "valorMensal": 0.01,
  "prazoVigenciaMeses": 120,
  "diaVencimentoNF": 31,
  "diaPagamento": 31,
  "dataInicioVigencia": "2025-12-04T15:00:12.646Z"
}
```

**Nota:** Quando `funcionarioPJId` é fornecido, `dadosContratadoManual` deve ser preenchido automaticamente pelo backend (não pelo frontend).
