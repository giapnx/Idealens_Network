using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoInactive : MonoBehaviour 
{
	public float waitTime;
	float _timeCount;
	// Use this for initialization
	void OnEnable () 
	{
		_timeCount = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		_timeCount += Time.deltaTime;
		if (_timeCount >= waitTime) 
		{
			gameObject.SetActive (false);
		}
	}
}
