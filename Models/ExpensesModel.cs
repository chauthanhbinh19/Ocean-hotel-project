using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Expenses{
    public int id {get;set;}
    public string? expenses_number {get;set;}
    public string? furniture_name {get; set;}
    [Required(ErrorMessage = "Purchase from is required")]
    public string? purchase_from {get; set;}
    [Required(ErrorMessage = "Purchase day is required")]
    public string? purchase_at {get; set;}
    public int? number {get; set;}
    public string? employee_name {get; set;}
    public float? total_amount {get;set;}
    [Required(ErrorMessage = "Status is required")]
    public string? status {get; set;}
    public List<ExpensesDetails> expensesDetails {get;set;}
    public Expenses(){
        expensesDetails=new List<ExpensesDetails>();
    }
    public int count{get;set;}
}