// <copyright file="Container.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

// TODO(nabond251): eager garbage collection mode

/// <summary>
/// A blueprint for creating some data, given a <see cref="ICapsuleHandle"/>.
/// See the documentation for more.
/// </summary>
/// <typeparam name="T">Type of encapsulated data.</typeparam>
/// <param name="handle">Capsule handle.</param>
/// <returns>Encapsulated data.</returns>
public delegate T Capsule<out T>(ICapsuleHandle handle);

/// <summary>
/// Capsule listeners are a mechanism to <i>temporarily</i> listen to changes
/// to a set of <see cref="Capsule{T}"/>s.
/// </summary>
/// <param name="capsuleReader">Capsule reader.</param>
/// <seealso cref="Container.Listen(CapsuleListener)"/>
public delegate void CapsuleListener(ICapsuleReader capsuleReader);

/// <summary>
/// Contains the data of <see cref="Capsule{T}"/>s.
/// See the documentation for more.
/// </summary>
public class Container : IDisposable
{
    private readonly Dictionary<
        object,
        UntypedCapsuleManager> capsules = [];

    /// <summary>
    /// Gets map of container to manager.
    /// </summary>
    internal Dictionary<object, UntypedCapsuleManager> Capsules => this.capsules;

    /// <summary>
    /// Reads the current data of the supplied <see cref="Capsule{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated data.</typeparam>
    /// <param name="capsule">Capsule whose data to read.</param>
    /// <returns><paramref name="capsule"/> data.</returns>
    public T Read<T>(Capsule<T> capsule) =>
        this.Manager(capsule).Data;

    /// <summary>
    /// <i>Temporarily</i> listens to changes in a given set of <see cref="Capsule{T}"/>s.
    /// If you want to listen to capsule(s) <i>not temporarily</i>,
    /// instead just make an impure capsule and <see cref="Read{T}(Capsule{T})"/> it once to initialize it.
    /// <c>Listen</c> calls the supplied listener immediately,
    /// and then after any capsules its listening to change.
    /// </summary>
    /// <param name="listener">Capsule listener.</param>
    /// <returns>Listener handle to dispose when done listening.</returns>
    /// <remarks><see cref="ListenerHandle"/> will leak its listener if it is not disposed.</remarks>
    public ListenerHandle Listen(CapsuleListener listener)
    {
        // Create a temporary *impure* capsule so that it doesn't get super-pure
        // garbage collected
        object Capsule(ICapsuleHandle use)
        {
            use.Register(_ => new object());
            listener(use);
            return new object();
        }

        // Put the temporary capsule into the container so it gets data updates
        this.Read(Capsule);

        return new ListenerHandle(this, Capsule);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets manager for given capsule.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated data.</typeparam>
    /// <param name="capsule">Capsule whose manager to get.</param>
    /// <returns><paramref name="capsule"/> manager.</returns>
    internal CapsuleManager<T> Manager<T>(Capsule<T> capsule)
    {
        if (!this.Capsules.ContainsKey(capsule))
        {
            this.Capsules[capsule] = new CapsuleManager<T>(this, capsule);
        }

        return this.Capsules[capsule] as CapsuleManager<T> ??
            throw new InvalidOperationException();
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
            // We need ToList() to prevent container modification during iteration
            foreach (var manager in this.Capsules.Values.ToList())
            {
                manager.Dispose();
            }
        }
    }
}
