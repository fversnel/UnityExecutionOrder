using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityDependencyBasedInitialization
{
    public static class DependencyGraph
    {
        public static Node<Type> CreateDependencyGraph(Type someType)
        {
            var dependencies = DependsOn.ExtractDependencies(someType);
            var children = new List<Node<Type>>();
            foreach (Type dependency in dependencies)
            {
                children.Add(CreateDependencyGraph(dependency));
            }
            return new Node<Type>(someType, children);
        }

        public static IEnumerable<T> EvaluationOrder<T>(Node<T> graph)
        {
            return BreadthFirstEvaluationOrder(graph)
                .Reverse()
                .Select(node => node.Value);
        }

        private static IEnumerable<Node<T>> BreadthFirstEvaluationOrder<T>(Node<T> root)
        {
            var visited = new HashSet<Node<T>>();

            var q = new Queue<Node<T>>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                Node<T> current = q.Dequeue();
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    yield return current;
                }
                foreach (var child in current.Children)
                    q.Enqueue(child);
            }
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