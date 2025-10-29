using Aure.Application.DTOs.User;
using Aure.Application.DTOs.Company;
using Aure.Domain.Entities;

namespace Aure.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetUserProfileAsync(Guid userId, Guid? requestingUserId = null);
    Task<UserProfileResponse> UpdateFullProfileAsync(Guid userId, UpdateFullProfileRequest request);
    Task<NotificationPreferencesDTO> GetNotificationPreferencesAsync(Guid userId);
    Task<NotificationPreferencesDTO> UpdateNotificationPreferencesAsync(Guid userId, NotificationPreferencesDTO preferences);
    Task AcceptTermsAsync(Guid userId, AcceptTermsRequest request);
    Task<TermsVersionsResponse> GetTermsVersionsAsync(Guid userId);
    Task<UpdateCompanyPJResponse> UpdateCompanyPJAsync(Guid userId, UpdateCompanyPJRequest request);
}
