// <copyright file="CapsuleContainer.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

// TODO(nabond251): eager garbage collection mode

/// <summary>
/// A blueprint for creating some data, given a <see cref="ICapsuleHandle"/>.
/// </summary>
/// <typeparam name="T">Type of encapsulated data.</typeparam>
/// <param name="handle">Capsule handle.</param>
/// <returns>Encapsulated data.</returns>
/// <remarks>
/// See the documentation for more.
/// </remarks>
public delegate T Capsule<out T>(ICapsuleHandle handle);

/// <summary>
/// Capsule listeners are a mechanism to <i>temporarily</i> listen to changes
/// to a set of <see cref="Capsule{T}"/>s.
/// </summary>
/// <param name="capsuleReader">Capsule reader.</param>
/// <seealso cref="CapsuleContainer.Listen(CapsuleListener)"/>
public delegate void CapsuleListener(ICapsuleReader capsuleReader);

/// <summary>
/// Defines what a <see cref="SideEffect{T}"/> should look like (a
/// <see cref="Func{T, TResult}"/> that consumes an
/// <see cref="ISideEffectApi"/> and returns something).
/// </summary>
/// <remarks>
/// If your side effect is more advanced or requires parameters,
/// simply make a callable class instead of just a regular
/// <see cref="Func{T, TResult}"/>!<br/>
/// <br/>
/// See the documentation for more.
/// </remarks>
public class CapsuleContainer : IDisposable
{
    private readonly Dictionary<
        object,
        UntypedCapsuleManager> capsules = new();

    /// <summary>
    /// Gets map of container to manager.
    /// </summary>
    internal Dictionary<object, UntypedCapsuleManager> Capsules =>
        this.capsules;

    /// <summary>
    /// Reads the current data of the supplied <see cref="Capsule{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated data.</typeparam>
    /// <param name="capsule">Capsule whose data to read.</param>
    /// <returns><paramref name="capsule"/> data.</returns>
    public T Read<T>(Capsule<T> capsule) =>
        this.ManagerOf(capsule).Data;

    /// <summary>
    /// <i>Temporarily</i> listens to changes in a given set of
    /// <see cref="Capsule{T}"/>s.
    /// </summary>
    /// <param name="listener">Capsule listener.</param>
    /// <returns>Listener handle to dispose when done listening.</returns>
    /// <remarks>
    /// If you want to listen to capsule(s) <i>not temporarily</i>,
    /// instead just make an impure capsule and
    /// <see cref="Read{T}(Capsule{T})"/> it once to initialize it.<br/>
    /// <c>Listen</c> calls the supplied listener immediately,
    /// and then after any capsules its listening to change.<br/>
    /// <note><see cref="ListenerHandle"/> will leak its listener if it is not
    /// disposed.</note>
    /// </remarks>
    public ListenerHandle Listen(CapsuleListener listener)
    {
        // Create a temporary *impure* capsule so that it doesn't get
        // super-pure garbage collected
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
    /// Runs the supplied <paramref name="callback"/> the next time
    /// <paramref name="capsule"/> is updated <i>or</i> disposed.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated data.</typeparam>
    /// <param name="capsule">Capsule whose next update to handle.</param>
    /// <param name="callback">
    /// Callback to run when <paramref name="capsule"/> is updated <i>or</i>
    /// disposed.
    /// </param>
    /// <returns>
    /// <see cref="Action"/> which, if invoked, removes the callback.
    /// </returns>
    /// <remarks>
    /// You can also prematurely remove the callback from the container
    /// via the returned <see cref="Action"/> (which does not invoke
    /// <paramref name="callback"/>).<br/>
    /// It is highly recommended not to use this method as I reserve the right
    /// to breakingly change or remove it at will in a new <i>minor</i> release.
    /// </remarks>
#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.Experimental("Rearch")]
#endif
    public Action OnNextUpdate<T>(
        Capsule<T> capsule,
        Action callback)
    {
        // This uses the fact that if we add a super pure capsule, it will be
        // automatically disposed whenever the supplied capsule is
        // updated/disposed via the super pure gc.
        object? TempCapsule(ICapsuleHandle use)
        {
            use.Invoke(capsule);
            return null;
        }

        var manager = new CapsuleManager<object?>(this, TempCapsule);
        var apiCallback = new SideEffectApiCallback(callback);
        manager.ToDispose.Add(apiCallback);
        this.capsules[(object)TempCapsule] = manager;

        return () =>
        {
            manager.ToDispose.Remove(apiCallback);
            manager.Dispose();
        };
    }

    /// <summary>
    /// Gets manager for given capsule.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated data.</typeparam>
    /// <param name="capsule">Capsule whose manager to get.</param>
    /// <returns><paramref name="capsule"/> manager.</returns>
    internal CapsuleManager<T> ManagerOf<T>(Capsule<T> capsule)
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
            // We need ToList() to prevent container modification during
            // iteration
            foreach (var manager in this.Capsules.Values.ToList())
            {
                manager.Dispose();
            }
        }
    }
}
