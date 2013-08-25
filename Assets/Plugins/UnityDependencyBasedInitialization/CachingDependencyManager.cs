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
            var newExecutionOrder = DependencyGraph.ExecutionOrder(dependencyGraph).ToList();
            _executionOrderCache.Add(someType, newExecutionOrder);
            return newExecutionOrder;
        }
    }
}
