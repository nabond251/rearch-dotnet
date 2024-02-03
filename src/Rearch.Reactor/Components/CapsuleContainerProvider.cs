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
public partial class CapsuleContainerProvider<TComponent> : Component
    where TComponent : CapsuleConsumer, new()
{
    [Param]
    IParameter<CapsuleContainerParameter> containerParameter;

    protected override void OnMounted()
    {
        base.OnMounted();

        containerParameter.Set(p => p.Container = new CapsuleContainer());
    }

    public override VisualNode Render()
    {
        return new TComponent();
    }
}
