using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Feedback{
    public int id {get;set;}
    public int? customer_id {get;set;}
    public string? comment {get; set;}
}