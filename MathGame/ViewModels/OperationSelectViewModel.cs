using MathGame.Models;
using MathGame.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MathGame.ViewModels;

public class LeaderboardRowViewModel
{
    public string Rank    { get; init; } = "";
    public string Name    { get; init; } = "";
    public string Time    { get; init; } = "";
    public string Diff    { get; init; } = "";
}

public class OperationSelectViewModel : ViewModelBase
{
    private readonly LeaderboardService  _leaderboard;
    private readonly AchievementService  _achievement;

    // Fires with (operation, difficulty) when the player presses an operation button
    public event Action<Operation, Difficulty>? OperationSelected;

    // ── Difficulty ────────────────────────────────────────────────────────
    private Difficulty _selectedDifficulty = Difficulty.Easy;
    public Difficulty SelectedDifficulty
    {
        get => _selectedDifficulty;
        private set
        {
            if (Set(ref _selectedDifficulty, value))
            {
                OnPropertyChanged(nameof(IsEasySelected));
                OnPropertyChanged(nameof(IsMediumSelected));
                OnPropertyChanged(nameof(IsHardSelected));
            }
        }
    }

    public bool IsEasySelected   => SelectedDifficulty == Difficulty.Easy;
    public bool IsMediumSelected => SelectedDifficulty == Difficulty.Medium;
    public bool IsHardSelected   => SelectedDifficulty == Difficulty.Hard;

    // Set the backing field directly in the constructor when skipping phase 1,
    // so no PropertyChanged fires before the view is attached.
    private bool _difficultyConfirmed;
    public bool DifficultyConfirmed
    {
        get => _difficultyConfirmed;
        private set => Set(ref _difficultyConfirmed, value);
    }

    public ICommand SelectEasyCommand   { get; }
    public ICommand SelectMediumCommand { get; }
    public ICommand SelectHardCommand   { get; }

    // ── Operations ────────────────────────────────────────────────────────
    public ICommand SelectAdditionCommand       { get; }
    public ICommand SelectSubtractionCommand    { get; }
    public ICommand SelectMultiplicationCommand { get; }
    public ICommand SelectDivisionCommand       { get; }
    public ICommand ResetAllCommand             { get; }

    // ── Animal image paths (depend on selected difficulty) ───────────────
    private string _horseImage   = "";
    private string _dolphinImage = "";
    private string _tigerImage   = "";
    private string _dragonImage  = "";
    public string HorseImage   { get => _horseImage;   private set => Set(ref _horseImage,   value); }
    public string DolphinImage { get => _dolphinImage; private set => Set(ref _dolphinImage, value); }
    public string TigerImage   { get => _tigerImage;   private set => Set(ref _tigerImage,   value); }
    public string DragonImage  { get => _dragonImage;  private set => Set(ref _dragonImage,  value); }

    private void SetImagePaths(Difficulty diff)
    {
        int n = diff switch { Difficulty.Easy => 1, Difficulty.Medium => 2, Difficulty.Hard => 3, _ => 1 };
        HorseImage   = $"/Assets/horse{n}.jpg";
        DolphinImage = $"/Assets/dolphin{n}.jpg";
        TigerImage   = $"/Assets/tiger{n}.jpg";
        DragonImage  = $"/Assets/dragon{n}.jpg";
    }

    // ── Puzzle pieces per animal ──────────────────────────────────────────
    public ObservableCollection<PuzzlePieceViewModel> HorsePieces    { get; } = new();
    public ObservableCollection<PuzzlePieceViewModel> DolphinPieces  { get; } = new();
    public ObservableCollection<PuzzlePieceViewModel> TigerPieces    { get; } = new();
    public ObservableCollection<PuzzlePieceViewModel> DragonPieces   { get; } = new();

    private string _horseProgress   = "";
    private string _dolphinProgress = "";
    private string _tigerProgress   = "";
    private string _dragonProgress  = "";
    public string HorseProgress   { get => _horseProgress;   private set => Set(ref _horseProgress,   value); }
    public string DolphinProgress { get => _dolphinProgress; private set => Set(ref _dolphinProgress, value); }
    public string TigerProgress   { get => _tigerProgress;   private set => Set(ref _tigerProgress,   value); }
    public string DragonProgress  { get => _dragonProgress;  private set => Set(ref _dragonProgress,  value); }

    // ── Completion flags (all pieces revealed for current difficulty) ──────
    private bool _isHorseComplete;
    private bool _isDolphinComplete;
    private bool _isTigerComplete;
    private bool _isDragonComplete;
    public bool IsHorseComplete   { get => _isHorseComplete;   private set => Set(ref _isHorseComplete,   value); }
    public bool IsDolphinComplete { get => _isDolphinComplete; private set => Set(ref _isDolphinComplete, value); }
    public bool IsTigerComplete   { get => _isTigerComplete;   private set => Set(ref _isTigerComplete,   value); }
    public bool IsDragonComplete  { get => _isDragonComplete;  private set => Set(ref _isDragonComplete,  value); }

    // ── Leaderboard data ──────────────────────────────────────────────────
    public ObservableCollection<LeaderboardRowViewModel> AdditionBoard    { get; } = new();
    public ObservableCollection<LeaderboardRowViewModel> SubtractionBoard { get; } = new();
    public ObservableCollection<LeaderboardRowViewModel> MultiplyBoard    { get; } = new();
    public ObservableCollection<LeaderboardRowViewModel> DivisionBoard    { get; } = new();

    // startConfirmed: pass the current difficulty when returning from a game
    // to skip phase 1 and land directly on the puzzle overview.
    public OperationSelectViewModel(LeaderboardService leaderboard, AchievementService achievement,
        Difficulty? startConfirmed = null)
    {
        _leaderboard = leaderboard;
        _achievement = achievement;

        SelectEasyCommand   = new RelayCommand(() => ConfirmDifficulty(Difficulty.Easy));
        SelectMediumCommand = new RelayCommand(() => ConfirmDifficulty(Difficulty.Medium));
        SelectHardCommand   = new RelayCommand(() => ConfirmDifficulty(Difficulty.Hard));

        SelectAdditionCommand       = new RelayCommand(() => Fire(Operation.Addition));
        SelectSubtractionCommand    = new RelayCommand(() => Fire(Operation.Subtraction));
        SelectMultiplicationCommand = new RelayCommand(() => Fire(Operation.Multiplication));
        SelectDivisionCommand       = new RelayCommand(() => Fire(Operation.Division));
        ResetAllCommand             = new RelayCommand(DoResetAll, () => DifficultyConfirmed);

        if (startConfirmed.HasValue)
        {
            _selectedDifficulty    = startConfirmed.Value; // set backing field — no event
            _difficultyConfirmed   = true;                 // view code-behind reads this on attach
            SetImagePaths(startConfirmed.Value);
            LoadPuzzles();
            LoadLeaderboard();
        }
    }

    private void ConfirmDifficulty(Difficulty diff)
    {
        SelectedDifficulty = diff;
        SetImagePaths(diff);
        LoadPuzzles();
        LoadLeaderboard();
        DifficultyConfirmed = true; // triggers animation in code-behind
    }

    private void LoadPuzzles()
    {
        BuildPieces(HorsePieces,   Operation.Addition,       SelectedDifficulty, out string hp);
        BuildPieces(DolphinPieces, Operation.Subtraction,    SelectedDifficulty, out string dp);
        BuildPieces(TigerPieces,   Operation.Multiplication, SelectedDifficulty, out string tp);
        BuildPieces(DragonPieces,  Operation.Division,       SelectedDifficulty, out string drp);
        HorseProgress   = hp;
        DolphinProgress = dp;
        TigerProgress   = tp;
        DragonProgress  = drp;
        IsHorseComplete   = _achievement.IsCompleted(Operation.Addition,       SelectedDifficulty);
        IsDolphinComplete = _achievement.IsCompleted(Operation.Subtraction,    SelectedDifficulty);
        IsTigerComplete   = _achievement.IsCompleted(Operation.Multiplication, SelectedDifficulty);
        IsDragonComplete  = _achievement.IsCompleted(Operation.Division,       SelectedDifficulty);
    }

    private void BuildPieces(ObservableCollection<PuzzlePieceViewModel> col,
                              Operation op, Difficulty diff, out string progress)
    {
        col.Clear();
        int total    = AchievementService.TotalPieces(diff);
        int revealed = _achievement.RevealedPieces(op, diff);

        // Deterministic random unlock order: same spread every run for this (op, diff).
        int seed = ((int)op + 1) * 37 + ((int)diff + 1) * 13;
        var rng   = new Random(seed);
        var order = Enumerable.Range(0, total).OrderBy(_ => rng.Next()).ToArray();
        var revealedSet = new HashSet<int>(order.Take(revealed));

        for (int i = 0; i < total; i++)
            col.Add(new PuzzlePieceViewModel { IsRevealed = revealedSet.Contains(i) });

        progress = $"{revealed} / {total}";
    }

    private void LoadLeaderboard()
    {
        PopulateBoard(AdditionBoard,    _leaderboard.GetTopFor(Operation.Addition,       SelectedDifficulty, 5));
        PopulateBoard(SubtractionBoard, _leaderboard.GetTopFor(Operation.Subtraction,    SelectedDifficulty, 5));
        PopulateBoard(MultiplyBoard,    _leaderboard.GetTopFor(Operation.Multiplication, SelectedDifficulty, 5));
        PopulateBoard(DivisionBoard,    _leaderboard.GetTopFor(Operation.Division,       SelectedDifficulty, 5));
    }

    private static void PopulateBoard(ObservableCollection<LeaderboardRowViewModel> col,
                                      List<LeaderboardEntry> entries)
    {
        col.Clear();
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            col.Add(new LeaderboardRowViewModel
            {
                Rank = $"{i + 1}.",
                Name = e.PlayerName,
                Time = e.TimeDisplay,
                Diff = e.Difficulty switch
                {
                    Difficulty.Easy   => "Easy",
                    Difficulty.Medium => "Med",
                    Difficulty.Hard   => "Hard",
                    _                 => ""
                }
            });
        }
        if (col.Count == 0)
            col.Add(new LeaderboardRowViewModel { Rank = "—", Name = "No records yet", Time = "", Diff = "" });
    }

    private void DoCheatAll()
    {
        _achievement.CheatAll();
        LoadPuzzles();
    }

    private void DoResetAll()
    {
        var result = MessageBox.Show(
            "Reset all puzzle progress? This cannot be undone.",
            "Reset Progress",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        _achievement.ResetAll();
        LoadPuzzles();
    }

    private void Fire(Operation op) => OperationSelected?.Invoke(op, SelectedDifficulty);
}
