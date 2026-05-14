using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class EmailVerification
    {       
        public Guid Id { get; set; }       
        [Required]
        public string Email { get; set; }        
        [Required]
        public string Otp { get; set; }        
        public string? VerifyToken { get; set; }    
        public bool IsUsed { get; set; } = false;      
        public DateTime ExpiredAt { get; set; }     
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? TemporaryUserData { get; set; } 
    }
}
