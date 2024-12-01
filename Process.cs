namespace BankersAlgorithm;

public class Process
{
    public int Number { get; set; }
    public int ResourceCount { get; set; }
    public bool IsFinished { get; set; }
    public decimal[] Allocation { get; set; }
    public decimal[] Maximum { get; set; }
    public decimal[] Needs
    {
        get => new int[ResourceCount]
        .Select((n, i) => Maximum[i] - Allocation[i])
        .ToArray();
    }

    public Process(int number, int resourceCount, int randomMax = 10)
    {
        Number = number;
        Allocation = new decimal[resourceCount];
        Maximum = new decimal[resourceCount];
        ResourceCount = resourceCount;

        Random random = new Random();
        for (int i = 0; i < resourceCount; i++)
        {
            Maximum[i] = random.Next(1, randomMax + 1);
            Allocation[i] = random.Next(1, (int)Maximum[i] + 1);
        }
    }

    public Process(int number, decimal[] allocation, decimal[] maximum)
    {
        Number = number;
        Allocation = allocation;
        Maximum = maximum;
    }

    public override string ToString()
    {
        int len = ResourceCount * 2;
        return $"P{Number,-5}{string.Join(" ", Allocation.Select(n => $"{n:0}")).PadRight(len + 10)} " +
            $"{string.Join(" ", Maximum.Select(n => $"{n:0}")).PadRight(len + 7)} " +
            $"{string.Join(" ", Needs.Select(n => $"{n:0}")).PadRight(len + 5)}";
    }
}
