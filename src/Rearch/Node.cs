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

        /*
        // TODO only build dependents when buildSelf returns true,
        //  and only gc nodes when depenency changes

        var disposable = new List<DataflowGraphNode>();

        buildOrder.Reverse().Where(node =>
        {
            var dependentsAllDisposable =
                node.dependents.All(disposable.Contains);
            return node.IsSuperPure && dependentsAllDisposable;
        }).ToList().ForEach(disposable.Add);

        return disposable;
        */

        var buildOrder = GarbageCollectDisposableNodes(
            this.CreateBuildOrder().Skip(1).ToList());
        foreach (var node in buildOrder)
        {
            node.BuildSelf();
        }
    }

    public IList<DataflowGraphNode> CreateBuildOrder()
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

    public static IEnumerable<DataflowGraphNode> GarbageCollectDisposableNodes(
        IList<DataflowGraphNode> buildOrder)
    {
        var nonDisposable = new List<DataflowGraphNode>();

        foreach (var node in buildOrder.Reverse())
        {
            var isDisposable = node.IsSuperPure && !node.dependents.Any();
            if (isDisposable)
            {
                node.Dispose();
            }
            else
            {
                nonDisposable.Add(node);
            }
        }

        return nonDisposable.AsEnumerable().Reverse();
    }
}
