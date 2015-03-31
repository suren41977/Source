using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Gesture;
using Xtr3D.Net.ExtremeMotion.Interop.Types;

class GestureMessage
{
	public GestureMessage(String _text)
	{
		timeToLiveCounter = 0;
		text = _text;
	}
	public int timeToLiveCounter;
	public String text;
};

public class GesturesDetection : MonoBehaviour
{
	public GUIText DetectedGesturesStreamGUIText;
	public StringBuilder detectedGesturesStreamString= new StringBuilder("");

	private Dictionary<BaseGesture.GestureType, int> gestureTypeToDelay = new Dictionary<BaseGesture.GestureType, int>() { 
		{BaseGesture.GestureType.STATIC_POSITION, 1},
		{BaseGesture.GestureType.HEAD_POSITION, 1},
		{BaseGesture.GestureType.SWIPE, 30},
		{BaseGesture.GestureType.WINGS, 5},
		{BaseGesture.GestureType.SEQUENCE, 30},
		{BaseGesture.GestureType.UP, 10},
		{BaseGesture.GestureType.DOWN, 10}};
	
	
	private Dictionary<int, GestureMessage> gestureMessages = new Dictionary<int, GestureMessage> {};  
	
	/**		
	 *  This is an example to detecting gesture in custom-code.
	 * The uncommented version uses the built-in xtr3d gestures stream and xml definition 
In Start() add:
	Xtr3dGeneratorStateManager.RegisterDataCallback(MyDataFrameReady);
	
	private void MyDataFrameReady(object sender, DataFrameReadyEventArgs e)
	{

        using (var dataFrame = e.OpenFrame() as DataFrame)
        {
			if (dataFrame!=null)
			{
				JointCollection joints= dataFrame.Skeletons[0].Joints;
				if (dataFrame.Skeletons[0].TrackingState==TrackingState.Tracked)
					detectStaticTPosition(joints);
				
			}
		}
		
	} 
	
	private void detectStaticTPosition(JointCollection joints)
	{
		Point handLeft =     joints.HandLeft.skeletonPoint;
		Point shoulderLeft = joints.ShoulderLeft.skeletonPoint;
		
		float leftHandAngle = Vector2.Angle(
		       new Vector2(handLeft.X -  shoulderLeft.X, 
		                   handLeft.Y - shoulderLeft.Y),
		       new Vector2(0f,1f));
		
		Point handRight =     joints.HandRight.skeletonPoint;
		Point shoulderRight = joints.ShoulderRight.skeletonPoint;
		
		float rightHandAngle = Vector2.Angle(
		       new Vector2(handRight.X -  shoulderRight.X, 
		                   handRight.Y - shoulderRight.Y),
		       new Vector2(0f,1f));
		
		
		float DELTA=15f;
		if ((leftHandAngle > 90f-DELTA && leftHandAngle < 90f+DELTA && rightHandAngle> 90f-DELTA && rightHandAngle< 90f+DELTA) )
			customGesturesFromSkeletonString="Detected from custom code: T Position";
		else 
			customGesturesFromSkeletonString="";
	}

	
	*/

	
	public void Start()
	{
		Xtr3dGeneratorStateManager.RegisterGesturesCallback(MyGesturesFrameReady);
	}
	
	//code sample of getting gestures from xtr3d engine. These gestures are defined inside SamplePoses.xml
	private void MyGesturesFrameReady(object sender, GesturesFrameReadyEventArgs e)
	{
		// Opening the received frame
		using (var gesturesFrame = e.OpenFrame() as GesturesFrame)
		{
			if (gesturesFrame != null)
			{
				StringBuilder logLine = new StringBuilder();
				logLine.AppendFormat("Gestures frame: {0}, contains {1} gestures\n", gesturesFrame.FrameKey, gesturesFrame.FirstSkeletonGestures().Length);
				int gestureCounter = 0;
				foreach (BaseGesture gesture in gesturesFrame.FirstSkeletonGestures())
				{
					string additionalGestureData="";
					// Update messages for gesture
                    if (!gestureMessages.ContainsKey(gesture.ID))
                    {
                        gestureMessages.Add(gesture.ID, new GestureMessage(gesture.Description));
                    }
					gestureMessages[gesture.ID].timeToLiveCounter = gestureTypeToDelay[gesture.Type];
					switch (gesture.Type)
					{
						case BaseGesture.GestureType.HEAD_POSITION:
						{
							HeadPositionGesture headPositionGesture = gesture as HeadPositionGesture;
							gestureMessages[gesture.ID].text = gesture.Description + " " + headPositionGesture.RegionIndex;
							additionalGestureData = " (" + headPositionGesture.RegionIndex+")";
						    break;
						}
						case BaseGesture.GestureType.WINGS:
						{
							WingsGesture wingsGesture = gesture as WingsGesture;
							gestureMessages[gesture.ID].text = gesture.Description + " " + wingsGesture.ArmsAngle;
							additionalGestureData = " (" + wingsGesture.ArmsAngle + ")";
							break;
						}
						default:
							break;
					}
					logLine.AppendFormat("{0}. Gesture id: {1} -  {2}{3}\n",gestureCounter, gesture.ID , gesture.Description, additionalGestureData);
					gestureCounter++;
				}
				Debug.Log(logLine);
			}
			
			// Generate gestures text
			detectedGesturesStreamString = new StringBuilder();
			foreach (int id in gestureMessages.Keys)
			{
				if (gestureMessages[id].timeToLiveCounter > 0)
				{
					detectedGesturesStreamString.AppendFormat("{0} - {1}\n" , id, gestureMessages[id].text);
					gestureMessages[id].timeToLiveCounter--;
				}
			}
		}
	}


	void OnGUI () {
		DetectedGesturesStreamGUIText.text = detectedGesturesStreamString.ToString();
	}
}
	
	
	


