using System.Text.Json;
using DiscordRPC;
using DiscordRPC.Logging;

namespace LBPUnion.LighthouseRichPresence; 

public static class Program {
    public static readonly DiscordRpcClient DiscordClient = new("943720816896524319");
    public static HttpClient HttpClient;

    public static readonly Dictionary<int, Slot?> SlotCache = new();

    private static bool hadOnlineStatus = false;
    
    

    public static async Task Main(string[] args) {
        if(args.Length != 2) {
            Console.WriteLine("Usage: LighthouseRichPresence <Server URL> <User ID>");
            return;
        }
        
        string url = args[0];
        int userId = int.Parse(args[1]);
        
        HttpClient = new HttpClient {
            BaseAddress = new Uri(url + "/api/v1/"),
        };
        
        DiscordClient.Initialize();

        //Set the logger
        DiscordClient.Logger = new ConsoleLogger { Level = LogLevel.Warning };

        //Subscribe to events
        DiscordClient.OnReady += (sender, e) => {
            Console.WriteLine("Received Ready from user {0}", e.User.Username);
        };

        DiscordClient.OnPresenceUpdate += (sender, e) => {
            Console.WriteLine("Received Update! {0}", e.Presence);
        };

        while(true) {
            await UpdatePresence(userId);
            await Task.Delay(5000);
        }
    }

    public static async Task<Slot?> GetSlot(int slotId) {
        Slot? slot;
        
        if(SlotCache.TryGetValue(slotId, out slot) && slot != null) {
            return slot;
        }
        
        Console.WriteLine("Fetching slot" + slotId);

        string slotJson = await HttpClient.GetStringAsync("slot/" + slotId);
        slot = (Slot?)JsonSerializer.Deserialize(slotJson, typeof(Slot));
        if(slot == null) return null;
        
        SlotCache.Add(slotId, slot);
        return slot;
    }

    public static async Task UpdatePresence(int userId) {
        Console.WriteLine("Fetching status for user " + userId);
        
        string statusJson = await HttpClient.GetStringAsync($"user/{userId}/status");

        UserStatus? userStatus = (UserStatus?)JsonSerializer.Deserialize(statusJson, typeof(UserStatus));
        if(userStatus == null) return;

        if(userStatus.StatusType == StatusType.Offline) {
            if(hadOnlineStatus) {
                Console.WriteLine("You have signed off, the program is now exiting.");
                Environment.Exit(0);
            }
            else {
                Console.WriteLine("Awaiting LBP client...");
                await Task.Delay(15000);
                return;
            };
        }
        else {
            hadOnlineStatus = true;
        }
        
        string slotName = "";

        if(userStatus.CurrentRoom?.Slot.SlotType == SlotType.User) {
            Slot? slot = await GetSlot(userStatus.CurrentRoom?.Slot.SlotId ?? 0);
            if(slot != null) slotName = slot.Name;
        }

        string state = userStatus.CurrentRoom?.Slot.SlotType switch {
            SlotType.Developer => "Playing a story level",
            SlotType.User => $"Playing {slotName}",
            SlotType.Moon => "Playing/Creating a level",
            SlotType.Unknown => "Playing",
            SlotType.Unknown2 => "Playing",
            SlotType.Pod => "Sitting in the pod",
            SlotType.DLC => "Playing a DLC level",
            null => "Playing",
            _ => "Playing",
        };

        string details = $"{userStatus.CurrentVersion.ToPrettyString()} on {userStatus.CurrentPlatform}";

        DiscordClient.SetPresence(new RichPresence {
            Details = details,
            State = state,
            Assets = new Assets {
                LargeImageKey = userStatus.CurrentVersion.ToDiscordAsset(),
                LargeImageText = details,
                SmallImageKey = "lighthouse",
                SmallImageText = "Playing on Project Lighthouse",
            },
            Party = new Party {
                Max = 4,
                Size = userStatus.CurrentRoom?.PlayerCount ?? 1,
                ID = "room:" + userStatus.CurrentRoom?.RoomId,
            },
        });
    }
}