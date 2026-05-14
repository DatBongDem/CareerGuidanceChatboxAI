using DataAccess.Entities;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IPlanRepository : IGenericRepository<Plan, Guid>
    {
        Task<Plan?> GetPlanByNameAsync(string planName);
    }
}
