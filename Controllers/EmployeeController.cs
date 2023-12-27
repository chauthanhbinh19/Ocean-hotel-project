using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Project.Models;
using System.IO;
using System.Web;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Encodings;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;

namespace Project.Controllers;

public class EmployeeController : Controller
{
    private readonly string connectionString = DataConnection.Connection;
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var userSession = HttpContext.Session.GetString("username");
        if (string.IsNullOrEmpty(userSession))
        {
            // Session người dùng không tồn tại, chuyển hướng đến trang đăng nhập hoặc trang không được ủy quyền
            filterContext.Result = RedirectToAction("Login", "Login");
        }

        base.OnActionExecuting(filterContext);
    }
    public Employee GetAvatar(){
        Employee employee = new Employee();
        var usernameSession = HttpContext.Session.GetString("username");
        var passwordSession = HttpContext.Session.GetString("password");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select employees_img.img, employees.name, employees.role from employees, employees_img, accounts where employees.id=employees_img.employee_id and employees.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                employee.img=reader.GetString(0);
                employee.name=reader.GetString(1);
                employee.role=reader.GetString(2);
            }
            connection.Close();
        }
        return employee;
    }
    //Lấy dữ liệu employee từ database lên bảng
    public IActionResult AdminEmployee(int page)
    {
        HttpContext.Session.Remove("EmployeeSearch");
        List<Employee> employees = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "";
                int emp_status = reader.GetInt32(8);
                if (emp_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                Employee employee = new Employee
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender = reader.GetString(2),
                    dateofbirth = day.ToString("dd-MM-yyyy"),
                    joiningdate = day1.ToString("dd-MM-yyyy"),
                    email = reader.GetString(5),
                    phone = reader.GetString(6),
                    address = reader.GetString(7),
                    role = reader.GetString(11),
                    status = statusString,
                    img = reader.GetString(12)
                };
                employee.account = new Account();
                employee.account.username = reader.GetString(9);
                employee.account.password = reader.GetString(10);
                employees.Add(employee);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.status=TempData["status"];
        ViewBag.status1=TempData["status1"];
        return View("~/Views/HotelViews/AdminEmployee.cshtml", pagedEmployee);
    }
    public IActionResult AdminEmployeeActive(int page)
    {
        HttpContext.Session.Remove("EmployeeActiveSearch");
        List<Employee> employees_active = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int emp_status = reader.GetInt32(8);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                if (emp_status == 1)
                {
                    Employee employee_active = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Active",
                        img = reader.GetString(12)
                    };
                    employee_active.account = new Account();
                    employee_active.account.username = reader.GetString(9);
                    employee_active.account.password = reader.GetString(10);
                    employees_active.Add(employee_active);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_active.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        return View("~/Views/HotelViews/AdminEmployeeActive.cshtml",pagedEmployee);
    }
    public IActionResult AdminEmployeeInactive(int page)
    {
        HttpContext.Session.Remove("EmployeeInactiveSearch");
        List<Employee> employees_inactive = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int emp_status = reader.GetInt32(8);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                if (emp_status == 0){
                    Employee employee_inactive = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Inactive",
                        img = reader.GetString(12)
                    };
                    employee_inactive.account = new Account();
                    employee_inactive.account.username = reader.GetString(9);
                    employee_inactive.account.password = reader.GetString(10);
                    employees_inactive.Add(employee_inactive);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        return View("~/Views/HotelViews/AdminEmployeeInactive.cshtml", pagedEmployee);
    }
    public IActionResult AdminEmployeeSalary(int page)
    {
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                    email = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    role = reader.GetString(5),
                    salary = reader.GetFloat(6),
                    id1 = reader.GetInt32(7)
                };
                employeeSalaries.Add(employeeSalary);
            }
            connection.Close();
        }
        // ViewBag.employee_salary_list = employeeSalaries;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployeeSalary= employeeSalaries.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_salary_list=pagedEmployeeSalary;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminEmployeeSalary.cshtml", pagedEmployeeSalary);
    }
    public IActionResult AdminAddEmployee()
    {
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddEmployee.cshtml");
    }
    public IActionResult AdminAddEmployeeSalary()
    {
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME FROM EMPLOYEES";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                };
                employeeSalaries.Add(employeeSalary);
            }
            connection.Close();
        }
        ViewBag.employee_salary_list = employeeSalaries;
        return View("~/Views/HotelViews/AdminAddEmployeeSalary.cshtml");
    }
    public IActionResult AdminEditEmployee()
    {
        List<Employee> employees = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.NAME, EMPLOYEES.PHONE, EMPLOYEES.EMAIL, EMPLOYEES.ID FROM EMPLOYEES ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2),
                    id = reader.GetInt32(3)
                };
                employees.Add(employee);
            }
            connection.Close();
        }
        ViewBag.employee_list = employees;
        return View("~/Views/HotelViews/AdminEditEmployee.cshtml");
    }
    public IActionResult AdminEditEmployeeSalary()
    {
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME FROM EMPLOYEES";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                };
                employeeSalaries.Add(employeeSalary);
            }
            connection.Close();
        }
        ViewBag.employee_salary_list = employeeSalaries;
        return View("~/Views/HotelViews/AdminEditEmployeeSalary.cshtml");
    }
    public IActionResult AdminLeave(int page)
    {
        List<Leave> leaves = new List<Leave>();
        HttpContext.Session.Remove("LeaveSearch");
        HttpContext.Session.Remove("LeaveSearch1");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                string status1 = "";
                if (reader.GetInt32(6) == 1)
                {
                    status1 = "Active";
                }
                else
                {
                    status1 = "Inactive";
                }
                Leave leave = new Leave
                {
                    id = reader.GetInt32(0),
                    employee_id = reader.GetInt32(1),
                    leave_type = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    reason = reader.GetString(5),
                    status = status1,
                    employee_name = reader.GetString(7)
                };
                leaves.Add(leave);
            }
        }
        // ViewBag.leave_list = leaves;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedLeave= leaves.ToPagedList(pageNumber,pageSize);
        ViewBag.leave_list=pagedLeave;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminLeave.cshtml", pagedLeave);
    }
    public IActionResult AdminAddLeave()
    {
        List<Employee> employees = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.NAME, EMPLOYEES.PHONE, EMPLOYEES.EMAIL, EMPLOYEES.ID FROM EMPLOYEES ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2),
                    id = reader.GetInt32(3)
                };
                employees.Add(employee);
            }
        }
        ViewBag.employee_list = employees;
        return View("~/Views/HotelViews/AdminAddLeave.cshtml");
    }
    public IActionResult AdminEditLeave()
    {
        List<Employee> employees = new List<Employee>();
        List<Leave> leaves = new List<Leave>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.NAME, EMPLOYEES.PHONE, EMPLOYEES.EMAIL, EMPLOYEES.ID FROM EMPLOYEES ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2),
                    id = reader.GetInt32(3)
                };
                employees.Add(employee);
            }

            reader.Close();
            query = "SELECT ID FROM LEAVES";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Leave leave = new Leave
                {
                    id = reader.GetInt32(0)
                };
                leaves.Add(leave);
            }
            connection.Close();
        }
        ViewBag.employee_list = employees;
        ViewBag.leave_list = leaves;
        return View("~/Views/HotelViews/AdminEditLeave.cshtml");
    }
    public IActionResult AdminAttendance()
    {
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAttendance.cshtml");
    }
    public IActionResult AdminPayslip()
    {
        var usernameSession = HttpContext.Session.GetString("username");
        var passwordSession = HttpContext.Session.GetString("password");
        Employee employee = new Employee();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.JOININGDATE, EMPLOYEES.ADDRESS, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEES, EMPLOYEE_SALARY, ACCOUNTS WHERE EMPLOYEES.ID= EMPLOYEE_SALARY.EMPLOYEE_ID AND ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND ACCOUNTS.USERNAME= @username AND ACCOUNTS.PASSWORD=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", usernameSession);
            command.Parameters.AddWithValue("@password", passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                employee.id = reader.GetInt32(0);
                employee.name = reader.GetString(1);
                DateTime day = reader.GetDateTime(2);
                employee.joiningdate = day.ToString("dd-MM-yyyy");
                employee.address = reader.GetString(3);
                DateTime day1 = reader.GetDateTime(4);
                DateTime day2 = reader.GetDateTime(5);
                employee.employeeSalary.from_date = day1.ToString("dd-MM-yyyy");
                employee.employeeSalary.to_date = day2.ToString("dd-MM-yyyy");
                employee.employeeSalary.salary = reader.GetFloat(6);
                employee.employeeSalary.id1 = reader.GetInt32(7);
            }
        }
        ViewBag.employee = employee;
        return View("~/Views/HotelViews/AdminPayslip.cshtml");
    }
    [HttpPost]
    public async Task<IActionResult> AdminInsertEmployee(Employee employee, IFormFile file)
    {
        int? id = 0;
        int? account_id = 1;
        int? employee_id = 1;
        ModelState.Remove("file");
        ModelState.Remove("account.type");
        ModelState.Remove("status");
        ModelState.Remove("employeeSalary.role");
        ModelState.Remove("employeeSalary.email");
        ModelState.Remove("employeeSalary.salary");
        ModelState.Remove("employeeSalary.to_date");
        ModelState.Remove("employeeSalary.from_date");
        ModelState.Remove("employeeSalary.employee_name");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddEmployee");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM ACCOUNTS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (account_id == reader.GetInt32(0))
                {
                    account_id = account_id + 1;
                }
            }

            reader.Close();
            query = "SELECT ID FROM EMPLOYEES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (employee_id == reader.GetInt32(0))
                {
                    employee_id = employee_id + 1;
                }
            }

            reader.Close();
            query = "SELECT COUNT(*) FROM EMPLOYEES WHERE PHONE=@phone";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@phone",employee?.phone);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Phone number is already used";
                    return RedirectToAction("AdminEmployee");
                }
            }

            reader.Close();
            query = "SELECT COUNT(*) FROM EMPLOYEES WHERE EMAIL=@email";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email",employee?.email);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Email is already used";
                    return RedirectToAction("AdminEmployee");
                }
            }
            
            reader.Close();
            query="SELECT COUNT(*) FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND ACCOUNTS.USERNAME=@username";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username",employee?.account?.username);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Username is already existed";
                    return RedirectToAction("AdminEmployee");
                }
            }

            reader.Close();
            query = "INSERT INTO ACCOUNTS (ID,USERNAME,PASSWORD,TYPE,STATUS) VALUES(@id,@username,@password,@type,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", account_id);
            command.Parameters.AddWithValue("@username", employee?.account?.username);
            command.Parameters.AddWithValue("@password", employee?.account?.password);
            command.Parameters.AddWithValue("@type", "customer");
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();

            reader.Close();
            query = "SELECT ID FROM ACCOUNTS WHERE USERNAME=@username and PASSWORD=@password";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", employee?.account?.username);
            command.Parameters.AddWithValue("@password", employee?.account?.password);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }

            reader.Close();
            query = "INSERT INTO EMPLOYEES (ID,NAME,GENDER,DATEOFBIRTH,JOININGDATE,EMAIL,PHONE,ADDRESS,ROLE,ACCOUNT_ID,STATUS) VALUES(@id,@name,@gender,@dateofbirth,@joiningdate,@email,@phone,@address,@role,@account_id,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", employee_id);
            command.Parameters.AddWithValue("@name", employee?.name);
            command.Parameters.AddWithValue("@gender", employee?.gender);
            command.Parameters.AddWithValue("@dateofbirth", employee?.dateofbirth);
            command.Parameters.AddWithValue("@joiningdate", employee?.joiningdate);
            command.Parameters.AddWithValue("@email", employee?.email);
            command.Parameters.AddWithValue("@phone", employee?.phone);
            command.Parameters.AddWithValue("@address", employee?.address);
            command.Parameters.AddWithValue("@role", employee?.role);
            command.Parameters.AddWithValue("@account_id", id);
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();

            var newFileName = "";
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                newFileName = fileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image/Avatar", newFileName);
                if (!System.IO.File.Exists(path))
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            int emp_img_id = 1;
            reader.Close();
            query = "SELECT ID FROM EMPLOYEES_IMG ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (emp_img_id == reader.GetInt32(0))
                {
                    emp_img_id = emp_img_id + 1;
                }
            }

            reader.Close();
            query = "INSERT INTO EMPLOYEES_IMG (ID,EMPLOYEE_ID,IMG) VALUES(@id, @employee_id,@img)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", emp_img_id);
            command.Parameters.AddWithValue("@employee_id", employee_id);
            command.Parameters.AddWithValue("@img", newFileName);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminEmployee");
    }
    [HttpPost]
    public async Task<IActionResult> AdminUpdateEmployee(Employee employee, IFormFile file)
    {
        int? id = 0;
        int? account_id = 0;
        ModelState.Remove("file");
        ModelState.Remove("account.type");
        ModelState.Remove("status");
        ModelState.Remove("employeeSalary.role");
        ModelState.Remove("employeeSalary.email");
        ModelState.Remove("employeeSalary.salary");
        ModelState.Remove("employeeSalary.to_date");
        ModelState.Remove("employeeSalary.from_date");
        ModelState.Remove("employeeSalary.employee_name");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditEmployee");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(*) FROM EMPLOYEES WHERE PHONE=@phone and ID <> @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@phone",employee?.phone);
            command.Parameters.AddWithValue("@id",employee?.id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Phone number is already used";
                    return RedirectToAction("AdminEmployee");
                }
            }

            reader.Close();
            query = "SELECT COUNT(*) FROM EMPLOYEES WHERE EMAIL=@email and ID <> @id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email",employee?.email);
            command.Parameters.AddWithValue("@id",employee?.id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Email is already used";
                    return RedirectToAction("AdminEmployee");
                }
            }
            
            reader.Close();
            query="SELECT COUNT(*) FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID <> @id AND ACCOUNTS.USERNAME=@username";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",employee?.id);
            command.Parameters.AddWithValue("@username",employee?.account?.username);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Username is already existed";
                    return RedirectToAction("AdminEmployee");
                }
            }

            reader.Close();
            query = "UPDATE EMPLOYEES SET NAME=@name, GENDER=@gender, DATEOFBIRTH=@dateofbirth,JOININGDATE=@joiningdate, EMAIL=@email, PHONE=@phone, ADDRESS=@address, ROLE=@role, STATUS=@status WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", employee?.name);
            command.Parameters.AddWithValue("@gender", employee?.gender);
            command.Parameters.AddWithValue("@dateofbirth", employee?.dateofbirth);
            command.Parameters.AddWithValue("@joiningdate", employee?.joiningdate);
            command.Parameters.AddWithValue("@email", employee?.email);
            command.Parameters.AddWithValue("@phone", employee?.phone);
            command.Parameters.AddWithValue("@address", employee?.address);
            command.Parameters.AddWithValue("@role", employee?.role);
            command.Parameters.AddWithValue("@status", employee?.status);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();

            reader.Close();
            query="SELECT ACCOUNTS.ID FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",employee?.id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                account_id=reader.GetInt32(0);
            }

            reader.Close();
            query = "UPDATE ACCOUNTS SET USERNAME=@username, PASSWORD=@password WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", employee?.account?.username);
            command.Parameters.AddWithValue("@password", employee?.account?.password);
            command.Parameters.AddWithValue("@id", account_id);
            reader = command.ExecuteReader();

            reader.Close();
            var newFileName = "";
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                newFileName = fileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image/Avatar", newFileName);
                if (!System.IO.File.Exists(path))
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                query = "UPDATE EMPLOYEES_IMG SET IMG=@img WHERE EMPLOYEE_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@img", newFileName);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();
            }
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminEmployee");
    }
    [HttpPost]
    public IActionResult GetEmployeeInfo(string selectedOption)
    {
        string? name = "";
        string? phone = "";
        string? email = "";
        string? gender = "";
        string? dateofbirth = "";
        string? joiningdate = "";
        string? address = "";
        string? role = "";
        string? username = "";
        string? password = "";
        string? img="";
        string status="";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE, EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG, EMPLOYEES.STATUS FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID and EMPLOYEES.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                // id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender = reader.GetString(2);
                DateTime day = reader.GetDateTime(3);
                dateofbirth = day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone = reader.GetString(5);
                address = reader.GetString(6);
                username = reader.GetString(8);
                password = reader.GetString(9);
                DateTime day1 = reader.GetDateTime(10);
                joiningdate = day1.ToString("yyyy-MM-dd");
                role = reader.GetString(11);
                img= reader.GetString(12);
                if(reader.GetInt32(13)==1){
                    status="Active";
                }else{
                    status="Inactive";
                }
            }
            connection.Close();
        }
        return Json(new { name = name, phone = phone, email = email, gender = gender, dateofbirth = dateofbirth, address = address, username = username, password = password, joiningdate = joiningdate, role = role, img =img, status=status });
    }
    [HttpPost]
    public IActionResult GetEmployeeInfo2(string selectedOption)
    {
        string? name = "";
        string? phone = "";
        string? email = "";
        string? gender = "";
        string? dateofbirth = "";
        string? joiningdate = "";
        string? address = "";
        string? role = "";
        string? username = "";
        string? password = "";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE, EMPLOYEES.ROLE FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES.EMAIL=@email";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                // id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender = reader.GetString(2);
                DateTime day = reader.GetDateTime(3);
                dateofbirth = day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone = reader.GetString(5);
                address = reader.GetString(6);
                username = reader.GetString(8);
                password = reader.GetString(9);
                DateTime day1 = reader.GetDateTime(10);
                joiningdate = day1.ToString("yyyy-MM-dd");
                role = reader.GetString(11);
            }
            connection.Close();
        }
        return Json(new { name = name, phone = phone, email = email, gender = gender, dateofbirth = dateofbirth, address = address, username = username, password = password, joiningdate = joiningdate, role = role });
    }
    [HttpPost]
    public IActionResult GetEmployeeInfo3(string selectedOption)
    {
        string? name = "";
        string? phone = "";
        string? email = "";
        string? gender = "";
        string? dateofbirth = "";
        string? joiningdate = "";
        string? address = "";
        string? role = "";
        string? username = "";
        string? password = "";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE, EMPLOYEES.ROLE FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES.phone=@phone";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@phone", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                // id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender = reader.GetString(2);
                DateTime day = reader.GetDateTime(3);
                dateofbirth = day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone = reader.GetString(5);
                address = reader.GetString(6);
                username = reader.GetString(8);
                password = reader.GetString(9);
                DateTime day1 = reader.GetDateTime(10);
                joiningdate = day1.ToString("yyyy-MM-dd");
                role = reader.GetString(11);
            }
            connection.Close();
        }
        return Json(new { name = name, phone = phone, email = email, gender = gender, dateofbirth = dateofbirth, address = address, username = username, password = password, joiningdate = joiningdate, role = role });
    }
    [HttpPost]
    public IActionResult GetEmployeeInfo4(string selectedOption)
    {
        string? name = "";
        int? id = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE, EMPLOYEES.ROLE FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                name = reader.GetString(1);
            }
            connection.Close();
        }
        return Json(new { name = name, id = id });
    }
    [HttpPost]
    public IActionResult GetEmployeeInfo5(string selectedOption)
    {
        string? name = "";
        int? id = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE, EMPLOYEES.ROLE FROM ACCOUNTS, EMPLOYEES WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES.NAME=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                name = reader.GetString(1);
            }
            connection.Close();
        }
        return Json(new { name = name, id = id });
    }
    [HttpPost]
    public IActionResult GetLeaveInfo(string selectedOption)
    {
        string? name = "";
        string? leave_type = "";
        string? reason = "";
        string? from_date = "";
        string? to_date = "";
        int? employee_id = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID AND LEAVES.ID =@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                employee_id = reader.GetInt32(1);
                leave_type = reader.GetString(2);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                from_date = day.ToString("yyyy-MM-dd");
                to_date = day1.ToString("yyyy-MM-dd");
                reason = reader.GetString(5);
                name = reader.GetString(7);
            }
        }
        return Json(new { name = name, id = employee_id, leave_type = leave_type, reason = reason, from_date = from_date, to_date = to_date });
    }
    [HttpPost]
    public IActionResult AdminInsertEmployeeSalary(EmployeeSalary employeeSalary)
    {
        int id = 1;
        ModelState.Remove("email");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddEmployeeSalary");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM EMPLOYEE_SALARY ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (id == reader.GetInt32(0))
                {
                    id = id + 1;
                }
            }

            reader.Close();
            query = "INSERT INTO EMPLOYEE_SALARY (ID,EMPLOYEE_ID,FROM_DATE,TO_DATE, SALARY) VALUES(@id,@employee_id,@from_date,@to_date,@salary)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@employee_id", employeeSalary.id);
            command.Parameters.AddWithValue("@from_date", employeeSalary.from_date);
            command.Parameters.AddWithValue("@to_date", employeeSalary.to_date);
            command.Parameters.AddWithValue("@salary", employeeSalary.salary);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminEmployeeSalary");
    }
    [HttpPost]
    public IActionResult AdminUpdateEmployeeSalary(EmployeeSalary employeeSalary)
    {
        ModelState.Remove("email");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditEmployeeSalary");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE EMPLOYEE_SALARY SET EMPLOYEE_ID=@employee_id, FROM_DATE=@from_date,TO_DATE=@to_date,SALARY=@salary WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@employee_id", employeeSalary.id);
            command.Parameters.AddWithValue("@from_date", employeeSalary.from_date);
            command.Parameters.AddWithValue("@to_date", employeeSalary.to_date);
            command.Parameters.AddWithValue("@salary", employeeSalary.salary);
            command.Parameters.AddWithValue("@id", employeeSalary.id1);
            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminEmployeeSalary");
    }
    [HttpPost]
    public IActionResult AdminInsertLeave(Leave leave)
    {
        int leave_id = 1;
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddLeave");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM LEAVES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (leave_id == reader.GetInt32(0))
                {
                    leave_id = leave_id + 1;
                }
            }

            reader.Close();
            query = "INSERT INTO LEAVES (ID,EMPLOYEE_ID,LEAVE_TYPE,FROM_DATE,TO_DATE,REASON, STATUS) VALUES(@id,@employee_id,@leave_type,@from_date, @to_date,@reason,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", leave_id);
            command.Parameters.AddWithValue("@employee_id", leave.employee_id);
            command.Parameters.AddWithValue("@leave_type", leave.leave_type);
            command.Parameters.AddWithValue("@from_date", leave.from_date);
            command.Parameters.AddWithValue("@to_date", leave.to_date);
            command.Parameters.AddWithValue("@reason", leave.reason);
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminLeave");
    }
    [HttpPost]
    public IActionResult AdminUpdateLeave(Leave leave)
    {
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditLeave");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE LEAVES SET EMPLOYEE_ID=@employee_id,LEAVE_TYPE=@leave_type, FROM_DATE=@from_date, TO_DATE=@to_date,REASON=@reason WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", leave.id);
            command.Parameters.AddWithValue("@employee_id", leave.employee_id);
            command.Parameters.AddWithValue("@leave_type", leave.leave_type);
            command.Parameters.AddWithValue("@from_date", leave.from_date);
            command.Parameters.AddWithValue("@to_date", leave.to_date);
            command.Parameters.AddWithValue("@reason", leave.reason);
            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminLeave");
    }
    [HttpPost]
    public IActionResult RedirectToAdminAddLeave()
    {
        return RedirectToAction("AdminAddLeave");
    }
    [HttpPost]
    public IActionResult RedirectAdminAddEmployeeSalary()
    {
        return RedirectToAction("AdminAddEmployeeSalary");
    }
    public IActionResult EditEmployee(int id)
    {
        List<Employee> employees = new List<Employee>();
        Employee employee1 = new Employee();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditEmployee");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.NAME, EMPLOYEES.PHONE, EMPLOYEES.EMAIL, EMPLOYEES.ID FROM EMPLOYEES ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2),
                    id = reader.GetInt32(3)
                };
                employees.Add(employee);
            }

            reader.Close();
            query = "SELECT EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH, EMPLOYEES.EMAIL,EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, EMPLOYEES.JOININGDATE,EMPLOYEES.ROLE, EMPLOYEES.STATUS, EMPLOYEES_IMG.IMG FROM ACCOUNTS,EMPLOYEES,EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=@id AND EMPLOYEES_IMG.EMPLOYEE_ID=EMPLOYEES.ID";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                employee1.name = reader.GetString(0);
                employee1.gender = reader.GetString(1);
                DateTime day = reader.GetDateTime(2);
                employee1.dateofbirth = day.ToString("yyyy-MM-dd");
                employee1.email = reader.GetString(3);
                employee1.phone = reader.GetString(4);
                employee1.address = reader.GetString(5);
                employee1.account = new Account();
                employee1.account.username = reader.GetString(6);
                employee1.account.password = reader.GetString(7);
                DateTime day1 = reader.GetDateTime(8);
                employee1.joiningdate = day1.ToString("yyyy-MM-dd");
                employee1.role = reader.GetString(9);
                if(reader.GetInt32(10)==1){
                    employee1.status="Active";
                }else{
                    employee1.status="Inactive";
                }
                employee1.img=reader.GetString(11);
            }
            connection.Close();
        }
        ViewBag.employee_list = employees;
        return View("~/Views/HotelViews/AdminEditEmployee.cshtml", employee1);
    }
    public IActionResult DeleteEmployee(int id)
    {
        int id1 = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ACCOUNT_ID FROM EMPLOYEES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id1 = reader.GetInt32(0);
            }

            reader.Close();
            try
            {
                query = "DELETE FROM EMPLOYEES_IMG WHERE EMPLOYEE_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "DELETE FROM EMPLOYEES WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "DELETE FROM ACCOUNTS WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id1);
                reader = command.ExecuteReader();
            }
            catch (Exception)
            {
                // ViewBag.ModelState.AddModelError("Error", e.Message);
                return View("~/Views/HotelViews/Error.cshtml");
            }

            connection.Close();
        }
        return RedirectToAction("AdminEmployee");
    }
    public IActionResult EditEmployeeSalary(int id)
    {
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        EmployeeSalary employeeSalary1 = new EmployeeSalary();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditEmployeeSalary");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME FROM EMPLOYEES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                };
                employeeSalaries.Add(employeeSalary);
            }

            reader.Close();
            query = "SELECT EMPLOYEE_SALARY.ID, EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE, EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY FROM EMPLOYEES, EMPLOYEE_SALARY WHERE EMPLOYEES.ID=EMPLOYEE_SALARY.EMPLOYEE_ID AND EMPLOYEE_SALARY.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                employeeSalary1.id1 = id;
                employeeSalary1.id = reader.GetInt32(1);
                employeeSalary1.employee_name = reader.GetString(2);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                employeeSalary1.from_date = day.ToString("yyyy-MM-dd");
                employeeSalary1.to_date = day1.ToString("yyyy-MM-dd");
                employeeSalary1.role = reader.GetString(5);
                employeeSalary1.salary = reader.GetFloat(6);
            }
            connection.Close();
        }
        ViewBag.employee_salary_list = employeeSalaries;
        return View("~/Views/HotelViews/AdminEditEmployeeSalary.cshtml", employeeSalary1);
    }
    public IActionResult DeleteEmployeeSalary(int id)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM EMPLOYEE_SALARY WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                MySqlDataReader reader = command.ExecuteReader();
            }
            catch (Exception)
            {
                // ViewBag.ModelState.AddModelError("Error", e.Message);
                return View("~/Views/HotelViews/Error.cshtml");
            }

            connection.Close();
        }
        return RedirectToAction("AdminEmployeeSalary");
    }
    public IActionResult EditLeave(int id)
    {
        List<Employee> employees = new List<Employee>();
        List<Leave> leaves = new List<Leave>();
        Leave leave1 = new Leave();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditLeave");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.NAME, EMPLOYEES.PHONE, EMPLOYEES.EMAIL, EMPLOYEES.ID FROM EMPLOYEES ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2),
                    id = reader.GetInt32(3)
                };
                employees.Add(employee);
            }

            reader.Close();
            query = "SELECT ID FROM LEAVES";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Leave leave = new Leave
                {
                    id = reader.GetInt32(0)
                };
                leaves.Add(leave);
            }

            reader.Close();
            query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID, EMPLOYEES.NAME, LEAVES.LEAVE_TYPE, LEAVES.FROM_DATE, LEAVES.TO_DATE, LEAVES.STATUS,LEAVES.REASON from LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID AND LEAVES.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                leave1.id = reader.GetInt32(0);
                leave1.employee_id = reader.GetInt32(1);
                leave1.employee_name = reader.GetString(2);
                leave1.leave_type = reader.GetString(3);
                DateTime day = reader.GetDateTime(4);
                DateTime day1 = reader.GetDateTime(5);
                leave1.from_date = day.ToString("yyyy-MM-dd");
                leave1.to_date = day1.ToString("yyyy-MM-dd");
                leave1.status = reader.GetString(6);
                leave1.reason = reader.GetString(7);
            }
            connection.Close();
        }
        ViewBag.employee_list = employees;
        ViewBag.leave_list = leaves;
        return View("~/Views/HotelViews/AdminEditLeave.cshtml", leave1);
    }
    public IActionResult DeleteLeave(int id)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM LEAVES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                MySqlDataReader reader = command.ExecuteReader();
            }
            catch (Exception)
            {
                // ViewBag.ModelState.AddModelError("Error", e.Message);
                return View("~/Views/HotelViews/Error.cshtml");
            }

            connection.Close();
        }
        return RedirectToAction("AdminLeave");
    }
    public IActionResult AdminSearchEmployee(string searchkeyword, int page)
    {
        List<Employee> employees = new List<Employee>();
        List<Employee> employees_active = new List<Employee>();
        List<Employee> employees_inactive = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext?.Session.SetString("EmployeeSearch", searchkeyword);
            if (searchkeyword.ToLower() == "active")
            {
                searchkeyword = "1";
            }
            else if (searchkeyword.ToLower() == "inactive")
            {
                searchkeyword = "0";
            }
        }
        var a= HttpContext!.Session!.GetString("EmployeeSearch");
        if(a != null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG  WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES_IMG.EMPLOYEE_ID=EMPLOYEES.ID AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1) ORDER BY EMPLOYEES.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1", searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "";
                int emp_status = reader.GetInt32(8);
                if (emp_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                Employee employee = new Employee
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender = reader.GetString(2),
                    dateofbirth = day.ToString("dd-MM-yyyy"),
                    joiningdate = day1.ToString("dd-MM-yyyy"),
                    email = reader.GetString(5),
                    phone = reader.GetString(6),
                    address = reader.GetString(7),
                    role = reader.GetString(11),
                    status = statusString,
                    img = reader.GetString(12)
                };
                employee.account = new Account();
                employee.account.username = reader.GetString(9);
                employee.account.password = reader.GetString(10);
                if (emp_status == 1)
                {
                    Employee employee_active = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Active",
                        img = reader.GetString(12)
                    };
                    employee_active.account = new Account();
                    employee_active.account.username = reader.GetString(9);
                    employee_active.account.password = reader.GetString(10);
                    employees_active.Add(employee_active);
                }
                else
                {
                    Employee employee_inactive = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Inactive",
                        img = reader.GetString(12)
                    };
                    employee_inactive.account = new Account();
                    employee_inactive.account.username = reader.GetString(9);
                    employee_inactive.account.password = reader.GetString(10);
                    employees_inactive.Add(employee_inactive);
                }
                employees.Add(employee);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminEmployee.cshtml", pagedEmployee);
    }
    public IActionResult AdminSearchEmployeeActive(string searchkeyword, int page)
    {
        List<Employee> employees = new List<Employee>();
        List<Employee> employees_active = new List<Employee>();
        List<Employee> employees_inactive = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            if (searchkeyword.ToLower() == "active")
            {
                searchkeyword = "1";
            }
            else if (searchkeyword.ToLower() == "inactive")
            {
                searchkeyword = "0";
            }
            HttpContext?.Session.SetString("EmployeeActiveSearch", searchkeyword);
        }
        var a= HttpContext!.Session!.GetString("EmployeeActiveSearch");
        if(a != null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG  WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES_IMG.EMPLOYEE_ID=EMPLOYEES.ID AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1) ORDER BY EMPLOYEES.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1", searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "";
                int emp_status = reader.GetInt32(8);
                if (emp_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                Employee employee = new Employee
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender = reader.GetString(2),
                    dateofbirth = day.ToString("dd-MM-yyyy"),
                    joiningdate = day1.ToString("dd-MM-yyyy"),
                    email = reader.GetString(5),
                    phone = reader.GetString(6),
                    address = reader.GetString(7),
                    role = reader.GetString(11),
                    status = statusString,
                    img = reader.GetString(12)
                };
                employee.account = new Account();
                employee.account.username = reader.GetString(9);
                employee.account.password = reader.GetString(10);
                if (emp_status == 1)
                {
                    Employee employee_active = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Active",
                        img = reader.GetString(12)
                    };
                    employee_active.account = new Account();
                    employee_active.account.username = reader.GetString(9);
                    employee_active.account.password = reader.GetString(10);
                    employees_active.Add(employee_active);
                }
                else
                {
                    Employee employee_inactive = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Inactive",
                        img = reader.GetString(12)
                    };
                    employee_inactive.account = new Account();
                    employee_inactive.account.username = reader.GetString(9);
                    employee_inactive.account.password = reader.GetString(10);
                    employees_inactive.Add(employee_inactive);
                }
                employees.Add(employee);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_active.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminEmployee.cshtml", pagedEmployee);
    }
    public IActionResult AdminSearchEmployeeInactive(string searchkeyword, int page)
    {
        List<Employee> employees = new List<Employee>();
        List<Employee> employees_active = new List<Employee>();
        List<Employee> employees_inactive = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            if (searchkeyword.ToLower() == "active")
            {
                searchkeyword = "1";
            }
            else if (searchkeyword.ToLower() == "inactive")
            {
                searchkeyword = "0";
            }
            HttpContext?.Session.SetString("EmployeeInactiveSearch", searchkeyword);
        }
        var a= HttpContext!.Session!.GetString("EmployeeInactiveSearch");
        if(a != null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG  WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID and EMPLOYEES_IMG.EMPLOYEE_ID=EMPLOYEES.ID AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1) ORDER BY EMPLOYEES.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1", searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "";
                int emp_status = reader.GetInt32(8);
                if (emp_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                Employee employee = new Employee
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender = reader.GetString(2),
                    dateofbirth = day.ToString("dd-MM-yyyy"),
                    joiningdate = day1.ToString("dd-MM-yyyy"),
                    email = reader.GetString(5),
                    phone = reader.GetString(6),
                    address = reader.GetString(7),
                    role = reader.GetString(11),
                    status = statusString,
                    img = reader.GetString(12)
                };
                employee.account = new Account();
                employee.account.username = reader.GetString(9);
                employee.account.password = reader.GetString(10);
                if (emp_status == 1)
                {
                    Employee employee_active = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Active",
                        img = reader.GetString(12)
                    };
                    employee_active.account = new Account();
                    employee_active.account.username = reader.GetString(9);
                    employee_active.account.password = reader.GetString(10);
                    employees_active.Add(employee_active);
                }
                else
                {
                    Employee employee_inactive = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Inactive",
                        img = reader.GetString(12)
                    };
                    employee_inactive.account = new Account();
                    employee_inactive.account.username = reader.GetString(9);
                    employee_inactive.account.password = reader.GetString(10);
                    employees_inactive.Add(employee_inactive);
                }
                employees.Add(employee);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminEmployee.cshtml", pagedEmployee);
    }
    public IActionResult AdminSearchLeave(string searchkeyword, string searchkeyword1, int page)
    {
        // HttpContext.Session.Remove("LeaveSearch");
        // HttpContext.Session.Remove("LeaveSearch1");
        List<Leave> leaves = new List<Leave>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("LeaveSearch", searchkeyword);
        }
        if(searchkeyword1 !=null){
            HttpContext.Session.SetString("LeaveSearch1", searchkeyword1);
        }
        var a=HttpContext.Session.GetString("LeaveSearch");
        var b=HttpContext.Session.GetString("LeaveSearch1");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b != null && searchkeyword1 == null){
            searchkeyword1=b;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "";
            if (searchkeyword != null && searchkeyword1 == "Select")
            {
                query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID AND (EMPLOYEES.NAME LIKE @id)  ORDER BY LEAVES.ID ASC";
            }
            else if (searchkeyword == null && searchkeyword1 != null && searchkeyword1 != "Select")
            {
                query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID AND (LEAVES.LEAVE_TYPE LIKE @id1)  ORDER BY LEAVES.ID ASC";
            }
            else if (searchkeyword != null && searchkeyword1 != null && searchkeyword1 != "Select")
            {
                query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID AND (EMPLOYEES.NAME LIKE @id AND LEAVES.LEAVE_TYPE LIKE @id1)  ORDER BY LEAVES.ID ASC";
            }
            else if (searchkeyword == null && searchkeyword1 == "Select")
            {
                query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID ORDER BY LEAVES.ID ASC";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            if (searchkeyword != null && searchkeyword1 == null)
            {
                command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            }
            else if (searchkeyword == null && searchkeyword1 != null)
            {
                command.Parameters.AddWithValue("@id1", searchkeyword1);
            }
            else if (searchkeyword != null && searchkeyword1 != null)
            {
                command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
                command.Parameters.AddWithValue("@id1", searchkeyword1);
            }
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                string status1 = "";
                if (reader.GetInt32(6) == 1)
                {
                    status1 = "Active";
                }
                else
                {
                    status1 = "Inactive";
                }
                Leave leave = new Leave
                {
                    id = reader.GetInt32(0),
                    employee_id = reader.GetInt32(1),
                    leave_type = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    reason = reader.GetString(5),
                    status = status1,
                    employee_name = reader.GetString(7)
                };
                leaves.Add(leave);
            }
        }
        // ViewBag.leave_list = leaves;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedLeave= leaves.ToPagedList(pageNumber,pageSize);
        ViewBag.leave_list=pagedLeave;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminLeave.cshtml",pagedLeave);
    }
    public List<Employee> GetAllEmployee(string query)
    {
        List<Employee> employees = new List<Employee>();
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + EmployeeSearch + "%");
            command.Parameters.AddWithValue("@id1", EmployeeSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "";
                int emp_status = reader.GetInt32(8);
                if (emp_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                Employee employee = new Employee
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender = reader.GetString(2),
                    dateofbirth = day.ToString("dd-MM-yyyy"),
                    joiningdate = day1.ToString("dd-MM-yyyy"),
                    email = reader.GetString(5),
                    phone = reader.GetString(6),
                    address = reader.GetString(7),
                    role = reader.GetString(11),
                    status = statusString,
                    img = reader.GetString(12)
                };
                employee.account = new Account();
                employee.account.username = reader.GetString(9);
                employee.account.password = reader.GetString(10);
                employees.Add(employee);
            }
            connection.Close();
        }
        return employees;
    }
    public List<Employee> GetActiveEmployee(string query)
    {
        List<Employee> employees_active = new List<Employee>();
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + EmployeeSearch + "%");
            command.Parameters.AddWithValue("@id1", EmployeeSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int emp_status = reader.GetInt32(8);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                if (emp_status == 1)
                {
                    Employee employee_active = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Active",
                        img = reader.GetString(12)
                    };
                    employee_active.account = new Account();
                    employee_active.account.username = reader.GetString(9);
                    employee_active.account.password = reader.GetString(10);
                    employees_active.Add(employee_active);
                }
            }
            connection.Close();
        }
        return employees_active;
    }
    public List<Employee> GetInactiveEmployee(string query)
    {
        List<Employee> employees_inactive = new List<Employee>();
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + EmployeeSearch + "%");
            command.Parameters.AddWithValue("@id1", EmployeeSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int emp_status = reader.GetInt32(8);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                if (emp_status == 0)
                {
                    Employee employee_inactive = new Employee
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender = reader.GetString(2),
                        dateofbirth = day.ToString("dd-MM-yyyy"),
                        joiningdate = day1.ToString("dd-MM-yyyy"),
                        email = reader.GetString(5),
                        phone = reader.GetString(6),
                        address = reader.GetString(7),
                        role = reader.GetString(11),
                        status = "Inactive",
                        img = reader.GetString(12)
                    };
                    employee_inactive.account = new Account();
                    employee_inactive.account.username = reader.GetString(9);
                    employee_inactive.account.password = reader.GetString(10);
                    employees_inactive.Add(employee_inactive);
                }
            }
            connection.Close();
        }
        return employees_inactive;
    }
    public IActionResult AdminSortEmployee(string id, int page)
    {
        List<Employee> employees = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
        string likequery = " AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1)";
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        if (EmployeeSearch != null)
        {
            query = query + likequery;
        }
        if (id == "id_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ID ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "id_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ID DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "name_asc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "name_desc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "gender_asc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "gender_desc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "dateofbirth_asc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "dateofbirth_desc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "phone_asc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "phone_desc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "email_asc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "email_desc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "address_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "address_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "username_asc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "username_desc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "password_asc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "password_desc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "joiningdate_asc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "joiningdate_desc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "role_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "role_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE DESC";
            employees = GetAllEmployee(query);
        }
        else if (id == "status_asc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS ASC";
            employees = GetAllEmployee(query);
        }
        else if (id == "status_desc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS DESC";
            employees = GetAllEmployee(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminEmployee.cshtml", pagedEmployee);
    }
    public IActionResult AdminSortEmployeeActive(string id, int page)
    {
        List<Employee> employees_active = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
        string likequery = " AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1)";
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        if (EmployeeSearch != null)
        {
            query = query + likequery;
        }
        if (id == "id_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ID ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "id_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ID DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "name_asc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "name_desc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "gender_asc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "gender_desc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "dateofbirth_asc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "dateofbirth_desc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "phone_asc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "phone_desc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "email_asc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "email_desc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "address_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "address_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "username_asc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "username_desc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "password_asc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "password_desc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "joiningdate_asc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "joiningdate_desc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "role_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "role_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE DESC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "status_asc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS ASC";
            employees_active = GetActiveEmployee(query);
        }
        else if (id == "status_desc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS DESC";
            employees_active = GetActiveEmployee(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_active.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminEmployeeActive.cshtml", pagedEmployee);
    }
    public IActionResult AdminSortEmployeeInactive(string id, int page)
    {
        List<Employee> employees_inactive = new List<Employee>();
        ViewBag.employee_avatar=GetAvatar();
        string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.GENDER, EMPLOYEES.DATEOFBIRTH,EMPLOYEES.JOININGDATE, EMPLOYEES.EMAIL, EMPLOYEES.PHONE, EMPLOYEES.ADDRESS, EMPLOYEES.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD , EMPLOYEES.ROLE, EMPLOYEES_IMG.IMG FROM ACCOUNTS, EMPLOYEES, EMPLOYEES_IMG WHERE ACCOUNTS.ID=EMPLOYEES.ACCOUNT_ID AND EMPLOYEES.ID=EMPLOYEES_IMG.EMPLOYEE_ID";
        string likequery = " AND (EMPLOYEES.NAME like @id or EMPLOYEES.GENDER like @id1 or EMPLOYEES.DATEOFBIRTH like @id or EMPLOYEES.JOININGDATE like @id or EMPLOYEES.PHONE like @id1 or EMPLOYEES.EMAIL like @id1 or EMPLOYEES.ROLE like @id1 or EMPLOYEES.STATUS like @id1)";
        var EmployeeSearch = HttpContext.Session.GetString("EmployeeSearch");
        if (EmployeeSearch != null)
        {
            query = query + likequery;
        }
        if (id == "id_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ID ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "id_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ID DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "name_asc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "name_desc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "gender_asc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "gender_desc")
        {
            query = query + " ORDER BY EMPLOYEES.GENDER DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "dateofbirth_asc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "dateofbirth_desc")
        {
            query = query + " ORDER BY EMPLOYEES.DATEOFBIRTH DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "phone_asc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "phone_desc")
        {
            query = query + " ORDER BY EMPLOYEES.PHONE DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "email_asc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "email_desc")
        {
            query = query + " ORDER BY EMPLOYEES.EMAIL DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "address_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "address_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ADDRESS DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "username_asc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "username_desc")
        {
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "password_asc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "password_desc")
        {
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "joiningdate_asc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "joiningdate_desc")
        {
            query = query + " ORDER BY EMPLOYEES.JOININGDATE DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "role_asc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "role_desc")
        {
            query = query + " ORDER BY EMPLOYEES.ROLE DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "status_asc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS ASC";
            employees_inactive = GetInactiveEmployee(query);
        }
        else if (id == "status_desc")
        {
            query = query + " ORDER BY EMPLOYEES.STATUS DESC";
            employees_inactive = GetInactiveEmployee(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployee= employees_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_list=pagedEmployee;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminEmployeeInactive.cshtml", pagedEmployee);
    }
    public List<Leave> GetAllLeave(string query)
    {
        List<Leave> leaves = new List<Leave>();
        var LeaveSearch=HttpContext.Session.GetString("LeaveSearch");
        var LeaveSearch1=HttpContext.Session.GetString("LeaveSearch1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + LeaveSearch + "%");
            command.Parameters.AddWithValue("@id1", LeaveSearch1);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                string status1 = "";
                if (reader.GetInt32(6) == 1)
                {
                    status1 = "Active";
                }
                else
                {
                    status1 = "Inactive";
                }
                Leave leave = new Leave
                {
                    id = reader.GetInt32(0),
                    employee_id = reader.GetInt32(1),
                    leave_type = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    reason = reader.GetString(5),
                    status = status1,
                    employee_name = reader.GetString(7)
                };
                leaves.Add(leave);
            }
        }
        return leaves;
    }
    public IActionResult AdminSortLeave(string id, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        string query = "SELECT LEAVES.ID, LEAVES.EMPLOYEE_ID,LEAVES.LEAVE_TYPE,LEAVES.FROM_DATE,LEAVES.TO_DATE,LEAVES.REASON,LEAVES.STATUS, EMPLOYEES.NAME FROM LEAVES, EMPLOYEES WHERE LEAVES.EMPLOYEE_ID=EMPLOYEES.ID";
        List<Leave> leaves = new List<Leave>();
        var LeaveSearch=HttpContext.Session.GetString("LeaveSearch");
        var LeaveSearch1=HttpContext.Session.GetString("LeaveSearch1");
        if (LeaveSearch != null && LeaveSearch1 == "Select")
        {
            query = query + " AND (EMPLOYEES.NAME LIKE @id)";
        }
        else if (LeaveSearch == null && LeaveSearch1 != null && LeaveSearch1 != "Select")
        {
            query = query + " AND (LEAVES.LEAVE_TYPE LIKE @id1)";
        }
        else if (LeaveSearch != null && LeaveSearch1 != null && LeaveSearch1 != "Select")
        {
            query = query + " AND (EMPLOYEES.NAME LIKE @id AND LEAVES.LEAVE_TYPE LIKE @id1)";
        }
        if (id == "id_asc")
        {
            query = query + " ORDER BY LEAVES.ID ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "id_desc")
        {
            query = query + " ORDER BY LEAVES.ID DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "name_asc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "name_desc")
        {
            query = query + " ORDER BY EMPLOYEES.NAME DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "type_asc")
        {
            query = query + " ORDER BY LEAVES.LEAVE_TYPE ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "type_desc")
        {
            query = query + " ORDER BY LEAVES.LEAVE_TYPE DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "from_asc")
        {
            query = query + " ORDER BY LEAVES.FROM_DATE ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "from_desc")
        {
            query = query + " ORDER BY LEAVES.FROM_DATE DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "to_asc")
        {
            query = query + " ORDER BY LEAVES.TO_DATE ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "to_desc")
        {
            query = query + " ORDER BY LEAVES.TO_DATE DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "reason_asc")
        {
            query = query + " ORDER BY LEAVES.REASON ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "reason_desc")
        {
            query = query + " ORDER BY LEAVES.REASON DESC";
            leaves = GetAllLeave(query);
        }
        else if (id == "status_asc")
        {
            query = query + " ORDER BY LEAVES.STATUS ASC";
            leaves = GetAllLeave(query);
        }
        else if (id == "status_desc")
        {
            query = query + " ORDER BY LEAVES.STATUS DESC";
            leaves = GetAllLeave(query);
        }
        // ViewBag.leave_list = leaves;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedLeave= leaves.ToPagedList(pageNumber,pageSize);
        ViewBag.leave_list=pagedLeave;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminLeave.cshtml", pagedLeave);
    }
    public List<EmployeeSalary> GetAllEmployeeSalary(string query)
    {
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                    email = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    role = reader.GetString(5),
                    salary = reader.GetFloat(6),
                    id1 = reader.GetInt32(7)
                };
                employeeSalaries.Add(employeeSalary);
            }
            connection.Close();
        }
        return employeeSalaries;
    }
    public IActionResult AdminSortEmployeeSalary(string id, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        if (id == "id_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.ID ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "id_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.ID DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "salaryid_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.ID ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "salaryid_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.ID DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "name_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.NAME ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "name_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.NAME DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "email_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.EMAIL ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "email_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.EMAIL DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "from_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.FROM_DATE ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "from_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.FROM_DATE DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "to_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.TO_DATE ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "to_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.TO_DATE DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "role_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.ROLE ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "role_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.ROLE DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "salary_asc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.SALARY ASC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        else if (id == "salary_desc")
        {
            string query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEE_SALARY.SALARY DESC";
            employeeSalaries = GetAllEmployeeSalary(query);
        }
        // ViewBag.employee_salary_list = employeeSalaries;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployeeSalary= employeeSalaries.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_salary_list=pagedEmployeeSalary;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminEmployeeSalary.cshtml", pagedEmployeeSalary);
    }
    public IActionResult AdminSearchEmployeeSalary(string searchkeyword,string searchkeyword1, int page){

        ViewBag.employee_avatar=GetAvatar();
        List<EmployeeSalary> employeeSalaries = new List<EmployeeSalary>();
        if(searchkeyword!=null){
            HttpContext.Session.SetString("EmployeeSalarySearch", searchkeyword);
        }
        if(searchkeyword1!=null){
            HttpContext.Session.SetString("EmployeeSalarySearch1", searchkeyword1);
        }
        var a=HttpContext.Session.GetString("EmployeeSalarySearch");
        var b=HttpContext.Session.GetString("EmployeeSalarySearch1");
        if(a != null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b!=null && searchkeyword1==null){
            searchkeyword1=b;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="";
            if(searchkeyword!= null && searchkeyword1 == null){
                query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID AND (EMPLOYEES.NAME LIKE @name) ORDER BY EMPLOYEES.ID ASC";
            }else if(searchkeyword == null && searchkeyword1 != null){
                query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID AND (EMPLOYEES.ROLE LIKE @role) ORDER BY EMPLOYEES.ID ASC";
            }else if(searchkeyword != null && searchkeyword1 != null){
                query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID AND (EMPLOYEES.ROLE LIKE @name and EMPLOYEES.NAME like @name) ORDER BY EMPLOYEES.ID ASC";
            }else{
                query = "SELECT EMPLOYEES.ID, EMPLOYEES.NAME, EMPLOYEES.EMAIL, EMPLOYEE_SALARY.FROM_DATE, EMPLOYEE_SALARY.TO_DATE,EMPLOYEES.ROLE, EMPLOYEE_SALARY.SALARY, EMPLOYEE_SALARY.ID FROM EMPLOYEE_SALARY, EMPLOYEES WHERE EMPLOYEE_SALARY.EMPLOYEE_ID = EMPLOYEES.ID ORDER BY EMPLOYEES.ID ASC";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name","%"+ searchkeyword + "%");
            command.Parameters.AddWithValue("@role","%"+ searchkeyword1 +"%");
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                EmployeeSalary employeeSalary = new EmployeeSalary
                {
                    id = reader.GetInt32(0),
                    employee_name = reader.GetString(1),
                    email = reader.GetString(2),
                    from_date = day.ToString("dd-MM-yyyy"),
                    to_date = day1.ToString("dd-MM-yyyy"),
                    role = reader.GetString(5),
                    salary = reader.GetFloat(6),
                    id1 = reader.GetInt32(7)
                };
                employeeSalaries.Add(employeeSalary);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedEmployeeSalary= employeeSalaries.ToPagedList(pageNumber,pageSize);
        ViewBag.employee_salary_list=pagedEmployeeSalary;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminEmployeeSalary.cshtml", pagedEmployeeSalary);
    }
}