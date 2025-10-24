# Configuração do Back-end no Azure DevOps

## 🎯 Objetivo
Configurar o repositório do back-end para sincronizar com o Azure DevOps usando a URL SSH: `git@ssh.dev.azure.com:v3/AureControll/Aure/Aure%20BackEnd`

## 📋 Passos para execução

### 1. Verificar status atual do repositório
```powershell
# Verificar se é um repositório git
git status

# Ver remotes existentes
git remote -v

# Ver qual branch está ativa
git branch
```

### 2. Configurar remote do Azure DevOps

**Opção A: HTTPS (Recomendada - mais fácil)**
```powershell
git remote add azure "https://dev.azure.com/AureControll/Aure/_git/Aure%20BackEnd"
```

**Opção B: SSH (se SSH estiver configurado)**
```powershell
git remote add azure "git@ssh.dev.azure.com:v3/AureControll/Aure/Aure%20BackEnd"
```

### 3. Verificar configuração
```powershell
git remote -v
```

**Resultado esperado:**
```
azure   https://dev.azure.com/AureControll/Aure/_git/Aure%20BackEnd (fetch)
azure   https://dev.azure.com/AureControll/Aure/_git/Aure%20BackEnd (push)
```

### 4. Fazer commit das alterações pendentes (se houver)
```powershell
# Ver arquivos modificados
git status

# Adicionar todos os arquivos
git add .

# Fazer commit
git commit -m "Preparando backend para sincronização com Azure DevOps"
```

### 5. Push inicial para Azure DevOps
```powershell
# Push da branch principal
git push azure master

# Ou se estiver usando 'main':
git push azure main
```

### 6. Configurar para pushes futuros simultâneos (opcional)
```powershell
# Para sempre fazer push nos dois lugares:
git remote set-url --add --push azure "https://dev.azure.com/AureControll/Aure/_git/Aure%20BackEnd"
git remote set-url --add --push azure "URL_DO_GITHUB_SE_TIVER"
```

## ⚠️ Troubleshooting

### Se der erro "repository not found":
1. Verificar se o repositório "Aure BackEnd" existe no Azure DevOps
2. Acessar: https://dev.azure.com/AureControll/Aure
3. Ir em "Repos" e verificar se existe "Aure BackEnd"
4. Se não existir, criar o repositório com esse nome exato

### Se der erro de autenticação:
1. Usar HTTPS em vez de SSH
2. Sistema vai pedir credenciais do Azure DevOps
3. Usar token de acesso pessoal se necessário

### Se estiver na branch errada:
```powershell
# Criar e trocar para branch master
git checkout -b master

# Ou trocar para master existente
git checkout master
```

## 🔄 Workflow para commits futuros

```powershell
# 1. Fazer alterações no código
# 2. Verificar status
git status

# 3. Adicionar alterações
git add .

# 4. Commit com mensagem descritiva
git commit -m "Descrição das alterações"

# 5. Push para Azure DevOps
git push azure master

# 6. Push para GitHub (se configurado)
git push origin master
```

## ✅ Verificação final

Após executar os passos, verificar:
1. `git remote -v` mostra o remote 'azure' configurado
2. Push foi realizado com sucesso
3. Repositório aparece no Azure DevOps: https://dev.azure.com/AureControll/Aure/_git/Aure%20BackEnd

## 📞 Em caso de problemas

Se algo não funcionar:
1. Verificar se o repositório existe no Azure DevOps
2. Verificar permissões de acesso
3. Tentar HTTPS em vez de SSH
4. Verificar se não há conflitos de branch (master vs main)

---

**Nota**: Execute estes comandos no diretório raiz do seu projeto back-end.