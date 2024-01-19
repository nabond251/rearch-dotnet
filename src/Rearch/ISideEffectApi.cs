// <copyright file="ISideEffectApi.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// An <c>Action</c> callback.
/// </summary>
public delegate void SideEffectApiCallback();

/// <summary>
/// The api given to <see cref="SideEffect{T}"/>s to create their state.
/// </summary>
public interface ISideEffectApi
{
    /// <summary>
    /// Triggers a rebuild in the supplied capsule.
    /// </summary>
    void Rebuild();

    /// <summary>
    /// Registers the given <see cref="SideEffectApiCallback"/>
    /// to be called on capsule disposal.
    /// </summary>
    /// <param name="callback">Callback to register.</param>
    void RegisterDispose(SideEffectApiCallback callback);

    /// <summary>
    /// Unregisters the given <see cref="SideEffectApiCallback"/>
    /// from being called on capsule disposal.
    /// </summary>
    /// <param name="callback">Callback to unregister.</param>
    void UnregisterDispose(SideEffectApiCallback callback);
}
