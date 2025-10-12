using Aure.Domain.Enums;

namespace Aure.Application.DTOs.Payment;

public record CreatePaymentRequest(
    Guid ContractId,
    decimal Amount,
    PaymentMethod Method
);

public record UpdatePaymentStatusRequest(
    PaymentStatus Status
);

public record PaymentResponse(
    Guid Id,
    Guid ContractId,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime? PaymentDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PaymentListResponse(
    IEnumerable<PaymentResponse> Payments,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record PaymentReportResponse(
    decimal TotalAmount,
    int TotalPayments,
    Dictionary<PaymentStatus, int> StatusCount,
    Dictionary<PaymentMethod, decimal> AmountByMethod
);