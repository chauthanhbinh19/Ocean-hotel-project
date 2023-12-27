using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Amenities{
    public int id {get;set;}
    [Required(ErrorMessage = "Amenities name is required")]
    public string? name {get;set;}
    [Required(ErrorMessage = "Description is required")]
    public string? description {get; set;}
    [RegularExpression(@"^[0-9]+$")]
    [Required(ErrorMessage = "Quantity is required")]
    public int? quantity {get;set;}
}