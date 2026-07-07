using Microsoft.AspNetCore.Components;
using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages;

public partial class EventDetails
{
    [Parameter]
    public int Id { get; set; }

    private EventItem? _event;

    protected override async Task OnParametersSetAsync()
    {
        _event = await Supabase.GetEventByIdAsync(Id);
    }
}
