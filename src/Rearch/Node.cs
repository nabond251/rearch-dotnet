// <copyright file="Node.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

internal abstract class DataflowGraphNode : IDisposable
{
    private readonly HashSet<DataflowGraphNode> dependencies = [];
    private readonly HashSet<DataflowGraphNode> dependents = [];

    public abstract bool IsSuperPure { get; }
    public abstract bool BuildSelf();

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
            this.ClearDependencies();
        }
    }

    public void AddDependency(DataflowGraphNode node)
    {
        this.dependencies.Add(node);
        node.dependents.Add(this);
    }

    public void ClearDependencies()
    {
        foreach (var dep in this.dependencies)
        {
            dep.dependents.Remove(this);
        }

        dependencies.Clear();
    }

    public void BuildSelfAndDependents()
    {
        var selfChanged = this.BuildSelf();
        if (!selfChanged)
        {
            return;
        }

        // Build or garbage collect (dispose) all remaining nodes
        // (We use skip(1) to avoid building this node twice)
        var buildOrder = CreateBuildOrder().Skip(1).ToList();
        File.AppendAllLines(
            @"c:\Users\Son of Eorl\Documents\test.txt",
            buildOrder
            .Select(n => (n as CapsuleManager<int>))
            .Select(m => $"{m?.Name}: {m?.Data}, {m?.IsSuperPure}"));
        var disposableNodes = GetDisposableNodesFromBuildOrder(buildOrder);
        File.AppendAllLines(
            @"c:\Users\Son of Eorl\Documents\test.txt",
            disposableNodes
            .Select(n => (n as CapsuleManager<int>))
            .Select(m => $"{m?.Name}: {m?.Data}, {m?.IsSuperPure}"));
        HashSet<DataflowGraphNode> changedNodes = [this];
        foreach (var node in buildOrder)
        {
            var haveDepsChanged = node.dependencies.Any(changedNodes.Contains);
            if (!haveDepsChanged)
            {
                continue;
            }

            if (disposableNodes.Contains(node))
            {
                // Note: dependency/dependent relationships will be after this,
                // since we are disposing all dependents in the build order,
                // because we are adding this node to changedNodes
                node.Dispose();
                changedNodes.Add(node);
            }
            else
            {
                var didNodeChange = node.BuildSelf();
                if (didNodeChange)
                {
                    changedNodes.Add(node);
                }
            }
        }
    }

    private IList<DataflowGraphNode> CreateBuildOrder()
    {
        // We need some more information alongside of each node
        // in order to do the topological sort:
        // - False is for the first visit, which adds all deps to be visited,
        //   and then node again
        // - True is for the second visit, which pushes the node to the build order
        var toVisitStack = new List<(bool, DataflowGraphNode)>
        {
            (false, this),
        };
        var visited = new HashSet<DataflowGraphNode>();
        var buildOrderStack = new List<DataflowGraphNode>();

        while (toVisitStack.Count != 0)
        {
            var (hasVisitedBefore, node) = toVisitStack[^1];
            toVisitStack.RemoveAt(toVisitStack.Count - 1);

            if (hasVisitedBefore)
            {
                // Already processed this node's dependents, so add to build order
                buildOrderStack.Add(node);
            }
            else if (!visited.Contains(node))
            {
                // New node, so mark this node to be added later and process dependents
                visited.Add(node);
                toVisitStack.Add((true, node)); // mark to be added to build order later
                node.dependents
                    .Where(dep => !visited.Contains(dep))
                    .ToList()
                    .ForEach(dep => toVisitStack.Add((false, dep)));
            }
        }

        return buildOrderStack.AsEnumerable().Reverse().ToList();
    }

    private static ISet<DataflowGraphNode> GetDisposableNodesFromBuildOrder(
        IList<DataflowGraphNode> buildOrder)
    {
        HashSet<DataflowGraphNode> disposable = [];

        var ds = buildOrder.Reverse().Where(node =>
        {
            var mgr = node as CapsuleManager<int>;
            var dependentsAllDisposable =
                node.dependents.All(disposable.Contains);
            File.AppendAllLines(
                @"c:\Users\Son of Eorl\Documents\test.txt",
                node.dependents
                .Select(n => (n as CapsuleManager<int>))
                .Select(m => $"{m?.Name}: {m?.Data}, {m?.IsSuperPure}")
                .Prepend($"{mgr?.Name} dependents (all disposable {dependentsAllDisposable})"));
            return node.IsSuperPure && dependentsAllDisposable;
        });
        
        foreach (var d in ds)
        {
            disposable.Add(d);
        }

        return disposable;
    }
}
