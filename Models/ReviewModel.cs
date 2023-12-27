using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Review{
    public int id {get;set;}
    public int? customer_id {get;set;}
    public Double? rate {get; set;}
    public string? comment {get; set;}
}