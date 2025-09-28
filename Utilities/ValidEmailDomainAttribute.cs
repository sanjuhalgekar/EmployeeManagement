
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Utilities
{
    //Here we create custom valiadtion calss which is inherited from ValidationAttribute
    public class ValidEmailDomainAttribute : ValidationAttribute //This is abstract class
    {
        private readonly string _allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this._allowedDomain = allowedDomain;
        }

        //here IsValid() method overide using custom logic
        public override bool IsValid(object? value)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            var email = value.ToString();

            if (!email.Contains('@'))
                return false;

            string[] parts = email.Split('@');

            if (parts.Length != 2)
                return false;

            return string.Equals(parts[1], _allowedDomain, StringComparison.OrdinalIgnoreCase);
        }
    }
}
