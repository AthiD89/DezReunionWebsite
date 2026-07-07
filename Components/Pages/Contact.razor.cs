using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages;

public partial class Contact
{
    private ContactMessage _model = new();
    private bool _submitted;
    private bool _sending;
    private string? _error;

    private async Task HandleSubmit()
    {
        _error = null;
        _sending = true;
        try
        {
            var sent = await EmailService.SendAsync("New Contact Message — Dez Reunion Events", new Dictionary<string, string>
            {
                ["name"] = _model.Name,
                ["email"] = _model.Email,
                ["message"] = _model.Message
            });

            if (sent)
            {
                _submitted = true;
            }
            else
            {
                _error = "Something went wrong sending your message. Please try again, or reach out via WhatsApp instead.";
            }
        }
        catch (Exception)
        {
            _error = "Something went wrong sending your message. Please try again, or reach out via WhatsApp instead.";
        }
        finally
        {
            _sending = false;
        }
    }
}
