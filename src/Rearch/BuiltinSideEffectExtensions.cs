// <copyright file="BuiltinSideEffectExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

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
    /// <i>The setter will not trigger rebuilds</i>.
    /// The initial state will be set to the result of running
    /// <paramref name="init"/>, if it was provided. Otherwise, you must
    /// manually set it via the setter before ever calling the getter.
    /// </summary>
    /// <typeparam name="T">Type of raw value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">Callback to initialize side effect state.</param>
    /// <returns>
    /// A raw value wrapper; i.e., a getter and setter for some value.
    /// </returns>
    public static (Func<T> Getter, Action<T> Setter) RawValueWrapper<T>(
        this ISideEffectRegistrar registrar,
        Func<T>? init = null)
    {
        return registrar.Register<(Func<T>, Action<T>)>(api =>
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
    /// Similar to the <c>useState</c> hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </summary>
    /// <typeparam name="T">Type of side effect state.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">Callback to initialize side effect state.</param>
    /// <returns>Side effect state and setter.</returns>
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
    /// Similar to the <c>useState</c> hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </summary>
    /// <typeparam name="T">Type of side effect state.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="initial">Initial side effect state.</param>
    /// <returns>Side effect state and setter.</returns>
    public static (T State, Action<T> SetState) State<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        registrar.LazyState(() => initial);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds, where the initial value is computationally expensive.
    /// Similar to the <c>useRef</c> hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </summary>
    /// <typeparam name="T">Type of side effect lazy value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="init">Callback to initialize side effect lazy value.</param>
    /// <returns>Side effect lazy value.</returns>
    public static T LazyValue<T>(
        this ISideEffectRegistrar registrar,
        Func<T> init) =>
        registrar.InvokeOnce(init);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds.
    /// Similar to the <c>useRef</c> hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </summary>
    /// <typeparam name="T">Type of side effect value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="initial">Initial side effect value.</param>
    /// <returns>Side effect value.</returns>
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
    /// See https://react.dev/reference/react/useMemo.
    /// </summary>
    /// <typeparam name="T">Type of side effect value.</typeparam>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="memo">Memoization function to obtain value.</param>
    /// <param name="dependencies">Dependencies to trigger update.</param>
    /// <returns>Memoized value.</returns>
    public static T Memo<T>(
        this ISideEffectRegistrar registrar,
        Func<T> memo,
        IList<object?>? dependencies = null)
    {
        var deps = dependencies ?? [];
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
    /// See https://react.dev/reference/react/useEffect.
    /// </summary>
    /// <param name="registrar">Side effect registrar.</param>
    /// <param name="effect">Effect callback.</param>
    /// <param name="dependencies">Dependencies to trigger effect.</param>
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
