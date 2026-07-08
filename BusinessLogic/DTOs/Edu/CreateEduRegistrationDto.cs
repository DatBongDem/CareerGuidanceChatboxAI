using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Edu
{
    public class CreateEduRegistrationDto
    {
        [Required(ErrorMessage = "Tên trường không được để trống")]
        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Người liên hệ không được để trống")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số học sinh không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số học sinh phải lớn hơn 0")]
        public int StudentCount { get; set; }

        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plan ID không được để trống")]
        public Guid PlanId { get; set; }
    }
}
