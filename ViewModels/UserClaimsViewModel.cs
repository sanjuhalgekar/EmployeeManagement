
namespace EmployeeManagement.ViewModels
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel() { 
            Claims = new List<UserClaim>();
        }

        public string UserId { get; set; } = string.Empty;

        public List<UserClaim> Claims { get; set; }
    }
}
