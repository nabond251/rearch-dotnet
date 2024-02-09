// <copyright file="CapsuleHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Implementation of the handle given to a <see cref="Capsule{T}"/> to build its
/// data.
/// </summary>
internal sealed class CapsuleHandle :
    ICapsuleHandle
{
    private readonly UntypedCapsuleManager manager;
    private int sideEffectDataIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleHandle"/> class.
    /// </summary>
    /// <param name="manager">Capsule manager.</param>
    public CapsuleHandle(UntypedCapsuleManager manager)
    {
        this.manager = manager;
    }

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule) =>
        this.manager.Read(capsule);

    /// <inheritdoc/>
    public T Register<T>(SideEffect<T> sideEffect)
    {
        if (this.sideEffectDataIndex == this.manager.SideEffectData.Count)
        {
            this.manager.SideEffectData.Add(sideEffect(this.manager));
        }

        return (T)this.manager.SideEffectData[this.sideEffectDataIndex++]!;
    }
}
