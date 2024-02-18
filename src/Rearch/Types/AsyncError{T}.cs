// <copyright file="AsyncError{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// The error variant for an <see cref="AsyncValue{T}"/>.
/// </summary>
/// <typeparam name="T">Type of async data.</typeparam>
/// <remarks>
/// To be in this state, a <see cref="Task{TResult}"/> or
/// <see cref="IObservable{T}"/> emitted an error event.
/// </remarks>
public sealed class AsyncError<T> : AsyncValue<T>, IEquatable<AsyncError<T>?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncError{T}"/> class.
    /// Creates an <see cref="AsyncError{T}"/> with the supplied
    /// <paramref name="error"/> and <paramref name="previousData"/>.
    /// </summary>
    /// <param name="error">
    /// The emitted error associated with this <see cref="AsyncError{T}"/>.
    /// </param>
    /// <param name="previousData">
    /// The previous data (from a predecessor <see cref="AsyncData{T}"/>), if it
    /// exists.
    /// </param>
    public AsyncError(Exception error, Maybe<T> previousData)
    {
        this.Error = error;
        this.PreviousData = previousData;
    }

    /// <summary>
    /// Gets the emitted error associated with this <see cref="AsyncError{T}"/>.
    /// </summary>
    public Exception Error { get; }

    /// <summary>
    /// Gets the previous data (from a predecessor <see cref="AsyncData{T}"/>),
    /// if it exists.
    /// </summary>
    /// <remarks>
    /// This can happen if a new
    /// <see cref="Task{TResult}"/>/<see cref="IObservable{T}"/> is watched and
    /// the <see cref="Task{TResult}"/>/<see cref="IObservable{T}"/> it is
    /// replacing was in the <see cref="AsyncData{T}"/> state.
    /// </remarks>
    public Maybe<T> PreviousData { get; }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left-hand operand.</param>
    /// <param name="right">Right-hand operand.</param>
    /// <returns>
    /// A value indicating whether <paramref name="left"/> and
    /// <paramref name="right"/> are equal.
    /// </returns>
    public static bool operator ==(AsyncError<T>? left, AsyncError<T>? right)
    {
        return EqualityComparer<AsyncError<T>>.Default.Equals(left, right);
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
    public static bool operator !=(AsyncError<T>? left, AsyncError<T>? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as AsyncError<T>);
    }

    /// <inheritdoc/>
    public bool Equals(AsyncError<T>? other)
    {
        return other is not null &&
               EqualityComparer<Exception>.Default.Equals(this.Error, other.Error) &&
               EqualityComparer<Maybe<T>>.Default.Equals(this.PreviousData, other.PreviousData);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Error, this.PreviousData);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"AsyncError(previousData: {this.PreviousData}, error: ${this.Error})";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onData,
        Func<Exception, Maybe<T>, TResult> onError,
        Func<Maybe<T>, TResult> onLoading) =>
        onError(this.Error, this.PreviousData);
}
