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

public class ChartController : Controller
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
    public IActionResult AdminBarChart()
    {
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        List<Chart> charts5 = new List<Chart>();
        List<Chart> charts6 = new List<Chart>();
        List<Chart> charts7 = new List<Chart>();
        List<Chart> charts8 = new List<Chart>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, SUM(PAYMENT.AMOUNT) FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts1.Add(chart1);
            }
            reader.Close();
            query = "SELECT distinct dayofweek(INVOICES_DETAILS.CREATE_DATE) AS Day_Of_Week, sum(PAYMENT.AMOUNT) AS TOTAL FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and INVOICES_DETAILS.CREATE_DATE >= (current_date() - interval 7 day) GROUP BY Day_Of_Week ORDER BY Day_Of_Week ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts2.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts3.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and CUSTOMERS.ID=PAYMENT.CUSTOMER_ID and CUSTOMERS.VISIT >1 GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts4.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.TYPE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.TYPE ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    type = reader.GetString(0),
                    quantity = reader.GetInt32(1)
                };
                charts5.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.PRICE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.PRICE";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    price = reader.GetFloat(0),
                    quantity = reader.GetInt32(1)
                };
                charts6.Add(chart1);
            }
            reader.Close();
            query = "SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=1 ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    quantity = reader.GetInt32(0),
                };
                charts7.Add(chart1);
            }
            reader.Close();
            query = "SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=0 ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    quantity = reader.GetInt32(0),
                };
                charts8.Add(chart1);
            }
            connection.Close();
        }
        ViewBag.barchart_lists = new List<List<Chart>>(){
            charts1,
            charts2,
            charts3,
            charts4,
            charts5,
            charts6,
            charts7,
            charts8
        };
        return View("~/Views/HotelViews/AdminBarChart.cshtml");
    }
    public IActionResult AdminDonutChart()
    {
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        List<Chart> charts5 = new List<Chart>();
        List<Chart> charts6 = new List<Chart>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, SUM(PAYMENT.AMOUNT) FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts1.Add(chart1);
            }
            reader.Close();
            query = "SELECT distinct dayofweek(INVOICES_DETAILS.CREATE_DATE) AS Day_Of_Week, sum(PAYMENT.AMOUNT) AS TOTAL FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and INVOICES_DETAILS.CREATE_DATE >= (current_date() - interval 7 day) GROUP BY Day_Of_Week ORDER BY Day_Of_Week ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts2.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts3.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and CUSTOMERS.ID=PAYMENT.CUSTOMER_ID and CUSTOMERS.VISIT >1 GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts4.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.TYPE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.TYPE ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    type = reader.GetString(0),
                    quantity = reader.GetInt32(1)
                };
                charts5.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.PRICE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.PRICE";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    price = reader.GetFloat(0),
                    quantity = reader.GetInt32(1)
                };
                charts6.Add(chart1);
            }
            connection.Close();
        }
        ViewBag.donutchart_lists = new List<List<Chart>>(){
            charts1,
            charts2,
            charts3,
            charts4,
            charts5,
            charts6
        };
        return View("~/Views/HotelViews/AdminDonutChart.cshtml");
    }
    public IActionResult AdminLineChart()
    {
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        List<Chart> charts5 = new List<Chart>();
        List<Chart> charts6 = new List<Chart>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, SUM(PAYMENT.AMOUNT) FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts1.Add(chart1);
            }
            reader.Close();
            query = "SELECT distinct dayofweek(INVOICES_DETAILS.CREATE_DATE) AS Day_Of_Week, sum(PAYMENT.AMOUNT) AS TOTAL FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and INVOICES_DETAILS.CREATE_DATE >= (current_date() - interval 7 day) GROUP BY Day_Of_Week ORDER BY Day_Of_Week ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts2.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts3.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and CUSTOMERS.ID=PAYMENT.CUSTOMER_ID and CUSTOMERS.VISIT >1 GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts4.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.TYPE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.TYPE ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    type = reader.GetString(0),
                    quantity = reader.GetInt32(1)
                };
                charts5.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.PRICE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.PRICE";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    price = reader.GetFloat(0),
                    quantity = reader.GetInt32(1)
                };
                charts6.Add(chart1);
            }
            connection.Close();
        }
        ViewBag.linechart_lists = new List<List<Chart>>(){
            charts1,
            charts2,
            charts3,
            charts4,
            charts5,
            charts6
        };
        return View("~/Views/HotelViews/AdminLineChart.cshtml");
    }
    public IActionResult AdminMixedChart()
    {
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND date_format(INVOICES_DETAILS.CREATE_DATE, '%Y')=2023  GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts1.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and CUSTOMERS.ID=PAYMENT.CUSTOMER_ID and CUSTOMERS.VISIT >1 AND date_format(INVOICES_DETAILS.CREATE_DATE, '%Y')=2023  GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts2.Add(chart1);
            }
            reader.Close();
            query = "SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=1 ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    quantity = reader.GetInt32(0),
                };
                charts3.Add(chart1);
            }
            reader.Close();
            query = "SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE ROOMS.STATUS=0 ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    quantity = reader.GetInt32(0),
                };
                charts4.Add(chart1);
            }
            connection.Close();
        }
        ViewBag.mixedchart_lists = new List<List<Chart>>(){
            charts1,
            charts2,
            charts3,
            charts4
        };
        return View("~/Views/HotelViews/AdminMixedChart.cshtml");
    }
    public IActionResult AdminPieChart()
    {
        List<Chart> charts1 = new List<Chart>();
        List<Chart> charts2 = new List<Chart>();
        List<Chart> charts3 = new List<Chart>();
        List<Chart> charts4 = new List<Chart>();
        List<Chart> charts5 = new List<Chart>();
        List<Chart> charts6 = new List<Chart>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, SUM(PAYMENT.AMOUNT) FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts1.Add(chart1);
            }
            reader.Close();
            query = "SELECT distinct dayofweek(INVOICES_DETAILS.CREATE_DATE) AS Day_Of_Week, sum(PAYMENT.AMOUNT) AS TOTAL FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and INVOICES_DETAILS.CREATE_DATE >= (current_date() - interval 7 day) GROUP BY Day_Of_Week ORDER BY Day_Of_Week ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts2.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts3.Add(chart1);
            }
            reader.Close();
            query = "SELECT date_format(INVOICES_DETAILS.CREATE_DATE, '%m') AS MONTH, count(PAYMENT.CUSTOMER_ID) AS CUSTOMER FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and CUSTOMERS.ID=PAYMENT.CUSTOMER_ID and CUSTOMERS.VISIT >1 GROUP BY date_format(INVOICES_DETAILS.CREATE_DATE, '%m') ORDER BY MONTH ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    month = reader.GetInt32(0),
                    price = reader.GetFloat(1)
                };
                charts4.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.TYPE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.TYPE ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    type = reader.GetString(0),
                    quantity = reader.GetInt32(1)
                };
                charts5.Add(chart1);
            }
            reader.Close();
            query = "SELECT DISTINCT ROOMS.PRICE ,COUNT(INVOICES_DETAILS.BOOKING_ID) as NUMBER FROM INVOICES, INVOICES_DETAILS, BOOKINGS, ROOMS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID GROUP BY ROOMS.PRICE";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Chart chart1 = new Chart
                {
                    price = reader.GetFloat(0),
                    quantity = reader.GetInt32(1)
                };
                charts6.Add(chart1);
            }
            connection.Close();
        }
        ViewBag.piechart_lists = new List<List<Chart>>(){
            charts1,
            charts2,
            charts3,
            charts4,
            charts5,
            charts6
        };
        return View("~/Views/HotelViews/AdminPieChart.cshtml");
    }
}