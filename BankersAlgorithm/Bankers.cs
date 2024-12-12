namespace BankersAlgorithm;

public class Bankers
{
    public bool IsSafe { get; set; }
    public List<Process> Processes { get; set; }
    public List<decimal[]> Available { get; set; }
    public List<string> SafeSequence { get; set; }
    public decimal[] TotalSystemResources
    {
        get => new decimal[Processes.First().ResourceCount]
            .Select((n, i) => Processes.Sum(p => p.Allocation[i]))
            .Select((n, i) => n + Available.First()[i])
            .ToArray();
    }

    public void ResourceRequest(int processNumber, decimal[] request)
    {
        Console.WriteLine($"\nP{processNumber} is requesting: {PrintResources(request)}");

        bool isLessThanNeeds = Processes[processNumber].Needs.Zip(request, (n, r) => r <= n).All(b => b);
        Console.WriteLine("\nRequest <= Needs");
        Console.WriteLine($"{PrintResources(request)} <= {PrintResources(Processes[processNumber].Needs)} ? {isLessThanNeeds}");

        Console.WriteLine("\nRequest <= Available");
        bool isLessThanAvailable = Available[0].Zip(request, (n, r) => r <= n).All(b => b);

        Console.WriteLine($"{PrintResources(request)} <= {PrintResources(Available[0])} ? {isLessThanAvailable}");

        if (!isLessThanNeeds || !isLessThanAvailable)
        {
            Console.WriteLine("\nInvalid request");
            return;
        }

        List<decimal[]> originalAvailable = Available;
        List<string> originalSafeSequence = SafeSequence;
        var originalNeeds = Processes[processNumber].Needs;
        var originalAllocation = Processes[processNumber].Allocation;

        Processes[processNumber].Allocation = Processes[processNumber].Allocation.Select((n, i) => n + request[i]).ToArray();
        Processes[processNumber].Needs = Processes[processNumber].Needs.Select((n, i) => n - request[i]).ToArray();
        Available = [originalAvailable[0].Select((n, i) => n - request[i]).ToArray()];

        Console.WriteLine($"\nAvailable = {PrintResources(originalAvailable[0])} - " +
            $"{PrintResources(request)} = {PrintResources(Available[0])}");

        // fix the needs, not updating maybe because of the getter
        Console.WriteLine($"P{processNumber} Needs = {PrintResources(originalNeeds)} - " +
            $"{PrintResources(request)} = {PrintResources(Processes[processNumber].Needs)}");

        Console.WriteLine($"P{processNumber} Allocation = {PrintResources(originalAllocation)} + " +
            $"{PrintResources(request)} = {PrintResources(Processes[processNumber].Allocation)}");

        SafeStateCheck();

        Console.WriteLine($"The system is {(IsSafe ? "in" : "not in")} a safe state after the request");
        Console.WriteLine($"Therefore, the request should {(IsSafe ? "be" : "not be")} granted\n");

        if (!IsSafe)
        {
            Console.WriteLine("New Available = Available + Request");
            Console.WriteLine($"Available = {PrintResources(Available[0])} + " +
                $"{PrintResources(request)} = {PrintResources(originalAvailable[0])}");
            Available = originalAvailable;

            Console.WriteLine($"\nNew P{processNumber} Needs = Needs + Request");
            Console.WriteLine($"P{processNumber} Needs = {PrintResources(Processes[processNumber].Needs)} + " +
                $"{PrintResources(request)} = {PrintResources(originalNeeds)}");
            Processes[processNumber].Needs = originalNeeds;

            Console.WriteLine($"\nNew P{processNumber} Allocation = Allocation - Request");
            Console.WriteLine($"P{processNumber} Allocation = {PrintResources(Processes[processNumber].Allocation)} - " +
                $"{PrintResources(request)} = {PrintResources(originalAllocation)}");
            Processes[processNumber].Allocation = originalAllocation;

            SafeSequence = originalSafeSequence;
            IsSafe = true;

            Console.WriteLine();
            PrintMatrix();
        }
    }

    public void SafeStateCheck()
    {
        SafeSequence = new List<string>();
        Console.WriteLine("\nBefore Safe State Check:");
        PrintMatrix();

        int i = 0;
        bool hasNewAvailable = false;
        List<Process> unfinished = Processes.Select(p => new Process(p.Number, p.Allocation, p.Maximum)).ToList();
        while (true)
        {
            Process process = unfinished[i];
            bool isLessThan = process.Needs.Zip(Available.Last(), (n, a) => n <= a).All(b => b);

            Console.WriteLine($"\nP{process.Number} Needs <= Available");
            Console.WriteLine($"{PrintResources(process.Needs)} <= {PrintResources(Available.Last())} ? {isLessThan}");
            if (isLessThan)
            {
                Available.Add(new decimal[process.ResourceCount].Select((n, j) => Available.Last()[j] + process.Allocation[j]).ToArray());
                SafeSequence.Add($"P{process.Number}");
                unfinished.Remove(process);
                hasNewAvailable = true;
                i--;

                Console.WriteLine($"\nAvailable = Available + P{process.Number} Allocation");
                Console.WriteLine($"Available = {PrintResources(Available[Available.Count - 2])} + {PrintResources(process.Allocation)} = {PrintResources(Available.Last())}");
                Console.WriteLine($"Safe Sequence: {string.Join(" -> ", SafeSequence)}");
            }
            else Console.WriteLine("Proceed to next process");

            if (i >= unfinished.Count - 1 || i < 0)
            {
                if (!hasNewAvailable || unfinished.Count == 0) break;
                hasNewAvailable = false;
                i = 0;
            }
            else i++;
        }
        IsSafe = unfinished.Count == 0;
        if (!IsSafe) Console.WriteLine($"\nThere is a deadlock: {string.Join(" ", unfinished.Select(i => $"P{i.Number}").ToArray())}");

        Console.WriteLine("\nAfter Safe State Check:");
        PrintMatrix();
        if (!IsSafe) Console.WriteLine($"Deadlock: {string.Join(" ", unfinished.Select(i => $"P{i.Number}").ToArray())}");
        Console.WriteLine($"The system is {(IsSafe ? "in" : "not in")} a safe state\n");
    }

    public void Prompt()
    {
        Console.WriteLine("This is an implementation of Banker's Algorithm");

        int numberOfProcesses = 0;
        do Console.Write("Specify the number of processes: ");
        while (!int.TryParse(Console.ReadLine(), out numberOfProcesses));

        int numberOfResourceTypes = 0;
        do Console.Write("Specify the number of resource types: ");
        while (!int.TryParse(Console.ReadLine(), out numberOfResourceTypes));

        string selection = "";
        do Console.Write("Should the resource values be randomized (YES/NO): ");
        while (!"YES,NO".Split(",").Contains(selection = Console.ReadLine().ToUpper()));
        bool shouldRandomize = selection == "YES";

        GenerateProcesses(numberOfProcesses, numberOfResourceTypes, shouldRandomize);

        SafeStateCheck();

        string options = $"{(IsSafe ? "Resource Request [R]\n" : "")}Do another Safe State Check [S]\nExit [E]";
        string choice = "";

        while (true)
        {
            do Console.Write($"\n\nWhat do you want to do:\n{options}\nSelect an option: ");
            while (!$"{(IsSafe ? "R," : "")}S,E".Split(",").Contains(choice = Console.ReadLine().ToUpper()));
            Console.Write("\n\n");

            switch (choice)
            {
                case "R":
                    int processNumber = 0;
                    do Console.Write("Specify the process number to request: ");
                    while (!int.TryParse(Console.ReadLine(), out processNumber));

                    decimal[] request;
                    do
                    {
                        Console.Write($"Specify the resources for request ({numberOfResourceTypes} values): ");
                        request = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToArray();
                    }
                    while (request.Length != numberOfResourceTypes);

                    ResourceRequest(processNumber, request);
                    break;

                case "S":
                    Prompt();
                    break;

                case "E":
                    Console.Write("Exit");
                    Environment.Exit(0);
                    break;
            }
        }
    }

    public void GenerateProcesses(int numberOfProcesses, int numberOfResourceTypes, bool shouldRandomize = false)
    {
        Available = [new decimal[numberOfResourceTypes]];
        bool isAvailableValid = false;
        do
        {
            Console.Write($"Specify the available ({numberOfResourceTypes} values): ");
            Available[0] = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToArray();
            isAvailableValid = Available[0].Length == numberOfResourceTypes;
            foreach (decimal value in Available[0])
            {
                if (value < 0)
                {
                    Console.WriteLine("Available values cannot be negative");
                    isAvailableValid = false;
                    break;
                }
            }
        }
        while (!isAvailableValid);

        Processes = new List<Process>();
        for (int i = 0; i < numberOfProcesses; i++)
        {
            if (shouldRandomize) Processes.Add(new Process(i, numberOfResourceTypes));
            else
            {
                decimal[] allocation = [];
                bool isAllocationValid = false;
                do
                {
                    Console.Write($"Specify the allocation for P{i} ({numberOfResourceTypes} values): ");
                    allocation = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToArray();

                    isAllocationValid = allocation.Length == numberOfResourceTypes;
                    foreach (decimal value in allocation)
                    {
                        if (value < 0)
                        {
                            Console.WriteLine("Allocation values cannot be negative\n");
                            isAllocationValid = false;
                            break;
                        }
                    }
                }
                while (!isAllocationValid);

                decimal[] maximum = [];
                bool isMaximumValid = false;
                do
                {
                    Console.Write($"Specify the maximum for P{i} ({numberOfResourceTypes} values): ");
                    maximum = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToArray();

                    isMaximumValid = maximum.Length == numberOfResourceTypes;
                    for (int j = 0; j < maximum.Length && isMaximumValid; j++)
                    {
                        if (maximum[j] < allocation[j])
                        {
                            Console.WriteLine("Maximum values cannot be lesser than allocation values\n");
                            isMaximumValid = false;
                            break;
                        }
                    }
                }
                while (!isMaximumValid);
                Processes.Add(new Process(i, allocation, maximum));
            }
        }
    }

    public void PrintMatrix()
    {
        int len = Processes.First().ResourceCount * 2;
        Console.WriteLine($"{"P",-5} {"Allocation".PadRight(len + 10)} {"Maximum".PadRight(len + 7)} {"Needs".PadRight(len + 5)} {"Available".PadRight(len + 9)}");
        for (int i = 0; i < Processes.Count; i++) Console.WriteLine($"{Processes[i]} {(i < Available.Count ? PrintResources(Available[i]) : "")}");
        if (Available.Count > Processes.Count) Console.WriteLine($"Current/Last Available: {PrintResources(Available.Last())}");
        Console.WriteLine($"Total System Resources: {PrintResources(TotalSystemResources)}");
        Console.WriteLine($"Safe Sequence: {(SafeSequence.Count == 0 ? "" : string.Join(" -> ", SafeSequence))}");
    }

    public string PrintResources(decimal[] resources)
        => string.Join(" ", resources.Select(n => $"{n:0}"));
}
