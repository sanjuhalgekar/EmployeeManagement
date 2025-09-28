
namespace EmployeeManagement.Models
{
    public interface IEmployeeRepository
    {
        Employee? GetEmployeeById(int? id);

        IEnumerable<Employee> GetAllEmployees();

        Employee AddEmployee(Employee employee);

        Employee UpdateEmployee(Employee employee);

        IEnumerable<Employee> DeleteEmployee(int id);

        Employee? DelEmployee(int? id);
    }
}
