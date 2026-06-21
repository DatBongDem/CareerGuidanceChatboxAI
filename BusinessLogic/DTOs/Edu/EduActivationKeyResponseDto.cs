using System;

namespace BusinessLogic.DTOs.Edu
{
    public class EduActivationKeyResponseDto
    {
        public Guid Id { get; set; }
        public Guid RegistrationId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ActivationKey { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public Guid? UsedByUserId { get; set; }
        public DateTime? ActivatedAt { get; set; }
    }
}
