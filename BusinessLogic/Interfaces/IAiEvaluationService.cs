using DataAccess.Entities.ChatAI;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IAiEvaluationService
    {
        Task<string> EvaluateCategoryAsync(Guid userId, Guid categoryId);
        Task<AiEvaluation?> GetEvaluationAsync(Guid userId, Guid categoryId);
    }
}
