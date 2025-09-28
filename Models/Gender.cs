
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public enum Gender
    {
        [Display(Name = "-- Please Select --")]
        NotSelected = 0,

        [Display(Name = "Male")]
        Male = 1,

        [Display(Name = "Female")]
        Female = 2,

        [Display(Name = "Transgender")]
        Transgender = 3
    }
}
