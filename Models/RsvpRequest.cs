using System.ComponentModel.DataAnnotations;

namespace DezReunionWebsite.Models;

public class RsvpRequest
{
    [Required(ErrorMessage = "Please tell us your name.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "We need an email so we can confirm your RSVP.")]
    [EmailAddress(ErrorMessage = "That email doesn't look right.")]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "That phone number doesn't look right.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select an event.")]
    public int? EventId { get; set; }

    [Range(1, 20, ErrorMessage = "Enter a guest count between 1 and 20.")]
    public int Guests { get; set; } = 1;
}

public class ContactMessage
{
    [Required(ErrorMessage = "Please tell us your name.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "We need an email to reply to you.")]
    [EmailAddress(ErrorMessage = "That email doesn't look right.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Message can't be empty.")]
    public string Message { get; set; } = "";
}
