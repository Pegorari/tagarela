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
class TagarelaEditorPopupNewFile : TagarelaEditor
{
    public TagarelaEditor parent;
    public Tagarela tagarela;

    bool isAudioBased;
    float newTime = 1;
    int newAudioIndex = 0;
    int oldAudioIndex = -1;
    //public List<string> audioList;
    string newFilename = "";
    GUIStyle windowStyle = new GUIStyle();
    string[] toolbarContent = new string[] { "Custom Timer", "Audio Sync" };
    int toolbar = 0;

    /*
    public void OnEnable ()
	{
        //parent = TagarelaEditor.GetWindow(typeof(TagarelaEditor)) as TagarelaEditor;
        title = "New Animation";
        minSize = maxSize = new Vector2(400, 270);
        windowStyle = new GUIStyle();
        windowStyle.margin = new RectOffset(10, 10, 10, 10);
    }
    */

    void OnGUI() {

        GUILayout.BeginVertical(windowStyle);
        {

            EditorGUILayout.LabelField("Choose your type of animation: ");
            EditorGUILayout.Space();

            toolbar = GUILayout.Toolbar(toolbar, toolbarContent);

            if (toolbar == 0)
            {
                EditorGUILayout.LabelField("*Unity timer will control the animation", new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(30) });

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(GUIStyle.none);
                EditorGUILayout.LabelField("Animation Length: ", new GUILayoutOption[] { GUILayout.Width(110), GUILayout.Height(20) });
                newTime = EditorGUILayout.FloatField(newTime, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(20) });
                EditorGUILayout.LabelField("seconds", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(20) });
                GUILayout.EndHorizontal();
                isAudioBased = false;
            }
            else
            {
                EditorGUILayout.LabelField("*audio current play time will control the animation", new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(30) });

                EditorGUILayout.Space();

                if (audioList == null || audioList.Count == 0) {
                    EditorGUILayout.LabelField("PLEASE, ASSIGN AN AUDIO FILE IN THE TAGARELA INSPECTOR", new GUILayoutOption[] { GUILayout.Width(400), GUILayout.Height(30) });
                    return;
                };

                EditorGUILayout.BeginHorizontal(GUIStyle.none);
                newAudioIndex = EditorGUILayout.Popup(newAudioIndex, audioList.ToArray(), new GUILayoutOption[] { GUILayout.Width(200), GUILayout.Height(22) });
                //if modified audio file, changes the new name suggestion
                if (oldAudioIndex != newAudioIndex && audioList != null) {
                    newFilename = audioList[newAudioIndex] + "_" + (tagarela.animationFiles.Count);
                    oldAudioIndex = newAudioIndex;
                    newTime = tagarela.audioFiles[newAudioIndex].length;
                }
                EditorGUILayout.LabelField(newTime.ToString("0.00") + " seconds", new GUILayoutOption[] { GUILayout.Width(180), GUILayout.Height(20) });
                GUILayout.EndHorizontal();
                isAudioBased = true;
            }
            EditorGUILayout.Separator();
            
            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animation name: ", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(15) });
            newFilename = EditorGUILayout.TextArea(newFilename, new GUILayoutOption[] { GUILayout.Width(260), GUILayout.Height(20) });
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(40f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(20) }))
            {
                Close();
            }
            
            GUILayout.Space(10f);

            if (newFilename == "") GUI.enabled = false;
            if (GUILayout.Button("Save", new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(20) }))
            {

                //check if exist the same filename
                bool filename_is_unique = true;
                for (int i = 0; i < tagarela.animationFiles.Count; i++)
                {
                    if (tagarela.animationFiles[i].name == newFilename){
                        filename_is_unique = false;
                    }
                }

                if (!filename_is_unique)
                {
                    ShowNotification(new GUIContent("Error: File name already exists!"));

                } else {
                    if (newTime <= 0) newTime = 1;
                    parent.CreateNewAnimation(isAudioBased, newAudioIndex, newFilename, newTime);
                    Close();
                }

            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

        } //end vertical
        GUILayout.EndVertical();

    }

    void OnInspectorUpdate() {
        Repaint();
    }
}