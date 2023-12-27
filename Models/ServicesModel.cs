using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Service{
    public int id {get;set;}
    [Required(ErrorMessage = "Service name is required")]
    public string? name {get;set;}
    [Required(ErrorMessage = "Description is required")]
    public string? description {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Price is required")]
    public float? price {get; set;}
    [Required(ErrorMessage = "Shuttle is required")]
    public string? shuttle {get; set;}
    public string? img{get;set;}
}