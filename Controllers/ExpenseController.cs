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

public class ExpenseController : Controller
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
    public IActionResult AdminExpense(int page)
    {
        HttpContext.Session.Remove("ExpenseSearch");
        HttpContext.Session.Remove("ExpenseSearch1");
        List<Expenses> expenses = new List<Expenses>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID order by EXPENSES.ID asc";
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
                DateTime day = reader.GetDateTime(6);

                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        // ViewBag.expense_list = expenses;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_list=pagedExpense;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminExpense.cshtml",pagedExpense);
    }
    public IActionResult AdminAddExpense()
    {
        List<Furniture> furnitures = new List<Furniture>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE_NAME FROM FURNITURE ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    name = reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT TAXES_NAME FROM TAXES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0)
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0)
                };
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.count = 1;
        ViewBag.furniture_list = furnitures;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminAddExpense.cshtml");
    }
    public IActionResult AdminAddExpense2(int count)
    {
        List<Furniture> furnitures = new List<Furniture>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE_NAME FROM FURNITURE ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    name = reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT TAXES_NAME FROM TAXES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0)
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0)
                };
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.count = count;
        ViewBag.furniture_list = furnitures;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminAddExpense.cshtml");
    }
    public IActionResult AdminEditExpense()
    {
        List<Furniture> furnitures = new List<Furniture>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        List<Expenses> expenses = new List<Expenses>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE_NAME FROM FURNITURE ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    name = reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT TAXES_NAME FROM TAXES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0)
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0)
                };
                promotions.Add(promotion);
            }

            reader.Close();
            query = "SELECT ID FROM EXPENSES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Expenses expenses1 = new Expenses
                {
                    id = reader.GetInt32(0)
                };
                expenses.Add(expenses1);
            }
            connection.Close();
        }
        ViewBag.count = 1;
        ViewBag.expense_list=expenses;
        ViewBag.furniture_list = furnitures;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminEditExpense.cshtml");
    }
    public IActionResult AdminExpenseReport(int page)
    {
        HttpContext.Session.Remove("ExpenseReportSearch");
        HttpContext.Session.Remove("ExpenseReportSearch1");
        List<Expenses> expenses = new List<Expenses>();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID order by EXPENSES.ID asc";
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
                DateTime day = reader.GetDateTime(6);
                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        // ViewBag.expense_report_list = expenses;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_report_list=pagedExpense;
        return View("~/Views/HotelViews/AdminExpenseReport.cshtml",pagedExpense);
    }
    [HttpPost]
    public IActionResult GetCount(Expenses expenses)
    {
        int count = expenses.count + 1;
        return RedirectToAction("AdminAddExpense2", new { count = count });
    }
    [HttpPost]
    public IActionResult GetCount2(Expenses expenses)
    {
        int count = expenses.count - 1;
        return RedirectToAction("AdminAddExpense2", new { count = count });
    }
    [HttpPost]
    public IActionResult AdminInsertExpense(Expenses expenses)
    {
        int expense_id = 1;
        int expense_details_id = 1;
        float? total_amount = 0;
        int taxes_id = 1;
        int promotion_id = 1;
        int furniture_id = 1;
        for(int i=0;i< expenses.expensesDetails.Count;i++){
            ModelState.Remove("expensesDetails["+i+"].taxes.status");
            ModelState.Remove("expensesDetails["+i+"].taxes.taxes_name");
            ModelState.Remove("expensesDetails["+i+"].taxes.tax_percentage");
            ModelState.Remove("expensesDetails["+i+"].furniture.name");
            ModelState.Remove("expensesDetails["+i+"].furniture.type");
            ModelState.Remove("expensesDetails["+i+"].furniture.price");
            ModelState.Remove("expensesDetails["+i+"].furniture.quanity");
            ModelState.Remove("expensesDetails["+i+"].promotion.name");
            ModelState.Remove("expensesDetails["+i+"].promotion.valid_to");
            ModelState.Remove("expensesDetails["+i+"].promotion.valid_from");
            ModelState.Remove("expensesDetails["+i+"].promotion.description");
            ModelState.Remove("expensesDetails["+i+"].promotion.discount_percent");
            ModelState.Remove("expensesDetails["+i+"].amount");
            ModelState.Remove("expensesDetails["+i+"].quanity");
            ModelState.Remove("expensesDetails["+i+"].purchase_at");
            ModelState.Remove("expensesDetails["+i+"].purchase_from");
        }
        ModelState.Remove("status");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddExpense");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM EXPENSES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (expense_id == reader.GetInt32(0))
                {
                    expense_id = expense_id + 1;
                }
            }

            reader.Close();
            foreach (var expense in expenses.expensesDetails)
            {
                total_amount = total_amount + expense.amount;
            }
            string expense_number = "EX-" + expense_id;
            query = "INSERT INTO EXPENSES (ID,EXPENSE_NUMBER,EMPLOYEE_ID,TOTAL_AMOUNT,STATUS) VALUES (@id, @expense_number,@employee_id,@total_amount,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", expense_id);
            command.Parameters.AddWithValue("@expense_number", expense_number);
            command.Parameters.AddWithValue("@employee_id", 1);
            command.Parameters.AddWithValue("@total_amount", total_amount);
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();

            foreach (var expense in expenses.expensesDetails)
            {
                reader.Close();
                query = "SELECT ID FROM EXPENSES_DETAILS ORDER BY ID ASC";
                command = new MySqlCommand(query, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (expense_details_id == reader.GetInt32(0))
                    {
                        expense_details_id = expense_details_id + 1;
                    }
                }

                reader.Close();
                query = "SELECT ID FROM TAXES WHERE TAXES_NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.taxes!.taxes_name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    taxes_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "SELECT ID FROM PROMOTIONS WHERE NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.promotion!.name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    promotion_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "SELECT ID FROM FURNITURE WHERE FURNITURE_NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.furniture!.name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    furniture_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "INSERT INTO EXPENSES_DETAILS (ID,EXPENSE_ID,QUANITY,PURCHASE_AT,PURCHASE_FROM,AMOUNT,TAXES_ID,PROMOTION_ID,FURNITURE_ID) VALUES (@id, @expense_id,@quantity,@purchase_at,@purchase_from, @amount,@taxes_id,@promotion_id,@furniture_id)";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", expense_details_id);
                command.Parameters.AddWithValue("@expense_id", expense_id);
                command.Parameters.AddWithValue("@quantity", expense.quanity);
                command.Parameters.AddWithValue("@purchase_at", expenses.purchase_at);
                command.Parameters.AddWithValue("@purchase_from", expenses.purchase_from);
                command.Parameters.AddWithValue("@amount", expense.amount);
                command.Parameters.AddWithValue("@taxes_id", taxes_id);
                command.Parameters.AddWithValue("@promotion_id", promotion_id);
                command.Parameters.AddWithValue("@furniture_id", furniture_id);
                reader = command.ExecuteReader();

                expense_details_id=1;
            }
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminExpense");
    }
    [HttpPost]
    public IActionResult AdminUpdateExpense(Expenses expenses)
    {
        int expense_id = 1;
        int expense_details_id = 1;
        float? total_amount = 0;
        int taxes_id = 1;
        int promotion_id = 1;
        int furniture_id = 1;
        for(int i=0;i< expenses.expensesDetails.Count;i++){
            ModelState.Remove("expensesDetails["+i+"].taxes.status");
            ModelState.Remove("expensesDetails["+i+"].taxes.taxes_name");
            ModelState.Remove("expensesDetails["+i+"].taxes.tax_percentage");
            ModelState.Remove("expensesDetails["+i+"].furniture.name");
            ModelState.Remove("expensesDetails["+i+"].furniture.type");
            ModelState.Remove("expensesDetails["+i+"].furniture.price");
            ModelState.Remove("expensesDetails["+i+"].furniture.quanity");
            ModelState.Remove("expensesDetails["+i+"].promotion.name");
            ModelState.Remove("expensesDetails["+i+"].promotion.valid_to");
            ModelState.Remove("expensesDetails["+i+"].promotion.valid_from");
            ModelState.Remove("expensesDetails["+i+"].promotion.description");
            ModelState.Remove("expensesDetails["+i+"].promotion.discount_percent");
            ModelState.Remove("expensesDetails["+i+"].amount");
            ModelState.Remove("expensesDetails["+i+"].quanity");
            ModelState.Remove("expensesDetails["+i+"].purchase_at");
            ModelState.Remove("expensesDetails["+i+"].purchase_from");
        }
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditExpense");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM EXPENSES ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (expense_id == reader.GetInt32(0))
                {
                    expense_id = expense_id + 1;
                }
            }

            reader.Close();
            foreach (var expense in expenses.expensesDetails)
            {
                total_amount = total_amount + expense.amount;
            }
            string expense_number = "EX-" + expense_id;
            query = "INSERT INTO EXPENSES (ID,EXPENSE_NUMBER,EMPLOYEE_ID,TOTAL_AMOUNT,STATUS) VALUES (@id, @expense_number,@employee_id,@total_amount,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", expense_id);
            command.Parameters.AddWithValue("@expense_number", expense_number);
            command.Parameters.AddWithValue("@employee_id", 1);
            command.Parameters.AddWithValue("@total_amount", total_amount);
            command.Parameters.AddWithValue("@status", 1);
            reader = command.ExecuteReader();

            foreach (var expense in expenses.expensesDetails)
            {
                reader.Close();
                query = "SELECT ID FROM EXPENSES_DETAILS ORDER BY ID ASC";
                command = new MySqlCommand(query, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (expense_details_id == reader.GetInt32(0))
                    {
                        expense_details_id = expense_details_id + 1;
                    }
                }

                reader.Close();
                query = "SELECT ID FROM TAXES WHERE TAXES_NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.taxes!.taxes_name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    taxes_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "SELECT ID FROM PROMOTIONS WHERE NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.promotion!.name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    promotion_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "SELECT ID FROM FURNITURE WHERE FURNITURE_NAME=@name";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", expense.furniture!.name);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    furniture_id = reader.GetInt32(0);
                }

                reader.Close();
                query = "INSERT INTO EXPENSES_DETAILS (ID,EXPENSE_ID,QUANITY,PURCHASE_AT,PURCHASE_FROM,AMOUNT,TAXES_ID,PROMOTION_ID,FURNITURE_ID) VALUES (@id, @expense_id,@quantity,@purchase_at,@purchase_from, @amount,@taxes_id,@promotion_id,@furniture_id)";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", expense_details_id);
                command.Parameters.AddWithValue("@expense_id", expense_id);
                command.Parameters.AddWithValue("@quantity", expense.quanity);
                command.Parameters.AddWithValue("@purchase_at", expenses.purchase_at);
                command.Parameters.AddWithValue("@purchase_from", expenses.purchase_from);
                command.Parameters.AddWithValue("@amount", expense.amount);
                command.Parameters.AddWithValue("@taxes_id", taxes_id);
                command.Parameters.AddWithValue("@promotion_id", promotion_id);
                command.Parameters.AddWithValue("@furniture_id", furniture_id);
                reader = command.ExecuteReader();

                expense_details_id=1;
            }
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminExpense");
    }
    [HttpPost]
    public IActionResult RedirectAdminAddExpense(){
        return RedirectToAction("AdminAddExpense");
    }
    public IActionResult EditExpense(int id){
        List<Furniture> furnitures = new List<Furniture>();
        List<Taxes> taxes = new List<Taxes>();
        List<Promotion> promotions = new List<Promotion>();
        List<Expenses> expenses = new List<Expenses>();
        Expenses expenses2 =new Expenses();
        ViewBag.employee_avatar=GetAvatar();
        int counting =0;
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditExpense");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT FURNITURE_NAME FROM FURNITURE ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Furniture furniture = new Furniture
                {
                    name = reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT TAXES_NAME FROM TAXES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Taxes taxes1 = new Taxes
                {
                    taxes_name = reader.GetString(0)
                };
                taxes.Add(taxes1);
            }

            reader.Close();
            query = "SELECT NAME FROM PROMOTIONS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Promotion promotion = new Promotion
                {
                    name = reader.GetString(0)
                };
                promotions.Add(promotion);
            }

            reader.Close();
            query = "SELECT ID FROM EXPENSES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Expenses expenses1 = new Expenses
                {
                    id = reader.GetInt32(0)
                };
                expenses.Add(expenses1);
            }

            expenses2.expensesDetails=new List<ExpensesDetails>();
            reader.Close();
            query = "SELECT EXPENSES.ID, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.STATUS, FURNITURE.FURNITURE_NAME, EXPENSES_DETAILS.QUANITY, IFNULL(PROMOTIONS.NAME, '') AS PROMOTION_NAME, IFNULL(TAXES.TAXES_NAME, '') AS TAXES_NAME, EXPENSES_DETAILS.AMOUNT FROM EXPENSES JOIN EXPENSES_DETAILS ON EXPENSES.ID = EXPENSES_DETAILS.EXPENSE_ID JOIN FURNITURE ON EXPENSES_DETAILS.FURNITURE_ID = FURNITURE.ID LEFT JOIN PROMOTIONS ON EXPENSES_DETAILS.PROMOTION_ID = PROMOTIONS.ID LEFT JOIN TAXES ON EXPENSES_DETAILS.TAXES_ID = TAXES.ID WHERE EXPENSES.ID =@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                expenses2.id=reader.GetInt32(0);
                expenses2.purchase_from=reader.GetString(1);
                DateTime day= reader.GetDateTime(2);
                expenses2.purchase_at=day.ToString("yyyy-MM-dd");
                if(reader.GetInt32(3)==1){
                    expenses2.status="Active";
                }else{
                    expenses2.status="Inactive";
                }
                ExpensesDetails expenses3 =new ExpensesDetails();
                expenses3.furniture!.name=reader.GetString(4);
                expenses3.quanity=reader.GetInt32(5);
                expenses3.promotion!.name=reader.GetString(6);
                expenses3.taxes!.taxes_name=reader.GetString(7);
                expenses3.amount=reader.GetFloat(8);
                expenses2.expensesDetails.Add(expenses3);
                counting++;
            }
            connection.Close();
        }
        ViewBag.count = counting;
        ViewBag.expense_list=expenses;
        ViewBag.furniture_list = furnitures;
        ViewBag.taxes_list = taxes;
        ViewBag.promotion_list = promotions;
        return View("~/Views/HotelViews/AdminEditExpense.cshtml",expenses2);
    }
    public IActionResult DeleteExpense(int id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM EXPENSES WHERE ID=@id";
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
        return RedirectToAction("AdminExpense");
    }
    public IActionResult AdminSearchExpense(string searchkeyword,string searchkeyword1, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("ExpenseSearch", searchkeyword);
        }
        if(searchkeyword1 != null){
            HttpContext.Session.SetString("ExpenseSearch1", searchkeyword1);
        }
        List<Expenses> expenses = new List<Expenses>();
        var a=HttpContext.Session.GetString("ExpenseSearch");
        var b=HttpContext.Session.GetString("ExpenseSearch1");
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
            if(searchkeyword != null && searchkeyword1 == null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES.EXPENSE_NUMBER like @id) order by EXPENSES.ID asc";
            }else if(searchkeyword == null && searchkeyword1 !=null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES_DETAILS.PURCHASE_AT like @id1) order by EXPENSES.ID asc";
            }else if(searchkeyword != null && searchkeyword1 !=null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES.EXPENSE_NUMBER like @id AND EXPENSES_DETAILS.PURCHASE_AT like @id1) order by EXPENSES.ID asc";
            }else{
                query = "SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID order by EXPENSES.ID asc";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
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
                DateTime day = reader.GetDateTime(6);

                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        ViewBag.expense_list = expenses;
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_list=pagedExpense;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminExpense.cshtml",pagedExpense);
    }
    public IActionResult AdminSearchExpenseReport(string searchkeyword,string searchkeyword1, int page)
    {
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword != null){
            HttpContext.Session.SetString("ExpenseReportSearch", searchkeyword);
        }
        if(searchkeyword1 != null){
            HttpContext.Session.SetString("ExpenseReportSearch1", searchkeyword1);
        }
        List<Expenses> expenses = new List<Expenses>();
        var a=HttpContext.Session.GetString("ExpenseReportSearch");
        var b=HttpContext.Session.GetString("ExpenseReportSearch1");
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
            if(searchkeyword != null && searchkeyword1 == null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES.EXPENSE_NUMBER like @id) order by EXPENSES.ID asc";
            }else if(searchkeyword == null && searchkeyword1 !=null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES_DETAILS.PURCHASE_AT like @id1) order by EXPENSES.ID asc";
            }else if(searchkeyword != null && searchkeyword1 !=null){
                query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID and (EXPENSES.EXPENSE_NUMBER like @id AND EXPENSES_DETAILS.PURCHASE_AT like @id1) order by EXPENSES.ID asc";
            }else{
                query = "SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID order by EXPENSES.ID asc";
            }
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
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
                DateTime day = reader.GetDateTime(6);

                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_report_list=pagedExpense;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminExpenseReport.cshtml", pagedExpense);
    }
    public List<Expenses> GetAllExpense(string query)
    {
        List<Expenses> expenses = new List<Expenses>();
        var searchkeyword= HttpContext.Session.GetString("ExpenseSearch");
        var searchkeyword1= HttpContext.Session.GetString("ExpenseSearch1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
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
                DateTime day = reader.GetDateTime(6);

                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        return expenses;
    }
    public List<Expenses> GetAllExpenseReport(string query)
    {
        List<Expenses> expenses = new List<Expenses>();
        var searchkeyword= HttpContext.Session.GetString("ExpenseReportSearch");
        var searchkeyword1= HttpContext.Session.GetString("ExpenseReportSearch1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            command.Parameters.AddWithValue("@id1",searchkeyword1);
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
                DateTime day = reader.GetDateTime(6);

                Expenses expense = new Expenses
                {
                    id = (int)reader["id"],
                    expenses_number = (string)reader["expense_number"],
                    furniture_name = (string)reader["furniture_name"],
                    purchase_from = (string)reader["purchase_from"],
                    purchase_at = day.ToString("dd-MM-yyyy"),
                    number = (int)reader["quanity"],
                    employee_name = (string)reader["name"],
                    total_amount = Convert.ToSingle(reader["total_amount"]),
                    status = statusString
                };
                expenses.Add(expense);
            }
            connection.Close();
        }
        return expenses;
    }
    public IActionResult AdminSortExpense(string id, int page){
        List<Expenses> expenses = new List<Expenses>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID";
        var searchkeyword= HttpContext.Session.GetString("ExpenseSearch");
        var searchkeyword1= HttpContext.Session.GetString("ExpenseSearch1");
        if(searchkeyword != null && searchkeyword1 == null){
            query=query + " and (EXPENSES.EXPENSE_NUMBER like @id)";
        }else if(searchkeyword == null && searchkeyword1 !=null){
            query=query + " and (EXPENSES_DETAILS.PURCHASE_AT like @id1)";
        }else if(searchkeyword != null && searchkeyword1 !=null){
            query=query + " and (EXPENSES.EXPENSE_NUMBER like @id AND EXPENSES_DETAILS.PURCHASE_AT like @id1)";
        }
        if(id == "id_asc"){
            query = query + " order by EXPENSES.ID asc";
            expenses=GetAllExpense(query);
        }else if(id == "id_desc"){
            query = query + " order by EXPENSES.ID desc";
            expenses=GetAllExpense(query);
        }else if(id == "number_asc"){
            query = query + " order by EXPENSES.EXPENSE_NUMBER asc";
            expenses=GetAllExpense(query);
        }else if(id == "number_desc"){
            query = query + " order by EXPENSES.EXPENSE_NUMBER desc";
            expenses=GetAllExpense(query);
        }else if(id == "item_asc"){
            query = query + " order by FURNITURE.FURNITURE_NAME asc";
            expenses=GetAllExpense(query);
        }else if(id == "item_desc"){
            query = query + " order by FURNITURE.FURNITURE_NAME desc";
            expenses=GetAllExpense(query);
        }else if(id == "quantity_asc"){
            query = query + " order by EXPENSES_DETAILS.QUANITY asc";
            expenses=GetAllExpense(query);
        }else if(id == "quantity_desc"){
            query = query + " order by EXPENSES_DETAILS.QUANITY desc";
            expenses=GetAllExpense(query);
        }else if(id == "employeename_asc"){
            query = query + " order by EMPLOYEES.NAME asc";
            expenses=GetAllExpense(query);
        }else if(id == "employeename_desc"){
            query = query + " order by EMPLOYEES.NAME desc";
            expenses=GetAllExpense(query);
        }else if(id == "purchasefrom_asc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_FROM asc";
            expenses=GetAllExpense(query);
        }else if(id == "purchasefrom_desc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_FROM desc";
            expenses=GetAllExpense(query);
        }else if(id == "purchaseat_asc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_AT asc";
            expenses=GetAllExpense(query);
        }else if(id == "purchaseat_desc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_AT desc";
            expenses=GetAllExpense(query);
        }else if(id == "total_asc"){
            query = query + " order by EXPENSES.TOTAL_AMOUNT asc";
            expenses=GetAllExpense(query);
        }else if(id == "total_desc"){
            query = query + " order by EXPENSES.TOTAL_AMOUNT desc";
            expenses=GetAllExpense(query);
        }else if(id == "status_asc"){
            query = query + " order by EXPENSES.STATUS asc";
            expenses=GetAllExpense(query);
        }else if(id == "status_asc"){
            query = query + " order by EXPENSES.STATUS desc";
            expenses=GetAllExpense(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_list=pagedExpense;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminExpense.cshtml", pagedExpense);
    }
    public IActionResult AdminSortExpenseReport(string id, int page){
        List<Expenses> expenses = new List<Expenses>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT distinct EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, FURNITURE.FURNITURE_NAME, EXPENSES.STATUS, EMPLOYEES.NAME, EXPENSES_DETAILS.PURCHASE_FROM, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.QUANITY FROM BOOKINGS, EXPENSES, EXPENSES_DETAILS, FURNITURE, EMPLOYEES WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID and EXPENSES_DETAILS.FURNITURE_ID=FURNITURE.ID and EXPENSES.EMPLOYEE_ID=EMPLOYEES.ID";
        var searchkeyword= HttpContext.Session.GetString("ExpenseReportSearch");
        var searchkeyword1= HttpContext.Session.GetString("ExpenseReportSearch1");
        if(searchkeyword != null && searchkeyword1 == null){
            query=query + " and (EXPENSES.EXPENSE_NUMBER like @id)";
        }else if(searchkeyword == null && searchkeyword1 !=null){
            query=query + " and (EXPENSES_DETAILS.PURCHASE_AT like @id1)";
        }else if(searchkeyword != null && searchkeyword1 !=null){
            query=query + " and (EXPENSES.EXPENSE_NUMBER like @id AND EXPENSES_DETAILS.PURCHASE_AT like @id1)";
        }
        if(id == "id_asc"){
            query = query + " order by EXPENSES.ID asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "id_desc"){
            query = query + " order by EXPENSES.ID desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "number_asc"){
            query = query + " order by EXPENSES.EXPENSE_NUMBER asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "number_desc"){
            query = query + " order by EXPENSES.EXPENSE_NUMBER desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "item_asc"){
            query = query + " order by FURNITURE.FURNITURE_NAME asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "item_desc"){
            query = query + " order by FURNITURE.FURNITURE_NAME desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "quantity_asc"){
            query = query + " order by EXPENSES_DETAILS.QUANITY asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "quantity_desc"){
            query = query + " order by EXPENSES_DETAILS.QUANITY desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "employeename_asc"){
            query = query + " order by EMPLOYEES.NAME asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "employeename_desc"){
            query = query + " order by EMPLOYEES.NAME desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "purchasefrom_asc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_FROM asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "purchasefrom_desc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_FROM desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "purchaseat_asc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_AT asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "purchaseat_desc"){
            query = query + " order by EXPENSES_DETAILS.PURCHASE_AT desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "total_asc"){
            query = query + " order by EXPENSES.TOTAL_AMOUNT asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "total_desc"){
            query = query + " order by EXPENSES.TOTAL_AMOUNT desc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "status_asc"){
            query = query + " order by EXPENSES.STATUS asc";
            expenses=GetAllExpenseReport(query);
        }else if(id == "status_asc"){
            query = query + " order by EXPENSES.STATUS desc";
            expenses=GetAllExpenseReport(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedExpense= expenses.ToPagedList(pageNumber,pageSize);
        ViewBag.expense_report_list=pagedExpense;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminExpenseReport.cshtml", pagedExpense);
    }
    public IActionResult PrintExpense(int id){
        Expenses expenses=new Expenses();
        Taxes taxes=new Taxes();
        Promotion promotion=new Promotion();
        float? subTotal=0;
        float? Total=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT EXPENSES.ID, EXPENSES.EXPENSE_NUMBER, EXPENSES.TOTAL_AMOUNT, EXPENSES_DETAILS.PURCHASE_AT, EXPENSES_DETAILS.PURCHASE_FROM,EXPENSES_DETAILS.AMOUNT,FURNITURE.FURNITURE_NAME, EXPENSES_DETAILS.QUANITY,TAXES.TAX_PERCENTAGE, PROMOTIONS.DISCOUNT_PERCENT, FURNITURE.ID, FURNITURE.PRICE FROM EXPENSES, FURNITURE, EXPENSES_DETAILS LEFT JOIN TAXES ON EXPENSES_DETAILS.TAXES_ID=TAXES.ID LEFT JOIN PROMOTIONS ON EXPENSES_DETAILS.PROMOTION_ID=PROMOTIONS.ID WHERE EXPENSES.ID=EXPENSES_DETAILS.EXPENSE_ID AND FURNITURE.ID=EXPENSES_DETAILS.FURNITURE_ID AND EXPENSES.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                expenses.id=reader.GetInt32(0);
                expenses.expenses_number=reader.GetString(1);
                expenses.total_amount=reader.GetFloat(2);
                DateTime day= reader.GetDateTime(3);
                ExpensesDetails expensesDetails=new ExpensesDetails{
                    purchase_at=day.ToString("dd-MM-yyyy"),
                    purchase_from=reader.GetString(4),
                    amount=reader.GetInt32(5),
                    quanity=reader.GetInt32(7)
                };
                expensesDetails.furniture!.name=reader.GetString(6);
                expensesDetails.furniture!.id=reader.GetInt32(10);
                expensesDetails.furniture!.price=reader.GetFloat(11);
                expenses.expensesDetails.Add(expensesDetails);
                if(!reader.IsDBNull(8)){
                    taxes.tax_percentage=reader.GetFloat(8);
                }
                if(!reader.IsDBNull(9)){
                    promotion.discount_percent=reader.GetFloat(9);
                }
                subTotal=subTotal+reader.GetFloat(11);
                Total=reader.GetFloat(2);
            }
        }
        ViewBag.expense=expenses;
        ViewBag.taxes=taxes;
        ViewBag.promotion=promotion;
        ViewBag.subTotal=subTotal;
        ViewBag.Total=Total;
        return View("~/Views/HotelViews/Invoice1.cshtml");
    }
}