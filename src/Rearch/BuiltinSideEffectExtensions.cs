// <copyright file="BuiltinSideEffectExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

using System.Reactive.Threading.Tasks;
using Rearch.Types;

/// <summary>
/// A reducer function that consumes some <typeparamref name="TState"/> and
/// <typeparamref name="TAction"/> and returns a new, transformed
/// <typeparamref name="TState"/>.
/// </summary>
/// <typeparam name="TState">Type of state to consume.</typeparam>
/// <typeparam name="TAction">Type of action to consume.</typeparam>
/// <param name="state">State to consume.</param>
/// <param name="action">Action to consume.</param>
/// <returns>Reduced state.</returns>
public delegate TState Reducer<TState, in TAction>(TState state, TAction action);

/// <summary>
/// A collection of builtin side effects.
/// </summary>
public static class BuiltinSideEffectExtensions
{
    /// <summary>
    /// Convenience side effect that gives a copy of the
    /// <see cref="ISideEffectApi"/>.
    /// </summary>
    /// <param name="registrar">Side effect registrar.</param>
    /// <returns>A copy of the <see cref="ISideEffectApi"/>.</returns>
    public static ISideEffectApi Api(this ISideEffectRegistrar registrar) =>
        registrar.Register((api) => api);

    /// <summary>
    /// Convenience side effect that gives a copy of
    /// <see cref="ISideEffectApi.Rebuild"/>.
    /// </summary>
    /// <param name="registrar">Side effect registrar.</param>
    /// <returns>A copy of <see cref="ISideEffectApi.Rebuild"/>.</returns>
    public static Action Rebuilder(this ISideEffectRegistrar registrar) =>
        registrar.Api().Rebuild;

    /// <summary>
    /// Side effect that calls the supplied <paramref name="callback"/> once,
    /// on the first build.
    /// </summary>
    /// <typeparam name="T">
    /// Type of side effect <paramref name="callback"/> result.
    /// </typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="callback">Callback to be called once.</param>
    /// <returns><paramref name="callback"/> result.</returns>
    public static T InvokeOnce<T>(
        this ISideEffectRegistrar registrar,
        Func<T> callback) =>
        registrar.Register((_) => callback());

    /// <summary>
    /// Returns a raw value wrapper; i.e., a getter and setter for some value.
    /// </summary>
    /// <typeparam name="T">Type of raw value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">Callback to initialize side effect state.</param>
    /// <returns>
    /// A raw value wrapper; i.e., a getter and setter for some value.
    /// </returns>
    /// <remarks>
    /// <i>The setter will not trigger rebuilds</i>.<br/>
    /// The initial state will be set to the result of running
    /// <paramref name="init"/>, if it was provided. Otherwise, you must
    /// manually set it via the setter before ever calling the getter.
    /// </remarks>
    public static (Func<T> Getter, Action<T> Setter) RawValueWrapper<T>(
        this ISideEffectRegistrar registrar,
        Func<T>? init = null)
    {
        return registrar.InvokeOnce<(Func<T>, Action<T>)>(() =>
        {
            T state = default!;
            if (init != null)
            {
                state = init();
            }

            return (() => state, (T newState) => state = newState);
        });
    }

    /// <summary>
    /// Side effect that provides a way for capsules to contain some state,
    /// where the initial state is computationally expensive.
    /// </summary>
    /// <typeparam name="T">Type of side effect state.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">Callback to initialize side effect state.</param>
    /// <returns>Side effect state and setter.</returns>
    /// <remarks>
    /// Similar to the <c>useState</c> hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </remarks>
    public static (T State, Action<T> SetState) LazyState<T>(
        this ISideEffectRegistrar registrar,
        Func<T> init)
    {
        // We use register directly to keep the same setter function
        // across rebuilds, which actually can help skip certain rebuilds
        var (getter, setter) = registrar.Register<(Func<T>, Action<T>)>(api =>
        {
            var state = init();

            T Getter() => state;
            void Setter(T newState)
            {
                state = newState;
                api.Rebuild();
            }

            return (Getter, Setter);
        });

        return (getter(), setter);
    }

    /// <summary>
    /// Side effect that provides a way for capsules to contain some state.
    /// </summary>
    /// <typeparam name="T">Type of side effect state.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="initial">Initial side effect state.</param>
    /// <returns>Side effect state and setter.</returns>
    /// <remarks>
    /// Similar to the <c>useState</c> hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </remarks>
    public static (T State, Action<T> SetState) State<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        registrar.LazyState(() => initial);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds, where the initial value is computationally expensive.
    /// </summary>
    /// <typeparam name="T">Type of side effect lazy value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">
    /// Callback to initialize side effect lazy value.
    /// </param>
    /// <returns>Side effect lazy value.</returns>
    /// <remarks>
    /// Similar to the <c>useRef</c> hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </remarks>
    public static T LazyValue<T>(
        this ISideEffectRegistrar registrar,
        Func<T> init) =>
        registrar.InvokeOnce(init);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds.
    /// </summary>
    /// <typeparam name="T">Type of side effect value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="initial">Initial side effect value.</param>
    /// <returns>Side effect value.</returns>
    /// <remarks>
    /// Similar to the <c>useRef</c> hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </remarks>
    public static T Value<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        registrar.LazyValue(() => initial);

    /// <summary>
    /// Returns the previous value passed into
    /// <see cref="Previous{T}(ISideEffectRegistrar, T)"/>, or <c>null</c> on
    /// first build.
    /// </summary>
    /// <typeparam name="T">Type of side effect value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="current">The current value.</param>
    /// <returns>The previous value, or <c>null</c> on first build.</returns>
    public static T? Previous<T>(
        this ISideEffectRegistrar registrar,
        T current)
    {
        var (getter, setter) = registrar.RawValueWrapper<T?>(() => default);
        var prev = getter();
        setter(current);
        return prev;
    }

    /// <summary>
    /// Equivalent to the `useMemo` hook from React.
    /// </summary>
    /// <typeparam name="T">Type of side effect value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="memo">Memoization function to obtain value.</param>
    /// <param name="dependencies">Dependencies to trigger update.</param>
    /// <returns>Memoized value.</returns>
    /// <remarks>
    /// See https://react.dev/reference/react/useMemo.
    /// </remarks>
    public static T Memo<T>(
        this ISideEffectRegistrar registrar,
        Func<T> memo,
        IList<object?>? dependencies = null)
    {
        var deps = dependencies ?? new List<object?>();
        var oldDependencies = registrar.Previous(deps);
        var (getData, setData) = registrar.RawValueWrapper<T>();
        if (DidDepsListChange(deps, oldDependencies))
        {
            setData(memo());
        }

        return getData();
    }

    /// <summary>
    /// Equivalent to the `useEffect` hook from React.
    /// </summary>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="effect">Effect callback.</param>
    /// <param name="dependencies">Dependencies to trigger effect.</param>
    /// <remarks>
    /// See https://react.dev/reference/react/useEffect.
    /// </remarks>
    public static void Effect(
        this ISideEffectRegistrar registrar,
        Func<Action?> effect,
        IList<object?>? dependencies = null)
    {
        var oldDependencies = registrar.Previous(dependencies);
        var (getDispose, setDispose) =
            registrar.RawValueWrapper<Action?>(() => null);
        registrar.Register((api) =>
        {
            api.RegisterDispose(() => getDispose()?.Invoke());
            return new object();
        });

        if (dependencies == null ||
            DidDepsListChange(dependencies, oldDependencies))
        {
            getDispose()?.Invoke();
            setDispose(effect());
        }
    }

    /// <summary>
    /// A simple implementation of the reducer pattern as a side effect.
    /// <br />
    /// <br />
    /// The React docs do a great job of explaining the reducer pattern more.
    /// See https://react.dev/reference/react/useReducer.
    /// </summary>
    /// <typeparam name="TState">Type of state to consume.</typeparam>
    /// <typeparam name="TAction">Type of action to consume.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="reducer">Reducer function.</param>
    /// <param name="initialState">Initial reducer state.</param>
    /// <returns>Reducer state and action dispatcher.</returns>
    public static (TState State, Action<TAction> Dispatch) Reducer<TState, TAction>(
        this ISideEffectRegistrar registrar,
        Reducer<TState, TAction> reducer,
        TState initialState)
    {
        var (currState, setState) = registrar.State(initialState);
        return (
            currState,
            (action) => setState(reducer(currState, action)));
    }

    /// <summary>
    /// Consumes a <see cref="System.Threading.Tasks.Task{T}"/> and watches the
    /// given <paramref name="task"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="task">The task to wrap.</param>
    /// <returns>
    /// An <see cref="AsyncValue{T}"/> wrapping the given
    /// <paramref name="task"/>.
    /// </returns>
    /// <remarks>
    /// Implemented by calling
    /// <see cref="TaskObservableExtensions.ToObservable(Task)"/> and forwarding
    /// calls onto <see cref="Observable"/>.<br/>
    /// <br/>
    /// If the given task changes, then the current <see cref="IDisposable"/>
    /// will be disposed and recreated for the new task.<br/>
    /// Thus, it is important that the task instance only changes when needed.
    /// It is incorrect to create a task in the same build as the
    /// <paramref name="task"/>, unless you use something like
    /// <see cref="Memo"/> to limit changes.<br/>
    /// Or, if possible, it is even better to wrap the task in an entirely
    /// new capsule (although this is not always possible).
    /// </remarks>
    public static AsyncValue<T> Task<T>(
        this ISideEffectRegistrar registrar,
        Task<T> task)
    {
        var toObservable = registrar.Memo(
            task.ToObservable,
            new List<object?> { task });
        return registrar.Observable(toObservable);
    }

    /// <summary>
    /// Consumes an <see cref="IObservable{T}"/> and watches the given
    /// observable.
    /// </summary>
    /// <typeparam name="T">Type of observable data.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="observable">The observable to wrap.</param>
    /// <returns>
    /// An <see cref="AsyncValue{T}"/> wrapping the given
    /// <paramref name="observable"/>.
    /// </returns>
    /// <remarks>
    /// If the given observable changes between build calls, then the current
    /// <see cref="IDisposable"/> will be disposed and recreated for the new
    /// observable.<br/>
    /// Thus, it is important that the observable instance only changes when
    /// needed. It is incorrect to create a observable in the same build as the
    /// <paramref name="observable"/>, unless you use something like
    /// <see cref="Memo"/> to limit changes.<br/>
    /// Or, if possible, it is even better to wrap the observable in an entirely
    /// new capsule (although this is not always possible).
    /// </remarks>
    public static AsyncValue<T> Observable<T>(
        this ISideEffectRegistrar registrar,
        IObservable<T> observable)
    {
        var rebuild = registrar.Rebuilder();
        var (getValue, setValue) = registrar.RawValueWrapper<AsyncValue<T>>(
            () => new AsyncLoading<T>(new None<T>()));

        var (getSubscription, setSubscription) =
            registrar.RawValueWrapper<IDisposable?>(() => null);
        registrar.Effect(
            () =>
            {
                var subscription = getSubscription();
                return subscription is null ?
                    () => { } :
                    new Action(subscription.Dispose);
            },
            new List<object?> { getSubscription() });

        var oldObservable = registrar.Previous(observable);
        var needToInitializeState = observable != oldObservable;

        if (needToInitializeState)
        {
            setValue(new AsyncLoading<T>(getValue().GetData()));
            setSubscription(
                observable.Subscribe(
                    data =>
                    {
                        setValue(new AsyncData<T>(data));
                        rebuild();
                    },
                    onError: error =>
                    {
                        setValue(new AsyncError<T>(error, getValue().GetData()));
                        rebuild();
                    }));
        }

        return getValue();
    }

    // TODO(nabond251): other side effects

    /// <summary>
    /// Checks to see whether <paramref name="newDeps"/> has changed from
    /// <paramref name="oldDeps"/> using a deep-ish equality check (compares
    /// <c>==</c> amongst <see cref="IList{T}"/> children).
    /// </summary>
    private static bool DidDepsListChange(
        IList<object?> newDeps,
        IList<object?>? oldDeps)
    {
        return
            oldDeps == null ||
            !newDeps.SequenceEqual(oldDeps);
    }
}
