// <copyright file="ExampleTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Example;

/// <summary>
/// Test example.
/// </summary>
public class ExampleTest
{
    /// <summary>
    /// Test that example runs.
    /// </summary>
    [Fact]
    public void MainFunctionRunsCorrectly()
    {
        using var container = new CapsuleContainer();

        Assert.True(
            container.Read(Count) == 0,
            "Count should start at 0");
        Assert.True(
          container.Read(CountPlusOne) == 1,
          "CountPlusOne should start at 1");

        var incrementCount = container.Read(CountIncrementer);
        incrementCount();

        Assert.True(
          container.Read(Count) == 1,
          "Count should be 1 after count increment");
        Assert.True(
          container.Read(CountPlusOne) == 2,
          "CountPlusOne should be 2 after count increment");
    }

    private static (int Count, Action<int> SetCount) CountManager(
        ICapsuleHandle use) =>
        use.State(0);

    private static Action CountIncrementer(ICapsuleHandle use)
    {
        var (count, setCount) = use.Invoke(CountManager);
        return () => setCount(count + 1);
    }

    private static int Count(ICapsuleHandle use) =>
        use.Invoke(CountManager).Count;

    private static int CountPlusOne(ICapsuleHandle use) =>
        use.Invoke(Count) + 1;
}
