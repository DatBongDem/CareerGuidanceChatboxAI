using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class EduActivationKey
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid RegistrationId { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string ActivationKey { get; set; } = string.Empty;
        
        public bool IsUsed { get; set; } = false;
        
        public Guid? UsedByUserId { get; set; }
        
        public DateTime? ActivatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("RegistrationId")]
        public EduRegistration? Registration { get; set; }
        
        [ForeignKey("UsedByUserId")]
        public User? UsedByUser { get; set; }
    }
}
