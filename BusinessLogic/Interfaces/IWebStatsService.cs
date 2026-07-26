using BusinessLogic.DTOs.Statistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IWebStatsService
    {
        Task<IEnumerable<DailyWebVisitDto>> GetDailyWebVisitsAsync();
        Task<int> IncrementDailyWebVisitsAsync();
        
        Task<IEnumerable<DailyUserVisitDto>> GetDailyUserVisitsAsync();
        Task<bool> RecordDailyUserVisitAsync(Guid userId);
    }
}
