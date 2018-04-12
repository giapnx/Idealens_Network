using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class IPAddressInput : SingletonMonoBehaviour<IPAddressInput> {

	public string ipAddress;
	public Toggle[] toggles = new Toggle[4];
	public int[] octets = new int[4];

	public Slider SliderIP;
	public ToggleGroup toggleGroup;
	int _currentToggleindex = 3;

	// Use this for initialization
	void Start () 
	{
		SetDefault ();
		SetTextIP ();

		OnToggleChanged (_currentToggleindex);
		SliderIP.onValueChanged.AddListener ((value) => SetCurrentOctet ());
	}

	void SetDefault()
	{
		octets [0] = 172;
		octets [1] = 20;
		octets [2] = 10;
		octets [3] = 6;

		for (int i = 0; i < toggles.Count (); i++) {
			toggles [i].GetComponent <ItemToggle> ().toggleId = i;
		}
	}

	public void OnToggleChanged(int _toggleId)
	{
		_currentToggleindex = _toggleId;
		// update value of Slider
		SliderIP.value = octets[_currentToggleindex];
	}

	public void MinusValueOfSlider()
	{
		SliderIP.value--;
	}

	public void PlusValueOfSlider()
	{
		SliderIP.value++;
	}

	public void SetCurrentOctet()
	{
		octets [_currentToggleindex] = (int)SliderIP.value;
		SetTextIP ();
	}

	public void SetTextIP()
	{
		for (int i = 0; i < toggles.Count (); i++) 
		{
			toggles[i].GetComponent <ItemToggle>().toggleText.text = octets[i].ToString ();
		}
		ipAddress = octets [0] + "." + octets [1] + "." + octets [2] + "." + octets [3];
	}

}
