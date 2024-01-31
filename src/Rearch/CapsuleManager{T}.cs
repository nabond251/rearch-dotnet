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
    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleManager{T}"/> class.
    /// </summary>
    /// <param name="container">Capsule data container.</param>
    /// <param name="capsule">Capsule whose data and effects to manage.</param>
    public CapsuleManager(Container container, Capsule<T> capsule)
        : base(container)
    {
        this.Capsule = capsule;

        this.BuildSelf();
    }

    /// <summary>
    /// Gets capsule whose data and effects to manage.
    /// </summary>
    public Capsule<T> Capsule { get; }

    /// <summary>
    /// Gets encapsulated data, if any.
    /// </summary>
    public T? Data { get; private set; }

    /// <inheritdoc/>
    public override bool BuildSelf()
    {
        // Clear dependency relationships as they will be repopulated via `Read`
        this.ClearDependencies();

        // Build the capsule's new data
        var newData = this.Capsule(new CapsuleHandle(this));
        var didChange =
            !this.HasBuilt ||
            (newData is null && this.Data is not null) ||
            (newData is not null && this.Data is null) ||
            (newData is not null && this.Data is not null && !newData.Equals(this.Data));
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
            this.Container.Capsules.Remove(this.Capsule);
            foreach (var callback in this.ToDispose)
            {
                callback();
            }
        }
    }
}
