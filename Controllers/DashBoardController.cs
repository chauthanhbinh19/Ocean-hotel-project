using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using ZstdSharp.Unsafe;

namespace Project.Controllers;

public class DashBoardController:Controller
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
    public IActionResult AdminDashBoard(){
        int check_in_date=0;
        int check_out_date=0;
        int room_available=0;
        int room_booked=0;
        int room_all=0;
        ViewBag.employee_avatar=GetAvatar();
        List<Customer> customers=new List<Customer>();
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT COUNT(BOOKINGS.ID) FROM BOOKINGS WHERE CHECK_IN_DATE =current_date()";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                check_in_date=reader.GetInt32(0);
            }

            reader.Close();
            query="SELECT COUNT(BOOKINGS.ID) FROM BOOKINGS WHERE CHECK_OUT_DATE =current_date()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                check_out_date=reader.GetInt32(0);
            }

            reader.Close();
            query="SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=1";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                room_available=reader.GetInt32(0);
            }

            reader.Close();
            query="SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=0";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                room_booked=reader.GetInt32(0);
            }

            reader.Close();
            query="SELECT COUNT(ROOMS.ID) FROM ROOMS";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                room_all=reader.GetInt32(0);
            }

            reader.Close();
            query="SELECT CUSTOMERS.NAME, REVIEWS.COMMENT, REVIEWS.RATING, CUSTOMERS_IMG.IMG FROM CUSTOMERS, REVIEWS, CUSTOMERS_IMG WHERE CUSTOMERS.ID=REVIEWS.CUSTOMER_ID AND CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID ORDER BY RAND() LIMIT 3";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer=new Customer{
                    name=reader.GetString(0),
                    img=reader.GetString(3)
                };
                customer.review!.comment=reader.GetString(1);
                customer.review!.rate=reader.GetInt32(2);
                customers.Add(customer);
            }

            for(int i=0;i<12;i++){
                Chart chart1 = new Chart
                {
                    month=i+1,
                    price=0
                };
                charts3.Add(chart1);
                Chart chart2 = new Chart
                {
                    month=i+1,
                    price=0
                };
                charts4.Add(chart2);
            }
            //CHECK IN
            reader.Close();
            query="SELECT DATE_FORMAT(CHECK_IN_DATE,'%m') AS MONTH, count(BOOKINGS.ID) FROM BOOKINGS WHERE CHECK_IN_DATE <= NOW() AND CHECK_OUT_DATE >= NOW() group by MONTH ORDER BY MONTH ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                for(int i=0; i<12;i++){
                    if(charts3[i].month ==reader.GetInt32(0)){
                        charts3[i].price =reader.GetFloat(1);
                    }
                }
            }
            //CHECKOUT
            reader.Close();
            query="SELECT DATE_FORMAT(CHECK_IN_DATE,'%m') AS MONTH, count(BOOKINGS.ID) FROM BOOKINGS WHERE CHECK_OUT_DATE < NOW() group by MONTH ORDER BY MONTH ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                for(int i=0; i<12;i++){
                    if(charts4[i].month ==reader.GetInt32(0)){
                        charts4[i].price =reader.GetFloat(1);
                    }
                }
            }
            
            connection.Close();
        }
        ViewBag.check_in_date=check_in_date;
        ViewBag.check_out_date=check_out_date;
        ViewBag.room_available=room_available;
        ViewBag.room_booked=room_booked;
        ViewBag.room_all=room_all;
        ViewBag.available=room_available*100/room_all;
        ViewBag.booked=room_booked*100/room_all;
        ViewBag.customer_list=customers;
        ViewBag.mixedchart_lists = new List<List<Chart>>(){
            charts3,
            charts4
        };
        return View("~/Views/HotelViews/AdminDashBoard.cshtml");
    }
    public IActionResult AdminDashBoard1(){
        string? a= HttpContext!.Session!.GetString("username");
        string? b= HttpContext!.Session!.GetString("password");
        return View("~/Views/HotelViews/AdminDashBoard.cshtml");
    }
    public IActionResult AdminInvoice(){
        return View("~/Views/HotelViews/Invoice.cshtml");
    }
    public IActionResult AdminTest(int page){
        List<Taxes> taxes=new List<Taxes>();
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

        ViewBag.taxes_list=taxes;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =1;
        var pagedTaxes= taxes.ToPagedList(pageNumber,pageSize);
        ViewBag.taxes_list=pagedTaxes;
        return View("~/Views/HotelViews/AdminTaxesTest.cshtml",pagedTaxes);
    }
}