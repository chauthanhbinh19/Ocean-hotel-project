using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;

namespace Project.Controllers;

public class ServiceController:Controller
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
    public IActionResult AdminServices(int page){
        List<Service> services= new List<Service>();
        HttpContext.Session.Remove("ServiceSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM services WHERE TYPE=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","service");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        // ViewBag.service_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.service_list=pagedService;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminServices.cshtml",pagedService);
    }
    public IActionResult AdminShuttle(int page){
        List<Service> services= new List<Service>();
        HttpContext.Session.Remove("ShuttleSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM services where type='shuttle'";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        // ViewBag.shuttle_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.shuttle_list=pagedService;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminShuttle.cshtml", pagedService);
    }
    public IActionResult AdminAddService(){
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddService.cshtml");
    }
    public IActionResult AdminEditService(){
        List<Service> services= new List<Service>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM SERVICES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service= new Service{
                    id= reader.GetInt32(0)
                };
                services.Add(service);
            }
            connection.Close();
        }
        ViewBag.service_list=services;
        return View("~/Views/HotelViews/AdminEditService.cshtml");
    }
    public IActionResult AdminInsertService(Service service){
        int id=1;
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddService");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM SERVICES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(id == reader.GetInt32(0)){
                    id=id+1;
                }
            }

            reader.Close();
            query = "INSERT INTO SERVICES (ID, NAME,DESCRIPTION, PRICE, TYPE) VALUES(@id, @name, @description, @price, @type)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", service.name);
            command.Parameters.AddWithValue("@description", service.description);
            command.Parameters.AddWithValue("@price", service.price);
            command.Parameters.AddWithValue("@type", service.shuttle);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        if(service.shuttle == "service"){
            return RedirectToAction("AdminServices");
        }else{
            return RedirectToAction("AdminShuttle");
        }
    }
    public IActionResult AdminUpdateService(Service service){
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditService");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE SERVICES SET NAME=@name, DESCRIPTION=@description, PRICE=@price,TYPE=@type WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", service.name);
            command.Parameters.AddWithValue("@description", service.description);
            command.Parameters.AddWithValue("@price", service.price);
            command.Parameters.AddWithValue("@type", service.shuttle);
            command.Parameters.AddWithValue("@id", service.id);
            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        if(service.shuttle == "service"){
            return RedirectToAction("AdminServices");
        }else{
            return RedirectToAction("AdminShuttle");
        }
    }
    public IActionResult RedirectAdminAddService(){
        return RedirectToAction("AdminAddService");
    }
    public IActionResult GetService(string selectedOption){
        string name ="";
        string description="";
        float price=0;
        string type="";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME, DESCRIPTION, PRICE, TYPE FROM SERVICES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption );
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                name=reader.GetString(0);
                description=reader.GetString(1);
                price=reader.GetFloat(2);
                type=reader.GetString(3);
            }
            connection.Close();
        }
        return Json(new {name=name, description=description, price=price, type=type});
    }
    public IActionResult EditService(int id){
        Service services= new Service();
        ViewBag.employee_avatar=GetAvatar();
        List<Service> services1= new List<Service>();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditService");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, NAME, DESCRIPTION, PRICE, TYPE FROM SERVICES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                services.id= reader.GetInt32(0);
                services.name=reader.GetString(1);
                services.description=reader.GetString(2);
                services.price=reader.GetFloat(3);
                services.shuttle=reader.GetString(4);
            }

            reader.Close();
            query = "SELECT ID FROM SERVICES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Service service= new Service{
                    id= reader.GetInt32(0)
                };
                services1.Add(service);
            }
            connection.Close();
        }
        ViewBag.service_list=services1;
        return View("~/Views/HotelViews/AdminEditService.cshtml", services);
    }
    public IActionResult DeleteService(int id){
        Service services= new Service();
        List<Service> services1= new List<Service>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try{
                connection.Open();
                string query = "DELETE FROM SERVICES WHERE ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();
                if(rowsAffected ==0){
                    throw new Exception("Foreign key constraint violation");
                }
            }catch(Exception){
                return View("~/Views/HotelViews/Error.cshtml");
            }
        }
        return RedirectToAction("AdminServices");
    }
    public IActionResult AdminSearchService(string searchkeyword, int page){
        ViewBag.employee_avatar=GetAvatar();
        List<Service> services= new List<Service>();
        if(searchkeyword != null ){
            HttpContext.Session.SetString("ServiceSearch",searchkeyword);
        }
        var a=HttpContext.Session.GetString("ServiceSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM services WHERE (NAME like @id OR PRICE like @id) and TYPE like 'service'";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        // ViewBag.service_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.service_list=pagedService;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminServices.cshtml", pagedService);
    }
    public IActionResult AdminSearchShuttle(string searchkeyword, int page){
        ViewBag.employee_avatar=GetAvatar();
        List<Service> services= new List<Service>();
        if(searchkeyword != null ){
            HttpContext.Session.SetString("ShuttleSearch",searchkeyword);
        }
        var a=HttpContext.Session.GetString("ShuttleSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM services WHERE (NAME like @id OR PRICE like @id) and TYPE like 'shuttle'";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        // ViewBag.service_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.shuttle_list=pagedService;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminShuttle.cshtml", pagedService);
    }
    public List<Service> GetAllServices(string query){
        List<Service> services= new List<Service>();
        var ServiceSearch=HttpContext.Session.GetString("ServiceSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + ServiceSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        return services;
    }
    public List<Service> GetAllShuttle(string query){
        List<Service> services= new List<Service>();
        var ServiceSearch=HttpContext.Session.GetString("ServiceSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + ServiceSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service service =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                };
                services.Add(service);
            }
            connection.Close();
        }
        return services;
    }
    public IActionResult AdminSortService(string id, int page){
        List<Service> services= new List<Service>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT * FROM services WHERE TYPE LIKE 'service'" ;
        string likequery=" and (NAME like @id OR PRICE like @id)";
        var ServiceSearch=HttpContext.Session.GetString("ServiceSearch");
        if(ServiceSearch !=null){
            query =query + likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ID ASC";
            services=GetAllServices(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ID DESC";
            services=GetAllServices(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY NAME ASC";
            services=GetAllServices(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY NAME DESC";
            services=GetAllServices(query);
        }else if(id == "des_asc"){
            query = query + " ORDER BY DESCRIPTION ASC";
            services=GetAllServices(query);
        }else if(id == "des_desc"){
            query = query + " ORDER BY DESCRIPTION DESC";
            services=GetAllServices(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY PRICE ASC";
            services=GetAllServices(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY PRICE DESC";
            services=GetAllServices(query);
        }
        // ViewBag.service_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.service_list=pagedService;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminServices.cshtml", pagedService);
    }
    public IActionResult AdminSortShuttle(string id, int page){
        List<Service> services= new List<Service>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT * FROM services WHERE TYPE LIKE 'shuttle'" ;
        string likequery=" and (NAME like @id OR PRICE like @id)";
        var ServiceSearch=HttpContext.Session.GetString("ShuttleSearch");
        if(ServiceSearch !=null){
            query =query + likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ID ASC";
            services=GetAllShuttle(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ID DESC";
            services=GetAllShuttle(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY NAME ASC";
            services=GetAllShuttle(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY NAME DESC";
            services=GetAllShuttle(query);
        }else if(id == "des_asc"){
            query = query + " ORDER BY DESCRIPTION ASC";
            services=GetAllShuttle(query);
        }else if(id == "des_desc"){
            query = query + " ORDER BY DESCRIPTION DESC";
            services=GetAllShuttle(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY PRICE ASC";
            services=GetAllShuttle(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY PRICE DESC";
            services=GetAllShuttle(query);
        }
        // ViewBag.shuttle_list=services;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedService= services.ToPagedList(pageNumber,pageSize);
        ViewBag.service_list=pagedService;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminShuttle.cshtml", pagedService);
    }
}