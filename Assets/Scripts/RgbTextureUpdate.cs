using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ColorImage;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Interop.Types;
using Xtr3D.Net;

/*
 * This example shows how to extract the 640x480 RGB byte[] value which the engine provides, 
 * and shows a sample unity3d code of rendering this byte[] into a texture.
 * 
 * Make sure the object on which the texture is applied is flipped on the x  (transform.scale is negative)
 */

public class RgbTextureUpdate : MonoBehaviour {
	
	public bool drawRGB = true;
	public GUIText myText;
	public GameObject myTexture;
	private Texture2D buffer;
	private long lastDrawnFrameId= -1;
	private byte[] newRgbImage;
	private long newFrameId = -1;
	private FrameRateCalc frameRateCalc;
    private ImageInfo imageInfo;

	void Start () {
        Xtr3dGeneratorStateManager.RegisterColorImageCallback(MyColorImageFrameReadyEventHandler);
        imageInfo = Xtr3dGeneratorStateManager.GeneratorImageInfo;
		Debug.Log("ImageInfo format"+ imageInfo.Format + " "+ imageInfo.Width +","+ imageInfo.Height);
        buffer = new Texture2D(imageInfo.Width, imageInfo.Height,TextureFormat.RGB24,false);
	
		// init frameRateCalc for calculating avarage running fps in the last given frames
		frameRateCalc = new FrameRateCalc(50);
		newRgbImage = new byte[imageInfo.Width*imageInfo.Height*imageInfo.BitsPerPixel];
		
	}
	private void MyColorImageFrameReadyEventHandler(object sender, ColorImageFrameReadyEventArgs e)
	{
        using (ColorImageFrame colorImageFrame = e.OpenFrame() as ColorImageFrame)
        {
            if (colorImageFrame != null)
            {
				//update frameRateCalc, we need to call this every frame as we are calculating avarage fps in the last x frames.
				frameRateCalc.UpdateAvgFps();
				
				ColorImage colorImage = colorImageFrame.ColorImage;
				
				colorImage.Image.CopyTo(newRgbImage,0); 
				newFrameId = colorImageFrame.FrameKey.FrameNumberKey;
				Debug.Log ("Raw image frame: " + newFrameId);
			} 
        }
	}
	
	// Update is called once per frame
	void Update () {
		if (lastDrawnFrameId != newFrameId)  //no need to draw again if RGB image was not changed
			if (newRgbImage!=null)
			{
				lastDrawnFrameId=newFrameId;
				
				if (drawRGB)
				{
					buffer.LoadRawTextureData(newRgbImage);
					buffer.Apply();
					myTexture.GetComponent<Renderer>().material.mainTexture = buffer;
				}
		}
	}
	
	void OnGUI()
	{
		myText.text = string.Format("{0}\n{1:F2} RGB FPS", drawRGB? "":"Click RGB to resume", frameRateCalc.GetAvgFps());
	}
}
