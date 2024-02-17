// <copyright file="None{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// A <see cref="Maybe{T}"/> that does not have a value.
/// </summary>
/// <typeparam name="T">Type of associated immutable value.</typeparam>
public sealed class None<T> : Maybe<T>, IEquatable<None<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None{T}"/> class.
    /// Creates a <see cref="Maybe{T}"/> that does not have a value.
    /// </summary>
    public None()
    {
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>
    /// A value indicating whether <paramref name="left"/> and
    /// <paramref name="right"/> are equal.
    /// </returns>
    public static bool operator ==(None<T>? left, None<T>? right)
    {
        return EqualityComparer<None<T>>.Default.Equals(left, right);
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
    public static bool operator !=(None<T>? left, None<T>? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as None<T>);
    }

    /// <inheritdoc/>
    public bool Equals(None<T>? other)
    {
        return other is not null;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return 0;
    }

    /// <inheritdoc/>
    public override string ToString() => "None()";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onJust,
        Func<TResult> onNone) =>
        onNone();
}
