using MathGame.Models;
using MathGame.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;

namespace MathGame.ViewModels;

// One column in the grid (= one block, questions vertically)
public class BlockColumnViewModel : ViewModelBase
{
    private readonly Block _block;

    public BlockColumnViewModel(Block block)
    {
        _block = block;
        Questions = new ObservableCollection<QuestionViewModel>(
            block.Questions.Select(q => new QuestionViewModel(q)));

        foreach (var q in block.Questions)
            q.PropertyChanged += (_, _) => OnPropertyChanged(nameof(IsCompleted));
    }

    public string Header => _block.Number.ToString();
    public ObservableCollection<QuestionViewModel> Questions { get; }
    public bool IsCompleted   => _block.IsCompleted;
    public bool IsAllAnswered => _block.IsAllAnswered;
}

// Main game screen: all questions visible across all columns, one highlighted at a time
public class GameViewModel : ViewModelBase
{
    private readonly Random             _rng = new();
    private readonly int                _totalQuestions;
    private readonly DispatcherTimer    _timer;
    private readonly Stopwatch          _stopwatch = new();
    private readonly Operation          _op;
    private readonly Difficulty         _difficulty;
    private readonly LeaderboardService? _leaderboardService;
    private readonly AchievementService? _achievementService;
    private double                      _finalTimeSeconds;

    public ObservableCollection<BlockColumnViewModel> Columns { get; }
    public string OperationLabel { get; }

    private string _playerInput = "";
    public string PlayerInput
    {
        get => _playerInput;
        set => Set(ref _playerInput, value);
    }

    private string _feedbackMessage = "";
    public string FeedbackMessage
    {
        get => _feedbackMessage;
        set => Set(ref _feedbackMessage, value);
    }

    private int _correctCount;
    public int CorrectCount
    {
        get => _correctCount;
        private set
        {
            if (Set(ref _correctCount, value))
                OnPropertyChanged(nameof(UnicornX));
        }
    }

    private string _elapsedTime = "0:00";
    public string ElapsedTime
    {
        get => _elapsedTime;
        private set => Set(ref _elapsedTime, value);
    }

    private bool _isGameWon;
    public bool IsGameWon
    {
        get => _isGameWon;
        private set => Set(ref _isGameWon, value);
    }

    private bool _isGameFailed;
    public bool IsGameFailed
    {
        get => _isGameFailed;
        private set => Set(ref _isGameFailed, value);
    }

    private string _finalTime = "0:00";
    public string FinalTime
    {
        get => _finalTime;
        private set => Set(ref _finalTime, value);
    }

    private string _scoreText = "";
    public string ScoreText
    {
        get => _scoreText;
        private set => Set(ref _scoreText, value);
    }

    // ── Name entry / leaderboard ──────────────────────────────────────────
    private string _playerName = "";
    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (Set(ref _playerName, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool _isScoreSaved;
    public bool IsScoreSaved
    {
        get => _isScoreSaved;
        private set => Set(ref _isScoreSaved, value);
    }

    private string _savedScoreMessage = "";
    public string SavedScoreMessage
    {
        get => _savedScoreMessage;
        private set => Set(ref _savedScoreMessage, value);
    }

    // Canvas X in [0..1000] space: unicorn travels from house (44) to castle (944)
    public double UnicornX => _totalQuestions == 0
        ? 44
        : _correctCount * (900.0 / _totalQuestions) + 44;

    public ICommand SubmitCommand    { get; }
    public ICommand BackCommand      { get; }
    public ICommand PlayAgainCommand { get; }
    public ICommand SaveScoreCommand { get; }

    public event Action? BackRequested;
    public event Action? PlayAgainRequested;

    public GameViewModel(List<Block> blocks, Operation op, Difficulty difficulty,
        LeaderboardService? leaderboardService = null,
        AchievementService? achievementService = null)
    {
        _op                 = op;
        _difficulty         = difficulty;
        _leaderboardService = leaderboardService;
        _achievementService = achievementService;

        var opLabel = op switch
        {
            Operation.Addition       => "Addition  +",
            Operation.Subtraction    => "Subtraction  −",
            Operation.Multiplication => "Multiplication  ×",
            Operation.Division       => "Division  ÷",
            _                        => ""
        };
        var diffLabel = difficulty switch
        {
            Difficulty.Easy   => "Easy",
            Difficulty.Medium => "Medium",
            Difficulty.Hard   => "Hard",
            _                 => ""
        };
        OperationLabel = $"{opLabel}   ·   {diffLabel}";

        Columns = new ObservableCollection<BlockColumnViewModel>(
            blocks.Select(b => new BlockColumnViewModel(b)));

        _totalQuestions = ActiveQuestions().Count();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
        {
            var e = _stopwatch.Elapsed;
            ElapsedTime = $"{(int)e.TotalMinutes}:{e.Seconds:D2}";
        };
        _stopwatch.Start();
        _timer.Start();

        SubmitCommand    = new RelayCommand(Submit, () => PlayerInput.Trim().Length > 0);
        BackCommand      = new RelayCommand(OnBack);
        PlayAgainCommand = new RelayCommand(() => PlayAgainRequested?.Invoke());
        SaveScoreCommand = new RelayCommand(SaveScore, () => PlayerName.Trim().Length > 0 && !IsScoreSaved);

        ActivateNextQuestion();
    }

    private void OnBack()
    {
        _stopwatch.Stop();
        _timer.Stop();
        BackRequested?.Invoke();
    }

    private void Submit()
    {
        if (!int.TryParse(PlayerInput.Trim(), out int answer))
        {
            FeedbackMessage = "Please enter a whole number.";
            return;
        }

        var active = ActiveQuestions().FirstOrDefault(q => q.Model.State == QuestionState.Active);
        if (active is null) return;

        if (answer == active.Model.Answer)
        {
            active.Model.State = QuestionState.Correct;
            FeedbackMessage    = "✓  Correct!";
            CorrectCount++;
        }
        else
        {
            active.Model.State = QuestionState.Wrong;
            FeedbackMessage    = "✗  Wrong — try the next one!";
        }

        PlayerInput = "";
        ActivateNextQuestion();

        if (Columns.All(c => c.IsAllAnswered))
        {
            _stopwatch.Stop();
            _timer.Stop();
            _finalTimeSeconds = _stopwatch.Elapsed.TotalSeconds;
            FinalTime  = ElapsedTime;
            bool allCorrect = ActiveQuestions().All(q => q.Model.State == QuestionState.Correct);
            ScoreText  = $"{CorrectCount} / {_totalQuestions} correct";
            if (allCorrect)
            {
                _achievementService?.RecordWin(_op, _difficulty);
                IsGameWon = true;
            }
            else
            {
                IsGameFailed = true;
            }
        }
    }

    private void SaveScore()
    {
        if (_leaderboardService == null) return;

        var entry = new LeaderboardEntry
        {
            PlayerName  = PlayerName.Trim(),
            TimeSeconds = _finalTimeSeconds,
            Timestamp   = DateTime.Now,
            Operation   = _op,
            Difficulty  = _difficulty
        };
        _leaderboardService.Save(entry);

        var top  = _leaderboardService.GetTopFor(_op, _difficulty, 100);
        int rank = top.FindIndex(e => Math.Abs(e.TimeSeconds - entry.TimeSeconds) < 1.0
                                      && e.PlayerName == entry.PlayerName) + 1;

        SavedScoreMessage = rank > 0
            ? $"Saved!  You are #{rank} in {_op} scores!"
            : "Score saved!";
        IsScoreSaved = true;
    }

    private void ActivateNextQuestion()
    {
        var unsolved = ActiveQuestions()
            .Where(q => q.Model.State == QuestionState.Unsolved)
            .ToList();

        if (unsolved.Count == 0) return;
        unsolved[_rng.Next(unsolved.Count)].Model.State = QuestionState.Active;
    }

    // Excludes Disabled placeholder cells from all game logic
    private IEnumerable<QuestionViewModel> ActiveQuestions()
        => Columns.SelectMany(c => c.Questions)
                  .Where(q => q.Model.State != QuestionState.Disabled);
}
