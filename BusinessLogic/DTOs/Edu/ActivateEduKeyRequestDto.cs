using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Edu
{
    public class ActivateEduKeyRequestDto
    {
        [Required(ErrorMessage = "Mã kích hoạt không được để trống")]
        public string ActivationKey { get; set; } = string.Empty;
    }
}
