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
    private readonly Dictionary<Capsule<object>, CapsuleManager<object>> capsules = [];

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
            foreach (var manager in this.capsules.Values.ToList())
            {
                manager.Dispose();
            }
        }
    }

    private CapsuleManager<T> Manager<T>(Capsule<T> capsule)
    {
        if (!this.capsules.ContainsKey(capsule as Capsule<object>))
        {
            this.capsules[capsule as Capsule<object>] =
                new CapsuleManager<T>(this, capsule) as CapsuleManager<object>;
        }

        return this.capsules[capsule as Capsule<object>] as CapsuleManager<T>;
    }

    private class CapsuleManager<T> : DataflowGraphNode, ISideEffectApi
    {
        public CapsuleManager(Container container, Capsule<T> capsule)
        {
            this.Container = container;
            this.Capsule = capsule;

            this.BuildSelf();
        }

        public Container Container { get; }
        public Capsule<T> Capsule { get; }

        public T Data { get; private set; }
        public bool HasBuilt { get; private set; } = false;
        public List<object?> SideEffectData { get; } = [];
        public HashSet<SideEffectApiCallback> ToDispose { get; } = [];

        public R Read<R>(Capsule<R> otherCapsule)
        {
            var otherManager = this.Container.Manager(otherCapsule);
            this.AddDependency(otherManager);
            return otherManager.Data;
        }

        public override bool BuildSelf()
        {
            // Clear dependency relationships as they will be repopulated via `read`
            this.ClearDependencies();

            // Build the capsule's new data
            var newData = this.Capsule(new CapsuleHandleImpl(this as CapsuleManager<object>));
            var didChange = !this.HasBuilt || !EqualityComparer<T>.Default.Equals(newData, this.Data);
            this.Data = newData;
            this.HasBuilt = true;
            return didChange;
        }

        public override bool IsSuperPure => this.SideEffectData.Count == 0;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.Container.capsules.Remove(this.Capsule as Capsule<object>);
                foreach (var callback in this.ToDispose)
                {
                    callback();
                }
            }
        }

        public void Rebuild() => this.BuildSelfAndDependents();

        public void RegisterDispose(SideEffectApiCallback callback) =>
            this.ToDispose.Add(callback);

        public void UnregisterDispose(SideEffectApiCallback callback) =>
            this.ToDispose.Remove(callback);
    }

    private class CapsuleHandleImpl : ICapsuleHandle
    {
        public CapsuleHandleImpl(CapsuleManager<object> manager)
        {
            this.Manager = manager;
        }

        public CapsuleManager<object> Manager { get; }

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
