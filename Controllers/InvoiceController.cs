using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Filters;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using System.Data;

namespace Project.Controllers;

public class InvoiceController : Controller
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
    public IActionResult AdminInvoice(int page)
    {
        List<Invoice> invoices = new List<Invoice>();
        List<Customer> customers = new List<Customer>();
        HttpContext.Session.Remove("InvoiceSearch");
        HttpContext.Session.Remove("InvoiceSearch1");
        HttpContext.Session.Remove("InvoiceSearch2");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.STATUS=1 order by INVOICES.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }

            reader.Close();
            query = "SELECT CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL FROM CUSTOMERS ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Customer customer = new Customer
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2)
                };
                customers.Add(customer);
            }
            connection.Close();
        }
        // ViewBag.invoice_list = invoices;
        ViewBag.customer_list = customers;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_list=pagedInvoice;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminInvoice.cshtml",pagedInvoice);
    }
    public IActionResult AdminInvoiceReport(int page)
    {
        List<Invoice> invoices = new List<Invoice>();
        ViewBag.employee_avatar=GetAvatar();
        HttpContext.Session.Remove("InvoiceReportSearch");
        HttpContext.Session.Remove("InvoiceReportSearch1");
        HttpContext.Session.Remove("InvoiceReportSearch2");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID order by INVOICES.ID asc";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }
            connection.Close();
        }
        ViewBag.invoice_report_list = invoices;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_report_list=pagedInvoice;
        return View("~/Views/HotelViews/AdminInvoiceReport.cshtml", pagedInvoice);
    }
    public IActionResult AdminCreateInvoice()
    {
        List<Booking> bookings = new List<Booking>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM BOOKINGS order by ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Booking booking = new Booking
                {
                    id = reader.GetInt32(0),
                };
                bookings.Add(booking);
            }

            reader.Close();
            query = "SELECT taxes_name FROM TAXES order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0),
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0),
                };
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.booking_list = bookings;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminCreateInvoice.cshtml");
    }
    public IActionResult AdminEditInvoice()
    {
        List<Booking> bookings = new List<Booking>();
        List<Invoice> invoices = new List<Invoice>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM BOOKINGS order by ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Booking booking = new Booking
                {
                    id = reader.GetInt32(0),
                };
                bookings.Add(booking);
            }

            reader.Close();
            query = "SELECT taxes_name FROM TAXES order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0),
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0),
                };
                promotions.Add(promotion);
            }

            reader.Close();
            query = "SELECT ID FROM INVOICES order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Invoice invoice = new Invoice
                {
                    id = reader.GetInt32(0),
                };
                invoices.Add(invoice);
            }
            connection.Close();
        }
        ViewBag.booking_list = bookings;
        ViewBag.invoice_list = invoices;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminEditInvoice.cshtml");
    }
    [HttpPost]
    public IActionResult AdminInsertInvoice(Invoice invoice)
    {
        int invoice_id = 1;
        int payment_id = 1;
        int taxes_id = 1;
        int promotion_id = 1;
        int customer_id = 1;
        int invoice_details_id = 1;
        ModelState.Remove("taxes.taxes_name");
        ModelState.Remove("taxes.tax_percentage");
        ModelState.Remove("taxes.status");
        ModelState.Remove("promotion.name");
        ModelState.Remove("promotion.valid_to");
        ModelState.Remove("promotion.valid_from");
        ModelState.Remove("promotion.description");
        ModelState.Remove("status");
        ModelState.Remove("promotion.discount_percent");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminCreateInvoice");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM INVOICES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (invoice_id == reader.GetInt32(0))
                {
                    invoice_id = invoice_id + 1;
                }
            }

            string invoice_number = "Invoice-" + invoice_id;
            reader.Close();
            query = "INSERT INTO INVOICES (ID,INVOICE_NUMBER,TOTAL_AMOUNT,STATUS) VALUES(@id, @invoice_number,@total_amount,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice_id);
            command.Parameters.AddWithValue("@invoice_number", invoice_number);
            command.Parameters.AddWithValue("@total_amount", invoice.total_amount);
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();

            reader.Close();
            query = "SELECT CUSTOMER_ID FROM BOOKINGS WHERE ID=@booking";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@booking", invoice.booking_id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                customer_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT ID FROM PAYMENT ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (payment_id == reader.GetInt32(0))
                {
                    payment_id = payment_id + 1;
                }
            }

            reader.Close();
            query = "INSERT INTO PAYMENT (ID,CUSTOMER_ID,AMOUNT,PAYMENT_TYPE,PAYMENT_DATE,STATUS) VALUES(@id, @customer_id,@amount,@payment_type,@payment_date,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", payment_id);
            command.Parameters.AddWithValue("@customer_id", customer_id);
            command.Parameters.AddWithValue("@amount", invoice.total_amount);
            command.Parameters.AddWithValue("@payment_type", "Paypal");
            command.Parameters.AddWithValue("@payment_date", invoice.create_date);
            command.Parameters.AddWithValue("@status", "Full payment");
            reader = command.ExecuteReader();

            reader.Close();
            query = "SELECT ID FROM TAXES WHERE TAXES_NAME=@name";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", invoice?.taxes?.taxes_name);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                taxes_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT ID FROM PROMOTIONS WHERE NAME=@name";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", invoice?.promotion?.name);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                promotion_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT ID FROM INVOICES_DETAILS order by id asc";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (invoice_details_id == reader.GetInt32(0))
                {
                    invoice_details_id = invoice_details_id + 1;
                }
            }
            
            reader.Close();
            query = "INSERT INTO INVOICES_DETAILS (ID,INVOICE_ID,BOOKING_ID,CREATE_DATE,DUE_DATE,PAYMENT_ID,TAXES_ID, PROMOTION_ID) VALUES(@id, @invoice_id,@booking_id,@create_date,@due_date,@payment_id,@taxes_id,@promotion_id)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice_details_id);
            command.Parameters.AddWithValue("@invoice_id", invoice_id);
            command.Parameters.AddWithValue("@booking_id", invoice?.booking_id);
            command.Parameters.AddWithValue("@create_date", invoice?.create_date);
            command.Parameters.AddWithValue("@due_date", invoice?.due_date);
            command.Parameters.AddWithValue("@payment_id", payment_id);
            command.Parameters.AddWithValue("@taxes_id", taxes_id);
            command.Parameters.AddWithValue("@promotion_id", promotion_id);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminInvoice");
    }
    [HttpPost]
    public IActionResult AdminUpdateInvoice(Invoice invoice)
    {
        int payment_id = 1;
        int taxes_id = 1;
        int promotion_id = 1;
        int invoice_details_id = 1;
        ModelState.Remove("taxes.taxes_name");
        ModelState.Remove("taxes.tax_percentage");
        ModelState.Remove("taxes.status");
        ModelState.Remove("promotion.name");
        ModelState.Remove("promotion.valid_to");
        ModelState.Remove("promotion.valid_from");
        ModelState.Remove("promotion.description");
        ModelState.Remove("status");
        ModelState.Remove("promotion.discount_percent");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditInvoice");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE INVOICES SET TOTAL_AMOUNT=@total_amount,STATUS=@status WHERE ID =@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@total_amount", invoice.total_amount);
            command.Parameters.AddWithValue("@status", 1);
            command.Parameters.AddWithValue("@id", invoice.id);
            MySqlDataReader reader = command.ExecuteReader();

            reader.Close();
            query = "SELECT PAYMENT.ID FROM INVOICES,INVOICES_DETAILS, PAYMENT WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and INVOICES.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice.id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                payment_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "UPDATE PAYMENT SET AMOUNT=@amount,PAYMENT_DATE=@payment_date WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", payment_id);
            command.Parameters.AddWithValue("@amount", invoice.total_amount);
            command.Parameters.AddWithValue("@payment_date", invoice.create_date);
            reader = command.ExecuteReader();

            reader.Close();
            query = "SELECT ID FROM TAXES WHERE TAXES_NAME=@name";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", invoice?.taxes?.taxes_name);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                taxes_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT ID FROM PROMOTIONS WHERE NAME=@name";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", invoice?.promotion?.name);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                promotion_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT INVOICES_DETAILS.ID FROM INVOICES_DETAILS, INVOICES WHERE INVOICES.ID=@id and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice?.id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                invoice_details_id = reader.GetInt32(0);
            }

            reader.Close();
            query = "DELETE FROM INVOICES_DETAILS WHERE INVOICE_ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice!.id);
            reader = command.ExecuteReader();

            reader.Close();
            query = "INSERT INTO INVOICES_DETAILS (ID,INVOICE_ID,BOOKING_ID,CREATE_DATE,DUE_DATE,PAYMENT_ID,TAXES_ID, PROMOTION_ID) VALUES(@id, @invoice_id,@booking_id,@create_date,@due_date,@payment_id,@taxes_id,@promotion_id)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", invoice_details_id);
            command.Parameters.AddWithValue("@invoice_id", invoice?.id);
            command.Parameters.AddWithValue("@booking_id", invoice?.booking_id);
            command.Parameters.AddWithValue("@create_date", invoice?.create_date);
            command.Parameters.AddWithValue("@due_date", invoice?.due_date);
            command.Parameters.AddWithValue("@payment_id", payment_id);
            command.Parameters.AddWithValue("@taxes_id", taxes_id);
            command.Parameters.AddWithValue("@promotion_id", promotion_id);
            reader = command.ExecuteReader();

            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminInvoice");
    }
    [HttpPost]
    public IActionResult GetPrice(string selectedOption)
    {
        float? price = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT TOTAL_PRICE FROM BOOKINGS WHERE ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                price = reader.GetInt32(0);
            }
            connection.Close();
        }
        return Json(new { price = price });
    }
    [HttpPost]
    public IActionResult GetPrice2(string selectedOption)
    {
        float? price = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT PRICE FROM SERVICES WHERE name=@name";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                price = reader.GetFloat(0);
            }
            connection.Close();
        }
        return Json(new { price = price });
    }
    [HttpPost]
    public IActionResult GetPrice3(string selectedOption)
    {
        int booking_id=0;
        float? price = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT INVOICES_DETAILS.BOOKING_ID, INVOICES.TOTAL_AMOUNT FROM INVOICES, INVOICES_DETAILS WHERE INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                booking_id=reader.GetInt32(0);
                price = reader.GetFloat(1);
            }
            connection.Close();
        }
        return Json(new { price = price, booking_id=booking_id });
    }
    [HttpPost]
    public IActionResult GetTaxes(string selectedOption)
    {
        float? price = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT TAX_PERCENTAGE FROM TAXES WHERE taxes_name=@taxname";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@taxname", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                price = reader.GetFloat(0);
            }
            connection.Close();
        }
        return Json(new { price = price });
    }
    [HttpPost]
    public IActionResult GetPromotion(string selectedOption)
    {
        float? price = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT discount_percent FROM promotions WHERE name=@name";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                price = reader.GetFloat(0);
            }
            connection.Close();
        }
        return Json(new { price = price });
    }
    [HttpPost]
    public IActionResult RedirectAdminCreateInvoice(){
        return RedirectToAction("AdminCreateInvoice");
    }
    public IActionResult EditInvoice(int id)
    {
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditInvoice");
        }
        List<Booking> bookings = new List<Booking>();
        List<Invoice> invoices = new List<Invoice>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        Invoice invoice1=new Invoice();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM BOOKINGS order by ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Booking booking = new Booking
                {
                    id = reader.GetInt32(0),
                };
                bookings.Add(booking);
            }

            reader.Close();
            query = "SELECT taxes_name FROM TAXES order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0),
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0),
                };
                promotions.Add(promotion);
            }

            reader.Close();
            query = "SELECT ID FROM INVOICES order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Invoice invoice = new Invoice
                {
                    id = reader.GetInt32(0),
                };
                invoices.Add(invoice);
            }

            reader.Close();
            query = "SELECT INVOICES.ID, INVOICES.TOTAL_AMOUNT, INVOICES_DETAILS.BOOKING_ID, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE, IFNULL(TAXES.TAXES_NAME, '') AS TAXES_NAME, IFNULL(PROMOTIONS.NAME, '') as PROMOTION_NAME FROM INVOICES LEFT JOIN INVOICES_DETAILS ON INVOICES.ID = INVOICES_DETAILS.INVOICE_ID LEFT JOIN TAXES ON INVOICES_DETAILS.TAXES_ID = TAXES.ID LEFT JOIN PROMOTIONS ON INVOICES_DETAILS.PROMOTION_ID = PROMOTIONS.ID WHERE INVOICES.ID =@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            invoice1.taxes=new Taxes();
            invoice1.promotion=new Promotion();
            while(reader.Read()){
                invoice1.id=reader.GetInt32(0);
                invoice1.total_amount=reader.GetFloat(1);
                invoice1.booking_id=reader.GetInt32(2);
                DateTime day = reader.GetDateTime(3);
                DateTime day1 = reader.GetDateTime(4);
                invoice1.create_date=day.ToString("yyyy-MM-dd");
                invoice1.due_date=day1.ToString("yyyy-MM-dd");
                invoice1.taxes.taxes_name=reader.GetString(5);
                invoice1.promotion.name=reader.GetString(6);
            }
            connection.Close();
        }
        ViewBag.booking_list = bookings;
        ViewBag.invoice_list = invoices;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminEditInvoice.cshtml",invoice1);
    }
    public IActionResult DeleteInvoice(int id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM INVOICES WHERE ID=@id";
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
        return RedirectToAction("AdminInvoice");
    }
    public IActionResult AdminSearchInvoice(string searchkeyword, string searchkeyword1,string searchkeyword2, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        List<Invoice> invoices = new List<Invoice>();
        List<Customer> customers = new List<Customer>();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("InvoiceSearch", searchkeyword);
        }
        if(searchkeyword1 !=null){
            HttpContext.Session.SetString("InvoiceSearch1", searchkeyword1);
        }
        if(searchkeyword2 !=null){
            HttpContext.Session.SetString("InvoiceSearch2", searchkeyword2);
        }
        var a=HttpContext.Session.GetString("InvoiceSearch");
        var b=HttpContext.Session.GetString("InvoiceSearch1");
        var c=HttpContext.Session.GetString("InvoiceSearch2");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b != null && searchkeyword1 == null){
            searchkeyword1=b;
        }
        if(c != null && searchkeyword2 ==null){
            searchkeyword2=c;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="";
            if(searchkeyword != null && searchkeyword1== null && searchkeyword2 == null ){
                query = "SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID  and (INVOICES.INVOICE_NUMBER like @id) order by INVOICES.ID asc";
            }else if(searchkeyword == null && searchkeyword1!= null && searchkeyword2 != null){
                query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and (INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2) order by INVOICES.ID asc";  
            }else if(searchkeyword != null && searchkeyword1!= null && searchkeyword2 != null){
                query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and (INVOICES.INVOICE_NUMBER like @id and INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2) order by INVOICES.ID asc";  
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
            command.Parameters.AddWithValue("@id2",searchkeyword2);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }

            reader.Close();
            query = "SELECT CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL FROM CUSTOMERS ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Customer customer = new Customer
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2)
                };
                customers.Add(customer);
            }
            connection.Close();
        }
        // ViewBag.invoice_list = invoices;
        ViewBag.customer_list = customers;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_list=pagedInvoice;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminInvoice.cshtml", pagedInvoice);
    }
    public IActionResult AdminSearchInvoiceReport(string searchkeyword, string searchkeyword1,string searchkeyword2, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        List<Invoice> invoices = new List<Invoice>();
        List<Customer> customers = new List<Customer>();
        var a=HttpContext.Session.GetString("InvoiceReportSearch");
        var b=HttpContext.Session.GetString("InvoiceReportSearch1");
        var c=HttpContext.Session.GetString("InvoiceReportSearch2");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        if(b != null && searchkeyword1 == null){
            searchkeyword1=b;
        }
        if(c != null && searchkeyword2 ==null){
            searchkeyword2=c;
        }
        if(searchkeyword !=null){
            HttpContext.Session.SetString("InvoiceReportSearch", searchkeyword);
        }
        if(searchkeyword1 !=null){
            HttpContext.Session.SetString("InvoiceReportSearch1", searchkeyword1);
        }
        if(searchkeyword2 !=null){
            HttpContext.Session.SetString("InvoiceReportSearch2", searchkeyword2);
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="";
            if(searchkeyword != null && searchkeyword1== null && searchkeyword2 == null ){
                query = "SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID  and (INVOICES.INVOICE_NUMBER like @id) order by INVOICES.ID asc";
            }else if(searchkeyword == null && searchkeyword1!= null && searchkeyword2 != null){
                query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and (INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2) order by INVOICES.ID asc";  
            }else if(searchkeyword != null && searchkeyword1!= null && searchkeyword2 != null){
                query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and (INVOICES.INVOICE_NUMBER like @id and INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2) order by INVOICES.ID asc";  
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
            command.Parameters.AddWithValue("@id2",searchkeyword2);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }

            reader.Close();
            query = "SELECT CUSTOMERS.NAME, CUSTOMERS.PHONE, CUSTOMERS.EMAIL FROM CUSTOMERS ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Customer customer = new Customer
                {
                    name = reader.GetString(0),
                    phone = reader.GetString(1),
                    email = reader.GetString(2)
                };
                customers.Add(customer);
            }
            connection.Close();
        }
        // ViewBag.invoice_report_list = invoices;
        ViewBag.customer_list = customers;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_report_list=pagedInvoice;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminInvoiceReport.cshtml", pagedInvoice);
    }
    public List<Invoice> GetAllInvoice(string query)
    {
        List<Invoice> invoices = new List<Invoice>();
        var searchkeyword=HttpContext.Session.GetString("InvoiceSearch");
        var searchkeyword1=HttpContext.Session.GetString("InvoiceSearch1");
        var searchkeyword2=HttpContext.Session.GetString("InvoiceSearch2");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
            command.Parameters.AddWithValue("@id2",searchkeyword2);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }
            connection.Close();
        }
        return invoices;
    }
    public List<Invoice> GetAllInvoiceReport(string query)
    {
        List<Invoice> invoices = new List<Invoice>();
        var searchkeyword=HttpContext.Session.GetString("InvoiceReportSearch");
        var searchkeyword1=HttpContext.Session.GetString("InvoiceReportSearch1");
        var searchkeyword2=HttpContext.Session.GetString("InvoiceReportSearch2");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
            command.Parameters.AddWithValue("@id2",searchkeyword2);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString = "Active";
                int cus_status = Convert.ToInt32(reader["status"]);
                if (cus_status == 1)
                {
                    statusString = "Active";
                }
                else
                {
                    statusString = "Inactive";
                }
                Invoice invoice = new Invoice
                {
                    id = (int)reader["id"],
                    name = (string)reader["name"],
                    phonenumber = (string)reader["phone"],
                    email = (string)reader["email"],
                    invoice_number = (string)reader["invoice_number"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                invoices.Add(invoice);
            }

            reader.Close();
            foreach (Invoice invoice1 in invoices)
            {
                query = "SELECT distinct ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID and INVOICES.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", invoice1.id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime day = reader.GetDateTime(1);
                    DateTime day1 = reader.GetDateTime(2);
                    InvoiceDetails invoiceDetails = new InvoiceDetails
                    {
                        room_type = reader.GetString(0),
                        create_date = day.ToString("dd-MM-yyyy"),
                        due_date = day.ToString("dd-MM-yyyy"),
                    };
                    if (invoice1.invoiceDetails == null)
                    {
                        invoice1.invoiceDetails = new List<InvoiceDetails>();
                    }
                    invoice1.invoiceDetails.Add(invoiceDetails);
                }
                reader.Close();
            }
            connection.Close();
        }
        return invoices;
    }
    public IActionResult AdminSortInvoice(string id, int page){
        List<Invoice> invoices = new List<Invoice>();
        ViewBag.employee_avatar=GetAvatar();
        var searchkeyword=HttpContext.Session.GetString("InvoiceSearch");
        var searchkeyword1=HttpContext.Session.GetString("InvoiceSearch1");
        var searchkeyword2=HttpContext.Session.GetString("InvoiceSearch2");
        string query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID";
        if(searchkeyword != null && searchkeyword1== null && searchkeyword2 == null ){
            query = query + " and (INVOICES.INVOICE_NUMBER like @id)";
        }else if(searchkeyword == null && searchkeyword1!= null && searchkeyword2 != null){
            query = query + " and (INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2)";  
        }else if(searchkeyword != null && searchkeyword1!= null && searchkeyword2 != null){
            query = query + " and (INVOICES.INVOICE_NUMBER like @id and INVOICES_DETAILS.CREATE_DATE >= @id1 and INVOICES_DETAILS.CREATE_DATE <= @id2)";  
        }
        if(id == "id_asc"){
            query = query + " order by INVOICES.ID asc";
            invoices=GetAllInvoice(query);
        }else if(id == "id_desc"){
            query = query + " order by INVOICES.ID desc";
            invoices=GetAllInvoice(query);
        }else if(id == "number_asc"){
            query = query + " order by INVOICES.INVOICE_NUMBER asc";
            invoices=GetAllInvoice(query);
        }else if(id == "number_desc"){
            query = query + " order by INVOICES.INVOICE_NUMBER desc";
            invoices=GetAllInvoice(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            invoices=GetAllInvoice(query);
        }else if(id == "name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            invoices=GetAllInvoice(query);
        }else if(id == "phone_asc"){
            query = query + " order by CUSTOMERS.PHONE asc";
            invoices=GetAllInvoice(query);
        }else if(id == "phone_desc"){
            query = query + " order by CUSTOMERS.PHONE desc";
            invoices=GetAllInvoice(query);
        }else if(id == "email_asc"){
            query = query + " order by CUSTOMERS.EMAIL asc";
            invoices=GetAllInvoice(query);
        }else if(id == "email_desc"){
            query = query + " order by CUSTOMERS.EMAIL desc";
            invoices=GetAllInvoice(query);
        }else if(id == "createdate_asc"){
            query = query + " order by INVOICES_DETAILS.CREATE_DATE asc";
            invoices=GetAllInvoice(query);
        }else if(id == "createdate_desc"){
            query = query + " order by INVOICES_DETAILS.CREATE_DATE desc";
            invoices=GetAllInvoice(query);
        }else if(id == "duedate_asc"){
            query = query + " order by INVOICES_DETAILS.DUE_DATE asc";
            invoices=GetAllInvoice(query);
        }else if(id == "duedate_desc"){
            query = query + " order by INVOICES_DETAILS.DUE_DATE desc";
            invoices=GetAllInvoice(query);
        }else if(id == "type_asc"){
            query = query + " order by ROOMS.TYPE asc";
            invoices=GetAllInvoice(query);
        }else if(id == "type_desc"){
            query = query + " order by ROOMS.TYPE desc";
            invoices=GetAllInvoice(query);
        }else if(id == "total_asc"){
            query = query + " order by INVOICES.TOTAL_AMOUNT asc";
            invoices=GetAllInvoice(query);
        }else if(id == "total_desc"){
            query = query + " order by INVOICES.TOTAL_AMOUNT desc";
            invoices=GetAllInvoice(query);
        }else if(id == "status_asc"){
            query = query + " order by INVOICES.STATUS asc";
            invoices=GetAllInvoice(query);
        }else if(id == "status_desc"){
            query = query + " order by INVOICES.STATUS desc";
            invoices=GetAllInvoice(query);
        }
        ViewBag.invoice_list = invoices;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_list=pagedInvoice;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminInvoice.cshtml", pagedInvoice);
    }
    public IActionResult AdminSortInvoiceReport(string id, int page){
        List<Invoice> invoices = new List<Invoice>();
        ViewBag.employee_avatar=GetAvatar();
        var searchkeyword=HttpContext.Session.GetString("InvoiceReportSearch");
        var searchkeyword1=HttpContext.Session.GetString("InvoiceReportSearch1");
        var searchkeyword2=HttpContext.Session.GetString("InvoiceReportSearch2");
        string query="SELECT distinct INVOICES.ID, CUSTOMERS.NAME,CUSTOMERS.PHONE,CUSTOMERS.EMAIL, INVOICES.INVOICE_NUMBER, INVOICES.TOTAL_AMOUNT, INVOICES.STATUS, ROOMS.TYPE, INVOICES_DETAILS.CREATE_DATE, INVOICES_DETAILS.DUE_DATE FROM BOOKINGS, INVOICES, INVOICES_DETAILS, PAYMENT, CUSTOMERS, ROOMS WHERE INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID and INVOICES.ID=INVOICES_DETAILS.INVOICE_ID and INVOICES_DETAILS.PAYMENT_ID=PAYMENT.ID and PAYMENT.CUSTOMER_ID= CUSTOMERS.ID and BOOKINGS.ROOM_ID=ROOMS.ID";
        if(searchkeyword != null && searchkeyword1== null && searchkeyword2 == null ){
            query = query + " and (INVOICES.INVOICE_NUMBER like @id)";
        }else if(searchkeyword == null && searchkeyword1!= null && searchkeyword2 != null){
            query = query + " and (INVOICES_DETAILS.CREATE_DATE BETWEEN  @id1 and @id2)";  
        }else if(searchkeyword != null && searchkeyword1!= null && searchkeyword2 != null){
            query = query + " and (INVOICES.INVOICE_NUMBER like @id and INVOICES_DETAILS.CREATE_DATE BETWEEN  @id1 and @id2)";  
        }
        if(id == "id_asc"){
            query = query + " order by INVOICES.ID asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "id_desc"){
            query = query + " order by INVOICES.ID desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "number_asc"){
            query = query + " order by INVOICES.INVOICE_NUMBER asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "number_desc"){
            query = query + " order by INVOICES.INVOICE_NUMBER desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "name_asc"){
            query = query + " order by CUSTOMERS.NAME asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "name_desc"){
            query = query + " order by CUSTOMERS.NAME desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "createdate_asc"){
            query = query + " order by INVOICES_DETAILS.CREATE_DATE asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "createdate_desc"){
            query = query + " order by INVOICES_DETAILS.CREATE_DATE desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "duedate_asc"){
            query = query + " order by INVOICES_DETAILS.DUE_DATE asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "duedate_desc"){
            query = query + " order by INVOICES_DETAILS.DUE_DATE desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "type_asc"){
            query = query + " order by ROOMS.TYPE asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "type_desc"){
            query = query + " order by ROOMS.TYPE desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "total_asc"){
            query = query + " order by INVOICES.TOTAL_AMOUNT asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "total_desc"){
            query = query + " order by INVOICES.TOTAL_AMOUNT desc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "status_asc"){
            query = query + " order by INVOICES.STATUS asc";
            invoices=GetAllInvoiceReport(query);
        }else if(id == "status_desc"){
            query = query + " order by INVOICES.STATUS desc";
            invoices=GetAllInvoiceReport(query);
        }
        ViewBag.invoice_list = invoices;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedInvoice= invoices.ToPagedList(pageNumber,pageSize);
        ViewBag.invoice_report_list=pagedInvoice;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminInvoiceReport.cshtml", pagedInvoice);
    }
    public IActionResult PrintInvoice(int id){
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
            string query = "SELECT CUSTOMERS.NAME, CUSTOMERS.ADDRESS, CUSTOMERS.PHONE, CUSTOMERS.EMAIL, INVOICES.ID, INVOICES_DETAILS.CREATE_DATE, ROOMS.ID, ROOMS.NAME, ROOMS.TYPE,ROOMS.PRICE, BOOKINGS.NUMBER_OF_GUESTS, SERVICES.ID, SERVICES.NAME, SERVICES.DESCRIPTION, SERVICES.PRICE, PROMOTIONS.DISCOUNT_PERCENT, TAXES.TAX_PERCENTAGE, BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE FROM CUSTOMERS, INVOICES, INVOICES_DETAILS LEFT JOIN PROMOTIONS ON INVOICES_DETAILS.PROMOTION_ID = PROMOTIONS.ID LEFT JOIN TAXES ON INVOICES_DETAILS.TAXES_ID=TAXES.ID,ROOMS,PAYMENT, BOOKINGS LEFT JOIN BOOKING_SERVICE ON BOOKINGS.ID=BOOKING_SERVICE.BOOKING_ID LEFT JOIN SERVICES ON SERVICES.ID=BOOKING_SERVICE.SERVICE_ID WHERE CUSTOMERS.ID=PAYMENT.CUSTOMER_ID AND PAYMENT.ID=INVOICES_DETAILS.PAYMENT_ID AND INVOICES_DETAILS.BOOKING_ID=BOOKINGS.ID AND INVOICES_DETAILS.INVOICE_ID=INVOICES.ID AND BOOKINGS.ROOM_ID=ROOMS.ID AND INVOICES.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
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
        return View("~/Views/HotelViews/Invoice.cshtml");
    }
}