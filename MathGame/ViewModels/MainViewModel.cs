using MathGame.Models;
using MathGame.Services;

namespace MathGame.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly LeaderboardService _leaderboard = new();
    private readonly AchievementService _achievement = new();

    private object? _currentView;
    public object? CurrentView
    {
        get => _currentView;
        set => Set(ref _currentView, value);
    }

    public MainViewModel() => ShowOperationSelect();

    // returnDifficulty: when coming back from a finished game, skip phase 1 and
    // land directly on the puzzle overview for the same difficulty.
    private void ShowOperationSelect(Difficulty? returnDifficulty = null)
    {
        var vm = new OperationSelectViewModel(_leaderboard, _achievement, returnDifficulty);
        vm.OperationSelected += ShowGame;
        CurrentView = vm;
    }

    private void ShowGame(Operation op, Difficulty difficulty)
    {
        var blocks = QuestionGenerator.CreateAllBlocks(op, difficulty);
        var vm     = new GameViewModel(blocks, op, difficulty, _leaderboard, _achievement);
        vm.BackRequested      += () => ShowOperationSelect(difficulty);
        vm.PlayAgainRequested += () => ShowGame(op, difficulty);
        CurrentView = vm;
    }
}
