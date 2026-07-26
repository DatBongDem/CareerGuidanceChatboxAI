using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IDailyUserVisitRepository : IGenericRepository<DailyUserVisit, Guid>
    {
    }
}
