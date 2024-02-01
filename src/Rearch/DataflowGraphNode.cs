// <copyright file="DataflowGraphNode.cs" company="SdgApps">
// Copyright (c) SdgApps. All rights reserved.
// </copyright>

namespace Rearch;

/// <summary>
/// Node used to manage dataflow computation.
/// </summary>
internal abstract class DataflowGraphNode : IDisposable
{
    private readonly HashSet<DataflowGraphNode> dependencies = [];
    private readonly HashSet<DataflowGraphNode> dependents = [];

    /// <summary>
    /// Gets a value indicating whether the node is free of side effects.
    /// </summary>
    public abstract bool IsSuperPure { get; }

    /// <summary>
    /// Prune unused nodes from the network.  Nodes are unused if nothing depends on them and they have no side effects.
    /// </summary>
    /// <param name="buildOrder">List of nodes to consider for pruning.</param>
    /// <returns>List of nodes after pruning.</returns>
    public static IEnumerable<DataflowGraphNode> GarbageCollectDisposableNodes(
        IList<DataflowGraphNode> buildOrder)
    {
        var nonDisposable = new List<DataflowGraphNode>();

        foreach (var node in buildOrder.Reverse())
        {
            var isDisposable = node.IsSuperPure && node.dependents.Count == 0;
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

    /// <summary>
    /// Updates node data.
    /// </summary>
    /// <returns>A value indicating whether the node data changed.</returns>
    public abstract bool BuildSelf();

    /// <summary>
    /// Adds a dependency node in the node network.
    /// </summary>
    /// <param name="node">Dependency node to add.</param>
    public void AddDependency(DataflowGraphNode node)
    {
        this.dependencies.Add(node);
        node.dependents.Add(this);
    }

    /// <summary>
    /// Clears node network dependencies.
    /// </summary>
    public void ClearDependencies()
    {
        foreach (var dep in this.dependencies)
        {
            dep.dependents.Remove(this);
        }

        this.dependencies.Clear();
    }

    /// <summary>
    /// Updates node data and propogates any changes to other associated nodes in the network.
    /// </summary>
    public void BuildSelfAndDependents()
    {
        var selfChanged = this.BuildSelf();
        if (!selfChanged)
        {
            return;
        }

        // Build or garbage collect (dispose) all remaining nodes
        // (We use skip(1) to avoid building this node twice)
        var buildOrder = this.CreateBuildOrder().Skip(1).ToList();
        var disposableNodes = GetDisposableNodesFromBuildOrder(buildOrder);
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

    private static HashSet<DataflowGraphNode> GetDisposableNodesFromBuildOrder(
        IList<DataflowGraphNode> buildOrder)
    {
        HashSet<DataflowGraphNode> disposable = [];

        var ds = buildOrder.Reverse().Where(node =>
        {
            var dependentsAllDisposable =
                node.dependents.All(disposable.Contains);
            return node.IsSuperPure && dependentsAllDisposable;
        });

        foreach (var d in ds)
        {
            disposable.Add(d);
        }

        return disposable;
    }

    private List<DataflowGraphNode> CreateBuildOrder()
    {
        // We need some more information alongside of each node
        // in order to do the topological sort:
        // - False is for the first visit, which adds all deps to be visited,
        //   and then node again
        // - True is for the second visit, which pushes the node to the build
        //   order
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
            else
            {
                // New node, so mark this node to be added later and process
                // dependents
                visited.Add(node);

                // mark to be added to build order later
                toVisitStack.Add((true, node));
                node.dependents
                    .Where(dep => !visited.Contains(dep))
                    .ToList()
                    .ForEach(dep => toVisitStack.Add((false, dep)));
            }
        }

        return buildOrderStack.AsEnumerable().Reverse().ToList();
    }
}
