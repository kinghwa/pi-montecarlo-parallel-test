// Estimating the value of Pi using Monte Carlo
// threading samples
using System.Diagnostics;

double x, y;
long totalCnt =  600000000;
int pointInSquare = 0;
int pointInCircle = 0;
var rnd = new Random();
var stopwatch = new Stopwatch();
int numThreads = Environment.ProcessorCount;

Console.WriteLine($"Number of processor threads = {numThreads}");

// Single thread
stopwatch.Start();
for (long i = 0; i < totalCnt; i++)
{
    x = rnd.NextDouble();
    y = rnd.NextDouble();
    if (Math.Sqrt(x * x + y * y) <= 1.0)
    {
     pointInCircle++;
    }

    pointInSquare++;
}
double pi = 4.0 * pointInCircle / pointInSquare;
stopwatch.Stop();

Console.WriteLine($"Pi = {pi}  Time elapse = {stopwatch.Elapsed.TotalSeconds}sec run={pointInSquare}");

// Default parallel.for loop
pointInSquare = 0;
pointInCircle = 0;
rnd = new Random();
stopwatch.Restart();
Parallel.For(0L, totalCnt, p => {
        x = rnd.NextDouble();
        y = rnd.NextDouble();    
        if (Math.Sqrt(x * x + y * y) <= 1.0)
        {
            Interlocked.Increment(ref pointInCircle);
        }

        Interlocked.Increment(ref pointInSquare);        
});
pi = 4.0 * pointInCircle / pointInSquare;
stopwatch.Stop();
Console.WriteLine($"P4 Pi = {pi}  Time elapse = {stopwatch.Elapsed.TotalSeconds}sec run={pointInSquare}");

// parallel.for, spliting operations into the number of system threads
pointInSquare = 0;
pointInCircle = 0;
rnd = new Random();
long innerCnt = totalCnt/numThreads;
stopwatch.Restart();
Parallel.For(0, numThreads, new ParallelOptions { MaxDegreeOfParallelism = numThreads },
i => 
{
    int inCircle = 0;
    int inSquare = 0;
    for (long j = 0; j < innerCnt; j++)
    {
        x = rnd.NextDouble();
        y = rnd.NextDouble();
        if (Math.Sqrt(x * x + y * y) <= 1.0)
        {
            inCircle++;
        }
        inSquare++;       
    }
    Interlocked.Add(ref pointInCircle, inCircle);
    Interlocked.Add(ref pointInSquare, inSquare);
});
pi = 4.0 * pointInCircle / pointInSquare;
stopwatch.Stop();
Console.WriteLine($"Parallel Pi = {pi}  Time elapse = {stopwatch.Elapsed.TotalSeconds}sec  run={pointInSquare}");

// using task library for parallelism
rnd = new Random();
var proc = (object? c) => {
    int inCircle = 0;
    int inSquare = 0;
    long cnt = (long)(c ?? 0);
    //Console.WriteLine($"Task {cnt}");
    for (long j = 0; j < cnt; j++)
    {
        double x = rnd.NextDouble();
        double y = rnd.NextDouble();
        if (Math.Sqrt(x * x + y * y) <= 1.0)
        {
            inCircle++;
        }

        inSquare++;
    }

    return new Tuple<int,int>(inCircle, inSquare);
};

stopwatch.Restart();
List<Task<Tuple<int, int>>> tasklist = new List<Task<Tuple<int, int>>>();
for (int i = 0; i < numThreads; i++)
{
    var t = new Task<Tuple<int,int>>(proc, innerCnt);
    tasklist.Add(t);
    t.Start();
}

Task.WaitAll(tasklist.ToArray());
pointInSquare = 0;
pointInCircle = 0;
for (int i = 0; i < numThreads; i++)
{
    var (incircle, insquare) = tasklist[i].Result;
    pointInCircle += incircle;
    pointInSquare += insquare;
}
pi = 4.0 * pointInCircle / pointInSquare;
stopwatch.Stop();

Console.WriteLine($"Parallel 2 Pi = {pi}  Time elapse = {stopwatch.Elapsed.TotalSeconds}sec  run={pointInSquare}");

// using async and await 
var newproc = async(long cnt) => 
{
    int inCircle = 0;
    int inSquare = 0;
    //Console.WriteLine($"Async {cnt}");
    for (long j = 0; j < cnt; j++)
    {
        double x = rnd.NextDouble();
        double y = rnd.NextDouble();
        if (Math.Sqrt(x * x + y * y) <= 1.0)
        {
            inCircle++;
        }

        inSquare++;
    }

    return new Tuple<int,int>(inCircle, inSquare);
};

stopwatch.Restart();
pointInSquare = 0;
pointInCircle = 0;
rnd = new Random();
for (int i = 0; i < numThreads; i++)
{
    var (incircle, insquare) = await(newproc(innerCnt));
    pointInCircle += incircle;
    pointInSquare += insquare;
}
pi = 4.0 * pointInCircle / pointInSquare;
stopwatch.Stop();

Console.WriteLine($"Parallel 3 Pi = {pi}  Time elapse = {stopwatch.Elapsed.TotalSeconds}sec  run={pointInSquare}");
