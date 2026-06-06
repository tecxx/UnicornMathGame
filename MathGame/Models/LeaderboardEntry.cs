using System.Text.Json.Serialization;

namespace MathGame.Models;

public class LeaderboardEntry
{
    public string PlayerName { get; set; } = "";
    public double TimeSeconds { get; set; }
    public DateTime Timestamp { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Operation Operation { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Difficulty Difficulty { get; set; }

    [JsonIgnore]
    public string TimeDisplay =>
        $"{(int)(TimeSeconds / 60)}:{(int)(TimeSeconds % 60):D2}";
}
