using System;
using System.Collections.Generic;
using UnityEngine;
using UnityDependencyBasedInitialization;

public class DependencyBasedMonoBehaviourInitialization : MonoBehaviour {

    private void Start()
    {
        // TODO Allow multiple implementations of the dependency manager
        var dependencyManager = FindObjectOfType(typeof (CachingDependencyManager)) as IDependencyManager;
        if (dependencyManager == null)
        {
            throw new Exception("Failed to initialize game object " + this +
                                " because the dependency manager could not be found.");
        }

        var components = GetComponents<Component>();
        var componentReference = Initialization.CreateComponentReference(components);

        var visitedComponents = new HashSet<Type>();
        foreach (var component in components)
        {
            var executionOrder = dependencyManager.GetExecutionOrder(component.GetType());

            foreach (Type evaluatedType in executionOrder)
            {
                if (!visitedComponents.Contains(evaluatedType))
                {
                    visitedComponents.Add(evaluatedType);

                    Component dependentComponent;
                    componentReference.TryGetValue(evaluatedType, out dependentComponent);
                    if (dependentComponent != null)
                    {
                        if (Initialization.IsInitializable(evaluatedType))
                        {
                            (dependentComponent as IInitializeable).Initialize();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Initializable [" + evaluatedType + "] was depended upon but " +
                                            "is not present on " + gameObject);       
                    }
                }
            }
        }
    }

}