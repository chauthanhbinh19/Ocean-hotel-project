using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class InvoiceDetails{
    public int id {get;set;}
    public int invoice_id {get;set;}
    public int booking_id {get;set;}
    [Required(ErrorMessage = "Create date is required")]
    public string? create_date {get;set;}
    [Required(ErrorMessage = "Due date is required")]
    public string? due_date {get; set;}
    public int? payment_id {get; set;}
    [Required(ErrorMessage = "Room type is required")]
    public string? room_type {get;set;}
}