using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Taxes{
    public int id {get;set;}
    public string? taxes_number {get;set;}
    [Required(ErrorMessage = "Tax name is required")]
    public string? taxes_name {get; set;}
    [RegularExpression(@"^[0-9]+$")]
    [Required(ErrorMessage = "Tax percentage is required")]
    public float? tax_percentage {get; set;}
    [Required(ErrorMessage = "Status is required")]
    public string? status {get; set;}
}