using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Promotion{
    public int id {get;set;}
    [Required(ErrorMessage = "Name is required")]
    public string? name {get;set;}
    [Required(ErrorMessage = "Discription is required")]
    public string? description {get; set;}
    [Required(ErrorMessage = "Discount is required")]
    public float? discount_percent {get; set;}
    [Required(ErrorMessage = "Valid from date is required")]
    public string? valid_from {get; set;}
    [Required(ErrorMessage = "Valid to date is required")]
    public string? valid_to {get; set;}
    public string? room_id {get; set;}
    public string? customer_id {get; set;}
}