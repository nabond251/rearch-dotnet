// <copyright file="Example.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Example;

using Rearch;
using Xunit;

public static class Example
{
    /// <summary>
    /// Represents a manager of the count;
    /// i.e., has the count and a way to manage that count.
    /// </summary>
    public static (int, Action<int>) CountManager(ICapsuleHandle use) => use.State(0);

    /// <summary>
    /// Provides a function that increments the count.
    /// Note: normally this would just be done with the `countManager` capsule;
    /// it is separate in this example to demonstrate capsule composability.
    /// </summary>
    public static Action CountIncrementer(ICapsuleHandle use)
    {
        var (count, setCount) = use.Call(CountManager);
        return () => setCount(count + 1);
    }

    /// <summary>
    /// Provides the current count.
    /// Note: normally this would just be done with the `countManager` capsule;
    /// it is separate in this example to demonstrate capsule composability.
    /// </summary>
    public static int Count(ICapsuleHandle use) => use.Call(CountManager).Item1;

    /// <summary>
    /// Provides the current count plus one.
    /// This helps showcase rearch's reactivity.
    /// </summary>
    public static int CountPlusOne(ICapsuleHandle use) => use.Call(Count) + 1;

    /// <summary>
    /// Entrypoint of the application.
    /// </summary>
    public static void Main()
    {
        using var container = new Container();

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
}
