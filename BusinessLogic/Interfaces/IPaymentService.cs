using BusinessLogic.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponseDto> CreatePaymentAsync(Guid userId, Guid planId);

        Task ConfirmPaymentAsync(string transactionCode);
    }
}
