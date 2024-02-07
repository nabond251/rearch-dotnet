// <copyright file="ComponentSideEffectApi.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

/// <summary>
/// This is needed so that <see cref="ISideEffectApi.Rebuild"/> doesn't
/// conflict with <see cref="Component.Render()"/>.
/// </summary>
internal sealed class ComponentSideEffectApi(CapsuleConsumer manager) : IComponentSideEffectApi
{
    public CapsuleConsumer Manager { get; } = manager;

    public CapsuleConsumer Context => this.Manager;

    public void Rebuild() => this.Manager.Invalidate();

    public void AddUnmountListener(SideEffectApiCallback callback) =>
      this.Manager.UnmountListeners.Add(callback);

    public void RemoveUnmountListener(SideEffectApiCallback callback) =>
        this.Manager.UnmountListeners.Remove(callback);

    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.Manager.UnmountListeners.Add(callback);

    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.Manager.UnmountListeners.Remove(callback);
}
