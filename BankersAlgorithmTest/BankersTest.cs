using BankersAlgorithm;

namespace BankersAlgorithmTest;

public class BankersTest
{
    [Theory]
    [MemberData(nameof(SafeStateCorrectValues))]
    public void SafeStateCheck_WhenNoDeadlocks_ReturnsCorrectValues(Bankers bankers, List<string> safeSequence)
    {
        bankers.SafeStateCheck();
        Assert.True(bankers.IsSafe);
        Assert.Equal(safeSequence, bankers.SafeSequence);
        Assert.Equal(bankers.TotalSystemResources, bankers.Available.Last());
    }

    [Theory]
    [MemberData(nameof(UnsafeStateCorrectValues))]
    public void SafeStateCheck_WhenDeadlock_ReturnsCorrectValues(Bankers bankers, List<string> safeSequence)
    {
        bankers.SafeStateCheck();
        Assert.False(bankers.IsSafe);
        Assert.Equal(safeSequence, bankers.SafeSequence);
        Assert.NotEqual(bankers.TotalSystemResources, bankers.Available.Last());
    }

    [Theory]
    [MemberData(nameof(NoDeadlocksRequestData))]
    public void ResourceRequest_WhenNoDeadlocks_ReturnsCorrectValues(
        Bankers bankers, int number, decimal[] request,
        decimal[] available, decimal[] needs, decimal[] allocation)
    {
        bankers.SafeStateCheck();
        bankers.ResourceRequest(number, request);
        Assert.True(bankers.IsSafe);
        Assert.Equal(available, bankers.Available[0]);
        Assert.Equal(needs, bankers.Processes[number].Needs);
        Assert.Equal(allocation, bankers.Processes[number].Allocation);
    }

    [Theory]
    [MemberData(nameof(WithDeadlocksRequestData))]
    public void ResourceRequest_WithDeadlocks_ReturnsCorrectValues(
        Bankers bankers, int number, decimal[] request,
        decimal[] available, decimal[] needs, decimal[] allocation)
    {
        bankers.SafeStateCheck();
        bankers.ResourceRequest(number, request);
        Assert.False(bankers.IsSafe);
        Assert.Equal(available, bankers.Available[0]);
        Assert.Equal(needs, bankers.Processes[number].Needs);
        Assert.Equal(allocation, bankers.Processes[number].Allocation);
    }

    [Theory]
    [MemberData(nameof(WithInvalidRequestData))]
    public void ResourceRequest_WithInvalidRequest_ReturnsCorrectValues(
       Bankers bankers, int number, decimal[] request,
       decimal[] available, decimal[] needs, decimal[] allocation)
    {
        bankers.SafeStateCheck();
        bankers.ResourceRequest(number, request);
        Assert.True(bankers.IsSafe);
        Assert.Equal(available, bankers.Available[0]);
        Assert.Equal(needs, bankers.Processes[number].Needs);
        Assert.Equal(allocation, bankers.Processes[number].Allocation);
    }

    public static List<Bankers> SafeSequenceBankersData =>
        new List<Bankers>()
        {
                new Bankers() {
                    Available = new List<decimal[]>() { new decimal[] { 3, 3, 2 } },
                    Processes = new List<Process>()
                    {
                        new Process(0, [2,1,1], [7,5,3]),
                        new Process(1, [2,1,1], [3,2,2]),
                        new Process(2, [3,0,2], [9,0,2]),
                        new Process(3, [2,1,1], [2,2,2]),
                        new Process(4, [2,1,2], [4,3,3]),
                    }
                },
                new Bankers() {
                    Available = new List<decimal[]>() { new decimal[] { 1, 2, 3 } },
                    Processes = new List<Process>()
                    {
                        new Process(0, [1,1,1], [6,7,8]),
                        new Process(1, [1,1,1], [5,6,7]),
                        new Process(2, [1,1,1], [4,5,6]),
                        new Process(3, [1,1,1], [3,4,5]),
                        new Process(4, [1,1,1], [1,2,3]),
                    }
                },
        };

    public static List<Bankers> UnsafeSequenceBankersData =>
        new List<Bankers>
        {
                new Bankers() {
                    Available = new List<decimal[]>() { new decimal[] { 1, 2, 3 } },
                    Processes = new List<Process>()
                    {
                        new Process(0, [1,1,1], [8,9,10]),
                        new Process(1, [1,1,1], [6,7,8]),
                        new Process(2, [1,1,1], [5,6,7]),
                        new Process(3, [1,1,1], [4,5,6]),
                        new Process(4, [1,1,1], [3,4,5]),
                        new Process(5, [1,1,1], [1,2,3]),
                    }
                },
        };

    public static IEnumerable<object[]> SafeStateCorrectValues =>
        new List<object[]>
        {
                new object[] { SafeSequenceBankersData[0],
                    new List<string>{"P1", "P3", "P4", "P0", "P2"} },
                new object[] { SafeSequenceBankersData[1],
                    new List<string>{"P4", "P3", "P2", "P1", "P0"} },
        };

    public static IEnumerable<object[]> UnsafeStateCorrectValues =>
        new List<object[]>
        {
                new object[] { UnsafeSequenceBankersData[0],
                    new List<string>{"P5","P4", "P3", "P2", "P1"} },
        };

    public static IEnumerable<object[]> NoDeadlocksRequestData =>
        new List<object[]>
        {
                new object[] { SafeSequenceBankersData[0], 1, new decimal[] {0, 1, 1}, new decimal[] {3, 2, 1},
                    new decimal[] {1, 0, 0}, new decimal[] {2, 2, 2 } },
        };

    public static IEnumerable<object[]> WithDeadlocksRequestData =>
        new List<object[]>
        {
                new object[] {  SafeSequenceBankersData[0], 0, new decimal[] {1, 3, 2}, new decimal[] {3, 3, 2},
                    new decimal[] {5, 4, 2}, new decimal[] {2, 1, 1 } },
                new object[] { SafeSequenceBankersData[1], 0, new decimal[] {1, 0, 0}, new decimal[] {1, 2, 3},
                    new decimal[] {5, 6, 7 }, new decimal[] {1, 1, 1} },
        };

    public static IEnumerable<object[]> WithInvalidRequestData =>
        new List<object[]>
        {
                new object[] {  SafeSequenceBankersData.ElementAt(0), 2, new decimal[] {5, 2, 0}, new decimal[] {3, 3, 2},
                    new decimal[] {6, 0, 0}, new decimal[] {3, 0, 2 } },
        };
}
