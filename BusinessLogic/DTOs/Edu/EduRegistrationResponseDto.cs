using System;

namespace BusinessLogic.DTOs.Edu
{
    public class EduRegistrationResponseDto
    {
        public Guid Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }
        public string? TransactionCode { get; set; }
        public string? Key { get; set; }
    }
}
