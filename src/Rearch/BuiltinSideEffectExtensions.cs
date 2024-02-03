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
    public static T Callonce<T>(
        this ISideEffectRegistrar registrar,
        Func<T> callback) =>
        registrar.Register((_) => callback());

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
        registrar.Callonce(init);

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
        throw new NotImplementedException();
    }

    // TODO(nabond251): other side effects
}
