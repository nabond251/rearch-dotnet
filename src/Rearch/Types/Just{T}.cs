// <copyright file="Just{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// A <see cref="Maybe{T}"/> that has a <see cref="Value"/>.
/// </summary>
/// <typeparam name="T">Type of associated immutable value.</typeparam>
public sealed class Just<T> : Maybe<T>, IEquatable<Just<T>?>
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

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>
    /// A value indicating whether <paramref name="left"/> and
    /// <paramref name="right"/> are equal.
    /// </returns>
    public static bool operator ==(Just<T>? left, Just<T>? right)
    {
        return EqualityComparer<Just<T>>.Default.Equals(left, right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>
    /// A value indicating whether <paramref name="left"/> and
    /// <paramref name="right"/> are not equal.
    /// </returns>
    public static bool operator !=(Just<T>? left, Just<T>? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Just<T>);
    }

    /// <inheritdoc/>
    public bool Equals(Just<T>? other)
    {
        return other is not null &&
               EqualityComparer<T>.Default.Equals(this.Value, other.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Value);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Just(value: {this.Value})";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onJust,
        Func<TResult> onNone) =>
        onJust(this.Value);
}
