using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

public static class Program
{
    private static ConcurrentDictionary<string, int> tripletsCount =
        new ConcurrentDictionary<string, int>(Environment.ProcessorCount, 100);

    private static bool exitLoop = false;

    public static void Main()
    {
        if (!TryGetPath(out var filePath))
           return;
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        FillTripletsDictionary(filePath);
        PrintTop10Triplets();
        stopwatch.Stop();

        Console.WriteLine($"Время выполненния программы: {stopwatch.ElapsedMilliseconds}мс. ");
    }

    private static bool TryGetPath(out string filePath)
    {
        Console.WriteLine("Введите путь к файлу:");
        filePath = Console.ReadLine();

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Такого файла нет.");
            return false;
        }

        if (filePath.Substring(filePath.Length - 4, 4) != ".txt")
        {
            Console.WriteLine("Неверный формат файла, нужен файл с расширением \".txt\".");
            return false;
        }

        return true;
    }

    private static void FillTripletsDictionary(string path)
    {
        var fileLength = new FileInfo(path).Length;
        Console.WriteLine($"Файл весит {GetWeight(fileLength)}, желаете его читать по частям? Y/N");
        switch (Console.ReadLine().ToUpper())
        {
            case "Y":
                Console.WriteLine("Введите объем буфера в символах");
                if (int.TryParse(Console.ReadLine(), out int bufferLengt))
                {
                    if (bufferLengt < 2)
                    {
                        Console.WriteLine("Введите буфер большего размера.");
                        return;
                    }

                    if (bufferLengt > fileLength)
                    {
                        Console.WriteLine("Размер буфера не может превышать размер файла.");
                        return;
                    }

                    StartFilling(ChunksRead, path, bufferLengt);
                }
                else
                    Console.WriteLine("Неверный размер, размер буфера не должен превышать размеры int.");

                break;
            case "N":
                StartFilling(FullRead, path, 0);
                break;
            default:
                Console.WriteLine("Неправильный символ");
                break;
        }
    }
    
    private static void CountTriplets(string text)
    {
        Parallel.For(0, text.Length - 2, (i, state) =>
        {
            if (exitLoop)
                state.Break();

            var triplet = text.Substring(i, 3);
            if (char.IsLetter(triplet[0]) && char.IsLetter(triplet[1]) && char.IsLetter(triplet[2]))
            {
                if (tripletsCount.ContainsKey(triplet))
                    tripletsCount[triplet]++;
                else
                    tripletsCount[triplet] = 1;
            }
        });
    }

    private static void StartFilling(Action<string, int> filler, string path, int bufferrLength)
    {
        Thread thread = new Thread(() => filler(path, bufferrLength));
        thread.Start();

        while (!exitLoop)
        {
            if (Console.KeyAvailable)
            {
                exitLoop = true;
            }

            Thread.Sleep(100);
        }

        thread.Join();
    }

    private static void ChunksRead(string path, int bufferrLength)
    {
        using (FileStream fs = File.OpenRead(path))
        {
            byte[] buffer = new byte[bufferrLength];
            int bytesRead;

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                string textChunk = Encoding.Default.GetString(buffer, 0, bytesRead);
                CountTriplets(textChunk);
                if (exitLoop)
                    break;
            }
        }

        exitLoop = true;
    }

    private static void FullRead(string path, int dummy = 0)
    {
        CountTriplets(File.ReadAllText(path));
        exitLoop = true;
    }

    private static void PrintTop10Triplets()
    {
        var topTriplets = tripletsCount.OrderByDescending(x => x.Value)
            .Take(10);

        foreach (var triplet in topTriplets)
        {
            Console.WriteLine($"Триплет {triplet.Key} встречается {triplet.Value} раз");
        }
    }

    private static string GetWeight(long fileWeight)
    {
        var fileExtencion = new string[] { "Bytes", "KB", "MB", "GB", "TB" };
        var number = (decimal)fileWeight;
        var counter = 0;
        while (number / 1024 > 1024)
        {
            number /= 1024;
            ++counter;
        }

        return $"{number:N4}{fileExtencion[counter]}";
    }
}