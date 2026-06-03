using DataAccess.DataContext;
using DataAccess.Repositories;

public class RecommendationRepository
    : GenericRepository<Recommendation, Guid>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}
