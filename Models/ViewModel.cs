using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class View{
    public int id {get;set;}
    [Required(ErrorMessage = "View name is required")]
    public string? name {get;set;}
}