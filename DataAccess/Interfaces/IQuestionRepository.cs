using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question, Guid>
    {
    }
}
