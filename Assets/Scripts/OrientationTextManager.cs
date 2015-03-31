using UnityEngine;
using System.Collections;

public class OrientationTextManager : MonoBehaviour {
	
	public TextMesh MessageText;
	public TextMesh MessageTextUpsideDown;
	
	void Awake () {
		MessageText.GetComponent<Renderer>().material.color = Color.red;
		MessageTextUpsideDown.GetComponent<Renderer>().material.color = Color.red;
	}
	
	// Update is called once per frame
	void Update () {
		MessageText.GetComponent<Renderer>().enabled = Input.deviceOrientation == DeviceOrientation.Portrait;
		MessageTextUpsideDown.GetComponent<Renderer>().enabled = Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown;
	}
}
