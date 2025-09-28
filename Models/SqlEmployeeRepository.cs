
using System;

namespace EmployeeManagement.Models
{
    public class SqlEmployeeRepository : IEmployeeRepository
    {
        private AppDBContext _appDBContext;

        public SqlEmployeeRepository(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        public Employee AddEmployee(Employee employee)
        {
            _appDBContext.Employees.Add(employee);
            _appDBContext.SaveChanges();
            return employee;
        }

        public Employee? DelEmployee(int? id)
        {
            Employee? employee = _appDBContext.Employees.Find(id);

            if (employee != null) 
            {
                _appDBContext.Remove(employee);
                _appDBContext.SaveChanges();
            }

            return employee;
        }

        public IEnumerable<Employee> DeleteEmployee(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _appDBContext.Employees;
        }

        public Employee? GetEmployeeById(int? id)
        {
            return _appDBContext.Employees.Find(id);
        }

        public Employee UpdateEmployee(Employee employee)
        {
            var emp = _appDBContext.Employees.Attach(employee);
            emp.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _appDBContext.SaveChanges();
            return employee;
        }
    }
}
