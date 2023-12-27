using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Attendance{
    public int id {get;set;}
    public int? employee_id {get;set;}
    public string? date {get; set;}
    public int? status {get; set;}
}