using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DezReunionWebsite.Models;

public class EventItem
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = "";

    public DateTime Date { get; set; }
    public string? Time { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    public string Venue { get; set; } = "";

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; } = "";

    public string? DressCode { get; set; }

    [Required(ErrorMessage = "Summary is required.")]
    public string Summary { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Please upload an event photo or paste an image URL.")]
    public string ImageUrl { get; set; } = "";

    public string? TicketUrl { get; set; }
    public decimal? Price { get; set; }

    [JsonIgnore]
    public bool IsPast => Date.Date < DateTime.Today;
}
