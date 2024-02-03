// <copyright file="IComponentSideEffectApi.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch.Reactor;

using Rearch.Reactor.Components;

/// <summary>
/// An <c>Action</c> callback.
/// </summary>
public delegate void ComponentSideEffectApiCallback();

/// <summary>
/// The API exposed to <see cref="CapsuleConsumer"/>s to extend their
/// functionality.
/// </summary>
public interface IComponentSideEffectApi : ISideEffectApi
{
    CapsuleConsumer Context { get; }

    /// <summary>
    /// Adds an unmount lifecycle listener.
    /// </summary>
    /// <param name="callback">Callback added for executing on unmount.</param>
    void AddUnmountListener(SideEffectApiCallback callback);

    /// <summary>
    /// Removes the specified unmount lifecycle listener.
    /// </summary>
    /// <param name="callback">
    /// Callback removed from executing on unmount.</param>
    void RemoveUnmountListener(SideEffectApiCallback callback);
}
