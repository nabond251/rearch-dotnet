// <copyright file="BasicTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

using Newtonsoft.Json.Bson;
using System.Runtime.Intrinsics.X86;

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

    // We use a more sophisticated graph here for a more thorough
    // test of all functionality
    //
    // -> A -> B -> C -> D
    //      \      / \
    //  H -> E -> F -> G
    //
    // C, D, E, G, H are super pure. A, B, F are not.
    [Fact]
    public void ComplexDependencyGraph()
    {
        Dictionary<Func<ICapsuleHandle, object?>, int> builds = [];

        object? A(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(A))
            {
                builds[A] = 1;
            }
            else
            {
                builds[A]++;
            }

            return use.State(0);
        }

        object? B(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(B))
            {
                builds[B] = 1;
            }
            else
            {
                builds[B]++;
            }

            use.Register(_ => new object());
            return (((int, Action<int>))use.Call(A)).Item1;
        }

        object? H(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(H))
            {
                builds[H] = 1;
            }
            else
            {
                builds[H]++;
            }

            return 1;
        }

        object? E(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(E))
            {
                builds[E] = 1;
            }
            else
            {
                builds[E]++;
            }

            return (((int, Action<int>))use.Call(A)).Item1 + (int)use.Call(H);
        }

        object? F(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(F))
            {
                builds[F] = 1;
            }
            else
            {
                builds[F]++;
            }

            use.Register(_ => new object());
            return use.Call(E);
        }

        object? C(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(C))
            {
                builds[C] = 1;
            }
            else
            {
                builds[C]++;
            }

            return (int)use.Call(B) + (int)use.Call(F);
        }

        object? D(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(D))
            {
                builds[D] = 1;
            }
            else
            {
                builds[D]++;
            }

            return use.Call(C);
        }

        object? G(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(G))
            {
                builds[G] = 1;
            }
            else
            {
                builds[G]++;
            }

            return (int)use.Call(C) + (int)use.Call(F);
        }

        using var container = new Container();
        Assert.True(builds.Count == 0);

        Assert.Equal(1, (int)container.Read(D));
        Assert.Equal(2, (int)container.Read(G));
        Assert.Equal(1, builds[A]);
        Assert.Equal(1, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(1, builds[E]);
        Assert.Equal(1, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        (((int, Action<int>))container.Read(A)).Item2(0);
        Assert.Equal(1, (int)container.Read(D));
        Assert.Equal(2, (int)container.Read(G));
        Assert.Equal(2, builds[A]);
        Assert.Equal(1, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(1, builds[E]);
        Assert.Equal(1, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        (((int, Action<int>))container.Read(A)).Item2(1);
        Assert.Equal(3, builds[A]);
        Assert.Equal(2, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(2, builds[E]);
        Assert.Equal(2, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        Assert.Equal(3, (int)container.Read(D));
        Assert.Equal(5, (int)container.Read(G));
        Assert.Equal(3, builds[A]);
        Assert.Equal(2, builds[B]);
        Assert.Equal(2, builds[C]);
        Assert.Equal(2, builds[D]);
        Assert.Equal(2, builds[E]);
        Assert.Equal(2, builds[F]);
        Assert.Equal(2, builds[G]);
        Assert.Equal(1, builds[H]);
    }
}
