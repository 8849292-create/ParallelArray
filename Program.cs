using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelArray
{
    class Result
    {
        public long Sum;
        public int Min = int.MaxValue;
        public int Max = int.MinValue;
        public long EvenCount;
    }
    internal class Program
    {
        private const int ArraySize = 50_000_000;
        static Result[] results;
        static int[] numbers;
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Паралельна обробка великого масиву ===");
            Console.WriteLine("\n=== Создание массива ===");
            numbers = new int[ArraySize];

            Random rnd = new Random();
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = rnd.Next(1, 1000);
            }
            Console.WriteLine("Массив создан");
            Console.WriteLine($"Кол-во элементов: {numbers.Length:N0}");

            //------------------------
            
            Console.Write("Количество потоков K: ");
            string input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int manualK) && manualK > 0 && manualK<=numbers.Length)
            {
                Run(manualK);
                
            }
            else if (!string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Некорректный ввод"); return;
                
            }          

            //----------
            int[] tests = { 1, 2, 4, 8 };
            Console.WriteLine("\nПоток\t| Время (ms)");
            Console.WriteLine("-------------------");

            foreach (int k in tests)
            {
                long time = Run(k);
                Console.WriteLine($"{k}\t| {time}");                
            }            
        }
        static long Run(int k)
        {
            int chunk = numbers.Length / k;
            results = new Result[k];
            Thread[] threads = new Thread[k];

            Console.WriteLine($"\nЗапуск с {k} потоками, Размер : {chunk:N0} элементов");

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < k; i++)
            {
                int index = i; 
                int start = i * chunk;                
                int end = (i == k - 1) ? numbers.Length : start + chunk;

                threads[i] = new Thread(() =>
                {
                    ProcessPart(index, start, end);
                });
                threads[i].Start();
            }
            
            foreach (var thread in threads)
            {
                thread.Join();
            }

            sw.Stop();
           
            long sum = 0;
            int min = int.MaxValue;
            int max = int.MinValue;
            long evenCount = 0;

            for (int i = 0; i < k; i++)
            {
                sum += results[i].Sum;
                if (results[i].Min < min)
                    min = results[i].Min;
                if (results[i].Max > max)
                    max = results[i].Max;
                evenCount += results[i].EvenCount;
            }

            Console.WriteLine();
            Console.WriteLine("=== Результат для K = " + k + " ===");
            Console.WriteLine($"Сумма: {sum:N0}");
            Console.WriteLine($"Минимум: {min}");
            Console.WriteLine($"Максимум: {max}");
            Console.WriteLine($"Четных чисел: {evenCount:N0}");
            Console.WriteLine($"Среднее: {(double)sum / ArraySize:F2}");
            Console.WriteLine($"Время: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine("-------------------");

            return sw.ElapsedMilliseconds;
        }

        static void ProcessPart(int index, int start, int end)
        {
            var r = new Result();
            for (int i = start; i < end; i++)
            {
                int value = numbers[i];

                r.Sum += value;
                if (value < r.Min)
                    r.Min = value;
                if (value > r.Max)
                    r.Max = value;

                if ((value & 1) == 0)
                    r.EvenCount++;
            }
            results[index] = r;            
            Console.WriteLine($"Поток {index}: обработано [{start}, {end})");
        }
    }
}
