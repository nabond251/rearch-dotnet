// <copyright file="ComponentSideEffectApiProxy.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

/// <summary>
/// This is needed so that <see cref="ISideEffectApi.Rebuild"/> doesn't conflict
/// with <see cref="Component.Render()"/>.
/// </summary>
/// <typeparam name="TProps">Type of component props.</typeparam>
internal sealed class ComponentSideEffectApiProxy<TProps>(
    CapsuleConsumer<TProps> manager) : ISideEffectApi
    where TProps : class, new()
{
    /// <summary>
    /// Gets component consuming capsule's data.
    /// </summary>
    internal CapsuleConsumer<TProps> Manager { get; } = manager;

    /// <inheritdoc/>
    public void Rebuild() => this.Manager.Invalidate();

    /// <inheritdoc/>
    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.Manager.UnmountListeners.Add(callback);

    /// <inheritdoc/>
    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.Manager.UnmountListeners.Remove(callback);
}
