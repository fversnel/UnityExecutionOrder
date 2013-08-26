using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityDependencyBasedInitialization
{
    public static class Initialization
    {
        public static IDictionary<Type, Component> CreateComponentReference(IEnumerable<Component> components)
        {
            var componentReference = new Dictionary<Type, Component>();
            foreach (var component in components)
            {
                componentReference.Add(component.GetType(), component);
            }
            return componentReference;
        } 

        public static bool IsInitializable(Type componentType)
        {
            var interfaces = componentType.GetInterfaces();
            return interfaces.Contains(typeof (IInitializeable));
        }
    }

    public interface IDependencyManager
    {
        IEnumerable<Type> GetExecutionOrder(Type someType);
    }

    public static class DependencyGraph
    {
        public static Node<Type> CreateDependencyGraph(Type rootType)
        {
            var nonInitializables = new HashSet<Type>();
            var circularRefs = new HashSet<Type>();
            
            Func<Type, IEnumerable<Type>, Node<Type>> inner = null;
            inner = (someType, visitedNodes) =>
            {
                var dependencies = DependsOn.ExtractDependencies(someType);
                var children = new List<Node<Type>>();
                foreach (Type dependency in dependencies)
                {
                    if (!visitedNodes.Contains(dependency))
                    {
                        if (!Initialization.IsInitializable(dependency))
                        {
                            nonInitializables.Add(dependency);
                        }

                        children.Add(inner(dependency, new HashSet<Type>(visitedNodes) { someType }));
                    }
                    else
                    {
                        circularRefs.Add(someType);
                    }
                }

                return new Node<Type>(someType, children);
            };

            var graph = inner(rootType, new HashSet<Type>());

            foreach (var circularRef in circularRefs)
            {
                Debug.LogWarning("Circular dependency detected at " + circularRef);
            }
            foreach (var nonInitializable in nonInitializables)
            {
                Debug.LogWarning(nonInitializable + " is part of a dependency graph but does not implement the IInitializable interface.");
            }

            return graph;
        }

        public static IEnumerable<T> ExecutionOrder<T>(Node<T> graph)
        {
            return DepthFirstEvaluation(graph)
                .Select(node => node.Value);
        }

        public static IEnumerable<Node<T>> DepthFirstEvaluation<T>(Node<T> root)
        {
            Func<Queue<Node<T>>, Node<T>, Queue<Node<T>>> inner = null;
            inner = (order, node) =>
            {
                foreach (var child in node.Children)
                {
                    order = inner(order, child);
                }
                order.Enqueue(node);
                return order;
            };

            return inner(new Queue<Node<T>>(), root);
        } 

        public class Node<T> : IEquatable<Node<T>>
        {
            private readonly T _value;
            private readonly IEnumerable<Node<T>> _children;

            public Node(T value, IEnumerable<Node<T>> children)
            {
                _value = value;
                _children = children;
            }

            public IEnumerable<Node<T>> Children
            {
                get { return _children; }
            }

            public T Value
            {
                get { return _value; }
            }

            public bool Equals(Node<T> other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return EqualityComparer<T>.Default.Equals(_value, other._value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Node<T>) obj);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<T>.Default.GetHashCode(_value);
            }
        }
    }

    public interface IInitializeable
    {
        /// <summary>
        /// Use this for initialization instead of Awake() and Start()
        /// </summary>
        void Initialize();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsOn : Attribute
    {
        private readonly Type _dependencyType;

        public DependsOn(Type dependencyType)
        {
            _dependencyType = dependencyType;
        }

        public Type DependencyType
        {
            get { return _dependencyType; }
        }

        public static IEnumerable<Type> ExtractDependencies(Type someType)
        {
            var attributes = someType.GetCustomAttributes(typeof(DependsOn), true).Cast<DependsOn>();
            return attributes.Select(attribute => attribute.DependencyType);
        }
    }
}
