// <copyright file="Container.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

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
