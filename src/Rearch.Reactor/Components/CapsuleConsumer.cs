// <copyright file="CapsuleConsumer.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using System.Diagnostics;
using MauiReactor;
using MauiReactor.Parameters;

/// <summary>
/// A <see cref="Component"/> that has access to a
/// <see cref="ICapsuleHandle"/>, and can consequently consume
/// <see cref="Capsule{T}"/>s and <see cref="SideEffect{T}"/>s.
/// </summary>
public abstract partial class CapsuleConsumer : Component
{
    [Param]
    private readonly IParameter<CapsuleContainerParameter> containerParameter;

    /// <summary>
    /// Gets set of callbacks to execute <see cref="Component.OnWillUnmount"/>.
    /// </summary>
    internal HashSet<SideEffectApiCallback> UnmountListeners { get; } = [];

    /// <summary>
    /// Gets data of registered side effects.
    /// </summary>
    internal List<object?> SideEffectData { get; } = [];

    /// <summary>
    /// Gets listener handles to dispose.
    /// </summary>
    internal List<ListenerHandle> ListenerHandles { get; } = [];

    /// <inheritdoc/>
    public sealed override VisualNode Render()
    {
        // listeners will be repopulated via ComponentHandle
        this.ClearHandles();

        var container = this.containerParameter.Value.Container;
        Debug.Assert(
            container != null,
            "No CapsuleContainerProvider found in the component tree!\nDid you forget to add UseRearchReactorApp to MauiProgram?");

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
    public abstract VisualNode Render(ICapsuleHandle use);

    /// <summary>
    /// Invalidates component, triggering re-render.
    /// </summary>
    internal new void Invalidate() => base.Invalidate();

    /// <inheritdoc/>
    protected override void OnWillUnmount()
    {
        foreach (var listener in this.UnmountListeners)
        {
            listener();
        }

        this.ClearHandles();

        // Clean up after any side effects to avoid possible leaks
        this.UnmountListeners.Clear();

        base.OnWillUnmount();
    }

    private void ClearHandles()
    {
        foreach (var handle in this.ListenerHandles)
        {
            handle.Dispose();
        }

        this.ListenerHandles.Clear();
    }
}
