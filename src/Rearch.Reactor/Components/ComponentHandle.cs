// <copyright file="ComponentHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

/// <summary>
/// Implementation of the handle given to a <see cref="Capsule{T}"/> to build its
/// data.
/// </summary>
internal class ComponentHandle(
    ComponentSideEffectApi api,
    CapsuleContainer container) : ICapsuleHandle
{
    private int sideEffectDataIndex;

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule)
    {
        // Add capsule as dependency
        var hasCalledBefore = false;
        var handle = container.Listen(use =>
        {
            use.Invoke(capsule); // mark capsule as a dependency

            // If this isn't the immediate call after registering, rebuild
            if (hasCalledBefore)
            {
                api.Rebuild();
            }

            hasCalledBefore = true;
        });
        api.Manager.ListenerHandles.Add(handle);

        return container.Read(capsule);
    }

    /// <inheritdoc/>
    public T Register<T>(SideEffect<T> sideEffect)
    {
        if (this.sideEffectDataIndex == api.Manager.SideEffectData.Count)
        {
            api.Manager.SideEffectData.Add(sideEffect(api));
        }

        return (T)api.Manager.SideEffectData[this.sideEffectDataIndex++]!;
    }
}
