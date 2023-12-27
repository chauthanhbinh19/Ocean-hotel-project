using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class ExpensesDetails{
    public int id {get;set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Quantity is required")]
    public int? quanity {get;set;}
    [Required(ErrorMessage = "Purchase day is required")]
    public string? purchase_at {get; set;}
    [Required(ErrorMessage = "Purchase from is required")]
    public string? purchase_from {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Amount is required")]
    public float? amount {get; set;}
    public Furniture? furniture{get;set;}
    public Taxes? taxes{get;set;}
    public Promotion promotion{get;set;}
    public ExpensesDetails(){
        furniture= new Furniture();
        taxes =new Taxes();
        promotion=new Promotion();
    }
}