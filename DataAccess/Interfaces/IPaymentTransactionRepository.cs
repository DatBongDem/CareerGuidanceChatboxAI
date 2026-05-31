using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IPaymentTransactionRepository
        : IGenericRepository<PaymentTransaction, Guid>
    {
        Task<PaymentTransaction?>
            GetByTransactionCodeAsync(string code);

        Task<IEnumerable<PaymentTransaction>>
            GetByUserIdAsync(Guid userId);
    }
}
