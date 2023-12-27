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

public class PaymentController : Controller
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
    public IActionResult AdminPayment(int page){
        List<Payment> payments=new List<Payment>();
        HttpContext.Session.Remove("PaymentSearch");
        HttpContext.Session.Remove("PaymentSearch1");
        HttpContext.Session.Remove("PaymentSearch2");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT, PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID order by PAYMENT.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(5);
                Payment payment= new Payment{
                    id=reader.GetInt32(0),
                    invoice_id=reader.GetInt32(1),
                    customer_name=reader.GetString(2),
                    payment_type=reader.GetString(3),
                    amount=reader.GetFloat(4),
                    payment_date=day.ToString("dd-MM-yyyy")
                };
                payments.Add(payment);
            }
            connection.Close();
        }
        // ViewBag.payment_list=payments;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedPayment= payments.ToPagedList(pageNumber,pageSize);
        ViewBag.payment_list=pagedPayment;
        return View("~/Views/HotelViews/AdminPayment.cshtml", pagedPayment);
    }
    public IActionResult AdminSearchPayment(string searchkeyword, string searchkeyword1, string searchkeywor2, int page){
        ViewBag.employee_avatar=GetAvatar();
        List<Payment> payments=new List<Payment>();
        if(searchkeyword != null){
            HttpContext.Session.SetString("PaymentSearch", searchkeyword);
        }
        if(searchkeyword1 !=null){
            HttpContext.Session.SetString("PaymentSearch1", searchkeyword1);
        }
        if(searchkeywor2 != null){
            HttpContext.Session.SetString("PaymentSearch2", searchkeywor2);
        }
        var a=HttpContext.Session.GetString("PaymentSearch");
        var b=HttpContext.Session.GetString("PaymentSearch1");
        var c=HttpContext.Session.GetString("PaymentSearch2");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b != null && searchkeyword1 == null){
            searchkeyword1=b;
        }
        if(c != null && searchkeywor2 ==null){
            searchkeywor2=c;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="";
            if(searchkeyword != null && searchkeyword1 == null && searchkeywor2 ==null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (INVOICES.ID like @id) order by PAYMENT.ID ASC"; 
            }else if(searchkeyword == null && searchkeyword1 != null && searchkeywor2 ==null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (CUSTOMERS.NAME like @id1) order by PAYMENT.ID ASC";
            }else if(searchkeyword == null && searchkeyword1 == null && searchkeywor2 !=null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (PAYMENT.PAYMENT_DATE like @id2) order by PAYMENT.ID ASC";
            }else if(searchkeyword != null && searchkeyword1 != null && searchkeywor2 ==null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (INVOICES.ID like @id AND CUSTOMERS.NAME like @id1) order by PAYMENT.ID ASC";
            }else if(searchkeyword != null && searchkeyword1 == null && searchkeywor2 !=null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (INVOICES.ID like @id AND PAYMENT.PAYMENT_DATE like @id2) order by PAYMENT.ID ASC";
            }else if(searchkeyword == null && searchkeyword1 != null && searchkeywor2 !=null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (CUSTOMERS.NAME like @id1 AND PAYMENT.PAYMENT_DATE like @id2) order by PAYMENT.ID ASC";
            }else if(searchkeyword != null && searchkeyword1 != null && searchkeywor2 !=null){
                query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT , PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID AND (INVOICES.ID like @id AND CUSTOMERS.NAME like @id1 AND PAYMENT.PAYMENT_DATE like @id2) order by PAYMENT.ID ASC";
            }else{
                query = "SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT, PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID order by PAYMENT.ID ASC";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1","%" + searchkeyword1 + "%");
            command.Parameters.AddWithValue("@id2",searchkeywor2);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(5);
                Payment payment= new Payment{
                    id=reader.GetInt32(0),
                    invoice_id=reader.GetInt32(1),
                    customer_name=reader.GetString(2),
                    payment_type=reader.GetString(3),
                    amount=reader.GetFloat(4),
                    payment_date=day.ToString("dd-MM-yyyy")
                };
                payments.Add(payment);
            }
            connection.Close();
        }
        // ViewBag.payment_list=payments;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedPayment= payments.ToPagedList(pageNumber,pageSize);
        ViewBag.payment_list=pagedPayment;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminPayment.cshtml", pagedPayment);
    }
    public List<Payment> GetAllPayment(string query){
        List<Payment> payments=new List<Payment>();
        var searchkeyword=HttpContext.Session.GetString("PaymentSearch");
        var searchkeyword1=HttpContext.Session.GetString("PaymentSearch1");
        var searchkeywor2=HttpContext.Session.GetString("PaymentSearch2");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1","%" + searchkeyword1 + "%");
            command.Parameters.AddWithValue("@id2",searchkeywor2);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(5);
                Payment payment= new Payment{
                    id=reader.GetInt32(0),
                    invoice_id=reader.GetInt32(1),
                    customer_name=reader.GetString(2),
                    payment_type=reader.GetString(3),
                    amount=reader.GetFloat(4),
                    payment_date=day.ToString("dd-MM-yyyy")
                };
                payments.Add(payment);
            }
            connection.Close();
        }
        return payments;
    }
    public IActionResult AdminSortPayment(string id, int page){
        List<Payment> payments=new List<Payment>();
        ViewBag.employee_avatar=GetAvatar();
        var searchkeyword=HttpContext.Session.GetString("PaymentSearch");
        var searchkeyword1=HttpContext.Session.GetString("PaymentSearch1");
        var searchkeywor2=HttpContext.Session.GetString("PaymentSearch2");
        string query="SELECT distinct  PAYMENT.ID, INVOICES.ID, CUSTOMERS.NAME, PAYMENT.PAYMENT_TYPE, PAYMENT.AMOUNT, PAYMENT.PAYMENT_DATE FROM INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS WHERE INVOICES.ID= INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID AND PAYMENT.CUSTOMER_ID=CUSTOMERS.ID";
        if(searchkeyword != null && searchkeyword1 == null && searchkeywor2 ==null){
            query=query + " AND (INVOICES.ID like @id)"; 
        }else if(searchkeyword == null && searchkeyword1 != null && searchkeywor2 ==null){
            query=query + " AND (CUSTOMERS.NAME like @id1)";
        }else if(searchkeyword == null && searchkeyword1 == null && searchkeywor2 !=null){
            query=query + " AND (PAYMENT.PAYMENT_DATE like @id2)";
        }else if(searchkeyword != null && searchkeyword1 != null && searchkeywor2 ==null){
            query=query + " AND (INVOICES.ID like @id AND CUSTOMERS.NAME like @id1)";
        }else if(searchkeyword != null && searchkeyword1 == null && searchkeywor2 !=null){
            query=query + " AND (INVOICES.ID like @id AND PAYMENT.PAYMENT_DATE like @id2)";
        }else if(searchkeyword == null && searchkeyword1 != null && searchkeywor2 !=null){
            query=query + " AND (CUSTOMERS.NAME like @id1 AND PAYMENT.PAYMENT_DATE like @id2)";
        }else if(searchkeyword != null && searchkeyword1 != null && searchkeywor2 !=null){
            query=query + " AND (INVOICES.ID like @id AND CUSTOMERS.NAME like @id1 AND PAYMENT.PAYMENT_DATE like @id2)";
        }
        if(id == "id_asc"){
            query = query + " order by PAYMENT.ID ASC";
            payments=GetAllPayment(query);
        }else if(id == "id_desc"){
            query = query + " order by PAYMENT.ID DESC";
            payments=GetAllPayment(query);
        }else if(id == "number_asc"){
            query = query + " order by INVOICES.ID ASC";
            payments=GetAllPayment(query);
        }else if(id == "number_desc"){
            query = query + " order by INVOICES.ID DESC";
            payments=GetAllPayment(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME ASC";
            payments=GetAllPayment(query);
        }else if(id == "name_desc"){
            query = query + " order by CUSTOMERS.NAME DESC";
            payments=GetAllPayment(query);
        }else if(id == "type_asc"){
            query = query + " order by PAYMENT.PAYMENT_TYPE ASC";
            payments=GetAllPayment(query);
        }else if(id == "type_desc"){
            query = query + " order by PAYMENT.PAYMENT_TYPE DESC";
            payments=GetAllPayment(query);
        }else if(id == "paymentdate_asc"){
            query = query + " order by PAYMENT.PAYMENT_DATE ASC";
            payments=GetAllPayment(query);
        }else if(id == "paymentdate_desc"){
            query = query + " order by PAYMENT.PAYMENT_DATE DESC";
            payments=GetAllPayment(query);
        }else if(id == "amount_asc"){
            query = query + " order by PAYMENT.AMOUNT ASC";
            payments=GetAllPayment(query);
        }else if(id == "amount_desc"){
            query = query + " order by PAYMENT.AMOUNT DESC";
            payments=GetAllPayment(query);
        }
        // ViewBag.payment_list=payments;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedPayment= payments.ToPagedList(pageNumber,pageSize);
        ViewBag.payment_list=pagedPayment;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminPayment.cshtml", pagedPayment);
    }
}

