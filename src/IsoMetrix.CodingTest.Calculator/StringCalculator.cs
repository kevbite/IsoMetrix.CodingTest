using System.Globalization;

namespace IsoMetrix.CodingTest.Calculator;

public static class StringCalculator
{
    private static readonly SortedSet<char> DefaultDelimiters = new() { ',', '\n' };

    public static int Add(ReadOnlySpan<char> input)
    {
        var tokens = GetTokens(input);

        var numberTokens = tokens.OfType<Number>();

        var values = numberTokens
            .Select(x => x.GetValue())
            .ToList();

        var invalidNumbers = values.Where(x => x < 0).ToList();
        if (invalidNumbers.Any())
        {
            throw new ArgumentException($"Negatives not allowed: {string.Join(",", invalidNumbers)}", nameof(input));
        }

        return values.Sum();
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

            if (!numberStart.HasValue && (char.IsDigit(input[i]) || input[i] == '-'))
            {
                numberStart = i;
            }
        }

        TryAddNumberAndResetStart(input, input.Length);

        return tokens.AsReadOnly();
    }

    private static ReadOnlySpan<char> ConsumeToDelimiters(ReadOnlySpan<char> input, out SortedSet<char> delimiters)
    {
        if (input.StartsWith("//"))
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
    const int MaxValue = 1000;

    public int GetValue() => int.Parse(Value, CultureInfo.InvariantCulture)
        switch
        {
            > MaxValue => 0,
            var x => x
        };
};