using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class Chart{
    public string? type{get;set;}
    public int? month {get; set;}
    public float? price {get; set;}
    public int? quantity {get;set;}
}