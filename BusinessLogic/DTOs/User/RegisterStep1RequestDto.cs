using DataAccess.Shares;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class RegisterStep1RequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Address { get; set; }

        public string Gender { get; set; } = StatusEnum.Other;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
