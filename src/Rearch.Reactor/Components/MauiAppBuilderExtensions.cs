// <copyright file="MauiAppBuilderExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder UseRearchReactorApp<TComponent>(
        this MauiAppBuilder appBuilder,
        Action<ReactorApplication>? configureApplication = null)
        where TComponent : CapsuleConsumer, new() =>
        appBuilder.UseMauiReactorApp<
            CapsuleContainerProvider<TComponent>>(
            configureApplication);
}
