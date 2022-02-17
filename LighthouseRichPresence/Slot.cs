using System.Text.Json.Serialization;

namespace LighthouseRichPresence; 

public class Slot {
    [JsonPropertyName("slotId")]
    public int SlotId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}