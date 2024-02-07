// <copyright file="ComponentHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

internal class ComponentHandle(
    ComponentSideEffectApi api,
    CapsuleContainer container) : IComponentHandle
{
    private int sideEffectDataIndex = 0;

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
                this.Api.Rebuild();
            }

            hasCalledBefore = true;
        });
        this.Api.Manager.ListenerHandles.Add(handle);

        return this.Container.Read(capsule);
    }

    /// <inheritdoc/>
    public T Register<T>(SideEffect<T> sideEffect) =>
        this.RegisterInternal<T>(api => sideEffect(api));

    /// <inheritdoc/>
    public T Register<T>(ComponentSideEffect<T> sideEffect) =>
        this.RegisterInternal<T>(api => sideEffect(api));

    private T RegisterInternal<T>(
        Func<IComponentSideEffectApi, object?> sideEffect)
    {
        if (this.sideEffectDataIndex == this.Api.Manager.SideEffectData.Count)
        {
            this.Api.Manager.SideEffectData.Add(sideEffect(this.Api));
        }

        return (T)this.Api.Manager.SideEffectData[this.sideEffectDataIndex++]!;
    }
}
