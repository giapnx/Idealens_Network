using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TCPClient))]
public class TCPClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TCPClient client = (TCPClient)target;
        if (GUILayout.Button("Run"))
        {
            client.ConnectToTcpServer();
        }
    }
}
#endif