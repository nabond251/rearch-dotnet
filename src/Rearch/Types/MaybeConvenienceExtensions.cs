// <copyright file="MaybeConvenienceExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

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
        T defaultValue)
    {
        return source switch
        {
            Just<T> just => just.Value,
            _ => defaultValue,
        };
    }

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
        Func<T> defaultFn)
    {
        return source switch
        {
            Just<T> just => just.Value,
            _ => defaultFn(),
        };
    }

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
    public static T? AsNullable<T>(this Maybe<T> source)
    {
        return source switch
        {
            Just<T> just => just.Value,
            _ => default,
        };
    }
}
