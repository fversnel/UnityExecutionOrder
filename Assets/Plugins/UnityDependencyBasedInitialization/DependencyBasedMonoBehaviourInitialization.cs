using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityDependencyBasedInitialization;

public class DependencyBasedMonoBehaviourInitialization : MonoBehaviour {

    void Start()
    {
        // TODO Allow multiple implementations of the dependency manager
        var dependencyManager = FindObjectOfType(typeof(CachingDependencyManager)) as IDependencyManager;
        if (dependencyManager == null)
        {
            throw new Exception("Failed to initialize game object " + this + 
                " because the dependency manager could not be found.");
        }

        var initializableComponents = Initialization.FindInitializeables(gameObject);
        var componentReference = Initialization.CreateInitializableReference(initializableComponents);

        
        var visitedComponents = new HashSet<Type>();
        foreach (var component in initializableComponents)
        {
            var executionOrder = dependencyManager.GetExecutionOrder(component.GetType());

            foreach (Type evaluatedType in executionOrder)
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