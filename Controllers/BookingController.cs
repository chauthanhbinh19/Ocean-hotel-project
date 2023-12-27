using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using MySqlX.XDevAPI;
using System.Web;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting.Server;
// using WebApi.Entities;

namespace Project.Controllers;

public class BookingController : Controller
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
    public void SendEmail(int booking_id){
        Customer customer=new Customer();
        Invoice invoice=new Invoice();
        Rooms rooms=new Rooms();
        Booking booking=new Booking();
        List<Service> services=new List<Service>();
        Taxes taxes=new Taxes();
        Promotion promotion=new Promotion();
        float? subTotal=0;
        float? Total=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT CUSTOMERS.NAME, CUSTOMERS.ADDRESS, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, INVOICES.ID, INVOICES_DETAILS.CREATE_DATE, ROOMS.ID, ROOMS.NAME, ROOMS.TYPE,ROOMS.PRICE, BOOKINGS.NUMBER_OF_GUESTS, SERVICES.ID, SERVICES.NAME, SERVICES.DESCRIPTION, SERVICES.PRICE, PROMOTIONS.DISCOUNT_PERCENT, TAXES.TAX_PERCENTAGE, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.TOTAL_PRICE FROM CUSTOMERS, INVOICES, INVOICES_DETAILS LEFT JOIN PROMOTIONS ON INVOICES_DETAILS.PROMOTION_ID = PROMOTIONS.ID LEFT JOIN TAXES ON INVOICES_DETAILS.TAXES_ID=TAXES.ID,ROOMS,PAYMENT, BOOKINGS LEFT JOIN BOOKING_SERVICE ON BOOKINGS.ID=BOOKING_SERVICE.BOOKING_ID LEFT JOIN SERVICES ON SERVICES.ID=BOOKING_SERVICE.SERVICE_ID WHERE CUSTOMERS.ID=PAYMENT.CUSTOMER_ID AND PAYMENT.ID=INVOICES_DETAILS.PAYMENT_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ROOM_ID=ROOMS.ID AND BOOKINGS.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",booking_id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.name=reader.GetString(0);
                customer.address=reader.GetString(1);
                customer.phone=reader.GetString(2);
                customer.email=reader.GetString(3);
                invoice.id=reader.GetInt32(4);
                DateTime day=reader.GetDateTime(5);
                invoice.create_date=day.ToString("dd-MM-yyyy");
                rooms.id=reader.GetInt32(6);
                rooms.name=reader.GetString(7);
                rooms.type=reader.GetString(8);
                rooms.price=reader.GetFloat(9);
                booking.number_of_guest=reader.GetInt32(10);
                Service service=new Service();
                if(!reader.IsDBNull(11)){
                    service.id=reader.GetInt32(11);
                }
                if(!reader.IsDBNull(12)){
                    service.name=reader.GetString(12);
                }
                if(!reader.IsDBNull(13)){
                    service.description=reader.GetString(13);
                }
                if(!reader.IsDBNull(14)){
                    service.price=reader.GetFloat(14);
                }
                if(service != null){
                    services.Add(service);
                }
                if(!reader.IsDBNull(15)){
                    promotion.discount_percent=reader.GetFloat(15);
                }
                if(!reader.IsDBNull(16)){
                    taxes.tax_percentage=reader.GetFloat(16);
                }
                DateTime day1=reader.GetDateTime(17);
                DateTime day2=reader.GetDateTime(18);
                booking.check_in_date=day1.ToString("dd-MM-yyyy");
                booking.check_out_date=day2.ToString("dd-MM-yyyy");
                booking.total_price=reader.GetFloat(19);
            }
            connection.Close();
        }
        subTotal=subTotal+rooms.price;
        if(services[0].name != null){
            foreach(var service in services){
                subTotal=subTotal+service.price;
            }
        }
        Total=subTotal;
        if(taxes.tax_percentage != null){
            Total=Total+taxes.tax_percentage/100;
        }
        if(promotion.discount_percent != null){
            Total=Total+promotion.discount_percent/100;
        }
        ViewBag.customer=customer;
        ViewBag.invoice=invoice;
        ViewBag.room=rooms;
        ViewBag.booking=booking;
        ViewBag.service=services;
        ViewBag.taxes=taxes;
        ViewBag.promotion=promotion;
        ViewBag.subTotal=subTotal;
        ViewBag.Total=Total;

        string fromEmail = "binh96.pch.1718@gmail.com";
        string toEmail = "21521871@gm.uit.edu.vn";
        // string toEmail = "21521176@gm.uit.edu.vn";
        string subject = "Invoice of reservation";
        string htmlBody="";
        using(StreamReader streamReader = System.IO.File.OpenText("Views/HotelViews/Email.html")){
            htmlBody=streamReader.ReadToEnd();
        }

        string rowTemplate ="<tr style='border-bottom: 1px solid rgba(0, 0, 0, 0.1); padding: 12px;'><td style='padding: 12px;'>{0}</td><td style='padding: 12px;'>{1}</td><td style='padding: 12px;'>{2}</td><td style='text-align: right; padding: 12px;'>${3}</td></tr>";
        string rows="";
        foreach(var service in services){
            string row= string.Format(rowTemplate, service.id,service.name,service.description,service.price);
            rows+=row;
        }
        string mesBody=string.Format(htmlBody,
            booking.check_in_date,
            invoice.id,
            customer.name,
            customer.address,
            customer.phone,
            customer.email,
            booking.check_in_date,
            booking.check_out_date,
            rooms.id,
            rooms.name,
            booking.number_of_guest,
            rooms.type,
            rooms.price,
            subTotal,
            taxes.tax_percentage,
            promotion.discount_percent,
            booking.total_price,
            rows
        );
        MailMessage mail = new MailMessage(fromEmail, toEmail, subject, mesBody);
        mail.IsBodyHtml=true;

        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
        smtpClient.Credentials = new NetworkCredential("binh96.pch.1718@gmail.com", "rnwe sqwr nrxu dgys");
        smtpClient.EnableSsl = true;
        try
        {
            smtpClient.Send(mail);
        }
        catch (SmtpException)
        {
        }
    }
    public IActionResult AdminBookingDetails(int page, string id, string id1){
        ViewBag.employee_avatar=GetAvatar();
        List<Booking> bookings=new List<Booking>();
        List<Booking> bookings1=new List<Booking>();
        Payment payment=new Payment();
        if(id != null && id1 != null){
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG, ROOMS_IMG.IMG, ROOMS.STATUS, CUSTOMERS.STATUS, CUSTOMERS.ADDRESS FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG, ROOMS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND CUSTOMERS.ID=@id1 AND BOOKINGS.ID=@id order by BOOKINGS.ID desc";
            bookings=GetBookingDetails(query,id,id1);
            string query2 = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG, ROOMS_IMG.IMG, ROOMS.STATUS, CUSTOMERS.STATUS, CUSTOMERS.ADDRESS FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG, ROOMS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND CUSTOMERS.ID=@id1 order by BOOKINGS.ID desc";
            bookings1=GetBookingDetails(query2,id,id1);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query1 = "SELECT PAYMENT.STATUS FROM INVOICES_DETAILS, PAYMENT WHERE PAYMENT.ID=INVOICES_DETAILS.PAYMENT_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
                MySqlCommand command = new MySqlCommand(query1, connection);
                command.Parameters.AddWithValue("@id", id);
                MySqlDataReader reader = command.ExecuteReader();
                while(reader.Read()){
                    payment.status=reader.GetString(0);
                }
            }
        }else{
             using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG, ROOMS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG, ROOMS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                Booking booking= new Booking{
                    id=reader.GetInt32(0),
                    // service_id=(int)reader["services_id"],
                    check_in_date=day.ToString("dd-MM-yyyy"),
                    check_out_date=day1.ToString("dd-MM-yyyy"),
                    number_of_guest=reader.GetInt32(6),
                    total_price=reader.GetFloat(2),
                    status=reader.GetString(3)
                };
                booking.customer=new Customer();
                booking.room=new Rooms();
                booking.customer.id=reader.GetInt32(1);
                booking.customer.name=reader.GetString(7);
                booking.customer.phone=reader.GetString(9);
                booking.customer.email=reader.GetString(10);
                booking.room.type=reader.GetString(8);
                booking.room.name=reader.GetString(11);
                booking.room.id=reader.GetInt32(12);
                booking.customer.img=reader.GetString(13);
                booking.room.img=reader.GetString(14);
                bookings.Add(booking);
            }

            reader.Close();
            string query1 = "SELECT PAYMENT.STATUS FROM INVOICES_DETAILS, PAYMENT WHERE PAYMENT.ID=INVOICES_DETAILS.PAYMENT_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
            command = new MySqlCommand(query1, connection);
            command.Parameters.AddWithValue("@id", id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                payment.status=reader.GetString(0);
            }
            connection.Close();
        }
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.booking_list1=bookings1;
        ViewBag.payment=payment;
        return View("~/Views/HotelViews/AdminBookingDetails.cshtml",pagedBooking);
    }
    public List<Booking> GetBookingDetails(string query, string id, string id1){
        List<Booking> bookings=new List<Booking>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            command.Parameters.AddWithValue("@id1",id1);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                Booking booking= new Booking{
                    id=reader.GetInt32(0),
                    check_in_date=day.ToString("dd-MM-yyyy"),
                    check_out_date=day1.ToString("dd-MM-yyyy"),
                    number_of_guest=reader.GetInt32(6),
                    total_price=reader.GetFloat(2),
                    status=reader.GetString(3)
                };
                booking.customer=new Customer();
                booking.room=new Rooms();
                booking.customer.id=reader.GetInt32(1);
                booking.customer.name=reader.GetString(7);
                booking.customer.phone=reader.GetString(9);
                booking.customer.email=reader.GetString(10);
                booking.room.type=reader.GetString(8);
                booking.room.name=reader.GetString(11);
                booking.room.id=reader.GetInt32(12);
                booking.customer.img=reader.GetString(13);
                booking.room.img=reader.GetString(14);
                if(reader.GetInt32(15)==1){
                    booking.room.status="Available";
                }else{
                    booking.room.status="Booked";
                }
                if(reader.GetInt32(16)==1){
                    booking.customer.status="Active";
                }else{
                    booking.customer.status="Inactive";
                }
                booking.customer.address=reader.GetString(17);
                bookings.Add(booking);
            }
            reader.Close();
            foreach(var booking in bookings){
                query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE ROOMS.ID=FURNITURE_ROOM.ROOMS_ID AND FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND ROOMS.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",booking.room!.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        name=reader.GetString(0)
                    };
                    booking.room.furnitures!.Add(furniture);
                }
                reader.Close();
            }
            connection.Close();
        }
        return bookings;
    }
    public IActionResult AdminBooking(int page){
        HttpContext.Session.Remove("BookingSearch");
        List<Booking> bookings=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                Booking booking= new Booking{
                    id=reader.GetInt32(0),
                    // service_id=(int)reader["services_id"],
                    check_in_date=day.ToString("dd-MM-yyyy"),
                    check_out_date=day1.ToString("dd-MM-yyyy"),
                    number_of_guest=reader.GetInt32(6),
                    total_price=reader.GetFloat(2),
                    status=reader.GetString(3)
                };
                booking.customer=new Customer();
                booking.room=new Rooms();
                booking.customer.id=reader.GetInt32(1);
                booking.customer.name=reader.GetString(7);
                booking.customer.phone=reader.GetString(9);
                booking.customer.email=reader.GetString(10);
                booking.room.type=reader.GetString(8);
                booking.room.name=reader.GetString(11);
                booking.room.id=reader.GetInt32(12);
                booking.customer.img=reader.GetString(13);
                bookings.Add(booking);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminBooking.cshtml", pagedBooking);
    }
    public IActionResult AdminBookingPending(int page){
        HttpContext.Session.Remove("BookingPendingSearch");
        List<Booking> bookings_pending=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Pending"){
                    Booking booking_pending= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_pending.customer=new Customer();
                    booking_pending.room=new Rooms();
                    booking_pending.customer.id=reader.GetInt32(1);
                    booking_pending.customer.name=reader.GetString(7);
                    booking_pending.customer.phone=reader.GetString(9);
                    booking_pending.customer.email=reader.GetString(10);
                    booking_pending.room.type=reader.GetString(8);
                    booking_pending.room.name=reader.GetString(11);
                    booking_pending.room.id=reader.GetInt32(12);
                    booking_pending.customer.img=reader.GetString(13);
                    bookings_pending.Add(booking_pending);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_pending.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        return View("~/Views/HotelViews/AdminBookingPending.cshtml", pagedBooking);
    }
    public IActionResult AdminBookingBooked(int page){
        HttpContext.Session.Remove("BookingBookedSearch");
        List<Booking> bookings_booked=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Booked"){
                    Booking booking_booked= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_booked.customer=new Customer();
                    booking_booked.room=new Rooms();
                    booking_booked.customer.id=reader.GetInt32(1);
                    booking_booked.customer.name=reader.GetString(7);
                    booking_booked.customer.phone=reader.GetString(9);
                    booking_booked.customer.email=reader.GetString(10);
                    booking_booked.room.type=reader.GetString(8);
                    booking_booked.room.name=reader.GetString(11);
                    booking_booked.room.id=reader.GetInt32(12);
                    booking_booked.customer.img=reader.GetString(13);
                    bookings_booked.Add(booking_booked);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_booked.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        return View("~/Views/HotelViews/AdminBookingBooked.cshtml", pagedBooking);
    }
    public IActionResult AdminBookingRefund(int page){
        HttpContext.Session.Remove("BookingRefundSearch");
        List<Booking> bookings_refund=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Refund"){
                    Booking booking_refund= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_refund.customer=new Customer();
                    booking_refund.room=new Rooms();
                    booking_refund.customer.id=reader.GetInt32(1);
                    booking_refund.customer.name=reader.GetString(7);
                    booking_refund.customer.phone=reader.GetString(9);
                    booking_refund.customer.email=reader.GetString(10);
                    booking_refund.room.type=reader.GetString(8);
                    booking_refund.room.name=reader.GetString(11);
                    booking_refund.room.id=reader.GetInt32(12);
                    booking_refund.customer.img=reader.GetString(13);
                    bookings_refund.Add(booking_refund);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_refund.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        return View("~/Views/HotelViews/AdminBookingRefund.cshtml", pagedBooking);
    }
    public IActionResult AdminBookingCanceled(int page){
        HttpContext.Session.Remove("BookingCanceledSearch");
        List<Booking> bookings_canceled=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Canceled"){
                    Booking booking_canceled= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_canceled.customer=new Customer();
                    booking_canceled.room=new Rooms();
                    booking_canceled.customer.id=reader.GetInt32(1);
                    booking_canceled.customer.name=reader.GetString(7);
                    booking_canceled.customer.phone=reader.GetString(9);
                    booking_canceled.customer.email=reader.GetString(10);
                    booking_canceled.room.type=reader.GetString(8);
                    booking_canceled.room.name=reader.GetString(11);
                    booking_canceled.room.id=reader.GetInt32(12);
                    booking_canceled.customer.img=reader.GetString(13);
                    bookings_canceled.Add(booking_canceled);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_canceled.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        return View("~/Views/HotelViews/AdminBookingCanceled.cshtml", pagedBooking);
    }
    public IActionResult AdminAddBooking(){
        List<Customer> customersname= new List<Customer>();
        List<Rooms> roomsname= new List<Rooms>();
        List<Service> services= new List<Service>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, NAME,PHONE, EMAIL FROM customers";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer= new Customer{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    phone=reader.GetString(2),
                    email=reader.GetString(3),
                };
                customersname.Add(customer);
            }
            reader.Close();
            query = "SELECT ID, NAME, STATUS FROM ROOMS";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                string sta;
                if(reader.GetInt32(2)==0){
                    sta="Booked";
                }else{
                    sta="Available";
                }
                Rooms room= new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    status=sta
                };
                roomsname.Add(room);
            }
            // reader.Close();
            // query = "SELECT ID, NAME FROM Services";
            // command = new MySqlCommand(query, connection);
            // reader = command.ExecuteReader();
            // while(reader.Read()){
            //     Service service= new Service{
            //         id=reader.GetInt32(0),
            //         name=reader.GetString(1),
            //     };
            //     services.Add(service);
            // }
            connection.Close();
        }
        ViewBag.customername_list=customersname;
        ViewBag.roomsname_list= roomsname;
        // ViewBag.services_list= services;
        return View("~/Views/HotelViews/AdminAddBooking.cshtml");
    }
    public IActionResult AdminEditBooking(){
        List<Customer> customersname= new List<Customer>();
        List<Rooms> roomsname= new List<Rooms>();
        List<Service> services= new List<Service>();
        List<Booking> bookings= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID, NAME,PHONE, EMAIL FROM customers";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer= new Customer{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    phone=reader.GetString(2),
                    email=reader.GetString(3),
                };
                customersname.Add(customer);
            }
            reader.Close();
            query = "SELECT ID, NAME, STATUS FROM ROOMS";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                string sta;
                if(reader.GetInt32(2)==0){
                    sta="Booked";
                }else{
                    sta="Available";
                }
                Rooms room= new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    status=sta
                };
                roomsname.Add(room);
            }
            reader.Close();
            query = "SELECT BOOKINGS.ID FROM BOOKINGS order by BOOKINGS.ID asc";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Booking booking= new Booking{
                    id=reader.GetInt32(0)
                };
                bookings.Add(booking);
            }
            connection.Close();
        }
        ViewBag.customername_list=customersname;
        ViewBag.roomsname_list= roomsname;
        ViewBag.booking_list=bookings;
        return View("~/Views/HotelViews/AdminEditBooking.cshtml");
    }
    [HttpPost]
    public IActionResult AdminInsertBooking(Booking booking){
        int? id=1;
        int? booking_id=1;
        ModelState.Remove("customer.gender");
        ModelState.Remove("customer.dateofbirth");
        ModelState.Remove("customer.address");
        ModelState.Remove("customer.status");
        ModelState.Remove("customer.img");
        ModelState.Remove("customer.account.username");
        ModelState.Remove("customer.account.password");
        ModelState.Remove("customer.account.type");
        ModelState.Remove("room.floor");
        ModelState.Remove("room.size");
        ModelState.Remove("room.status");
        ModelState.Remove("room.price");
        ModelState.Remove("room.img");
        ModelState.Remove("room.view.name");
        ModelState.Remove("service.name");
        ModelState.Remove("service.description");
        ModelState.Remove("service.price");
        ModelState.Remove("service.shuttle");
        ModelState.Remove("service.img");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddBooking");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM customers where NAME=@name and PHONE=@phone and EMAIL=@email";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",booking?.customer?.name);
            command.Parameters.AddWithValue("@phone",booking?.customer?.phone);
            command.Parameters.AddWithValue("@email",booking?.customer?.email);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
            }
            reader.Close();
            query="SELECT ID FROM BOOKINGS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(booking_id==reader.GetInt32(0)){
                    booking_id=booking_id+1;
                }
            }

            reader.Close();
            query = "INSERT INTO BOOKINGS (ID,CUSTOMER_ID,ROOM_ID,CHECK_IN_DATE,CHECK_OUT_DATE,NUMBER_OF_GUESTS,TOTAL_PRICE,STATUS) values (@id,@customerId,@roomId,@checkin,@checkout,@numberofguest,@totalprice,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",booking_id);
            command.Parameters.AddWithValue("@customerId",id);
            command.Parameters.AddWithValue("@roomId",booking?.room?.id);
            command.Parameters.AddWithValue("@checkin",booking?.check_in_date);
            command.Parameters.AddWithValue("@checkout",booking?.check_out_date);
            command.Parameters.AddWithValue("@numberofguest",booking?.number_of_guest);
            command.Parameters.AddWithValue("@totalprice",booking?.total_price);
            command.Parameters.AddWithValue("@status","Booked");
            reader = command.ExecuteReader();
            connection.Close();
        }
        // return View("~/Views/HotelViews/AdminAddBooking.cshtml");
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminBooking");
    }
    [HttpPost]
    public IActionResult AdminUpdateBooking(Booking booking){
        int? id=1;
        ModelState.Remove("customer.gender");
        ModelState.Remove("customer.dateofbirth");
        ModelState.Remove("customer.address");
        ModelState.Remove("customer.status");
        ModelState.Remove("customer.img");
        ModelState.Remove("customer.account.username");
        ModelState.Remove("customer.account.password");
        ModelState.Remove("customer.account.type");
        ModelState.Remove("room.floor");
        ModelState.Remove("room.size");
        ModelState.Remove("room.status");
        ModelState.Remove("room.price");
        ModelState.Remove("room.img");
        ModelState.Remove("room.view.name");
        ModelState.Remove("service.name");
        ModelState.Remove("service.description");
        ModelState.Remove("service.price");
        ModelState.Remove("service.shuttle");
        ModelState.Remove("service.img");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditBooking");
        }
        int invoice_id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM customers where NAME=@name and PHONE=@phone and EMAIL=@email";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",booking?.customer?.name);
            command.Parameters.AddWithValue("@phone",booking?.customer?.phone);
            command.Parameters.AddWithValue("@email",booking?.customer?.email);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
            }

            reader.Close();
            query = "UPDATE BOOKINGS SET CUSTOMER_ID=@customerId, ROOM_ID=@roomId,CHECK_IN_DATE=@checkin, CHECK_OUT_DATE=@checkout,NUMBER_OF_GUESTS=@numberofguest, TOTAL_PRICE=@totalprice,STATUS=@status WHERE ID=@bookingId";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@customerId",id);
            command.Parameters.AddWithValue("@roomId",booking?.room?.id);
            command.Parameters.AddWithValue("@checkin",booking?.check_in_date);
            command.Parameters.AddWithValue("@checkout",booking?.check_out_date);
            command.Parameters.AddWithValue("@numberofguest",booking?.number_of_guest);
            command.Parameters.AddWithValue("@totalprice",booking?.total_price);
            command.Parameters.AddWithValue("@status",booking?.status);
            command.Parameters.AddWithValue("@bookingId",booking?.id);
            reader = command.ExecuteReader();

            reader.Close();
            query="SELECT INVOICES.ID FROM INVOICES, INVOICES_DETAILS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",booking?.id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                invoice_id=reader.GetInt32(0);
            }

            reader.Close();
            if(booking?.status=="Booked"){
                query="UPDATE INVOICES SET STATUS =1 WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();
            }else if(booking?.status=="Refund"){
                query="UPDATE INVOICES SET STATUS =0 WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();
            }else if(booking?.status=="Canceled"){
                query="UPDATE INVOICES SET STATUS =0 WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();
            }
            connection.Close();
        }
        // return View("~/Views/HotelViews/AdminAddBooking.cshtml");
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminBooking");
    }
    [HttpPost]
    public IActionResult GetCustomerInfo(string selectedOption){
        // Customer customer= new Customer();
        string? phone="0101910101";
        string? email="19928@gmail.com";
        int? id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID,PHONE, EMAIL FROM customers where name =@customerName";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@customerName",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
                phone=reader.GetString(1);
                email=reader.GetString(2);
            }
            connection.Close();
        }
        return Json(new {id=id,phone=phone, email=email});
    }
    [HttpPost]
    public IActionResult GetCustomerInfo2(string selectedOption){
        // Customer customer= new Customer();
        string? name="";
        string? email="";
        int? id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID,NAME, EMAIL FROM customers where phone =@customerPhone";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@customerPhone",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
                name=reader.GetString(1);
                email=reader.GetString(2);
            }
            connection.Close();
        }
        return Json(new {id=id,name=name, email=email});
    }
    [HttpPost]
    public IActionResult GetCustomerInfo3(string selectedOption){
        // Customer customer= new Customer();
        string? name="";
        string? phone="";
        int? id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID,NAME, PHONE FROM customers where email =@customerEmail";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@customerEmail",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
                name=reader.GetString(1);
                phone=reader.GetString(2);
            }
            connection.Close();
        }
        return Json(new {id=id,name=name, phone=phone});
    }
    [HttpPost]
    public IActionResult GetRoomInfo(string selectedOption){
        string? type="";
        int? id=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID,TYPE FROM ROOMS where id =@roomid";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@roomid",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
                type=reader.GetString(1);
            }
            connection.Close();
        }
        return Json(new {id=id,type=type});
    }
    [HttpPost]
    public IActionResult GetRoomInfo2(string selectedOption){
        float? type=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT PRICE FROM ROOMS where id =@roomid";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@roomid",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                type=reader.GetFloat(0);
            }
            connection.Close();
        }
        return Json(new {type=type});
    }
    [HttpPost]
    public IActionResult GetBookingInfo(string selectedOption){
        string? customer_name="";
        string? customer_phone="";
        string? customer_email="";
        string? room_name="";
        string? room_type="";
        int? number_of_guest=0;
        string? check_in="";
        string? check_out="";
        float? total_price=0;
        string? booking_status="";
        int? room_id=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, ROOMS.NAME, ROOMS.TYPE, BOOKINGS.NUMBER_OF_GUESTS, CHECK_IN_DATE, CHECK_OUT_DATE, BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS,ROOMS.ID from CUSTOMERS, BOOKINGS, ROOMS where CUSTOMERS.ID=BOOKINGS.CUSTOMER_ID and BOOKINGS.ROOM_ID=ROOMS.ID and BOOKINGS.ID=@bookingId";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@bookingId",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer_name=reader.GetString(0);
                customer_phone=reader.GetString(1);
                customer_email=reader.GetString(2);
                room_name=reader.GetString(3);
                room_type=reader.GetString(4);
                number_of_guest=reader.GetInt32(5);
                DateTime check_in_date=reader.GetDateTime(6);
                check_in=check_in_date.ToString("yyyy-MM-dd");
                DateTime check_out_date=reader.GetDateTime(7);
                check_out=check_out_date.ToString("yyyy-MM-dd");
                total_price=reader.GetFloat(8);
                booking_status=reader.GetString(9);
                room_id=reader.GetInt32(10);
            }
            connection.Close();
        }
        return Json(new {customer_name=customer_name, customer_phone=customer_phone, customer_email=customer_email, room_name=room_name, room_type=room_type,number_of_guest=number_of_guest, check_in=check_in, check_out=check_out,total_price=total_price, booking_status=booking_status, room_id=room_id});
    }
    public IActionResult EditBooking(int id){
        List<Customer> customersname= new List<Customer>();
        List<Rooms> roomsname= new List<Rooms>();
        List<Service> services= new List<Service>();
        List<Booking> bookings1= new List<Booking>();
        Booking bookings= new Booking();
        ViewBag.employee_avatar=GetAvatar();
        ModelState.Remove("customer.gender");
        ModelState.Remove("customer.dateofbirth");
        ModelState.Remove("customer.address");
        ModelState.Remove("customer.status");
        ModelState.Remove("customer.img");
        ModelState.Remove("customer.account.username");
        ModelState.Remove("customer.account.password");
        ModelState.Remove("customer.account.type");
        ModelState.Remove("room.floor");
        ModelState.Remove("room.size");
        ModelState.Remove("room.status");
        ModelState.Remove("room.price");
        ModelState.Remove("room.img");
        ModelState.Remove("room.view.name");
        ModelState.Remove("service.name");
        ModelState.Remove("service.description");
        ModelState.Remove("service.price");
        ModelState.Remove("service.shuttle");
        ModelState.Remove("service.img");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditBooking");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT BOOKINGS.ID, CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, ROOMS.ID, ROOMS.TYPE, BOOKINGS.NUMBER_OF_GUESTS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, ROOMS.NAME FROM BOOKINGS, CUSTOMERS, ROOMS WHERE BOOKINGS.CUSTOMER_ID= CUSTOMERS.ID AND ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(7);
                DateTime day1=reader.GetDateTime(8);
                
                bookings.id=reader.GetInt32(0);
                bookings.number_of_guest=reader.GetInt32(6);
                bookings.check_in_date=day.ToString("yyyy-MM-dd");
                bookings.check_out_date=day1.ToString("yyyy-MM-dd");
                bookings.total_price=reader.GetFloat(9);
                bookings.status=reader.GetString(10);
                
                bookings.customer=new Customer();
                bookings.customer.name=reader.GetString(1);
                bookings.customer.phone=reader.GetString(2);
                bookings.customer.email=reader.GetString(3);
                bookings.room=new Rooms();
                bookings.room.id=reader.GetInt32(4);
                bookings.room.type=reader.GetString(5);
                bookings.room.name=reader.GetString(11);
            }
            reader.Close();
            query = "SELECT ID, NAME,PHONE, EMAIL FROM customers";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer= new Customer{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    phone=reader.GetString(2),
                    email=reader.GetString(3),
                };
                customersname.Add(customer);
            }
            reader.Close();
            query = "SELECT ID, NAME, STATUS FROM ROOMS";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                string sta;
                if(reader.GetInt32(2)==0){
                    sta="Booked";
                }else{
                    sta="Available";
                }
                Rooms room= new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    status=sta
                };
                roomsname.Add(room);
            }
            reader.Close();
            query = "SELECT BOOKINGS.ID FROM BOOKINGS order by BOOKINGS.ID asc";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Booking booking1= new Booking{
                    id=reader.GetInt32(0)
                };
                bookings1.Add(booking1);
            }
            connection.Close();
        }
        ViewBag.customername_list=customersname;
        ViewBag.roomsname_list= roomsname;
        ViewBag.booking_list=bookings1;
        return View("~/Views/HotelViews/AdminEditBooking.cshtml",bookings);
    }
    public IActionResult DeleteBooking(int id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM BOOKINGS WHERE ID=@id";
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
        return RedirectToAction("AdminBooking");
    }
    public List<Booking> GetAllBooking(string query){
        List<Booking> bookings=new List<Booking>();
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + BookingSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Booking booking= new Booking{
                    id=reader.GetInt32(0),
                    // service_id=(int)reader["services_id"],
                    check_in_date=reader.GetDateTime(4).ToString(),
                    check_out_date=reader.GetDateTime(5).ToString(),
                    number_of_guest=reader.GetInt32(6),
                    total_price=reader.GetFloat(2),
                    status=reader.GetString(3)
                };
                booking.customer=new Customer();
                booking.room=new Rooms();
                booking.customer.id=reader.GetInt32(1);
                booking.customer.name=reader.GetString(7);
                booking.customer.phone=reader.GetString(9);
                booking.customer.email=reader.GetString(10);
                booking.room.type=reader.GetString(8);
                booking.room.name=reader.GetString(11);
                booking.room.id=reader.GetInt32(12);
                booking.customer.img=reader.GetString(13);
                bookings.Add(booking);
            }
            connection.Close();
        }
        return bookings;
    }
    public List<Booking> GetPendBooking(string query){
        List<Booking> bookings_pending=new List<Booking>();
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + BookingSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string booking_status=(string)reader["status"];
                if(booking_status=="Pending"){
                    Booking booking_pending= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=reader.GetDateTime(4).ToString(),
                        check_out_date=reader.GetDateTime(5).ToString(),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_pending.customer=new Customer();
                    booking_pending.room=new Rooms();
                    booking_pending.customer.id=reader.GetInt32(1);
                    booking_pending.customer.name=reader.GetString(7);
                    booking_pending.customer.phone=reader.GetString(9);
                    booking_pending.customer.email=reader.GetString(10);
                    booking_pending.room.type=reader.GetString(8);
                    booking_pending.room.name=reader.GetString(11);
                    booking_pending.room.id=reader.GetInt32(12);
                    booking_pending.customer.img=reader.GetString(13);
                    bookings_pending.Add(booking_pending);
                }
            }
            connection.Close();
        }
        return bookings_pending;
    }
    public List<Booking> GetBookedBooking(string query){
        List<Booking> bookings_booked=new List<Booking>();
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + BookingSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string booking_status=(string)reader["status"];
                if(booking_status=="Booked"){
                    Booking booking_booked= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=reader.GetDateTime(4).ToString(),
                        check_out_date=reader.GetDateTime(5).ToString(),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_booked.customer=new Customer();
                    booking_booked.room=new Rooms();
                    booking_booked.customer.id=reader.GetInt32(1);
                    booking_booked.customer.name=reader.GetString(7);
                    booking_booked.customer.phone=reader.GetString(9);
                    booking_booked.customer.email=reader.GetString(10);
                    booking_booked.room.type=reader.GetString(8);
                    booking_booked.room.name=reader.GetString(11);
                    booking_booked.room.id=reader.GetInt32(12);
                    booking_booked.customer.img=reader.GetString(13);
                    bookings_booked.Add(booking_booked);
                }
            }
            connection.Close();
        }
        return bookings_booked;
    }
    public List<Booking> GetRefundBooking(string query){
        List<Booking> bookings_refund=new List<Booking>();
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + BookingSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string booking_status=(string)reader["status"];
                if(booking_status=="Refund"){
                    Booking booking_refund= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=reader.GetDateTime(4).ToString(),
                        check_out_date=reader.GetDateTime(5).ToString(),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_refund.customer=new Customer();
                    booking_refund.room=new Rooms();
                    booking_refund.customer.id=reader.GetInt32(1);
                    booking_refund.customer.name=reader.GetString(7);
                    booking_refund.customer.phone=reader.GetString(9);
                    booking_refund.customer.email=reader.GetString(10);
                    booking_refund.room.type=reader.GetString(8);
                    booking_refund.room.name=reader.GetString(11);
                    booking_refund.room.id=reader.GetInt32(12);
                    booking_refund.customer.img=reader.GetString(13);
                    bookings_refund.Add(booking_refund);
                }
            }
            connection.Close();
        }
        return bookings_refund;
    }
    public List<Booking> GetCanceledBooking(string query){
        List<Booking> bookings_canceled=new List<Booking>();
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + BookingSearch + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string booking_status=(string)reader["status"];
                if(booking_status=="Refund"){
                    Booking booking_canceled= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=reader.GetDateTime(4).ToString(),
                        check_out_date=reader.GetDateTime(5).ToString(),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_canceled.customer=new Customer();
                    booking_canceled.room=new Rooms();
                    booking_canceled.customer.id=reader.GetInt32(1);
                    booking_canceled.customer.name=reader.GetString(7);
                    booking_canceled.customer.phone=reader.GetString(9);
                    booking_canceled.customer.email=reader.GetString(10);
                    booking_canceled.room.type=reader.GetString(8);
                    booking_canceled.room.name=reader.GetString(11);
                    booking_canceled.room.id=reader.GetInt32(12);
                    booking_canceled.customer.img=reader.GetString(13);
                    bookings_canceled.Add(booking_canceled);
                }
            }
            connection.Close();
        }
        return bookings_canceled;
    }
    public IActionResult AdminSearchBooking(string searchkeyword, int page){
        List<Booking> bookings=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("BookingSearch",searchkeyword);
        }
        var a= HttpContext.Session.GetString("BookingSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME, ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID,CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id) order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                Booking booking= new Booking{
                    id=reader.GetInt32(0),
                    // service_id=(int)reader["services_id"],
                    check_in_date=day.ToString("dd-MM-yyyy"),
                    check_out_date=day1.ToString("dd-MM-yyyy"),
                    number_of_guest=reader.GetInt32(6),
                    total_price=reader.GetFloat(2),
                    status=reader.GetString(3)
                };
                booking.customer=new Customer();
                booking.room=new Rooms();
                booking.customer.id=reader.GetInt32(1);
                booking.customer.name=reader.GetString(7);
                booking.customer.phone=reader.GetString(9);
                booking.customer.email=reader.GetString(10);
                booking.room.type=reader.GetString(8);
                booking.room.name=reader.GetString(11);
                booking.room.id=reader.GetInt32(12);
                booking.customer.img=reader.GetString(13);
                bookings.Add(booking);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminBooking.cshtml",pagedBooking);
    }
    public IActionResult AdminSearchBookingPending(string searchkeyword, int page){
        List<Booking> bookings_pending=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("BookingPendingSearch",searchkeyword);
        }
        var a= HttpContext.Session.GetString("BookingPendingSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME, ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID,CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id) order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Pending"){
                    Booking booking_pending= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_pending.customer=new Customer();
                    booking_pending.room=new Rooms();
                    booking_pending.customer.id=reader.GetInt32(1);
                    booking_pending.customer.name=reader.GetString(7);
                    booking_pending.customer.phone=reader.GetString(9);
                    booking_pending.customer.email=reader.GetString(10);
                    booking_pending.room.type=reader.GetString(8);
                    booking_pending.room.name=reader.GetString(11);
                    booking_pending.room.id=reader.GetInt32(12);
                    booking_pending.customer.img=reader.GetString(13);
                    bookings_pending.Add(booking_pending);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_pending.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminBookingPending.cshtml",pagedBooking);
    }
    public IActionResult AdminSearchBookingBooked(string searchkeyword, int page){
        List<Booking> bookings_booked=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("BookingBookedSearch",searchkeyword);
        }
        var a= HttpContext.Session.GetString("BookingBookedSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME, ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID,CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id) order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Booked"){
                    Booking booking_booked= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_booked.customer=new Customer();
                    booking_booked.room=new Rooms();
                    booking_booked.customer.id=reader.GetInt32(1);
                    booking_booked.customer.name=reader.GetString(7);
                    booking_booked.customer.phone=reader.GetString(9);
                    booking_booked.customer.email=reader.GetString(10);
                    booking_booked.room.type=reader.GetString(8);
                    booking_booked.room.name=reader.GetString(11);
                    booking_booked.room.id=reader.GetInt32(12);
                    booking_booked.customer.img=reader.GetString(13);
                    bookings_booked.Add(booking_booked);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_booked.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminBookingBooked.cshtml",pagedBooking);
    }
    public IActionResult AdminSearchBookingRefund(string searchkeyword, int page){
        List<Booking> bookings_refund=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("BookingRefundSearch",searchkeyword);
        }
        var a= HttpContext.Session.GetString("BookingRefundSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME, ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID,CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id) order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Refund"){
                    Booking booking_refund= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_refund.customer=new Customer();
                    booking_refund.room=new Rooms();
                    booking_refund.customer.id=reader.GetInt32(1);
                    booking_refund.customer.name=reader.GetString(7);
                    booking_refund.customer.phone=reader.GetString(9);
                    booking_refund.customer.email=reader.GetString(10);
                    booking_refund.room.type=reader.GetString(8);
                    booking_refund.room.name=reader.GetString(11);
                    booking_refund.room.id=reader.GetInt32(12);
                    booking_refund.customer.img=reader.GetString(13);
                    bookings_refund.Add(booking_refund);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_refund.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminBookingRefund.cshtml",pagedBooking);
    }
    public IActionResult AdminSearchBookingCanceled(string searchkeyword, int page){
        List<Booking> bookings_canceled=new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("BookingCanceledSearch",searchkeyword);
        }
        var a= HttpContext.Session.GetString("BookingCanceledSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword =a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME, ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID,CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS_IMG.CUSTOMER_ID=CUSTOMERS.ID and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id) order by BOOKINGS.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","%" + searchkeyword + "%");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                DateTime day=reader.GetDateTime(4);
                DateTime day1=reader.GetDateTime(5);
                string booking_status=(string)reader["status"];
                if(booking_status=="Canceled"){
                    Booking booking_canceled= new Booking{
                        id=reader.GetInt32(0),
                        // service_id=(int)reader["services_id"],
                        check_in_date=day.ToString("dd-MM-yyyy"),
                        check_out_date=day1.ToString("dd-MM-yyyy"),
                        number_of_guest=reader.GetInt32(6),
                        total_price=reader.GetFloat(2),
                        status=reader.GetString(3)
                    };
                    booking_canceled.customer=new Customer();
                    booking_canceled.room=new Rooms();
                    booking_canceled.customer.id=reader.GetInt32(1);
                    booking_canceled.customer.name=reader.GetString(7);
                    booking_canceled.customer.phone=reader.GetString(9);
                    booking_canceled.customer.email=reader.GetString(10);
                    booking_canceled.room.type=reader.GetString(8);
                    booking_canceled.room.name=reader.GetString(11);
                    booking_canceled.room.id=reader.GetInt32(12);
                    booking_canceled.customer.img=reader.GetString(13);
                    bookings_canceled.Add(booking_canceled);
                }
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_canceled.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminBookingCanceled.cshtml",pagedBooking);
    }
    public IActionResult AdminSortBooking(string id, int page){
        List<Booking> bookings= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID";
        string likequery=" and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id)";
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        if(BookingSearch != null){
            query=query+ likequery;
        }
        if(id == "id_asc"){
            query = query + " order by BOOKINGS.ID asc";
            bookings= GetAllBooking(query);
        }else if(id =="id_desc"){
            query = query + " order by BOOKINGS.ID desc";
            bookings= GetAllBooking(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            bookings= GetAllBooking(query);
        }else if(id =="name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            bookings= GetAllBooking(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            bookings= GetAllBooking(query);
        }else if(id =="phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            bookings= GetAllBooking(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            bookings= GetAllBooking(query);
        }else if(id =="email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            bookings= GetAllBooking(query);
        }else if(id == "numberofguest_asc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS asc";
            bookings= GetAllBooking(query);
        }else if(id =="numberofguest_desc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS desc";
            bookings= GetAllBooking(query);
        }else if(id == "checkin_asc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE asc";
            bookings= GetAllBooking(query);
        }else if(id =="checkin_desc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE desc";
            bookings= GetAllBooking(query);
        }else if(id == "checkout_asc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE asc";
            bookings= GetAllBooking(query);
        }else if(id =="checkout_desc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE desc";
            bookings= GetAllBooking(query);
        }else if(id == "total_asc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE asc";
            bookings= GetAllBooking(query);
        }else if(id =="total_desc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE desc";
            bookings= GetAllBooking(query);
        }else if(id == "roomname_asc"){
            query = query + " order by ROOMS.NAME asc";
            bookings= GetAllBooking(query);
        }else if(id =="roomname_desc"){
            query = query + " order by ROOMS.NAME desc";
            bookings= GetAllBooking(query);
        }else if(id == "roomtype_asc"){
            query = query + " order by ROOMS.TYPE asc";
            bookings= GetAllBooking(query);
        }else if(id =="roomtype_desc"){
            query = query + " order by ROOMS.TYPE desc";
            bookings= GetAllBooking(query);
        }else if(id == "status_asc"){
            query = query + " order by BOOKINGS.STATUS asc";
            bookings= GetAllBooking(query);
        }else if(id =="status_desc"){
            query = query + " order by BOOKINGS.STATUS desc";
            bookings= GetAllBooking(query);
        }           
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminBooking.cshtml", pagedBooking);
    }
    public IActionResult AdminSortBookingPending(string id, int page){
        List<Booking> bookings_pending= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID";
        string likequery=" and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id)";
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        if(BookingSearch != null){
            query=query+ likequery;
        }
        if(id == "id_asc"){
            query = query + " order by BOOKINGS.ID asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="id_desc"){
            query = query + " order by BOOKINGS.ID desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "numberofguest_asc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="numberofguest_desc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "checkin_asc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="checkin_desc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "checkout_asc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="checkout_desc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "total_asc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="total_desc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "roomname_asc"){
            query = query + " order by ROOMS.NAME asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="roomname_desc"){
            query = query + " order by ROOMS.NAME desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "roomtype_asc"){
            query = query + " order by ROOMS.TYPE asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="roomtype_desc"){
            query = query + " order by ROOMS.TYPE desc";
            bookings_pending= GetPendBooking(query);
        }else if(id == "status_asc"){
            query = query + " order by BOOKINGS.STATUS asc";
            bookings_pending= GetPendBooking(query);
        }else if(id =="status_desc"){
            query = query + " order by BOOKINGS.STATUS desc";
            bookings_pending= GetPendBooking(query);
        }           
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_pending.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminBookingPending.cshtml", pagedBooking);
    }
    public IActionResult AdminSortBookingBooked(string id, int page){
        List<Booking> bookings_booked= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID";
        string likequery=" and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id)";
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        if(BookingSearch != null){
            query=query+ likequery;
        }
        if(id == "id_asc"){
            query = query + " order by BOOKINGS.ID asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="id_desc"){
            query = query + " order by BOOKINGS.ID desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "numberofguest_asc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="numberofguest_desc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "checkin_asc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="checkin_desc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "checkout_asc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="checkout_desc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "total_asc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="total_desc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "roomname_asc"){
            query = query + " order by ROOMS.NAME asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="roomname_desc"){
            query = query + " order by ROOMS.NAME desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "roomtype_asc"){
            query = query + " order by ROOMS.TYPE asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="roomtype_desc"){
            query = query + " order by ROOMS.TYPE desc";
            bookings_booked= GetBookedBooking(query);
        }else if(id == "status_asc"){
            query = query + " order by BOOKINGS.STATUS asc";
            bookings_booked= GetBookedBooking(query);
        }else if(id =="status_desc"){
            query = query + " order by BOOKINGS.STATUS desc";
            bookings_booked= GetBookedBooking(query);
        }           
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_booked.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminBookingBooked.cshtml", pagedBooking);
    }
    public IActionResult AdminSortBookingRefund(string id, int page){
        List<Booking> bookings_refund= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID";
        string likequery=" and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id)";
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        if(BookingSearch != null){
            query=query+ likequery;
        }
        if(id == "id_asc"){
            query = query + " order by BOOKINGS.ID asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="id_desc"){
            query = query + " order by BOOKINGS.ID desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "numberofguest_asc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="numberofguest_desc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "checkin_asc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="checkin_desc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "checkout_asc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="checkout_desc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "total_asc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="total_desc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "roomname_asc"){
            query = query + " order by ROOMS.NAME asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="roomname_desc"){
            query = query + " order by ROOMS.NAME desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "roomtype_asc"){
            query = query + " order by ROOMS.TYPE asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="roomtype_desc"){
            query = query + " order by ROOMS.TYPE desc";
            bookings_refund= GetRefundBooking(query);
        }else if(id == "status_asc"){
            query = query + " order by BOOKINGS.STATUS asc";
            bookings_refund= GetRefundBooking(query);
        }else if(id =="status_desc"){
            query = query + " order by BOOKINGS.STATUS desc";
            bookings_refund= GetRefundBooking(query);
        }           
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_refund.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminBookingRefund.cshtml", pagedBooking);
    }
    public IActionResult AdminSortBookingCanceled(string id, int page){
        List<Booking> bookings_canceled= new List<Booking>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct BOOKINGS.ID,BOOKINGS.CUSTOMER_ID,BOOKINGS.TOTAL_PRICE, BOOKINGS.STATUS, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, CUSTOMERS.NAME,ROOMS.TYPE, CUSTOMERS.PHONE, CUSTOMERS.EMAIl, ROOMS.NAME, ROOMS.ID, CUSTOMERS_IMG.IMG FROM BOOKINGS, CUSTOMERS,ROOMS, SERVICES, CUSTOMERS_IMG WHERE BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and CUSTOMERS.ID =CUSTOMERS_IMG.CUSTOMER_ID";
        string likequery=" and (CUSTOMERS.NAME like @id or ROOMS.TYPE like @id or BOOKINGS.NUMBER_OF_GUESTS like @id or BOOKINGS.check_in_date like @id or BOOKINGS.CHECK_OUT_DATE like @id or BOOKINGS.ID like @id or BOOKINGS.STATUS like @id)";
        var BookingSearch= HttpContext.Session.GetString("BookingSearch");
        if(BookingSearch != null){
            query=query+ likequery;
        }
        if(id == "id_asc"){
            query = query + " order by BOOKINGS.ID asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="id_desc"){
            query = query + " order by BOOKINGS.ID desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "numberofguest_asc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="numberofguest_desc"){
            query = query + " order by BOOKINGS.NUMBER_OF_GUESTS desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "checkin_asc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="checkin_desc"){
            query = query + " order by BOOKINGS.CHECK_IN_DATE desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "checkout_asc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="checkout_desc"){
            query = query + " order by BOOKINGS.CHECK_OUT_DATE desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "total_asc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="total_desc"){
            query = query + " order by BOOKINGS.TOTAL_PRICE desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "roomname_asc"){
            query = query + " order by ROOMS.NAME asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="roomname_desc"){
            query = query + " order by ROOMS.NAME desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "roomtype_asc"){
            query = query + " order by ROOMS.TYPE asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="roomtype_desc"){
            query = query + " order by ROOMS.TYPE desc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id == "status_asc"){
            query = query + " order by BOOKINGS.STATUS asc";
            bookings_canceled= GetCanceledBooking(query);
        }else if(id =="status_desc"){
            query = query + " order by BOOKINGS.STATUS desc";
            bookings_canceled= GetCanceledBooking(query);
        }           
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedBooking= bookings_canceled.ToPagedList(pageNumber,pageSize);
        ViewBag.booking_list=pagedBooking;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminBookingCanceled.cshtml", pagedBooking);
    }
    public IActionResult Confirm(int id, string status){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            if(status=="Booked"){
                int invoice_id=1;
                int room_id=1;
                string query = "UPDATE BOOKINGS SET STATUS=@status where ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status","Booked");
                command.Parameters.AddWithValue("@id",id);
                MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                query="SELECT INVOICES_DETAILS.INVOICE_ID, BOOKINGS.ROOM_ID FROM INVOICES, INVOICES_DETAILS,BOOKINGS WHERE INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ID=INVOICES_DETAILS.BOOKING_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    invoice_id=reader.GetInt32(0);
                    room_id=reader.GetInt32(1);
                }

                reader.Close();
                query="UPDATE INVOICES SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();

                reader.Close();
                query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",0);
                command.Parameters.AddWithValue("@id",room_id);
                reader = command.ExecuteReader();
                connection.Close();
                SendEmail(id);
            }else if(status=="Refund"){
                int invoice_id=1;
                int room_id=1;
                string query = "UPDATE BOOKINGS SET STATUS=@status where ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status","Refund");
                command.Parameters.AddWithValue("@id",id);
                MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                query="SELECT INVOICES_DETAILS.INVOICE_ID, BOOKINGS.ROOM_ID FROM INVOICES, INVOICES_DETAILS,BOOKINGS WHERE INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ID=INVOICES_DETAILS.BOOKING_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    invoice_id=reader.GetInt32(0);
                    room_id=reader.GetInt32(1);
                }

                reader.Close();
                query="DELETE FROM INVOICES_DETAILS WHERE BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();

                reader.Close();
                query="DELETE FROM INVOICES WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();

                reader.Close();
                query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",room_id);
                reader = command.ExecuteReader();
                connection.Close();
            }else if(status=="Canceled"){
                int invoice_id=1;
                int room_id=1;
                string query = "UPDATE BOOKINGS SET STATUS=@status where ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status","Canceled");
                command.Parameters.AddWithValue("@id",id);
                MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                query="SELECT INVOICES_DETAILS.INVOICE_ID, BOOKINGS.ROOM_ID FROM INVOICES, INVOICES_DETAILS,BOOKINGS WHERE INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ID=INVOICES_DETAILS.BOOKING_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    invoice_id=reader.GetInt32(0);
                    room_id=reader.GetInt32(1);
                }

                reader.Close();
                query="DELETE FROM INVOICES_DETAILS WHERE BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();

                reader.Close();
                query="DELETE FROM INVOICES WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();

                reader.Close();
                query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",room_id);
                reader = command.ExecuteReader();
                connection.Close();
            }else if(status=="Pending"){
                int invoice_id=1;
                int room_id=1;
                string query = "UPDATE BOOKINGS SET STATUS=@status where ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status","Pending");
                command.Parameters.AddWithValue("@id",id);
                MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                query="SELECT INVOICES_DETAILS.INVOICE_ID, BOOKINGS.ROOM_ID FROM INVOICES, INVOICES_DETAILS,BOOKINGS WHERE INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ID=INVOICES_DETAILS.BOOKING_ID AND INVOICES_DETAILS.BOOKING_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    invoice_id=reader.GetInt32(0);
                    room_id=reader.GetInt32(1);
                }

                reader.Close();
                query="UPDATE INVOICES SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",invoice_id);
                reader = command.ExecuteReader();

                reader.Close();
                query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",0);
                command.Parameters.AddWithValue("@id",room_id);
                reader = command.ExecuteReader();
                connection.Close();
            }
            
        }
        return RedirectToAction("AdminBooking");
    }
}

