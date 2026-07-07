using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DezReunionWebsite.Models;

public enum GalleryMediaType
{
    Photo,
    Video
}

public class GalleryItem
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please add a title.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Please select an event.")]
    public string EventName { get; set; } = "";

    public string ImageUrl { get; set; } = "";
    public string? VideoUrl { get; set; }
    public string? TikTokVideoId { get; set; }
    public GalleryMediaType Type { get; set; } = GalleryMediaType.Photo;
}
