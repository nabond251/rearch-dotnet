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
    /// Returns <i>any</i> data contained within this
    /// <see cref="AsyncValue{T}"/>, including <c>PreviousData</c> for the
    /// <see cref="AsyncLoading{T}"/> and <see cref="AsyncError{T}"/> cases.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <returns>
    /// <see cref="Just{T}"/> of <see cref="AsyncData{T}.Data"/> if
    /// <paramref name="source"/> is an <see cref="AsyncData{T}"/>.<br/>
    /// <see cref="AsyncLoading{T}.PreviousData"/> if <paramref name="source"/>
    /// is an <see cref="AsyncLoading{T}"/>.<br/>
    /// <see cref="AsyncError{T}.PreviousData"/> if <paramref name="source"/> is
    /// an <see cref="AsyncError{T}"/>.
    /// </returns>
    /// <remarks>
    /// See also <see cref="UnwrapOr{T}(AsyncValue{T}, T)"/>, which will only
    /// return the value on the <see cref="AsyncData{T}"/> case.
    /// </remarks>
    public static Maybe<T> GetData<T>(this AsyncValue<T> source) =>
        source.Match(
            onData: Maybe.Just,
            onLoading: previousData => previousData,
            onError: (_, previousData) => previousData);

    /// <summary>
    /// Returns <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is
    /// an <see cref="AsyncData{T}"/>.<br/>
    /// Otherwise, returns <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <param name="defaultValue">
    /// Default value when <see cref="AsyncLoading{T}"/> or
    /// <see cref="AsyncError{T}"/>.
    /// </param>
    /// <returns>
    /// <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is an
    /// <see cref="AsyncData{T}"/>; otherwise, <paramref name="defaultValue"/>.
    /// </returns>
    /// <remarks>
    /// See also <see cref="DataOr{T}(AsyncValue{T}, T)"/>, which will always
    /// return any <c>Data</c>/<c>PreviousData</c> contained within the
    /// <see cref="AsyncValue{T}"/>.
    /// </remarks>
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

    /// <summary>
    /// Returns <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is
    /// an <see cref="AsyncData{T}"/>.<br/>
    /// Otherwise, calls and returns the result of <paramref name="defaultFn"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <param name="defaultFn">
    /// Callback to obtain a result when <see cref="AsyncLoading{T}"/> or
    /// <see cref="AsyncError{T}"/>.
    /// </param>
    /// <returns>
    /// <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is an
    /// <see cref="AsyncData{T}"/>; otherwise, calls and returns the result of
    /// <paramref name="defaultFn"/>.
    /// </returns>
    /// <remarks>
    /// See also <see cref="DataOrElse{T}(AsyncValue{T}, Func{T})"/>, which will
    /// always return any <c>Data</c>/<c>PreviousData</c> contained within the
    /// <see cref="AsyncValue{T}"/>.
    /// </remarks>
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

    /// <summary>
    /// Returns <i>any</i> data contained within this
    /// <see cref="AsyncValue{T}"/>, including <c>PreviousData</c> for the
    /// <see cref="AsyncLoading{T}"/> and <see cref="AsyncError{T}"/> cases.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <param name="defaultValue">
    /// Default value when <see cref="None{T}"/>.
    /// </param>
    /// <returns>
    /// <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is an
    /// <see cref="AsyncData{T}"/>.<br/>
    /// The value contained in <see cref="AsyncLoading{T}.PreviousData"/> if
    /// <paramref name="source"/> is
    /// an <see cref="AsyncLoading{T}"/> and
    /// <see cref="AsyncLoading{T}.PreviousData"/> is <see cref="Just{T}"/>.
    /// <br/>
    /// The value contained in <see cref="AsyncError{T}.PreviousData"/> if
    /// <paramref name="source"/> is an <see cref="AsyncError{T}"/> and
    /// <see cref="AsyncError{T}.PreviousData"/> is <see cref="Just{T}"/>.<br/>
    /// Otherwise, <paramref name="defaultValue"/>.
    /// </returns>
    /// <remarks>
    /// See also <see cref="UnwrapOr{T}(AsyncValue{T}, T)"/>, which will only
    /// return the value on the <see cref="AsyncData{T}"/> case.
    /// </remarks>
    public static T DataOr<T>(
        this AsyncValue<T> source,
        T defaultValue) => source.GetData().UnwrapOr(defaultValue);

    /// <summary>
    /// Returns <i>any</i> data contained within this
    /// <see cref="AsyncValue{T}"/>, including <c>PreviousData</c> for the
    /// <see cref="AsyncLoading{T}"/> and <see cref="AsyncError{T}"/> cases.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <param name="defaultFn">
    /// Callback to obtain a result (when <see cref="None{T}"/>).
    /// </param>
    /// <returns>
    /// <see cref="AsyncData{T}.Data"/> if <paramref name="source"/> is an
    /// <see cref="AsyncData{T}"/>.
    /// the value contained in <see cref="AsyncLoading{T}.PreviousData"/> if
    /// <paramref name="source"/> is
    /// an <see cref="AsyncLoading{T}"/> and
    /// <see cref="AsyncLoading{T}.PreviousData"/> is <see cref="Just{T}"/>.
    /// the value contained in <see cref="AsyncError{T}.PreviousData"/> if
    /// <paramref name="source"/> is an <see cref="AsyncError{T}"/> and
    /// <see cref="AsyncError{T}.PreviousData"/> is <see cref="Just{T}"/>.
    /// Otherwise, calls and returns the result of <paramref name="defaultFn"/>.
    /// </returns>
    /// <remarks>
    /// See also <see cref="UnwrapOrElse{T}(AsyncValue{T}, Func{T})"/>, which
    /// will only return the value on the <see cref="AsyncData{T}"/> case.
    /// </remarks>
    public static T DataOrElse<T>(
        this AsyncValue<T> source,
        Func<T> defaultFn) => source.GetData().UnwrapOrElse(defaultFn);

    /// <summary>
    /// Fills in the <see cref="AsyncLoading{T}.PreviousData"/> or
    /// <see cref="AsyncError{T}.PreviousData"/> with
    /// <paramref name="newPreviousData"/> if
    /// <see cref="AsyncLoading{T}.PreviousData"/> or
    /// <see cref="AsyncError{T}.PreviousData"/> are <see cref="None{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of async data.</typeparam>
    /// <param name="source">The source of the async data.</param>
    /// <param name="newPreviousData">
    /// Data to fill in if none already.
    /// </param>
    /// <returns>
    /// <see cref="AsyncValue{T}"/> with data filled in.
    /// </returns>
    /// <remarks>
    /// If <see cref="AsyncLoading{T}.PreviousData"/> or
    /// <see cref="AsyncError{T}.PreviousData"/> are <see cref="Just{T}"/>, then
    /// <paramref name="newPreviousData"/> will not be filled in.
    /// </remarks>
    public static AsyncValue<T> FillInPreviousData<T>(
        this AsyncValue<T> source,
        Maybe<T> newPreviousData)
    {
        return source switch
        {
            AsyncLoading<T> { PreviousData: None<T> } =>
                new AsyncLoading<T>(newPreviousData),
            AsyncError<T> { PreviousData: None<T> } asyncError =>
                new AsyncError<T>(asyncError.Error, newPreviousData),
            _ => source,
        };
    }

    /// <summary>
    /// Maps a AsyncValue&lt;T&gt; into a AsyncValue&lt;TResult&gt; by applying
    /// the given <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="selector">The selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given <paramref name="selector"/>.
    /// </returns>
    public static AsyncValue<TResult> Select<T, TResult>(
        this AsyncValue<T> source,
        Func<T, TResult> selector)
    {
        return source.SelectMany(
            data => new AsyncData<TResult>(selector(data)));
    }

    /// <summary>
    /// Maps a AsyncValue&lt;T&gt; into a AsyncValue&lt;TResult&gt; by applying
    /// the given <paramref name="selector"/>, and flattens the result.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="selector">The selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given <paramref name="selector"/>.
    /// </returns>
    public static AsyncValue<TResult> SelectMany<T, TResult>(
        this AsyncValue<T> source,
        Func<T, AsyncValue<TResult>> selector)
    {
        return source.Match(
            onData: outerData => selector(outerData),
            onLoading: outerPreviousValue => outerPreviousValue.Match(
                onJust: outerValue => selector(outerValue).Match<AsyncValue<TResult>>(
                    onData: innerData => new AsyncLoading<TResult>(new Just<TResult>(innerData)),
                    onLoading: innerPreviousData => new AsyncLoading<TResult>(innerPreviousData),
                    onError: (error, innerPreviousData) => new AsyncError<TResult>(error, innerPreviousData)),
                onNone: () => new AsyncLoading<TResult>(new None<TResult>())),
            onError: (error, outerPreviousValue) => outerPreviousValue.Match(
                onJust: outerValue => selector(outerValue).Match<AsyncValue<TResult>>(
                    onData: innerData => new AsyncError<TResult>(error, new Just<TResult>(innerData)),
                    onLoading: innerPreviousData => new AsyncError<TResult>(error, innerPreviousData),
                    onError: (error, innerPreviousData) => new AsyncError<TResult>(error, innerPreviousData)),
                onNone: () => new AsyncError<TResult>(error, new None<TResult>())));
    }

    /// <summary>
    /// Maps a AsyncValue&lt;T&gt; into a AsyncValue&lt;TResult&gt; by applying
    /// the given <paramref name="maybeSelector"/>, flattens the result, and
    /// applies the given <paramref name="resultSelector"/>.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TAsyncValue">
    /// Type of nested immutable value.
    /// </typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="maybeSelector">The selector to apply.</param>
    /// <param name="resultSelector">The final result selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given
    /// <paramref name="maybeSelector"/>, and then by given
    /// <paramref name="resultSelector"/>.
    /// </returns>
    public static AsyncValue<TResult> SelectMany<T, TAsyncValue, TResult>(
        this AsyncValue<T> source,
        Func<T, AsyncValue<TAsyncValue>> maybeSelector,
        Func<T, TAsyncValue, TResult> resultSelector)
    {
        return source
            .SelectMany(x => maybeSelector(x)
            .Select(y => resultSelector(x, y)));
    }
}
