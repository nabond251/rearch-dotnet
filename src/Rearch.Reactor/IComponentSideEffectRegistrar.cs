// <copyright file="IComponentSideEffectRegistrar.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor;

/// <summary>
/// Defines what a <see cref="ComponentSideEffect{T}"/> should look like (a
/// <see cref="Func{T, TResult}"/> that consumes a
/// <see cref="IComponentSideEffectApi"/> and returns something).
///
/// If your side effect is more advanced or requires parameters,
/// simply make a callable class instead of just a regular
/// <see cref="Func{T, TResult}"/>.
/// </summary>
/// <typeparam name="T">Type of side effect result.</typeparam>
/// <param name="sideEffectApi">
/// The API to create this side effect's state.
/// </param>
/// <returns>Side effect result.</returns>
public delegate T ComponentSideEffect<out T>(ISideEffectApi sideEffectApi);

/// <summary>
/// Represents an object that can
/// <see cref="Register{T}(ComponentSideEffect{T})"/>
/// <see cref="ComponentSideEffect{T}"/>s.
/// </summary>
public interface IComponentSideEffectRegistrar
{
    /// <summary>
    /// Registers the given side effect.
    /// </summary>
    /// <typeparam name="T">Type of side effect result.</typeparam>
    /// <param name="sideEffect">Side effect to register.</param>
    /// <returns>Side effect result.</returns>
    T Register<T>(ComponentSideEffect<T> sideEffect);
}
