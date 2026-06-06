using MathGame.Models;

namespace MathGame.Services;

public static class QuestionGenerator
{
    // Column values (primary operand N) and how many of the 100 cells are active:
    //   Easy   — N = 1..10 (fixed)
    //   Medium — 10 random distinct values from 1..20, sorted
    //   Hard   — 10 random distinct values from 1..50, sorted
    private static int[] RandomDistinct(int min, int max, int count)
        => Enumerable.Range(min, max - min + 1)
                     .OrderBy(_ => Random.Shared.Next())
                     .Take(count)
                     .OrderBy(n => n)
                     .ToArray();

    public static List<Block> CreateAllBlocks(Operation op, Difficulty difficulty)
    {
        var (columns, activeCount) = difficulty switch
        {
            Difficulty.Easy   => (Enumerable.Range(1, 10).ToArray(),  20),
            Difficulty.Medium => (RandomDistinct(1, 20, 10),          50),
            Difficulty.Hard   => (RandomDistinct(1, 50, 10),         100),
            _                 => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };

        // Always build the full 10 rows per block
        var blocks = columns.Select(n => CreateBlock(n, op, difficulty)).ToList();

        // Randomly disable the questions that are beyond the active quota
        if (activeCount < 100)
        {
            var excess = blocks
                .SelectMany(b => b.Questions)
                .OrderBy(_ => Random.Shared.Next())
                .Skip(activeCount);

            foreach (var q in excess)
                q.State = QuestionState.Disabled;
        }

        return blocks;
    }

    private static Block CreateBlock(int n, Operation op, Difficulty difficulty)
    {
        var questions = Enumerable.Range(1, 10)
            .Select(k => MakeQuestion(n, k, op, difficulty))
            .ToList();
        return new Block(n, op, questions);
    }

    private static Question MakeQuestion(int n, int k, Operation op, Difficulty difficulty)
        => op switch
        {
            Operation.Addition       => new Question(n, k, op, n + k),

            // Easy/Medium: always positive  (N+k) − N = k
            // Hard: N − k, which can be negative when k > N (e.g. 5 − 7 = −2)
            Operation.Subtraction    => difficulty == Difficulty.Hard
                                        ? new Question(n, k, op, n - k)
                                        : new Question(n + k, n, op, k),

            Operation.Multiplication => new Question(n, k, op, n * k),

            // (N×k) ÷ N = k — always a whole number
            Operation.Division       => new Question(n * k, n, op, k),

            _                        => throw new ArgumentOutOfRangeException(nameof(op))
        };
}
