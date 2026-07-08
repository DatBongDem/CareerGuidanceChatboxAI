using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class OperationalExpenseRepository : GenericRepository<OperationalExpense, Guid>, IOperationalExpenseRepository
    {
        public OperationalExpenseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
