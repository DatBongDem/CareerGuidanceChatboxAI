using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IQuestionCategoryRepository : IGenericRepository<QuestionCategory, Guid>
    {
    }
}
