using DataAccess.Entities;
using System;

namespace DataAccess.Interfaces
{
    public interface IFeedbackResponseRepository : IGenericRepository<FeedbackResponse, Guid>
    {
    }
}
