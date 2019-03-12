using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClientReceiveUI : MonoBehaviour 
{
	public Image clientImage;
	public Text clientNumber;
	public Image processReceive;
	public Text processText;

	public void SetClientImage(Color _clientColor)
	{
		clientImage.color = _clientColor;
	}

	public void SetClientIndex(int index)
	{
		clientNumber.text = index.ToString ();
	}

	public void CreateClientReceiveUI(int _number)
	{
		
	}

	public void SetProcessReceive(float _percent)
	{
		processReceive.fillAmount = _percent;
		if (_percent < 1)
		{
			processText.text = _percent * 100 + "%";
		}
		else
		{
			processText.text = "Done";
		}
	}

}
