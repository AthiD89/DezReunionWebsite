using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using DezReunionWebsite.Models;
using DezReunionWebsite.Services;
using Microsoft.JSInterop;

namespace DezReunionWebsite.Components.Pages.Admin;

public partial class GalleryForm
{
    [Parameter]
    public int? Id { get; set; }

    private GalleryItem _model = new();
    private GalleryMediaType _mediaType = GalleryMediaType.Photo;
    private string _videoUrlInput = "";
    private string? _uploadedVideoUrl;
    private List<EventItem> _events = new();
    private bool _uploading;
    private bool _notFound;
    private string? _error;
    private readonly string _videoPlaceholder = "https://www.tiktok.com/@dez.reunion/video/... or a YouTube link";

    private static readonly Regex GuidPattern = new(
        @"[0-9a-f]{8}[-_]?[0-9a-f]{4}[-_]?[0-9a-f]{4}[-_]?[0-9a-f]{4}[-_]?[0-9a-f]{12}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex GenericCameraNamePattern = new(
        @"^(img|image|photo|pic|picture|video|vid|clip|movie|temp|tmp)[\s_-]*\d*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            _events = await Supabase.GetAllEventsAsync();

            if (Id.HasValue)
            {
                var existing = await Supabase.GetGalleryItemByIdAsync(Id.Value);
                if (existing is null)
                {
                    _notFound = true;
                    return;
                }

                _model = existing;
                _mediaType = existing.Type;

                if (_mediaType == GalleryMediaType.Video)
                {
                    var isUploadedFile = existing.VideoUrl?.Contains("/storage/v1/object/public/gallary/", StringComparison.OrdinalIgnoreCase) ?? false;
                    if (isUploadedFile)
                    {
                        _uploadedVideoUrl = existing.VideoUrl;
                    }
                    else
                    {
                        _videoUrlInput = existing.VideoUrl ?? "";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }

    private async Task OpenLinkToResolve()
    {
        if (string.IsNullOrWhiteSpace(_videoUrlInput))
        {
            _error = "Paste a video link first.";
            return;
        }

        await JS.InvokeVoidAsync("open", _videoUrlInput.Trim(), "_blank");
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
            if (string.IsNullOrWhiteSpace(_model.Title))
            {
                _model.Title = FileNameToTitle(file.Name, "Photo");
            }
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

    private async Task HandleVideoFileSelected(InputFileChangeEventArgs e)
    {
        _error = null;
        _uploading = true;
        try
        {
            var file = e.File;
            await using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            _uploadedVideoUrl = await Supabase.UploadFileAsync(stream, file.Name, file.ContentType);
            if (string.IsNullOrWhiteSpace(_model.Title))
            {
                _model.Title = FileNameToTitle(file.Name, "Video");
            }
        }
        catch (Exception ex)
        {
            _error = $"Video upload failed: {ex.Message}";
        }
        finally
        {
            _uploading = false;
        }
    }

    private static string FileNameToTitle(string fileName, string fallback)
    {
        var nameOnly = System.IO.Path.GetFileNameWithoutExtension(fileName);

        var hasSeparators = nameOnly.Contains('_') || nameOnly.Contains('-') || nameOnly.Contains(' ');
        var looksLikeHash = !hasSeparators
            && nameOnly.Length >= 24
            && Regex.IsMatch(nameOnly, @"^[a-zA-Z0-9]+$")
            && nameOnly.Count(char.IsDigit) >= 4;

        if (GuidPattern.IsMatch(nameOnly) || looksLikeHash)
        {
            return fallback;
        }

        var spaced = nameOnly.Replace('_', ' ').Replace('-', ' ').Trim();

        if (string.IsNullOrWhiteSpace(spaced) || GenericCameraNamePattern.IsMatch(spaced))
        {
            return fallback;
        }

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(spaced.ToLowerInvariant());
    }

    private async Task HandleSubmit()
    {
        _error = null;
        _model.Type = _mediaType;

        try
        {
            if (_mediaType == GalleryMediaType.Video)
            {
                if (!string.IsNullOrWhiteSpace(_uploadedVideoUrl))
                {
                    _model.VideoUrl = _uploadedVideoUrl;
                    _model.TikTokVideoId = null;
                }
                else
                {
                    var parsed = SupabaseClient.ParseVideoLink(_videoUrlInput);
                    if (parsed is null)
                    {
                        _error = "Please upload a video file, or paste a valid video link.";
                        return;
                    }

                    _model.VideoUrl = parsed.EmbedUrl;
                    _model.TikTokVideoId = parsed.TikTokVideoId;
                }
            }
            else if (string.IsNullOrWhiteSpace(_model.ImageUrl))
            {
                _error = "Please upload a photo.";
                return;
            }

            if (Id.HasValue)
            {
                await Supabase.UpdateGalleryItemAsync(_model);
            }
            else
            {
                await Supabase.AddGalleryItemAsync(_model);
            }

            Navigation.NavigateTo("/admin");
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
    }
}
