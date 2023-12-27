using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Project.Controllers;

public class LoginController : Controller
{
    // private readonly IHttpContextAccessor? contx;
    private readonly string connectionString = DataConnection.Connection;
    public IActionResult Login()
    {
        ViewBag.status1=TempData["status1"];
        return View("~/Views/HotelViews/Login.cshtml");
    }
    [HttpPost]
    public IActionResult SignIn(Account account)
    {
        string type = "";
        int id=1;
        int user_id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT TYPE, ID FROM ACCOUNTS WHERE USERNAME = @username and PASSWORD=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", account.username);
            command.Parameters.AddWithValue("@password", account.password);
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    type = reader.GetString(0);
                }

                reader.Close();
                if (type == "employee"){
                    query="SELECT EMPLOYEES.ID FROM EMPLOYEES WHERE ACCOUNT_ID=@id";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);
                    reader = command.ExecuteReader();
                    while(reader.Read()){
                        user_id=reader.GetInt32(0);
                    }
                    if(account!=null){
                        HttpContext?.Session.SetString("username",account?.username!);
                        HttpContext?.Session.SetString("password",account?.password!);
                    }
                    // Session["Username"]
                }
                else{
                    query="SELECT CUSTOMERS.ID FROM CUSTOMERS WHERE ACCOUNT_ID=@id";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);
                    reader = command.ExecuteReader();
                    while(reader.Read()){
                        user_id=reader.GetInt32(0);
                    }
                    if(account!=null){
                        HttpContext?.Session.SetString("username1",account?.username!);
                        HttpContext?.Session.SetString("password1",account?.password!);
                    }
                }
            }
            else
            {
                return RedirectToAction("Login");
            }

        }
        if (type == "employee")
        {
            return RedirectToAction("AdminDashBoard", "DashBoard");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }

    }
    public IActionResult SignOut1(){
        HttpContext.Session.Remove("username");
        HttpContext.Session.Remove("username1");
        HttpContext.Session.Remove("password");
        HttpContext.Session.Remove("password1");
        return RedirectToAction("Login");
    }
    [HttpPost]
    public IActionResult SignUp(Account account){
        int id=1;
        if(account.password != account.confirmpassword){
            TempData["status1"]="Password and Confirm password must be the same";
            return RedirectToAction("Login");
        }
        int exist=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, USERNAME FROM ACCOUNTS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(id == reader.GetInt32(0)){
                    id=id+1;
                }
                if(account.username == reader.GetString(1)){
                    exist=1;
                    
                }
            }
            if(exist ==1){
                TempData["status1"]="Username is already existed";
                return RedirectToAction("Login");
            }

            reader.Close();
            query = "INSERT INTO ACCOUNTS (ID, USERNAME, PASSWORD,TYPE,STATUS) VALUES (@id, @username,@password,@type,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            command.Parameters.AddWithValue("@username",account.username);
            command.Parameters.AddWithValue("@password",account.password);
            command.Parameters.AddWithValue("@type","customer");
            command.Parameters.AddWithValue("@status",1);
            reader = command.ExecuteReader();
            connection.Close();
        }
        return RedirectToAction("EditAccount1", "Home");
    }
}