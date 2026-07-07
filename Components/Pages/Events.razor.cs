using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages;

public partial class Events
{
    private List<EventItem> _upcoming = new();
    private List<EventItem> _past = new();

    protected override async Task OnInitializedAsync()
    {
        _upcoming = await Supabase.GetUpcomingEventsAsync();
        _past = await Supabase.GetPastEventsAsync();
    }
}
