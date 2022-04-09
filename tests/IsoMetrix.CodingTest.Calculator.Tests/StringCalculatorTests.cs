using System;
using FluentAssertions;
using Xunit;

namespace IsoMetrix.CodingTest.Calculator.Tests;

public class StringCalculatorTests
{
    [Fact]
    public void ShouldReturnZeroForAnEmptyString()
    {
        StringCalculator.Add("")
            .Should().Be(0);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("2", 2)]
    [InlineData("3", 3)]
    [InlineData("4", 4)]
    [InlineData("5", 5)]
    [InlineData("6", 6)]
    [InlineData("7", 7)]
    [InlineData("8", 8)]
    [InlineData("9", 9)]
    [InlineData("10", 10)]
    public void ShouldReturnTheNumberForASingleNumber(string input, int expectedOutput)
    {
        StringCalculator.Add(input)
            .Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("1,2", 3)]
    [InlineData("3,4", 7)]
    [InlineData("5,6", 11)]
    [InlineData("7,8", 15)]
    [InlineData("9,10", 19)]
    public void ShouldReturnTheSumOfTwoNumbers(string input, int expectedOutput)
    {
        StringCalculator.Add(input)
            .Should().Be(expectedOutput);
    }
    
    [Theory]
    [InlineData("1,2,3", 6)]
    [InlineData("5,6,7,8", 26)]
    [InlineData("10,21,32,43", 106)]
    public void ShouldReturnTheSumOfAllNumbers(string input, int expectedOutput)
    {
        StringCalculator.Add(input)
            .Should().Be(expectedOutput);
    }
    
    [Theory]
    [InlineData("1\n2,3", 6)]
    [InlineData("5,6\n7,8", 26)]
    [InlineData("10,21,32\n43", 106)]
    public void ShouldReturnTheSumOfAllNumbersWithMixedCommaAndNewLineDelimiters(string input, int expectedOutput)
    {
        StringCalculator.Add(input)
            .Should().Be(expectedOutput);
    }
    
    [Theory]
    [InlineData("//;\n", 0)]
    [InlineData("//;\n;", 0)]
    [InlineData("//;\n1;2", 3)]
    [InlineData("//;\n1;2;3", 6)]
    [InlineData("//+\n1+2+3", 6)]
    public void ShouldReturnTheSumOfAllNumbersWithCustomSingleDelimiter(string input, int expectedOutput)
    {
        StringCalculator.Add(input)
            .Should().Be(expectedOutput);
    }
    
    [Theory]
    [InlineData("-1", "-1")]
    [InlineData("1,-3", "-3")]
    [InlineData("1,-3,-4", "-3,-4")]
    [InlineData("//;\n1;-2", "-2")]
    [InlineData("//;\n-1;2;-3", "-1,-3")]
    public void ShouldThrowAnInvalidArgumentExceptionWhenANegativeNumberIsPassed(string input, string expectedExceptionMessage)
    {
        Action action = () => StringCalculator.Add(input);
        
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Negatives not allowed: {expectedExceptionMessage} (Parameter 'input')");
    }
}