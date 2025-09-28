
using EmployeeManagement.Models;             // Import models like Employee and IEmployeeRepository
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;              // Import MVC framework classes

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        // Dependency injected repository to access employee data
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _dataProtector;

        // Constructor to receive repository instance via Dependency Injection
        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webHostEnvironment,IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurpose)
        {
            _employeeRepository = employeeRepository;  // Assign injected repo to private field
            _webHostEnvironment = webHostEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(dataProtectionPurpose.employeeIdRouteValue);
        }

        // GET: /Home/Index
        // Returns a view displaying a list of all employees
        public ViewResult Index()
        {
            try
            {
                // Fetch all employees from repository
                var model = _employeeRepository.GetAllEmployees().Select
                    (
                        e => 
                        {
                        e.encryptedId = _dataProtector.Protect(e.Id.ToString());
                        return e;
                        }
                    );

                if (model == null)
                    return View("~/Views/Home/Error.cshtml");

                // Pass employee list to the view for rendering
                return View(model);
            }
            catch
            {
                // Optional: log the exception
                // _logger.LogError(ex, "Error occurred while editing employee.");

                // Show a user-friendly error page
                ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                return View("~/Views/Home/Error.cshtml");
            }            
        }

        // GET: /Home/EmployeeDetailsById/{id}
        // Returns a view showing details of a single employee identified by id
        [Authorize(Policy = "CanViewUserPolicy")]
        public ViewResult EmployeeDetailsById(string id)
        {
            try 
            {
                int employeeId = int.Parse(_dataProtector.Unprotect(id));

                Employee employee = _employeeRepository.GetEmployeeById(employeeId);

                if (employee == null)
                    return View("~/Views/Home/Error.cshtml");

                // Create a ViewModel containing employee data and page title
                HomeDetailsViewModel viewModel = new HomeDetailsViewModel()
                {
                    employee = employee, // Fetch employee by id
                    PageTitle = "Employee Details"                      // Set page title
                };

                // Pass ViewModel to the view
                return View(viewModel);
            }
            catch {
                // Optional: log the exception
                // _logger.LogError(ex, "Error occurred while editing employee.");

                // Show a user-friendly error page
                ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                return View("~/Views/Home/Error.cshtml");
            }            
        }

        // GET: /Home/Create
        // Returns a view containing a form to add a new employee
        [Authorize(Policy = "CanCreateUserPolicy")]
        public ViewResult Create()
        {
            try
            {
                return View(); // Just return the empty form view
            }
            catch
            {
                // Optional: log the exception
                // _logger.LogError(ex, "Error occurred while editing employee.");

                // Show a user-friendly error page
                ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                return View("~/Views/Home/Error.cshtml");
            }           
        }

        // POST: /Home/Create
        // Handles form submission to add a new employee
        [HttpPost]
        [Authorize(Policy = "CanCreateUserPolicy")]
        public IActionResult Create(CreateNewEmployeeViewModel employee)
        {
            // Check if posted model passes validation rules
            if (ModelState.IsValid)
            {
                string? uniqueFileName = null;

                if (employee.PhotoPath != null) {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + employee.PhotoPath.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        employee.PhotoPath.CopyTo(fileStream);
                    }                    
                }

                // Add employee to repository and get the newly created employee object
                Employee newEmployee = new Employee { 
                    Name = employee.Name,
                    Email = employee.Email,
                    Department = employee.Department,
                    PhotoPath = uniqueFileName,
                    Gender = employee.Gender
                };

                _employeeRepository.AddEmployee(newEmployee);

                // Redirect to the details page of the newly added employee
                return RedirectToAction("EmployeeDetailsById", new { id = newEmployee.Id });
            }

            // If model validation fails, redisplay the form with validation errors
            return View();
        }

        // GET: /Home/Edit/{id}
        // Returns a view with a form pre-populated with employee data to edit
        [Authorize(Policy = "CanEditUserPolicy")]
        public ViewResult Edit(int id)
        {
            try
            {
                // Fetch employee details by id
                var employee = _employeeRepository.GetEmployeeById(id);

                if (employee == null)
                    return View("~/Views/Home/Error.cshtml");

                // Pass employee data to the view via ViewBag (dynamic property)
                var viewModel = new EditEmployeeViewModel
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Email = employee.Email,
                    Gender = employee.Gender,
                    Department = employee.Department,
                    PhotoPath = employee?.PhotoPath //?.Split('_').Last()
                };

                ViewBag.ImgPath = employee?.PhotoPath?.Split('_').Last();

                return View(viewModel);
            }
            catch (Exception ex) {
                // Optional: log the exception
                // _logger.LogError(ex, "Error occurred while editing employee.");

                // Show a user-friendly error page
                ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                return View("~/Views/Home/Error.cshtml");
            }            
        }

        // POST: /Home/Edit
        // Handles form submission to update employee details
        [HttpPost]
        [Authorize(Policy = "CanEditUserPolicy")]
        public IActionResult Edit(EditEmployeeViewModel employee)
        {
            var existingEmployee = _employeeRepository.GetEmployeeById(employee.Id);
            string? uniqueFileName = null;

            if (ModelState.IsValid)
            {
                try
                {
                    if (existingEmployee == null)
                    {
                        return View("~/Views/Home/Error.cshtml");
                    }

                    existingEmployee.Name = employee.Name;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.Department = employee.Department;
                    existingEmployee.Gender = employee.Gender;                    

                    if (employee.Photo != null)
                    {
                        string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + employee?.Photo?.FileName;
                        string filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            employee?.Photo?.CopyTo(fileStream);
                        }

                        existingEmployee.PhotoPath = uniqueFileName;
                    }

                    _employeeRepository.UpdateEmployee(existingEmployee);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Optional: log the exception
                    // _logger.LogError(ex, "Error occurred while editing employee.");

                    // Show a user-friendly error page
                    ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                    return View("~/Views/Home/Error.cshtml");
                }
            }

            if (existingEmployee != null)
                employee.PhotoPath = existingEmployee.PhotoPath;

            // If validation fails
            return View(employee);
        }

        // GET: /Home/Delete/{id}
        // Deletes the employee identified by id and redirects to the employee list
        [HttpGet]
        [Authorize(Policy = "CanDeleteUserPolicy")]
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return View("~/Views/Home/Error.cshtml");
                }

                // Remove employee from repository
                _employeeRepository.DelEmployee(id ?? 0);

                // Redirect to the index page listing all employees
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Optional: log the exception
                // _logger.LogError(ex, "Error occurred while editing employee.");

                // Show a user-friendly error page
                ViewBag.ErrorMessage = "An error occurred while updating the employee details.";
                return View("~/Views/Home/Error.cshtml");
            }            
        }

        // GET: /Home/Details
        // Example method showing different ways to pass data to views (commented out)
        public ViewResult Details()
        {
            // Option 1: Using ViewData dictionary (commented)
            /*
            Employee empModel = _employeeRepository.GetEmployeeById(1);
            ViewData["Employee"] = empModel;
            ViewData["PageTitle"] = "Employee Details";
            return View(empModel);
            */

            // Option 2: Using ViewBag dynamic properties (commented)
            /*
            Employee empModel = _employeeRepository.GetEmployeeById(1);
            ViewBag.EmployeeDtls = empModel;
            ViewBag.PageTitle = "Employee Details";
            return View(empModel);
            */

            // Option 3: Using strongly typed model (commented)
            /*
            Employee empModel = _employeeRepository.GetEmployeeById(1);
            ViewBag.PageTitle = "Employee Details";
            return View(empModel);
            */

            // Option 4: Using a ViewModel to encapsulate data (active)
            HomeDetailsViewModel viewModel = new HomeDetailsViewModel()
            {
                employee = _employeeRepository.GetEmployeeById(1),  // Get employee by id=1
                PageTitle = "Employee Details"                       // Set page title
            };

            // Pass ViewModel to the view
            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Home/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [Route("Home/Error")]
        public IActionResult Error()
        {
            ViewBag.ErrorTitle = "An error occurred while processing your request.";
            ViewBag.ErrorMessage = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
