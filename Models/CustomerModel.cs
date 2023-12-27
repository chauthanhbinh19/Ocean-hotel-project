using System.ComponentModel.DataAnnotations;
namespace Project.Models;
public class Customer
{
    public int id { get; set; }
    // [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Can not use special character")]
    [Required(ErrorMessage = "Name is required")]
    public string? name { get; set; }
    [Required(ErrorMessage = "Gender is required")]
    public string? gender { get; set; }
    [Required(ErrorMessage = "Date of birth is required")]
    public string? dateofbirth { get; set; }
    [Required(ErrorMessage = "Email is required")]
    public string? email { get; set; }
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Phone number is only include number ")]
    [Required(ErrorMessage = "Phone is required")]
    public string? phone { get; set; }
    [Required(ErrorMessage = "Address is required")]
    public string? address { get; set; }
    public int account_id { get; set; }
    [Required(ErrorMessage = "Status is required")]
    public string? status { get; set; }
    public Account? account { get; set; }
    public string? img {get;set;}
    public Review? review{get;set;}
    public Customer()
    {
        account = new Account();
        review=new Review();
    }
}
