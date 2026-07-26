using BusinessLogic.DTOs.Statistics;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class WebStatsService : IWebStatsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WebStatsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DailyWebVisitDto>> GetDailyWebVisitsAsync()
        {
            var visits = await _unitOfWork.DailyWebVisitRepository.GetAllAsync();
            return visits
                .OrderBy(v => v.Date)
                .Select(v => new DailyWebVisitDto
                {
                    Date = v.Date,
                    VisitCount = v.VisitCount
                });
        }

        public async Task<int> IncrementDailyWebVisitsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var existingList = await _unitOfWork.DailyWebVisitRepository.GetAsync(filter: v => v.Date == today);
            var existing = existingList.FirstOrDefault();

            if (existing != null)
            {
                existing.VisitCount += 1;
                await _unitOfWork.DailyWebVisitRepository.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();
                return existing.VisitCount;
            }
            else
            {
                var newVisit = new DailyWebVisit
                {
                    Id = Guid.NewGuid(),
                    Date = today,
                    VisitCount = 1
                };
                await _unitOfWork.DailyWebVisitRepository.AddAsync(newVisit);
                await _unitOfWork.SaveAsync();
                return 1;
            }
        }

        public async Task<IEnumerable<DailyUserVisitDto>> GetDailyUserVisitsAsync()
        {
            var userVisits = await _unitOfWork.DailyUserVisitRepository.GetAllAsync();
            return userVisits
                .GroupBy(uv => uv.Date)
                .Select(g => new DailyUserVisitDto
                {
                    Date = g.Key,
                    UserCount = g.Count()
                })
                .OrderBy(dto => dto.Date);
        }

        public async Task<bool> RecordDailyUserVisitAsync(Guid userId)
        {
            // Verify user exists first to prevent orphaned FK records
            var userExists = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (userExists == null)
            {
                throw new ApplicationException("User not found.");
            }

            var today = DateTime.UtcNow.Date;
            var existingList = await _unitOfWork.DailyUserVisitRepository.GetAsync(
                filter: uv => uv.Date == today && uv.UserId == userId
            );
            var existing = existingList.FirstOrDefault();

            if (existing != null)
            {
                return true; // Already recorded today
            }

            try
            {
                var newRecord = new DailyUserVisit
                {
                    Id = Guid.NewGuid(),
                    Date = today,
                    UserId = userId
                };
                await _unitOfWork.DailyUserVisitRepository.AddAsync(newRecord);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception)
            {
                // Unique constraint index bypass (if concurrent request finished first)
                return true;
            }

            return true;
        }
    }
}
