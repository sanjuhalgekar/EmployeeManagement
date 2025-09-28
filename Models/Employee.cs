
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [NotMapped]
        public string encryptedId    { get; set; }

        [Required]
        [MaxLength(50,ErrorMessage = "length Should not be more than 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",ErrorMessage = "Invalid Email Id")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Department? Department { get; set; }

        [DisplayName("Employee Photo")]
        public string? PhotoPath { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid gender.")]
        public Gender? Gender { get; set; }
    }
}
