using System.Globalization;

namespace IsoMetrix.CodingTest.Calculator;

public static class StringCalculator
{
    private static readonly SortedSet<string> DefaultDelimiters = new() { ",", "\n" };

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
            foreach (var delimiter in delimiters)
            {
                if (input[i..].StartsWith(delimiter))
                {
                    TryAddNumberAndResetStart(input, i);
                    tokens.Add(new Delimiter(input[i]));
                    i += delimiter.Length - 1;
                    break;
                }
            }

            if (!numberStart.HasValue && (char.IsDigit(input[i]) || input[i] == '-'))
            {
                numberStart = i;
            }
        }

        TryAddNumberAndResetStart(input, input.Length);

        return tokens.AsReadOnly();
    }

    private static ReadOnlySpan<char> ConsumeToDelimiters(ReadOnlySpan<char> input, out SortedSet<string> delimiters)
    {
        if (input.StartsWith("//"))
        {
            var indexOfDelimiterPattern = input.IndexOf('\n');
            var pattern = input[2..indexOfDelimiterPattern];

            delimiters = ParseDelimiterPattern(pattern);
            return input[(indexOfDelimiterPattern + 1)..];
        }

        delimiters = DefaultDelimiters;
        return input;
    }

    private static SortedSet<string> ParseDelimiterPattern(ReadOnlySpan<char> pattern)
    {
        if (pattern.StartsWith("[") && pattern.EndsWith("]"))
        {
            var delimiters = new SortedSet<string>();

            while (pattern.Length > 0)
            {
                var end = pattern.IndexOf("]");
                delimiters.Add(pattern[1..(end - 1)].ToString());
                pattern = pattern[(end + 1)..];
            }

            return delimiters;
        }

        return new SortedSet<string> { pattern[0].ToString() };
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