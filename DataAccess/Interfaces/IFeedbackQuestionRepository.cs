using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IFeedbackQuestionRepository : IGenericRepository<FeedbackQuestion, Guid>
    {
    }
}
