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

public class RoomController : Controller
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
    public IActionResult AdminRoom(int page){
        List<Rooms> rooms=new List<Rooms>();
        HttpContext.Session.Remove("RoomSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID ORDER BY ROOMS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                Rooms room=new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    type=reader.GetString(2),
                    floor=reader.GetInt32(3),
                    size=reader.GetString(4),
                    price=reader.GetFloat(5),
                    view_id=reader.GetInt32(6),
                    status=statusString,
                    img=reader.GetString(8),
                    number_of_guest=reader.GetInt32(9)
                };
                rooms.Add(room);
            }
            reader.Close();
            //Thêm Furniture vào rooms
            foreach(Rooms room in rooms){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms
            foreach(Rooms room in rooms){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.status=TempData["status"];
        return View("~/Views/HotelViews/AdminRoom.cshtml", pagedRoom);
    }
    public IActionResult AdminRoomStandard(int page){
        List<Rooms> rooms_standard=new List<Rooms>();
        HttpContext.Session.Remove("RoomStandardSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG,ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID ORDER BY ROOMS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Standard"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_standard.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_standard.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        return View("~/Views/HotelViews/AdminRoomStandard.cshtml", pagedRoom);
    }
    public IActionResult AdminRoomSuperior(int page){
        List<Rooms> rooms_superior=new List<Rooms>();
        HttpContext.Session.Remove("RoomSuperiorSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG,ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID ORDER BY ROOMS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Superior"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_superior.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_superior
            foreach(Rooms room in rooms_superior){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_superior
            foreach(Rooms room in rooms_superior){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_superior.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        return View("~/Views/HotelViews/AdminRoomSuperior.cshtml", pagedRoom);
    }
    public IActionResult AdminRoomDeluxe(int page){
        List<Rooms> rooms_deluxe=new List<Rooms>();
        HttpContext.Session.Remove("RoomDeluxeSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID ORDER BY ROOMS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Deluxe"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_deluxe.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_deluxe
            foreach(Rooms room in rooms_deluxe){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_deluxe
            foreach(Rooms room in rooms_deluxe){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_deluxe.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        return View("~/Views/HotelViews/AdminRoomDeluxe.cshtml", pagedRoom);
    }
    public IActionResult AdminRoomSuite(int page){
        List<Rooms> rooms_suite=new List<Rooms>();
        HttpContext.Session.Remove("RoomSuiteSearch");
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID ORDER BY ROOMS.ID ASC";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Suite"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_suite.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_suite
            foreach(Rooms room in rooms_suite){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_suite
            foreach(Rooms room in rooms_suite){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_suite.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        return View("~/Views/HotelViews/AdminRoomSuite.cshtml", pagedRoom);
    }
    public IActionResult AdminAddRoom(){
        List<Amenities> amenities= new List<Amenities>();
        List<Furniture> furnitures= new List<Furniture>();
        Rooms rooms= new Rooms();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME FROM amenities";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Amenities amenities1= new Amenities{
                    name=reader.GetString(0)
                };
                amenities.Add(amenities1);
            }
            reader.Close();
            query = "SELECT FURNITURE_NAME FROM furniture";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }
            connection.Close();
        }
        ViewBag.furnitures_list=furnitures;
        ViewBag.amenities_list=amenities;
        rooms.furnitures=furnitures;
        rooms.amenities=amenities;
        return View("~/Views/HotelViews/AdminAddRoom.cshtml",rooms);
    }
    public IActionResult AdminEditRoom(){
        List<Amenities> amenities= new List<Amenities>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Rooms> rooms1= new List<Rooms>();
        Rooms rooms= new Rooms();
        ViewBag.employee_avatar=GetAvatar();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME FROM amenities";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Amenities amenities1= new Amenities{
                    name=reader.GetString(0)
                };
                amenities.Add(amenities1);
            }

            reader.Close();
            query = "SELECT FURNITURE_NAME FROM furniture";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT * FROM ROOMS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms room= new Rooms{
                    id=(int)reader["id"],
                    name=(string)reader["name"]
                };
                rooms1.Add(room);
            }
            connection.Close();
        }
        rooms.furnitures=furnitures;
        rooms.amenities=amenities;
        ViewBag.furnitures_list=furnitures;
        ViewBag.amenities_list=amenities;
        ViewBag.rooms_list=rooms1;
        return View("~/Views/HotelViews/AdminEditRoom.cshtml", rooms);
    }
    [HttpPost]
    public async Task<IActionResult> AdminInsertRoom(Rooms room, IFormFile file){
        int? id=0;
        int room_id=1;
        for(int i=0; i< room.furnitures!.Count;i++){
            string query="furnitures["+ i +"].name";
            string query1="furnitures["+ i +"].type";
            string query2="furnitures["+ i +"].quanity";
            string query3="furnitures["+ i +"].price";
            ModelState.Remove(query);
            ModelState.Remove(query1);
            ModelState.Remove(query2);
            ModelState.Remove(query3);
        }
        for(int i=0; i< room.amenities!.Count;i++){
            string query="amenities["+ i +"].name";
            string query1="amenities["+ i +"].description";
            string query2="amenities["+ i +"].quanity";
            ModelState.Remove(query);
            ModelState.Remove(query1);
            ModelState.Remove(query2);
        }
        ModelState.Remove("file");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminAddRoom");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM VIEW WHERE NAME=@name";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",room?.view?.name);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
            }

            reader.Close();
            query = "SELECT ID FROM ROOMS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(room_id==reader.GetInt32(0)){
                    room_id=room_id+1;
                }
            }

            reader.Close();
            query = "INSERT INTO ROOMS (ID,NAME,TYPE,FLOOR,SIZE,PRICE,VIEW_ID,STATUS,NUMBER_OF_GUEST) VALUES(@id,@name, @type,@floor,@size,@price,@view_id,@status,@number_of_guest)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",room_id);
            command.Parameters.AddWithValue("@name",room?.name);
            command.Parameters.AddWithValue("@type",room?.type);
            command.Parameters.AddWithValue("@floor",room?.floor);
            command.Parameters.AddWithValue("@size",room?.size);
            command.Parameters.AddWithValue("@price",room?.price);
            command.Parameters.AddWithValue("@view_id",id);
            command.Parameters.AddWithValue("@status",1);
            command.Parameters.AddWithValue("@number_of_guest",room?.number_of_guest);
            reader = command.ExecuteReader();

            // List<Amenities>? amenities= room?.amenities;
            if(room?.amenities !=null){
                foreach(Amenities amenities1 in room?.amenities!){
                    if(amenities1.name!=null){
                        reader.Close();
                        int? amenities_id=0;
                        query="SELECT ID  FROM AMENITIES WHERE NAME=@name";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name",amenities1.name);
                        reader = command.ExecuteReader();
                        while(reader.Read()){
                            amenities_id=reader.GetInt32(0);
                        }

                        reader.Close();
                        query="INSERT INTO rooms_amenities (amenities_id,rooms_id,quanity) VALUES(@amenities_id,@rooms_id, @quantity)";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@amenities_id",amenities_id);
                        command.Parameters.AddWithValue("@rooms_id",room_id);
                        command.Parameters.AddWithValue("@quantity",amenities1.quantity);
                        reader = command.ExecuteReader();
                    }                   
                }
            }

            // List<Furniture>? furnitures = room?.furnitures;
            if(room?.furnitures != null){
                foreach(Furniture furniture in room?.furnitures!){
                    if(furniture.name != null){
                        reader.Close();
                        int? furniture_id=0;
                        query="SELECT ID  FROM furniture WHERE FURNITURE_NAME=@name";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name",furniture.name);
                        reader = command.ExecuteReader();
                        while(reader.Read()){
                            furniture_id=reader.GetInt32(0);
                        }

                         reader.Close();
                        query="INSERT INTO furniture_room (furniture_id,rooms_id,quanity) VALUES(@furniture_id,@rooms_id,@quantity)";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@furniture_id",furniture_id);
                        command.Parameters.AddWithValue("@rooms_id",room_id);
                        command.Parameters.AddWithValue("@quantity",furniture.quanity);
                        reader = command.ExecuteReader();
                    }
                }
            }

            var newFileName="";
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension=Path.GetExtension(file.FileName);
                newFileName = fileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image", newFileName);
                if(!System.IO.File.Exists(path)){
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            int emp_img_id=1;
            reader.Close();
            query="SELECT ID FROM ROOMS_IMG ORDER BY ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                if(emp_img_id == reader.GetInt32(0)){
                    emp_img_id=emp_img_id+1;
                }
            }

            reader.Close();
            query="INSERT INTO ROOMS_IMG (ID,ROOM_ID,IMG) VALUES(@id, @room_id,@img)";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", emp_img_id);
            command.Parameters.AddWithValue("@room_id", room_id);
            command.Parameters.AddWithValue("@img", newFileName);
            reader = command.ExecuteReader();
            connection.Close();
        }
        TempData["status"] ="Insert successfully";
        return RedirectToAction("AdminRoom");
    }
    [HttpPost]
    public async Task<IActionResult> AdminUpdateRoom(Rooms room, IFormFile file){
        int? id=0;
        int sta=1;
        for(int i=0; i< room.furnitures!.Count;i++){
            string query="furnitures["+ i +"].name";
            string query1="furnitures["+ i +"].type";
            string query2="furnitures["+ i +"].quanity";
            string query3="furnitures["+ i +"].price";
            ModelState.Remove(query);
            ModelState.Remove(query1);
            ModelState.Remove(query2);
            ModelState.Remove(query3);
        }
        for(int i=0; i< room.amenities!.Count;i++){
            string query="amenities["+ i +"].name";
            string query1="amenities["+ i +"].description";
            string query2="amenities["+ i +"].quanity";
            ModelState.Remove(query);
            ModelState.Remove(query1);
            ModelState.Remove(query2);
        }
        ModelState.Remove("file");
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditRoom");
        }
        if(room.status == "Active"){
            sta=1;
        }else{
            sta=0;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ID FROM VIEW WHERE NAME=@name";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",room?.view?.name);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                id=reader.GetInt32(0);
            }

            reader.Close();
            query = "UPDATE ROOMS SET NAME=@name, TYPE=@type, FLOOR=@floor,SIZE=@size,PRICE=@price,VIEW_ID=@view_id,STATUS=@status,NUMBER_OF_GUEST=@number_of_guest WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name",room?.name);
            command.Parameters.AddWithValue("@type",room?.type);
            command.Parameters.AddWithValue("@floor",room?.floor);
            command.Parameters.AddWithValue("@size",room?.size);
            command.Parameters.AddWithValue("@price",room?.price);
            command.Parameters.AddWithValue("@view_id",id);
            command.Parameters.AddWithValue("@status",sta);
            command.Parameters.AddWithValue("@number_of_guest",room?.number_of_guest);
            command.Parameters.AddWithValue("@id",room?.id);
            reader = command.ExecuteReader();

            // List<Amenities>? amenities= room?.amenities;
            if(room?.amenities !=null){
                reader.Close();
                query = "DELETE from rooms_amenities where rooms_id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",room?.id);
                reader = command.ExecuteReader();

                foreach(Amenities amenities1 in room?.amenities!){
                    if(amenities1.name!=null && amenities1.quantity != 0){
                        reader.Close();
                        int? amenities_id=0;
                        query="SELECT ID  FROM AMENITIES WHERE NAME=@name";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name",amenities1.name);
                        reader = command.ExecuteReader();
                        while(reader.Read()){
                            amenities_id=reader.GetInt32(0);
                        }

                        reader.Close();
                        query="INSERT INTO rooms_amenities (amenities_id,rooms_id,quanity) VALUES(@amenities_id,@rooms_id, @quantity)";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@amenities_id",amenities_id);
                        command.Parameters.AddWithValue("@rooms_id",room?.id);
                        command.Parameters.AddWithValue("@quantity",amenities1.quantity);
                        reader = command.ExecuteReader();
                    }                   
                }
            }

            // List<Furniture>? furnitures = room?.furnitures;
            if(room?.furnitures != null){
                reader.Close();
                query = "DELETE from furniture_room where rooms_id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",room?.id);
                reader = command.ExecuteReader();

                foreach(Furniture furniture in room?.furnitures!){
                    if(furniture.name != null && furniture.quanity != 0){
                        reader.Close();
                        int? furniture_id=0;
                        query="SELECT ID  FROM furniture WHERE FURNITURE_NAME=@name";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@name",furniture.name);
                        reader = command.ExecuteReader();
                        while(reader.Read()){
                            furniture_id=reader.GetInt32(0);
                        }

                         reader.Close();
                        query="INSERT INTO furniture_room (furniture_id,rooms_id,quanity) VALUES(@furniture_id,@rooms_id,@quantity)";
                        command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@furniture_id",furniture_id);
                        command.Parameters.AddWithValue("@rooms_id",room?.id);
                        command.Parameters.AddWithValue("@quantity",furniture.quanity);
                        reader = command.ExecuteReader();
                    }
                }
            }

            reader.Close();
            var newFileName="";
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension=Path.GetExtension(file.FileName);
                newFileName = fileName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Image", newFileName);
                if(!System.IO.File.Exists(path)){
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                query = "UPDATE ROOMS_IMG SET IMG=@img WHERE ROOM_ID=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@img", newFileName);
                command.Parameters.AddWithValue("@id", room?.id);
                reader = command.ExecuteReader();
            }
            connection.Close();
        }
        TempData["status"] ="Update successfully";
        return RedirectToAction("AdminRoom");
    }
    [HttpPost]
    public IActionResult GetRoomInfor(string selectedOption){
        string? name="";
        string? type="";
        int? floor=0;
        string size="";
        string status="";
        float price=0;
        int view_id=0;
        string view="";
        string img="";
        int number_of_guest=1;
        List<Amenities> amenities= new List<Amenities>();
        List<Furniture> furnitures= new List<Furniture>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT ROOMS.NAME,ROOMS.TYPE, ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.STATUS, ROOMS.VIEW_ID, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM ROOMS,ROOMS_IMG WHERE ROOMS.ID=@id and ROOMS_IMG.ROOM_ID=ROOMS.ID";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                name=reader.GetString(0);
                type=reader.GetString(1);
                floor=reader.GetInt32(2);
                size=reader.GetString(3);
                if(reader.GetInt32(5) ==1){
                    status="Active";
                }else{
                    status="Inactive";
                }
                view_id=reader.GetInt32(6);
                price=reader.GetFloat(4);
                img=reader.GetString(7);
                number_of_guest=reader.GetInt32(8);
            }

            reader.Close();
            query = "SELECT NAME FROM VIEW WHERE ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",view_id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                view=reader.GetString(0);
            }

            reader.Close();
            query = "select AMENITIES.name, ROOMS_AMENITIES.quanity from AMENITIES,ROOMS_AMENITIES, ROOMS where AMENITIES.ID=ROOMS_AMENITIES.amenities_id and ROOMS.ID=ROOMS_AMENITIES.ROOMS_ID and ROOMS.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Amenities amenities1= new Amenities{
                    name=reader.GetString(0),
                    quantity=reader.GetInt32(1)
                };
                amenities.Add(amenities1);
            }

            reader.Close();
            query = "select FURNITURE.furniture_name, FURNITURE_ROOM.quanity from FURNITURE,FURNITURE_ROOM, ROOMS where FURNITURE.ID=FURNITURE_ROOM.furniture_id and ROOMS.ID=FURNITURE_ROOM.ROOMS_ID and ROOMS.ID=@id";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",selectedOption);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0),
                    quanity=reader.GetInt32(1)
                };
                furnitures.Add(furniture);
            }

            connection.Close();
        }
        return Json(new {name=name, type=type,floor=floor, size=size,status=status, price=price, view=view, furnitures=furnitures, amenities=amenities, img=img, number_of_guest=number_of_guest});
    }
    public IActionResult EditRoom(int id){
        List<Amenities> amenities= new List<Amenities>();
        List<Furniture> furnitures= new List<Furniture>();
        List<Rooms> rooms1= new List<Rooms>();
        Rooms rooms= new Rooms();
        ViewBag.employee_avatar=GetAvatar();
        if(!ModelState.IsValid){
            return RedirectToAction("AdminEditRoom");
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT NAME FROM amenities";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                Amenities amenities1= new Amenities{
                    name=reader.GetString(0)
                };
                amenities.Add(amenities1);
            }

            reader.Close();
            query = "SELECT FURNITURE_NAME FROM furniture";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    name=reader.GetString(0)
                };
                furnitures.Add(furniture);
            }

            reader.Close();
            query = "SELECT * FROM ROOMS order by ID ASC";
            command = new MySqlCommand(query, connection);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Rooms room= new Rooms{
                    id=(int)reader["id"],
                    name=(string)reader["name"]
                };
                rooms1.Add(room);
            }

            reader.Close();
            query="SELECT ROOMS.ID, ROOMS.NAME, ROOMS.TYPE, ROOMS.FLOOR, ROOMS.SIZE, ROOMS.STATUS, VIEW.NAME, ROOMS.PRICE, ROOMS.STATUS, ROOMS_IMG.IMG,ROOMS.NUMBER_OF_GUEST FROM ROOMS,VIEW, ROOMS_IMG WHERE ROOMS.VIEW_ID=VIEW.ID AND ROOMS.ID=@roomId AND ROOMS_IMG.ROOM_ID=ROOMS.ID";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@roomId", id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                rooms.id=reader.GetInt32(0);
                rooms.name=reader.GetString(1);
                rooms.type=reader.GetString(2);
                rooms.floor=reader.GetInt32(3);
                rooms.size=reader.GetString(4);
                if(reader.GetInt32(5)==1){
                    rooms.status="Active";
                }else{
                    rooms.status="Inactive";
                }
                rooms.view!.name=reader.GetString(6);
                rooms.price=reader.GetFloat(7);
                if(reader.GetInt32(8)==1){
                    rooms.status="Available";
                }else{
                    rooms.status="Booked";
                }
                rooms.img=reader.GetString(9);
                rooms.number_of_guest=reader.GetInt32(10);
            }
            
            reader.Close();
            query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE_ROOM.QUANITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@RoomId",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Furniture furniture= new Furniture{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    type=reader.GetString(2),
                    quanity=reader.GetInt32(3)
                };
                if (rooms.furnitures == null)
                {
                    rooms.furnitures = new List<Furniture>();
                }
                rooms.furnitures.Add(furniture);
            }

            reader.Close();
            query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION,ROOMS_AMENITIES.QUANITY FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
            command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@RoomId",id);
            reader = command.ExecuteReader();
            while(reader.Read()){
                Amenities amenities1= new Amenities{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    description=reader.GetString(2),
                    quantity=reader.GetInt32(3)
                };
                if (rooms.amenities == null)
                {
                    rooms.amenities = new List<Amenities>();
                }
                rooms.amenities.Add(amenities1);
            }

            Furniture furniture1=new Furniture{
                name="???????????????"
            };
            for(int i=rooms.furnitures!.Count;i<furnitures.Count;i++){
                rooms.furnitures.Add(furniture1);
            }
            Amenities amenities2 = new Amenities{
                name="???????????????"
            };
            for(int i=rooms.amenities!.Count;i<amenities.Count;i++){
                rooms.amenities.Add(amenities2);
            }

            foreach(var a in furnitures){
                foreach(var b in rooms.furnitures){
                    if(a.name== b.name){
                        a.quanity=b.quanity;
                        break;
                    }
                }
            }
            foreach(var a in amenities){
                foreach(var b in rooms.amenities){
                    if(a.name==b.name){
                        a.quantity=b.quantity;
                        break;
                    }
                }
            }
            connection.Close();
        }
        ViewBag.furnitures_list=furnitures;
        ViewBag.amenities_list=amenities;
        ViewBag.rooms_list=rooms1;
        return View("~/Views/HotelViews/AdminEditRoom.cshtml", rooms);
    }
    public IActionResult DeleteRoom(int id){
        List<Rooms> rooms=new List<Rooms>();
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try{
                connection.Open();
                string query = "SELECT ROOMS.ID FROM ROOMS, BOOKINGS WHERE ROOMS.ID=BOOKINGS.ROOM_ID AND ROOMS.ID=@id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                MySqlDataReader reader = command.ExecuteReader();
                while(reader.Read()){
                    Rooms rooms1=new Rooms{
                        id=reader.GetInt32(0)
                    };
                    rooms.Add(rooms1);
                }

                if(rooms.Count !=0){
                    return View("~/Views/HotelViews/Error.cshtml");
                }
                query = "delete from rooms_amenities where rooms_id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "delete from furniture_room where rooms_id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "delete from rooms_img where room_id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();

                reader.Close();
                query = "delete from rooms where id=@id";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id",id);
                reader = command.ExecuteReader();
            }catch(Exception){
                return View("~/Views/HotelViews/Error.cshtml");
            }
        }
        return RedirectToAction("AdminRoom");
    }
    public IActionResult AdminSearchRoom(string searchkeyword,int page){
        List<Rooms> rooms=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("RoomSearch", searchkeyword);
        }
        var a=HttpContext.Session.GetString("RoomSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID and (name like @id or type like @id or floor like @id or price like @id or size like @id) ORDER BY ROOMS.ID ASC";
            // string query = "SELECT * FROM rooms Where name like @id or type like @id or floor like @id or price like @id or size like @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                Rooms room=new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    type=reader.GetString(2),
                    floor=reader.GetInt32(3),
                    size=reader.GetString(4),
                    price=reader.GetFloat(5),
                    view_id=reader.GetInt32(6),
                    status=statusString,
                    img=reader.GetString(8),
                    number_of_guest=reader.GetInt32(9)
                };
                rooms.Add(room);
            }
            reader.Close();
            //Thêm Furniture vào rooms
            foreach(Rooms room in rooms){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms
            foreach(Rooms room in rooms){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminRoom.cshtml", pagedRoom);
    }
    public IActionResult AdminSearchRoomStandard(string searchkeyword,int page){
        List<Rooms> rooms_standard=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("RoomStandardSearch", searchkeyword);
        }
        var a=HttpContext.Session.GetString("RoomStandardSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID and (name like @id or type like @id or floor like @id or price like @id or size like @id) ORDER BY ROOMS.ID ASC";
            // string query = "SELECT * FROM rooms Where name like @id or type like @id or floor like @id or price like @id or size like @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=(string)reader["type"];
                if(room_type=="Standard"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_standard.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
         int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_standard.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminRoomStandard.cshtml", pagedRoom);
    }
    public IActionResult AdminSearchRoomSuperior(string searchkeyword,int page){
        List<Rooms> rooms_superior=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("RoomSuperiorSearch", searchkeyword);
        }
        var a=HttpContext.Session.GetString("RoomSuperiorSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID and (name like @id or type like @id or floor like @id or price like @id or size like @id) ORDER BY ROOMS.ID ASC";
            // string query = "SELECT * FROM rooms Where name like @id or type like @id or floor like @id or price like @id or size like @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=(string)reader["type"];
                if(room_type=="Superior"){
                    Rooms room_superior = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_superior.Add(room_superior);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_superior
            foreach(Rooms room in rooms_superior){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_superior
            foreach(Rooms room in rooms_superior){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
         int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_superior.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminRoomSuperior.cshtml", pagedRoom);
    }
    public IActionResult AdminSearchRoomDeluxe(string searchkeyword,int page){
        List<Rooms> rooms_deluxe=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("RoomDeluxeSearch", searchkeyword);
        }
        var a=HttpContext.Session.GetString("RoomDeluxeSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID and (name like @id or type like @id or floor like @id or price like @id or size like @id) ORDER BY ROOMS.ID ASC";
            // string query = "SELECT * FROM rooms Where name like @id or type like @id or floor like @id or price like @id or size like @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=(string)reader["type"];
                if(room_type=="Deluxe"){
                    Rooms room_deluxe = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_deluxe.Add(room_deluxe);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_deluxe
            foreach(Rooms room in rooms_deluxe){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_deluxe
            foreach(Rooms room in rooms_deluxe){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
         int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_deluxe.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminRoomDeluxe.cshtml", pagedRoom);
    }
    public IActionResult AdminSearchRoomSuite(string searchkeyword,int page){
        List<Rooms> rooms_suite=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        if(searchkeyword !=null){
            HttpContext.Session.SetString("RoomSuiteSearch", searchkeyword);
        }
        var a=HttpContext.Session.GetString("RoomSuiteSearch");
        if(a!= null && searchkeyword == null){
            searchkeyword=a;
        }
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID and (name like @id or type like @id or floor like @id or price like @id or size like @id) ORDER BY ROOMS.ID ASC";
            // string query = "SELECT * FROM rooms Where name like @id or type like @id or floor like @id or price like @id or size like @id";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=(string)reader["type"];
                if(room_type=="Suite"){
                    Rooms room_suite = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_suite.Add(room_suite);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_suite
            foreach(Rooms room in rooms_suite){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_suite
            foreach(Rooms room in rooms_suite){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
         int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_suite.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=1;
        return View("~/Views/HotelViews/AdminRoomSuite.cshtml", pagedRoom);
    }
    public List<Rooms> GetAllRoom(string query){
        List<Rooms> rooms=new List<Rooms>();
        var searchkeyword=HttpContext.Session.GetString("RoomSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                Rooms room=new Rooms{
                    id=reader.GetInt32(0),
                    name=reader.GetString(1),
                    type=reader.GetString(2),
                    floor=reader.GetInt32(3),
                    size=reader.GetString(4),
                    price=reader.GetFloat(5),
                    view_id=reader.GetInt32(6),
                    status=statusString,
                    img=reader.GetString(8),
                    number_of_guest=reader.GetInt32(9)
                };
                rooms.Add(room);
            }

            reader.Close();
            //Thêm Furniture vào rooms
            foreach(Rooms room in rooms){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms
            foreach(Rooms room in rooms){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            reader.Close();
            connection.Close();
        }
        return rooms;
    }
    public List<Rooms> GetStandardRoom(string query){
        List<Rooms> rooms_standard=new List<Rooms>();
        var searchkeyword=HttpContext.Session.GetString("RoomSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Standard"){
                    Rooms room_standard = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_standard.Add(room_standard);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_standard){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        return rooms_standard;
    }
    public List<Rooms> GetSuperiorRoom(string query){
        List<Rooms> rooms_superior=new List<Rooms>();
        var searchkeyword=HttpContext.Session.GetString("RoomSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Superior"){
                    Rooms room_superior = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_superior.Add(room_superior);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_superior){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_superior){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        return rooms_superior;
    }
    public List<Rooms> GetDeluxeRoom(string query){
        List<Rooms> rooms_deluxe=new List<Rooms>();
        var searchkeyword=HttpContext.Session.GetString("RoomSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Deluxe"){
                    Rooms room_deluxe = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_deluxe.Add(room_deluxe);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_deluxe){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_deluxe){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        return rooms_deluxe;
    }
    public List<Rooms> GetSuiteRoom(string query){
        List<Rooms> rooms_suite=new List<Rooms>();
        var searchkeyword=HttpContext.Session.GetString("RoomSearch");
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id",searchkeyword);
            MySqlDataReader reader = command.ExecuteReader();
            while(reader.Read()){
                string? statusString="";
                int room_status=reader.GetInt32(7);
                if(room_status==1){
                    statusString="Available";
                }else{
                    statusString="Booked";
                }
                string? room_type=reader.GetString(2);
                if(room_type=="Suite"){
                    Rooms room_suite = new Rooms
                    {
                        id=reader.GetInt32(0),
                        name=reader.GetString(1),
                        type=reader.GetString(2),
                        floor=reader.GetInt32(3),
                        size=reader.GetString(4),
                        price=reader.GetFloat(5),
                        view_id=reader.GetInt32(6),
                        status=statusString,
                        img=reader.GetString(8),
                        number_of_guest=reader.GetInt32(9)
                    };
                    rooms_suite.Add(room_suite);
                }
            }
            reader.Close();
            //Thêm Furniture vào rooms_standard
            foreach(Rooms room in rooms_suite){
                query="SELECT FURNITURE.ID, FURNITURE.FURNITURE_NAME, FURNITURE.TYPE, FURNITURE.QUANTITY FROM FURNITURE, ROOMS, FURNITURE_ROOM WHERE FURNITURE.ID=FURNITURE_ROOM.FURNITURE_ID AND FURNITURE_ROOM.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Furniture furniture= new Furniture{
                        id=(int)reader["id"],
                        name=(string)reader["furniture_name"],
                        type=(string)reader["type"],
                        quanity=(int)reader["quantity"]
                    };
                    if (room.furnitures == null)
                    {
                        room.furnitures = new List<Furniture>();
                    }
                    room.furnitures.Add(furniture);
                }
                reader.Close();
            }
            //Thêm Amenities vào rooms_standard
            foreach(Rooms room in rooms_suite){
                query="SELECT AMENITIES.ID, AMENITIES.NAME, AMENITIES.DESCRIPTION FROM AMENITIES, ROOMS, ROOMS_AMENITIES WHERE AMENITIES.ID=ROOMS_AMENITIES.AMENITIES_ID AND ROOMS_AMENITIES.ROOMS_ID=ROOMS.ID AND ROOMS.ID =@RoomId";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@RoomId",room.id);
                reader = command.ExecuteReader();
                while(reader.Read()){
                    Amenities amenities= new Amenities{
                        id=(int)reader["id"],
                        name=(string)reader["name"],
                        description=(string)reader["description"]
                    };
                    if (room.amenities == null)
                    {
                        room.amenities = new List<Amenities>();
                    }
                    room.amenities.Add(amenities);
                }
                reader.Close();
            }
            connection.Close();
        }
        return rooms_suite;
    }
    public IActionResult AdminSortRoom(string id, int page){
        List<Rooms> rooms=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID";
        var RoomSearch=HttpContext.Session.GetString("RoomSearch");
        if(RoomSearch != null){
            query =query + " and (name like @id or type like @id or floor like @id or price like @id or size like @id)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ROOMS.ID ASC";
            rooms=GetAllRoom(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ROOMS.ID DESC";
            rooms=GetAllRoom(query);
        }else if(id == "roomname_asc"){
            query = query + " ORDER BY ROOMS.NAME ASC";
            rooms=GetAllRoom(query);
        }else if(id == "roomname_desc"){
            query = query + " ORDER BY ROOMS.NAME DESC";
            rooms=GetAllRoom(query);
        }else if(id == "floor_asc"){
            query = query + " ORDER BY ROOMS.FLOOR ASC";
            rooms=GetAllRoom(query);
        }else if(id == "floor_desc"){
            query = query + " ORDER BY ROOMS.FLOOR DESC";
            rooms=GetAllRoom(query);
        }else if(id == "roomtype_asc"){
            query = query + " ORDER BY ROOMS.TYPE ASC";
            rooms=GetAllRoom(query);
        }else if(id == "roomtype_desc"){
            query = query + " ORDER BY ROOMS.TYPE DESC";
            rooms=GetAllRoom(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY ROOMS.PRICE ASC";
            rooms=GetAllRoom(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY ROOMS.PRICE DESC";
            rooms=GetAllRoom(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY ROOMS.STATUS ASC";
            rooms=GetAllRoom(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY ROOMS.STATUS DESC";
            rooms=GetAllRoom(query);
        }else if(id == "numberofguest_asc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST ASC";
            rooms=GetAllRoom(query);
        }else if(id == "numberofguest_desc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST DESC";
            rooms=GetAllRoom(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminRoom.cshtml", pagedRoom);
    }
    public IActionResult AdminSortRoomStandard(string id, int page){
        List<Rooms> rooms_standard=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG,ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID";
        var RoomSearch=HttpContext.Session.GetString("RoomSearch");
        if(RoomSearch != null){
            query =query + " and (name like @id or type like @id or floor like @id or price like @id or size like @id)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ROOMS.ID ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ROOMS.ID DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "roomname_asc"){
            query = query + " ORDER BY ROOMS.NAME ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "roomname_desc"){
            query = query + " ORDER BY ROOMS.NAME DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "floor_asc"){
            query = query + " ORDER BY ROOMS.FLOOR ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "floor_desc"){
            query = query + " ORDER BY ROOMS.FLOOR DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "roomtype_asc"){
            query = query + " ORDER BY ROOMS.TYPE ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "roomtype_desc"){
            query = query + " ORDER BY ROOMS.TYPE DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY ROOMS.PRICE ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY ROOMS.PRICE DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY ROOMS.STATUS ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY ROOMS.STATUS DESC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "numberofguest_asc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST ASC";
            rooms_standard=GetStandardRoom(query);
        }else if(id == "numberofguest_desc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST DESC";
            rooms_standard=GetStandardRoom(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_standard.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminRoomStandard.cshtml", pagedRoom);
    }
    public IActionResult AdminSortRoomSuperior(string id, int page){
        List<Rooms> rooms_superior=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID";
        var RoomSearch=HttpContext.Session.GetString("RoomSearch");
        if(RoomSearch != null){
            query =query + " and (name like @id or type like @id or floor like @id or price like @id or size like @id)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ROOMS.ID ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ROOMS.ID DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "roomname_asc"){
            query = query + " ORDER BY ROOMS.NAME ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "roomname_desc"){
            query = query + " ORDER BY ROOMS.NAME DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "floor_asc"){
            query = query + " ORDER BY ROOMS.FLOOR ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "floor_desc"){
            query = query + " ORDER BY ROOMS.FLOOR DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "roomtype_asc"){
            query = query + " ORDER BY ROOMS.TYPE ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "roomtype_desc"){
            query = query + " ORDER BY ROOMS.TYPE DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY ROOMS.PRICE ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY ROOMS.PRICE DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY ROOMS.STATUS ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY ROOMS.STATUS DESC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "numberofguest_asc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST ASC";
            rooms_superior=GetSuperiorRoom(query);
        }else if(id == "numberofguest_desc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST DESC";
            rooms_superior=GetSuperiorRoom(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_superior.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminRoomSuperior.cshtml", pagedRoom);
    }
    public IActionResult AdminSortRoomDeluxe(string id, int page){
        List<Rooms> rooms_deluxe=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID";
        var RoomSearch=HttpContext.Session.GetString("RoomSearch");
        if(RoomSearch != null){
            query =query + " and (name like @id or type like @id or floor like @id or price like @id or size like @id)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ROOMS.ID ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ROOMS.ID DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "roomname_asc"){
            query = query + " ORDER BY ROOMS.NAME ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "roomname_desc"){
            query = query + " ORDER BY ROOMS.NAME DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "floor_asc"){
            query = query + " ORDER BY ROOMS.FLOOR ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "floor_desc"){
            query = query + " ORDER BY ROOMS.FLOOR DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "roomtype_asc"){
            query = query + " ORDER BY ROOMS.TYPE ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "roomtype_desc"){
            query = query + " ORDER BY ROOMS.TYPE DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY ROOMS.PRICE ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY ROOMS.PRICE DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY ROOMS.STATUS ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY ROOMS.STATUS DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "numberofguest_asc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST ASC";
            rooms_deluxe=GetDeluxeRoom(query);
        }else if(id == "numberofguest_desc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST DESC";
            rooms_deluxe=GetDeluxeRoom(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_deluxe.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminRoomDeluxe.cshtml", pagedRoom);
    }
    public IActionResult AdminSortRoomSuite(string id, int page){
        List<Rooms> rooms_suite=new List<Rooms>();
        ViewBag.employee_avatar=GetAvatar();
        string query="SELECT ROOMS.ID, ROOMS.NAME,ROOMS.TYPE,ROOMS.FLOOR,ROOMS.SIZE, ROOMS.PRICE, ROOMS.VIEW_ID,ROOMS.STATUS, ROOMS_IMG.IMG, ROOMS.NUMBER_OF_GUEST FROM rooms, rooms_img where ROOMS.ID=ROOMS_IMG.ROOM_ID";
        var RoomSearch=HttpContext.Session.GetString("RoomSearch");
        if(RoomSearch != null){
            query =query + " and (name like @id or type like @id or floor like @id or price like @id or size like @id)";
        }
        if(id == "id_asc"){
            query = query + " ORDER BY ROOMS.ID ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "id_desc"){
            query = query + " ORDER BY ROOMS.ID DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "roomname_asc"){
            query = query + " ORDER BY ROOMS.NAME ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "roomname_desc"){
            query = query + " ORDER BY ROOMS.NAME DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "floor_asc"){
            query = query + " ORDER BY ROOMS.FLOOR ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "floor_desc"){
            query = query + " ORDER BY ROOMS.FLOOR DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "roomtype_asc"){
            query = query + " ORDER BY ROOMS.TYPE ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "roomtype_desc"){
            query = query + " ORDER BY ROOMS.TYPE DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "price_asc"){
            query = query + " ORDER BY ROOMS.PRICE ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "price_desc"){
            query = query + " ORDER BY ROOMS.PRICE DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "status_asc"){
            query = query + " ORDER BY ROOMS.STATUS ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "status_desc"){
            query = query + " ORDER BY ROOMS.STATUS DESC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "numberofguest_asc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST ASC";
            rooms_suite=GetSuiteRoom(query);
        }else if(id == "numberofguest_desc"){
            query = query + " ORDER BY ROOMS.NUMBER_OF_GUEST DESC";
            rooms_suite=GetSuiteRoom(query);
        }
        int pageNumber=page<1 ? 1 : page;
        int pageSize =10;
        var pagedRoom= rooms_suite.ToPagedList(pageNumber,pageSize);
        ViewBag.room_list=pagedRoom;
        ViewBag.searchResult=2;
        return View("~/Views/HotelViews/AdminRoomSuite.cshtml", pagedRoom);
    }
}


