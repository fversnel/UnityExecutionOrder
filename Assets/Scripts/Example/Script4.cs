using UnityEngine;
using System.Collections;
using UnityExecutionOrder;

[Run.Before(typeof(Script1))]
[Run.Before(typeof(Script3))]
public class Script4 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
