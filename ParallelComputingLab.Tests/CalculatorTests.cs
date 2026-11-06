using ParallelComputingLab;

namespace ParallelComputingLab.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(new[] { 7, 14, 20, 21 }, 42, 3)]
    [InlineData(new[] { 1, 2, 3, 4 }, 0, 0)]
    [InlineData(new[] { 7, 7, 7 }, 21, 3)]
    [InlineData(new int[] { }, 0, 0)]
    [InlineData(new[] { 14 }, 14, 1)]
    public void SequentialAndParallel_ReturnSameResult(
        int[] numbers,
        long expectedSum,
        int expectedCount)
    {
        var sequential =
            Calculator.CalculateSequential(numbers);

        var parallel =
            Calculator.CalculateParallel(numbers);

        Assert.Equal(expectedSum, sequential.Sum);
        Assert.Equal(expectedCount, sequential.Count);
        Assert.Equal(sequential, parallel);
    }

    [Fact]
    public void Parallel_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Calculator.CalculateParallel(null!));
    }

    [Fact]
    public void Parallel_WithInvalidDegree_ThrowsException()
    {
        int[] numbers = { 7, 14, 21 };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Calculator.CalculateParallel(numbers, 0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(-1)]
    public void Parallel_WithDifferentDegrees_ReturnsSameResult(int degree)
    {
        int[] numbers = DataGenerator.Generate(
            100_000,
            seed: 12345);

        var expected =
            Calculator.CalculateSequential(numbers);

        var actual =
            Calculator.CalculateParallel(numbers, degree);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Max_ReturnsLargestMatchingElement()
    {
        int[] numbers = { 7, 14, 21, 20 };

        var result = Calculator.CalculateSequential(numbers);

        Assert.Equal(21, result.Max);
    }

    [Fact]
    public void Sequential_WithDivisor13_CountsOnlyMultiplesOf13()
    {
        int[] numbers = { 13, 26, 7, 39 };

        var result = Calculator.CalculateSequential(numbers, 13);

        Assert.Equal(78, result.Sum);
        Assert.Equal(3, result.Count);
        Assert.Equal(39, result.Max);
    }

    [Fact]
    public void EmptyArray_ReturnsZeroes()
    {
        var result = Calculator.CalculateParallel(Array.Empty<int>());

        Assert.Equal(0, result.Sum);
        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.Max);
    }
}
