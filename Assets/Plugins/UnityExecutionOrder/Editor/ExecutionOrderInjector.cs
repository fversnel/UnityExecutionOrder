using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace UnityExecutionOrder {

    [InitializeOnLoad]
    public class ExecutionOrderInjector {
        private static readonly string ExecutionOrderPath = "./execution_order.xml";

        static ExecutionOrderInjector() {
            var monoScripts = MonoImporter.GetAllRuntimeMonoScripts()
                .Where(script => script.GetClass() != null)
                .ToDictionary(script => script.GetClass());
            var executionOrder = ExecutionOrder.GetOrder(ExecutionOrder.DependencyList(monoScripts.Keys));

            var serializer = new XmlSerializer(typeof (List<string>));
            IList<Type> serializedExecutionOrder;
            try {
                var monoScriptsByString = monoScripts.ToDictionary(kvPair => kvPair.Key.ToString(), kvPair => kvPair.Value);
                using (var reader = new FileStream(ExecutionOrderPath, FileMode.Open)) {
                    serializedExecutionOrder = (serializer.Deserialize(reader) as List<string>)
                        .Where(serializedType => monoScriptsByString.ContainsKey(serializedType))
                        .Select(serializedType => monoScriptsByString[serializedType].GetClass())
                        .ToList();
                }
            } catch (FileNotFoundException) {
                serializedExecutionOrder = new List<Type>();
            }

            if (!executionOrder.SequenceEqual(serializedExecutionOrder)) {
                Debug.Log("Setting script execution order: " + executionOrder.JoinToString(" -> "));

                for (int i = 0; i < executionOrder.Count; i++) {
                    var scriptType = executionOrder[i];
                    MonoImporter.SetExecutionOrder(monoScripts[scriptType], order: 100 + i);
                }

                // Re-serialize execution order
                using (var writer = new FileStream(ExecutionOrderPath, FileMode.OpenOrCreate)) {
                    serializer.Serialize(writer, executionOrder.Select(type => type.ToString()).ToList());    
                }
            }
        }
    }
}
