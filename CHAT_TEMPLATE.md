# 📋 Template para Novos Chats do GitHub Copilot

## 🎯 Copie e cole este contexto em novos chats:

```
Estou trabalhando no projeto Aure, um sistema fintech backend em C# .NET 8 com DDD e PostgreSQL.

REGRAS OBRIGATÓRIAS DE CÓDIGO:
❌ NUNCA gerar: Console.WriteLine(), comentários desnecessários, TODO/FIXME
✅ SEMPRE gerar: Logging estruturado com Serilog, nomes autoexplicativos, async/await

Stack: .NET 8, Entity Framework Core, PostgreSQL, Clean Architecture, Docker
Padrões: DDD, Repository Pattern, CQRS, Result Pattern

Exemplo de código limpo:
public async Task<Result<User>> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id);
    
    if (user == null)
    {
        _logger.LogWarning("User not found with ID {UserId}", id);
        return Result<User>.Failure("User not found");
    }
    
    return Result<User>.Success(user);
}

Siga as diretrizes em .copilot-instructions.md
```

## 🚀 Como usar:
1. **Novo chat** → Cole o template acima
2. **Mencione o arquivo** → "Siga .copilot-instructions.md"
3. **Reforce se necessário** → "Sem Console.WriteLine, sem comentários"

## ⚡ Template curto para uso rápido:
```
Projeto Aure: C# .NET 8 fintech + PostgreSQL + DDD
Regras: Sem Console.WriteLine, sem comentários desnecessários, use Serilog
Stack: EF Core, FluentValidation, AutoMapper, Clean Architecture
```