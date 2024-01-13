// <copyright file="BasicTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

using Newtonsoft.Json.Bson;

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

    [Fact]
    public void MultipleSideEffects()
    {
        ((int, Action<int>), (int, Action<int>)) Multi(
            ICapsuleHandle use)
        {
            return (use.State(0), use.State(1));
        }

        using var container = new Container();

        {
            var ((s1, set1), (s2, set2)) = container.Read(Multi);
            Assert.Equal(0, s1);
            Assert.Equal(1, s2);
            set1(1);
            set2(2);
        }

        {
            var ((s1, set1), (s2, set2)) = container.Read(Multi);
            Assert.Equal(1, s1);
            Assert.Equal(2, s2);
        }
    }

    [Fact]
    public void ListenerGetsUpdates()
    {
        (int, Action<int>) Stateful(ICapsuleHandle use) => use.State(0);

        using var container = new Container();
        void SetState(int state) => container.Read(Stateful).Item2(state);

        List<int> states = [];
        void Listener(ICapsuleReader use) => states.Add(use.Call(Stateful).Item1);

        SetState(1);
        var handle1 = container.Listen(Listener);
        SetState(2);
        SetState(3);

        handle1.Dispose();
        SetState(4);

        SetState(5);
        var handle2 = container.Listen(Listener);
        SetState(6);
        SetState(7);

        handle2.Dispose();
        SetState(8);

        Assert.Equal([1, 2, 3, 5, 6, 7], states);
    }
}
