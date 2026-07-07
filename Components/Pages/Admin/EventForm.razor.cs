using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using DezReunionWebsite.Models;

namespace DezReunionWebsite.Components.Pages.Admin;

public partial class EventForm
{
    [Parameter]
    public int? Id { get; set; }

    private EventItem _model = new() { Date = DateTime.Today };
    private bool _notFound;
    private bool _uploading;
    private string? _error;

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue)
        {
            return;
        }

        try
        {
            var existing = await Supabase.GetEventByIdAsync(Id.Value);
            if (existing is null)
            {
                _notFound = true;
            }
            else
            {
                _model = existing;
            }
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        _error = null;
        _uploading = true;
        try
        {
            var file = e.File;
            await using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            _model.ImageUrl = await Supabase.UploadFileAsync(stream, file.Name, file.ContentType);
        }
        catch (Exception ex)
        {
            _error = $"Photo upload failed: {ex.Message}";
        }
        finally
        {
            _uploading = false;
        }
    }

    private async Task HandleSubmit()
    {
        _error = null;
        _model.Time = string.IsNullOrWhiteSpace(_model.Time) ? null : _model.Time;
        _model.DressCode = string.IsNullOrWhiteSpace(_model.DressCode) ? null : _model.DressCode;
        _model.TicketUrl = string.IsNullOrWhiteSpace(_model.TicketUrl) ? null : _model.TicketUrl;

        try
        {
            if (Id.HasValue)
            {
                await Supabase.UpdateEventAsync(_model);
            }
            else
            {
                await Supabase.AddEventAsync(_model);
            }

            Navigation.NavigateTo("/admin");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }
}
