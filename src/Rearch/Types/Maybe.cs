// <copyright file="Maybe.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Contains <see cref="Maybe{T}"/> factory methods.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> with the associated immutable
    /// <see cref="Just{T}.Value"/>.
    /// </summary>
    /// <typeparam name="T">Type of contained data, if any.</typeparam>
    /// <param name="value">
    /// The immutable <see cref="Just{T}.Value"/> associated with this
    /// <see cref="Maybe{T}"/>.
    /// </param>
    /// <returns>New <see cref="Just{T}"/>.</returns>
    public static Maybe<T> Just<T>(T value) => new Just<T>(value);

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> that does not have a value.
    /// </summary>
    /// <typeparam name="T">Type of contained data, if any.</typeparam>
    /// <returns>New <see cref="None{T}"/>.</returns>
    public static Maybe<T> None<T>() => new None<T>();
}
