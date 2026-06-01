using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class QuestionCategoryRepository : GenericRepository<QuestionCategory, Guid>, IQuestionCategoryRepository
    {
        public QuestionCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
