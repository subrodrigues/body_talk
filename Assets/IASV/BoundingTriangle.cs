using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingTriangle : MonoBehaviour {
	public GameObject leftHandObj, rightHandObj, headObj;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		printWorldPos (leftHandObj);
		printWorldPos (rightHandObj);
		printWorldPos (headObj);
	}

	void printWorldPos(GameObject obj){
		Debug.Log (obj.transform.position.ToString("F4"));
	}
}
