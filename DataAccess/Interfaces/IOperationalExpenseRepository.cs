using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IOperationalExpenseRepository : IGenericRepository<OperationalExpense, Guid>
    {
    }
}
