// <copyright file="ICapsuleReader.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Provides a mechanism to read the current state of <see cref="Capsule{T}"/>s.
/// </summary>
public interface ICapsuleReader
{
    /// <summary>
    /// Reads the data of the supplied <see cref="Capsule{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of encapsulated state.</typeparam>
    /// <param name="capsule">Capsule whose state to use.</param>
    /// <returns><paramref name="capsule"/> state.</returns>
    T Call<T>(Capsule<T> capsule);
}
