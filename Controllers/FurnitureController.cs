using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ZstdSharp.Unsafe;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;

namespace Project.Controllers;

public class FurnitureController : Controller
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
    public IActionResult AdminFurniture(int page)
    {
        HttpContext.Session.Remove("FurnitureSearch");
        List<Furniture> furnitures = new List<Furniture>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, FURNITURE_NAME, TYPE, QUANTITY,PRICE FROM FURNITURE ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    type = reader.GetString(2),
                    quanity = reader.GetInt32(3),
                    price=reader.GetFloat(4)
                };
                furnitures.Add(furniture);
            }
            connection.Close();
        }
        // ViewBag.furniture_list = furnitures;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedFurniture= furnitures.ToPagedList(pageNumber,pageSize);
        ViewBag.furniture_list=pagedFurniture;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminFurniture.cshtml", pagedFurniture);
    }
    public IActionResult AdminAddFurniture()
    {
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddFurniture.cshtml");
    }
    public IActionResult AdminEditFurniture()
    {
        List<Furniture> furnitures = new List<Furniture>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, FURNITURE_NAME, TYPE, QUANTITY FROM FURNITURE ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    type = reader.GetString(2),
                    quanity = reader.GetInt32(3)
                };
                furnitures.Add(furniture);
            }
            connection.Close();
        }
        ViewBag.furniture_list = furnitures;
        return View("~/Views/HotelViews/AdminEditFurniture.cshtml");
    }
    [HttpPost]
    public IActionResult AdminInsertFurniture(Furniture furniture)
    {
        int furniture_id = 1;
        ModelState.Remove("quanity");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddFurniture");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM FURNITURE ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (furniture_id == reader.GetInt32(0))
                {
                    furniture_id = furniture_id + 1;
                }
            }

            reader.Close();
            query = "INSERT INTO FURNITURE (ID,FURNITURE_NAME,TYPE,QUANTITY, PRICE) VALUES(@id,@name,@type,@quantity, @price)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", furniture_id);
            command.Parameters.AddWithValue("@name", furniture.name);
            command.Parameters.AddWithValue("@type", furniture.type);
            command.Parameters.AddWithValue("@quantity", 0);
            command.Parameters.AddWithValue("@price", furniture.price);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminFurniture");
    }
    [HttpPost]
    public IActionResult AdminUpdateFurniture(Furniture furniture)
    {
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditFurniture");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE FURNITURE SET FURNITURE_NAME=@name, QUANTITY=@quantity,TYPE=@type, PRICE=@price WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", furniture.name);
            command.Parameters.AddWithValue("@quantity", furniture.quanity);
            command.Parameters.AddWithValue("@type", furniture.type);
            command.Parameters.AddWithValue("@price", furniture.price);
            command.Parameters.AddWithValue("@id", furniture.id);
            MySqlDataReader reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminFurniture");
    }
    public IActionResult RedirectAdminAddFurniture()
    {
        return RedirectToAction("AdminAddFurniture");
    }
    [HttpPost]
    public IActionResult GetFurniture(string selectedOption)
    {
        string type = "";
        string name = "";
        int quantity = 0;
        string icon="";
        string img="";
        int id=0;
        float price=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY, FURNITURE_IMG.ICON,FURNITURE_IMG.IMG, FURNITURE.PRICE FROM FURNITURE, FURNITURE_IMG WHERE FURNITURE.ID=@id AND FURNITURE_IMG.FURNITURE_ID=FURNITURE.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id=reader.GetInt32(0);
                type = reader.GetString(2);
                quantity = reader.GetInt32(3);
                name = reader.GetString(1);
                icon=reader.GetString(4);
                img=reader.GetString(5);
                price=reader.GetFloat(6);
            }
        }
        return Json(new { type = type, quantity = quantity, name = name, icon=icon, img=img, id=id, price=price });
    }
    public IActionResult EditFurniture(int id)
    {
        List<Furniture> furnitures = new List<Furniture>();
        Furniture furniture1 = new Furniture();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditFurniture");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY, FURNITURE_IMG.ICON, FURNITURE_IMG.IMG FROM FURNITURE, FURNITURE_IMG WHERE FURNITURE.ID=FURNITURE_IMG.FURNITURE_ID ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    type = reader.GetString(2),
                    quanity = reader.GetInt32(3),
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY, FURNITURE_IMG.ICON, FURNITURE_IMG.IMG FROM FURNITURE, FURNITURE_IMG WHERE FURNITURE.ID=FURNITURE_IMG.FURNITURE_ID AND FURNITURE.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                furniture1.id = reader.GetInt32(0);
                furniture1.name = reader.GetString(1);
                furniture1.type = reader.GetString(2);
                furniture1.quanity = reader.GetInt32(3);
                furniture1.icon = reader.GetString(4);
                furniture1.img = reader.GetString(5);
            }
        }
        ViewBag.furniture_list = furnitures;
        return View("~/Views/HotelViews/AdminEditFurniture.cshtml", furniture1);
    }
    public IActionResult DeleteFurniture(int id)
    {
        List<Furniture> furnitures = new List<Furniture>();
        Furniture furniture1 = new Furniture();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM FURNITURE WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                MySqlDataReader reader = command.ExecuteReader();
            }
            catch (Exception)
            {
                return View("~/Views/HotelViews/Error.cshtml");
            }
        }
        ViewBag.furniture_list = furnitures;
        return RedirectToAction("AdminFurniture");
    }
    public IActionResult AdminSearchFurniture(string searchkeyword)
    {
        List<Furniture> furnitures = new List<Furniture>();
        HttpContext.Session.SetString("FurnitureSearch", searchkeyword);
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, FURNITURE_NAME, TYPE, QUANTITY FROM FURNITURE WHERE FURNITURE_NAME LIKE @searchkeyword ORDER BY ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@searchkeyword", "%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    type = reader.GetString(2),
                    quanity = reader.GetInt32(3)
                };
                furnitures.Add(furniture);
            }
        }
        ViewBag.furniture_list = furnitures;
        return View("~/Views/HotelViews/AdminFurniture.cshtml");
    }
    public List<Furniture> GetAllFurniture(string query)
    {
        List<Furniture> furnitures = new List<Furniture>();
        var FurnitureSearch=HttpContext.Session.GetString("FurnitureSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", "%" + FurnitureSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    type = reader.GetString(2),
                    quanity = reader.GetInt32(3)
                };
                furnitures.Add(furniture);
            }
            connection.Close();
        }
        return furnitures;
    }
    public IActionResult AdminSortFurniture(string id){
        List<Furniture> furnitures = new List<Furniture>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ID, FURNITURE_NAME, TYPE, QUANTITY FROM FURNITURE";
        string likequery=" WHERE FURNITURE_NAME LIKE @id";
        var FurnitureSearch=HttpContext.Session.GetString("FurnitureSearch");
        if(FurnitureSearch!=null){
            query=query+likequery;
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ID ASC";
            furnitures=GetAllFurniture(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ID DESC";
            furnitures=GetAllFurniture(query);
        }else if(id == "name_asc"){
            query = query + " ORDER BY FURNITURE_NAME ASC";
            furnitures=GetAllFurniture(query);
        }else if(id == "name_desc"){
            query = query + " ORDER BY FURNITURE_NAME DESC";
            furnitures=GetAllFurniture(query);
        }else if(id == "type_asc"){
            query = query + " ORDER BY TYPE ASC";
            furnitures=GetAllFurniture(query);
        }else if(id == "type_desc"){
            query = query + " ORDER BY TYPE DESC";
            furnitures=GetAllFurniture(query);
        }else if(id == "quantity_asc"){
            query = query + " ORDER BY QUANTITY ASC";
            furnitures=GetAllFurniture(query);
        }else if(id == "quantity_desc"){
            query = query + " ORDER BY QUANTITY DESC";
            furnitures=GetAllFurniture(query);
        }
        ViewBag.furniture_list = furnitures;
        return View("~/Views/HotelViews/AdminFurniture.cshtml");
    }
}