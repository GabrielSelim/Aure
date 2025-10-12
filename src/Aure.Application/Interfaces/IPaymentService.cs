using Aure.Application.DTOs.Payment;
using Aure.Domain.Common;
using Aure.Domain.Enums;

namespace Aure.Application.Interfaces;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> GetByIdAsync(Guid id);
    Task<Result<PaymentListResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<Result<IEnumerable<PaymentResponse>>> GetByContractIdAsync(Guid contractId);
    Task<Result<IEnumerable<PaymentResponse>>> GetByStatusAsync(PaymentStatus status);
    Task<Result<PaymentResponse>> CreateAsync(CreatePaymentRequest request);
    Task<Result<PaymentResponse>> UpdateStatusAsync(Guid id, UpdatePaymentStatusRequest request);
    Task<Result> ProcessPaymentAsync(Guid id);
    Task<Result> CancelPaymentAsync(Guid id);
    Task<Result<PaymentReportResponse>> GetPaymentReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Result> DeleteAsync(Guid id);
}