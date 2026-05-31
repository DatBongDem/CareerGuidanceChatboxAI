using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IQuestionOptionRepository : IGenericRepository<QuestionOption, Guid>
    {
    }
}
