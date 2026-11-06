namespace ParallelComputingLab;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Parallel Computing Lab ===");

        RunCorrectnessChecks();
        RunLostUpdateExperiment();

        int[] sizes = { 10_000, 100_000, 1_000_000, 5_000_000, 10_000_000 };

        foreach (int size in sizes)
        {
            int[] numbers = DataGenerator.Generate(size, 12345);

            BenchmarkRunner.Run(numbers);
            BenchmarkRunner.CompareParallelism(numbers);

            Console.WriteLine();
        }

        Console.WriteLine("Работа завершена.");
    }

    private static void RunCorrectnessChecks()
    {
        Console.WriteLine();
        Console.WriteLine("=== ПРОВЕРКИ КОРРЕКТНОСТИ ===");

        int failed = 0;

        Case("7, 14, 20, 21 / делитель 7", new[] { 7, 14, 20, 21 }, 7, 42, 3, 21);
        Case("1, 2, 3, 4 / делитель 7", new[] { 1, 2, 3, 4 }, 7, 0, 0, 0);
        Case("пустой массив", Array.Empty<int>(), 7, 0, 0, 0);
        Case("7, 7, 7 / делитель 7", new[] { 7, 7, 7 }, 7, 21, 3, 7);
        Case("один элемент 14", new[] { 14 }, 7, 14, 1, 14);
        Case("13, 26, 39 / делитель 13", new[] { 13, 26, 39 }, 13, 78, 3, 39);
        Case("7, 14, 20, 21 / делитель 13", new[] { 7, 14, 20, 21 }, 13, 0, 0, 0);

        try
        {
            Calculator.CalculateParallel(null!);
            Console.WriteLine("null: ошибки нет — FAIL");
            failed++;
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("null: ArgumentNullException — OK");
        }

        try
        {
            Calculator.CalculateParallel(new[] { 7 }, 0);
            Console.WriteLine("Degree = 0: ошибки нет — FAIL");
            failed++;
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Degree = 0: ArgumentOutOfRangeException — OK");
        }

        Console.WriteLine(
            failed == 0
                ? "Все проверки пройдены."
                : $"Провалено проверок: {failed}");

        void Case(
            string name,
            int[] numbers,
            int divisor,
            long sum,
            int count,
            long max)
        {
            var sequential = Calculator.CalculateSequential(numbers, divisor);
            var parallel = Calculator.CalculateParallel(numbers, -1, divisor);

            bool ok =
                sequential == parallel &&
                parallel.Sum == sum &&
                parallel.Count == count &&
                parallel.Max == max;

            Console.WriteLine($"{name}: {(ok ? "OK" : "FAIL")}");

            if (!ok)
            {
                failed++;
            }
        }
    }

    private static void RunLostUpdateExperiment()
    {
        Console.WriteLine();
        Console.WriteLine("=== LOST UPDATE ===");

        int[] numbers = DataGenerator.Generate(5_000_000, 12345);

        var correct = Calculator.CalculateParallel(numbers);
        var wrong = Calculator.CalculateParallelLostUpdate(numbers);
        var fixedWrong = Calculator.CalculateParallelWrong(numbers);

        Console.WriteLine(
            $"Правильно:         сумма {correct.Sum:N0}, количество {correct.Count:N0}");

        Console.WriteLine(
            $"Без синхронизации: сумма {wrong.Sum:N0}, количество {wrong.Count:N0}");

        Console.WriteLine(
            $"Потеряно:          {correct.Sum - wrong.Sum:N0} и {correct.Count - wrong.Count:N0}");

        Console.WriteLine(
            $"CalculateParallelWrong: {(fixedWrong == correct ? "совпадает" : "не совпадает")}");
    }
}
