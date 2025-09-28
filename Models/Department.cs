
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public enum Department
    {
        [Display(Name = "-- Please Select --")]
        NotSelected = 0,
        None,
        IT,
        Account,
        Payroll,
        HR
    }
}
