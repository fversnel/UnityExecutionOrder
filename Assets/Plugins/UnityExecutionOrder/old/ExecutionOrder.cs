//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.Networking.NetworkSystem;
//
//namespace UnityExecutionOrder {
//
//    public abstract class Run : Attribute, IEquatable<Run> {
//
//        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//        public sealed class Before : Run {
//            public Before(Type type) : base(type) {}
//        }
//
//        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//        public sealed class After : Run {
//            public After(Type type) : base(type) {}
//        }
//
//        public readonly Type Type;
//
//        private Run(Type type) {
//            Type = type;
//        }
//
//        public bool Equals(Run other) {
//            if (ReferenceEquals(null, other)) return false;
//            if (ReferenceEquals(this, other)) return true;
//            return base.Equals(other) && Equals(Type, other.Type);
//        }
//
//        public override bool Equals(object obj) {
//            if (ReferenceEquals(null, obj)) return false;
//            if (ReferenceEquals(this, obj)) return true;
//            if (obj.GetType() != this.GetType()) return false;
//            return Equals((Run) obj);
//        }
//
//        public override int GetHashCode() {
//            unchecked {
//                return (base.GetHashCode() * 397) ^ (Type != null ? Type.GetHashCode() : 0);
//            }
//        }
//
//        public static bool operator ==(Run left, Run right) {
//            return Equals(left, right);
//        }
//
//        public static bool operator !=(Run left, Run right) {
//            return !Equals(left, right);
//        }
//    }
//
//    public static class ExecutionOrder {
//
//        // Translates all types with dependency annotations
//        // into a single map with Type -> Set<Type> where the Set<Type> denotes the 
//        // RunBefore relation
//        // The RunAfter atttribute needs to be translated to RunBefore
//        public static IDictionary<Type, HashSet<Run.Before>> DependencyList(IEnumerable<Type> allTypes) {
//            var typesWithDeps = allTypes
//                .Select(type => new {type, dependencies = Dependencies(type)})
//                .Where(typeWithDeps => typeWithDeps.dependencies.Any())
//                .ToDictionary(typeWithDeps => typeWithDeps.type, typeWithDeps => typeWithDeps.dependencies.ToList());
//            foreach (var type in typesWithDeps.Keys.ToList()) {
//                var dependencies = typesWithDeps[type];
//                var runAfterDeps = (from dependency in dependencies
//                     where dependency is Run.After
//                     select dependency as Run.After).ToList();
//                foreach (var runAfterDep in runAfterDeps) {
//                    if (!typesWithDeps.ContainsKey(runAfterDep.Type)) {
//                        typesWithDeps.Add(runAfterDep.Type, new List<Run>());
//                    }
//                    typesWithDeps[runAfterDep.Type].Add(new Run.Before(type));
//                    dependencies.Remove(runAfterDep);
//                }
//            }
//            return typesWithDeps.ToDictionary(
//                kvPair => kvPair.Key,
//                kvPair => {
//                    var dependencies = kvPair.Value.Select(a => a as Run.Before);
//                    return new HashSet<Run.Before>(dependencies);
//                });
//        }
//
//        public static Tree<Type> DependencyTree(IDictionary<Type, HashSet<Run.Before>> typesWithDependencies) {
//            var visitedTypes = new HashSet<Type>();
//            Func<Tree<Type>, IEnumerable<Type>, Type, Tree<Type>> addType = null;
//            addType = (tree, dependencyTracker, type) => {
//                Tree<Type> updatedDependencyTree;
//                if (dependencyTracker.Contains(type)) {
//                    throw new Exception("Circular reference detected in dependency chain: " + dependencyTracker.JoinToString(" -> "));
//                } else if (!visitedTypes.Contains(type)) {
//                    visitedTypes.Add(type);
//
//                    HashSet<Run.Before> dependencies;
//                    if (!typesWithDependencies.TryGetValue(type, out dependencies)) {
//                        dependencies = new HashSet<Run.Before>();
//                    }
//
//                    if (dependencies.Count > 0) {
//                        // Recursively add the dependencies
//                        // Then find the common ancestor of all of these types
//                        // Replace the common ancestor with the current type
//
//                        var treeWithAddedDeps = tree;
//                        foreach (var dependency in dependencies) {
//                            treeWithAddedDeps = addType(treeWithAddedDeps, dependencyTracker.Append(type), dependency.Type);
//                        }
//
//                        var commonAncestor = dependencies
//                            .Select(dependency => treeWithAddedDeps.Find(dependency.Type))
//                            .Aggregate(FindCommonAncestor);
//
//                        updatedDependencyTree = commonAncestor.InsertBefore(type).SelectRoot();
//
//                        Debug.Log("added type " + type + " new tree: " + Traverse(updatedDependencyTree).JoinToString(" -> "));
//                    } else {
//                        // This type has no dependencies, just add it to the root of the tree
//                        updatedDependencyTree = tree.AddChild(type);
//                    }
//                } else {
//                    updatedDependencyTree = tree;
//                }
//
//                return updatedDependencyTree.SelectRoot();
//            };
//
//            Tree<Type> dependencyTree = Tree<Type>.Empty;
//            foreach (var type in typesWithDependencies.Keys) {
//                dependencyTree = addType(dependencyTree, Enumerable.Empty<Type>(), type);
//            }
//            return dependencyTree;
//        }
//
//
//        public static IEnumerable<Run> Dependencies(Type type) {
//            return type.GetCustomAttributes(typeof (Run), inherit: true).Cast<Run>().ToList();
//        }
//
//        public static Tree<T> FindCommonAncestor<T>(Tree<T> tree1, Tree<T> tree2) {
//            return (from node1 in TraverseUp(tree1)
//                    from node2 in TraverseUp(tree2)
//                    where node1 == node2
//                    select node1).FirstOrDefault();
//        }
//
//        /// <summary>
//        /// Lazy traversal from the given node to the root of the tree
//        /// </summary>
//        public static IEnumerable<Tree<T>> TraverseUp<T>(this Tree<T> tree) {
//            while (!tree.IsRoot) {
//                yield return tree;
//                tree = tree.AsNode().Parent.Value;
//            }
//            yield return tree;
//        }
//
//        /// <summary>
//        /// Lazy depth-first traversal
//        /// </summary>
//        public static IEnumerable<Tree<T>> Traverse<T>(this Tree<T> tree) {
//            yield return tree;
//            foreach (var child in tree.Children) {
//                foreach (var node in Traverse(child)) {
//                    yield return node;
//                }
//            }
//        }
//
//        public static Tree<T> Find<T>(this Tree<T> tree, T t) {
//            return (from node in Traverse(tree)
//                    where !node.IsRoot && node.AsNode().Data.Equals(t)
//                    select node).FirstOrDefault();
//        }
//
//        public static Tree<T> InsertBefore<T>(this Tree<T> tree, T newNodeData) {
//            Tree<T>.Node newNode = null;
//            if (tree.IsRoot) {
//                var newParent = new Lazy<Tree<T>>(() => new Tree<T>.Root(children: new[] {newNode.AsNode()}));
//                var newNodeChildren = tree.Children.Select(child => {
//                    return child.SwitchParent(new Lazy<Tree<T>>(() => newNode));
//                });
//                newNode = new Tree<T>.Node(newParent, newNodeData, newNodeChildren);
//            } else {
//                // TODO Recusively update the parent!
//
//
//                var newParent = new Lazy<Tree<T>>(() => {
//                    var oldParent = tree.AsNode().Parent.Value;
//                    var children = oldParent.Children.Replace(tree.AsNode(), newNode);
//
//                    if (oldParent.IsRoot) {
//                        return new Tree<T>.Root(children);
//                    }
//                    return new Tree<T>.Node(oldParent.AsNode().Parent, oldParent.AsNode().Data, children);
//                });
//
//                var newNodeChildren = new [] {
//                    tree.AsNode().SwitchParent(new Lazy<Tree<T>>(() => newNode))
//                };
//                newNode = new Tree<T>.Node(newParent, newNodeData, newNodeChildren);
//            }
//            return newNode;
//        }
//
//        public static Tree<T> SelectRoot<T>(this Tree<T> tree) {
//            return TraverseUp(tree).Last();
//        }
//
//        public static IEnumerable<T> Replace<T>(this IEnumerable<T> e, T original, T replacement) {
//            return e.Select(elem => elem.Equals(original) ? replacement : elem);
//        } 
//
//        public static IEnumerable<T> Append<T>(this IEnumerable<T> e, T value) {
//            return e.Concat(new[] {value});
//        }
//
//        public static string JoinToString<T>(this IEnumerable<T> e, string separator) {
//            return e.Aggregate("", (acc, value) => {
//                if (acc == "") {
//                    return value.ToString();
//                }
//                return acc + separator + value;
//            });
//        }
//    }
//
//    public abstract class Tree<T> {
//        public static readonly Tree<T> Empty = new Root(Enumerable.Empty<Node>());
//
//        public readonly IEnumerable<Node> Children;
//
//        private Tree(IEnumerable<Node> children) {
//            Children = children;
//        }
//
//        public abstract bool IsRoot { get; }
//
//        public Root AsRoot() {
//            if (IsRoot) {
//                return this as Root;
//            }
//            throw new Exception("Node is not root");
//        }
//
//        public Node AsNode() {
//            if (!IsRoot) {
//                return this as Node;
//            }
//            throw new Exception("Root is not node");
//        }
//
//        public sealed class Root : Tree<T> {
//            public Root(IEnumerable<Node> children) : base(children) {}
//
//            public override bool IsRoot {
//                get { return true; }
//            }
//
//            public override string ToString() {
//                return string.Format("Root, children: " + Children.Count());
//            }
//        }
//
//        public sealed class Node : Tree<T> {
//            private readonly Lazy<Tree<T>> _parent;
//            public readonly T Data;
//
//            public Node(Lazy<Tree<T>> parent, T data, IEnumerable<Node> children) : base(children) {
//                Data = data;
//                _parent = parent;
//            }
//
//            public Lazy<Tree<T>> Parent {
//                get { return _parent; }
//            }
//
//            public override bool IsRoot {
//                get { return false; }
//            }
//
//            public Node SwitchParent(Lazy<Tree<T>> newParent) {
//                return new Node(newParent, Data, Children);
//            }
//
//            public override string ToString() {
//                return string.Format("Node " + Data + ", children: " + Children.Count());
//            }
//        }
//
//        public Tree<T> AddChild(T childData) {
//            return AddChild(childData, existingChildren: null);
//        }
//
//        public Tree<T> AddChild(T childData, IEnumerable<Node> existingChildren) {
//            Tree<T> parent = null;
//            var childNode = new Node(
//                new Lazy<Tree<T>>(() => parent), 
//                childData, 
//                existingChildren ?? Enumerable.Empty<Node>());
//            var children = Children.Append(childNode);
//            if (IsRoot) {
//                parent = new Root(children);
//            } else {
//                var node = this as Node;
//                parent = new Node(node.Parent, node.Data, children);
//            }
//            return parent;
//        }
//    }
//}
