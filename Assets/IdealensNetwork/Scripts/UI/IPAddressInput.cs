using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;

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
		LoadLastIpInput ();
		SetDefault ();
		SetTextIP ();

		OnToggleChanged (_currentToggleindex);
		SliderIP.onValueChanged.AddListener ((value) => SetCurrentOctet ());
	}

	void SetDefault()
	{
//		octets [0] = 172;
//		octets [1] = 20;
//		octets [2] = 10;
//		octets [3] = 6;

		for (int i = 0; i < toggles.Count (); i++) {
			toggles [i].GetComponent <ItemToggle> ().toggleId = i;
		}
	}

	void LoadLastIpInput()
	{
		string path = Application.persistentDataPath + "/" + DataConfig.IP_SAVE_PATH;
		if (File.Exists (path))
		{
			StreamReader reader = new StreamReader (path);
			string data = reader.ReadLine ();
			print (data);
			string[] _ip = data.Trim ().Split ("."[0]);
			octets [0] = int.Parse (_ip[0]);
			octets [1] = int.Parse (_ip[1]);
			octets [2] = int.Parse (_ip[2]);
			octets [3] = int.Parse (_ip[3]);
		}
		else
		{
			octets [0] = 192;
			octets [1] = 168;
			octets [2] = 1;
			octets [3] = 87;
		}

	}

	public void SaveLastIpInput()
	{
		string ip = octets [0] + "." + octets [1] + "." + octets [2] + "." + octets [3];

		string path = Application.persistentDataPath + "/" + DataConfig.IP_SAVE_PATH;
		StreamWriter writer = new StreamWriter (path, false);
		writer.WriteLine (ip);
		writer.Close ();
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
