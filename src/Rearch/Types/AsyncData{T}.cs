// <copyright file="AsyncData{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// The data variant for an <see cref="AsyncValue{T}"/>.
/// </summary>
/// <typeparam name="T">Type of async data.</typeparam>
/// <remarks>
/// To be in this state, a <see cref="Task{TResult}"/> or
/// <see cref="IObservable{T}"/> emitted a data event.
/// </remarks>
public sealed class AsyncData<T> : AsyncValue<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncData{T}"/> class.
    /// Creates an <see cref="AsyncData{T}"/> with the supplied
    /// <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The data of this <see cref="AsyncData{T}"/>.</param>
    public AsyncData(T data)
    {
        this.Data = data;
    }

    /// <summary>
    /// Gets the data of this <see cref="AsyncData{T}"/>.
    /// </summary>
    public T Data { get; }

    /// <inheritdoc/>
    public override int GetHashCode() => this.Data?.GetHashCode() ?? 0;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is AsyncData<T> asyncData &&
        EqualityComparer<T>.Default.Equals(asyncData.Data, this.Data);

    /// <inheritdoc/>
    public override string ToString() => $"AsyncData(data: {this.Data})";

    /// <inheritdoc/>
    public override TResult Match<TResult>(
        Func<T, TResult> onData,
        Func<Exception, Maybe<T>, TResult> onError,
        Func<Maybe<T>, TResult> onLoading) =>
        onData(this.Data);
}
