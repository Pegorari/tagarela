//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Tagarela))]
public class TagarelaInspector : Editor
{ 
    private string versionNumber = "Tagarela Lip Sync V"+ Assembly.GetExecutingAssembly().GetName().Version;
	private Tagarela tagarela;
	public List<GameObject> listaObjetos;
	public Texture logo_inspector;
	public Texture bg_inspector;
	private Object[] filelist_animation_loader;
	private ArrayList filelist_animation = new ArrayList ();
	//private GUISkin guiskin_;
	//private GUISkin guiskin_original;
	private Mesh tempMesh;
	private AudioClip tempAudio;
	private TextAsset tempLipFile;
	private TagarelaFileStructure tempLipFilePreview;
	private GUIStyle
            styleBoxStep, styleBoxStepTitle, styleBoxStepContent,
            styleBoxError, styleBoxErrorTitle, styleBoxErrorContent,
            styleLabelGrid, styleButtonGrid, styleGrid, styleMainPanel,
            styleMiniLabel;
	int vertexCount = 0;
	int toolbar = 0;
	string[] toolbarContent = new string[] { "Meshes", "Audio", ".xml" };
	int nextStepBox = 0;
	string messageError = "";
	bool nextStep = false, setStyles = false;

	public void OnEnable ()
	{
		tagarela = target as Tagarela;
		tagarela.Repair ();

		if (!Selection.activeGameObject.GetComponent (typeof(AudioSource))) {
			Selection.activeGameObject.AddComponent (typeof(AudioSource));
		}

		logo_inspector = TagarelaFileManager.LoadImageResource("logoInspector.png");
		logo_inspector.hideFlags = HideFlags.DontSave;

        bg_inspector = TagarelaFileManager.LoadImageResource("bgInspector.png");
		bg_inspector.hideFlags = HideFlags.DontSave;
		bg_inspector.wrapMode = TextureWrapMode.Clamp;

		filelist_animation_loader = Resources.LoadAll ("animations", typeof(TextAsset));
		filelist_animation = new ArrayList ();
		if (filelist_animation_loader.Length > 0) {
			for (int i = 0; i < filelist_animation_loader.Length; i++) {
				if ((filelist_animation_loader [i] as TextAsset).text.Contains ("<MeshVertexCount>" + (Selection.activeGameObject.GetComponent (typeof(MeshFilter)) as MeshFilter).sharedMesh.vertexCount + "</MeshVertexCount>")) {
					filelist_animation.Add (filelist_animation_loader [i].name);
				}
			}
		}
		setStyles = true;
	}

	public override void OnInspectorGUI ()
	{
		if (setStyles) {
			styleBoxStep = new GUIStyle (GUI.skin.window);
			styleBoxStep.margin = new RectOffset (6, 6, 0, 0);
			styleBoxStep.padding = new RectOffset (4, 4, 1, 4);

			styleBoxStepTitle = new GUIStyle (EditorStyles.boldLabel);
			styleBoxStepTitle.margin = new RectOffset (0, 0, 0, 0);
			styleBoxStepTitle.padding = new RectOffset (0, 0, 1, 0);
			styleBoxStepTitle.wordWrap = true;

			styleBoxStepContent = new GUIStyle (EditorStyles.wordWrappedLabel);
			styleBoxStepContent.margin = new RectOffset (0, 0, 4, 0);
			styleBoxStepContent.padding = new RectOffset (0, 0, 0, 0);
			styleBoxStepContent.wordWrap = true;

			styleBoxError = new GUIStyle (styleBoxStep);
			styleBoxErrorTitle = new GUIStyle (styleBoxStepTitle);
			styleBoxErrorContent = new GUIStyle (styleBoxStepContent);

			styleBoxErrorTitle = new GUIStyle ();
			styleBoxErrorTitle.normal.textColor = Color.white;
			styleBoxErrorContent.normal.textColor = Color.white;

			styleLabelGrid = new GUIStyle ();
			styleLabelGrid.normal.background = EditorStyles.toolbarTextField.normal.background;
			styleLabelGrid.normal.textColor = EditorStyles.toolbarTextField.normal.textColor;
			styleLabelGrid.fontStyle = EditorStyles.toolbarTextField.fontStyle;
			styleLabelGrid.margin = new RectOffset (0, 0, 0, 0);
			styleLabelGrid.border = new RectOffset (2, 2, 3, 3);
			styleLabelGrid.padding = new RectOffset (2, 2, 1, 2);

			styleButtonGrid = new GUIStyle ();
			styleButtonGrid = EditorStyles.miniButtonRight;
			styleButtonGrid.margin.top = 0;
			styleButtonGrid.padding.top = 0;
			styleButtonGrid.padding.left = 0;
			styleButtonGrid.margin.right = 0;
			styleButtonGrid.margin.bottom = 2;

			styleGrid = new GUIStyle ();
			styleGrid.margin = new RectOffset (6, 4, 2, 2);

			styleMainPanel = new GUIStyle ();

            styleMainPanel.normal.background = TagarelaFileManager.LoadImageResource("MainPanel.png");
			styleMainPanel.normal.background.hideFlags = HideFlags.DontSave;

			styleMainPanel.border = new RectOffset (7, 7, 2, 6);
			styleMainPanel.padding = new RectOffset (10, 10, 10, 10);

			styleMiniLabel = new GUIStyle (EditorStyles.miniLabel);
			styleMiniLabel.alignment = TextAnchor.MiddleCenter;

			setStyles = false;
		}

		GUI.color = Color.white;
		GUILayout.Space (0);
		
		Rect lastRect = GUILayoutUtility.GetLastRect ();

        GUI.DrawTexture(new Rect (lastRect.x, lastRect.y + 4, Screen.width, bg_inspector.height), bg_inspector);

		//GUI.DrawTexture (new Rect (lastRect.x, lastRect.y + 4, Screen.width, bg_inspector.height), bg_inspector);
        
		GUILayout.Space (10);
		
		EditorGUILayout.BeginHorizontal ();
		{
			GUILayout.Box (new GUIContent (logo_inspector), new GUIStyle (), new GUILayoutOption[] { GUILayout.Width (110), GUILayout.Height (35) });
			if (nextStepBox != 0 || messageError != "") {
				GUI.enabled = false;
				GUI.color = Color.black;
			}

			if (GUILayout.Button ("Open Editor", new GUILayoutOption[] { GUILayout.Height (22) })) {
				TagarelaEditor.GetWindow(typeof(TagarelaEditor)).Show();
			}

		}
		EditorGUILayout.EndHorizontal ();
		GUI.color = Color.white;
		GUI.enabled = true;
		EditorGUILayout.BeginVertical (styleMainPanel);
		{
			GUILayout.Space (4);
			GUI.enabled = true;

			tagarela.mainObject = EditorGUILayout.ObjectField ("Main Object", tagarela.mainObject, typeof(GameObject), true) as GameObject;

			nextStep = true;
			nextStepBox = 0;
			messageError = "";

			//Check Main Object
			if (tagarela.mainObject == null) {
				nextStepBox = 1;
				nextStep = false;
			} else if (!tagarela.mainObject.GetComponent<MeshFilter> () && !tagarela.mainObject.GetComponent<SkinnedMeshRenderer> ()) {
				nextStepBox = 1;
				nextStep = false;
				EditorGUILayout.Separator ();
				messageError = "The selected object doesn't have a MeshFilter or SkinnedMeshRenderer Component.";
			} else if (tagarela.mainObject.GetComponent<MeshFilter> () && tagarela.mainObject.GetComponent<MeshFilter> ().sharedMesh != null) {
				vertexCount = tagarela.mainObject.GetComponent<MeshFilter> ().sharedMesh.vertexCount;
			} else if (tagarela.mainObject.GetComponent<SkinnedMeshRenderer> () && tagarela.mainObject.GetComponent<MeshFilter> ().sharedMesh != null) {
				vertexCount = tagarela.mainObject.GetComponent<SkinnedMeshRenderer> ().sharedMesh.vertexCount;
			}

			//Check Neutral Mesh
			if (nextStep) {
				if (tagarela.neutralMesh == null) {
					nextStepBox = 2;
					nextStep = false;
				} else if (tagarela.neutralMesh.vertexCount != vertexCount) {
					nextStepBox = 2;
					nextStep = false;
					toolbar = 0;
					messageError = "The Neutral Mesh Vertex Count must have the same that the Main Object.";
				}
			}

			//Check Morph Targets
			if (nextStep && (tagarela.morphTargets == null || tagarela.morphTargets.Count == 0)) {
				nextStep = false;
				nextStepBox = 3;
			}
			
			/*
			//Check Audio Files
			if (nextStep && (tagarela.audioFiles == null || tagarela.audioFiles.Count == 0)) {
				nextStepBox = 4;
				nextStep = false;
			}
			*/
			
			if (nextStepBox != 1) {
				EditorGUILayout.Separator ();
				toolbar = GUILayout.Toolbar (toolbar, toolbarContent);
				GUI.enabled = true;

				switch (toolbar) {
				case 0: //Mesh

					EditorGUILayout.Separator ();

					EditorGUILayout.BeginHorizontal ();
					{
						GUILayout.Label ("Neutral Mesh:");
						tagarela.neutralMesh = EditorGUILayout.ObjectField (tagarela.neutralMesh, typeof(Mesh), false) as Mesh;
					}
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.Separator ();

					if (nextStepBox != 2) {
						EditorGUILayout.BeginHorizontal ();
						{
							GUILayout.Label ("Blend Shape:");
							tempMesh = EditorGUILayout.ObjectField (tempMesh, typeof(Mesh), false) as Mesh;

							if (tempMesh == null || tempMesh != null && tempMesh.vertexCount != vertexCount || tempMesh != null && tempMesh == tagarela.neutralMesh) {
								GUI.enabled = false;
							}

							if (GUILayout.Button ("Add", new GUILayoutOption[] { GUILayout.Width (60), GUILayout.Height (16) })) {
								if (!tagarela.morphTargets.Find (delegate(Mesh m) {
									return m.name == tempMesh.name; }))
									tagarela.morphTargets.Add (tempMesh);
								tempMesh = null;
							}
							GUI.enabled = true;

						}
						EditorGUILayout.EndHorizontal ();

						if (tempMesh != null && tempMesh.vertexCount != vertexCount) {
							EditorGUILayout.Separator ();
							GUIErrorBox ("Blend Shape Vertex Count must have the same that the Main Object.");
							EditorGUILayout.Separator ();
						} else if (tempMesh != null && tempMesh == tagarela.neutralMesh) {
							EditorGUILayout.Separator ();
							GUIErrorBox ("The selected Mesh is the same that Neutral Mesh");
							EditorGUILayout.Separator ();
						}

						if (tagarela.morphTargets.Count > 0) {
							EditorGUILayout.BeginVertical (styleGrid);
							{

								tagarela.morphTargets.ForEach (delegate(Mesh m)
								{

									EditorGUILayout.BeginHorizontal ();

									GUILayout.Label (m.name, styleLabelGrid, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16) });
									if (GUILayout.Button ("Remove", styleButtonGrid, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16), GUILayout.Width (59) })) {
										tagarela.morphTargets.Remove (m);
										tempMesh = null;
									}
									;
									EditorGUILayout.EndHorizontal ();


								});
							}
							EditorGUILayout.EndVertical ();
						}

					}
					tempLipFilePreview = null;
					break;

				case 1: //Audio

					EditorGUILayout.Separator ();

					EditorGUILayout.BeginHorizontal ();
					{
						GUILayout.Label ("Audio Clip:");
						tempAudio = EditorGUILayout.ObjectField (tempAudio, typeof(AudioClip), false) as AudioClip;
						if (tempAudio == null)
							GUI.enabled = false;
						if (GUILayout.Button ("Add", new GUILayoutOption[] { GUILayout.Width (60), GUILayout.Height (16) }) && !tagarela.audioFiles.Find (delegate(AudioClip a) {
							return a.name == tempAudio.name; })) {
							tagarela.audioFiles.Add (tempAudio);
							tempAudio = null;
						}
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal ();


					if (tagarela.audioFiles.Count > 0) {
						EditorGUILayout.BeginVertical (styleGrid);
						{
							tagarela.audioFiles.ForEach (delegate(AudioClip a)
							{
								EditorGUILayout.BeginHorizontal ();

								GUILayout.Box (a.name, styleLabelGrid, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16) });
								if (GUILayout.Button ("Remove", styleButtonGrid, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16), GUILayout.Width (59) })) {
                                    //if (GUILayout.Button("X ", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(20) }))
									tagarela.audioFiles.Remove (a);
									tempAudio = null;
								}

								EditorGUILayout.EndHorizontal ();
							});
						}
						EditorGUILayout.EndVertical ();
					}
					tempLipFilePreview = null;
					break;

				case 2: //lip files

					EditorGUILayout.Separator ();

                        //tempLipFile = EditorGUILayout.ObjectField("Lip Sync files", tempLipFile, typeof(TextAsset), false) as TextAsset;
					if (GUILayout.Button ("Add Animation from Assets")) {
						string path = EditorUtility.OpenFilePanel ("Tagarela Animation Files", Application.dataPath + "/Tagarela/Animations/", "xml");
						path = path.Replace (Application.dataPath, "Assets");

						tempLipFile = AssetDatabase.LoadMainAssetAtPath (path) as TextAsset;
						if (tempLipFile != null) {
							tempLipFilePreview = TagarelaFileManager.Load (tempLipFile);
							if (tempLipFilePreview.meshVertexCount == vertexCount) {
								if (tagarela.animationFiles.Find (delegate(TextAsset a) {
									return a.name == tempLipFile.name; }) == null) {
									tagarela.animationFiles.Add (tempLipFile);
									tempLipFile = null;
								}
								;
							}
						}

					}

					if (tempLipFilePreview != null && tempLipFilePreview.meshVertexCount != vertexCount) {
						EditorGUILayout.Separator ();
						GUIErrorBox ("The selected file has inconsistent data.");
						EditorGUILayout.Separator ();
					}

					if (tagarela.animationFiles.Count > 0) {
						EditorGUILayout.BeginVertical (styleGrid);
						{
                            //int index = 0;

                            for (int index = tagarela.animationFiles.Count - 1; index >= 0; index--)
                            {
                                EditorGUILayout.BeginHorizontal();
                                TextAsset f = tagarela.animationFiles[index];
                                GUILayout.Box("(" + index + ") " + f.name, styleLabelGrid, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(16) });

                                if (GUILayout.Button("Remove", styleButtonGrid, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(16), GUILayout.Width(59) }))
                                {
                                    switch (EditorUtility.DisplayDialogComplex("Remove", "Do you want to delete completely the file?", "Yes", "No, Just remove", "Cancel"))
                                    {
                                        case 0:
                                            tagarela.animationFiles.Remove(f);
                                            TagarelaFileManager.Delete(f);
                                            tempLipFile = null;
                                            Repaint();
                                            break;
                                        case 1:
                                            tagarela.animationFiles.Remove(f);
                                            tempLipFile = null;
                                            break;
                                    }
                                    //tagarela.animationFiles.Remove (f);


                                }

                                EditorGUILayout.EndHorizontal();
                            }
                            /*
							tagarela.animationFiles.ForEach (delegate(TextAsset f)
							{
								EditorGUILayout.BeginHorizontal ();

                                GUILayout.Box("(" + index + ") " + f.name, styleLabelGrid, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(16) });
                                index++;
                                if (GUILayout.Button ("Remove", styleButtonGrid, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16), GUILayout.Width (59) })) {
                                    switch (EditorUtility.DisplayDialogComplex("Remove", "Do you want to delete completely the file?", "Yes", "No, Just remove", "Cancel"))
                                    {
                                        case 0:
                                            tagarela.animationFiles.Remove(f);
                                            TagarelaFileManager.Delete(f);
                                            tempLipFile = null;
                                            Repaint();
                                            break;
                                        case 1:
                                            tagarela.animationFiles.Remove(f);
                                            tempLipFile = null;
                                            break;
                                    }
									//tagarela.animationFiles.Remove (f);


								}
								
								EditorGUILayout.EndHorizontal ();

							});
                             */
						}
						EditorGUILayout.EndVertical ();
					}

					break;
				}
			}

			GUI.enabled = true;

			if (!nextStep && messageError != "") {
				GUIErrorBox (messageError);
			}

			EditorGUILayout.Separator ();

			switch (nextStepBox) {
			case 1:
				GUInextStepBox ("Step 1 - Main Object", "Select in Scene or Hierarchy Window the Main Object that you want to control. This object must have a Mesh Renderer or Skinned Mesh Renderer Component. In some cases, this is the Main Object.");
				EditorGUILayout.Separator ();
				break;

			case 2:
				GUInextStepBox ("Step 2 - Neutral Mesh", "Select a Neutral Mesh in your Project Window.");
				EditorGUILayout.Separator ();
				break;

			case 3:
				GUInextStepBox ("Step 3 - Blend Shapes", "Select at least one Blend Shape in your Project Window and click in Add.");
				EditorGUILayout.Separator ();
				break;
				/*
			case 4:
				GUInextStepBox ("Step 4 - Audio Files", "Select at least one Audio File in your Project Window and click in Add.");
				EditorGUILayout.Separator ();
				break;*/
			}

			GUILayout.Label (versionNumber, styleMiniLabel, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (16) });
			GUILayout.Label ("Developed by Rodrigo Pegorari - 2013\nhttp://www.rodrigopegorari.net", styleMiniLabel, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (26) });
			GUILayout.Space (4f);
		}
		EditorGUILayout.EndVertical ();
	}

	void GUInextStepBox (string title, string content)
	{
		GUILayout.BeginVertical (styleBoxStep, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (1) });
		GUILayout.Label (title, styleBoxStepTitle);
		GUILayout.Label (content, styleBoxStepContent);
		GUILayout.EndVertical ();
	}

	void GUIErrorBox (string content)
	{
		GUI.color = Color.red;
		GUILayout.BeginVertical (styleBoxError, new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (1) });
		GUI.color = Color.white;
		GUILayout.Label ("Error", styleBoxErrorTitle);
		GUILayout.Label (content, styleBoxErrorContent);
		GUILayout.EndVertical ();
	}
}
