//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Resources;
class TagarelaEditorPopupTimeLength : TagarelaEditor
{
    public TagarelaEditor parent;
    public float currentLength = 0f;
    public float newLength = 0f;
    /*
    public void OnEnable ()
	{
        title = "Time Settings";
        newLength = currentLength;
        minSize = maxSize = new Vector2(250, 100);
    }
    */
    void OnGUI() {

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal(GUIStyle.none);
            EditorGUILayout.LabelField("Animation Length: ", new GUILayoutOption[] { GUILayout.Width(110), GUILayout.Height(20) });
            newLength =  EditorGUILayout.FloatField(newLength, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(20) });

            EditorGUILayout.LabelField("seconds", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(20) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            if (newLength == currentLength || newLength <= 0) GUI.enabled = false;
            if (GUILayout.Button("Ok")) {
                Change();
            }
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

    }

    void Change() {
        if (newLength < 0) newLength = 0.1f;
        
        if (newLength < currentLength)
        {
            string message = "Will be necessary to remove some keyframes from your timeline.\n Do you confirm?";
            if (EditorUtility.DisplayDialog("Attention", message, "Confirm", "Cancel"))
            {
                parent.ChangeAnimLength(newLength);
                Close();
            }
        }
        else
        {

            string message = "Your animation length will be changed! \n Do you confirm?";
            if (EditorUtility.DisplayDialog("Confirm", message, "Ok", "Cancel"))
            {
                parent.ChangeAnimLength(newLength);
                Close();
            }
        }
    }

    void OnInspectorUpdate() {
        Repaint();
    }


}