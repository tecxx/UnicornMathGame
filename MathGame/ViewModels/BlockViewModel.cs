using MathGame.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MathGame.ViewModels;

public class BlockViewModel : ViewModelBase
{
    private readonly Block _block;
    private readonly Random _rng = new();

    public ObservableCollection<QuestionViewModel> Questions { get; }
    public string Title => $"Block {_block.Number}  —  {OperationLabel(_block.Op)}";

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

    private bool _isCompleted;
    public bool IsCompleted
    {
        get => _isCompleted;
        set => Set(ref _isCompleted, value);
    }

    private bool _isAllAnswered;
    public bool IsAllAnswered
    {
        get => _isAllAnswered;
        set => Set(ref _isAllAnswered, value);
    }

    public string FinishedMessage =>
        $"Block done! {_block.CorrectCount}/10 correct";

    public ICommand SubmitCommand { get; }
    public ICommand BackCommand   { get; }

    public event Action? BackRequested;

    public BlockViewModel(Block block)
    {
        _block = block;
        Questions = new ObservableCollection<QuestionViewModel>(
            block.Questions.Select(q => new QuestionViewModel(q)));

        SubmitCommand = new RelayCommand(Submit, () => PlayerInput.Trim().Length > 0);
        BackCommand   = new RelayCommand(() => BackRequested?.Invoke());

        // Reset any stale Active state from a previous visit, then pick a question
        ResetActiveQuestion();
        ActivateNextQuestion();
    }

    private void Submit()
    {
        if (!int.TryParse(PlayerInput.Trim(), out int answer))
        {
            FeedbackMessage = "Please enter a whole number.";
            return;
        }

        var active = Questions.FirstOrDefault(q => q.Model.State == QuestionState.Active);
        if (active is null) return;

        if (answer == active.Model.Answer)
        {
            active.Model.State = QuestionState.Correct;
            FeedbackMessage = "✓  Correct!";
        }
        else
        {
            active.Model.State = QuestionState.Wrong;
            FeedbackMessage    = "✗  Wrong — try the next one!";
        }

        PlayerInput = "";

        if (_block.IsCompleted)
        {
            IsCompleted = true;
            FeedbackMessage = "";
            return;
        }

        if (_block.IsAllAnswered)
        {
            IsAllAnswered = true;
            FeedbackMessage = "";
            return;
        }

        ActivateNextQuestion();
    }

    private void ResetActiveQuestion()
    {
        foreach (var q in _block.Questions.Where(q => q.State == QuestionState.Active))
            q.State = QuestionState.Unsolved;
    }

    private void ActivateNextQuestion()
    {
        var unsolved = Questions
            .Where(q => q.Model.State == QuestionState.Unsolved)
            .ToList();

        if (unsolved.Count == 0) return;
        unsolved[_rng.Next(unsolved.Count)].Model.State = QuestionState.Active;
    }

    private static string OperationLabel(Operation op) => op switch
    {
        Operation.Addition       => "Addition (+)",
        Operation.Subtraction    => "Subtraction (−)",
        Operation.Multiplication => "Multiplication (×)",
        Operation.Division       => "Division (÷)",
        _                        => ""
    };
}
