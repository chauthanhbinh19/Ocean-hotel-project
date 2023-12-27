using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Leave{
    public int id {get;set;}
    [Required(ErrorMessage = "Employee id is required")]
    public int? employee_id {get; set;}
    [Required(ErrorMessage = "Employee name is required")]
    public string? employee_name {get; set;}
    [Required(ErrorMessage = "Leave type is required")]
    public string? leave_type {get; set;}
    [Required(ErrorMessage = "From date is required")]
    public string? from_date {get;set;}
    [Required(ErrorMessage = "To date is required")]
    public string? to_date {get;set;}
    [Required(ErrorMessage = "Reason is required")]
    public string? reason {get;set;}
    public string? status {get; set;}
}