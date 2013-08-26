using System;
using System.Collections.Generic;
using System.Linq;
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
        var initializables = components.Where(component => Initialization.IsInitializable(component.GetType()));
        foreach (var initializable in initializables)
        {
            var executionOrder = dependencyManager.GetExecutionOrder(initializable.GetType());
            foreach (Type componentType in executionOrder)
            {
                if (!visitedComponents.Contains(componentType))
                {
                    visitedComponents.Add(componentType);

                    Component component;
                    componentReference.TryGetValue(componentType, out component);
                    if (component != null)
                    {
                        if (Initialization.IsInitializable(componentType))
                        {
                            (component as IInitializeable).Initialize();
                        }
                        else
                        {
                            Debug.LogWarning(component + " is part of a dependency graph but does not implement the IInitializable interface.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Initializable [" + componentType + "] was depended upon but " +
                                       "is not present on " + gameObject);
                    }
                }
            }
        }
    }

}