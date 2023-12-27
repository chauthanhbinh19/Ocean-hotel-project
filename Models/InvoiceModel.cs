using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Invoice{
    public int id {get;set;}
    [Required(ErrorMessage = "Booking id is required")]
    public int booking_id {get;set;}
    public string? name {get;set;}
    public string? phonenumber {get;set;}
    public string? email {get;set;}
    public string? invoice_number {get; set;}
    [Required(ErrorMessage = "Create date is required")]
    public string? create_date {get; set;}
    [Required(ErrorMessage = "Due date is required")]
    public string? due_date {get; set;}
    [RegularExpression(@"^[0-9]*$", ErrorMessage ="Can only use number")]
    [Required(ErrorMessage = "Total amount is required")]
    public float? total_amount {get;set;}
    [Required(ErrorMessage = "Status is required")]
    public string? status {get; set;}
    public List<Customer>? customers {get;set;}
    public List<InvoiceDetails>? invoiceDetails {get;set;}
    public Taxes? taxes {get;set;}
    public Promotion? promotion{get;set;}
    public Invoice(){
        customers=new List<Customer>();
        invoiceDetails=new List<InvoiceDetails>();
        taxes=new Taxes();
        promotion=new Promotion();
    }
}