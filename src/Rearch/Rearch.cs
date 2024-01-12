// <copyright file="Rearch.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

public delegate T Capsule<out T>(ICapsuleHandle capsuleHandle);

public delegate void CapsuleListener(ICapsuleReader capsuleReader);

public interface ICapsuleReader
{
    T Call<T>(Capsule<T> capsule);
}

public interface ISideEffectRegistrar
{
    T Register<T>(SideEffect<T> sideEffect) where T : class;
}

public interface ICapsuleHandle : ICapsuleReader, ISideEffectRegistrar
{
}

public delegate T SideEffect<T>(ISideEffectApi sideEffectApi);

public delegate void SideEffectApiCallback();

public interface ISideEffectApi
{
    void Rebuild();

    void RegisterDispose(SideEffectApiCallback callback);

    void UnregisterDispose(SideEffectApiCallback callback);
}

/// <summary>
/// Contains the data of capsules.
/// </summary>
public class Container : IDisposable
{
    private readonly Dictionary<object, UntypedCapsuleManager> capsules = [];

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public T Read<T>(Capsule<T> capsule) => this.Manager(capsule).Data;

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
            foreach (var manager in this.capsules.Values.Cast<IDisposable>().ToList())
            {
                manager.Dispose();
            }
        }
    }

    private CapsuleManager<T> Manager<T>(Capsule<T> capsule)
    {
        if (!this.capsules.ContainsKey(capsule))
        {
            this.capsules[capsule] = new CapsuleManager<T>(this, capsule);
        }

        return this.capsules[capsule] as CapsuleManager<T>;
    }

    private abstract class UntypedCapsuleManager : DataflowGraphNode, ISideEffectApi
    {
        protected UntypedCapsuleManager(Container container)
        {
            this.Container = container;
        }

        public Container Container { get; }

        public bool HasBuilt { get; protected set; } = false;
        public List<object?> SideEffectData { get; } = [];
        public HashSet<SideEffectApiCallback> ToDispose { get; } = [];

        public R Read<R>(Capsule<R> otherCapsule)
        {
            var otherManager = this.Container.Manager(otherCapsule);
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

    private class CapsuleManager<T> : UntypedCapsuleManager
    {
        public CapsuleManager(Container container, Capsule<T> capsule)
            : base(container)
        {
            this.Capsule = capsule;

            this.BuildSelf();
        }

        public Capsule<T> Capsule { get; }

        public T Data { get; private set; }

        public override bool BuildSelf()
        {
            // Clear dependency relationships as they will be repopulated via `read`
            this.ClearDependencies();

            // Build the capsule's new data
            var newData = this.Capsule(new CapsuleHandleImpl(this));
            var didChange = !this.HasBuilt || !EqualityComparer<T>.Default.Equals(newData, this.Data);
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

    private class CapsuleHandleImpl : ICapsuleHandle
    {
        public CapsuleHandleImpl(UntypedCapsuleManager manager)
        {
            this.Manager = manager;
        }

        public UntypedCapsuleManager Manager { get; }

        public int SideEffectDataIndex { get; private set; } = 0;

        public T Call<T>(Capsule<T> capsule) => this.Manager.Read(capsule);

        public T Register<T>(SideEffect<T> sideEffect) where T : class
        {
            if (this.SideEffectDataIndex == this.Manager.SideEffectData.Count)
            {
                this.Manager.SideEffectData.Add(sideEffect(this.Manager));
            }

            return this.Manager.SideEffectData[this.SideEffectDataIndex++] as T;
        }
    }
}
