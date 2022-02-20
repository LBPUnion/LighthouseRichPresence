using System.Text.Json.Serialization;

namespace LBPUnion.LighthouseRichPresence; 

public class RoomSlot {
    [JsonPropertyName("slotId")]
    public int SlotId { get; set; }

    [JsonPropertyName("slotType")]
    public SlotType SlotType { get; set; }
}