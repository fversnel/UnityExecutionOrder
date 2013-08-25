using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityDependencyBasedInitialization;

public class DependencyBasedMonoBehaviourInitialization : MonoBehaviour {

    void Awake()
    {
        var componentReference = new Dictionary<Type, IInitializeable>();
        var dependencyGraphs = new List<DependencyGraph.Node<Type>>();
        var components = GetComponents<Component>();
        foreach (var component in components)
        {
            var componentType = component.GetType();
            var interfaces = componentType.GetInterfaces();
            if (interfaces.Contains(typeof (IInitializeable)))
            {
                var dependencyGraph = DependencyGraph.CreateDependencyGraph(componentType);
                dependencyGraphs.Add(dependencyGraph);
                componentReference.Add(componentType, component as IInitializeable);
            }
        }

        var visitedComponents = new HashSet<Type>();
        foreach (DependencyGraph.Node<Type> graph in dependencyGraphs)
        {
            var evaluationOrder = DependencyGraph.EvaluationOrder(graph);
            foreach (Type evaluatedType in evaluationOrder)
            {
                if (!visitedComponents.Contains(evaluatedType))
                {
                    visitedComponents.Add(evaluatedType);
                    componentReference[evaluatedType].Initialize();
                }
            }
        }
    }
}