// <copyright file="AsyncValue{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// The current state of a <see cref="Task{TResult}"/> or
/// <see cref="IObservable{T}"/>, accessible from a synchronous context.
/// </summary>
/// <typeparam name="T">Type of async data.</typeparam>
/// <remarks>
/// One of three variants: <see cref="AsyncData{T}"/>,
/// <see cref="AsyncError{T}"/>, or <see cref="AsyncLoading{T}"/>.<br/>
/// <br/>
/// Often, when a <see cref="Task{TResult}"/>/<see cref="IObservable{T}"/> emits
/// an error, or is swapped out and is put back into the loading state, you want
/// access to the previous data. (Example: pull-to-refresh in UI and you want to
/// show the current data.) Thus, a `previousData` is provided in the
/// <see cref="AsyncError{T}"/> and <see cref="AsyncLoading{T}"/> states so you
/// can access the previous data (if it exists).
/// </remarks>
public abstract class AsyncValue<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncValue{T}"/> class.
    /// Base constructor for <see cref="AsyncValue{T}"/>s.
    /// </summary>
    private protected AsyncValue()
    {
    }

    /// <summary>
    /// Uses the value with the matching case callback.
    /// </summary>
    /// <param name="onData">Callback to execute if data.</param>
    /// <param name="onError">Callback to execute if error.</param>
    /// <param name="onLoading">Callback to execute if loading.</param>
    public void Match(
        Action<T> onData,
        Action<Exception, Maybe<T>> onError,
        Action<Maybe<T>> onLoading) =>
        this.Match<object?>(
            onData: data =>
            {
                onData(data);
                return null;
            },
            onError: (error, previousData) =>
            {
                onError(error, previousData);
                return null;
            },
            onLoading: previousData =>
            {
                onLoading(previousData);
                return null;
            });

    /// <summary>
    /// Transforms the value with the matching case callback.
    /// </summary>
    /// <typeparam name="TResult">Type of transformed result value.</typeparam>
    /// <param name="onData">Callback to transform value if data.</param>
    /// <param name="onError">Callback to transform value if error.</param>
    /// <param name="onLoading">Callback to transform value if loading.</param>
    /// <returns>Transformed value from the matching case.</returns>
    public abstract TResult Match<TResult>(
        Func<T, TResult> onData,
        Func<Exception, Maybe<T>, TResult> onError,
        Func<Maybe<T>, TResult> onLoading);
}
