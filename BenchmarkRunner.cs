using System.Diagnostics;

namespace ParallelComputingLab;

public static class BenchmarkRunner
{
    private const int MeasurementCount = 10;

    public static void Run(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        Console.WriteLine();
        Console.WriteLine($"Размер массива: {numbers.Length:N0}");

        Calculator.CalculateSequential(numbers);
        Calculator.CalculateParallel(numbers);

        var sequentialTimes = new List<double>();
        var parallelTimes = new List<double>();
        CalculationResult? expected = null;

        for (int i = 0; i < MeasurementCount; i++)
        {
            var sequential = Measure(
                () => Calculator.CalculateSequential(numbers));

            var parallel = Measure(
                () => Calculator.CalculateParallel(numbers));

            expected ??= sequential.Result;

            if (sequential.Result != parallel.Result)
            {
                throw new InvalidOperationException(
                    "Последовательный и параллельный результаты не совпадают.");
            }

            sequentialTimes.Add(sequential.ElapsedMilliseconds);
            parallelTimes.Add(parallel.ElapsedMilliseconds);
        }

        Console.WriteLine($"Сумма:           {expected!.Sum:N0}");
        Console.WriteLine($"Количество:      {expected.Count:N0}");
        Console.WriteLine($"Максимум:        {expected.Max:N0}");
        Console.WriteLine($"Последовательно: {Median(sequentialTimes):F3} мс");
        Console.WriteLine($"Параллельно:     {Median(parallelTimes):F3} мс");
    }

    public static void CompareParallelism(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        int[] degrees = { 1, 2, 3, 4, -1 };

        Console.WriteLine();
        Console.WriteLine(
            $"Степени параллелизма, размер {numbers.Length:N0}:");
        Console.WriteLine("Вариант         | Время, мс | Ускорение");
        Console.WriteLine("---------------------------------------");

        CalculationResult expected = Calculator.CalculateSequential(numbers);

        foreach (int degree in degrees)
        {
            Calculator.CalculateParallel(numbers, degree);
        }

        var sequentialTimes = new List<double>();

        for (int i = 0; i < MeasurementCount; i++)
        {
            sequentialTimes.Add(
                Measure(() => Calculator.CalculateSequential(numbers))
                    .ElapsedMilliseconds);
        }

        double sequential = Median(sequentialTimes);

        Console.WriteLine(
            $"{"Последовательно",-15} | {sequential,9:F3} | {1.0,7:F2}x");

        foreach (int degree in degrees)
        {
            var times = new List<double>();

            for (int i = 0; i < MeasurementCount; i++)
            {
                var measurement = Measure(
                    () => Calculator.CalculateParallel(numbers, degree));

                if (measurement.Result != expected)
                {
                    throw new InvalidOperationException(
                        $"Degree = {degree}: результат не совпал.");
                }

                times.Add(measurement.ElapsedMilliseconds);
            }

            double median = Median(times);

            Console.WriteLine(
                $"{degree,15} | {median,9:F3} | {sequential / median,7:F2}x");
        }
    }

    public static (CalculationResult Result, double ElapsedMilliseconds)
        Measure(Func<CalculationResult> action)
    {
        var stopwatch = Stopwatch.StartNew();
        CalculationResult result = action();
        stopwatch.Stop();

        return (result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static double Median(List<double> values)
    {
        double[] sorted = values.OrderBy(x => x).ToArray();
        int middle = sorted.Length / 2;

        if (sorted.Length % 2 == 1)
        {
            return sorted[middle];
        }

        return (sorted[middle - 1] + sorted[middle]) / 2;
    }
}
