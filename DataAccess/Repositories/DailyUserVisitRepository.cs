using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class DailyUserVisitRepository : GenericRepository<DailyUserVisit, Guid>, IDailyUserVisitRepository
    {
        public DailyUserVisitRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
