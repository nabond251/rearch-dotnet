// <copyright file="MauiAppBuilderExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;

/// <summary>
/// Rearch Reactor bootstrapper.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers Rearch Reactor app services.
    /// </summary>
    /// <typeparam name="TComponent">Type of root component.</typeparam>
    /// <param name="appBuilder">MAUI app builder.</param>
    /// <param name="configureApplication">
    /// Application configuration callback.
    /// </param>
    /// <returns>Rearch Reactor app builder.</returns>
    public static MauiAppBuilder UseRearchReactorApp<TComponent>(
        this MauiAppBuilder appBuilder,
        Action<ReactorApplication>? configureApplication = null)
        where TComponent : CapsuleConsumer, new() =>
        appBuilder.UseMauiReactorApp<
            CapsuleContainerProvider<TComponent>>(
            configureApplication);
}
