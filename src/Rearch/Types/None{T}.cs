// <copyright file="None{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// A <see cref="Maybe{T}"/> that does not have a value.
/// </summary>
/// <typeparam name="T">Type of associated immutable value.</typeparam>
public sealed class None<T> : Maybe<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None{T}"/> class.
    /// Creates a <see cref="Maybe{T}"/> that does not have a value.
    /// </summary>
    public None()
    {
    }

    /// <inheritdoc/>
    public override int GetHashCode() => 0;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Just<T>;

    /// <inheritdoc/>
    public override string ToString() => "None()";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onJust,
        Func<TResult> onNone) =>
        onNone();
}
