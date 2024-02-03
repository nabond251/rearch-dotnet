// <copyright file="CapsuleConsumer.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

/// <summary>
/// A <see cref="Component"/> that has access to a
/// <see cref="IComponentHandle"/>, and can consequently consume
/// <see cref="Capsule{T}"/>s and <see cref="SideEffect{T}"/>s.
/// </summary>
public abstract class CapsuleConsumer : Component
{
    /// <inheritdoc/>
    public sealed override VisualNode Render()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Render with ability to consume <see cref="Capsule{T}"/>s and
    /// <see cref="SideEffect{T}"/>s.
    /// </summary>
    /// <param name="use">Component handle.</param>
    /// <returns>Rendered node.</returns>
    public abstract VisualNode Render(IComponentHandle use);
}
