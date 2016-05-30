using UnityEngine;
using System.Collections;
using UnityExecutionOrder;

[Run.After(typeof(Script3))]
[Run.Before(typeof(Script1))]
public class Script2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
