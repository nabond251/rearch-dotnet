// <copyright file="Maybe{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Types;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type of contained data, if any.</typeparam>
/// <remarks>
/// A <see cref="Maybe{T}"/> is either:
/// <list type="bullet">
/// <item>
/// <see cref="Just{T}"/>, which contains a value of type
/// <typeparamref name="T"/>
/// </item>
/// <item><see cref="None{T}"/>, which does not contain a value</item>
/// </list>
/// Adapted from Rust's <c>Maybe</c>, see more here:
/// https://doc.rust-lang.org/std/option/index.html.
/// </remarks>
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
    /// Uses the value with the matching case callback.
    /// </summary>
    /// <param name="onJust">Callback to execute with value.</param>
    /// <param name="onNone">Callback to execute if none.</param>
    public void Match(
        Action<T> onJust,
        Action onNone) =>
        this.Match<object?>(
            onJust: value =>
            {
                onJust(value);
                return null;
            },
            onNone: () =>
            {
                onNone();
                return null;
            });

    /// <summary>
    /// Transforms the value with the matching case callback.
    /// </summary>
    /// <typeparam name="TResult">Type of transformed result value.</typeparam>
    /// <param name="onJust">Callback to transform value.</param>
    /// <param name="onNone">Callback to obtain value if none.</param>
    /// <returns>Transformed value from the matching case.</returns>
    public abstract TResult Match<TResult>(
        Func<T, TResult> onJust,
        Func<TResult> onNone);
}
