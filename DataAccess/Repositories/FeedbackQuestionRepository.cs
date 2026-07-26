using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class FeedbackQuestionRepository : GenericRepository<FeedbackQuestion, Guid>, IFeedbackQuestionRepository
    {
        public FeedbackQuestionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
