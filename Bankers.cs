namespace BankersAlgorithm;

public class Bankers
{
    private bool isSafe;
    private List<Process> processes;
    private List<decimal[]> available;
    private List<string> safeSequence;
    private decimal[] totalSystemResources => new decimal[processes.First().ResourceCount]
        .Select((n, i) => processes.Sum(p => p.Allocation[i]))
        .Select((n, i) => n + available.First()[i])
        .ToArray();

    public void ResourceRequest(int processNumber, decimal[] resources)
    {
        // @Mark Rodney Tan
        // Check here if the request is valid
        // then do the necessary computations
        // search for the formulas in the web
        // add the code in here before SafeStateCheck()

        SafeStateCheck();
        Console.WriteLine($"The system is {(isSafe ? "in" : "not in")} a safe state after the request");
        Console.WriteLine($"Therefore, the request should {(isSafe ? "be" : "not be")} granted");
        Console.WriteLine();

        if (!isSafe)
        {
            // @Mark Rodney Tan
            // Revert the original values for needs, allocation, and available
            // of the requesting process
            // use the List<Process> processes, List<decimal[]> available, 
            // and List<string> safeSequence fields to update the values
            // the allocation, needs and maximum is in the Process class
            // add the code in here, before PrintMatrix()

            PrintMatrix();
        }
    }

    public void SafeStateCheck()
    {
        safeSequence = new List<string>();
        Console.WriteLine("\nBefore Safe State Check:");
        PrintMatrix();

        // @John Cleford Ricafranca
        // Check here if the needs of the processes can be satisfied
        // with the current work available, and update the
        // safe sequence, available resources, and isSafe 
        // use the List<Process> processes, List<decimal[]> available, 
        // and List<string> safeSequence, isSafe fields to update the values
        // the allocation, needs and maximum is in the Process class
        // add the code in here, before the After Safe State Check

        Console.WriteLine("\nAfter Safe State Check:");
        PrintMatrix();

        Console.WriteLine("Safe Sequence: " + (safeSequence.Count == 0 ? "" : string.Join(" -> ", safeSequence)));
        Console.WriteLine($"The system is {(isSafe ? "in" : "not in")} a safe state\n");
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

        // Initialize the values needed for the Banker's Algorithm
        // either by user input or random generation of values
        GenerateProcesses(numberOfProcesses, numberOfResourceTypes, shouldRandomize);

        // Now check if the system is in a safe state
        SafeStateCheck();

        // Ask user if they want to request resource for the current state if 
        // the system is in a safe state, do another safe state check
        // or else exit the program
        string options = $"{(isSafe ? "Resource Request [R]\n" : "")}Do another Safe State Check [S]\nExit [E]";
        string choice = "";
        do Console.Write($"What do you want to do:\n{options}\nSelect an option: ");
        while (!"R,S,E".Split(",").Contains(choice = Console.ReadLine().ToUpper()));
        Console.Write("\n\n");

        switch (choice)
        {
            case "R":
                int processNumber = 0;
                do Console.Write("Specify the process number to request: ");
                while (!int.TryParse(Console.ReadLine(), out processNumber));

                decimal[] resources = new decimal[numberOfResourceTypes];
                for (int i = 0; i < numberOfResourceTypes; i++)
                {
                    do Console.Write($"Specify the resources for resource type {i + 1} ({numberOfResourceTypes} values): ");
                    while (!decimal.TryParse(Console.ReadLine(), out resources[i]));
                }

                ResourceRequest(processNumber, resources);
                break;

            case "S":
                Prompt();
                break;

            case "E": Console.Write("Exit"); break;
        }
    }

    public void GenerateProcesses(int numberOfProcesses, int numberOfResourceTypes, bool shouldRandomize = false)
    {
        available = [new decimal[numberOfResourceTypes]];
        bool isAvailableValid = false;
        do
        {
            Console.Write($"Specify the available ({numberOfResourceTypes} values): ");
            available[0] = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(decimal.Parse).ToArray();
            isAvailableValid = available[0].Length == numberOfResourceTypes;
            foreach (decimal value in available[0])
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

        processes = new List<Process>();
        for (int i = 0; i < numberOfProcesses; i++)
        {
            if (shouldRandomize) processes.Add(new Process(i, numberOfResourceTypes));
            else
            {
                decimal[] allocation = [];
                bool isAllocationValid = false;
                do
                {
                    Console.Write($"Specify the allocation for process {i + 1} ({numberOfResourceTypes} values): ");
                    allocation = Console.ReadLine().Split(" ").Select(decimal.Parse).ToArray();

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
                    Console.Write($"Specify the maximum for process {i + 1} ({numberOfResourceTypes} values): ");
                    maximum = Console.ReadLine().Split(" ").Select(decimal.Parse).ToArray();

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

                processes.Add(new Process(i, allocation, maximum));
            }
        }
    }

    public void PrintMatrix()
    {
        int len = processes.First().ResourceCount * 2;
        Console.WriteLine($"{"P",-5} {"Allocation".PadRight(len + 10)} {"Maximum".PadRight(len + 7)} {"Needs".PadRight(len + 5)} {"Available".PadRight(len + 9)}\n");
        for (int i = 0; i < processes.Count; i++) Console.WriteLine($"{processes[i]} {(i < available.Count ? string.Join(" ", available[i].Select(n => $"{n:0}")) : "")}");
        if (available.Count > processes.Count) Console.WriteLine($"{string.Join(" ", available[processes.Count].Select(n => $"{n:0}"))}");
        Console.WriteLine($"Total System Resources: {string.Join(" ", totalSystemResources.Select(n => $"{n:0}"))}");
    }
}
