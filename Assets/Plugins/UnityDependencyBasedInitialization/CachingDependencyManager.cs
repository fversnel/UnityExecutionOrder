using System;
using System.Collections.Generic;
using UnityDependencyBasedInitialization;
using UnityEngine;

public class CachingDependencyManager : MonoBehaviour, IDependencyManager
{
    private IDictionary<Type, IEnumerable<Type>> _executionOrderCache;

    public IEnumerable<Type> GetExecutionOrder(Type someType)
    {
        IEnumerable<Type> cachedExecutionOrder;
        if (_executionOrderCache.TryGetValue(someType, out cachedExecutionOrder))
        {
            return cachedExecutionOrder;
        }
        DependencyGraph.Node<Type> dependencyGraph = DependencyGraph.CreateDependencyGraph(someType);
        CacheDependencyGraph(dependencyGraph);
        return _executionOrderCache[someType];
    }

    private void Awake()
    {
        _executionOrderCache = new Dictionary<Type, IEnumerable<Type>>();
    }

    private void CacheDependencyGraph(DependencyGraph.Node<Type> root)
    {
        // Store an execution order for each node in the graph
        foreach (var node in DependencyGraph.DepthFirstEvaluation(root))
        {
            if (!_executionOrderCache.ContainsKey(node.Value))
            {
                IEnumerable<Type> executionOrder = DependencyGraph.ExecutionOrder(node);
                _executionOrderCache.Add(node.Value, executionOrder);                  
            }
        }
    }
}