using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class RefreshToken
    {
        public Guid RefreshTokenId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User? User { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; }

        public string? IpAddress { get; set; }
    }
}