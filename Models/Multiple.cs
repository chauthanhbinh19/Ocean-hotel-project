using System.ComponentModel.DataAnnotations;
namespace Project.Models;

public class Multiple{
    public Booking? booking{get;set;}
    public Rooms? room{get;set;}
    public Customer? customer{get;set;}
    public Promotion? promotion{get;set;}
    public List<Service> services{get;set;}
    public Multiple(){
        services=new List<Service>();
    }
}