// <copyright file="BasicTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Tests;

/// <summary>
/// Test basic rearch functionality.
/// </summary>
public class BasicTest
{
    /// <summary>
    /// Basic count example.
    /// </summary>
    [Fact]
    public void BasicCountExample()
    {
        int Count(ICapsuleHandle _) => 0;
        int CountPlusOne(ICapsuleHandle use) => use.Call(Count) + 1;

        using var container = new Container();
        Assert.Equal(0, container.Read(Count));
        Assert.Equal(1, container.Read(CountPlusOne));
    }

    [Fact]
    public void StateUpdatesForStatefulCapsule()
    {
        (int, Action<int>) Stateful(ICapsuleHandle use) => use.State(0);
        int PlusOne(ICapsuleHandle use) => use.Call(Stateful).Item1 + 1;

        using var container = new Container();

        {
            var (state, setState) = container.Read(Stateful);
            Assert.Equal(0, state);
            setState(1);
        }

        {
            var (state, setState) = container.Read(Stateful);
            Assert.Equal(1, state);
            setState(2);
            setState(3);
        }

        {
            var (state, _) = container.Read(Stateful);
            Assert.Equal(3, state);
        }
    }

    [Fact]
    public void StateUpdatesForDependentCapsule()
    {
        (int, Action<int>) Stateful(ICapsuleHandle use) => use.State(0);
        int PlusOne(ICapsuleHandle use) => use.Call(Stateful).Item1 + 1;

        using var container = new Container();

        {
            var (state, setState) = container.Read(Stateful);
            var statefulPlusOne = container.Read(PlusOne);
            Assert.Equal(0, state);
            Assert.Equal(1, statefulPlusOne);
            setState(1);
        }

        {
            var (state, _) = container.Read(Stateful);
            var statefulPlusOne = container.Read(PlusOne);
            Assert.Equal(1, state);
            Assert.Equal(2, statefulPlusOne);
        }
    }
}
