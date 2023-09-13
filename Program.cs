using System.Diagnostics;

public static class Program
{
    private static Dictionary<string, int> tripletsCount = new Dictionary<string, int>();
    private static object lockObject = new object();

    public static void Main()
    {
        Console.WriteLine("Введите путь к файлу:");
        var filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Такого файла нет");
            return;
        }

        if (filePath.Substring(filePath.Length - 4, 4) != ".txt")
        {
            Console.WriteLine("Неверный формат файла, ужен файл с расширением \".txt\"");
            return;
        }

        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        CountTriplets(File.ReadAllText(filePath));
        PrintTop10Triplets();
        stopwatch.Stop();
        
        Console.WriteLine($"Время выполненния программы: {stopwatch.ElapsedMilliseconds}мс. ");
    }

    private static void CountTriplets(string text)
    {
        Parallel.For(0, text.Length - 2, i =>
        {
            string triplet = text.Substring(i, 3);

            lock (lockObject)
            {
                if (tripletsCount.ContainsKey(triplet))
                {
                    tripletsCount[triplet]++;
                }
                else
                {
                    tripletsCount[triplet] = 1;
                }
            }
        });
    }

    private static void PrintTop10Triplets()
    {
        var topTriplets = tripletsCount.OrderByDescending(x => x.Value)
            .Where(x => char.IsLetter(x.Key[0]) && char.IsLetter(x.Key[1]) && char.IsLetter(x.Key[2]))
            .Take(10);
        foreach (var triplet in topTriplets)
        {
            Console.WriteLine($"Триплет {triplet.Key} встречается {triplet.Value} раз");
        }
    }
}