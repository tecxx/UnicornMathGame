using MathGame.Models;
using System.IO;
using System.Text.Json;

namespace MathGame.Services;

public class LeaderboardService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MathGame");
    private static readonly string DataFile = Path.Combine(DataDir, "leaderboard.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public List<LeaderboardEntry> LoadAll()
    {
        if (!File.Exists(DataFile)) return new();
        try
        {
            var json = File.ReadAllText(DataFile);
            return JsonSerializer.Deserialize<List<LeaderboardEntry>>(json, JsonOptions) ?? new();
        }
        catch { return new(); }
    }

    public void Save(LeaderboardEntry entry)
    {
        var entries = LoadAll();
        entries.Add(entry);
        Directory.CreateDirectory(DataDir);
        File.WriteAllText(DataFile, JsonSerializer.Serialize(entries, JsonOptions));
    }

    public List<LeaderboardEntry> GetTopFor(Operation op, Difficulty diff, int count = 5)
        => LoadAll()
            .Where(e => e.Operation == op && e.Difficulty == diff)
            .OrderBy(e => e.TimeSeconds)
            .Take(count)
            .ToList();
}
