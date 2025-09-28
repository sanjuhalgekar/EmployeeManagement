
using EmployeeManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class EditEmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Full Name")]
        [MaxLength(50, ErrorMessage = "length Should not be more than 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter Office Mail Id")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email Id")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Select Department")]
        public Department? Department { get; set; }

        //[DisplayName("Employee Photo")]
        public IFormFile? Photo { get; set; }

        [Required(ErrorMessage = "Select Gender")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid gender.")]
        public Gender? Gender { get; set; }

        public string? PhotoPath { get; set; }
    }
}
