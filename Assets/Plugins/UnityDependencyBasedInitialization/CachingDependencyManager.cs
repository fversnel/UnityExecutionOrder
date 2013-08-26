using System;
using System.Collections.Generic;
using System.Linq;
using UnityDependencyBasedInitialization;
using UnityEngine;

public class CachingDependencyManager : MonoBehaviour, IDependencyManager
{
    private IDictionary<Type, IEnumerable<Type>> _executionOrderCache;

    void Awake()
    {
        _executionOrderCache = new Dictionary<Type, IEnumerable<Type>>();
    }

    public IEnumerable<Type> GetExecutionOrder(Type someType)
    {
        IEnumerable<Type> cachedExecutionOrder;
        if (_executionOrderCache.TryGetValue(someType, out cachedExecutionOrder))
        {
            return cachedExecutionOrder;
        }
        else
        {
            var dependencyGraph = DependencyGraph.CreateDependencyGraph(someType);
            CacheDependencyGraph(dependencyGraph);
            return _executionOrderCache[someType]
        }
    }

    private void CacheDependencyGraph(DependencyGraph.Node<Type> node)
    {
        var executionOrder = DependencyGraph.ExecutionOrder(graph);
        _executionOrderCache.Add(node.Value, executionOrder);
        // Also cache sub graphs
        foreach(var dependency in graph.Children)
        {
            CacheDependencyGraph(dependency);
        }
    }
}
