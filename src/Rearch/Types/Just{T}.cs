// <copyright file="Just{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// A <see cref="Maybe{T}"/> that has a <see cref="Value"/>.
/// </summary>
/// <typeparam name="T">Type of associated immutable value.</typeparam>
public sealed class Just<T> : Maybe<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Just{T}"/> class.
    /// Creates a <see cref="Maybe{T}"/> with the associated immutable
    /// <see cref="Value"/>.
    /// </summary>
    /// <param name="value">
    /// The immutable <see cref="Value"/> associated with this
    /// <see cref="Maybe{T}"/>.
    /// </param>
    public Just(T value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the immutable <see cref="Value"/> associated with this
    /// <see cref="Maybe{T}"/>.
    /// </summary>
    public T Value { get; }

    /// <inheritdoc/>
    public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Just<T> some &&
        EqualityComparer<T>.Default.Equals(some.Value, this.Value);

    /// <inheritdoc/>
    public override string ToString() => $"Just(value: {this.Value})";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onJust,
        Func<TResult> onNone) =>
        onJust(this.Value);
}
