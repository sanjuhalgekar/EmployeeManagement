
namespace EmployeeManagement.ViewModels
{
    public class EditRoleViewModel : CreateRoleViewModel
    {

        public EditRoleViewModel()
        {
            users = new List<string>();
        }
        public string Id { get; set; } = string.Empty;

        public List<string> users { get; set; }
    }
}
