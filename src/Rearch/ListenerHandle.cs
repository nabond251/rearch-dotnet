// <copyright file="ListenerHandle.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// A handle onto the lifecycle of a listener from
/// <see cref="Container.Listen(CapsuleListener)"/>.
/// You <i>must</i> <see cref="Dispose()"/> the <see cref="ListenerHandle"/>
/// when you no longer need the listener in order to prevent leaks.
/// </summary>
public class ListenerHandle : IDisposable
{
    private readonly Container container;
    private readonly Capsule<object?> capsule;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenerHandle"/> class.
    /// </summary>
    /// <param name="container">Container containing listener's data.</param>
    /// <param name="capsule">Capsule whose data to listen to.</param>
    internal ListenerHandle(Container container, Capsule<object?> capsule)
    {
        this.container = container;
        this.capsule = capsule;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implements the disposable pattern.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether this is being disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            var capsules = this.container.Capsules;
            if (capsules.ContainsKey(this.capsule))
            {
                capsules[this.capsule].Dispose();
            }
        }
    }
}
