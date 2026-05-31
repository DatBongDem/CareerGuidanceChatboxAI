using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class QuestionRepository : GenericRepository<Question, Guid>, IQuestionRepository
    {
        public QuestionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
