namespace LighthouseRichPresence;

public enum GameVersion {
    LittleBigPlanet1 = 0,
    LittleBigPlanet2 = 1,
    LittleBigPlanet3 = 2,
    LittleBigPlanetVita = 3,
    LittleBigPlanetPSP = 4,
    Unknown = -1,
}

public static class GameVersionExtensions {
    public static string ToPrettyString(this GameVersion? gameVersion) {
        if(gameVersion == null) return "Unknown";

        return gameVersion.ToString()!.Replace("LittleBigPlanet", "LittleBigPlanet ");
    }
    
    public static string ToDiscordAsset(this GameVersion? gameVersion) {
        if(gameVersion == null) return "unknown";
        
        return gameVersion switch {
            GameVersion.LittleBigPlanet1 => "lbp1",
            GameVersion.LittleBigPlanet2 => "lbp2",
            GameVersion.LittleBigPlanet3 => "lbp3",
            GameVersion.LittleBigPlanetVita => "lbpvita",
            GameVersion.LittleBigPlanetPSP => "lbppsp",
            GameVersion.Unknown => "unknown",
            _ => "unknown",
        };
    }
}