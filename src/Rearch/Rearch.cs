// <copyright file="Rearch.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

// TODO(nabond251): eager garbage collection mode

/// <summary>
/// A blueprint for creating some data, given a <see cref="ICapsuleHandle"/>.
/// See the documentation for more.
/// </summary>
public delegate T Capsule<T>(ICapsuleHandle handle);

/// <summary>
/// Capsule listeners are a mechanism to <i>temporarily</i> listen to changes
/// to a set of <see cref="Capsule{T}"/>s.
/// </summary>
/// <seealso cref="Container.Listen(CapsuleListener)"/>
public delegate void CapsuleListener(ICapsuleReader capsuleReader);

/// <summary>
/// Provides a mechanism to read the current state of <see cref="Capsule{T}"/>s.
/// </summary>
public interface ICapsuleReader
{
    /// <summary>
    /// Reads the data of the supplied <see cref="Capsule{T}"/>.
    /// </summary>
    T Call<T>(Capsule<T> capsule, string name = "");
}

/// <summary>
/// Provides a mechanism (<see cref="Register{T}(SideEffect{T})"/>) to register side effects.
/// </summary>
public interface ISideEffectRegistrar
{
    /// <summary>
    /// Registers the given side effect.
    /// </summary>
    T Register<T>(SideEffect<T> sideEffect);
}

/// <summary>
/// The handle given to a <see cref="Capsule{T}"/> to build its data.
/// </summary>
public interface ICapsuleHandle : ICapsuleReader, ISideEffectRegistrar
{
}

/// <summary>
/// Represents a side effect.
/// See the documentation for more.
/// </summary>
public delegate T SideEffect<T>(ISideEffectApi sideEffectApi);

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
    void RegisterDispose(SideEffectApiCallback callback);

    /// <summary>
    /// Unregisters the given <see cref="SideEffectApiCallback"/>
    /// from being called on capsule disposal.
    /// </summary>
    void UnregisterDispose(SideEffectApiCallback callback);
}

/// <summary>
/// Contains the data of <see cref="Capsule{T}"/>s.
/// See the documentation for more.
/// </summary>
public class Container : IDisposable
{
    internal readonly Dictionary<
        object,
        UntypedCapsuleManager> capsules = [];

    internal CapsuleManager<T> Manager<T>(Capsule<T> capsule, string name)
    {
        if (!this.capsules.ContainsKey(capsule))
        {
            this.capsules[capsule] = new CapsuleManager<T>(this, capsule, name);
        }

        return this.capsules[capsule] as CapsuleManager<T>;
    }

    /// <summary>
    /// Reads the current data of the supplied <see cref="Capsule"/>.
    /// </summary>
    public T Read<T>(Capsule<T> capsule, string name = "") => this.Manager(capsule, name).Data;

    /// <summary>
    /// <i>Temporarily</i> listens to changes in a given set of <see cref="Capsule{T}"/>s.
    /// If you want to listen to capsule(s) <i>not temporarily</i>,
    /// instead just make an impure capsule and <see cref="Read{T}(Capsule{T})"/> it once to initialize it.
    /// <c>Listen</c> calls the supplied listener immediately,
    /// and then after any capsules its listening to change.
    /// </summary>
    /// <remarks><see cref="ListenerHandle"/> will leak its listener if it is not disposed.</remarks>
    public ListenerHandle Listen(CapsuleListener listener, string name = "")
    {
        // Create a temporary *impure* capsule so that it doesn't get super-pure
        // garbage collected
        object Capsule(ICapsuleHandle use)
        {
            use.Register(_ => new object());
            listener(use);
            return new object();
        }

        // Put the temporary capsule into the container so it gets data updates
        this.Read(Capsule, name);

        return new ListenerHandle(this, Capsule);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implements the disposable pattern.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether this is being disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // We need ToList() to prevent container modification during iteration
            foreach (var manager in this.capsules.Values.ToList())
            {
                manager.Dispose();
            }
        }
    }
}

/// <summary>
/// A handle onto the lifecycle of a listener from <see cref="Container.Listen(CapsuleListener)"/>.
/// You <i>must</i> <see cref="Dispose()"/> the <see cref="ListenerHandle"/>
/// when you no longer need the listener in order to prevent leaks.
/// </summary>
public class ListenerHandle : IDisposable
{
    private readonly Container container;
    private readonly Capsule<object?> capsule;

    internal ListenerHandle(Container container, Capsule<object?> capsule)
    {
        this.container = container;
        this.capsule = capsule;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implements the disposable pattern.
    /// </summary>
    /// <param name="disposing">
    /// A value indicating whether this is being disposed.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.container.Manager(this.capsule, "").Dispose();
        }
    }
}

internal abstract class UntypedCapsuleManager : DataflowGraphNode,
    ISideEffectApi
{
    protected UntypedCapsuleManager(Container container)
    {
        this.Container = container;
    }

    public Container Container { get; }

    public bool HasBuilt { get; protected set; } = false;
    public List<object?> SideEffectData { get; } = [];
    public HashSet<SideEffectApiCallback> ToDispose { get; } = [];

    public R Read<R>(Capsule<R> otherCapsule, string name = "")
    {
        var otherManager = this.Container.Manager(otherCapsule, name);
        this.AddDependency(otherManager);
        return otherManager.Data;
    }

    public override bool IsSuperPure => this.SideEffectData.Count == 0;

    public void Rebuild() => this.BuildSelfAndDependents();

    public void RegisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Add(callback);

    public void UnregisterDispose(SideEffectApiCallback callback) =>
        this.ToDispose.Remove(callback);
}

internal class CapsuleManager<T> : UntypedCapsuleManager
{
    public CapsuleManager(Container container, Capsule<T> capsule, string name)
        : base(container)
    {
        this.Capsule = capsule;
        this.Name = name;

        this.BuildSelf();
    }

    public Capsule<T> Capsule { get; }

    public string Name { get; }

    public T Data { get; private set; }

    public override bool BuildSelf()
    {
        // Clear dependency relationships as they will be repopulated via `Read`
        this.ClearDependencies();

        // Build the capsule's new data
        var newData = this.Capsule(new CapsuleHandle(this));
        var didChange = !this.HasBuilt || !newData.Equals(this.Data);
        File.AppendAllLines(
            @"c:\Users\Son of Eorl\Documents\test.txt",
            new List<string>
            {
                $"this.HasBuilt: {this.HasBuilt}",
                $"this.Data: {this.Data}",
                $"newData: {newData}",
                $"didChange: {didChange}",
            });
        this.Data = newData;
        this.HasBuilt = true;
        return didChange;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            this.Container.capsules.Remove(this.Capsule);
            foreach (var callback in this.ToDispose)
            {
                callback();
            }
        }
    }
}

internal class CapsuleHandle : ICapsuleHandle
{
    public CapsuleHandle(UntypedCapsuleManager manager)
    {
        this.Manager = manager;
    }

    public UntypedCapsuleManager Manager { get; }

    public int SideEffectDataIndex { get; private set; } = 0;

    public T Call<T>(Capsule<T> capsule, string name = "") => this.Manager.Read(capsule, name);

    public T Register<T>(SideEffect<T> sideEffect)
    {
        if (this.SideEffectDataIndex == this.Manager.SideEffectData.Count)
        {
            this.Manager.SideEffectData.Add(sideEffect(this.Manager));
        }

        return (T)this.Manager.SideEffectData[this.SideEffectDataIndex++];
    }
}
