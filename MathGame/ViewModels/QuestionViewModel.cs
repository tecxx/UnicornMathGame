using MathGame.Models;
using System.Windows.Media;

namespace MathGame.ViewModels;

public class QuestionViewModel : ViewModelBase
{
    private static readonly SolidColorBrush BrushActive    = new(Color.FromRgb(74,  144, 217));
    private static readonly SolidColorBrush BrushCorrect   = new(Color.FromRgb(92,  184, 92));
    private static readonly SolidColorBrush BrushWrong     = new(Color.FromRgb(217, 83,  79));
    private static readonly SolidColorBrush BrushUnsolved  = new(Color.FromRgb(240, 240, 248));
    private static readonly SolidColorBrush BrushDisabled  = new(Color.FromRgb(220, 220, 228));

    private readonly Question _model;

    public QuestionViewModel(Question model)
    {
        _model = model;
        _model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Question.State))
            {
                OnPropertyChanged(nameof(Background));
                OnPropertyChanged(nameof(Foreground));
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(IsSolved));
            }
        };
    }

    public Question Model => _model;

    // Compact tile: show question text + state symbol; answer never revealed on tile
    public string DisplayText => _model.State switch
    {
        QuestionState.Disabled => "",
        QuestionState.Correct  => $"{_model.QuestionText} ✓",
        QuestionState.Wrong    => $"{_model.QuestionText} ✗",
        _                      => $"{_model.QuestionText} ="
    };

    public Brush Background => _model.State switch
    {
        QuestionState.Active   => BrushActive,
        QuestionState.Correct  => BrushCorrect,
        QuestionState.Wrong    => BrushWrong,
        QuestionState.Disabled => BrushDisabled,
        _                      => BrushUnsolved
    };

    public Brush Foreground => _model.State switch
    {
        QuestionState.Unsolved  => Brushes.DimGray,
        QuestionState.Disabled  => BrushDisabled,   // text same as bg → invisible
        _                       => Brushes.White
    };

    public bool IsSolved => _model.State is QuestionState.Correct or QuestionState.Wrong;
}
