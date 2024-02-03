// <copyright file="CapsuleConsumer.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using System.Diagnostics;
using MauiReactor;
using MauiReactor.Parameters;

/// <summary>
/// A <see cref="Component"/> that has access to a
/// <see cref="IComponentHandle"/>, and can consequently consume
/// <see cref="Capsule{T}"/>s and <see cref="SideEffect{T}"/>s.
/// </summary>
public abstract partial class CapsuleConsumer : Component
{
    [Param]
    IParameter<CapsuleContainer> containerParameter;

    public readonly HashSet<SideEffectApiCallback> unmountListeners = [];
    public readonly HashSet<SideEffectApiCallback> disposeListeners = [];
    public readonly List<object?> sideEffectData = [];
    public readonly List<ListenerHandle> listenerHandles = [];

    public void ClearHandles()
    {
        foreach (var handle in this.listenerHandles)
        {
            handle.Dispose();
        }

        this.listenerHandles.Clear();
    }

    /// <inheritdoc/>
    public sealed override VisualNode Render()
    {
        this.ClearHandles(); // listeners will be repopulated via ComponentHandle

        var container = containerParameter.Value;
        Debug.Assert(
            container != null,
            "No CapsuleContainerProvider found in the component tree!\n" +
            "Did you forget to add UseRearchReactorApp to MauiProgram?");

        return this.Render(
          new ComponentHandle(
            new ComponentSideEffectApi(this),
            container));
    }

    /// <summary>
    /// Render with ability to consume <see cref="Capsule{T}"/>s and
    /// <see cref="SideEffect{T}"/>s.
    /// </summary>
    /// <param name="use">Component handle.</param>
    /// <returns>Rendered node.</returns>
    public abstract VisualNode Render(IComponentHandle use);

    internal new void Invalidate() => base.Invalidate();
}

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
      this.Manager.unmountListeners.Add(callback);

    public void RemoveUnmountListener(SideEffectApiCallback callback) =>
        this.Manager.unmountListeners.Remove(callback);

    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.Manager.disposeListeners.Add(callback);

    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.Manager.disposeListeners.Remove(callback);
}

internal class ComponentHandle(
    ComponentSideEffectApi api,
    CapsuleContainer container) : IComponentHandle
{
    int sideEffectDataIndex = 0;

    public ComponentSideEffectApi Api { get; } = api;

    public CapsuleContainer Container { get; } = container;

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule)
    {
        // Add capsule as dependency
        var hasCalledBefore = false;
        var handle = this.Container.Listen(use =>
        {
            use.Invoke(capsule); // mark capsule as a dependency

            // If this isn't the immediate call after registering, rebuild
            if (hasCalledBefore)
            {
                api.Rebuild();
            }

            hasCalledBefore = true;
        });
        api.Manager.listenerHandles.Add(handle);

        return container.Read(capsule);
    }

    /// <inheritdoc/>
    public T Register<T>(ComponentSideEffect<T> sideEffect)
    {
        if (this.sideEffectDataIndex == api.Manager.sideEffectData.Count)
        {
            api.Manager.sideEffectData.Add(sideEffect(api));
        }

        return (T)api.Manager.sideEffectData[this.sideEffectDataIndex++]!;
  }
}
