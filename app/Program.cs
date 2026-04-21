namespace app;

using Math;
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var sum = Math.Add(2, 3);
        Console.WriteLine($"The sum is: {sum}");
    }
}

