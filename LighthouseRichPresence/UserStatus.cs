using System.Text.Json.Serialization;

namespace LBPUnion.LighthouseRichPresence; 

[Serializable]
public class UserStatus {
    [JsonPropertyName("statusType")]
    public StatusType StatusType { get; set; }

    [JsonPropertyName("currentVersion")]
    public GameVersion? CurrentVersion { get; set; }

    [JsonPropertyName("currentPlatform")]
    public Platform? CurrentPlatform { get; set; }

    [JsonPropertyName("currentRoom")]
    public Room? CurrentRoom { get; set; }
}