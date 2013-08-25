#Unity dependency-based initialization

Library for Unity3D that allows components to be initialized in a specific order.

## The problem

Sometimes when you have a complex web of components depending on each other then initialization of these components can become a problem as Unity does not guarantee the order in which components on a `GameObject` are initialized. Consider the following example:

    public class BehaviourA : MonoBehaviour 
    {
        public int state;

        void Awake() {
            state = 42;
        }
    }

    public class BehaviourB : MonoBehaviour 
    {
        public int state;

        // We do initialization of BehaviourB in Start so we know 
        // for sure that BehaviourA is initialized
        void Start() 
        {
            state = GetComponent<BehaviourA>().state + 1;
        }
    }

    public class BehaviourC : MonoBehaviour 
    {
        public int state;

        void Start() 
        {
            // Warning might fail since BehaviourB is not guaranteed 
            // to be initialized.
            state = GetComponent<BehaviourB>().state + 1;
        }
    }

`BehaviourC` depends on `BehaviourB` which depends on `BehaviourA`, because they depend on each others state. So, `BehaviourC` cannot be initialized in a proper way using `Awake` and `Start` because `BehaviourB` might not have been initialized when `BehaviourC.Start()` is called.

## The solution

This library solves the problem of components depending on each other by introducing a dependency-based initialization mechanism. Its usage is very simple:

Let your components implement the `IInitializable` interface, like this:

    using UnityEngine;
    using UnityDependencyBasedInitialization;

    public class BehaviourA : MonoBehaviour, IInitializeable 
    {
        // There is no need to implement either Awake or Start anymore
        // the library will handle the initialization of this component.
        void Initialize()
        {
            // Put your initialization code here.
        }
    }

Optionally add a `DependsOn` attribute to your class so the library knows that there is a dependency and will initialize the dependency before initializing this component:

    [DependsOn(typeof(BehaviourB))]
    public class BehaviourA : MonoBehaviour, IInitializeable 

In the Unity editor make sure to add the `DependencyBasedMonoBehaviourInitialization` to your `GameObject` otherwise initialization will not be performed at all.
    