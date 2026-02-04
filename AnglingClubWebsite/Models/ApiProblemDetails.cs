using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnglingClubWebsite.Models
{

    public sealed class ApiProblemDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }

        // This will bind your current payload directly
        public string? TraceId { get; set; }

        // Captures any other extension members the server might add later
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

}
