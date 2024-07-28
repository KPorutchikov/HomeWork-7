using System.Diagnostics;

// Создадим массивы чисел:
int limit_1 = 1_000_000;
int limit_2 = 10_000_000;
int limit_3 = 100_000_000;

int[][] nums = new int[3][];
nums[0] = Enumerable.Range(0, limit_1).ToArray();
nums[1] = Enumerable.Range(0, limit_2).ToArray();
nums[2] = Enumerable.Range(0, limit_3).ToArray();

List<ParallelOptions> options = new List<ParallelOptions>()
    {new ParallelOptions() { MaxDegreeOfParallelism = 2 }, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, new ParallelOptions() { MaxDegreeOfParallelism = 10 }};

List<Proto> _prototips = new List<Proto>();
long _sum;

// Произведем подсчет - последовательный, один поток
foreach (int[] numbers in nums)
{
    _sum = 0;
    Proto p = new Proto("Синхронный", numbers.Length);
    
    var _watch = Stopwatch.StartNew();
    for (int i = 0; i < numbers.Length; i++) { _sum += numbers[i]; }
    _watch.Stop();

    p.SpendOfTime = _watch.ElapsedMilliseconds;
    p.SumOfArray  = _sum;
    _prototips.Add(p);
}

// Произведем подсчет - паралельно, в несколько потоков (2, 4, 10)
foreach (int[] numbers in nums)
{
    foreach (ParallelOptions option in options)
    {   _sum = 0;
        Proto p = new Proto($"Паралельный-{option.MaxDegreeOfParallelism}", numbers.Length, option.MaxDegreeOfParallelism);
        var _watch = Stopwatch.StartNew();

        Parallel.ForEach<int, long>( numbers, option,
                                    () => 0,
                                    (num, state, localSum) => { localSum += num; return localSum; },
                                    (localSum) => Interlocked.Add(ref _sum, localSum) );
        _watch.Stop();
        p.SpendOfTime = _watch.ElapsedMilliseconds;
        p.SumOfArray = _sum;
        _prototips.Add(p);
    }
}

// Произведем подсчет - паралельно, с помощью LINQ
foreach (int[] numbers in nums)
{   _sum = 0;
    Proto p = new Proto("PLINQ      ", numbers.Length, -1);
    var _watch = Stopwatch.StartNew();

    _sum = numbers.AsParallel().Sum(x => (long)x);

    _watch.Stop();
    p.SpendOfTime = _watch.ElapsedMilliseconds;
    p.SumOfArray = _sum;
    _prototips.Add(p);
}

// Вывод результатов:
Console.WriteLine(new string('_', 90));

var _proto = _prototips.Where(p => p.ArrayLength == limit_1).OrderBy(p => p.SpendOfTime);
foreach (Proto p in _proto) { p.Print(); }
Console.WriteLine(new string('_', 90));

_proto = _prototips.Where(p => p.ArrayLength == limit_2).OrderBy(p => p.SpendOfTime);
foreach (Proto p in _proto) { p.Print(); }
Console.WriteLine(new string('_', 90));

_proto = _prototips.Where(p => p.ArrayLength == limit_3).OrderBy(p => p.SpendOfTime);
foreach (Proto p in _proto) { p.Print(); }
Console.WriteLine(new string('_', 90));

Console.ReadKey();


struct Proto
{
    public string Name;
    public int ArrayLength;
    public long SumOfArray;
    public int ThreadsCount;
    public float SpendOfTime;

    public Proto(string name, int arrayLength, int threadsCount = 1)
    {
        this.Name = name;
        this.ArrayLength = arrayLength;
        this.ThreadsCount = threadsCount;
    }
    public void Print() => Console.WriteLine($"{this.Name}\t|Массив: {this.ArrayLength}\t|Сумма: {this.SumOfArray}\tThreads:{this.ThreadsCount}\t|Time:{this.SpendOfTime} ms");
}