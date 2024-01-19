// <copyright file="ISideEffectRegistrar.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Represents a side effect.
/// See the documentation for more.
/// </summary>
/// <typeparam name="T">Type of side effect result.</typeparam>
/// <param name="sideEffectApi">The API to create this side effect's state.</param>
/// <returns>Side effect result.</returns>
public delegate T SideEffect<out T>(ISideEffectApi sideEffectApi);

/// <summary>
/// Provides a mechanism (<see cref="Register{T}(SideEffect{T})"/>) to register side effects.
/// </summary>
public interface ISideEffectRegistrar
{
    /// <summary>
    /// Registers the given side effect.
    /// </summary>
    /// <typeparam name="T">Type of side effect result.</typeparam>
    /// <param name="sideEffect">Side effect to register.</param>
    /// <returns>Side effect result.</returns>
    T Register<T>(SideEffect<T> sideEffect);
}
