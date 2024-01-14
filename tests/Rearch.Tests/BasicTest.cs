// <copyright file="BasicTest.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Tests;

record class Integer(int Value)
{
    public static implicit operator Integer(int value) => new(value);
}

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
        Integer Count(ICapsuleHandle _) => 0;
        Integer CountPlusOne(ICapsuleHandle use) => use.Call(Count).Value + 1;

        using var container = new Container();
        Assert.Equal(0, container.Read(Count));
        Assert.Equal(1, container.Read(CountPlusOne));
    }

    [Fact]
    public void StateUpdatesForStatefulCapsule()
    {
        Tuple<Integer, Action<Integer>> Stateful(ICapsuleHandle use) => use.State(new Integer(0));
        Integer PlusOne(ICapsuleHandle use) => use.Call(Stateful).Item1.Value + 1;

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
        Tuple<Integer, Action<Integer>> Stateful(ICapsuleHandle use) => use.State(new Integer(0));
        Integer PlusOne(ICapsuleHandle use) => use.Call(Stateful).Item1.Value + 1;

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
        Tuple<Tuple<Integer, Action<Integer>>, Tuple<Integer, Action<Integer>>> Multi(
            ICapsuleHandle use)
        {
            return Tuple.Create(use.State(new Integer(0)), use.State(new Integer(1)));
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
        Tuple<Integer, Action<Integer>> Stateful(ICapsuleHandle use) => use.State(new Integer(0));

        using var container = new Container();
        void SetState(Integer state) => container.Read(Stateful).Item2(state);

        List<Integer> states = [];
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
        Dictionary<Func<ICapsuleHandle, object?>, Integer> builds = [];

        Tuple<Integer, Action<Integer>> A(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(A))
            {
                builds[A] = 1;
            }
            else
            {
                builds[A] = builds[A].Value + 1;
            }

            return use.State(new Integer(0));
        }

        Integer B(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(B))
            {
                builds[B] = 1;
            }
            else
            {
                builds[B] = builds[B].Value + 1;
            }

            use.Register(_ => new object());
            return use.Call(A).Item1;
        }

        Integer H(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(H))
            {
                builds[H] = 1;
            }
            else
            {
                builds[H] = builds[H].Value + 1;
            }

            return 1;
        }

        Integer E(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(E))
            {
                builds[E] = 1;
            }
            else
            {
                builds[E] = builds[E].Value + 1;
            }

            return use.Call(A).Item1.Value + use.Call(H).Value;
        }

        Integer F(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(F))
            {
                builds[F] = 1;
            }
            else
            {
                builds[F] = builds[F].Value + 1;
            }

            use.Register(_ => new object());
            return use.Call(E);
        }

        Integer C(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(C))
            {
                builds[C] = 1;
            }
            else
            {
                builds[C] = builds[C].Value + 1;
            }

            return use.Call(B).Value + use.Call(F).Value;
        }

        Integer D(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(D))
            {
                builds[D] = 1;
            }
            else
            {
                builds[D] = builds[D].Value + 1;
            }

            return use.Call(C);
        }

        Integer G(ICapsuleHandle use)
        {
            if (!builds.ContainsKey(G))
            {
                builds[G] = 1;
            }
            else
            {
                builds[G] = builds[G].Value + 1;
            }

            return use.Call(C).Value + use.Call(F).Value;
        }

        using var container = new Container();
        Assert.True(builds.Count == 0);

        Assert.Equal(1, container.Read(D));
        Assert.Equal(2, container.Read(G));
        Assert.Equal(1, builds[A]);
        Assert.Equal(1, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(1, builds[E]);
        Assert.Equal(1, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        container.Read(A).Item2(0);
        Assert.Equal(1, container.Read(D));
        Assert.Equal(2, container.Read(G));
        Assert.Equal(2, builds[A]);
        Assert.Equal(1, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(1, builds[E]);
        Assert.Equal(1, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        container.Read(A).Item2(1);
        Assert.Equal(3, builds[A]);
        Assert.Equal(2, builds[B]);
        Assert.Equal(1, builds[C]);
        Assert.Equal(1, builds[D]);
        Assert.Equal(2, builds[E]);
        Assert.Equal(2, builds[F]);
        Assert.Equal(1, builds[G]);
        Assert.Equal(1, builds[H]);

        Assert.Equal(3, container.Read(D));
        Assert.Equal(5, container.Read(G));
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
