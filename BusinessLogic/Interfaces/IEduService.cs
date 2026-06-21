using BusinessLogic.DTOs.Edu;
using BusinessLogic.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Interfaces
{
    public interface IEduService
    {
        Task<EduRegistrationResponseDto> RegisterEduAsync(CreateEduRegistrationDto dto);
        Task<IEnumerable<EduRegistrationResponseDto>> GetEduRegistrationsAsync();
        Task<CreatePaymentResponseDto> CreateEduPaymentLinkAsync(Guid registrationId);
        Task ConfirmEduPaymentAsync(string transactionCode);
        Task CancelEduPaymentAsync(string transactionCode);
        Task<IEnumerable<EduActivationKeyResponseDto>> ImportEduKeysAsync(Guid registrationId, IFormFile file);
        Task ActivateEduKeyAsync(Guid userId, string activationKey);
    }
}
