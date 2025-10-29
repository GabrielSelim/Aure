using Aure.Domain.Enums;

namespace Aure.Application.DTOs.User;

public class EmployeeListItemResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public string? TelefoneCelular { get; set; }
    public string? EmpresaPJ { get; set; }
}

public class EmployeeListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public UserRole? Role { get; set; }
    public string? Cargo { get; set; }
    public string? Status { get; set; }
    public string? Busca { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
