// <copyright file="Rearch.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>


namespace Rearch;

public delegate T Capsule<T>(ICapsuleHandle capsuleHandle);

public delegate void CapsuleListener(ICapsuleReader capsuleReader);

public interface ICapsuleReader
{
    T Call<T>(Capsule<T> capsule);
}

public interface ISideEffectRegistrar
{
    T Register<T>(SideEffect<T> sideEffect);
}

public interface ICapsuleHandle : ICapsuleReader, ISideEffectRegistrar
{
}

public delegate T SideEffect<T>(ISideEffectApi sideEffectApi);

public delegate void SideEffectApiCallback();

public interface ISideEffectApi
{
    void Rebuild();

    void RegisterDispose(SideEffectApiCallback callback);

    void UnregisterDispose(SideEffectApiCallback callback);
}

/// <summary>
/// Contains the data of capsules.
/// </summary>
public class Container : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public T Read<T>(Capsule<T> capsule) => default;

    /// <summary>
    /// Implements the disposable pattern.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether this is being disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup
    }
}
