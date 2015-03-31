using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ColorImage;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Interop.Types;

public class JointsUpdate : MonoBehaviour {
	
	private const float HEIGHT_MULTIPLIER = 2; // Value should be 2 when using ortographic camera in unity
	private const int DEPTH_CONSTANT = 5; // Value should be 2 when using ortographic camera in unity
	
	private float textureXPos;
	private float textureYPos;
	private float textureDimensionX;
	private float textureDimensionY;
	
	public GameObject myCube;
	public GUIText guiTextComponent;
	public LineRenderer lineRenderer;
	
	private FrameRateCalc frameRateCalc;

	public GameObject joint;
	
	private Dictionary<JointType, GameObject> jointsGameObjects = new Dictionary<JointType, GameObject>();
	private Dictionary<JointType, Xtr3D.Net.ExtremeMotion.Data.Joint> typesToJoints = new Dictionary<JointType, Xtr3D.Net.ExtremeMotion.Data.Joint>();
	
	private JointType[] jointTypesArray = new JointType[]{ JointType.Head ,JointType.ShoulderCenter,JointType.Spine,JointType.HipCenter,JointType.ShoulderLeft
										 ,JointType.ShoulderRight, JointType.ElbowLeft, JointType.ElbowRight,JointType.HandRight,JointType.HandLeft };
	
	long lastFrameID = -1;
	long currFrameID = -1;
	
	
	void Start () {
		CalculateTextureParams();
		CreateJoints();
        Xtr3dGeneratorStateManager.RegisterDataCallback(MyDataFrameReady);
		// init frameRateCalc for calculating avarage running fps in the last given frames
		frameRateCalc = new FrameRateCalc(50);
	}
	
	private void CalculateTextureParams()
	{
		float heightMeasure = Camera.main.orthographicSize * HEIGHT_MULTIPLIER; // Calculating the current world view height measure
		textureDimensionX = Math.Abs(myCube.transform.localScale.x * (Screen.height/heightMeasure)); // calculating current cube size accroding to current screen resolution
		textureDimensionY = Math.Abs(myCube.transform.localScale.y * (Screen.height/heightMeasure)); // calculating current cube size accroding to current screen resolution
		Vector3 cubePos = Camera.main.WorldToScreenPoint(myCube.transform.position);
		textureXPos = cubePos.x - textureDimensionX/2;
		textureYPos = cubePos.y + textureDimensionY/2;
		
	}
	
	private void CreateJoints()
	{
		foreach (JointType type in jointTypesArray)
			jointsGameObjects[type] = (GameObject) Instantiate(joint,new Vector3(0f,0f,-5),Quaternion.identity);
	}
	
	
	private void MyDataFrameReady(object sender, Xtr3D.Net.ExtremeMotion.Data.DataFrameReadyEventArgs e)
	{
        using (var dataFrame = e.OpenFrame() as DataFrame)
        {
			if (dataFrame!=null)
			{
				currFrameID = dataFrame.FrameKey.FrameNumberKey;
				Debug.Log ("Skeleton frame: " + currFrameID + ", state: " + dataFrame.Skeletons[0].TrackingState + ", proximity: " + dataFrame.Skeletons[0].Proximity.SkeletonProximity);
				if (currFrameID <= lastFrameID  && currFrameID!=1) //currFrameId=1 on reset/resume!!!
					return;
				
				lastFrameID = currFrameID;
				
				//update frameRateCalc, we need to call this every frame as we are calculating avarage fps in the last x frames.
				frameRateCalc.UpdateAvgFps();
				JointCollection skl = dataFrame.Skeletons[0].Joints;

				//use a copy of the Joints data structure as the dataFrame values can change.
				typesToJoints[JointType.ShoulderCenter] = skl.ShoulderCenter;
				typesToJoints[JointType.Spine] 			= skl.Spine;
				typesToJoints[JointType.HipCenter] 		= skl.HipCenter;
				typesToJoints[JointType.ShoulderLeft] 	= skl.ShoulderLeft;
				typesToJoints[JointType.ShoulderRight]	= skl.ShoulderRight;
				typesToJoints[JointType.ElbowLeft] 		= skl.ElbowLeft;
				typesToJoints[JointType.ElbowRight] 	= skl.ElbowRight;
				typesToJoints[JointType.HandRight]		= skl.HandRight;
				typesToJoints[JointType.HandLeft] 		= skl.HandLeft;
				typesToJoints[JointType.Head] 			= skl.Head;
			}
		}
		
	}
	// Update is called once per frame
	void OnGUI () {
		//display in our text component the avg fps
		guiTextComponent.text = System.String.Format("{0:F2} Skeleton FPS",frameRateCalc.GetAvgFps());
	}
	
	void Update()
	{
		//don`t do anything if no info was passed yet
		if (!typesToJoints.ContainsKey(JointType.Head))
			return;
		
		foreach (JointType type in jointTypesArray)
		{
			if (typesToJoints [type].jointTrackingState == JointTrackingState.Tracked)
			{
				float x = textureXPos +     typesToJoints[type].skeletonPoint.ImgCoordNormHorizontal * textureDimensionX;
				float y = textureYPos + -1* typesToJoints[type].skeletonPoint.ImgCoordNormVertical * textureDimensionY;
				float z = DEPTH_CONSTANT;
				
				jointsGameObjects[type].SetActive(true);
				jointsGameObjects[type].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, z));
			}
			else 
			{
				jointsGameObjects[type].SetActive(false);
			}
		}
	}
	
	
}
