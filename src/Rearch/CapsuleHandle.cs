// <copyright file="CapsuleHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Implementation of the handle given to a <see cref="Capsule{T}"/> to build its
/// data.
/// </summary>
internal sealed class CapsuleHandle(UntypedCapsuleManager manager) :
    ICapsuleHandle
{
    private int sideEffectDataIndex;

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule) =>
        manager.Read(capsule);

    /// <inheritdoc/>
    public T Register<T>(SideEffect<T> sideEffect)
    {
        if (this.sideEffectDataIndex == manager.SideEffectData.Count)
        {
            manager.SideEffectData.Add(sideEffect(manager));
        }

        return (T)manager.SideEffectData[this.sideEffectDataIndex++]!;
    }
}
