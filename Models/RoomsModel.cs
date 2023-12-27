using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class Rooms{
    public int id {get;set;}
    // [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage ="Can not use special character or number")]
    [Required(ErrorMessage = "Room name is required")]
    public string? name {get;set;}
    [Required(ErrorMessage = "Room type is required")]
    public string? type {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Floor is required")]
    public int? floor {get; set;}
    [Required(ErrorMessage = "Size is required")]
    public string? size {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Price is required")]
    public float? price {get; set;}
    public int? view_id {get;set;}
    [Required(ErrorMessage = "Status is required")]
    public string? status {get; set;}
    public List<Furniture>? furnitures {get; set;}
    public List<Amenities>? amenities {get; set;}
    public View? view {get;set;}
    public Review? review {get;set;}
    public string? img {get;set;}
    public int number_of_guest{get;set;}
    public Rooms()
    {
        furnitures = new List<Furniture>();
        amenities= new List<Amenities>();
        view=new View();
        review=new Review();
    }
}