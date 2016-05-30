using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityExecutionOrder {
    public static class ExecutionOrder {

        public static IDictionary<Type, HashSet<Run.Before>> DependencyList(IEnumerable<Type> allTypes) {
            var typesWithDeps = allTypes
                .Select(type => new {type, dependencies = Dependencies(type)})
                .Where(typeWithDeps => typeWithDeps.dependencies.Any())
                .ToDictionary(typeWithDeps => typeWithDeps.type, typeWithDeps => typeWithDeps.dependencies.ToList());
            foreach (var type in typesWithDeps.Keys.ToList()) {
                var dependencies = typesWithDeps[type];
                var runAfterDeps = (from dependency in dependencies
                     where dependency is Run.After
                     select dependency as Run.After).ToList();
                foreach (var runAfterDep in runAfterDeps) {
                    if (!typesWithDeps.ContainsKey(runAfterDep.Type)) {
                        typesWithDeps.Add(runAfterDep.Type, new List<Run>());
                    }
                    typesWithDeps[runAfterDep.Type].Add(new Run.Before(type));
                    dependencies.Remove(runAfterDep);
                }
            }
            return typesWithDeps.ToDictionary(
                kvPair => kvPair.Key,
                kvPair => {
                    var dependencies = kvPair.Value.Select(a => a as Run.Before);
                    return new HashSet<Run.Before>(dependencies);
                });
        }

        public static IList<Type> GetOrder(IDictionary<Type, HashSet<Run.Before>> typesWithDependencies) {
            var visitedTypes = new HashSet<Type>();
            var order = new List<Type>();
            Action<IEnumerable<Type>, Type> addType = null;
            addType = (dependencyTracker, type) => {
                if (dependencyTracker.Contains(type)) {
                    throw new Exception("Circular reference detected in dependency chain: " +
                                        dependencyTracker.JoinToString(" -> "));
                } 
                
                if (!visitedTypes.Contains(type)) {
                    visitedTypes.Add(type);

                    HashSet<Run.Before> dependencies;
                    if (!typesWithDependencies.TryGetValue(type, out dependencies)) {
                        dependencies = new HashSet<Run.Before>();
                    }

                    if (dependencies.Count > 0) {
                        foreach (var dependency in dependencies) {
                            addType(dependencyTracker.Append(type), dependency.Type);
                        }

                        var index = order.Aggregate(order.Count - 1, (lowestIndex, t) => {
                            var typeIndex = order.IndexOf(t);
                            return typeIndex < lowestIndex ? typeIndex : lowestIndex;
                        });
                        order.Insert(index, type);
                    } else {
                        order.Add(type);
                    }
                }
            };

            foreach (var type in typesWithDependencies.Keys) {
                addType(Enumerable.Empty<Type>(), type);
            }
            return order.ToList();
        }

        public static IEnumerable<Run> Dependencies(Type type) {
            return type.GetCustomAttributes(typeof (Run), inherit: true).Cast<Run>().ToList();
        }

        private static IEnumerable<T> Append<T>(this IEnumerable<T> e, T value) {
            return e.Concat(new[] {value});
        }

        public static string JoinToString<T>(this IEnumerable<T> e, string separator) {
            return e.Aggregate("", (acc, value) => {
                if (acc == "") {
                    return value.ToString();
                }
                return acc + separator + value;
            });
        }
    }
}
