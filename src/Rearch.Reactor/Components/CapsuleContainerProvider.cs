// <copyright file="CapsuleContainerProvider.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;
using MauiReactor.Parameters;

public class CapsuleContainerParameter()
{
    public CapsuleContainer Container { get; set; }
}

/// <summary>
/// Provides a <see cref="CapsuleContainer"/> to the rest of the
/// <see cref="Component"/> tree using an <see cref="IParameter"/>.
///
/// Does not manage the lifecycle of the supplied
/// <see cref="CapsuleContainer"/>. You typically should use
/// <see cref="RearchBootstrapper"/> instead.
/// </summary>
public partial class CapsuleContainerProvider : Component
{
    [Param]
    IParameter<CapsuleContainerParameter> containerParameter;

    public CapsuleContainerProvider(CapsuleContainer container, Component child)
    {
        this.Container = container;
        this.Child = child;
    }

    /// <summary>
    /// Gets the <see cref="CapsuleContainer"/> this
    /// <see cref="CapsuleContainerProvider"/> is providing to the rest of the
    /// <see cref="Component"/> tree.
    /// </summary>
    public CapsuleContainer Container { get; }

    public Component Child { get; }

    protected override void OnMounted()
    {
        base.OnMounted();

        containerParameter.Set(_ => _.Container = this.Container);
    }

    public override VisualNode Render()
    {
        return this.Child;
    }
}
