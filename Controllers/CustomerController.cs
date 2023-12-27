using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Project.Controllers;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;

public class CustomerController:Controller
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
    public IActionResult AdminCustomer(int page){
        HttpContext.Session.Remove("CustomerSearch");
        List<Customer> customers=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                customers.Add(customer);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.status=TempData["status"];
        ViewBag.status1=TempData["status1"];
        return View("~/Views/HotelViews/AdminCustomer.cshtml", pagedCustomer);
    }
    public IActionResult AdminCustomerActive(int page){
        HttpContext.Session.Remove("CustomerActiveSearch");
        List<Customer> customers_active=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int cus_status=reader.GetInt32(7);
                DateTime day=reader.GetDateTime(3);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_active.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        return View("~/Views/HotelViews/AdminCustomerActive.cshtml",pagedCustomer);
    }
    public IActionResult AdminCustomerInactive(int page){
        HttpContext.Session.Remove("CustomerInactiveSearch");
        List<Customer> customers_inactive=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int cus_status=reader.GetInt32(7);
                DateTime day=reader.GetDateTime(3);
                if(cus_status==0){
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        return View("~/Views/HotelViews/AdminCustomerInactive.cshtml", pagedCustomer);
    }
    public IActionResult AdminAddCustomer(){
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddCustomer.cshtml");
    }
    public IActionResult AdminEditCustomer(){
        List<Customer> customers= new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, CUSTOMERS.ID FROM CUSTOMERS ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer= new Customer{
                    name=reader.GetString(0),
                    phone=reader.GetString(1),
                    email=reader.GetString(2),
                    id=reader.GetInt32(3)
                };
                customers.Add(customer);
            }
            connection.Close();
        }
        ViewBag.customer_list=customers;
        return View("~/Views/HotelViews/AdminEditCustomer.cshtml");
    }
    [HttpPost]
    public async Task<IActionResult> AdminInsertCustomer(Customer customer, IFormFile file){
        int? id=0;
        int? account_id=1;
        int? customer_id=1;
        ModelState.Remove("file");
        ModelState.Remove("account.type");
        ModelState.Remove("status");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddCustomer");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM ACCOUNTS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(account_id==reader.GetInt32(0)){
                    account_id=account_id+1;
                }
            }

            reader.Close();
            query = "SELECT ID FROM CUSTOMERS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(customer_id==reader.GetInt32(0)){
                    customer_id=customer_id+1;
                }
            }
            
            reader.Close();
            query = "SELECT COUNT(*) FROM CUSTOMERS WHERE PHONE=@phone";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@phone",customer?.phone);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Phone number is already used";
                    return RedirectToAction("AdminCustomer");
                }
            }

            reader.Close();
            query = "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL=@email";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email",customer?.email);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Email is already used";
                    return RedirectToAction("AdminCustomer");
                }
            }

            reader.Close();
            query="SELECT COUNT(*) FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND ACCOUNTS.USERNAME=@username";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username",customer?.account?.username);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(reader.GetInt32(0)>0){
                TempData["status1"] ="Username is already existed";
                return RedirectToAction("AdminCustomer");
                }
            }

            reader.Close();
            query = "INSERT INTO ACCOUNTS (ID,USERNAME,PASSWORD,TYPE,STATUS) VALUES(@id,@username,@password,@type,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",account_id);
            command.Parameters.AddWithValue("@username",customer?.account?.username);
            command.Parameters.AddWithValue("@password",customer?.account?.password);
            command.Parameters.AddWithValue("@type","customer");
            command.Parameters.AddWithValue("@status",1);
            reader = command.ExecuteReader();

            reader.Close();
            query="SELECT ID FROM ACCOUNTS WHERE USERNAME=@username and PASSWORD=@password";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username",customer?.account?.username);
            command.Parameters.AddWithValue("@password",customer?.account?.password);
            reader= command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
            }

            reader.Close();
            query = "INSERT INTO CUSTOMERS (ID,NAME,GENDER,DATEOFBIRTH,EMAIL,PHONE,ADDRESS,ACCOUNT_ID,STATUS,VISIT) VALUES(@id,@name,@gender,@dateofbirth,@email,@phone,@address,@account_id,@status,@visit)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",customer_id);
            command.Parameters.AddWithValue("@name",customer?.name);
            command.Parameters.AddWithValue("@gender",customer?.gender);
            command.Parameters.AddWithValue("@dateofbirth",customer?.dateofbirth);
            command.Parameters.AddWithValue("@email",customer?.email);
            command.Parameters.AddWithValue("@phone",customer?.phone);
            command.Parameters.AddWithValue("@address",customer?.address);
            command.Parameters.AddWithValue("@account_id",id);
            command.Parameters.AddWithValue("@status",1);
            command.Parameters.AddWithValue("@visit",1);
            reader = command.ExecuteReader();

            var newFileName="";
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension=Path.GetExtension(file.FileName);
                newFileName = fileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image/Avatar", newFileName);
                if(!System.IO.File.Exists(path)){
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            int emp_img_id=1;
            reader.Close();
            query="SELECT ID FROM CUSTOMERS_IMG ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(emp_img_id == reader.GetInt32(0)){
                    emp_img_id=emp_img_id+1;
                }
            }

            reader.Close();
            query="INSERT INTO CUSTOMERS_IMG (ID,CUSTOMER_ID,IMG) VALUES(@id, @customer_id,@img)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", emp_img_id);
            command.Parameters.AddWithValue("@customer_id", customer_id);
            command.Parameters.AddWithValue("@img", newFileName);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminCustomer");
    }
    [HttpPost]
    public async Task<IActionResult> AdminUpdateCustomer(Customer customer, IFormFile file){
        int? account_id=0;
        ModelState.Remove("file");
        ModelState.Remove("account.type");
        ModelState.Remove("status");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditCustomer");
        }
        // try{
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {   
                connection.Open();
                string query = "SELECT COUNT(*) FROM CUSTOMERS WHERE PHONE=@phone and ID <> @id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@phone",customer?.phone);
                command.Parameters.AddWithValue("@id",customer?.id);
                MySqlDataReader reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                        TempData["status1"] ="Phone number is already used";
                        return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query = "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL=@email and ID <> @id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email",customer?.email);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                        TempData["status1"] ="Email is already used";
                        return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query="SELECT COUNT(*) FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS.ID <> @id AND ACCOUNTS.USERNAME=@username";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",customer?.id);
                command.Parameters.AddWithValue("@username",customer?.account?.username);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Username is already existed";
                    return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query = "UPDATE CUSTOMERS SET NAME=@name, GENDER=@gender, DATEOFBIRTH=@dateofbirth, EMAIL=@email, PHONE=@phone, ADDRESS=@address, STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name",customer?.name);
                command.Parameters.AddWithValue("@gender",customer?.gender);
                command.Parameters.AddWithValue("@dateofbirth",customer?.dateofbirth);
                command.Parameters.AddWithValue("@email",customer?.email);
                command.Parameters.AddWithValue("@phone",customer?.phone);
                command.Parameters.AddWithValue("@address",customer?.address);
                command.Parameters.AddWithValue("@status",customer?.status);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();

                reader.Close();
                query="SELECT ACCOUNTS.ID FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    account_id=reader.GetInt32(0);
                }

                reader.Close();
                query="UPDATE ACCOUNTS SET USERNAME=@username, PASSWORD=@password WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username",customer?.account?.username);
                command.Parameters.AddWithValue("@password",customer?.account?.password);
                command.Parameters.AddWithValue("@id",account_id);
                reader = command.ExecuteReader();
                reader.Close();
                var newFileName="";
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension=Path.GetExtension(file.FileName);
                    newFileName = fileName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image/Avatar", newFileName);
                    if(!System.IO.File.Exists(path)){
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    query = "UPDATE CUSTOMERS_IMG SET IMG=@img WHERE CUSTOMER_ID=@id";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@img", newFileName);
                    command.Parameters.AddWithValue("@id",customer?.id);
                    reader = command.ExecuteReader();
                }
                connection.Close();
            }
            TempData["status"] ="Update successfully";
        // }catch(Exception){
        //     TempData["status1"] ="Update Failed";
        // }
        return RedirectToAction("AdminCustomer");
    }
    [HttpPost]
    public IActionResult GetCustomerInfo(string selectedOption){
        string? name="";
        string? phone="";
        string? email="";
        string? gender="";
        string? dateofbirth="";
        string? address="";
        string? username="";
        string? password="";
        int id=1;
        string? img="";
        string status="";
        int statusint=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG, CUSTOMERS.STATUS FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS.ID=CUSTOMERS_IMG.CUSTOMER_ID and CUSTOMERS.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                dateofbirth=day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone=reader.GetString(5);
                address=reader.GetString(6);
                username=reader.GetString(8);
                password=reader.GetString(9);
                img=reader.GetString(10);
                if(reader.GetInt32(11)==1){
                    status="Active";
                }else{
                    status="Inactive";
                }
                statusint=reader.GetInt32(11);
            }
            connection.Close();
        }
        return Json(new {name=name, phone=phone, email =email, gender=gender, dateofbirth=dateofbirth, address=address, username=username, password=password, id=id, img=img, status=status,statusint =statusint});
    }
    [HttpPost]
    public IActionResult GetCustomerInfo2(string selectedOption){
        string? name="";
        string? phone="";
        string? email="";
        string? gender="";
        string? dateofbirth="";
        string? address="";
        string? username="";
        string? password="";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS.EMAIL=@email";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                // id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                dateofbirth=day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone=reader.GetString(5);
                address=reader.GetString(6);
                username=reader.GetString(8);
                password=reader.GetString(9);
            }
            connection.Close();
        }
        return Json(new {name=name, phone=phone, email =email, gender=gender, dateofbirth=dateofbirth, address=address, username=username, password=password});
    }
    [HttpPost]
    public IActionResult GetCustomerInfo3(string selectedOption){
        string? name="";
        string? phone="";
        string? email="";
        string? gender="";
        string? dateofbirth="";
        string? address="";
        string? username="";
        string? password="";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS.PHONE=@phone";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@phone",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                // id = reader.GetInt32(0);
                name = reader.GetString(1);
                gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                dateofbirth=day.ToString("yyyy-MM-dd");
                email = reader.GetString(4);
                phone=reader.GetString(5);
                address=reader.GetString(6);
                username=reader.GetString(8);
                password=reader.GetString(9);
            }
            connection.Close();
        }
        return Json(new {name=name, phone=phone, email =email, gender=gender, dateofbirth=dateofbirth, address=address, username=username, password=password});
    }
    public IActionResult EditCustomer(int id){
        List<Customer> customers= new List<Customer>();
        Customer customer= new Customer();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditCustomer");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, CUSTOMERS.ID FROM CUSTOMERS ";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer1= new Customer{
                    name=reader.GetString(0),
                    phone=reader.GetString(1),
                    email=reader.GetString(2),
                    id=reader.GetInt32(3)
                };
                customers.Add(customer1);
            }

            reader.Close();
            query= "SELECT CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL,CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS.STATUS, CUSTOMERS_IMG.IMG FROM ACCOUNTS,CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS.ID=@id AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                customer.id=id;
                customer.name=reader.GetString(0);
                customer.gender=reader.GetString(1);
                DateTime day=reader.GetDateTime(2);
                customer.dateofbirth=day.ToString("yyyy-MM-dd");
                customer.email=reader.GetString(3);
                customer.phone=reader.GetString(4);
                customer.address=reader.GetString(5);
                customer.account=new Account();
                customer.account.username=reader.GetString(6);
                customer.account.password=reader.GetString(7);
                if(reader.GetInt32(8)==1){
                    customer.status="Active";
                }else{
                    customer.status="Inactive";
                }
                customer.img=reader.GetString(9);
            }
            connection.Close();
        }
        ViewBag.customer_list=customers;
        return View("~/Views/HotelViews/AdminEditCustomer.cshtml", customer);
    }
    public IActionResult DeleteCustomer(int id){
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
            try{
                query = "DELETE FROM CUSTOMERS_IMG WHERE CUSTOMER_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "DELETE FROM CUSTOMERS WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "DELETE FROM ACCOUNTS WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id1);
                reader = command.ExecuteReader();
            }catch(Exception){
                // ViewBag.ModelState.AddModelError("Error", e.Message);
                return View("~/Views/HotelViews/Error.cshtml");
            }
            
            connection.Close();
        }
        return RedirectToAction("AdminCustomer");
    }
    public IActionResult AdminSearchCustomer(string searchkeyword, int page){
        List<Customer> customers=new List<Customer>();
        List<Customer> customers_active=new List<Customer>();
        List<Customer> customers_inactive=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null ){
            HttpContext?.Session.SetString("CustomerSearch",searchkeyword);
            if(searchkeyword.ToLower()=="active"){
                searchkeyword="1";
            }else if(searchkeyword.ToLower() == "inactive"){
                searchkeyword="0";
            }
        }
        var a= HttpContext!.Session.GetString("CustomerSearch");
        if(a != null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1) order by CUSTOMERS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }else{
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
                customers.Add(customer);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminCustomer.cshtml", pagedCustomer);
    }
    public IActionResult AdminSearchCustomerActive(string searchkeyword, int page){
        List<Customer> customers=new List<Customer>();
        List<Customer> customers_active=new List<Customer>();
        List<Customer> customers_inactive=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext?.Session.SetString("CustomerActiveSearch",searchkeyword);
            if(searchkeyword.ToLower()=="active"){
                searchkeyword="1";
            }else if(searchkeyword.ToLower() == "inactive"){
                searchkeyword="0";
            }
        }
        var a= HttpContext!.Session.GetString("CustomerActiveSearch");
        if(a != null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1) order by CUSTOMERS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }else{
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
                customers.Add(customer);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_active.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminCustomerActive.cshtml", pagedCustomer);
    }
    public IActionResult AdminSearchCustomerInactive(string searchkeyword, int page){
        List<Customer> customers=new List<Customer>();
        List<Customer> customers_active=new List<Customer>();
        List<Customer> customers_inactive=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext?.Session.SetString("CustomerInactiveSearch",searchkeyword);
            if(searchkeyword.ToLower()=="active"){
                searchkeyword="1";
            }else if(searchkeyword.ToLower() == "inactive"){
                searchkeyword="0";
            }
        }
        var a= HttpContext!.Session.GetString("CustomerInactiveSearch");
        if(a != null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1) order by CUSTOMERS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }else{
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
                customers.Add(customer);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminCustomerActive.cshtml", pagedCustomer);
    }
    public List<Customer> GetActiveCustomer(string query){
        List<Customer> customers_active=new List<Customer>();
        var CustomerSearch=HttpContext.Session.GetString("CustomerSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + CustomerSearch + "%");
            command.Parameters.AddWithValue("@id1",CustomerSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                int cus_status=reader.GetInt32(7);
                DateTime day=reader.GetDateTime(3);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }
            }
            connection.Close();
        }
        return customers_active;
    }
    public List<Customer> GetAllCustomer(string query){
        List<Customer> customers=new List<Customer>();
        var CustomerSearch=HttpContext.Session.GetString("CustomerSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + CustomerSearch + "%");
            command.Parameters.AddWithValue("@id1",CustomerSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                customers.Add(customer);
            }
            connection.Close();
        }
        return customers;
    }
    public List<Customer> GetInactiveCustomer(string query){
        List<Customer> customers_inactive=new List<Customer>();
        var CustomerSearch=HttpContext.Session.GetString("CustomerSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + CustomerSearch + "%");
            command.Parameters.AddWithValue("@id1",CustomerSearch);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                int cus_status=reader.GetInt32(7);
                DateTime day=reader.GetDateTime(3);
                if(cus_status==0){
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
            }
            connection.Close();
        }
        return customers_inactive;
    }
    public IActionResult AdminSortCustomer(string id, int page){
        List<Customer> customers=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
        string likequery=" AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1)";
        var CustomerSearch=HttpContext.Session.GetString("CustomerSearch");
        if(CustomerSearch != null){
            query =query +likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY CUSTOMERS.ID ASC";
            customers=GetAllCustomer(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY CUSTOMERS.ID DESC";
            customers=GetAllCustomer(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY CUSTOMERS.NAME ASC";
            customers=GetAllCustomer(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY CUSTOMERS.NAME DESC";
            customers=GetAllCustomer(query);
        }else if(id == "gender_asc"){
            query = query + " ORDER BY CUSTOMERS.GENDER ASC";
            customers=GetAllCustomer(query);
        }else if(id == "gender_desc"){
            query = query + " ORDER BY CUSTOMERS.GENDER DESC";
            customers=GetAllCustomer(query);
        }else if(id == "dateofbirth_asc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH ASC";
            customers=GetAllCustomer(query);
        }else if(id == "dateofbirth_desc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH DESC";
            customers=GetAllCustomer(query);
        }else if(id == "phone_asc"){
            query = query + " ORDER BY CUSTOMERS.PHONE ASC";
            customers=GetAllCustomer(query);
        }else if(id == "phone_desc"){
            query = query + " ORDER BY CUSTOMERS.PHONE DESC";
            customers=GetAllCustomer(query);
        }else if(id == "email_asc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL ASC";
            customers=GetAllCustomer(query);
        }else if(id == "email_desc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL DESC";
            customers=GetAllCustomer(query);
        }else if(id == "address_asc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS ASC";
            customers=GetAllCustomer(query);
        }else if(id == "address_desc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS DESC";
            customers=GetAllCustomer(query);
        }else if(id == "username_asc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            customers=GetAllCustomer(query);
        }else if(id == "username_desc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            customers=GetAllCustomer(query);
        }else if(id == "password_asc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            customers=GetAllCustomer(query);
        }else if(id == "password_desc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            customers=GetAllCustomer(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY CUSTOMERS.STATUS ASC";
            customers=GetAllCustomer(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY CUSTOMERS.STATUS DESC";
            customers=GetAllCustomer(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminCustomer.cshtml", pagedCustomer);
    }
    public IActionResult AdminSortCustomerActive(string id, int page){
        List<Customer> customers_active=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
        string likequery=" AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1)";
        var CustomerSearch=HttpContext.Session.GetString("CustomerActiveSearch");
        if(CustomerSearch != null){
            query =query +likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY CUSTOMERS.ID ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY CUSTOMERS.ID DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY CUSTOMERS.NAME ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY CUSTOMERS.NAME DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "gender_asc"){
            query = query + " ORDER BY CUSTOMERS.GENDER ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "gender_desc"){
            query = query + " ORDER BY CUSTOMERS.GENDER DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "dateofbirth_asc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "dateofbirth_desc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "phone_asc"){
            query = query + " ORDER BY CUSTOMERS.PHONE ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "phone_desc"){
            query = query + " ORDER BY CUSTOMERS.PHONE DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "email_asc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "email_desc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "address_asc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "address_desc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "username_asc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "username_desc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "password_asc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "password_desc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY CUSTOMERS.STATUS ASC";
            customers_active=GetActiveCustomer(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY CUSTOMERS.STATUS DESC";
            customers_active=GetActiveCustomer(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_active.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminCustomerActive.cshtml", pagedCustomer);
    }
    public IActionResult AdminSortCustomerInactive(string id, int page){
        List<Customer> customers_inactive=new List<Customer>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
        string likequery=" AND (CUSTOMERS.NAME like @id or CUSTOMERS.GENDER like @id1 or CUSTOMERS.EMAIL like @id1 or CUSTOMERS.PHONE like @id1 or CUSTOMERS.ID like @id1 or CUSTOMERS.STATUS like @id1)";
        var CustomerSearch=HttpContext.Session.GetString("CustomerInactiveSearch");
        if(CustomerSearch != null){
            query =query +likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY CUSTOMERS.ID ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY CUSTOMERS.ID DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY CUSTOMERS.NAME ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY CUSTOMERS.NAME DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "gender_asc"){
            query = query + " ORDER BY CUSTOMERS.GENDER ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "gender_desc"){
            query = query + " ORDER BY CUSTOMERS.GENDER DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "dateofbirth_asc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "dateofbirth_desc"){
            query = query + " ORDER BY CUSTOMERS.DATEOFBIRTH DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "phone_asc"){
            query = query + " ORDER BY CUSTOMERS.PHONE ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "phone_desc"){
            query = query + " ORDER BY CUSTOMERS.PHONE DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "email_asc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "email_desc"){
            query = query + " ORDER BY CUSTOMERS.EMAIL DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "address_asc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "address_desc"){
            query = query + " ORDER BY CUSTOMERS.ADDRESS DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "username_asc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "username_desc"){
            query = query + " ORDER BY ACCOUNTS.USERNAME DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "password_asc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "password_desc"){
            query = query + " ORDER BY ACCOUNTS.PASSWORD DESC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY CUSTOMERS.STATUS ASC";
            customers_inactive=GetInactiveCustomer(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY CUSTOMERS.STATUS DESC";
            customers_inactive=GetInactiveCustomer(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedCustomer= customers_inactive.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminCustomerInactive.cshtml", pagedCustomer);
    }
    public IActionResult AdminGetCustomer(int page){
        List<Customer> customers=new List<Customer>();
        List<Customer> customers_active=new List<Customer>();
        List<Customer> customers_inactive=new List<Customer>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.ID, CUSTOMERS.NAME, CUSTOMERS.GENDER, CUSTOMERS.DATEOFBIRTH, CUSTOMERS.EMAIL, CUSTOMERS.PHONE, CUSTOMERS.ADDRESS, CUSTOMERS.STATUS,ACCOUNTS.USERNAME, ACCOUNTS.PASSWORD, CUSTOMERS_IMG.IMG FROM ACCOUNTS, CUSTOMERS, CUSTOMERS_IMG WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int cus_status=reader.GetInt32(7);
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                DateTime day=reader.GetDateTime(3);
                Customer customer = new Customer
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    gender=reader.GetString(2),
                    dateofbirth=day.ToString("dd-MM-yyyy"),
                    email = reader.GetString(4),
                    phone=reader.GetString(5),
                    address=reader.GetString(6),
                    status=statusString,
                    img=reader.GetString(10)
                };
                customer.account=new Account();
                customer.account.username=reader.GetString(8);
                customer.account.password=reader.GetString(9);
                if(cus_status==1){
                    Customer customer_active = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Active",
                        img=reader.GetString(10)
                    };
                    customer_active.account=new Account();
                    customer_active.account.username=reader.GetString(8);
                    customer_active.account.password=reader.GetString(9);
                    customers_active.Add(customer_active);
                }else{
                    Customer customer_inactive = new Customer
                    {
                        id = reader.GetInt32(0),
                        name = reader.GetString(1),
                        gender=reader.GetString(2),
                        dateofbirth=day.ToString("dd-MM-yyyy"),
                        email = reader.GetString(4),
                        phone=reader.GetString(5),
                        address=reader.GetString(6),
                        status="Inactive",
                        img=reader.GetString(10)
                    };
                    customer_inactive.account=new Account();
                    customer_inactive.account.username=reader.GetString(8);
                    customer_inactive.account.password=reader.GetString(9);
                    customers_inactive.Add(customer_inactive);
                }
                customers.Add(customer);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =4;
        var pagedCustomer= customers.ToPagedList(pageNumber,pageSize);
        ViewBag.customer_list=pagedCustomer;
        // ViewBag.invoice_list=pagedCustomer;
        return Json(new {customers=pagedCustomer, message=page});
    }
}