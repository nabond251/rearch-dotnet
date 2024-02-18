// <copyright file="AsyncValueConvenienceExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Convenience methods for handling <see cref="AsyncValue{T}"/>s.
/// </summary>
/// <remarks>
/// Help is wanted here! Please open PRs to add any methods you want!<br/>
/// When possible, try to follow function names in Rust for Result/Maybe:
/// <list type="bullet">
/// <item>https://doc.rust-lang.org/std/option/enum.Maybe.html</item>
/// <item>https://doc.rust-lang.org/std/result/enum.Result.html</item>
/// </list>
/// </remarks>
public static class AsyncValueConvenienceExtensions
{
    /// <summary>
    /// Returns <i>any</i> data contained within this <see cref="AsyncValue{T}"/>,
    /// including `previousData` for the [AsyncLoading] and [AsyncError] cases.
    /// </summary>
    /// <returns>
    /// Returns [Some] of [AsyncData.data] if `this` is an [AsyncData].<br/>
    /// Returns [AsyncLoading.previousData] if `this` is an [AsyncLoading].<br/>
    /// Returns [AsyncError.previousData] if `this` is an [AsyncError].
    /// </returns>
    /// <remarks>
    /// See also [unwrapOr], which will only return the value
    /// on the [AsyncData] case.
    /// </remarks>
    public static Maybe<T> GetData<T>(this AsyncValue<T> source) =>
        source.Match(
            onData: Maybe.Just,
            onLoading: previousData => previousData,
            onError: (_, previousData) => previousData);

    /// Returns [AsyncData.data] if `this` is an [AsyncData].
    /// Otherwise, returns [defaultValue].
    ///
    /// See also [dataOr], which will always return any `data`/`previousData`
    /// contained within the <see cref="AsyncValue{T}"/>.
    public static T UnwrapOr<T>(
        this AsyncValue<T> source,
        T defaultValue)
    {
        return source switch
        {
            AsyncData<T> asyncData => asyncData.Data,
            _ => defaultValue,
        };
    }

    /// Returns [AsyncData.data] if `this` is an [AsyncData].
    /// Otherwise, calls and returns the result of [defaultFn].
    ///
    /// See also [dataOrElse], which will always return any `data`/`previousData`
    /// contained within the <see cref="AsyncValue{T}"/>.
    public static T UnwrapOrElse<T>(
        this AsyncValue<T> source,
        Func<T> defaultFn)
    {
        return source switch
        {
            AsyncData<T> asyncData => asyncData.Data,
            _ => defaultFn(),
        };
    }

    /// Returns *any* data contained within this <see cref="AsyncValue{T}"/>,
    /// including `previousData` for the [AsyncLoading] and [AsyncError] cases.
    ///
    /// Returns [AsyncData.data] if `this` is an [AsyncData].
    /// Returns the value contained in [AsyncLoading.previousData] if `this` is
    /// an [AsyncLoading] and [AsyncLoading.previousData] is [Some].
    /// Returns the value contained in [AsyncError.previousData] if `this` is
    /// an [AsyncError] and [AsyncError.previousData] is [Some].
    /// Otherwise, returns [defaultValue].
    ///
    /// See also [unwrapOr], which will only return the value
    /// on the [AsyncData] case.
    public static T DataOr<T>(
        this AsyncValue<T> source,
        T defaultValue) => source.GetData().UnwrapOr(defaultValue);

    /// Returns *any* data contained within this <see cref="AsyncValue{T}"/>,
    /// including `previousData` for the [AsyncLoading] and [AsyncError] cases.
    ///
    /// Returns [AsyncData.data] if `this` is an [AsyncData].
    /// Returns the value contained in [AsyncLoading.previousData] if `this` is
    /// an [AsyncLoading] and [AsyncLoading.previousData] is [Some].
    /// Returns the value contained in [AsyncError.previousData] if `this` is
    /// an [AsyncError] and [AsyncError.previousData] is [Some].
    /// Otherwise, calls and returns the result of [defaultFn].
    ///
    /// See also [unwrapOrElse], which will only return the value
    /// on the [AsyncData] case.
    public static T DataOrElse<T>(
        this AsyncValue<T> source,
        Func<T> defaultFn) => source.GetData().UnwrapOrElse(defaultFn);

    /// Fills in the [AsyncLoading.previousData] or [AsyncError.previousData] with
    /// [newPreviousData] if [AsyncLoading.previousData] or
    /// [AsyncError.previousData] are [None].
    /// If [AsyncLoading.previousData] or [AsyncError.previousData] are [Some],
    /// then [newPreviousData] will not be filled in.
    public static AsyncValue<T> FillInPreviousData<T>(
        this AsyncValue<T> source,
        Maybe<T> newPreviousData)
    {
        return source switch
        {
            AsyncLoading<T> { PreviousData: None<T> } => new AsyncLoading<T>(newPreviousData),
            AsyncError<T> { PreviousData: None<T> } asyncError =>
                new AsyncError<T>(asyncError.Error, newPreviousData),
            _ => source,
        };
    }
}