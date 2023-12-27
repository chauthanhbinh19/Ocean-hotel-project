using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using X.PagedList;
using X.PagedList.Mvc;
using X.PagedList.Web.Common;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace Project.Controllers;

public class HomeController : Controller
{
    private readonly string connectionString = DataConnection.Connection;
    public Customer GetAvatar(){
        Customer customer =new Customer();
        var usernameSession = HttpContext.Session.GetString("username1");
        var passwordSession = HttpContext.Session.GetString("password1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select customers_img.img, customers.name, customers.id from customers, customers_img, accounts where customers.id=customers_img.customer_id and customers.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.img=reader.GetString(0);
                string fullName=reader.GetString(1);
                string[] parts=fullName.Split(' ');
                customer.name=parts[parts.Length-1];
                customer.id=reader.GetInt32(2);
            }
            connection.Close();
        }
        return customer;
    }
    public IActionResult Index(){
        Customer customer =new Customer();
        List<Rooms> rooms=new List<Rooms>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.STATUS FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND ROOMS.ID NOT IN( SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE BOOKINGS.ROOM_ID=ROOMS.ID AND BOOKINGS.CHECK_OUT_DATE>= CURRENT_DATE() and BOOKINGS.CHECK_IN_DATE <= CURRENT_DATE()) AND ROOMS.STATUS=0";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms rooms1=new Rooms{
                    id=reader.GetInt32(0)
                };
                rooms.Add(rooms1);
            }

            reader.Close();
            foreach(var room in rooms){
                query = "UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",room.id);
                reader = command.ExecuteReader();
                reader.Close();
            }
        }
        customer=GetAvatar();
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Home.cshtml");
    }
    public IActionResult Bar(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Bar.cshtml");
    }
    public IActionResult Beach(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Beach.cshtml");
    }
    public IActionResult Corporate(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Corporate.cshtml");
    }
    public IActionResult Gallery(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Gallery.cshtml");
    }
    public IActionResult Gym(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Gym.cshtml");
    }
    public IActionResult Meeting(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Meeting.cshtml");
    }
    public IActionResult ReflectionR(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/ReflextionR.cshtml");
    }
    public IActionResult Cafe(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Cafe.cshtml");
    }
    public IActionResult Buffet(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Buffet.cshtml");
    }
    public IActionResult Tapas(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Tapas.cshtml");
    }
    public IActionResult Date(int id){
        var userSession = HttpContext.Session.GetString("username1");
        if (string.IsNullOrEmpty(userSession))
        {
            // Session người dùng không tồn tại, chuyển hướng đến trang đăng nhập hoặc trang không được ủy quyền
            return RedirectToAction("Login", "Login");
        }
        Rooms rooms=new Rooms();
        List<Promotion> promotions=new List<Promotion>();
        string query="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.TYPE FROM VIEW,ROOMS, ROOMS_IMG , REVIEWS WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.ID=REVIEWS.ROOM_ID AND ROOMS.ID=@id group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                rooms.id=reader.GetInt32(0);
                rooms.name=reader.GetString(1);
                rooms.floor=reader.GetInt32(2);
                rooms.size=reader.GetString(3);
                rooms.price=reader.GetFloat(4);
                rooms.view!.name=reader.GetString(5);
                rooms.img=reader.GetString(6);
                rooms.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.type=reader.GetString(8);
            }

            reader.Close(); 
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID, VALID_FROM, VALID_TO FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                DateTime day=reader.GetDateTime(5);
                DateTime day1=reader.GetDateTime(6);
                promotion.valid_from=day.ToString("yyyy-MM-dd");
                promotion.valid_to=day1.ToString("yyyy-MM-dd");
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        return View("~/Views/HotelViews/Customer/Date.cshtml", rooms);
    }
    public IActionResult ServiceDetail( int id, float? total){
        List<Service> services=new List<Service>();
        List<Service> services1=new List<Service>();
        List<Promotion> promotions=new List<Promotion>();
        Rooms rooms=new Rooms();
        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        int? number=HttpContext.Session.GetInt32("Numberofguest");
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT SERVICES.ID, SERVICES.NAME, SERVICES.DESCRIPTION, SERVICES.PRICE,SERVICE_IMG.IMG FROM SERVICES,SERVICE_IMG WHERE SERVICES.TYPE=@id AND SERVICES.ID=SERVICE_IMG.SERVICE_ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id","service");
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service name =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                    img=reader.GetString(4)
                };
                services.Add(name);
            }

            reader.Close();
            query="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.TYPE FROM VIEW,ROOMS, ROOMS_IMG , REVIEWS WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.ID=REVIEWS.ROOM_ID AND ROOMS.ID=@id group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                rooms.id=reader.GetInt32(0);
                rooms.name=reader.GetString(1);
                rooms.floor=reader.GetInt32(2);
                rooms.size=reader.GetString(3);
                rooms.price=reader.GetFloat(4);
                rooms.view!.name=reader.GetString(5);
                rooms.img=reader.GetString(6);
                rooms.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.type=reader.GetString(8);
            }
            rooms.price=rooms.price*diffDays;
            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }

            reader.Close();
            query = "SELECT SERVICES.ID, SERVICES.NAME, SERVICES.DESCRIPTION, SERVICES.PRICE,SERVICE_IMG.IMG FROM SERVICES,SERVICE_IMG WHERE SERVICES.ID=SERVICE_IMG.SERVICE_ID";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Service name =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                    img=reader.GetString(4)
                };
                services1.Add(name);
            }
            connection.Close();
        }
        if(promotions.Count !=0){
            total=rooms.price-rooms.price*promotions[0].discount_percent/100;
        }
        else{
            total=rooms.price;
        }
        ViewBag.service_list=services;
        ViewBag.checkin=checkin;
        ViewBag.checkout=checkout;
        ViewBag.number=number;
        ViewBag.room=rooms;
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.total=total;
        ViewBag.service_list1=services1;
        return View("~/Views/HotelViews/Customer/ServiceDetail.cshtml");
    }
    public IActionResult GetServiceList(string selectedOption){
        List<Service> services=new List<Service>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT SERVICES.ID, SERVICES.NAME, SERVICES.DESCRIPTION, SERVICES.PRICE,SERVICE_IMG.IMG FROM SERVICES,SERVICE_IMG WHERE SERVICES.TYPE=@id AND SERVICES.ID=SERVICE_IMG.SERVICE_ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Service name =new Service{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    price=reader.GetFloat(3),
                    img=reader.GetString(4)
                };
                services.Add(name);
            }

            connection.Close();
        }
        return PartialView("~/Views/PartialViews/ServiceList.cshtml",services);
    }
    public IActionResult RoomStandard(){
        List<Rooms> rooms=new List<Rooms>();
        List<Rooms> rooms2=new List<Rooms>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Promotion> promotions=new List<Promotion>();
        float min=0;
        float max=0;
        DateTime currenDate=DateTime.Now.Date;

        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        int? number=HttpContext.Session.GetInt32("Numberofguest");
        if(checkin == null){
            HttpContext.Session.SetString("Checkin", currenDate.ToString("yyyy-MM-dd"));
            checkin=currenDate.ToString("yyyy-MM-dd");
        }
        if(checkout == null){
            HttpContext.Session.SetString("Checkout", currenDate.ToString("yyyy-MM-dd"));
            checkout=currenDate.ToString("yyyy-MM-dd");
        }
        if(number == null){
            HttpContext.Session.SetInt32("Numberofguest", 1);
            number=1;
        }
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='STANDARD' AND ROOMS.NUMBER_OF_GUEST >=@numberofguest AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= @date)) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS ORDER BY ROOMS.ID ASC";
            query =query+" LIMIT 10";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", checkin);
            command.Parameters.AddWithValue("@numberofguest", number);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.Add(rooms1);
            }
            
            reader.Close();
            query="SELECT distinct ROOMS.NAME FROM ROOMS WHERE ROOMS.TYPE='standard' group by ROOMS.NAME ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms rooms1 = new Rooms{
                    name=reader.GetString(0)
                };
                rooms2.Add(rooms1);
            }

            reader.Close();
            query="SELECT MIN(PRICE), MAX(PRICE) FROM ROOMS WHERE TYPE='STANDARD' ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                min=reader.GetFloat(0);
                max=reader.GetFloat(1);
            }

            reader.Close();
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC LIMIT 5";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        if(rooms.Count ==0){
            return RedirectToAction("RoomSuperior");
        }
        ViewBag.room_list=rooms;
        ViewBag.room_name=rooms2;
        ViewBag.type="Standard";
        ViewBag.min=min;
        ViewBag.max=max;
        ViewBag.furniture=furnitures;
        ViewBag.NumberStandard=GetRoomNumber("Standard");
        ViewBag.NumberSuperior=GetRoomNumber("Superior");
        ViewBag.NumberDeluxe=GetRoomNumber("Deluxe");
        ViewBag.NumberSuite=GetRoomNumber("Suite");
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.check_in=checkin;
        ViewBag.check_out=checkout;
        ViewBag.number=number;
        return View("~/Views/HotelViews/Customer/Room.cshtml");
    }
    public IActionResult RoomSuperior(){
        List<Rooms> rooms=new List<Rooms>();
        List<Rooms> rooms2=new List<Rooms>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Promotion> promotions=new List<Promotion>();
        float min=0;
        float max=0;
        DateTime currenDate=DateTime.Now.Date;

        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        int? number=HttpContext.Session.GetInt32("Numberofguest");
        if(checkin == null){
            HttpContext.Session.SetString("Checkin", currenDate.ToString("yyyy-MM-dd"));
            checkin=currenDate.ToString("yyyy-MM-dd");
        }
        if(checkout == null){
            HttpContext.Session.SetString("Checkout", currenDate.ToString("yyyy-MM-dd"));
            checkout=currenDate.ToString("yyyy-MM-dd");
        }
        if(number == null){
            HttpContext.Session.SetInt32("Numberofguest", 1);
            number=1;
        }
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='SUPERIOR' AND ROOMS.NUMBER_OF_GUEST >=@numberofguest AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= @date)) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS ORDER BY ROOMS.ID ASC";
            query =query+" LIMIT 10";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", checkin);
            command.Parameters.AddWithValue("@numberofguest", number);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.Add(rooms1);
            }

            reader.Close();
            query="SELECT distinct ROOMS.NAME FROM ROOMS WHERE ROOMS.TYPE='superior' group by ROOMS.NAME ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms rooms1 = new Rooms{
                    name=reader.GetString(0)
                };
                rooms2.Add(rooms1);
            }

            reader.Close();
            query="SELECT MIN(PRICE), MAX(PRICE) FROM ROOMS WHERE TYPE='SUPERIOR' ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                min=reader.GetFloat(0);
                max=reader.GetFloat(1);
            }

            reader.Close();
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC LIMIT 5";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        if(rooms.Count ==0){
            return RedirectToAction("RoomDeluxe");
        }
        ViewBag.room_list=rooms;
        ViewBag.room_name=rooms2;
        ViewBag.type="Superior";
        ViewBag.min=min;
        ViewBag.max=max;
        ViewBag.furniture=furnitures;
        ViewBag.NumberStandard=GetRoomNumber("Standard");
        ViewBag.NumberSuperior=GetRoomNumber("Superior");
        ViewBag.NumberDeluxe=GetRoomNumber("Deluxe");
        ViewBag.NumberSuite=GetRoomNumber("Suite");
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.check_in=checkin;
        ViewBag.check_out=checkout;
        ViewBag.number=number;
        return View("~/Views/HotelViews/Customer/Room.cshtml");
    }
    public IActionResult RoomDeluxe(){
        List<Rooms> rooms=new List<Rooms>();
        List<Rooms> rooms2=new List<Rooms>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Promotion> promotions=new List<Promotion>();
        float min=0;
        float max=0;
        DateTime currenDate=DateTime.Now.Date;

        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        int? number=HttpContext.Session.GetInt32("Numberofguest");
        if(checkin == null){
            HttpContext.Session.SetString("Checkin", currenDate.ToString("yyyy-MM-dd"));
            checkin=currenDate.ToString("yyyy-MM-dd");
        }
        if(checkout == null){
            HttpContext.Session.SetString("Checkout", currenDate.ToString("yyyy-MM-dd"));
            checkout=currenDate.ToString("yyyy-MM-dd");
        }
        if(number == null){
            HttpContext.Session.SetInt32("Numberofguest", 1);
            number=1;
        }
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS,ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='DELUXE' AND ROOMS.NUMBER_OF_GUEST >=@numberofguest AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= @date)) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS ORDER BY ROOMS.ID ASC";
            query =query+" LIMIT 10";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", checkin);
            command.Parameters.AddWithValue("@numberofguest", number);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.Add(rooms1);
            }
            
            reader.Close();
            query="SELECT distinct ROOMS.NAME FROM ROOMS WHERE ROOMS.TYPE='deluxe' group by ROOMS.NAME ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms rooms1 = new Rooms{
                    name=reader.GetString(0)
                };
                rooms2.Add(rooms1);
            }

            reader.Close();
            query="SELECT MIN(PRICE), MAX(PRICE) FROM ROOMS WHERE TYPE='DELUXE' ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                min=reader.GetFloat(0);
                max=reader.GetFloat(1);
            }

            reader.Close();
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC LIMIT 5";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        if(rooms.Count ==0){
            return RedirectToAction("RoomSuite");
        }
        ViewBag.room_list=rooms;
        ViewBag.room_name=rooms2;
        ViewBag.type="Deluxe";
        ViewBag.min=min;
        ViewBag.max=max;
        ViewBag.furniture=furnitures;
        ViewBag.NumberStandard=GetRoomNumber("Standard");
        ViewBag.NumberSuperior=GetRoomNumber("Superior");
        ViewBag.NumberDeluxe=GetRoomNumber("Deluxe");
        ViewBag.NumberSuite=GetRoomNumber("Suite");
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.check_in=checkin;
        ViewBag.check_out=checkout;
        ViewBag.number=number;
        return View("~/Views/HotelViews/Customer/Room.cshtml");
    }
    public IActionResult RoomSuite(){
        List<Rooms> rooms=new List<Rooms>();
        List<Rooms> rooms2=new List<Rooms>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Promotion> promotions=new List<Promotion>();
        float min=0;
        float max=0;
        DateTime currenDate=DateTime.Now.Date;

        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        int? number=HttpContext.Session.GetInt32("Numberofguest");
        if(checkin == null){
            HttpContext.Session.SetString("Checkin", currenDate.ToString("yyyy-MM-dd"));
            checkin=currenDate.ToString("yyyy-MM-dd");
        }
        if(checkout == null){
            HttpContext.Session.SetString("Checkout", currenDate.ToString("yyyy-MM-dd"));
            checkout=currenDate.ToString("yyyy-MM-dd");
        }
        if(number == null){
            HttpContext.Session.SetInt32("Numberofguest", 1);
            number=1;
        }
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='SUITE' AND ROOMS.NUMBER_OF_GUEST >=@numberofguest AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= @date)) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS ORDER BY ROOMS.ID ASC";
            query =query+" LIMIT 10";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@date", checkin);
            command.Parameters.AddWithValue("@numberofguest", number);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.Add(rooms1);
            }

            reader.Close();
            query="SELECT distinct ROOMS.NAME FROM ROOMS WHERE ROOMS.TYPE='suite' group by ROOMS.NAME ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms rooms1 = new Rooms{
                    name=reader.GetString(0)
                };
                rooms2.Add(rooms1);
            }

            reader.Close();
            query="SELECT MIN(PRICE), MAX(PRICE) FROM ROOMS WHERE TYPE='SUITE' ";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                min=reader.GetFloat(0);
                max=reader.GetFloat(1);
            }

            reader.Close();
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC LIMIT 5";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.room_list=rooms;
        ViewBag.room_name=rooms2;
        ViewBag.type="Suite";
        ViewBag.min=min;
        ViewBag.max=max;
        ViewBag.furniture=furnitures;
        ViewBag.NumberStandard=GetRoomNumber("Standard");
        ViewBag.NumberSuperior=GetRoomNumber("Superior");
        ViewBag.NumberDeluxe=GetRoomNumber("Deluxe");
        ViewBag.NumberSuite=GetRoomNumber("Suite");
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.check_in=checkin;
        ViewBag.check_out=checkout;
        ViewBag.number=number;
        return View("~/Views/HotelViews/Customer/Room.cshtml");
    }
    public IActionResult RoomDetail(int id){
        Rooms rooms=new Rooms();
        List<Rooms> rooms1=new List<Rooms>();
        List<Customer> customers= new List<Customer>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Promotion> promotions=new List<Promotion>();
        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING), ROOMS.TYPE, ROOMS.STATUS FROM VIEW,ROOMS left join REVIEWS on ROOMS.ID=REVIEWS.ROOM_ID , ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.ID=@id group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(9)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                rooms.id=reader.GetInt32(0);
                rooms.name=reader.GetString(1);
                rooms.floor=reader.GetInt32(2);
                rooms.size=reader.GetString(3);
                rooms.price=reader.GetFloat(4);
                rooms.view!.name=reader.GetString(5);
                rooms.img=reader.GetString(6);
                if(!reader.IsDBNull(7)){
                    rooms.review!.rate=Math.Round(reader.GetFloat(7),2);
                }
                rooms.type=reader.GetString(8);
                rooms.status=status;
            }
            rooms.price=diffDays*rooms.price;
            reader.Close();
            query="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.TYPE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS FROM ROOMS, VIEW, ROOMS_IMG WHERE ROOMS.VIEW_ID =VIEW.ID AND ROOMS_IMG.ROOM_ID=ROOMS.ID AND ROOMS.TYPE=@type AND ROOMS.STATUS= '1' ORDER BY RAND() LIMIT 5";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@type",rooms.type);
            reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(6)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms2=new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    type=reader.GetString(2),
                    price=reader.GetFloat(3),
                    img=reader.GetString(5),
                    status=status
                };
                rooms2.view!.name=reader.GetString(4);
                rooms1.Add(rooms2);
            }
            
            reader.Close();
            query="SELECT CUSTOMERS.NAME, CUSTOMERS_IMG.IMG, REVIEWS.COMMENT, REVIEWS.RATING FROM ROOMS, CUSTOMERS, CUSTOMERS_IMG, REVIEWS WHERE REVIEWS.CUSTOMER_ID=CUSTOMERS.ID AND REVIEWS.ROOM_ID=ROOMS.ID AND CUSTOMERS.ID = CUSTOMERS_IMG.CUSTOMER_ID AND ROOMS.ID=@id ORDER BY RAND() LIMIT 5";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Customer customer=new Customer{
                    name=reader.GetString(0),
                    img=reader.GetString(1),
                };
                customer.review!.comment=reader.GetString(2);
                customer.review!.rate=reader.GetInt32(3);
                customers.Add(customer);
            }

            reader.Close();
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE, FURNITURE_ROOM, ROOMS WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND ROOMS.ID=FURNITURE_ROOM.ROOMS_ID AND ROOMS.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture=new Furniture{
                    name=reader.GetString(0),
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.room_list=rooms1;
        ViewBag.customer_list_review=customers;
        ViewBag.furniture_list=furnitures;
        ViewBag.customer_avatar=GetAvatar();
        ViewBag.promotion=promotions;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/Customer/RoomDetail.cshtml", rooms);
    }
    public List<Rooms> GetRoom(string query, string type){
        List<Rooms> rooms=new List<Rooms>();
        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    type=type,
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.Add(rooms1);
            }
            connection.Close();
        }
        return rooms;
    }
    public List<Rooms> GetRoomRate(string query){
        List<Rooms> rooms=new List<Rooms>();
        string? checkin=HttpContext.Session.GetString("Checkin");
        string? checkout=HttpContext.Session.GetString("Checkout");
        DateTime checkinDate=new DateTime();
        DateTime checkoutDate=new DateTime();
        if(checkin != null){
            checkinDate = DateTime.Parse(checkin!);
        }
        if(checkout !=null){
            checkoutDate =DateTime.Parse(checkout!);
        }
        TimeSpan duration = checkoutDate.Subtract(checkinDate);
        int diffDays = duration.Days;
        if(diffDays ==0){
            diffDays =1;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string status = "c";
                if(reader.GetInt32(8)==1){
                    status="Available";
                }else{
                    status="Booked";
                }
                Rooms rooms1 =new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    floor=reader.GetInt32(2),
                    size=reader.GetString(3),
                    price=reader.GetFloat(4),
                    img=reader.GetString(6),
                    status=status,
                    number_of_guest=reader.GetInt32(9)
                };
                rooms1.price=diffDays*rooms1.price;
                rooms1.view!.name=reader.GetString(5);
                if(!reader.IsDBNull(7)){
                    rooms1.review!.rate=Math.Round(reader.GetFloat(7),2);
                }
                rooms.Add(rooms1);
            }
            connection.Close();
        }
        return rooms;
    }
    [HttpPost]
    public IActionResult GetFilterRoom(List<string> selectedOption,string type){
        List<Rooms> rooms=new List<Rooms>();
        int? number_of_guest=1;
        int? a=HttpContext.Session.GetInt32("Numberofguest");
        if(a != null){
            number_of_guest=a;
        }
        string query="";
        if(selectedOption.Count != 0){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS,ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 AND ( ";
            for(int i=0; i<selectedOption.Count;i++){
                query=query+" ROOMS.NAME = '"+ selectedOption[i]+"'";
                if(i != selectedOption.Count-1 && (selectedOption.Count-1) != 0){
                    query=query+" OR ";
                }else if(i == selectedOption.Count-1){
                    query=query+")";
                }
            }
            query=query+"group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        }else{
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS,ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"'AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS";
        }
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query1="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            MySqlCommand command = new MySqlCommand(query1, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        query=query+" ORDER BY ROOMS.ID ASC";
        query =query+" LIMIT 10";
        rooms=GetRoomRate(query);
        ViewBag.promotion=promotions;
        return PartialView("~/Views/PartialViews/RoomList.cshtml", rooms);
    }
    [HttpPost]
    public IActionResult GetFilterRoomPrice(float selectedOption, string type){ 
        List<Rooms> rooms=new List<Rooms>();
        string query="";
        int? number_of_guest=1;
        int? a=HttpContext.Session.GetInt32("Numberofguest");
        if(a != null){
            number_of_guest=a;
        }
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query1="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            MySqlCommand command = new MySqlCommand(query1, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS,ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.PRICE >= '"+ selectedOption +"'AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS ORDER BY ROOMS.ID ASC";
        query =query+" LIMIT 10";
        rooms=GetRoomRate(query);
        ViewBag.promotion=promotions;
        return PartialView("~/Views/PartialViews/RoomList.cshtml", rooms);
    }
    public int GetRoomNumber(string text){
        int number=0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(ROOMS.ID) FROM ROOMS WHERE TYPE =@text";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@text",text);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                number=reader.GetInt32(0);
            }
        }
        return number;
    }
    [HttpPost]
    public IActionResult GetFilterRoomFurniture(List<string> selectedOption,string type){
        List<Rooms> rooms=new List<Rooms>();
        string query="";
        int? number_of_guest=1;
        int? a=HttpContext.Session.GetInt32("Numberofguest");
        if(a != null){
            number_of_guest=a;
        }
        if(selectedOption.Count != 0){
            query ="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG,FURNITURE, FURNITURE_ROOM WHERE ROOMS.VIEW_ID=VIEW.ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND FURNITURE_ROOM.FURNITURE_ID=FURNITURE.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"'AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 AND (";
            for(int i=0; i<selectedOption.Count;i++){
                query=query+" FURNITURE.FURNITURE_NAME = '"+ selectedOption[i]+"'";
                if(i != selectedOption.Count-1 && (selectedOption.Count-1) != 0){
                    query=query+" OR ";
                }else if(i == selectedOption.Count-1){
                    query=query+")";
                }
            }
        }else{
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS,ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG,FURNITURE, FURNITURE_ROOM  WHERE ROOMS.VIEW_ID=VIEW.ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND FURNITURE_ROOM.FURNITURE_ID=FURNITURE.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"'AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 ";
        }
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query1="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE() ";
            MySqlCommand command = new MySqlCommand(query1, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        query=query+"GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        query=query+" ORDER BY ROOMS.ID ASC";
        query =query+" LIMIT 10";
        rooms=GetRoom(query, type);
        ViewBag.promotion=promotions;
        return PartialView("~/Views/PartialViews/RoomList.cshtml", rooms);
    }
    public IActionResult GetFilterRoomRate(string selectedOption, string type){
        List<Rooms> rooms=new List<Rooms>();
        string query="";
        int? number_of_guest=1;
        int? a=HttpContext.Session.GetInt32("Numberofguest");
        if(a != null){
            number_of_guest=a;
        }
        if(selectedOption != null){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"'AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS HAVING AVG(REVIEWS.RATING)> '"+ selectedOption +"' ORDER BY ROOMS.ID ASC";
        }else{
            query = "SELECT DISTINCT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS FROM VIEW,ROOMS, ROOMS_IMG, FURNITURE, FURNITURE_ROOM WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND FURNITURE_ROOM.FURNITURE_ID=FURNITURE.ID AND ROOMS.ID NOT IN( SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE BOOKINGS.ROOM_ID=ROOMS.ID AND BOOKINGS.CHECK_OUT_DATE>= CURRENT_DATE() and BOOKINGS.CHECK_IN_DATE <= CURRENT_DATE()) AND ROOMS.TYPE='"+ type +"' ";
        }
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query1="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            MySqlCommand command = new MySqlCommand(query1, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        query =query+" LIMIT 10";
        rooms=GetRoomRate(query);
        ViewBag.promotion=promotions;
        return PartialView("~/Views/PartialViews/RoomList.cshtml", rooms);
    }
    public IActionResult GetFurnitureName(string selectedOption, string type){
        List<Furniture> furnitures= new List<Furniture>();
        string query="";
        if(selectedOption == "more"){
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC";
        }else{
            query="SELECT FURNITURE.FURNITURE_NAME FROM FURNITURE ORDER BY FURNITURE.ID ASC LIMIT 5";
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0),
                    type=type
                };
                furnitures.Add(furniture);
            }
        }
        return PartialView("~/Views/PartialViews/FurnitureNameList.cshtml", furnitures);
    }
    public IActionResult GetMoreRoom(List<string> selectedOption,string type, string selectedOption1, List<string> selectedOption2,string selectedOption3, string offset){
        List<Rooms> rooms=new List<Rooms>();
        string query="";
        int? number_of_guest=1;
        int? a=HttpContext.Session.GetInt32("Numberofguest");
        if(a != null){
            number_of_guest=a;
        }
        if(selectedOption.Count != 0){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 AND ( ";
            for(int i=0; i<selectedOption.Count;i++){
                query=query+" ROOMS.NAME = '"+ selectedOption[i]+"'";
                if(i != selectedOption.Count-1 && (selectedOption.Count-1) != 0){
                    query=query+" OR ";
                }else if(i == selectedOption.Count-1){
                    query=query+")";
                }
            }
            query=query+"group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        }else if(selectedOption1 != null){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS HAVING AVG(REVIEWS.RATING)> '"+ selectedOption1 +"'";
        }else if(selectedOption2.Count != 0){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG,FURNITURE, FURNITURE_ROOM WHERE ROOMS.VIEW_ID=VIEW.ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND FURNITURE_ROOM.FURNITURE_ID=FURNITURE.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 AND (";
            for(int i=0; i<selectedOption2.Count;i++){
                query=query+" FURNITURE.FURNITURE_NAME = '"+ selectedOption2[i]+"'";
                if(i != selectedOption2.Count-1 && (selectedOption2.Count-1) != 0){
                    query=query+" OR ";
                }else if(i == selectedOption2.Count-1){
                    query=query+")";
                }
            }
            query=query+"group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        }else if(selectedOption3 != null){
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.PRICE >= '"+ selectedOption3 +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 GROUP BY ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG, ROOMS.STATUS";
        }else{
            query = "SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.STATUS, ROOMS.NUMBER_OF_GUEST FROM VIEW,ROOMS left join bookings on BOOKINGS.ROOM_ID=ROOMS.ID left join REVIEWS ON ROOMS.ID=REVIEWS.ROOM_ID, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.TYPE='"+ type +"' AND ROOMS.NUMBER_OF_GUEST >= '"+ number_of_guest +"' AND ((ROOMS.ID NOT IN (SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND BOOKINGS.CHECK_OUT_DATE >= current_date())) OR BOOKINGS.CHECK_OUT_DATE is NULL) AND ROOMS.STATUS=1 ";
            query=query+"group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
        }
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query1="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE()";
            MySqlCommand command = new MySqlCommand(query1, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2)
                };
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        query=query+" ORDER BY ROOMS.ID ASC";
        query =query+" LIMIT 10";
        query =query+" OFFSET " + offset;
        rooms=GetRoomRate(query);
        ViewBag.promotion=promotions;
        return PartialView("~/Views/PartialViews/RoomList.cshtml", rooms);
    }
    public IActionResult Shuttle(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Shuttle.cshtml");
    }
    public IActionResult ShuttleService(){
        return View("~/Views/HotelViews/ShuttleService.cshtml");
    }
    public IActionResult Spa(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Spa.cshtml");
    }
    public IActionResult Payment(List<Service> services, float total,string checkin,string checkout,int number, int room_id){
        var userSession = HttpContext.Session.GetString("username1");
        if (string.IsNullOrEmpty(userSession))
        {
            // Session người dùng không tồn tại, chuyển hướng đến trang đăng nhập hoặc trang không được ủy quyền
            return RedirectToAction("Login", "Login");
        }
        List<Service> services1=new List<Service>();
        foreach(var service in services){
            if(service.id != 0){
                services1.Add(service);
            }
        }
        Rooms rooms=new Rooms();
        List<Promotion> promotions=new List<Promotion>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG , avg(REVIEWS.RATING),ROOMS.TYPE FROM VIEW,ROOMS, ROOMS_IMG , REVIEWS WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=ROOMS_IMG.ROOM_ID AND ROOMS.ID=REVIEWS.ROOM_ID AND ROOMS.ID=@id group by ROOMS.ID, ROOMS.NAME, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME, ROOMS_IMG.IMG";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",room_id);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                rooms.id=reader.GetInt32(0);
                rooms.name=reader.GetString(1);
                rooms.floor=reader.GetInt32(2);
                rooms.size=reader.GetString(3);
                rooms.price=reader.GetFloat(4);
                rooms.view!.name=reader.GetString(5);
                rooms.img=reader.GetString(6);
                rooms.review!.rate=Math.Round(reader.GetFloat(7),2);
                rooms.type=reader.GetString(8);
            }

            reader.Close();
            foreach(var service in services1){
                query="SELECT NAME, PRICE FROM SERVICES WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",service.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    service.name=reader.GetString(0);
                    service.price=reader.GetFloat(1);
                }
                reader.Close();
            }

            reader.Close();
            query="SELECT ID, NAME, DISCOUNT_PERCENT, ROOM_ID, CUSTOMER_ID,VALID_FROM, VALID_TO FROM PROMOTIONS WHERE VALID_FROM <= CURRENT_DATE() AND VALID_TO >= CURRENT_DATE() ORDER BY DISCOUNT_PERCENT DESC LIMIT 1";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Promotion promotion= new Promotion{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    discount_percent=reader.GetFloat(2),
                };
                DateTime day=reader.GetDateTime(5);
                DateTime day1=reader.GetDateTime(6);
                promotion.valid_from=day.ToString("yyyy-MM-dd");
                promotion.valid_to=day1.ToString("yyyy-MM-dd");
                if(!reader.IsDBNull(3)){
                    promotion.room_id=reader.GetString(3);
                }
                if(!reader.IsDBNull(4)){
                    promotion.customer_id=reader.GetString(4);
                }
                promotions.Add(promotion);
            }
            connection.Close();
        }
        ViewBag.service=services1;
        ViewBag.total=total;
        ViewBag.checkin=checkin;
        ViewBag.checkout=checkout;
        ViewBag.number=number;
        ViewBag.room=rooms;
        ViewBag.promotion=promotions;
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Payment.cshtml");
    }
    public IActionResult Pay(Multiple multiple){
        ViewBag.customer_avatar=GetAvatar();
        int booking_id=1;
        int payment_id=1;
        int invoice_id=1;
        int invoice_detail_id=1;
        int visit=1;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ID FROM BOOKINGS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(booking_id == reader.GetInt32(0)){
                    booking_id=booking_id+1;
                }
            }

            reader.Close();
            query="SELECT VISIT FROM CUSTOMERS WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",multiple.customer!.id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                visit=reader.GetInt32(0)+ 1;
            }

            reader.Close();
            query="UPDATE CUSTOMERS SET VISIT=@visit WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@visit",visit);
            command.Parameters.AddWithValue("@id",multiple.customer!.id);
            reader = command.ExecuteReader();

            reader.Close();
            query="INSERT INTO BOOKINGS (ID, CUSTOMER_ID, ROOM_ID,CHECK_IN_DATE, CHECK_OUT_DATE,NUMBER_OF_GUESTS,TOTAL_PRICE,STATUS) VALUES(@id,@customer_id,@room_id,@check_in_date, @check_out_date,@number_of_guests,@total_price,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",booking_id);
            command.Parameters.AddWithValue("@customer_id",multiple.customer!.id);
            command.Parameters.AddWithValue("@room_id",multiple.room!.id);
            command.Parameters.AddWithValue("@check_in_date",multiple.booking!.check_in_date);
            command.Parameters.AddWithValue("@check_out_date",multiple.booking!.check_out_date);
            command.Parameters.AddWithValue("@number_of_guests",multiple.booking!.number_of_guest);
            command.Parameters.AddWithValue("@total_price",multiple.booking!.total_price);
            command.Parameters.AddWithValue("@status","Pending");
            reader = command.ExecuteReader();

            reader.Close();
            foreach(var service in multiple.services){
                int booking_service_id=1;
                query="SELECT ID FROM BOOKING_SERVICE ORDER BY ID ASC";
                command = new MySqlCommand(query, connection);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    if(booking_service_id == reader.GetInt32(0)){
                        booking_service_id=booking_service_id+1;
                    }
                }
                reader.Close();
                query="INSERT INTO BOOKING_SERVICE (ID,BOOKING_ID,SERVICE_ID) VALUES(@id,@booking_id, @service_id)";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", booking_service_id);
                command.Parameters.AddWithValue("@booking_id", booking_id);
                command.Parameters.AddWithValue("@service_id", service.id);
                reader = command.ExecuteReader();

                reader.Close();
            }
             
            query="SELECT ID FROM PAYMENT ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(payment_id==reader.GetInt32(0)){
                    payment_id=payment_id+1;
                }
            }

            reader.Close();
            query="INSERT INTO PAYMENT (ID,CUSTOMER_ID,AMOUNT,PAYMENT_TYPE,PAYMENT_DATE, STATUS) values(@id, @customer_id,@amount,@payment_type,@payment_date,@status )";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",payment_id);
            command.Parameters.AddWithValue("@customer_id",multiple.customer!.id);
            command.Parameters.AddWithValue("@amount",multiple.booking!.total_price);
            command.Parameters.AddWithValue("@payment_type","Paypal");
            command.Parameters.Add("@payment_date", MySqlDbType.Date).Value = DateTime.Now;
            command.Parameters.AddWithValue("@status","Full payment");
            reader = command.ExecuteReader();

            reader.Close();
            query="SELECT ID FROM INVOICES ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(invoice_id==reader.GetInt32(0)){
                    invoice_id=invoice_id+1;
                }
            }

            reader.Close();
            query="INSERT INTO INVOICES (ID,INVOICE_NUMBER,TOTAL_AMOUNT,STATUS) VALUES(@id,@invoice_number,@total_amount,@status)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",invoice_id);
            command.Parameters.AddWithValue("@invoice_number","Invoice-"+invoice_id);
            command.Parameters.AddWithValue("@total_amount",multiple.booking.total_price);
            command.Parameters.AddWithValue("@status",0);
            reader = command.ExecuteReader();

            reader.Close();
            query="SELECT ID FROM INVOICES_DETAILS ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(invoice_detail_id== reader.GetInt32(0)){
                    invoice_detail_id=invoice_detail_id+1;
                }
            }

            reader.Close();
            query="INSERT INTO INVOICES_DETAILS (ID, INVOICE_ID,BOOKING_ID,CREATE_DATE, DUE_DATE,PAYMENT_ID) VALUES(@id,@invoice_id,@booking_id,@create_date,@due_date,@payment_id)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",invoice_detail_id);
            command.Parameters.AddWithValue("@invoice_id",invoice_id);
            command.Parameters.AddWithValue("@booking_id",booking_id);
            command.Parameters.Add("@create_date", MySqlDbType.Date).Value = DateTime.Now;
            command.Parameters.Add("@due_date", MySqlDbType.Date).Value = DateTime.Now;
            command.Parameters.AddWithValue("@payment_id",payment_id);
            reader = command.ExecuteReader();

            reader.Close();
            if(multiple.promotion != null){
                query="UPDATE INVOICES_DETAILS SET PROMOTION_ID=@promotion_id WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@promotion_id",multiple.promotion.id);
                command.Parameters.AddWithValue("@id",invoice_detail_id);
                reader = command.ExecuteReader();
            }

            reader.Close();
            query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@status",1);
            command.Parameters.AddWithValue("@id",multiple.room!.id);
            reader = command.ExecuteReader();

            connection.Close();
        }
        return View("~/Views/HotelViews/Customer/Notification.cshtml");
    }
    public IActionResult Wedding(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Wedding.cshtml");
    }
    public IActionResult Notification(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/Notification.cshtml");
    }
    public IActionResult Reservation(){
        ViewBag.customer_avatar=GetAvatar();
        Customer customer =new Customer();
        List<Booking> rooms=new List<Booking>();
        var usernameSession = HttpContext.Session.GetString("username1");
        var passwordSession = HttpContext.Session.GetString("password1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select customers.id, customers.name, customers.gender, customers.dateofbirth, customers.email,customers.phone, customers.address, customers_img.img, accounts.username, accounts.password from customers, customers_img, accounts where customers.id=customers_img.customer_id and customers.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.id=reader.GetInt32(0);
                customer.name=reader.GetString(1);
                customer.gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                customer.dateofbirth=day.ToString("dd-MM-yyyy");
                customer.email=reader.GetString(4);
                customer.phone=reader.GetString(5);
                customer.address=reader.GetString(6);
                customer.img=reader.GetString(7);
                customer.account!.username=reader.GetString(8);
                customer.account!.password=reader.GetString(9);
            }

            reader.Close();
            query = "select ROOMS.ID, ROOMS.NAME, ROOMS.TYPE, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME , BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, ROOMS_IMG.IMG, BOOKINGS.STATUS, BOOKINGS.TOTAL_PRICE, BOOKINGS.ID from customers, VIEW, accounts, ROOMS, BOOKINGS, ROOMS_IMG where ROOMS.VIEW_ID=VIEW.ID and customers.account_id=accounts.id AND BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID AND ROOMS_IMG.ROOM_ID=ROOMS.ID and accounts.username =@username and accounts.password=@password AND BOOKINGS.CHECK_OUT_DATE>= CURRENT_DATE()";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Booking booking=new Booking();
                booking.room!.id=reader.GetInt32(0);
                booking.room!.name=reader.GetString(1);
                booking.room!.type=reader.GetString(2);
                booking.room!.floor=reader.GetInt32(3);
                booking.room!.size=reader.GetString(4);
                booking.room!.price=reader.GetFloat(5);
                booking.room!.view!.name=reader.GetString(6);
                DateTime day =reader.GetDateTime(7);
                DateTime day1 =reader.GetDateTime(8);
                booking.check_in_date=day.ToString("dd-MM-yyyy");
                booking.check_out_date=day1.ToString("dd-MM-yyyy");
                booking.number_of_guest=reader.GetInt32(9);
                booking.room.img=reader.GetString(10);
                booking.status=reader.GetString(11);
                booking.total_price=reader.GetFloat(12);
                booking.id=reader.GetInt32(13);
                rooms.Add(booking);
            }
            connection.Close();
        }
        ViewBag.booking=rooms;
        return View("~/Views/HotelViews/Customer/Reservation.cshtml", customer);
    }
    public IActionResult Account(){
        ViewBag.customer_avatar=GetAvatar();
        Customer customer =new Customer();
        var usernameSession = HttpContext.Session.GetString("username1");
        var passwordSession = HttpContext.Session.GetString("password1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select customers.id, customers.name, customers.gender, customers.dateofbirth, customers.email,customers.phone, customers.address, customers_img.img, accounts.username, accounts.password from customers, customers_img, accounts where customers.id=customers_img.customer_id and customers.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.id=reader.GetInt32(0);
                customer.name=reader.GetString(1);
                customer.gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                customer.dateofbirth=day.ToString("dd-MM-yyyy");
                customer.email=reader.GetString(4);
                customer.phone=reader.GetString(5);
                customer.address=reader.GetString(6);
                customer.img=reader.GetString(7);
                customer.account!.username=reader.GetString(8);
                customer.account!.password=reader.GetString(9);
            }
            connection.Close();
        }
        return View("~/Views/HotelViews/Customer/Account.cshtml",customer);
    }
    public IActionResult EditAccount(){
        ViewBag.customer_avatar=GetAvatar();
        Customer customer =new Customer();
        var usernameSession = HttpContext.Session.GetString("username1");
        var passwordSession = HttpContext.Session.GetString("password1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select customers.id, customers.name, customers.gender, customers.dateofbirth, customers.email,customers.phone, customers.address, customers_img.img, accounts.username, accounts.password from customers, customers_img, accounts where customers.id=customers_img.customer_id and customers.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.id=reader.GetInt32(0);
                customer.name=reader.GetString(1);
                customer.gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                customer.dateofbirth=day.ToString("yyyy-MM-dd");
                customer.email=reader.GetString(4);
                customer.phone=reader.GetString(5);
                customer.address=reader.GetString(6);
                customer.img=reader.GetString(7);
                customer.account!.username=reader.GetString(8);
                customer.account!.password=reader.GetString(9);
            }
            connection.Close();
        }
        return View("~/Views/HotelViews/Customer/EditAccount.cshtml", customer);
    }
    public IActionResult EditAccount1(){
        ViewBag.customer_avatar=GetAvatar();
        return View("~/Views/HotelViews/Customer/EditAccount.cshtml");
    }
    public IActionResult History(){
        ViewBag.customer_avatar=GetAvatar();
        Customer customer =new Customer();
        List<Booking> rooms=new List<Booking>();
        var usernameSession = HttpContext.Session.GetString("username1");
        var passwordSession = HttpContext.Session.GetString("password1");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "select customers.id, customers.name, customers.gender, customers.dateofbirth, customers.email,customers.phone, customers.address, customers_img.img, accounts.username, accounts.password from customers, customers_img, accounts where customers.id=customers_img.customer_id and customers.account_id=accounts.id and accounts.username =@username and accounts.password=@password";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                customer.id=reader.GetInt32(0);
                customer.name=reader.GetString(1);
                customer.gender=reader.GetString(2);
                DateTime day=reader.GetDateTime(3);
                customer.dateofbirth=day.ToString("dd-MM-yyyy");
                customer.email=reader.GetString(4);
                customer.phone=reader.GetString(5);
                customer.address=reader.GetString(6);
                customer.img=reader.GetString(7);
                customer.account!.username=reader.GetString(8);
                customer.account!.password=reader.GetString(9);
            }

            reader.Close();
            query = "select ROOMS.ID, ROOMS.NAME, ROOMS.TYPE, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.PRICE, VIEW.NAME , BOOKINGS.CHECK_IN_DATE, BOOKINGS.CHECK_OUT_DATE, BOOKINGS.NUMBER_OF_GUESTS, ROOMS_IMG.IMG from customers, VIEW, accounts, ROOMS, BOOKINGS, ROOMS_IMG where ROOMS.VIEW_ID=VIEW.ID and customers.account_id=accounts.id AND BOOKINGS.CUSTOMER_ID=CUSTOMERS.ID AND BOOKINGS.ROOM_ID=ROOMS.ID AND ROOMS_IMG.ROOM_ID=ROOMS.ID and accounts.username =@username and accounts.password=@password";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("username",usernameSession);
            command.Parameters.AddWithValue("password",passwordSession);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Booking booking=new Booking();
                booking.room!.id=reader.GetInt32(0);
                booking.room!.name=reader.GetString(1);
                booking.room!.type=reader.GetString(2);
                booking.room!.floor=reader.GetInt32(3);
                booking.room!.size=reader.GetString(4);
                booking.room!.price=reader.GetFloat(5);
                booking.room!.view!.name=reader.GetString(6);
                DateTime day =reader.GetDateTime(7);
                DateTime day1 =reader.GetDateTime(8);
                booking.check_in_date=day.ToString("dd-MM-yyyy");
                booking.check_out_date=day1.ToString("dd-MM-yyyy");
                booking.number_of_guest=reader.GetInt32(9);
                booking.room.img=reader.GetString(10);
                rooms.Add(booking);
            }
            connection.Close();
        }
        ViewBag.booking=rooms;
        return View("~/Views/HotelViews/Customer/History.cshtml", customer);
    }
    public string Welcome(){
        return "Welcome";
    }
    public async Task<IActionResult> UpdateCustomer(Customer customer, IFormFile file){
        int? account_id=0;
        ModelState.Remove("file");
        ModelState.Remove("account.type");
        ModelState.Remove("status");
        if(!ModelState.IsValid){
            return RedirectToAction("EditAccount");
        }
        // try{
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {   
                connection.Open();
                string query = "SELECT COUNT(*) FROM CUSTOMERS WHERE PHONE=@phone and ID <> @id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@phone",customer?.phone);
                command.Parameters.AddWithValue("@id",customer?.id);
                MySqlDataReader reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                        TempData["status1"] ="Phone number is already used";
                        return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query = "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL=@email and ID <> @id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email",customer?.email);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                        TempData["status1"] ="Email is already used";
                        return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query="SELECT COUNT(*) FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS.ID <> @id AND ACCOUNTS.USERNAME=@username";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",customer?.id);
                command.Parameters.AddWithValue("@username",customer?.account?.username);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    if(reader.GetInt32(0)>0){
                    TempData["status1"] ="Username is already existed";
                    return RedirectToAction("AdminCustomer");
                    }
                }

                reader.Close();
                query = "UPDATE CUSTOMERS SET NAME=@name, GENDER=@gender, DATEOFBIRTH=@dateofbirth, EMAIL=@email, PHONE=@phone, ADDRESS=@address, STATUS=@status WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@name",customer?.name);
                command.Parameters.AddWithValue("@gender",customer?.gender);
                command.Parameters.AddWithValue("@dateofbirth",customer?.dateofbirth);
                command.Parameters.AddWithValue("@email",customer?.email);
                command.Parameters.AddWithValue("@phone",customer?.phone);
                command.Parameters.AddWithValue("@address",customer?.address);
                command.Parameters.AddWithValue("@status",1);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();

                reader.Close();
                query="SELECT ACCOUNTS.ID FROM ACCOUNTS, CUSTOMERS WHERE ACCOUNTS.ID=CUSTOMERS.ACCOUNT_ID AND CUSTOMERS.ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",customer?.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    account_id=reader.GetInt32(0);
                }

                reader.Close();
                query="UPDATE ACCOUNTS SET USERNAME=@username, PASSWORD=@password WHERE ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username",customer?.account?.username);
                command.Parameters.AddWithValue("@password",customer?.account?.password);
                command.Parameters.AddWithValue("@id",account_id);
                reader = command.ExecuteReader();
                reader.Close();
                var newFileName="";
                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension=Path.GetExtension(file.FileName);
                    newFileName = fileName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image/Avatar", newFileName);
                    if(!System.IO.File.Exists(path)){
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    query = "UPDATE CUSTOMERS_IMG SET IMG=@img WHERE CUSTOMER_ID=@id";
                    command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@img", newFileName);
                    command.Parameters.AddWithValue("@id",customer?.id);
                    reader = command.ExecuteReader();
                }
                connection.Close();
            }
            TempData["status"] ="Update successfully";
        // }catch(Exception){
        //     TempData["status1"] ="Update Failed";
        // }
        return RedirectToAction("Account");
    }
    [HttpPost]
    public IActionResult SearchRoom(Booking booking){
        HttpContext.Session.SetString("Checkin", booking.check_in_date!);
        HttpContext.Session.SetString("Checkout", booking.check_out_date!);
        HttpContext.Session.SetInt32("Numberofguest", booking.number_of_guest);
        return RedirectToAction("RoomStandard");
    }
    [HttpPost]
    public IActionResult SearchRoom1(string checkin, string checkout, int number){
        HttpContext.Session.SetString("Checkin", checkin);
        HttpContext.Session.SetString("Checkout", checkout);
        HttpContext.Session.SetInt32("Numberofguest", number);
        return RedirectToAction("RoomStandard");
    }
    [HttpPost]
    public IActionResult AddReview(int rate, string comment, int id){
        var userSession = HttpContext.Session.GetString("username1");
        if (string.IsNullOrEmpty(userSession))
        {
            // Session người dùng không tồn tại, chuyển hướng đến trang đăng nhập hoặc trang không được ủy quyền
            return RedirectToAction("Login", "Login");
        }
        int review_id=1;
        Customer customer=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM REVIEWS ORDER BY ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                if(review_id == reader.GetInt32(0)){
                    review_id=review_id+1;
                }
            }

            reader.Close();
            query = "INSERT INTO REVIEWS (ID,CUSTOMER_ID,RATING,ROOM_ID,COMMENT) VALUES(@id, @customer_id,@rating,@room_id,@comment)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",review_id);
            command.Parameters.AddWithValue("@customer_id",customer.id);
            command.Parameters.AddWithValue("@rating",rate);
            command.Parameters.AddWithValue("@room_id",id);
            command.Parameters.AddWithValue("@comment",comment);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert comment successfully";
        var routeValues = new RouteValueDictionary { { "id", id } };
        return RedirectToAction("RoomDetail", routeValues);
    }
    public IActionResult CanceledReservation(int booking_id, int room_id){
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE BOOKINGS SET STATUS=@status WHERE BOOKINGS.ID=@id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@status","Canceled");
            command.Parameters.AddWithValue("@id",booking_id);
            MySqlDataReader reader = command.ExecuteReader();

            reader.Close();
            query="UPDATE ROOMS SET STATUS=@status WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@status",1);
            command.Parameters.AddWithValue("@id",room_id);
            reader = command.ExecuteReader();
            connection.Close();
        }
        return RedirectToAction("Reservation");
    }
}
