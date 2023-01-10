using System.Text.Json.Serialization;

namespace LBPUnion.LighthouseRichPresence;

public class Slot
{
    [JsonPropertyName("slotId")]
    public int SlotId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}