
namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employees;

        public MockEmployeeRepository()
        {
            _employees = new List<Employee>()
            {
                new Employee(){Id = 1, Name = "Sanju Halgekar", Email = "sanjuhalgekar5@gmail.com", Department = Department.IT},
                new Employee(){Id = 2, Name = "Akash Patil", Email = "akashpatil5@gmail.com", Department = Department.HR},
                new Employee(){Id = 3, Name = "Amit Dewale", Email = "amitd@gmail.com", Department =  Department.IT},
                new Employee(){Id = 4, Name = "Sunil Patil", Email = "sunilpatil@gmail.com", Department = Department.Account},
                new Employee(){Id = 5, Name = "Pratik Dewale", Email = "pratikd@gmail.com", Department =  Department.IT},
                new Employee(){Id = 6, Name = "Manoj Patil", Email = "manojpatil@gmail.com", Department = Department.Account},
                new Employee(){Id = 7, Name = "Sanjana Patil", Email = "sanjanap@gmail.com", Department = Department.IT},
                new Employee(){Id = 8, Name = "Dinesh Prajapati", Email = "dineshp@gmail.com", Department = Department.HR},
                new Employee(){Id = 9, Name = "Mayur Dhokane", Email = "mayurd@gmail.com", Department =  Department.IT},
                new Employee(){Id = 10, Name = "Santosh Patil", Email = "santoshpatil@gmail.com", Department = Department.Account},
                new Employee(){Id = 11, Name = "Pravin Wankhede", Email = "pravinw@gmail.com", Department =  Department.Payroll},
                new Employee(){Id = 12, Name = "Nehal Patil", Email = "nehalpatil@gmail.com", Department = Department.Payroll}
            };
        }

        //Add new employee
        public Employee AddEmployee(Employee employee)
        {
            employee.Id = _employees.Max(e => e.Id) + 1;
            _employees.Add(employee);
            return employee;
        }        

        //Get all employees
        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employees;
        }

        //Get emplyoee by id
        public Employee? GetEmployeeById(int? id)
        {
            return _employees.FirstOrDefault(e => e.Id == id);
        }

        //Update employee details
        public Employee UpdateEmployee(Employee employee)
        {
            var employeeDtls = _employees.FirstOrDefault(e => e.Id == employee.Id);

            if (employeeDtls != null)
            {
                employeeDtls.Name = employee.Name;
                employeeDtls.Email = employee.Email;
                employeeDtls.Department = employee.Department;
            }
            return employee;
        }

        //Delete employee details
        public IEnumerable<Employee> DeleteEmployee(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                _employees.Remove(employee);
            }
            return _employees;
        }

        public Employee? DelEmployee(int? id)
        {
            Employee? employee = _employees.FirstOrDefault(x => x.Id == id);

            if (employee != null)
            {
                _employees.Remove(employee);
            }

            return employee; // Now returning nullable type is valid
        }

    }
}
