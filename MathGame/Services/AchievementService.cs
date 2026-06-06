using MathGame.Models;
using System.IO;
using System.Text.Json;

namespace MathGame.Services;

public class AchievementService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MathGame");
    private static readonly string DataFile = Path.Combine(DataDir, "achievements.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private Dictionary<string, int> _wins = new();

    public AchievementService() => Load();

    private void Load()
    {
        if (!File.Exists(DataFile)) return;
        try
        {
            var json = File.ReadAllText(DataFile);
            _wins = JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOptions) ?? new();
        }
        catch { }
    }

    private void Persist()
    {
        Directory.CreateDirectory(DataDir);
        File.WriteAllText(DataFile, JsonSerializer.Serialize(_wins, JsonOptions));
    }

    private static string Key(Operation op, Difficulty diff) => $"{op}_{diff}";

    public int GetWins(Operation op, Difficulty diff) =>
        _wins.TryGetValue(Key(op, diff), out int v) ? v : 0;

    public int RecordWin(Operation op, Difficulty diff)
    {
        var key = Key(op, diff);
        _wins[key] = GetWins(op, diff) + 1;
        Persist();
        return _wins[key];
    }

    public static int TotalPieces(Difficulty diff) => diff switch
    {
        Difficulty.Easy   => 20,
        Difficulty.Medium => 40,
        Difficulty.Hard   => 60,
        _                 => 20
    };

    public int RevealedPieces(Operation op, Difficulty diff) =>
        Math.Min(GetWins(op, diff), TotalPieces(diff));

    public bool IsCompleted(Operation op, Difficulty diff) =>
        RevealedPieces(op, diff) >= TotalPieces(diff);

    public void CheatAll()
    {
        foreach (var op in Enum.GetValues<Operation>())
            foreach (var diff in Enum.GetValues<Difficulty>())
                _wins[Key(op, diff)] = TotalPieces(diff);
        Persist();
    }

    public void ResetAll()
    {
        _wins.Clear();
        Persist();
    }
}
