using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IAiEvaluationRepository : IGenericRepository<AiEvaluation, Guid>
    {
    }
}
