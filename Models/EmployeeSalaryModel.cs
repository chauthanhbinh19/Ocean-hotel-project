using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class EmployeeSalary{
    public int id {get;set;}
    public int id1 {get;set;}
    [Required(ErrorMessage = "Employee name is required")]
    public string? employee_name {get;set;}
    [Required(ErrorMessage = "Email is required")]
    public string? email {get; set;}
    [Required(ErrorMessage = "From date is required")]
    public string? from_date {get; set;}
    [Required(ErrorMessage = "To date is required")]
    public string? to_date {get; set;}
    [Required(ErrorMessage = "Role is required")]
    public string? role {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number for salary")]
    [Required(ErrorMessage = "Salary is required")]
    public float? salary {get; set;}
}