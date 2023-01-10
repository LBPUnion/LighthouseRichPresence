using System.Text.Json.Serialization;

namespace LBPUnion.LighthouseRichPresence;

public class Room
{
    [JsonPropertyName("roomId")]
    public int RoomId { get; set; }

    [JsonPropertyName("slot")]
    public RoomSlot? Slot { get; set; }

    [JsonPropertyName("state")]
    public RoomState State { get; set; }

    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; }
}