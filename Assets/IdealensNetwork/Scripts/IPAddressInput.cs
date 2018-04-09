using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPAddressInput : MonoBehaviour {

	InputField inputField;
	[Range(0, 255)] public int fourthOctet; // 0 -> 255
	[Range(0, 255)] public int thirdOctet = 1; // 0 -> 255
	public Slider fourthSlider;
	public Slider thirdSlider;

	// Use this for initialization
	void Start () 
	{
		inputField = GetComponent <InputField>();
		fourthSlider.value = fourthOctet;
		thirdSlider.value = thirdOctet;
		SetIPAddressInputText (thirdOctet, fourthOctet);
	}

	public void SetIPAddressInput()
	{
		thirdOctet = (int)thirdSlider.value;
		fourthOctet = (int)fourthSlider.value;
		SetIPAddressInputText (thirdOctet, fourthOctet);
	}

	void SetIPAddressInputText(int _thirdOctet, int _fourthOctet)
	{
		string ip = "192.168." + _thirdOctet + "." + _fourthOctet;
		inputField.text = ip;
	}

}
