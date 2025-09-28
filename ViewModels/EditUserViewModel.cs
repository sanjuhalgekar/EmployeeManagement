
namespace EmployeeManagement.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            UserRoles = new List<string>();
            Claims = new List<string>();
        }

        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string NormalizedUserName { get; set; }

        public IList<string> UserRoles { get; set; }

        public List<string> Claims { get; set; }
    }
}
