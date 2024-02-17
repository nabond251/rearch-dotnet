// <copyright file="AsyncValue.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Contains <see cref="AsyncValue{T}"/> factory methods.
/// </summary>
public static class AsyncValue
{
    /// <summary>
    /// Creates an <see cref="AsyncData{T}"/> with the supplied <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="data">The data of this <see cref="AsyncData{T}"/>.</param>
    /// <returns>New <see cref="AsyncData{T}"/>.</returns>
    public static AsyncValue<T> Data<T>(T data) => new AsyncData<T>(data);

    /// <summary>
    /// Creates an <see cref="AsyncError{T}"/> with the supplied <paramref name="error"/>
    /// and <paramref name="previousData"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="error">
    /// The emitted error associated with this <see cref="AsyncError{T}"/>.
    /// </param>
    /// <param name="previousData">
    /// The previous data (from a predecessor <see cref="AsyncData{T}"/>), if it exists.
    /// </param>
    /// <returns>New <see cref="AsyncError{T}"/>.</returns>
    public static AsyncValue<T> Error<T>(
        Exception error,
        Maybe<T> previousData) => new AsyncError<T>(error, previousData);

    /// <summary>
    /// Creates an <see cref="AsyncLoading{T}"/> with the supplied <paramref name="previousData"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="previousData">
    /// The previous data (from a predecessor <see cref="AsyncData{T}"/>), if it exists.
    /// </param>
    /// <returns>New <see cref="AsyncLoading{T}"/>.</returns>
    public static AsyncValue<T> Loading<T>(Maybe<T> previousData) => new AsyncLoading<T>(previousData);

    /// <summary>
    /// Transforms a fallible <see cref="Task{TResult}"/> into a safe-to-read <see cref="AsyncValue{T}"/>.
    /// Useful when mutating state.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="fn">
    /// Function resulting in <see cref="Task{TResult}"/> to transform.
    /// </param>
    /// <returns>
    /// Result of <paramref name="fn"/> transformed into a safe-to-read
    /// <see cref="AsyncValue{T}"/>.
    /// </returns>
    public static async Task<AsyncValue<T>> Guard<T>(Func<Task<T>> fn)
    {
        try
        {
            return new AsyncData<T>(await fn());
        }
        catch (Exception error)
        {
            return new AsyncError<T>(error, new None<T>());
        }
    }
}
