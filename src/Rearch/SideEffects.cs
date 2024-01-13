// <copyright file="SideEffects.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

public static class BuiltinSideEffectExtensions
{
    /// <summary>
    /// Side effect that provides a way for capsules to contain some state.
    /// Similar to the `useState` hook from React;
    /// see https://react.dev/reference/react/useState.
    /// </summary>
    public static (T, Action<T>) State<T>(
        this ISideEffectRegistrar registrar,
        T initial) =>
        (default, _ => { });
}