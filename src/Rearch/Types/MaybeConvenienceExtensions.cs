// <copyright file="MaybeConvenienceExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

using System;

namespace Rearch.Types;

/// <summary>
/// Convenience methods for handling <see cref="Maybe{T}"/>s.
/// </summary>
/// <remarks>
/// Help is wanted here! Please open PRs to add any methods you want!
/// When possible, try to follow the function names in Rust for Option:
/// - https://doc.rust-lang.org/std/option/enum.Maybe.html.
/// </remarks>
public static class MaybeConvenienceExtensions
{
    /// <summary>
    /// Returns <see cref="Just{T}.Value"/> if <paramref name="source"/> is a
    /// <see cref="Just{T}"/>.<br/>
    /// Otherwise, returns <paramref name="defaultValue"/> (when
    /// <see cref="None{T}"/>).
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="defaultValue">
    /// Default value when <see cref="None{T}"/>.
    /// </param>
    /// <returns>
    /// <see cref="Just{T}.Value"/> if <paramref name="source"/> is a
    /// <see cref="Just{T}"/>;<br/> otherwise, <paramref name="defaultValue"/>
    /// (when <see cref="None{T}"/>).
    /// </returns>
    public static T UnwrapOr<T>(
        this Maybe<T> source,
        T defaultValue) =>
        source.Match(
            onJust: value => value,
            onNone: () => defaultValue);

    /// <summary>
    /// Returns <see cref="Just{T}.Value"/> if <paramref name="source"/> is a
    /// <see cref="Just{T}"/>.<br/>
    /// Otherwise, calls and returns the result of <paramref name="defaultFn"/>
    /// (when <see cref="None{T}"/>).
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="defaultFn">
    /// Callback to obtain a result (when <see cref="None{T}"/>).
    /// </param>
    /// <returns>
    /// <see cref="Just{T}.Value"/> if <paramref name="source"/> is a
    /// <see cref="Just{T}"/>;<br/>otherwise, calls and returns the result of
    /// <paramref name="defaultFn"/> (when <see cref="None{T}"/>).
    /// </returns>
    public static T UnwrapOrElse<T>(
        this Maybe<T> source,
        Func<T> defaultFn) =>
        source.Match(
            onJust: value => value,
            onNone: defaultFn);

    /// <summary>
    /// Returns <see cref="Just{T}.Value"/> or <c>default</c> for
    /// <see cref="None{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <returns>
    /// <see cref="Just{T}.Value"/> or <c>default</c> for
    /// <see cref="None{T}"/>.
    /// </returns>
    public static T? AsNullable<T>(this Maybe<T> source) =>
        source.Match(
            onJust: value => value,
            onNone: () => default!);

    /// <summary>
    /// Maps a Maybe&lt;T&gt; into a Maybe&lt;TResult&gt; by applying the given
    /// <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="selector">The selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given <paramref name="selector"/>.
    /// </returns>
    /// <remarks>
    /// Only calls <paramref name="selector"/> when this <see cref="Maybe"/> is
    /// <see cref="Just{T}"/>.
    /// </remarks>
    public static Maybe<TResult> Select<T, TResult>(
        this Maybe<T> source,
        Func<T, TResult> selector)
    {
        return source.SelectMany(value => new Just<TResult>(selector(value)));
    }

    /// <summary>
    /// Maps a Maybe&lt;T&gt; into a Maybe&lt;TResult&gt; by applying the given
    /// <paramref name="selector"/>, and flattens the result.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="selector">The selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given <paramref name="selector"/>.
    /// </returns>
    /// <remarks>
    /// Only calls <paramref name="selector"/> when this <see cref="Maybe"/> is
    /// <see cref="Just{T}"/>.
    /// </remarks>
    public static Maybe<TResult> SelectMany<T, TResult>(
        this Maybe<T> source,
        Func<T, Maybe<TResult>> selector)
    {
        return source.Match(
            onJust: value => selector(value),
            onNone: () => new None<TResult>());
    }

    /// <summary>
    /// Maps a Maybe&lt;T&gt; into a Maybe&lt;TResult&gt; by applying the given
    /// <paramref name="maybeSelector"/>, flattens the result, and applies the
    /// given <paramref name="resultSelector"/>.
    /// </summary>
    /// <typeparam name="T">Type of associated immutable value.</typeparam>
    /// <typeparam name="TMaybe">Type of nested immutable value.</typeparam>
    /// <typeparam name="TResult">Type of resulting immutable value.</typeparam>
    /// <param name="source">The source of the optional value.</param>
    /// <param name="maybeSelector">The selector to apply.</param>
    /// <param name="resultSelector">The final result selector to apply.</param>
    /// <returns>
    /// <paramref name="source"/> mapped via given
    /// <paramref name="maybeSelector"/>, and then by given
    /// <paramref name="resultSelector"/>.
    /// </returns>
    /// <remarks>
    /// Only calls <paramref name="maybeSelector"/> when this
    /// <see cref="Maybe"/> is <see cref="Just{T}"/>.
    /// </remarks>
    public static Maybe<TResult> SelectMany<T, TMaybe, TResult>(
        this Maybe<T> source,
        Func<T, Maybe<TMaybe>> maybeSelector,
        Func<T, TMaybe, TResult> resultSelector)
    {
        return source
            .SelectMany(x => maybeSelector(x)
            .Select(y => resultSelector(x, y)));
    }
}
