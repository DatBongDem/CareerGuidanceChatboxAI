using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class FeedbackResponseRepository : GenericRepository<FeedbackResponse, Guid>, IFeedbackResponseRepository
    {
        public FeedbackResponseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
