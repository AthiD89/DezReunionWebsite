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

    public string Title { get; set; } = "";
    public string EventName { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? VideoUrl { get; set; }
    public string? TikTokVideoId { get; set; }
    public GalleryMediaType Type { get; set; } = GalleryMediaType.Photo;
}
