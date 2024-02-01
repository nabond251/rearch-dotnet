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
        int Count(ICapsuleHandle x) => 0;
        int CountPlusOne(ICapsuleHandle use) => use.Call(Count) + 1;

        using var container = new Container();
        Assert.Equal(0, container.Read(Count));
        Assert.Equal(1, container.Read(CountPlusOne));
    }

    /// <summary>
    /// Test state updates for stateful capsule.
    /// </summary>
    [Fact]
    public void StateUpdatesForStatefulCapsule()
    {
        static (int, Action<int>) Stateful(ICapsuleHandle use) => use.State(0);

        using var container = new Container();

        static void Command1(Container container)
        {
            var (state, setState) = container.Read(Stateful);
            Assert.Equal(0, state);
            setState(1);
        }

        static void Query1Command23(Container container)
        {
            var (state, setState) = container.Read(Stateful);
            Assert.Equal(1, state);
            setState(2);
            setState(3);
        }

        static void Query3(Container container)
        {
            var (state, _) = container.Read(Stateful);
            Assert.Equal(3, state);
        }

        Command1(container);
        Query1Command23(container);
        Query3(container);
    }

    /// <summary>
    /// Tests state updates for dependent capsule.
    /// </summary>
    [Fact]
    public void StateUpdatesForDependentCapsule()
    {
        (int, Action<int>) Stateful(ICapsuleHandle use) => use.State(0);
        int PlusOne(ICapsuleHandle use) => use.Call(Stateful).Item1 + 1;

        using var container = new Container();

        void Command(Container container)
        {
            var (state, setState) = container.Read(Stateful);
            var statefulPlusOne = container.Read(PlusOne);
            Assert.Equal(0, state);
            Assert.Equal(1, statefulPlusOne);
            setState(1);
        }

        void Query(Container container)
        {
            var (state, _) = container.Read(Stateful);
            var statefulPlusOne = container.Read(PlusOne);
            Assert.Equal(1, state);
            Assert.Equal(2, statefulPlusOne);
        }

        Command(container);
        Query(container);
    }

    /// <summary>
    /// Tests multiple side effects.
    /// </summary>
    [Fact]
    public void MultipleSideEffects()
    {
        static ((int, Action<int>), (int, Action<int>)) Multi(
            ICapsuleHandle use)
        {
            return (use.State(0), use.State(1));
        }

        using var container = new Container();

        static void Command(Container container)
        {
            var ((s1, set1), (s2, set2)) = container.Read(Multi);
            Assert.Equal(0, s1);
            Assert.Equal(1, s2);
            set1(1);
            set2(2);
        }

        static void Query(Container container)
        {
            var ((s1, _), (s2, _)) = container.Read(Multi);
            Assert.Equal(1, s1);
            Assert.Equal(2, s2);
        }

        Command(container);
        Query(container);
    }

    /// <summary>
    /// Test listener gets updates.
    /// </summary>
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

    /// <summary>
    /// Test == check skips unneeded rebuilds.
    /// </summary>
    [Fact]
    public void EqualsCheckSkipsUnneededRebuilds()
    {
        Dictionary<object, int> builds = [];

        (int, Action<int>) Stateful(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)Stateful))
            {
                builds[(object)Stateful] = 1;
            }
            else
            {
                builds[(object)Stateful] = builds[(object)Stateful] + 1;
            }

            return use.State(0);
        }

        int UnchangingSuperPureDep(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)UnchangingSuperPureDep))
            {
                builds[(object)UnchangingSuperPureDep] = 1;
            }
            else
            {
                builds[(object)UnchangingSuperPureDep] = builds[(object)UnchangingSuperPureDep] + 1;
            }

            use.Call(Stateful);
            return 0;
        }

        int UnchangingWatcher(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)UnchangingWatcher))
            {
                builds[(object)UnchangingWatcher] = 1;
            }
            else
            {
                builds[(object)UnchangingWatcher] = builds[(object)UnchangingWatcher] + 1;
            }

            return use.Call(UnchangingSuperPureDep);
        }

        int ChangingSuperPureDep(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)ChangingSuperPureDep))
            {
                builds[(object)ChangingSuperPureDep] = 1;
            }
            else
            {
                builds[(object)ChangingSuperPureDep] = builds[(object)ChangingSuperPureDep] + 1;
            }

            return use.Call(Stateful).Item1;
        }

        int ChangingWatcher(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)ChangingWatcher))
            {
                builds[(object)ChangingWatcher] = 1;
            }
            else
            {
                builds[(object)ChangingWatcher] = builds[(object)ChangingWatcher] + 1;
            }

            return use.Call(ChangingSuperPureDep);
        }

        object ImpureSink(ICapsuleHandle use)
        {
            use.Register(_ => new object());
            use.Call(ChangingWatcher);
            use.Call(UnchangingWatcher);

            return new object();
        }

        using var container = new Container();

        Assert.Equal(0, container.Read(UnchangingWatcher));
        Assert.Equal(0, container.Read(ChangingWatcher));
        Assert.Equal(1, builds[(object)Stateful]);
        Assert.Equal(1, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(1, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(1, builds[(object)UnchangingWatcher]);
        Assert.Equal(1, builds[(object)ChangingWatcher]);

        container.Read(Stateful).Item2(0);
        Assert.Equal(2, builds[(object)Stateful]);
        Assert.Equal(1, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(1, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(1, builds[(object)UnchangingWatcher]);
        Assert.Equal(1, builds[(object)ChangingWatcher]);

        Assert.Equal(0, container.Read(UnchangingWatcher));
        Assert.Equal(0, container.Read(ChangingWatcher));
        Assert.Equal(2, builds[(object)Stateful]);
        Assert.Equal(1, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(1, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(1, builds[(object)UnchangingWatcher]);
        Assert.Equal(1, builds[(object)ChangingWatcher]);

        container.Read(Stateful).Item2(1);
        Assert.Equal(3, builds[(object)Stateful]);
        Assert.Equal(1, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(1, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(1, builds[(object)UnchangingWatcher]);
        Assert.Equal(1, builds[(object)ChangingWatcher]);

        Assert.Equal(0, container.Read(UnchangingWatcher));
        Assert.Equal(1, container.Read(ChangingWatcher));
        Assert.Equal(3, builds[(object)Stateful]);
        Assert.Equal(2, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(2, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(2, builds[(object)UnchangingWatcher]);
        Assert.Equal(2, builds[(object)ChangingWatcher]);

        // Disable the super pure gc
        container.Read(ImpureSink);

        container.Read(Stateful).Item2(2);
        Assert.Equal(4, builds[(object)Stateful]);
        Assert.Equal(3, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(3, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(2, builds[(object)UnchangingWatcher]);
        Assert.Equal(3, builds[(object)ChangingWatcher]);

        Assert.Equal(0, container.Read(UnchangingWatcher));
        Assert.Equal(2, container.Read(ChangingWatcher));
        Assert.Equal(4, builds[(object)Stateful]);
        Assert.Equal(3, builds[(object)UnchangingSuperPureDep]);
        Assert.Equal(3, builds[(object)ChangingSuperPureDep]);
        Assert.Equal(2, builds[(object)UnchangingWatcher]);
        Assert.Equal(3, builds[(object)ChangingWatcher]);
    }

    /// <summary>
    /// Tests complex dependency graph.
    /// </summary>
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
        Dictionary<object, int> builds = [];

        (int, Action<int>) A(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)A))
            {
                builds[(object)A] = 1;
            }
            else
            {
                builds[(object)A] = builds[(object)A] + 1;
            }

            return use.State(0);
        }

        int B(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)B))
            {
                builds[(object)B] = 1;
            }
            else
            {
                builds[(object)B] = builds[(object)B] + 1;
            }

            use.Register(_ => new object());
            return use.Call(A).Item1;
        }

        int H(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)H))
            {
                builds[(object)H] = 1;
            }
            else
            {
                builds[(object)H] = builds[(object)H] + 1;
            }

            return 1;
        }

        int E(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)E))
            {
                builds[(object)E] = 1;
            }
            else
            {
                builds[(object)E] = builds[(object)E] + 1;
            }

            return use.Call(A).Item1 + use.Call(H);
        }

        int F(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)F))
            {
                builds[(object)F] = 1;
            }
            else
            {
                builds[(object)F] = builds[(object)F] + 1;
            }

            use.Register(_ => new object());
            return use.Call(E);
        }

        int C(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)C))
            {
                builds[(object)C] = 1;
            }
            else
            {
                builds[(object)C] = builds[(object)C] + 1;
            }

            return use.Call(B) + use.Call(F);
        }

        int D(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)D))
            {
                builds[(object)D] = 1;
            }
            else
            {
                builds[(object)D] = builds[(object)D] + 1;
            }

            return use.Call(C);
        }

        int G(ICapsuleHandle use)
        {
            if (!builds.ContainsKey((object)G))
            {
                builds[(object)G] = 1;
            }
            else
            {
                builds[(object)G] = builds[(object)G] + 1;
            }

            return use.Call(C) + use.Call(F);
        }

        using var container = new Container();
        Assert.Empty(builds);

        Assert.Equal(1, container.Read(D));
        Assert.Equal(2, container.Read(G));
        Assert.Equal(1, builds[(object)A]);
        Assert.Equal(1, builds[(object)B]);
        Assert.Equal(1, builds[(object)C]);
        Assert.Equal(1, builds[(object)D]);
        Assert.Equal(1, builds[(object)E]);
        Assert.Equal(1, builds[(object)F]);
        Assert.Equal(1, builds[(object)G]);
        Assert.Equal(1, builds[(object)H]);

        container.Read(A).Item2(0);
        Assert.Equal(1, container.Read(D));
        Assert.Equal(2, container.Read(G));
        Assert.Equal(2, builds[(object)A]);
        Assert.Equal(1, builds[(object)B]);
        Assert.Equal(1, builds[(object)C]);
        Assert.Equal(1, builds[(object)D]);
        Assert.Equal(1, builds[(object)E]);
        Assert.Equal(1, builds[(object)F]);
        Assert.Equal(1, builds[(object)G]);
        Assert.Equal(1, builds[(object)H]);

        container.Read(A).Item2(1);
        Assert.Equal(3, builds[(object)A]);
        Assert.Equal(2, builds[(object)B]);
        Assert.Equal(1, builds[(object)C]);
        Assert.Equal(1, builds[(object)D]);
        Assert.Equal(2, builds[(object)E]);
        Assert.Equal(2, builds[(object)F]);
        Assert.Equal(1, builds[(object)G]);
        Assert.Equal(1, builds[(object)H]);

        Assert.Equal(3, container.Read(D));
        Assert.Equal(5, container.Read(G));
        Assert.Equal(3, builds[(object)A]);
        Assert.Equal(2, builds[(object)B]);
        Assert.Equal(2, builds[(object)C]);
        Assert.Equal(2, builds[(object)D]);
        Assert.Equal(2, builds[(object)E]);
        Assert.Equal(2, builds[(object)F]);
        Assert.Equal(2, builds[(object)G]);
        Assert.Equal(1, builds[(object)H]);
    }
}
