// <copyright file="CapsuleManager{T}.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Dataflow node that manages capsule data and effects.
/// </summary>
/// <typeparam name="T">Type of encapsulated data.</typeparam>
internal sealed class CapsuleManager<T> : UntypedCapsuleManager
{
    private readonly Capsule<T> capsule;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleManager{T}"/> class.
    /// </summary>
    /// <param name="container">Capsule data container.</param>
    /// <param name="capsule">Capsule whose data and effects to manage.</param>
    internal CapsuleManager(CapsuleContainer container, Capsule<T> capsule)
        : base(container)
    {
        this.capsule = capsule;

        this.BuildSelf();
    }

    /// <summary>
    /// Gets encapsulated data.
    /// </summary>
    internal T Data { get; private set; } = default!;

    /// <inheritdoc/>
    protected override bool BuildSelf()
    {
        // Clear dependency relationships as they will be repopulated via `Read`
        this.ClearDependencies();

        // Build the capsule's new data
        var newData = this.capsule(new CapsuleHandle(this));
        var didChange =
            !this.HasBuilt ||
            (newData is null && this.Data is not null) ||
            (newData is not null && this.Data is null) ||
            (
                newData is not null && this.Data is not null &&
                !newData.Equals(this.Data));
        this.Data = newData;
        this.HasBuilt = true;
        return didChange;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            this.Container.Capsules.Remove(this.capsule);
            foreach (var callback in this.ToDispose)
            {
                callback();
            }
        }
    }
}
