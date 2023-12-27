using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
// using WebApi.Entities;

namespace Project.Controllers;

public class TaxesController : Controller
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
    public IActionResult AdminTaxes(int page){
        List<Taxes> taxes=new List<Taxes>();
        HttpContext.Session.Remove("TaxesSearch");
        HttpContext.Session.Remove("TaxesSearch1");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="Active";
                int cus_status=Convert.ToInt32(reader.GetInt32(4));
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                Taxes tax= new Taxes{
                    id=reader.GetInt32(0),
                    taxes_number=reader.GetString(1),
                    taxes_name=reader.GetString(2),
                    tax_percentage=reader.GetFloat(3),
                    status=statusString
                };
                taxes.Add(tax);
            }
            connection.Close();
        }
        // ViewBag.taxes_list=taxes;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedTaxes= taxes.ToPagedList(pageNumber,pageSize);
        ViewBag.taxes_list=pagedTaxes;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminTaxes.cshtml", pagedTaxes);
    }
    public IActionResult AdminAddTaxes(){
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddTaxes.cshtml");
    }
    public IActionResult AdminEditTaxes(){
        List<Taxes> taxes= new List<Taxes>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM TAXES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Taxes taxes1=new Taxes{
                    id=reader.GetInt32(0)
                };
                taxes.Add(taxes1);
            }
            connection.Close();
        }
        ViewBag.tax_list=taxes;
        return View("~/Views/HotelViews/AdminEditTaxes.cshtml");
    }
    [HttpPost]
    public IActionResult AdminInsertTaxes(Taxes taxes){
        int taxes_id=1;
        int status=1;
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddTaxes");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM TAXES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(taxes_id== reader.GetInt32(0)){
                    taxes_id=taxes_id+1;
                }
            }

            if(taxes.status=="Active"){
                status=1;
            }else{
                status=0;
            }
            string taxes_number= "#"+ taxes_id;
            reader.Close();
            query = "INSERT INTO TAXES (ID,TAXES_NUMBER,TAXES_NAME,TAX_PERCENTAGE,STATUS) VALUES (@id,@taxes_number,@taxes_name,@tax_percentage,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",taxes_id);
            command.Parameters.AddWithValue("@taxes_number",taxes_number);
            command.Parameters.AddWithValue("@taxes_name",taxes.taxes_name);
            command.Parameters.AddWithValue("@tax_percentage",taxes.tax_percentage);
            command.Parameters.AddWithValue("@status",status);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminTaxes");
    }
    [HttpPost]
    public IActionResult AdminUpdateTaxes(Taxes taxes){
        int status=1;
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditTaxes");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            if(taxes.status=="Active"){
                status=1;
            }else{
                status=0;
            }
            string query = "UPDATE TAXES SET TAXES_NAME=@taxes_name, TAX_PERCENTAGE=@tax_percentage, STATUS=@status WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@taxes_name",taxes.taxes_name);
            command.Parameters.AddWithValue("@tax_percentage",taxes.tax_percentage);
            command.Parameters.AddWithValue("@status",status);
            command.Parameters.AddWithValue("@id",taxes.id);
            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminTaxes");
    }
    [HttpPost]
    public IActionResult GetTax(string selectedOption){
        string status="";
        float tax_percentage=0;
        string name="";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT TAXES_NAME,TAX_PERCENTAGE,STATUS FROM TAXES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                name=reader.GetString(0);
                tax_percentage=reader.GetFloat(1);
                if(reader.GetInt32(2)==1){
                    status="Active";
                }else{
                    status="Inactive";
                }
            }
            connection.Close();
        }
        return Json(new {name=name, tax_percentage=tax_percentage, status=status});
    }
    [HttpPost]
    public IActionResult RedirectAdminAddTaxes(){
        return RedirectToAction("AdminAddTaxes");
    }
    public IActionResult EditTaxes(int id){
        List<Taxes> taxes= new List<Taxes>();
        Taxes taxes2 = new Taxes();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditTaxes");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM TAXES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Taxes taxes1=new Taxes{
                    id=reader.GetInt32(0)
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query="SELECT ID,TAXES_NAME,TAX_PERCENTAGE,STATUS FROM TAXES WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                taxes2.id=reader.GetInt32(0);
                taxes2.taxes_name= reader.GetString(1);
                taxes2.tax_percentage= reader.GetFloat(2);
                if(reader.GetInt32(3)==1){
                    taxes2.status= "Active";
                }else{
                    taxes2.status= "Inactive";
                }
                
            }
            connection.Close();
        }
        ViewBag.tax_list=taxes;
        return View("~/Views/HotelViews/AdminEditTaxes.cshtml",taxes2);
    }
     public IActionResult DeleteTaxes(int id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM TAXES WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            try{
                MySqlDataReader reader = command.ExecuteReader();
            }catch(Exception){
                // ViewBag.ModelState.AddModelError("Error", e.Message);
                return View("~/Views/HotelViews/Error.cshtml");
            }
            
            connection.Close();
        }
        return RedirectToAction("AdminTaxes");
    }
    public IActionResult AdminSearchTaxes(string searchkeyword, string searchkeyword1, int page){
        List<Taxes> taxes=new List<Taxes>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("TaxesSearch", searchkeyword);
        }
        if(searchkeyword1 !=null){
            HttpContext.Session.SetString("TaxesSearch1", searchkeyword1);
        }
        var a=HttpContext.Session.GetString("TaxesSearch");
        var b=HttpContext.Session.GetString("TaxesSearch1");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b != null && searchkeyword1 == null){
            searchkeyword1=b;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="";
            if(searchkeyword != null && searchkeyword1 ==null){
                query="SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES WHERE (TAXES_NAME LIKE @id) ORDER BY ID";
            }else if(searchkeyword == null && searchkeyword1 !=null){
                query="SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES WHERE (TAX_PERCENTAGE = @id1) ORDER BY ID";
            }else if(searchkeyword !=null && searchkeyword1 !=null){
                query="SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES WHERE (TAXES_NAME LIKE @id and TAX_PERCENTAGE = @id1) ORDER BY ID";
            }else{
                query = "SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES ORDER BY ID";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1", searchkeyword1);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="Active";
                int cus_status=Convert.ToInt32(reader.GetInt32(4));
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                Taxes tax= new Taxes{
                    id=reader.GetInt32(0),
                    taxes_number=reader.GetString(1),
                    taxes_name=reader.GetString(2),
                    tax_percentage=reader.GetFloat(3),
                    status=statusString
                };
                taxes.Add(tax);
            }
            connection.Close();
        }
        // ViewBag.taxes_list=taxes;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedTaxes= taxes.ToPagedList(pageNumber,pageSize);
        ViewBag.taxes_list=pagedTaxes;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminTaxes.cshtml", pagedTaxes);
    }
    public List<Taxes> GetAllTaxes(string query){
        List<Taxes> taxes=new List<Taxes>();
        var searchkeyword = HttpContext.Session.GetString("TaxesSearch");
        var searchkeyword1 = HttpContext.Session.GetString("TaxesSearch1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + searchkeyword + "%");
            command.Parameters.AddWithValue("@id1", searchkeyword1);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="Active";
                int cus_status=Convert.ToInt32(reader.GetInt32(4));
                if(cus_status==1){
                    statusString="Active";
                }else{
                    statusString="Inactive";
                }
                Taxes tax= new Taxes{
                    id=reader.GetInt32(0),
                    taxes_number=reader.GetString(1),
                    taxes_name=reader.GetString(2),
                    tax_percentage=reader.GetFloat(3),
                    status=statusString
                };
                taxes.Add(tax);
            }
            connection.Close();
        }
        return taxes;
    }
    public IActionResult AdminSortTaxes(string id, int page){
        List<Taxes> taxes=new List<Taxes>();
        ViewBag.employee_avatar=GetAvatar();
        string query ="SELECT ID, TAXES_NUMBER, TAXES_NAME, TAX_PERCENTAGE, STATUS FROM TAXES";
        var searchkeyword = HttpContext.Session.GetString("TaxesSearch");
        var searchkeyword1 = HttpContext.Session.GetString("TaxesSearch1");
        if(searchkeyword != null && searchkeyword1 ==null){
            query=query + " WHERE (TAXES_NAME LIKE @id)";
        }else if(searchkeyword == null && searchkeyword1 !=null){
            query=query + " WHERE (TAX_PERCENTAGE = @id1)";
        }else if(searchkeyword !=null && searchkeyword1 !=null){
            query=query + " WHERE (TAXES_NAME LIKE @id and TAX_PERCENTAGE = @id1)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ID ASC";
            taxes=GetAllTaxes(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ID DESC";
            taxes=GetAllTaxes(query);
        }else if(id == "number_asc"){
            query = query + " ORDER BY TAXES_NUMBER ASC";
            taxes=GetAllTaxes(query);
        }else if(id == "number_desc"){
            query = query + " ORDER BY TAXES_NUMBER DESC";
            taxes=GetAllTaxes(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY TAXES_NAME ASC";
            taxes=GetAllTaxes(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY TAXES_NAME DESC";
            taxes=GetAllTaxes(query);
        }else if(id == "percent_asc"){
            query = query + " ORDER BY TAX_PERCENTAGE ASC";
            taxes=GetAllTaxes(query);
        }else if(id == "percent_desc"){
            query = query + " ORDER BY TAX_PERCENTAGE DESC";
            taxes=GetAllTaxes(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY STATUS ASC";
            taxes=GetAllTaxes(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY STATUS DESC";
            taxes=GetAllTaxes(query);
        }
        // ViewBag.taxes_list=taxes;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedTaxes= taxes.ToPagedList(pageNumber,pageSize);
        ViewBag.taxes_list=pagedTaxes;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminTaxes.cshtml", pagedTaxes);
    }
}

