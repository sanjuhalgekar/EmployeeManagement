
using System.Security.Claims;

namespace EmployeeManagement.Models
{
    public static class ClaimStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("Create User", "Create User"),
            new Claim("Edit User", "Edit User"),
            new Claim("Delete User", "Delete User"),
            new Claim("User View", "User View"),
            new Claim("Create Role", "Create Role"),
            new Claim("Edit Role", "Edit Role"),
            new Claim("Delete Role", "Delete Role")
        };
    }
}
