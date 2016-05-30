using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections;
using UnityExecutionOrder;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {

	    var assembly = Assembly.GetExecutingAssembly();
	    var dependencyList = ExecutionOrder.DependencyList(assembly.GetTypes());
	    var dependencyTree = ExecutionOrder.GetOrder(dependencyList);

        foreach (var node in dependencyTree) {
            Debug.Log(node);
	    }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
