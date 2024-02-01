// <copyright file="CapsuleHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Implementation of the handle given to a <see cref="Capsule{T}"/> to build its data.
/// </summary>
internal sealed class CapsuleHandle(UntypedCapsuleManager manager) : ICapsuleHandle
{
    /// <summary>
    /// Gets the capsule manager for this handle.
    /// </summary>
    public UntypedCapsuleManager Manager { get; } = manager;

    /// <summary>
    /// Gets index into manager's side effect data.
    /// </summary>
    public int SideEffectDataIndex { get; private set; }

    /// <inheritdoc/>
    public T Invoke<T>(Capsule<T> capsule) =>
        this.Manager.Read(capsule);

    /// <inheritdoc/>
    public T Register<T>(SideEffect<T> sideEffect)
    {
        if (this.SideEffectDataIndex == this.Manager.SideEffectData.Count)
        {
            this.Manager.SideEffectData.Add(sideEffect(this.Manager));
        }

        return (T)this.Manager.SideEffectData[this.SideEffectDataIndex++]!;
    }
}
