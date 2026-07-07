using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages;

public partial class Tickets
{
    private List<EventItem> _upcoming = new();
    private RsvpRequest _model = new();
    private bool _submitted;
    private bool _sending;
    private string? _error;
    private string? _loadError;

    [SupplyParameterFromQuery(Name = "event")]
    public int? EventQuery { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _upcoming = await Supabase.GetUpcomingEventsAsync();
        }
        catch (Exception)
        {
            _loadError = "Couldn't load upcoming events. Please refresh the page, or reach out via WhatsApp.";
            return;
        }

        if (EventQuery.HasValue && _upcoming.Any(e => e.Id == EventQuery.Value))
        {
            _model.EventId = EventQuery.Value;
        }
    }

    private async Task SelectEvent(int id)
    {
        _model.EventId = id;
        await JS.InvokeVoidAsync("scrollToElement", "rsvp");
    }

    private async Task HandleSubmit()
    {
        _error = null;
        _sending = true;
        try
        {
            var eventTitle = _upcoming.FirstOrDefault(e => e.Id == _model.EventId)?.Title ?? "Not specified";

            var sent = await EmailService.SendAsync("New RSVP — Dez Reunion Events", new Dictionary<string, string>
            {
                ["name"] = _model.Name,
                ["email"] = _model.Email,
                ["phone"] = _model.Phone ?? "",
                ["event"] = eventTitle,
                ["guests"] = _model.Guests.ToString()
            });

            if (sent)
            {
                _submitted = true;
            }
            else
            {
                _error = "Something went wrong sending your RSVP. Please try again, or reach out via WhatsApp instead.";
            }
        }
        catch (Exception)
        {
            _error = "Something went wrong sending your RSVP. Please try again, or reach out via WhatsApp instead.";
        }
        finally
        {
            _sending = false;
        }
    }
}
