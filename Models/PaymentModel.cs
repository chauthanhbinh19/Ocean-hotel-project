using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Payment{
    public int id {get;set;}
    [Required(ErrorMessage = "Invoice id is required")]
    public int? invoice_id {get; set;}
    [Required(ErrorMessage = "Customer name is required")]
    public string? customer_name {get;set;}
    [Required(ErrorMessage = "Promotion id is required")]
    public int? promotion_id {get; set;}
    [Required(ErrorMessage = "Payment type is required")]
    public string? payment_type {get; set;}
    [Required(ErrorMessage = "Payment date is required")]
    public string? payment_date {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Amount is required")]
    public float? amount {get; set;}
    public string? status {get;set;}
}