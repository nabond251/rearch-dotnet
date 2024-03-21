// <copyright file="ComponentHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

/// <summary>
/// Implementation of the handle given to a <see cref="Capsule{T}"/> to build
/// its data.
/// </summary>
/// <typeparam name="TProps">Type of component props.</typeparam>
internal sealed class ComponentHandle<TProps>(
    ComponentSideEffectApiProxy<TProps> api,
    CapsuleContainer container) : ICapsuleHandle
    where TProps : class, new()
{
    private int sideEffectDataIndex;

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule)
    {
#pragma warning disable Rearch // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var dispose = container.OnNextUpdate(capsule, api.Rebuild);
#pragma warning restore Rearch // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        api.Manager.DependencyDisposers.Add(dispose);
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
