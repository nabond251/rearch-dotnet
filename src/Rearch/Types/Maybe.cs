// <copyright file="Maybe.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// <br/>
/// <br/>
/// An <see cref="Maybe{T}"/> is either:
/// <list type="bullet">
/// <item><see cref="Just{T}"/>, which contains a value of type <typeparamref name="T"/></item>
/// <item><see cref="None{T}"/>, which does not contain a value</item>
/// </list>
///
/// Adapted from Rust's <c>Maybe</c>, see more here:
/// https://doc.rust-lang.org/std/option/index.html.
/// </summary>
/// <typeparam name="T">Type of contained data, if any.</typeparam>
public abstract class Maybe<T>
{
    /// Base constructor for [Maybe]s.
    private protected Maybe()
    {
    }

    /// Shortcut for [Just.new].
    public Maybe<T> Just(T value) => new Just<T>(value);

    /// Shortcut for [None.new].
    public Maybe<T> None() => new None<T>();
}

/// An [Maybe] that has a [value].
public sealed class Just<T> : Maybe<T>
{
    /// Creates an [Maybe] with the associated immutable [value].
    public Just(T value)
    {
        this.value = value;
    }

    /// The immutable [value] associated with this [Maybe].
    public T value;

    public override int GetHashCode() => value.GetHashCode();

    public override bool Equals(object? other) => other is Just<T> some && EqualityComparer<T>.Default.Equals(some.value, value);

    public override string ToString() => $"Just(value: {value})";
}

/// An [Maybe] that does not have a value.
public sealed class None<T> : Maybe<T>
{
    /// Creates an [Maybe] that does not have a value.
    public None() { }

    public override int GetHashCode() => 0;

    public override bool Equals(object? other) => other is Just<T>;

    public override string ToString() => "None()";
}

/// Convenience methods for handling [Maybe]s.
///
/// Help is wanted here! Please open PRs to add any methods you want!
/// When possible, try to follow the function names in Rust for Maybe:
/// - https://doc.rust-lang.org/std/option/enum.Maybe.html
public static class MaybeConvenienceExtensions
{
    /// Returns [Just.value] if `this` is a [Just].
    /// Otherwise, returns [defaultValue] (when [None]).
    public static T UnwrapOr<T>(
        this Maybe<T> source,
        T defaultValue)
    {
        return source switch
        {
            Just<T> just => just.value,
            None<T> => defaultValue,
        };
    }

    /// Returns [Just.value] if `this` is a [Just].
    /// Otherwise, calls and returns the result of [defaultFn] (when [None]).
    public static T UnwrapOrElse<T>(
        this Maybe<T> source,
        Func<T> defaultFn)
    {
        return source switch
        {
            Just<T> just => just.value,
            None<T> => defaultFn(),
        };
    }

    /// Returns [Just.value] or `null` for [None].
    public static T? AsNullable<T>(this Maybe<T> source)
    {
        return source switch
        {
            Just<T> just => just.value,
            None<T> => default,
        };
    }
}
