using UnityEngine;
using System.Collections;

public class StartStopDrawingRGB : MonoBehaviour {
	public RgbTextureUpdate textureUpdateScript;
	
	void  OnMouseDown () {
		bool oldState = textureUpdateScript.drawRGB;
		bool newState = !oldState;
		textureUpdateScript.drawRGB = newState;
		Debug.Log("Mouse down on the texture. DrawRGB is now " + newState);
	}
}
