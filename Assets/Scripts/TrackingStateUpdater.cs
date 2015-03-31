using UnityEngine;

using System;
using System.Collections.Generic;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ColorImage;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Interop.Types;

/* Note: this code need to be synced (using locks for example) , as the Update thread is read from a different thread than the MyColorImageFrameReadyEventHandler thread.
 * For clarity of the code, it was not added here.  See JointsUpdate.cs for an example.
 */
public class TrackingStateUpdater : MonoBehaviour {
	
	private TrackingState trackingState;
	private const string basicTrackingText = "Tracking State: ";
    private string text;
    
	private Dictionary<TrackingState, string> stateTextDictionary = new Dictionary<TrackingState, string>() { 
		{TrackingState.Initializing, "Initializing"},
		{TrackingState.Calibrating, "Calibrating"},
		{TrackingState.NotTracked, "Not Tracked"},
		{TrackingState.Tracked, "Tracked"}
	};
	public GUIText GuiTextComponent;
	public GameObject GuiCalibrationCube;
	
	// Use this for initialization
	void Start () {
		Xtr3dGeneratorStateManager.RegisterDataCallback(OnDataFrameReceived);
	}
	
	private void OnDataFrameReceived(object sender, Xtr3D.Net.ExtremeMotion.Data.DataFrameReadyEventArgs e)
	{
        using (var dataFrame = e.OpenFrame() as DataFrame)
        {
			if (dataFrame != null)
			{
				if (!stateTextDictionary.TryGetValue(trackingState, out text))
				{
					text = "UNRECOGNIZED STATE";
				}
				trackingState = dataFrame.Skeletons[0].TrackingState;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
        GuiTextComponent.text = basicTrackingText + text;
        // If we're in calibration mode - show the Calibration Image
		GuiCalibrationCube.GetComponent<Renderer>().enabled = (trackingState == TrackingState.Calibrating);
	}
}