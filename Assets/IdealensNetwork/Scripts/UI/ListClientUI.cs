using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListClientUI : SingletonMonoBehaviour<ListClientUI> {

	public Transform ContentTrans;
	public GameObject ItemClientPref; // prefab

	public Sprite playImage;
	public Sprite pauseImage;
	public Sprite stopImage;

	public Color connectingColor;
	public Color pauseColor;
	public Color disconnectColor;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Alpha9)) {
			CreateNewClient (1);
		}
	}

	public ItemClientUI CreateNewClient(int _index)
	{
		GameObject go = Instantiate (ItemClientPref);
		go.transform.parent = ContentTrans;
		go.transform.localPosition = Vector3.zero;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localScale = new Vector3 (1, 1, 1);
		go.GetComponent <ItemClientUI>().InitItemClient (_index);
		return go.GetComponent <ItemClientUI>();
	}
		
}
