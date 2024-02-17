// <copyright file="Maybe.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// <br/>
/// <br/>
/// A <see cref="Maybe{T}"/> is either:
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
    /// <summary>
    /// Initializes a new instance of the <see cref="Maybe{T}"/> class.
    /// Base constructor for <see cref="Maybe{T}"/>s.
    /// </summary>
    private protected Maybe()
    {
    }

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> with the associated immutable
    /// <see cref="Just{T}.Value"/>.
    /// </summary>
    /// <param name="value">Associated immutable value.</param>
    /// <returns>New <see cref="Just{T}"/>.</returns>
    public Maybe<T> Just(T value) => new Just<T>(value);

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> that does not have a value.
    /// </summary>
    /// <returns>New <see cref="None{T}"/>.</returns>
    public Maybe<T> None() => new None<T>();
}
