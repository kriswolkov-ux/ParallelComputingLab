namespace ParallelComputingLab;

public static class Calculator
{
    public static CalculationResult CalculateSequential(
        int[] numbers,
        int divisor = 7)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        ValidateDivisor(divisor);

        long sum = 0;
        int count = 0;
        long max = 0;

        foreach (int number in numbers)
        {
            if (number % divisor == 0)
            {
                sum += number;
                count++;

                if (number > max)
                {
                    max = number;
                }
            }
        }

        return new CalculationResult(sum, count, max);
    }

    public static CalculationResult CalculateParallel(
        int[] numbers,
        int maxDegreeOfParallelism = -1,
        int divisor = 7)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        ValidateDivisor(divisor);
        ValidateDegree(maxDegreeOfParallelism);

        long sum = 0;
        int count = 0;
        long max = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        Parallel.For(
            0,
            numbers.Length,
            options,
            () => (Sum: 0L, Count: 0, Max: 0L),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % divisor == 0)
                {
                    local.Sum += number;
                    local.Count++;

                    if (number > local.Max)
                    {
                        local.Max = number;
                    }
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.Sum);
                Interlocked.Add(ref count, local.Count);
                UpdateMax(ref max, local.Max);
            });

        return new CalculationResult(sum, count, max);
    }

    public static CalculationResult CalculateParallelWrong(
        int[] numbers,
        int divisor = 7)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        ValidateDivisor(divisor);

        long sum = 0;
        int count = 0;
        long max = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = -1
        };

        Parallel.For(
            0,
            numbers.Length,
            options,
            () => (LocalSum: 0L, LocalCount: 0, LocalMax: 0L),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % divisor == 0)
                {
                    local.LocalSum += number;
                    local.LocalCount++;

                    if (number > local.LocalMax)
                    {
                        local.LocalMax = number;
                    }
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.LocalSum);
                Interlocked.Add(ref count, local.LocalCount);
                UpdateMax(ref max, local.LocalMax);
            });

        return new CalculationResult(sum, count, max);
    }

    public static CalculationResult CalculateParallelLostUpdate(
        int[] numbers,
        int divisor = 7)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        ValidateDivisor(divisor);

        long sum = 0;
        int count = 0;

        Parallel.For(0, numbers.Length, i =>
        {
            int number = numbers[i];

            if (number % divisor == 0)
            {
                sum += number;
                count++;
            }
        });

        return new CalculationResult(sum, count, 0);
    }

    private static void UpdateMax(ref long target, long candidate)
    {
        long current = Volatile.Read(ref target);

        while (candidate > current)
        {
            long previous = Interlocked.CompareExchange(
                ref target,
                candidate,
                current);

            if (previous == current)
            {
                return;
            }

            current = previous;
        }
    }

    private static void ValidateDivisor(int divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(divisor));
        }
    }

    private static void ValidateDegree(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism != -1 &&
            maxDegreeOfParallelism < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDegreeOfParallelism));
        }
    }
}
