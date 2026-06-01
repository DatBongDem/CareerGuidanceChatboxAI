using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class QuestionOptionRepository : GenericRepository<QuestionOption, Guid>, IQuestionOptionRepository
    {
        public QuestionOptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
