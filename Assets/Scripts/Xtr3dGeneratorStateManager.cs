using UnityEngine;
using System.Collections;

using System;
using Xtr3D.Net.Exceptions;
using Xtr3D.Net.ExtremeMotion;
using Xtr3D.Net.ColorImage;
using Xtr3D.Net.AllFrames;
using Xtr3D.Net.ExtremeMotion.Data;
using Xtr3D.Net.ExtremeMotion.Gesture;
using Xtr3D.Net.ExtremeMotion.Interop.Types;
using Xtr3D.Net.BaseTypes;
using Xtr3D.Net;


/**
 * This script init and destory the Xtr3d engine. 
 * Only one instance of this class should exist in the whole project. 
 * 
 * Note:
 * 1. If you have multiple scenes in your project, make sure the GeneratorSingleton.Instance.Initialize() and Start() methods is 
 * only called once and not at each scene.
 * In this case, as there is only one scene, it is implemented on the Awake() and OnApplicationQuit() methods
 * 2. this code need to be synced (using locks for example) , as the Update thread is read from a different thread than the MyColorImageFrameReadyEventHandler thread.
 * For clarity of the code, it was not added here.  See JointsUpdate.cs for an example.
 */
public class Xtr3dGeneratorStateManager : MonoBehaviour {
	public GUIText myGuiText;
	private string messageString = "";
	private bool startAlreadyCalled  = false;
	private bool generatorInitialized = false;
	private bool cameraErrorDetected = false;

    //In this sample, GeneratorSingleton.Instance.AllFramesReady is being used, in order to prevent any lag between displaying the skeleton and the image.
    //However, in case displaying the skeleton is not needed (e.g. calibration stage), using only GeneratorSingleton.Instance.ColorImageFrameReady results in better performance.
    //For that matter, the following two members are to easily allow switching between waiting for synchronized frames, 
    //coming from all streams (via GeneratorSingleton.Instance.AllFramesReady) 
    //and waiting seperately for each stream (GeneratorSingleton.Instance.DataFrameReady and GeneratorSingleton.Instance.ColorImageFrameReady).
    //For the latter, these two members need not be used, but instead issue the following (each in the corresponding .cs file):
    //
    // GeneratorSingleton.Instance.DataFrameReady += JointsUpdate.MyDataFrameReady
    // GeneratorSingleton.Instance.ColorImageFrameReady += RgbTextureUpdate.MyColorImageFrameReadyEventHandler

    private static EventHandler<ColorImageFrameReadyEventArgs> MyColorImageFrameReadyEventHandler = null;
    private static EventHandler<DataFrameReadyEventArgs> MyDataFrameReady = null;
	private static EventHandler<GesturesFrameReadyEventArgs> MyGesturesFrameReady = null;

    public static ImageInfo GeneratorImageInfo
    {
        get;
		private set;
	}
	
    public static void RegisterColorImageCallback(EventHandler<ColorImageFrameReadyEventArgs> h)
    {
        MyColorImageFrameReadyEventHandler += h;
    }

    public static void RegisterDataCallback(EventHandler<DataFrameReadyEventArgs> h)
    {
        MyDataFrameReady += h;
    }	
	public static void RegisterGesturesCallback(EventHandler<GesturesFrameReadyEventArgs> h)
    {
        MyGesturesFrameReady += h;
    }	
	
	

	//Awake is called before onStart, thus other components can safely register.
	void Awake () {
		
		if (Application.platform == RuntimePlatform.IPhonePlayer)
			Application.targetFrameRate = 10;
		if (Application.platform== RuntimePlatform.Android)
			Application.targetFrameRate = 10; //reduce rendering on purpose, allow more CPU for engine
		
		PlatformType platformType = getPlatformType();
		Debug.Log("Platform type detected: "+ platformType +" and originally was " + Application.platform);
		ImageInfo.ImageFormat imageFormat = ImageInfo.ImageFormat.RGB888;
			
		GeneratorImageInfo = new ImageInfo(ImageResolution.Resolution640x480, imageFormat);
		Debug.Log("Calling Initialize()");
        try
        {
            GeneratorSingleton.Instance.Initialize(platformType, GeneratorImageInfo);
			generatorInitialized = true;
            Debug.Log("Initialize() completed");
			try 
			{
				string path="";
				
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					#if UNITY_WINRT  
						//System.IO do not compile exist on WinRT, so we also need a compilation ifdef
					#else
					try {
						
						System.IO.File.Copy(Application.streamingAssetsPath + "/SamplePoses.xml",Application.persistentDataPath+ "/SamplePoses.xml",true);
					} catch (Exception ex) {
						Debug.LogError("copying SamplePoses.xml file to Data folder failed: " + ex.Message);
					}
					#endif
				}
				else if (Application.platform == RuntimePlatform.Android)
				{
					path = Application.persistentDataPath + "/";
				}
			
				GeneratorSingleton.Instance.SetGestureRecognitionFile(path+"SamplePoses.xml");
			} catch (Exception ex)
			{
				Debug.LogError("failed to load gesture recognition. Either SamplePoses.xml ot SkeletonGestureRecognition.dll are missing");	
				Debug.LogException(ex);
			}

            Debug.Log("Registering AllFrames()");
            GeneratorSingleton.Instance.AllFramesReady += MyAllFramesReadyEventHandler;
			
            Debug.Log("Registering AllFrames() completed");
        }
        catch (InvalidLicenseException ex)
		{
            messageString = "License Error: Invalid license";
			Debug.LogError(messageString  + ex.Message);
		}
		catch (MissingLicenseException ex)
		{
			messageString = "License Error: Missing license";
			Debug.LogError(messageString  + ex.Message);
		}
		catch (ExpiredLicenseException ex)
		{
			messageString = "License Error: License expired";
			Debug.LogError(messageString  + ex.Message);
		}
        catch (NotInitializedException ex)
        {
            messageString = "Please verify Init was successfully called";
			Debug.LogError(messageString  + ex.Message);
        }
		catch (Exception ex)
		{
			if (ex.Message.Contains("DllNotFoundException: Xtr3dManager"))
				messageString = "dll not found in the directory, or xtr3d prerequisites not installed properly";
			else 
				messageString = "Generic exception: \n";
				Debug.LogError(messageString + ex.ToString());
		}
		Debug.Log("Calling Initialize() ended (with either success or error");
		
	}
	
	public static PlatformType getPlatformType()
	{
		PlatformType platform = PlatformType.WINDOWS; 
		
		if (Application.platform==RuntimePlatform.Android)
			platform = PlatformType.ANDROID;
		else if (Application.platform==RuntimePlatform.IPhonePlayer)
			platform = PlatformType.IOS;
		else if (Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.OSXEditor)
			platform = PlatformType.MAC;
#if NETFX_CORE
		else if (Application.platform==RuntimePlatform.MetroPlayerX86 || Application.platform==RuntimePlatform.MetroPlayerX64)
			platform = PlatformType.WINDOWS_STORE;
#endif
		//Debug.Log("Platform Type = "+platform);

		return platform;
	}

	void Update()
	{
		//call start only on Update as it is a blocking-call and may take time to finish
		//You might even consider running it on a seperate background thread!
		if (!generatorInitialized)
		{
			Debug.LogError("Extreme Motion Generator was not initialized.");
		}
		else if (!startAlreadyCalled)
		{
			try
			{
				Debug.Log("GeneratorSingleton.Instance.Start() was called.");
				//start can fail on missing/busy-camera. You should detect it and allow the user to fix the problem
	   			GeneratorSingleton.Instance.Start();    
				cameraErrorDetected = false;
				Debug.Log("GeneratorSingleton.Instance.Start() completed");
				messageString = "Camera found. Engine started.";
			}	
			catch (CameraAbsentException ex)  //can fail due to camera-busy / not-connected
			{
				cameraErrorDetected = true;
				messageString = "Camera failure:  Camera not found. Connect it and then click Reset Button";
				Debug.LogError(messageString + " " + ex.Message);
			} 
			catch (CameraBusyException ex)
			{
				cameraErrorDetected = true;
				messageString = "Camera failure:  Camera busy.  Free it and then click Reset Button";
				Debug.LogError(messageString + " " + ex.Message);
			}
			catch (Exception ex)
			{
				cameraErrorDetected = true;
				messageString = "Camera failure: Generic exception: \n";
				Debug.LogError(messageString + " " + ex.ToString());
			}
			startAlreadyCalled=true;
		}
			
		if (Input.GetKeyDown(KeyCode.Escape))
	    {
			Debug.Log("Escape key was called. Quitting");
	        Application.Quit();
	    }
        myGuiText.text = messageString;
	}
	
	void OnGUI()
	{
		
		if(GUI.Button(new Rect(Screen.width - 150,Screen.height/2 - 50,100,100),"Reset"))
		{
			if (!cameraErrorDetected)
			{
				Debug.Log("Call for Reset");
				GeneratorSingleton.Instance.Reset();
			} 
			else 
			{
				Debug.Log("Start failed due to camera problem. will try to Start() again once");
				startAlreadyCalled = false;
			}
		}
		
		
	}
		
	
	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");

        MyColorImageFrameReadyEventHandler = null;
        MyDataFrameReady = null;

		//stop prcoessing and release the camera. 
		GeneratorSingleton.Instance.Stop();
		GeneratorSingleton.Instance.Shutdown();	
		
		// Due to Unity3d issue, a standalone application which uses the engine can get non-responsive, instead of quiting.
		// to solve this, we kill the prcoess instead.
		#if UNITY_STANDALONE_WIN 
		if(! Application.isEditor && Application.platform == RuntimePlatform.WindowsPlayer) // only relevant for windows
			System.Diagnostics.Process.GetCurrentProcess().Kill();
		#endif
	}
	void OnApplicationPause(bool paused)
	{
		if(paused){
			Debug.Log("Application in background");
			GeneratorSingleton.Instance.Stop();
		}
		else{
			Debug.Log("Application in foreground");
			GeneratorSingleton.Instance.Start();
		}
	}

    private void MyAllFramesReadyEventHandler(object sender, AllFramesReadyEventArgs e)
    {
		try
		{
	        using (var allFrames = e.OpenFrame() as AllFramesFrame)
	        {
	            if (allFrames != null)
	            {
	                foreach (var evtArgs in allFrames.FramesReadyEventArgs)
	                {
	                    var colorImageFrameReady = evtArgs as ColorImageFrameReadyEventArgs;
	                    if ((MyColorImageFrameReadyEventHandler != null) && (null != colorImageFrameReady))
	                    {
	                        Xtr3dGeneratorStateManager.MyColorImageFrameReadyEventHandler(sender, colorImageFrameReady);
	                        continue;
	                    }
	                    var dataFrameReady = evtArgs as DataFrameReadyEventArgs;
	                    if ((MyDataFrameReady != null) && (null != dataFrameReady))
	                    {
	                        Xtr3dGeneratorStateManager.MyDataFrameReady(sender, dataFrameReady);
	                        continue;
	                    }
						var gesturesFrameReady = evtArgs as GesturesFrameReadyEventArgs;
	                    if ((MyGesturesFrameReady != null) && (null != gesturesFrameReady))
	                    {
	                        Xtr3dGeneratorStateManager.MyGesturesFrameReady(sender, gesturesFrameReady);
	                        continue;
	                    }
	                }
	            }
	        }
		}
		catch (System.Exception ex)
		{
			Debug.LogError("Error in MyAllFramesReadyEventHandler: \n" + ex.ToString());
		}
    }
}
