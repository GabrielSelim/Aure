# Configuração do Projeto Aure

## 📁 Estrutura de arquivos importantes:

```
Aure/
├── .copilot-instructions.md    # ← Instruções para GitHub Copilot
├── .editorconfig              # ← Formatação automática
├── .vscode/settings.json      # ← Configurações VS Code
├── PROMPT_BACKEND_AURE.md     # ← Especificações completas
├── CHAT_TEMPLATE.md           # ← Template para novos chats
└── README.md                  # ← Documentação básica
```

## 🎯 Como usar com GitHub Copilot:

### Para novos chats:
1. Copie o conteúdo de `CHAT_TEMPLATE.md`
2. Cole no início do novo chat
3. A IA seguirá as diretrizes automaticamente

### Para chats existentes:
- Mencione: "Siga as diretrizes em .copilot-instructions.md"
- Reforce: "Sem Console.WriteLine, sem comentários desnecessários"

### Se a IA não seguir as regras:
- Cole novamente o template
- Seja específico: "Use _logger.LogInformation ao invés de Console.WriteLine"
- Mencione: "Código deve ser autoexplicativo, sem comentários"

## ✅ Checklist de validação:
- [ ] Zero Console.WriteLine()
- [ ] Zero comentários desnecessários  
- [ ] Nomes autoexplicativos em inglês
- [ ] Async/await para I/O operations
- [ ] Logging estruturado com Serilog
- [ ] Exception handling adequado

## 🚀 Status dos arquivos:
- ✅ `.copilot-instructions.md` - Ativo
- ✅ `.editorconfig` - Ativo  
- ✅ `.vscode/settings.json` - Ativo
- ✅ `CHAT_TEMPLATE.md` - Criado
- ✅ `README.md` - Criado