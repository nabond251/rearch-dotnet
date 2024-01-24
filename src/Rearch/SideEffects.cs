// <copyright file="SideEffects.cs" company="SdgApps">
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
    public static ISideEffectApi Api(this ISideEffectRegistrar registrar) =>
        registrar.Register((api) => api);

    /// <summary>
    /// Convenience side effect that gives a copy of
    /// <see cref="ISideEffectApi.Rebuild"/>.
    /// </summary>
    public static Action Rebuilder(this ISideEffectRegistrar registrar) =>
        registrar.Api().Rebuild;

    /// <summary>
    /// Side effect that calls the supplied <paramref name="callback"/> once,
    /// on the first build.
    /// </summary>
    public static T Callonce<T>(
        this ISideEffectRegistrar registrar,
        Func<T> callback) =>
        registrar.Register((_) => callback());

    /// <summary>
    /// Side effect that provides a way for capsules to contain some state,
    /// where the initial state is computationally expensive.
    /// Similar to the `useState` hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </summary>
    public static (T, Action<T>) LazyState<T>(
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
    /// Similar to the `useState` hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </summary>
    public static (T, Action<T>) State<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        registrar.LazyState(() => initial);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds, where the initial value is computationally expensive.
    /// Similar to the `useRef` hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </summary>
    public static T LazyValue<T>(
        this ISideEffectRegistrar registrar,
        Func<T> init) =>
        registrar.Callonce(init);

    /// <summary>
    /// Side effect that provides a way for capsules to hold onto some value
    /// between builds.
    /// Similar to the `useRef` hook from React;
    /// see https://react.dev/reference/react/useRef.
    /// </summary>
    public static T Value<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        registrar.LazyValue(() => initial);

    // TODO(nabond251): other side effects
}
