using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
// using WebApi.Entities;

namespace Project.Controllers;

public class PromotionController : Controller
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
    public IActionResult AdminPricing(){
        List<Promotion> promotions= new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME, DESCRIPTION, DISCOUNT_PERCENT, VALID_FROM, VALID_TO, ID FROM PROMOTIONS";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(3);
                DateTime day1=reader.GetDateTime(4);
                Promotion promotion= new Promotion{
                    name=reader.GetString(0),
                    description=reader.GetString(1),
                    discount_percent=reader.GetFloat(2),
                    valid_from=day.ToString("dd-MM-yyyy"),
                    valid_to=day1.ToString("dd-MM-yyyy"),
                    id=reader.GetInt32(5)
                };
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.promotion_list= promotions;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminPricing.cshtml");
    }
    public IActionResult AdminAddPromotion(){
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminAddPromotion.cshtml");
    }
    public IActionResult AdminEditPromotion(){
        ViewBag.employee_avatar=GetAvatar();
        return View("~/Views/HotelViews/AdminEditPromotion.cshtml");
    }
    [HttpPost]
    public IActionResult RedirectAdminAddPromotion(){
        return RedirectToAction("AdminAddPromotion");
    }
    [HttpPost]
    public IActionResult AdminInsertPromotion(Promotion promotion){
        int promotion_id=1;
        ModelState.Remove("customer.name");
        ModelState.Remove("customer.gender");
        ModelState.Remove("customer.dateofbirth");
        ModelState.Remove("customer.email");
        ModelState.Remove("customer.phone");
        ModelState.Remove("customer.address");
        ModelState.Remove("customer.status");
        ModelState.Remove("room.name");
        ModelState.Remove("room.type");
        ModelState.Remove("room.floor");
        ModelState.Remove("room.size");
        ModelState.Remove("room.status");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddPromotion");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM PROMOTIONS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(promotion_id==reader.GetInt32(0)){
                    promotion_id=promotion_id+1;
                }
            }

            reader.Close();
            query = "INSERT INTO PROMOTIONS (ID,NAME,DESCRIPTION,DISCOUNT_PERCENT,VALID_FROM,VALID_TO) VALUES(@id,@name,@description,@discount_percent,@valid_from,@valid_to)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",promotion_id);
            command.Parameters.AddWithValue("@name",promotion.name);
            command.Parameters.AddWithValue("@description",promotion.description);
            command.Parameters.AddWithValue("@discount_percent",promotion.discount_percent);
            command.Parameters.AddWithValue("@valid_from",promotion.valid_from);
            command.Parameters.AddWithValue("@valid_to",promotion.valid_to);
            reader = command.ExecuteReader();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminPricing");
    }
    [HttpPost]
    public IActionResult AdminUpdatePromotion(Promotion promotion){
        ModelState.Remove("customer.name");
        ModelState.Remove("customer.gender");
        ModelState.Remove("customer.dateofbirth");
        ModelState.Remove("customer.email");
        ModelState.Remove("customer.phone");
        ModelState.Remove("customer.address");
        ModelState.Remove("customer.status");
        ModelState.Remove("room.name");
        ModelState.Remove("room.type");
        ModelState.Remove("room.floor");
        ModelState.Remove("room.size");
        ModelState.Remove("room.status");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditPromotion");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE PROMOTIONS SET NAME=@name, DESCRIPTION=@description, DISCOUNT_PERCENT=@discount_percent,VALID_FROM=@valid_from,VALID_TO=@valid_to WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",promotion.name);
            command.Parameters.AddWithValue("@description",promotion.description);
            command.Parameters.AddWithValue("@discount_percent",promotion.discount_percent);
            command.Parameters.AddWithValue("@valid_from",promotion.valid_from);
            command.Parameters.AddWithValue("@valid_to",promotion.valid_to);
            command.Parameters.AddWithValue("@id",promotion.id);
            MySqlDataReader reader = command.ExecuteReader();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminPricing");
    }
    public IActionResult DeletePromotion(int id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM PROMOTIONS WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
        }
        return RedirectToAction("AdminPricing");
    }
    public IActionResult EditPromotion(int id){
        Promotion promotions= new Promotion();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditPromotion");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME, DESCRIPTION, DISCOUNT_PERCENT, VALID_FROM, VALID_TO, ID FROM PROMOTIONS WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(3);
                DateTime day1=reader.GetDateTime(4);
                promotions.name=reader.GetString(0);
                promotions.description=reader.GetString(1);
                promotions.discount_percent=reader.GetFloat(2);
                promotions.valid_from=day.ToString("yyyy-MM-dd");
                promotions.valid_to=day1.ToString("yyyy-MM-dd");
                promotions.id=reader.GetInt32(5);
            }
            connection.Close();
        }
        return View("~/Views/HotelViews/AdminEditPromotion.cshtml", promotions);
    }
}

