using MathGame.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MathGame.ViewModels;

public class BlockSelectViewModel : ViewModelBase
{
    public event Action<Block>? BlockSelected;
    public event Action? BackRequested;

    public ObservableCollection<BlockItemViewModel> Blocks { get; }
    public string OperationLabel { get; }
    public ICommand BackCommand { get; }

    public BlockSelectViewModel(List<Block> blocks, Operation op)
    {
        OperationLabel = op switch
        {
            Operation.Addition       => "Addition  +",
            Operation.Subtraction    => "Subtraction  −",
            Operation.Multiplication => "Multiplication  ×",
            Operation.Division       => "Division  ÷",
            _                        => ""
        };

        Blocks = new ObservableCollection<BlockItemViewModel>(
            blocks.Select(b => new BlockItemViewModel(b, () => BlockSelected?.Invoke(b))));

        BackCommand = new RelayCommand(() => BackRequested?.Invoke());
    }
}

public class BlockItemViewModel : ViewModelBase
{
    private readonly Block _block;

    public BlockItemViewModel(Block block, Action onSelected)
    {
        _block = block;
        SelectCommand = new RelayCommand(onSelected);

        foreach (var q in block.Questions)
            q.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(StatusEmoji));
            };
    }

    public int    Number      => _block.Number;
    public string Label       => $"Block {_block.Number}";
    public string Progress    => $"{_block.SolvedCount} / 10";
    public bool   IsCompleted => _block.IsCompleted;
    public string StatusEmoji => _block.IsCompleted ? "🦄" : _block.SolvedCount > 0 ? "⭐" : "";

    public ICommand SelectCommand { get; }
}
