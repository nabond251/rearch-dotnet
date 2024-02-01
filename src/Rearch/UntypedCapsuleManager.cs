// <copyright file="UntypedCapsuleManager.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Base untyped dataflow node that manages capsule data and effects.
/// </summary>
internal abstract class UntypedCapsuleManager(Container container) :
    DataflowGraphNode,
    ISideEffectApi
{
    /// <summary>
    /// Gets container containing capsule's data.
    /// </summary>
    public Container Container { get; } = container;

    /// <summary>
    /// Gets or sets a value indicating whether encapsulated data has been built.
    /// </summary>
    public bool HasBuilt { get; protected set; }

    /// <summary>
    /// Gets data of registered side effects.
    /// </summary>
    public List<object?> SideEffectData { get; } = [];

    /// <summary>
    /// Gets set of callbacks to dispose.
    /// </summary>
    public HashSet<SideEffectApiCallback> ToDispose { get; } = [];

    /// <inheritdoc/>
    public override bool IsSuperPure => this.SideEffectData.Count == 0;

    /// <summary>
    /// Reads data from capsule other than the one this manages.
    /// </summary>
    /// <typeparam name="TRead">Type of read data.</typeparam>
    /// <param name="otherCapsule">Other capsule whose data to read.</param>
    /// <returns><paramref name="otherCapsule"/> data.</returns>
    public TRead Read<TRead>(Capsule<TRead> otherCapsule)
    {
        var otherManager = this.Container.ManagerOf(otherCapsule);
        this.AddDependency(otherManager);
        return otherManager.Data;
    }

    /// <inheritdoc/>
    public void Rebuild() => this.BuildSelfAndDependents();

    /// <inheritdoc/>
    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Add(callback);

    /// <inheritdoc/>
    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Remove(callback);
}
