using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IFeedbackAnswerRepository : IGenericRepository<FeedbackAnswer, Guid>
    {
    }
}
