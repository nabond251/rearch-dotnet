// <copyright file="AsyncValue.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

using Rearch.Types;

namespace Rearch;

/// <summary>
/// The current state of a [Future] or [Stream],
/// accessible from a synchronous context.
///
/// One of three variants: [AsyncData], [AsyncError], or [AsyncLoading].
///
/// Often, when a [Future]/[Stream] emits an error, or is swapped out and is put
/// back into the loading state, you want access to the previous data.
/// (Example: pull-to-refresh in UI and you want to show the current data.)
/// Thus, a `previousData` is provided in the [AsyncError] and [AsyncLoading]
/// states so you can access the previous data (if it exists).
/// </summary>
public class AsyncValue<T> {
    /// Base constructor for [AsyncValue]s.
    private protected AsyncValue()
    {
    }

    /// Shortcut for [AsyncData.new].
    public static AsyncValue<T> Data(T data) => new AsyncData<T>(data);

    /// Shortcut for [AsyncError.new].
    public static AsyncValue<T> Error(
        Exception ex,
        Maybe<T> previousData) => new AsyncError<T>(ex, previousData);

    /// Shortcut for [AsyncLoading.new].
    public static AsyncValue<T> Loading(Maybe<T> previousData) => new AsyncLoading<T>(previousData);

    /// Transforms a fallible [Future] into a safe-to-read [AsyncValue].
    /// Useful when mutating state.
    static async Task<AsyncValue<T>> Guard<T>(Func<Task<T>> fn)
    {
        try
        {
            return new AsyncData<T>(await fn());
        }
        catch (Exception ex)
        {
            return new AsyncError<T>(ex, new None<T>());
        }
    }
}

/// <summary>
/// The data variant for an [AsyncValue].
///
/// To be in this state, a [Future] or [Stream] emitted a data event.
/// </summary>
public sealed class AsyncData<T> : AsyncValue<T> {
    /// Creates an [AsyncData] with the supplied [data].
    public AsyncData(T data)
    {
        this.data = data;
    }

    /// The data of this [AsyncData].
    public readonly T data;

    public override int GetHashCode() => data.GetHashCode();

    public override bool Equals(object? other) => other is AsyncData<T> asyncData && EqualityComparer<T>.Default.Equals(asyncData.data, data);

    public override string ToString() => $"AsyncData(data: {data})";
}

/// <summary>
/// The loading variant for an [AsyncValue].
///
/// To be in this state, a new [Future] or [Stream] has not emitted
/// a data or error event yet.
/// </summary>
public sealed class AsyncLoading<T> : AsyncValue<T>
{
    /// Creates an [AsyncLoading] with the supplied [previousData].
    public AsyncLoading(Maybe<T> previousData)
    {
        this.previousData = previousData;
    }

    /// The previous data (from a predecessor [AsyncData]), if it exists.
    /// This can happen if a new [Future]/[Stream] is watched and the
    /// [Future]/[Stream] it is replacing was in the [AsyncData] state.
    public readonly Maybe<T> previousData;

    public override int GetHashCode() => previousData.GetHashCode();

    public override bool Equals(object? other) =>
            other is AsyncLoading<T> asyncLoading && asyncLoading.previousData.Equals(previousData);

    public override string ToString() => $"AsyncLoading(previousData: {previousData})";
}

/// <summary>
/// The error variant for an [AsyncValue].
///
/// To be in this state, a [Future] or [Stream] emitted an error event.
/// </summary>
public sealed class AsyncError<T> : AsyncValue<T>
{
    /// Creates an [AsyncError] with the supplied [error], [stackTrace],
    /// and [previousData].
    public AsyncError(Exception ex, Maybe<T> previousData)
    {
        this.ex = ex;
        this.previousData = previousData;
    }

    /// The emitted error associated with this [AsyncError].
    public readonly Exception ex;

    /// The previous data (from a predecessor [AsyncData]), if it exists.
    /// This can happen if a new [Future]/[Stream] is watched and the
    /// [Future]/[Stream] it is replacing was in the [AsyncData] state.
    public readonly Maybe<T> previousData;

    public override int GetHashCode() => HashCode.Combine(ex, previousData);

    public override bool Equals(object? other) =>
            other is AsyncError<T> asyncError &&
            asyncError.ex.Equals(ex) &&
            asyncError.previousData.Equals(previousData);

    public override string ToString() =>
        $"AsyncError(previousData: {previousData}, ex: ${ex})";
}
