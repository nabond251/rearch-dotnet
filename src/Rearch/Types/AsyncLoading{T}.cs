// <copyright file="AsyncLoading{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// The loading variant for an <see cref="AsyncValue{T}"/>.
/// </summary>
/// <typeparam name="T">Type of async data.</typeparam>
/// <remarks>
/// To be in this state, a new <see cref="Task{TResult}"/> or
/// <see cref="IObservable{T}"/> has not emitted a data or error event yet.
/// </remarks>
public sealed class AsyncLoading<T> : AsyncValue<T>, IEquatable<AsyncLoading<T>?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLoading{T}"/> class.
    /// Creates an <see cref="AsyncLoading{T}"/> with the supplied
    /// <paramref name="previousData"/>.
    /// </summary>
    /// <param name="previousData">
    /// The previous data (from a predecessor <see cref="AsyncData{T}"/>), if it
    /// exists.
    /// </param>
    public AsyncLoading(Maybe<T> previousData)
    {
        this.PreviousData = previousData;
    }

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
    public static bool operator ==(AsyncLoading<T>? left, AsyncLoading<T>? right)
    {
        return EqualityComparer<AsyncLoading<T>>.Default.Equals(left, right);
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
    public static bool operator !=(AsyncLoading<T>? left, AsyncLoading<T>? right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as AsyncLoading<T>);
    }

    /// <inheritdoc/>
    public bool Equals(AsyncLoading<T>? other)
    {
        return other is not null &&
               EqualityComparer<Maybe<T>>.Default.Equals(this.PreviousData, other.PreviousData);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.PreviousData);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"AsyncLoading(previousData: {this.PreviousData})";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onData,
        Func<Exception, Maybe<T>, TResult> onError,
        Func<Maybe<T>, TResult> onLoading) =>
        onLoading(this.PreviousData);
}
