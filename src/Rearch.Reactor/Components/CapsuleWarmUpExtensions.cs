// <copyright file="CapsuleWarmUpExtensions.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor.Components;

using MauiReactor;
using Rearch;
using Rearch.Types;

/// <summary>
/// Provides <see cref="ToWarmUpComponent"/>, a mechanism to create a
/// <see cref="Component"/> from a <see cref="IList{T}"/> of the current states
/// of some "warm up" <see cref="Capsule{T}"/>s.
/// </summary>
public static class CapsuleWarmUpExtensions
{
    /// <summary>
    /// Creates a <see cref="Component"/> from a <see cref="IList{T}"/> of the
    /// current states of some "warm up" <see cref="Capsule{T}"/>s.
    /// </summary>
    /// <param name="source">
    /// <see cref="IList{T}"/> of the current states of some "warm up"
    /// <see cref="Capsule{T}"/>s.
    /// </param>
    /// <param name="errorBuilder">
    /// Called to build the returned <see cref="Component"/> when any of the
    /// current states are <see cref="AsyncError{T}"/>.
    /// </param>
    /// <param name="loading">
    /// Returned when any of the current states are
    /// <see cref="AsyncLoading{T}"/>.
    /// </param>
    /// <param name="child">
    /// Returned when all of the current states are <see cref="AsyncData{T}"/>.
    /// </param>
    /// <returns>
    /// Error, <paramref name="loading"/>, or <paramref name="child"/>
    /// component, based on given states.
    /// </returns>
    public static Component ToWarmUpComponent(
        this IList<AsyncValue<dynamic>> source,
        Func<IList<AsyncError<dynamic>>, Component> errorBuilder,
        Component loading,
        Component child)
    {
        // Check for any errors first
        var asyncErrors = source.OfType<AsyncError<dynamic>>().ToList();
        if (asyncErrors.Count > 0)
        {
            return errorBuilder(asyncErrors);
        }

        // Check to see if we have any still loading
        if (source.Any((value) => value is AsyncLoading<dynamic>))
        {
            return loading;
        }

        // We have only AsyncData (no loading or error), so return the child
        return child;
    }
}
