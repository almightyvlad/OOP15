using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OOP15
{
    internal class Program
    {

        public static double[,] MultiplyMatrix(double[,] matrixA, double[,] matrixB, CancellationToken cancellationToken)
        {
            int rowsA = matrixA.GetLength(0);
            int colsA = matrixA.GetLength(1);
            int rowsB = matrixB.GetLength(0);
            int colsB = matrixB.GetLength(1);


            double[,] result = new double[rowsA, colsB];

            Parallel.For(0, rowsA, i =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Операция прервана");
                    return;
                }

                for (int j = 0; j < colsB; j++)
                {
                    double sum = 0;

                    for (int k = 0; k < colsA; k++)
                    {
                        sum += matrixA[i, k] * matrixB[k, j];
                    }

                    result[i, j] = sum;

                }
            });

            return result;
        }
        public static double[,] GenerateMatrix(int rows, int cols)
        {
            Random random = new Random();

            double[,] result = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = random.NextDouble() * 100;
                }
            }
            return result;
        }
        public static int CalculateFunc1()
        {
            Console.WriteLine("Func1");
            Thread.Sleep(1000);
            return 812;
        }
        public static int CalculateFunc2()
        {
            Console.WriteLine("Func2");
            Thread.Sleep(1500);
            return 18;
        }
        public static int CalculateFunc3()
        {
            Console.WriteLine("Func3");
            Thread.Sleep(2000);
            return 10;
        }
        public static int CalculateAll(int value1, int value2, int value3)
        {
            Console.WriteLine("Calculate All");
            return value1 + value2 * value3;
        }
        public static void ContinueMethod(Task t)
        {
            Console.WriteLine("ContinueMethod:");
            Console.WriteLine($"ID: {t.Id}");
        }
        public static double[] GenerateArray(int size)
        {
            Random random = new Random();

            double[] array = new double[size];

            for (int i = 0; i < size; i++)
            {
                array[i] = random.NextDouble() * 100;
            }

            return array;
        }
        static void DoTask1()
        {
            Console.WriteLine("Task 1 start");
            Task.Delay(1000).Wait();
            Console.WriteLine("Task 1 end");
        }
        static void DoTask2()
        {
            Console.WriteLine("Task 2 start");
            Task.Delay(1500).Wait();
            Console.WriteLine("Task 2 end");
        }
        static void DoTask3()
        {
            Console.WriteLine("Task 3 start");
            Task.Delay(2000).Wait();
            Console.WriteLine("Task 3 end");
        }
        static void Supplier(BlockingCollection<string> warehouse, int supplierId)
        {
            Random random = new Random();
            string[] products = { "Телевизор", "Холодильник", "Стиральная машина", "Микроволновка", "Утюг" };

            for (int i = 0; i < 10; i++)
            {
                string product = $"{products[random.Next(products.Length)]} от поставщика {supplierId}";

                Thread.Sleep(random.Next(500, 2000));

                warehouse.Add(product);

                Console.WriteLine($"Поставщик {supplierId} добавил товар: {product}");

                PrintWarehouseState(warehouse);
            }
        }
        static void PrintWarehouseState(BlockingCollection<string> warehouse)
        {
            Console.WriteLine("Текущее состояние склада:");
            foreach (var item in warehouse)
            {
                Console.WriteLine($" - {item}");
            }
            Console.WriteLine("==============================");
        }
        static void Customer(BlockingCollection<string> warehouse, int customerId)
        {
            Random random = new Random();

            while (!warehouse.IsCompleted)
            {
                if (warehouse.TryTake(out string product, random.Next(500, 1500)))
                {
                    Console.WriteLine($"Покупатель {customerId} купил товар: {product}");
                }
                else
                {
                    Console.WriteLine($"Покупатель {customerId} не нашел товара и ушел.");
                }
            }
        }
        static async Task Main(string[] args)
        {
            // 1, 2

            Stopwatch stopwatch = new Stopwatch();

            int size = 100;
            double[,] matrixA = GenerateMatrix(size, size);
            double[,] matrixB = GenerateMatrix(size, size);

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            stopwatch.Start();

            Task firstTask = new Task(() => MultiplyMatrix(matrixA, matrixB, token));

            Console.WriteLine($"ID: {firstTask.Id}");
            Console.WriteLine($"Status before: {firstTask.Status}");

            firstTask.Start();

            Thread.Sleep(1000);

            cancelTokenSource.Cancel();

            Thread.Sleep(1000);

            Console.WriteLine($"Task Status: {firstTask.Status}");

            cancelTokenSource.Dispose(); 

            firstTask.Wait();

            stopwatch.Stop();

            Console.WriteLine($"Status after: {firstTask.Status}");

            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");

            Console.WriteLine();

            // 3

            Task<int> task1 = Task.Run(() => CalculateFunc1());
            Task<int> task2 = Task.Run(() => CalculateFunc2());
            Task<int> task3 = Task.Run(() => CalculateFunc3());

            int[] calculateAllArr = await Task.WhenAll(task1, task2, task3);

            int result = CalculateAll(calculateAllArr[0], calculateAllArr[1], calculateAllArr[2]);

            Console.WriteLine($"Result of CalculateAll: {result}");

            // 4
            // 4.1
            Task continuationTask = Task.WhenAll(task1, task2, task3).ContinueWith(ContinueMethod);

            Console.WriteLine();

            // 4.2

            Task<int> mainTask = Task.Run(() =>
            {
                Console.WriteLine("Выполнение 4.2");
                Task.Delay(1000);
                return 42; 
            });

            var awaiter = mainTask.GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                Console.WriteLine($"Результат 4.2: {awaiter.GetResult()}");
            });

            mainTask.Wait();

            Console.WriteLine();

            // 5

            int arrCount = 10;
            int arrSize = 1000000;

            Stopwatch stopwatch1 = Stopwatch.StartNew();

            double[][] arrFor = new double[arrCount][];

            for (int i = 0; i < arrCount; i++)
            {
                arrFor[i] = GenerateArray(arrSize);
            }

            stopwatch1.Stop();

            Console.WriteLine($"For: {stopwatch1.ElapsedMilliseconds} ms");

            stopwatch1.Restart();

            double[][] arrParallel = new double[arrCount][];

            Parallel.For(0, arrCount, i =>
            {
                arrParallel[i] = GenerateArray(arrSize);
            });

            stopwatch1.Stop();

            Console.WriteLine($"Parallel.For: {stopwatch1.ElapsedMilliseconds} ms");

            Console.WriteLine();

            // 6

            Console.WriteLine("Start 5 Task");

            Parallel.Invoke(
                () => DoTask1(),
                () => DoTask2(),
                () => DoTask3()
            );

            // 7

            BlockingCollection<string> warehouse = new BlockingCollection<string>(10);

            Task[] suppliers = new Task[5];

            for (int i = 0; i < suppliers.Length; i++)
            {
                int supplierId = i + 1;
                suppliers[i] = Task.Run(() => Supplier(warehouse, supplierId));
            }

            Task[] customers = new Task[10];
            for (int i = 0; i < customers.Length; i++)
            {
                int customerId = i + 1;
                customers[i] = Task.Run(() => Customer(warehouse, customerId));
            }

            Task.WaitAll(suppliers);

            warehouse.CompleteAdding();

            Task.WaitAll(customers);

        }
    }
}
