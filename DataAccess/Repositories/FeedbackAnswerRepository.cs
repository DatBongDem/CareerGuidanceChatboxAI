using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class FeedbackAnswerRepository : GenericRepository<FeedbackAnswer, Guid>, IFeedbackAnswerRepository
    {
        public FeedbackAnswerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
