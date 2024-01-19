// <copyright file="Example.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Example;

using Rearch;
using Xunit;

/// <summary>
/// Usage example implementation.
/// </summary>
public static class Example
{
    /// <summary>
    /// Represents a manager of the count;
    /// i.e., has the count and a way to manage that count.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Count state and setter.</returns>
    public static (int Count, Action<int> SetCount) CountManager(ICapsuleHandle use) => use.State(0);

    /// <summary>
    /// Provides a function that increments the count.
    /// Note: normally this would just be done with the <c>countManager</c> capsule;
    /// it is separate in this example to demonstrate capsule composability.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Count incrementer.</returns>
    public static Action CountIncrementer(ICapsuleHandle use)
    {
        var (count, setCount) = use.Call(CountManager);
        return () => setCount(count + 1);
    }

    /// <summary>
    /// Provides the current count.
    /// Note: normally this would just be done with the <c>countManager</c> capsule;
    /// it is separate in this example to demonstrate capsule composability.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Count.</returns>
    public static int Count(ICapsuleHandle use) => use.Call(CountManager).Count;

    /// <summary>
    /// Provides the current count plus one.
    /// This helps showcase rearch's reactivity.
    /// </summary>
    /// <param name="use">Capsule handle.</param>
    /// <returns>Count plus one.</returns>
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
