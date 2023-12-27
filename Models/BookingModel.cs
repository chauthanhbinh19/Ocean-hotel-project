using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Project.Models;

public class Booking{
    public int id {get;set;}
    public Customer? customer {get;set;}
    public Rooms? room {get; set;}
    public Service? service {get;set;}
    public int? service_id {get; set;}
    [Required(ErrorMessage = "Check in date is required")]
    public string? check_in_date {get; set;}
    [Required(ErrorMessage = "Check out date is required")]
    public string? check_out_date {get; set;}
    [Required(ErrorMessage = "Number of people is required")]
    public int number_of_guest {get;set;}
    public float? total_price {get; set;}
    public string? status {get; set;}
    public Booking(){
        customer=new Customer();
        room=new Rooms();
        service=new Service();
    }
}