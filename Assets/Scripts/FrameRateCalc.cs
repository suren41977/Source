using System;
using UnityEngine;
using System.Collections.Generic;

public class FrameRateCalc
{
	private double m_updateInterval;
	private int m_framesCounter;
	private double m_frameRate;
	private bool m_isChanged;
	private float m_numOfFrames;
	private float m_totalStatsDurationMillis = 0;
	
	private DateTime m_lastFrameTime = DateTime.Now;
	private TimeSpan m_deltaT = TimeSpan.Zero;
	private float m_lastAverage = 0;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FrameRateCalc"/> class.
	/// </summary>
	/// <param name='updateInterval'>
	/// Update interval, in milliseconds, fps will be updated after x given miliseconds
	/// </param>
	public FrameRateCalc()
	{
		Initialize();
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="FrameRateCalc"/> class.
	/// Use this constructor for calculating running avarage fps
	/// </summary>
	/// <param name='numFrames'>
	/// Number frames to calculate avg.
	/// </param>
	/// <param name='updateInterval'>
	/// Update interval, in milliseconds, fps will be updated after x given miliseconds
	/// </param>
	public FrameRateCalc(int numFrames)
	{
		Initialize();
		m_numOfFrames = numFrames;
	}
	
	/// <summary>
	/// Initialize this instance with default params.
	/// </summary>
	private void Initialize()
	{
		m_updateInterval = 1000;
		m_framesCounter  = 0; // Frames drawn over the interval
		m_lastFrameTime = DateTime.Now;		
		m_isChanged = false;	
	}
	/// <summary>
	/// Calculates the current FPS (every one second)
	/// </summary>
	/// <returns>
	/// The current FPS.
	/// </returns>
	public double CalculateCurrentFPS()
	{
		DateTime now = DateTime.Now;
		double timeSpan = (now - m_lastFrameTime).TotalMilliseconds;
		++m_framesCounter;
		if (timeSpan > m_updateInterval)
		{
			m_frameRate = (m_framesCounter*m_updateInterval)/timeSpan ;
			m_framesCounter = 0;
			m_lastFrameTime = now;
			m_isChanged = true;
			
    	} else 
			m_isChanged =false;
		
		return m_frameRate;
	}
	/// <summary>
	/// Updates the avg fps.
	/// Call this each frame
	/// </summary>
	public void UpdateAvgFps()
	{
		m_deltaT = DateTime.Now - m_lastFrameTime;
		m_lastFrameTime = DateTime.Now;
		m_totalStatsDurationMillis += (float)m_deltaT.TotalMilliseconds;
		m_framesCounter++;
	}
	/// <summary>
	/// Gets the avg fps.
	/// </summary>
	/// <returns>
	/// The avg fps of the last x given frames in batches of size x (non-running)
	/// </returns>
	public float GetAvgFps()
	{
		if(m_framesCounter >= m_numOfFrames)
		{
			m_lastAverage = 1000 / (m_totalStatsDurationMillis / m_numOfFrames);
			m_totalStatsDurationMillis = 0;			
			m_framesCounter = 0;
		}
		
		return m_lastAverage;
	}
	
	public bool IsChanged()
	{
		return m_isChanged;
	}
}

