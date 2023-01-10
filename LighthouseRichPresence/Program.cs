using System.Text.Json;
using DiscordRPC;
using DiscordRPC.Logging;

/*
/  Remind me to go through and bump to .NET 7
*/

namespace LBPUnion.LighthouseRichPresence;

public static class Program
{
    public static readonly DiscordRpcClient DiscordClient = new("1060973475151495288");
    public static HttpClient HttpClient = null!;

    public static readonly Dictionary<int, Slot?> SlotCache = new();

    private static bool hadOnlineStatus;

    public static string url = "";
    public static string instanceCommon = "";
    public static string instanceImage = "";

    public static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Invalid usage. This program requires a Server URL and a User ID.");
            Console.WriteLine(" - Example: ./LighthouseRichPresence https://example.server 594");
            return;
        }

        url = args[0];

        int userId = int.Parse(args[1]);

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(url + "/api/v1/"),
        };

        DiscordClient.Initialize();

        //Set the logger
        DiscordClient.Logger = new ConsoleLogger { Level = LogLevel.Warning };

        //Subscribe to events
        DiscordClient.OnReady += (_, e) =>
        {
            Console.WriteLine($"[LighthouseRichPresence:connect] Connected to Discord Client under the user {e.User.Username}.");
        };

        DiscordClient.OnPresenceUpdate += (_, e) =>
        {
            Console.WriteLine($"[LighthouseRichPresence:fetch] Received a new {e.Presence} update.");
        };

        while (true)
        {
            await UpdatePresence(userId);
            await Task.Delay(5000);
        }
    }

    public static async Task<Slot?> GetSlot(int slotId)
    {
        if (SlotCache.TryGetValue(slotId, out Slot? slot) && slot != null)
        {
            return slot;
        }

        Console.WriteLine("[LighthouseRichPresence:fetch] Fetching slot information for " + slotId + "...");

        string slotJson = await HttpClient.GetStringAsync("slot/" + slotId);
        slot = (Slot?)JsonSerializer.Deserialize(slotJson, typeof(Slot));
        if (slot == null) return null;

        SlotCache.Add(slotId, slot);
        return slot;
    }

    public static async Task UpdatePresence(int userId)
    {

        void Deinitialize()
        {
            DiscordClient.Deinitialize();  // safety
        }

        Console.WriteLine("[LighthouseRichPresence:fetch] Fetching status information for User ID " + userId + "...");

        string statusJson = await HttpClient.GetStringAsync($"user/{userId}/status");

        UserStatus? userStatus = (UserStatus?)JsonSerializer.Deserialize(statusJson, typeof(UserStatus));
        if (userStatus == null) return;

        if (userStatus.StatusType == StatusType.Offline)
        {
            if (hadOnlineStatus)
            {
                Console.WriteLine("[LighthouseRichPresence:auth] You have signed out of Lighthouse. The program will now exit.");
                Deinitialize();
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine($"[LighthouseRichPresence:connect] Awaiting a connection to Lighthouse at {url}...");
                await Task.Delay(15000);
                return;
            };
        }
        else
        {
            hadOnlineStatus = true;
        }

        string slotName = "";

        if (userStatus.CurrentRoom?.Slot.SlotType == SlotType.User)
        {
            Slot? slot = await GetSlot(userStatus.CurrentRoom?.Slot.SlotId ?? 0);
            if (slot != null) slotName = slot.Name;
        }

        string state = userStatus.CurrentRoom?.Slot.SlotType switch
        {
            SlotType.Developer => "Playing a story level",
            SlotType.User => $"Playing {slotName}",
            SlotType.Moon => "Creating something awesome",
            SlotType.Unknown => "Chillin'",
            SlotType.Unknown2 => "Vibin'",
            SlotType.Pod => "Dwelling in the Pod",
            SlotType.DLC => "Playing a DLC level",
            null => "Doing something cool",
            _ => "What's this?",
        };

        string details = $"{userStatus.CurrentVersion.ToPrettyString()} on {userStatus.CurrentPlatform}";

        if (url.Contains("lbpunion.com"))
        {
            instanceCommon = "LBP Union's Beacon";
            instanceImage = "beacon";
        }
        else
        {
            instanceCommon = "a Private Instance";
            instanceImage = "private";
        }

        DiscordClient.SetPresence(new RichPresence
        {
            Details = details,
            State = state,
            Assets = new Assets
            {
                LargeImageKey = userStatus.CurrentVersion.ToDiscordAsset(),
                LargeImageText = details,
                SmallImageKey = instanceImage,
                SmallImageText = $"Playing on {instanceCommon}",
            },
            Party = new Party
            {
                Max = 4,
                Size = userStatus.CurrentRoom?.PlayerCount ?? 1,
                ID = "room:" + userStatus.CurrentRoom?.RoomId,
            },
        });
    }
}