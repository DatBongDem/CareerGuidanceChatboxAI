using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class DailyWebVisitRepository : GenericRepository<DailyWebVisit, Guid>, IDailyWebVisitRepository
    {
        public DailyWebVisitRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
