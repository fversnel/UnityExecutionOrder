﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityDependencyBasedInitialization
{
    public static class Initialization
    {
        public static IDictionary<Type, IList<Component>> CreateComponentReference(IEnumerable<Component> components)
        {
            return components.GroupBy(component => component.GetType())
                .ToDictionary(componentsOfSameType => componentsOfSameType.Key,
                    componentsOfSameType => componentsOfSameType.ToList() as IList<Component>);
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

    public static class DependencyTree
    {
        public static Node<Type> CreateDependencyTree(Type rootType)
        {
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

            return graph;
        }

        public static IEnumerable<T> ExecutionOrder<T>(Node<T> graph)
        {
            return PostOrderTraversal(graph)
                .Select(node => node.Value);
        }

        public static IEnumerable<Node<T>> PostOrderTraversal<T>(Node<T> root)
        {
            Func<IList<Node<T>>, Node<T>, Unit> inner = null;
            inner = (order, node) =>
            {
                foreach (var child in node.Children)
                {
                    inner(order, child);
                }
                order.Add(node);
                return Unit.Default;
            };

            var traverseOrder = new List<Node<T>>();
            inner(traverseOrder, root);
            return traverseOrder;
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
            var dependsOnAttributes = someType.GetCustomAttributes(typeof(DependsOn), true).Cast<DependsOn>();
            var dependencies = new HashSet<Type>();
            foreach (var dependsOnAttribute in dependsOnAttributes)
            {
                dependencies.Add(dependsOnAttribute.DependencyType);
            }
            return dependencies;
        }
    }

    class Unit
    {
        public static readonly Unit Default = new Unit(); 

        private Unit()
        {
        }
    }
}
