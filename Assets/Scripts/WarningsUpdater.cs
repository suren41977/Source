using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Xtr3D.Net;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ColorImage;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Interop.Types;
using Xtr3D.Net.Patterns;

public class WarningsUpdater : MonoBehaviour {
	
	private const string basicWarningsText = "Warnings:\n";
	
    private FrameEdges edgeWarnings = FrameEdges.None;
	private ImageWarnings imageWarnings = ImageWarnings.None;
	private ImageStreamWarnings imageSteamWarnings = ImageStreamWarnings.None;
	
	public GUIText WarningsText;
	
	void Start () {
		Xtr3dGeneratorStateManager.RegisterColorImageCallback(MyColorImageFrameReadyEventHandler);
		Xtr3dGeneratorStateManager.RegisterDataCallback(MyDataFrameReadyEventHandler);
		
		if (Application.platform == RuntimePlatform.WindowsPlayer)
		{
			WarningsText.fontSize = 20;
		}
		
		Debug.Log("Warnings update registration complete");
	}
	
	private void MyDataFrameReadyEventHandler(object sender, Xtr3D.Net.ExtremeMotion.Data.DataFrameReadyEventArgs e)
	{
        using (var dataFrame = e.OpenFrame() as DataFrame)
        {
			if (dataFrame != null)
			{
				Skeleton skl = dataFrame.Skeletons[0];
				edgeWarnings = skl.ClippedEdges;
				StringBuilder sb = new StringBuilder(basicWarningsText);
				CheckEdgeWarnings(sb);
				int warningsCount = CountLines(sb.ToString ()) - 1;
				Debug.Log("Warnings frame: " + dataFrame.FrameKey.FrameNumberKey + ", contains " + (warningsCount -1) + " " + sb.ToString());
			}
		}
	}

	private void MyColorImageFrameReadyEventHandler(object sender, ColorImageFrameReadyEventArgs e)
	{
        using (ColorImageFrame colorImageFrame = e.OpenFrame() as ColorImageFrame)
        {
            if (colorImageFrame != null)
            {
				imageWarnings = colorImageFrame.Warnings;
 			    
                // some of the warnings are on the stream and not on the image itself.
				var colorImageStream = colorImageFrame.Stream as Xtr3D.Net.BaseTypes.ImageStreamBase<FrameKey, ColorImage>;
                imageSteamWarnings = colorImageStream.Warnings;
				StringBuilder sb = new StringBuilder(basicWarningsText);
				CheckImageWarnings(sb);
				int warningsCount = CountLines(sb.ToString ()) - 1;
				Debug.Log ("Warnings frame: " + colorImageFrame.FrameKey.FrameNumberKey + ", contains " + (warningsCount - 1) + " " +sb.ToString());
			} 
        }
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {
		StringBuilder sb = new StringBuilder(basicWarningsText);
		
        if (edgeWarnings != FrameEdges.None)
        {
            CheckEdgeWarnings(sb);
        }
        
        if (imageWarnings != ImageWarnings.None)
        {
            CheckImageWarnings(sb);
        }
        
        if (imageSteamWarnings != ImageStreamWarnings.None)
        {
            CheckImageStreamWarnings(sb);
        }
		
		string text = sb.ToString();
		if (text == basicWarningsText)
		{
			WarningsText.text = String.Empty;
		}
		else
		{
			WarningsText.text = text;
		}
	}
	
	private void CheckEdgeWarnings(StringBuilder sb)
	{
		if (edgeWarnings.HasFlag(FrameEdges.Far))
		{
			sb.Append(" - Too far\n");
		}
		if (edgeWarnings.HasFlag(FrameEdges.Near))
		{
			sb.Append(" - Too close\n");
		}
		if (edgeWarnings.HasFlag(FrameEdges.Left))
		{
			sb.Append(" - Too left\n");
		}
		if (edgeWarnings.HasFlag(FrameEdges.Right))
		{
			sb.Append(" - Too right\n");
		}
	}
	
	private void CheckImageWarnings(StringBuilder sb)
	{
		if (imageWarnings.HasFlag(ImageWarnings.LightLow))
		{
			sb.Append(" - Low lighting\n");
		}
		if (imageWarnings.HasFlag(ImageWarnings.StrongBacklighting))
		{
			sb.Append(" - Strong backlighting\n");
		}
		if (imageWarnings.HasFlag(ImageWarnings.TooManyPeople))
		{
			sb.Append(" - Too many people\n");
		}
	}
	
	private void CheckImageStreamWarnings(StringBuilder sb)
	{
		if (imageSteamWarnings.HasFlag(ImageStreamWarnings.EnvironmentChanged))
		{
			sb.Append(" - Environment changed\n");
		}
		if (imageSteamWarnings.HasFlag(ImageStreamWarnings.LowFrameRate))
		{
			sb.Append(" - Low frame rate\n");
		}
	}

	private int CountLines(string text)
	{
		int linesCount = 0;
		int newLineIndex = 0;
		do
		{
			text = text.Substring (newLineIndex+1);
			linesCount++;
			newLineIndex = text.IndexOf("\n");
		}
		while(newLineIndex >= 0);
		return linesCount;
	}

}
