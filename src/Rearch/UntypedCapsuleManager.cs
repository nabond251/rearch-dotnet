// <copyright file="UntypedCapsuleManager.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Base untyped dataflow node that manages capsule data and effects.
/// </summary>
internal abstract class UntypedCapsuleManager :
    DataflowGraphNode,
    ISideEffectApi
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UntypedCapsuleManager"/> class.
    /// </summary>
    /// <param name="container">Capsule container.</param>
    protected UntypedCapsuleManager(CapsuleContainer container)
    {
        this.Container = container;
    }

    /// <summary>
    /// Gets or sets a value indicating whether encapsulated data has been
    /// built.
    /// </summary>
    internal bool HasBuilt { get; set; }

    /// <summary>
    /// Gets data of registered side effects.
    /// </summary>
    internal List<object?> SideEffectData { get; } = new();

    /// <summary>
    /// Gets set of callbacks to dispose.
    /// </summary>
    internal HashSet<SideEffectApiCallback> ToDispose { get; } = new();

    /// <summary>
    /// Gets container containing capsule's data.
    /// </summary>
    protected CapsuleContainer Container { get; }

    /// <inheritdoc/>
    protected override bool IsSuperPure => this.SideEffectData.Count == 0;

    /// <inheritdoc/>
    public void Rebuild() => this.BuildSelfAndDependents();

    /// <inheritdoc/>
    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Add(callback);

    /// <inheritdoc/>
    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Remove(callback);

    /// <summary>
    /// Reads data from capsule other than the one this manages.
    /// </summary>
    /// <typeparam name="TRead">Type of read data.</typeparam>
    /// <param name="otherCapsule">Other capsule whose data to read.</param>
    /// <returns><paramref name="otherCapsule"/> data.</returns>
    internal TRead Read<TRead>(Capsule<TRead> otherCapsule)
    {
        var otherManager = this.Container.ManagerOf(otherCapsule);
        this.AddDependency(otherManager);
        return otherManager.Data;
    }
}
