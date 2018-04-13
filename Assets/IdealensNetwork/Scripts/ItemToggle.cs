using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToggle : MonoBehaviour 
{
	public int toggleId;
	public Text toggleText;
	Toggle toggle;

	void Start()
	{
		toggle = GetComponent <Toggle> ();
	}

	public void ToggleListener()
	{
		if (toggle.isOn) 
		{
			IPAddressInput.Instance.OnToggleChanged (toggleId);
		}
	}
}
