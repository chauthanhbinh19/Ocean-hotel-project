using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Furniture{
    public int id {get;set;}
    [Required(ErrorMessage = "Name is required")]
    public string? name {get;set;}
    [Required(ErrorMessage = "Type is required")]
    public string? type {get; set;}
    public string? icon {get; set;}
    public string? img {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Quanity is required")]
    public int? quanity {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Price is required")]
    public float? price {get; set;}
}