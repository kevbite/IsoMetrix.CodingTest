using System.Globalization;

namespace IsoMetrix.CodingTest.Calculator;

public static class StringCalculator
{
    private static readonly SortedSet<char> PossibleDelimiters = new() { ',' };
    public static int Add(ReadOnlySpan<char> input)
    {
        var tokens = GetTokens(input);

        return tokens.OfType<Number>().Sum(x => x.GetValue());
    }

    private static IReadOnlyList<Token> GetTokens(ReadOnlySpan<char> input)
    {
        var tokens = new List<Token>();
        int? numberStart = null;
        void TryAddNumberAndResetStart(ReadOnlySpan<char> input, int index)
        {
            if (numberStart.HasValue)
            {
                tokens.Add(new Number(input[numberStart.Value..index].ToString()));
                numberStart = null;
            }
        }
        for (var i = 0; i < input.Length; i++)
        {
            if (PossibleDelimiters.Contains(input[i]))
            {
                TryAddNumberAndResetStart(input, i);
                tokens.Add(new Delimiter(input[i]));
            }

            if (!numberStart.HasValue && char.IsDigit(input[i]))
            {
                numberStart = i;
            }
        }
        
        TryAddNumberAndResetStart(input, input.Length);

        return tokens.AsReadOnly();
    }
}

internal abstract record Token;
internal record Delimiter(char Value) : Token;
internal record Number(string Value) : Token
{
    public int GetValue() => int.Parse(Value, CultureInfo.InvariantCulture);
};