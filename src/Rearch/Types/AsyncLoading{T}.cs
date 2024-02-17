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
public sealed class AsyncLoading<T> : AsyncValue<T>
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

    /// <inheritdoc/>
    public override int GetHashCode() => this.PreviousData.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is AsyncLoading<T> asyncLoading &&
        asyncLoading.PreviousData.Equals(this.PreviousData);

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
