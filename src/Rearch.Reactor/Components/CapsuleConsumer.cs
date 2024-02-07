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
    private readonly IParameter<CapsuleContainerParameter> containerParameter;

    internal HashSet<SideEffectApiCallback> UnmountListeners { get; } = [];

    internal List<object?> SideEffectData { get; } = [];

    internal List<ListenerHandle> ListenerHandles { get; } = [];

    public void ClearHandles()
    {
        foreach (var handle in this.ListenerHandles)
        {
            handle.Dispose();
        }

        this.ListenerHandles.Clear();
    }

    /// <inheritdoc/>
    public sealed override VisualNode Render()
    {
        this.ClearHandles(); // listeners will be repopulated via ComponentHandle

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
    public abstract VisualNode Render(IComponentHandle use);

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
}
