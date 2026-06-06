namespace MathGame.ViewModels;

public class PuzzlePieceViewModel : ViewModelBase
{
    private bool _isRevealed;
    public bool IsRevealed
    {
        get => _isRevealed;
        set => Set(ref _isRevealed, value);
    }
}
