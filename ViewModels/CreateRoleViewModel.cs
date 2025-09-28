
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Enter Role Name")]
        [DisplayName("Role Name")]
        public string roleName { get; set; } = string.Empty;
    }
}
