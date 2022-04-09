using System.Globalization;

namespace IsoMetrix.CodingTest.Calculator;

public static class StringCalculator
{
    private static readonly SortedSet<char> DefaultDelimiters = new() { ',', '\n' };
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
        
        input = ConsumeToDelimiters(input, out var delimiters);

        for (var i = 0; i < input.Length; i++)
        {
            if (delimiters.Contains(input[i]))
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

    private static ReadOnlySpan<char> ConsumeToDelimiters(ReadOnlySpan<char> input, out SortedSet<char> delimiters)
    {
        if(input.StartsWith("//"))
        {
            var indexOfDelimiterPattern = input.IndexOf('\n');
            var pattern = input[2..indexOfDelimiterPattern];

            delimiters = new SortedSet<char> { pattern[0] };
            return input[(indexOfDelimiterPattern + 1)..];
        }
        delimiters = DefaultDelimiters;
        return input;
    }
}

internal abstract record Token;
internal record Delimiter(char Value) : Token;
internal record Number(string Value) : Token
{
    public int GetValue() => int.Parse(Value, CultureInfo.InvariantCulture);
};