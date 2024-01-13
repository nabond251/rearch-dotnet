// <copyright file="SideEffects.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

public static class BuiltinSideEffectExtensions
{
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
        var (getter, setter) = registrar.Register(api =>
        {
            var state = init();

            T Getter() => state;
            void Setter(T newState)
            {
                state = newState;
                api.Rebuild();
            }

            return Tuple.Create(Getter, Setter);
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
}
