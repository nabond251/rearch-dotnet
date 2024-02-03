// <copyright file="BuiltinComponentSideEffectExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor;

/// <summary>
/// A collection of the builtin <see cref="ComponentSideEffect{T}"/>s.
/// </summary>
public static class BuiltinComponentSideEffectExtensions
{
    /// <summary>
    /// The <see cref="IComponentSideEffectApi"/> backing this
    /// <see cref="IComponentSideEffectRegistrar"/>.
    /// </summary>
    /// <param name="registrar">Side effect registrar.</param>
    /// <returns>A copy of the <see cref="IComponentSideEffectApi"/>.</returns>
    public static IComponentSideEffectApi Api(
        this IComponentSideEffectRegistrar registrar) =>
        registrar.Register((api) => api);
}
