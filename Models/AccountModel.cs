using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Account{
    public int id {get;set;}
    [Required(ErrorMessage = "Username is required")]
    public string? username {get;set;}
    [Required(ErrorMessage = "Password is required")]
    public string? password {get; set;} 
    public string? confirmpassword {get; set;} 
    [Required(ErrorMessage = "Type is required")]
    public string? type {get;set;}   
    [Required(ErrorMessage = "Status is required")]
    public int status {get; set;}
}