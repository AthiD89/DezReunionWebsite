using System.ComponentModel.DataAnnotations.Schema;

namespace DezReunionWebsite.Models;

public class EventItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public string? Time { get; set; }
    public string Venue { get; set; } = "";
    public string Address { get; set; } = "";
    public string? DressCode { get; set; }
    public string Summary { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? TicketUrl { get; set; }
    public decimal? Price { get; set; }

    [NotMapped]
    public bool IsPast => Date.Date < DateTime.Today;
}
