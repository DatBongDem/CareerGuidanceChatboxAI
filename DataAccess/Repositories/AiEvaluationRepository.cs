using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class AiEvaluationRepository : GenericRepository<AiEvaluation, Guid>, IAiEvaluationRepository
    {
        public AiEvaluationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
