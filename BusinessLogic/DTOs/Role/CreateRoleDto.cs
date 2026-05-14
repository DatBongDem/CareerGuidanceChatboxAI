using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Role
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}