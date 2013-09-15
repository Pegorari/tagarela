/*============================================================
TAGARELA LIP SYNC SYSTEM
Copyright (c) 2013 Rodrigo Pegorari

Tagarela Lip Sync is a vertex morphing tool to create facial animations for unity easily.
Once you have facial expressions exported into 3d meshes you only need to associate an audio file and add some keyframes and blend settings.
You can have multiple animations files per mesh. A simple Play("youranimation") will do the hard work.
If you use an audio file, in order to ensure a nice synchronism, animations will be run according to the audio current play time, to avoid latency problems.
Tagarela Lip Sync is currently in development, but you can already try it for your own risk. Feedbacks are welcome.
I hope you enjoy!
Rodrigo 
*/

/*
TERMS OF USE - EASING EQUATIONS
Open source under the BSD License.
Copyright (c)2001 Robert Penner
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Resources;

class TagarelaEditor : EditorWindow
{
    //UI_Ruler ruler = new UI_Ruler(new Rect(20, 100, 400, 1), 0f);
    Tagarela tagarela;

    enum ScreenDialog
    {
        InitialScreen, //Initial Screen - file selection
        InvalidObject, //Invalid object
        AddComponent, //Valid Object. Can add component
        NewFile, //Create a new file
        Timeline //File openned, show de timeline editor
    }
    ScreenDialog dialog = ScreenDialog.InitialScreen;

    TagarelaTimelineUI guiTimeline;
    TagarelaTimelineSegmentUI guiTimelineSegment;

    int lastKeyframeSelected = -1;

    float timeNormalized = 0f;

    public List<string> audioList;
    bool disableGuiControls = false;
    bool updateWindow = true;

    bool updateTimeline = false;
    bool updateMorph = false;

    float updateMorphValue = 0f;

    string log_msg = "";
    string fileName;

    private Texture2D logoEditor;
    private Texture2D bgEditor;
    private Texture2D bgTimeline;
    private Texture2D icoPlay, icoPlaySegment, icoClock, icoStop, icoAdd, icoRemove;
    private Texture2D audioPreviewSpectrum;

    Mesh originalObject;
    GameObject lastSelectionGameObjectEditing; //object that was the last selection used in tagarela for edition
    AudioClip SelectedAudioclip = new AudioClip();

    private TagarelaFileStructure settings;
    private GUIStyle styleLabelGrid, styleButtonGrid, styleGrid, styleBgSlider, styleBigButtons, styleBtTimeline, styleBgTimeline, styleFileTitle, styleScrollview, styleScrollviewTimeline, styleHeader;
    enum PlayMode
    {
        currentTime, segment, all, stopped
    }

    PlayMode playMode = PlayMode.stopped;

    public void OnEnable()
    {
        title = "Tagarela Editor";
    }

    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Tagarela/System/Editor/Images");

        if (importer.assetPath.Contains(".png"))
            importer.textureType = TextureImporterType.GUI;
    }

    public void OnDestroy()
    {
        if (dialog == ScreenDialog.Timeline)
        {
            if (EditorUtility.DisplayDialog("Close animation", "Do you want to save?        ", "Yes", "No"))
            {
                string audioName = SelectedAudioclip != null ? SelectedAudioclip.name : "";
                TagarelaFileManager.Save(fileName, tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, audioName, tagarela.settings.animationTime);
            }
        }
        SelectedAudioclip = null;
        if (tagarela != null) tagarela.Clean();
        CleanVars();

    }

    public void ChangeAnimLength(float newLength)
    {
        //Update the file settings

        guiTimeline.totalValue = newLength;

        List<TagarelaTimelineUI.TLkeyframe> keySet = new List<TagarelaTimelineUI.TLkeyframe>(guiTimeline.keyframeSet);

        keySet.RemoveAll(item => item.value > newLength);


        guiTimeline.keyframeSet = keySet;

        settings = TagarelaFileManager.UpdateSettings(tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, "", guiTimeline.totalValue);

        //guiTimeline.UpdateSelection();
        guiTimelineSegment.totalValue = newLength;
        guiTimelineSegment.KeyframeSet[0].Value = 0;
        guiTimelineSegment.KeyframeSet[1].Value = newLength;
        guiTimelineSegment.UpdateSelection();

        settings.animationTime = newLength;

        Repaint();
        tagarela.settings = settings;
        tagarela.BuildTimeline();
        updateTimeline = true;

        Repaint();
        Update();
        playMode = PlayMode.currentTime;

    }

    public void CreateNewAnimation(bool isAudioBased, int audioFileIndex, string newAnimFileName, float newTime)
    {

        if (newAnimFileName != "")
        {

            string newAudioName = "";
            if (isAudioBased) newAudioName = tagarela.audioFiles[audioFileIndex].name;

            TagarelaMorphTarget _MorphTarget = new TagarelaMorphTarget();
            _MorphTarget.Populate(tagarela.morphTargets);

            guiTimeline = new TagarelaTimelineUI(newTime);
            guiTimeline.AddKeyframe(0f, _MorphTarget);

            guiTimelineSegment = new TagarelaTimelineSegmentUI(newTime);

            if (TagarelaFileManager.Save(newAnimFileName, tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, newAudioName, newTime))
            {
                TextAsset newFile = AssetDatabase.LoadMainAssetAtPath("Assets/Tagarela/System/Animations/" + newAnimFileName + ".xml") as TextAsset;
                AssetDatabase.Refresh();
                tagarela.Clean();
                tagarela.animationFiles.Add(newFile);

                CleanVars();
                dialog = ScreenDialog.InitialScreen;
                Repaint();
                Update();

                LoadAnimationFile(tagarela.animationFiles[tagarela.animationFiles.Count - 1]);
            }

        }


    }

    IEnumerator teste()
    {
        yield return new WaitForSeconds(3);

        LoadAnimationFile(tagarela.animationFiles[tagarela.animationFiles.Count - 1]);
        dialog = ScreenDialog.Timeline;
        Repaint();
    }

    void Awake()
    {

        logoEditor = TagarelaFileManager.LoadImageResource("logoEditor.png");
        logoEditor.hideFlags = HideFlags.DontSave;

        bgEditor = TagarelaFileManager.LoadImageResource("bgEditor.png");
        bgEditor.hideFlags = HideFlags.DontSave;

        bgTimeline = TagarelaFileManager.LoadImageResource("bgTimeline.png");
        bgTimeline.hideFlags = HideFlags.DontSave;

        icoPlay = TagarelaFileManager.LoadImageResource("icoPlay.png");
        icoPlay.hideFlags = HideFlags.DontSave;

        icoClock = TagarelaFileManager.LoadImageResource("icoClock.png");
        icoClock.hideFlags = HideFlags.DontSave;

        icoPlaySegment = TagarelaFileManager.LoadImageResource("icoPlaySegment.png");
        icoPlaySegment.hideFlags = HideFlags.DontSave;

        icoStop = TagarelaFileManager.LoadImageResource("icoStop.png");
        icoStop.hideFlags = HideFlags.DontSave;

        icoAdd = TagarelaFileManager.LoadImageResource("icoAdd.png");
        icoAdd.hideFlags = HideFlags.DontSave;

        icoRemove = TagarelaFileManager.LoadImageResource("icoRemove.png");
        icoRemove.hideFlags = HideFlags.DontSave;

        styleHeader = new GUIStyle();
        styleHeader.padding = new RectOffset(0, 0, 73, 0);

        styleScrollview = new GUIStyle();
        styleScrollview.padding = new RectOffset(14, 14, 0, 10);

        styleScrollviewTimeline = new GUIStyle();
        styleScrollviewTimeline.padding = new RectOffset(0, 0, 0, 0);

        styleFileTitle = new GUIStyle();
        styleFileTitle.fontStyle = FontStyle.Bold;
        styleFileTitle.normal.textColor = Color.white;
        styleFileTitle.padding.left = 14;
        styleFileTitle.fontSize = 14;

        styleBgTimeline = new GUIStyle();
        styleBgTimeline.normal.background = bgTimeline;
        styleBgTimeline.padding = new RectOffset(10, 10, 0, 11);

        styleBtTimeline = new GUIStyle(GUI.skin.button);
        styleBtTimeline.padding = new RectOffset(0, 0, 0, 0);
        styleBtTimeline.margin.left = 0;
        styleBtTimeline.margin.right = 1;

        styleBigButtons = new GUIStyle(GUI.skin.button);
        styleBigButtons.padding.left = 0;
        styleBigButtons.margin.top = 0;

        styleLabelGrid = new GUIStyle();
        styleLabelGrid.normal.background = EditorStyles.toolbarTextField.normal.background;
        styleLabelGrid.normal.textColor = EditorStyles.toolbarTextField.normal.textColor;
        styleLabelGrid.fontStyle = EditorStyles.toolbarTextField.fontStyle;
        styleLabelGrid.margin = new RectOffset(0, 0, 0, 0);
        styleLabelGrid.border = new RectOffset(2, 2, 3, 3);
        styleLabelGrid.padding = new RectOffset(2, 2, 1, 2);

        styleBgSlider = new GUIStyle();
        styleBgSlider.normal.background = EditorStyles.toolbarTextField.normal.background;
        styleBgSlider.normal.textColor = Color.yellow;
        styleBgSlider.border = new RectOffset(3, 3, 3, 3);
        styleBgSlider.margin = new RectOffset(13, 13, 3, 3);
        styleBgSlider.padding = new RectOffset(10, 10, 4, 4);

        styleButtonGrid = new GUIStyle(GUI.skin.button);
        styleButtonGrid.margin.top = 0;
        styleButtonGrid.padding.top = 2;
        styleButtonGrid.padding.left = 4;
        styleButtonGrid.margin.right = 0;
        styleButtonGrid.margin.bottom = 2;

        styleGrid = new GUIStyle(GUI.skin.box);
        styleGrid.margin = new RectOffset(0, 0, 10, 0);
        styleGrid.padding = new RectOffset(10, 10, 10, 10);

        SelectObject();
    }

    public void CleanVars()
    {
        RestoreOriginalMesh();

        if (tagarela.mainObject.GetComponent<MeshFilter>())
        {
            tagarela.mainObject.GetComponent<MeshFilter>().sharedMesh.vertices = tagarela.neutralMesh.vertices;
        }
        else if (tagarela.mainObject.GetComponent<SkinnedMeshRenderer>())
        {
            tagarela.mainObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertices = tagarela.neutralMesh.vertices;
        }

        updateMorph = false;
        tagarela.audio.clip = null;
        lastSelectionGameObjectEditing = null;
        log_msg = "";
        fileName = "";
        settings = null;
    }

    void OnSelectionChange()
    {

        SelectObject();
        Repaint();
    }

    void SelectObject()
    {

        bool hasTagarela = (Selection.objects.Length == 1 && Selection.activeGameObject.GetComponent<Tagarela>());

        if (hasTagarela)
        {
            if (settings == null || guiTimeline == null || tagarela == null || tagarela.settings == null)
            {
                BackupOriginalMesh();
                tagarela.Clean();
                dialog = ScreenDialog.InitialScreen;
            }
            else if (lastSelectionGameObjectEditing == Selection.activeGameObject)
            {
                if (settings != null && tagarela.settings == settings && guiTimeline != null)
                {
                    dialog = ScreenDialog.Timeline;
                }
                else
                {
                    dialog = ScreenDialog.InitialScreen;
                }
            }
        }
        else if (Selection.objects.Length != 0)
        {
            log_msg = "Invalid Object: There is no Tagarela Component";
            dialog = ScreenDialog.InvalidObject;
            playMode = PlayMode.stopped;

            RestoreOriginalMesh();

        }
        else if (Selection.objects.Length == 0)
        {

            if (lastSelectionGameObjectEditing != null && settings != null && tagarela.settings == settings && guiTimeline != null)
            {
                dialog = ScreenDialog.Timeline;
            }
            else
            {
                Close();
            }



        }

        /*
        if (Selection.activeGameObject == null && dialog != ScreenDialog.Timeline || Selection.objects.Length > 1)
        {
            log_msg = "Select a valid object!";
            //valid_object = false;
            //exist_component = false;
            Debug.Log("opa " + dialog);
            if (dialog == ScreenDialog.Timeline)
            {
                if (EditorUtility.DisplayDialog("Close animation", "Do you want to save?        ", "Yes", "No"))
                {
                    string audioName = SelectedAudioclip != null ? SelectedAudioclip.name : "";
                    TagarelaFileManager.Save(arquivo_aberto, tagarela.morphTargets, guiTimeline.KeyframeSet, tagarela.neutralMesh.vertexCount, audioName, tagarela.settings.animationTime);
                    CleanVars();
                }
                RestoreOriginalMesh();
            }
            else
            {
                SelectedAudioclip = null;
                if (tagarela != null) tagarela.Clean();
                RestoreOriginalMesh();
            }
            dialog = ScreenDialog.InvalidObject;
        }
        else if (Selection.objects.Length == 1 && Selection.activeGameObject.GetComponent<MeshFilter>() && !Selection.activeGameObject.GetComponent<Tagarela>())
        {

            dialog = ScreenDialog.InvalidObject;
        }
        else if (Selection.objects.Length == 1 && Selection.activeGameObject.GetComponent<Tagarela>())
        {
            //Selection.activeGameObject.hideFlags = HideFlags.HideInHierarchy;
            RestoreOriginalMesh();
            BackupOriginalMesh();

            if (tagarela != null)
            {
                dialog = ScreenDialog.InitialScreen;
            }
            else
            {
                tagarela.Clean();
                dialog = ScreenDialog.InitialScreen;
            }
        }
        */
    }


    private Vector2 scrollViewVector = Vector2.zero;
    void OnGUI()
    {

        updateWindow = false;

        GUI.color = Color.white;

        GUI.DrawTexture(new Rect(0, 0, Screen.width, bgEditor.height), bgEditor);
        GUI.DrawTexture(new Rect(13, 14, logoEditor.width, logoEditor.height), logoEditor);

        GUILayout.BeginVertical(styleHeader);
        {
            GUI.color = Color.white;
            switch (dialog)
            {

                case ScreenDialog.InvalidObject:
                    GUILayout.Label(log_msg, EditorStyles.boldLabel);
                    break;

                case ScreenDialog.AddComponent:
                    GUILayout.Label("", EditorStyles.boldLabel);
                    GUILayout.Space(15f);

                    if (GUILayout.Button("Add LipSync Component!"))
                    {
                        //Debug.Log("UnityLipSync -> LipSync Component created on " + LastSelected.name);
                        Selection.activeGameObject.AddComponent(typeof(Tagarela));
                    }
                    break;

                case ScreenDialog.InitialScreen:

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Select Animation", styleFileTitle, GUILayout.Height(22));

                    if (GUILayout.Button("Create New Animation", styleBigButtons, new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(18) }))
                    {
                        TagarelaEditorPopupNewFile popupNewFile = CreateInstance<TagarelaEditorPopupNewFile>();
                        popupNewFile.parent = this;
                        RefreshAudiolist();
                        popupNewFile.audioList = audioList;
                        popupNewFile.tagarela = tagarela;
                        popupNewFile.ShowAuxWindow();
                    }
                    GUILayout.Space(8f);
                    GUILayout.EndHorizontal();

                    scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, styleScrollview);
                    {

                        GUILayout.BeginVertical();
                        {

                            GUILayout.Space(15f);

                            if (tagarela.animationFiles.Count > 0)
                            {

                                GUILayout.BeginVertical(styleGrid);
                                {
                                    for (int i = tagarela.animationFiles.Count - 1; i >= 0 ; i--)
                                    {
                                        GUILayout.BeginHorizontal();
                                        {
                                            //CLICK TO OPEN FILE
                                            if (GUILayout.Button(tagarela.animationFiles[i].name.ToString(), styleButtonGrid, GUILayout.Height(20)))
                                            {
                                                LoadAnimationFile(tagarela.animationFiles[i]);
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                            GUI.color = Color.white;


                            GUILayout.FlexibleSpace();

                        } //end vertical
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();
                    break;

                case ScreenDialog.Timeline:

                    GUILayout.BeginHorizontal();
                    {

                        GUILayout.Label(fileName, styleFileTitle, GUILayout.Height(21));

                        if (GUILayout.Button("Save", styleBigButtons, GUILayout.Width(70)))
                        {
                            string audioName = SelectedAudioclip != null ? SelectedAudioclip.name : "";
                            TagarelaFileManager.Save(fileName, tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, audioName, tagarela.settings.animationTime);
                        }
                        if (GUILayout.Button("Close", styleBigButtons, GUILayout.Width(70)))
                        {
                            switch (EditorUtility.DisplayDialogComplex("Close", "Do you want to save?        ", "Yes", "No", "Cancel"))
                            {
                                case 0:
                                    updateTimeline = true;
                                    string audioName = SelectedAudioclip != null ? SelectedAudioclip.name : "";
                                    TagarelaFileManager.Save(fileName, tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, audioName, tagarela.settings.animationTime);

                                    if (tagarela != null) tagarela.Clean();
                                    CleanVars();

                                    dialog = ScreenDialog.InitialScreen;
                                    break;
                                case 1:
                                    if (tagarela != null) tagarela.Clean();
                                    CleanVars();
                                    dialog = ScreenDialog.InitialScreen;
                                    break;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    updateMorph = false;

                    //######################################################################################

                    GUILayout.BeginVertical(styleBgTimeline, GUILayout.Height(bgTimeline.height));
                    {

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        {
                            if (SelectedAudioclip != null)
                            {
                                GUILayout.Label(SelectedAudioclip.name);
                            }

                            GUILayout.FlexibleSpace();

                            GUILayout.Space(20);

                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            {
                                if (settings != null) GUILayout.Label(guiTimeline.selectedValue.ToString("0.000") + " / " + settings.animationTime.ToString("0.000"));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5);
                        }
                        GUILayout.EndHorizontal();


                        //scrollViewTimelineVector = GUILayout.BeginScrollView(scrollViewTimelineVector, styleScrollviewTimeline);
                        //{

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        //GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(1040f) });
                        {

                            GUILayout.Box("", new GUIStyle(), GUILayout.ExpandWidth(true));

                            Rect lastRect = GUILayoutUtility.GetLastRect();

                            if (lastRect.width > 1)
                            {
                                guiTimeline.Draw(new Rect(lastRect.x, 124, lastRect.width, 57));
                                if (audioPreviewSpectrum != null) GUI.DrawTexture(new Rect(lastRect.x, 125, lastRect.width, 40), audioPreviewSpectrum);
                                guiTimelineSegment.Draw(new Rect(lastRect.x + 2, 125, lastRect.width - 2, 130));
                            }

                            switch (playMode)
                            {
                                case PlayMode.all:
                                    GUI.color = new Color(130f / 255f, 170f / 255f, 30f / 255f, 0.3f);
                                    GUI.DrawTexture(new Rect(guiTimeline.timeLineRect.x + 1, guiTimeline.timeLineRect.y + 44, (guiTimeline.timeLineRect.width) * timeNormalized, 11), EditorGUIUtility.whiteTexture);
                                    break;

                                case PlayMode.segment:
                                    GUI.color = new Color(130f / 255f, 170f / 255f, 30f / 255f, 0.3f);
                                    GUI.DrawTexture(new Rect(guiTimelineSegment.KeyframeSet[0].IconRect.x + 7, guiTimeline.timeLineRect.y + 44, (guiTimeline.timeLineRect.width - 18) * timeNormalized, 11), EditorGUIUtility.whiteTexture);
                                    break;
                            }

                        }
                        GUILayout.EndHorizontal();

                        GUI.color = Color.white;

                        //}
                        //GUILayout.EndScrollView();

                        GUILayout.Space(67);

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        {

                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            {
                                if (guiTimeline.keyframeSet.Count == 1 || disableGuiControls) GUI.enabled = false;
                                if (GUILayout.Button(icoRemove, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) })) //btRemove
                                {
                                    guiTimeline.RemoveKeyframe(guiTimeline.selectedIndex);
                                }
                                GUI.enabled = true;
                                if (disableGuiControls) GUI.enabled = false;
                                if (GUILayout.Button(icoAdd, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) })) //btAdd
                                {
                                    TagarelaMorphTarget _morphTargetList = new TagarelaMorphTarget();
                                    _morphTargetList.Populate(tagarela.morphTargets);
                                    guiTimeline.AddKeyframe(0.0f, _morphTargetList);
                                    updateTimeline = true;
                                    updateMorph = true;
                                }
                            }
                            GUILayout.EndHorizontal();


                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Sliders to zero", styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(120), GUILayout.Height(20) })) //btAdd
                            {
                                TagarelaMorphTarget _MorphTarget = guiTimeline.morphSliders;
                                if (_MorphTarget != null)
                                {
                                    for (int i = 0; i < _MorphTarget.id.Count; i++)
                                    {
                                        _MorphTarget.sliderValue[i] = 0;
                                    }
                                }
                                updateTimeline = true;
                            }

                            GUILayout.FlexibleSpace();

                            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            {
                                GUI.enabled = !disableGuiControls;

                                if (GUILayout.Button(icoPlay, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) }))
                                {
                                    //tagarela.setTimer(0);
                                    playMode = PlayMode.all;
                                }

                                GUI.enabled = true;
                                if (!guiTimelineSegment.active || disableGuiControls) GUI.enabled = false;
                                if (GUILayout.Button(icoPlaySegment, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) }))
                                {
                                    tagarela.setTimer(guiTimelineSegment.KeyframeSet[0].Value);
                                    playMode = PlayMode.segment;
                                }

                                GUI.enabled = false;
                                if (!playMode.Equals(PlayMode.stopped)) GUI.enabled = true;
                                if (GUILayout.Button(icoStop, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) }))
                                {
                                    //tagarela.audio.loop = false;
                                    tagarela.setTimer(0f);
                                    //tagarela.audio.Stop();
                                    playMode = PlayMode.stopped;
                                    if (tagarela.audio.isPlaying) tagarela.audio.Stop();
                                }

                                //if don´t have any audio, is a time based animation
                                if (SelectedAudioclip == null)
                                {
                                    GUI.enabled = !disableGuiControls;
                                    GUILayout.Space(10);
                                    if (GUILayout.Button(icoClock, styleBtTimeline, new GUILayoutOption[] { GUILayout.Width(22), GUILayout.Height(20) }))
                                    {
                                        TagarelaEditorPopupTimeLength popupAnimLength = CreateInstance<TagarelaEditorPopupTimeLength>();
                                        popupAnimLength.parent = this;
                                        popupAnimLength.title = "Time Settings";
                                        popupAnimLength.currentLength = popupAnimLength.newLength = guiTimeline.totalValue;
                                        popupAnimLength.ShowAuxWindow();
                                    };

                                }

                                GUI.enabled = true;

                            }
                            GUILayout.EndHorizontal();

                        }
                        GUILayout.EndHorizontal();

                    }
                    GUILayout.EndVertical();

                    GUI.enabled = !disableGuiControls;
                    scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, styleScrollview);
                    {

                        if (tagarela.morphTargets.Count > 0 && guiTimeline.selectedIndex != -1 && guiTimeline.keyframeSet.Count > 0)
                        {

                            EditorGUILayout.BeginVertical();
                            {
                                //seleciona a lista de acordo com o index selecionado
                                TagarelaMorphTarget _MorphTarget = guiTimeline.morphSliders;
                                GUI.color = GUI.contentColor;
                                if (_MorphTarget != null)
                                {
                                    //GUILayout.Space(13f);
                                    GUILayout.Space(14); //space before slider list
                                    for (int i = 0; i < _MorphTarget.id.Count; i++)
                                    {
                                        EditorGUILayout.BeginHorizontal(styleBgSlider, GUILayout.ExpandWidth(true));
                                        {
                                            float temp_value = _MorphTarget.sliderValue[i];

                                            GUILayout.Label(_MorphTarget.id[i], GUILayout.Width(150));
                                            temp_value = GUILayout.HorizontalSlider(temp_value, 0, 100, GUILayout.ExpandWidth(true));
                                            if (_MorphTarget.sliderValue[i] != temp_value)
                                            {
                                                _MorphTarget.sliderValue[i] = temp_value;

                                                updateTimeline = true;
                                            }

                                        }
                                        EditorGUILayout.EndHorizontal();
                                    }
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }



                        if (guiTimeline.refresh) updateWindow = true;
                        if (guiTimeline.isDragging)
                        {

                            guiTimelineSegment.isDragging = false;
                            guiTimelineSegment.enabled = false;

                            if (updateMorphValue != guiTimeline.selectedValue && SelectedAudioclip != null)
                            {
                                playMode = PlayMode.currentTime;
                                updateMorphValue = guiTimeline.selectedValue;
                            }

                        }

                        if (guiTimeline.enabled && (guiTimeline.selectedIndex == -1 || lastKeyframeSelected != guiTimeline.selectedIndex))
                        {
                            lastKeyframeSelected = guiTimeline.selectedIndex;
                            updateMorphValue = guiTimeline.selectedValue;
                            updateTimeline = true;
                        }

                        //if a new keyframe was create or cloned, call a update om mesh list 
                        if (guiTimeline.isKeyframeSetChanged)
                        {
                            lastKeyframeSelected = -1;
                            guiTimeline.isKeyframeSetChanged = false;
                            updateTimeline = true;
                            updateMorph = true;
                        }

                        if (guiTimelineSegment.refresh) updateWindow = true;
                        if (guiTimelineSegment.isDragging)
                        {
                            guiTimeline.isDragging = false;
                            guiTimeline.enabled = false;
                            
                            if (updateMorphValue != guiTimelineSegment.SelectedValue)
                            {
                                playMode = PlayMode.currentTime;
                                updateMorphValue = guiTimelineSegment.SelectedValue;
                                updateMorph = true;
                            }

                        }

                        if (!guiTimelineSegment.isDragging && !guiTimeline.isDragging)
                        {
                            guiTimelineSegment.enabled = true;
                            guiTimeline.enabled = true;
                        }

                        if (playMode.Equals(PlayMode.all) || playMode.Equals(PlayMode.segment))
                        {
                            guiTimeline.enabled = false;
                        }

                    }
                    GUILayout.EndScrollView();
                    GUI.enabled = true;
                    break;
            }

        }
        GUILayout.EndVertical();

        if (updateWindow || tagarela != null && tagarela.isPlaying) Repaint();

    }

    public void LoadAnimationFile(TextAsset file)
    {
        //RefreshMeshlist();
        RefreshAudiolist();
        tagarela.Clean();
        CleanVars();

        settings = TagarelaFileManager.Load(file);
        lastSelectionGameObjectEditing = Selection.activeGameObject;
        fileName = file.name;
        guiTimeline = new TagarelaTimelineUI(settings.animationTime);
        audioPreviewSpectrum = null;
        SelectedAudioclip = null;
        tagarela.audio.clip = null;
        tagarela.isPlaying = false;
        playMode = PlayMode.stopped;


        if (settings.audioFile != null)
        {

            for (int i = 0; i < tagarela.audioFiles.Count; i++)
            {

                if (settings.audioFile == tagarela.audioFiles[i].name)
                {
                    SelectedAudioclip = tagarela.audioFiles[i];
                    tagarela.audio.clip = tagarela.audioFiles[i];
                    audioPreviewSpectrum = TagarelaAudioSpectrum.CreatePreview(SelectedAudioclip, 1024, 64, new Color(150f / 255f, 200f / 255f, 25f / 255f, 0.8f), TagarelaAudioSpectrum.PreviewType.both);
                    audioPreviewSpectrum.hideFlags = HideFlags.DontSave;
                }
            }
        }

        //  CARREGA O ARQUIVO XML
        //configurações principais

        if (settings.meshList.id != null)
        {
            guiTimeline.keyframeSet = new List<TagarelaTimelineUI.TLkeyframe>();

            for (int i = 0; i < settings.keyframes.values.Length; i++)
            {

                TagarelaMorphTarget _MorphTarget = new TagarelaMorphTarget();
                _MorphTarget.Populate(tagarela.morphTargets);

                for (int j = 0; j < settings.keyframes.sliderSettings[i].Length; j++)
                {
                    float[] sliders = settings.keyframes.sliderSettings[i];
                    if (j < tagarela.morphTargets.Count)
                    {
                        //tagarela.morphTargets[j].hideFlags = HideFlags.DontSave;
                        _MorphTarget.sliderValue[j] = sliders[j];
                    }
                }
                guiTimeline.AddKeyframe(settings.keyframes.values[i], _MorphTarget);
                
            }
        }
        else
        {
            guiTimeline.keyframeSet = new List<TagarelaTimelineUI.TLkeyframe>();
            TagarelaMorphTarget _MorphTarget = new TagarelaMorphTarget();
            _MorphTarget.Populate(tagarela.morphTargets);
            guiTimeline.AddKeyframe(0f, _MorphTarget);

        }

        //Update the file settings
        settings = TagarelaFileManager.UpdateSettings(tagarela.morphTargets, guiTimeline.keyframeSet, tagarela.neutralMesh.vertexCount, settings.audioFile, guiTimeline.totalValue);
        guiTimelineSegment = new TagarelaTimelineSegmentUI(settings.animationTime);

        tagarela.OpenFile(file);

        timeNormalized = 0f;
        playMode = PlayMode.stopped;
        lastKeyframeSelected = -1;

        lastKeyframeSelected = guiTimeline.selectedIndex;
        updateMorphValue = guiTimeline.selectedValue;

        updateTimeline = true;

        if (settings != null && Selection.objects.Length == 1)
        {
            dialog = ScreenDialog.Timeline;
        }

    }



    void Update()
    {

        if (settings != null)
        {

            if (!tagarela.isPlaying && playMode != PlayMode.stopped)
            {
                if (playMode != PlayMode.currentTime)
                {
                    if (SelectedAudioclip != null)
                    {
                        if (tagarela.audio.clip == null) tagarela.audio.clip = SelectedAudioclip;
                        tagarela.audio.Stop();
                        tagarela.audio.Play();
                    }
                    tagarela.StartTimer();
                }

                tagarela.isPlaying = true;
                updateTimeline = true;
            }


            switch (playMode)
            {
                case PlayMode.all:
                    
                    if (tagarela.getTimer() >= tagarela.settings.animationTime - 0.01f)
                    {
                        tagarela.setTimer(0);
                    }
                    
                    timeNormalized = tagarela.getTimer() / tagarela.settings.animationTime;
                    updateMorphValue = tagarela.getTimer();

                    disableGuiControls = true;
                    updateMorph = true;

                    if (SelectedAudioclip != null && !tagarela.audio.isPlaying)
                    {
                        tagarela.audio.Stop();
                        tagarela.audio.Play();
                    }
                    
                    break;

                case PlayMode.currentTime:
                    if (SelectedAudioclip != null) tagarela.audio.Play();
                    playMode = !guiTimeline.isDragging && !guiTimelineSegment.isDragging ? PlayMode.stopped : playMode;
                    if (tagarela.audio.clip != null && SelectedAudioclip != null)
                    {
                        tagarela.PreviewAudio(updateMorphValue);
                    }
                    else
                    {
                        tagarela.setTimer(updateMorphValue);
                    }
                    disableGuiControls = false;
                    break;

                case PlayMode.segment:

                    float valueA = guiTimelineSegment.KeyframeSet[0].Value;
                    float valueB = guiTimelineSegment.KeyframeSet[1].Value;
                    if (tagarela.getTimer() < valueA)
                    {
                        timeNormalized = 0f;
                        tagarela.setTimer(valueA);
                    }
                    else if (tagarela.getTimer() > valueB)
                    {
                        timeNormalized = 0f;
                        tagarela.setTimer(valueA);
                    }
                    else
                    {
                        timeNormalized = (tagarela.getTimer() - valueA) / tagarela.settings.animationTime;
                    }
                    updateMorphValue = tagarela.getTimer();
                    updateMorph = true;
                    disableGuiControls = true;
                    break;
            }

            if (updateTimeline) UpdateTimeline();
            if (updateMorph) UpdateMorph();

            if (tagarela.isPlaying)
            {
                if (playMode.Equals(PlayMode.stopped))
                {
                    tagarela.isPlaying = false;
                    tagarela.audio.Stop();
                    lastKeyframeSelected = -1;
                    timeNormalized = 0f;
                    updateMorphValue = 0f;
                    updateMorph = false;
                    disableGuiControls = false;
                }
            }
        }

    }

    public void UpdateTimeline()
    {

        settings.keyframes.sliderSettings = new List<float[]>();
        settings.keyframes.values = new float[guiTimeline.keyframeSet.Count];
        for (int i = 0; i < guiTimeline.keyframeSet.Count; i++)
        {
            settings.keyframes.sliderSettings.Add(guiTimeline.keyframeSet[i].morphSliders.sliderValue.ToArray());
            settings.keyframes.values[i] = guiTimeline.keyframeSet[i].value;
        }
        tagarela.settings = settings;
        tagarela.BuildTimeline();

        updateMorphValue = guiTimeline.selectedValue;
        updateTimeline = false;
        UpdateMorph();
    }

    public void UpdateMorph()
    {
        if (tagarela.mainObject.GetComponent<MeshFilter>())
        {
            MeshFilter filter = tagarela.mainObject.GetComponent<MeshFilter>();
            filter.sharedMesh.vertices = tagarela.neutralMesh.vertices;
            if (filter.sharedMesh != null)
            {
                filter.sharedMesh = tagarela.neutralMesh_temp;
            }
            filter.sharedMesh.name = "Tagarela";
            tagarela.basemesh = filter.sharedMesh as Mesh;

        }
        else if (tagarela.mainObject.GetComponent<SkinnedMeshRenderer>())
        {
            SkinnedMeshRenderer filter = tagarela.mainObject.GetComponent<SkinnedMeshRenderer>();
            filter.sharedMesh.vertices = tagarela.neutralMesh.vertices;
            if (filter.sharedMesh != null)
            {
                filter.sharedMesh = tagarela.neutralMesh_temp;
            }
            filter.sharedMesh.name = "Tagarela";
            tagarela.basemesh = filter.sharedMesh as Mesh;
        }

        tagarela.PreviewAnimation(updateMorphValue);
        updateMorph = false;
    }

    private void RestoreOriginalMesh()
    {
        if (tagarela != null && originalObject != null)
        {
            if (tagarela.mainObject.GetComponent<MeshFilter>())
            {
                tagarela.mainObject.GetComponent<MeshFilter>().sharedMesh = originalObject;
            }
            else if (tagarela.mainObject.GetComponent<SkinnedMeshRenderer>())
            {
                tagarela.mainObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = originalObject;
            }
        }
    }

    private void BackupOriginalMesh()
    {
        tagarela = Selection.activeGameObject.GetComponent<Tagarela>();

        if (tagarela.mainObject.GetComponent<MeshFilter>())
        {
            originalObject = tagarela.mainObject.GetComponent<MeshFilter>().sharedMesh;
        }
        else if (tagarela.mainObject.GetComponent<SkinnedMeshRenderer>())
        {
            originalObject = tagarela.mainObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
    }

    /*
    public void RefreshMeshlist()
    {
        morphTargetList = new MorphTargetList();
        //morphTargetList.Add(tagarela.meshNeutral.name, tagarela.meshNeutral);
        tagarela.morphTargets.ForEach(delegate(Mesh m)
        {
            morphTargetList.Add(m.name, m);
            //Debug.Log(m.name);
        }); 

    }
    */

    private void RefreshAudiolist()
    {
        audioList = new List<string>();
        tagarela.audioFiles.ForEach(delegate(AudioClip a)
        {
            audioList.Add(a.name);
        });
    }

}