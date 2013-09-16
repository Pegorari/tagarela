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
using System;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Tagarela/LipSync")]

public class Tagarela : MonoBehaviour
{
    public static Tagarela instance;
    public TagarelaFileStructure settings;

    public GameObject mainObject;
    public Mesh neutralMesh,neutralMesh_temp;
    public List<Mesh> morphTargets = new List<Mesh>();
    public List<TextAsset> animationFiles = new List<TextAsset>();
    public List<AudioClip> audioFiles = new List<AudioClip>();

    private float[] keyValues;
    private List<float[]> _SliderSettings = new List<float[]>();

    public Mesh basemesh;
    private List<Mesh> listademesh = new List<Mesh>();
    private vertexTarget[] v3_listademesh;
    private Vector3[] morphFrom, morphTo;

    private Timeline[] timeline;

    public bool isPlaying = false;
    private bool editionMode = false;
    private float timer = 0;
	private float timerLast = 0;
	public float timerCurrent = 0;
	
    private float keyframeCurrentTime, keyframeDurationTime;
    private int keyframeCurrent, keyframeNew;

    private class Timeline
    {
        public float keyframeTime;
        public Vector3[] morphTarget;

        public Timeline(float keyframeCurrentTime, Vector3[] k_morphTarget)
        {
            this.keyframeTime = keyframeCurrentTime;
            this.morphTarget = k_morphTarget;
        }
    }

    private class vertexTarget
    {
        public Vector3[] target;
        //public int[] index;
    }

    public void Awake()
    {
        audio.Stop();
        instance = this;
    }

    public void Reset()
    {
        //basemesh.RecalculateBounds();
        if (neutralMesh != null && timeline != null)
        {
            morphFrom = neutralMesh.vertices;
            morphTo = timeline[0].morphTarget;
        }
    }

    public void Repair()
    {
        morphTargets.ForEach(delegate(Mesh m)
        {
            if (m == null) morphTargets.Remove(m);
        });
        audioFiles.ForEach(delegate(AudioClip a)
        {
            if (a == null) audioFiles.Remove(a);
        });
        animationFiles.ForEach(delegate(TextAsset t)
        {
            if (t == null) animationFiles.Remove(t);
        });
    }
    
    public void PreviewAudio(float newTime)
    {
        if (audio.clip != null)
        {
            audio.time = Mathf.Clamp(newTime, 0f, audio.clip.length);
        }
    }

    public void PreviewAnimation(float newTime)
    {
        if (timer == newTime) keyframeCurrent = -1;
        
        if (!editionMode) {
            editionMode = true;
        }
         
        timer = Mathf.Clamp(newTime, 0f, settings.animationTime);
        LateUpdate();
    }
	
	public void StartTimer(){

        if (settings.audioFile != null && audio.clip != null)
        {
            audio.Play();
            timerLast = 0;
            timerCurrent = 0;
            timer = 0;
        }
        else
        {
            timerLast = Time.realtimeSinceStartup;
            timerCurrent = timerLast;
            timer = 0;
        }
        isPlaying = true;
	}
	
    public void setTimer(float newTimer)
    {
        if (audio != null & audio.isPlaying)
        {
            audio.time = newTimer;
            timerCurrent = newTimer;
            timer = newTimer;
        } else {
            timerLast = Time.realtimeSinceStartup - newTimer;
            timerCurrent = Time.realtimeSinceStartup;
            timer = timerCurrent;
        }
    }

    public float getTimer()
    {
		if (isPlaying){

            if (settings.audioFile != null && audio.clip != null)
            {
                if (audio.isPlaying)
                {
                    timerCurrent = audio.time;
                    timerCurrent = Mathf.Clamp(timerCurrent, 0, audio.clip.length);
                }
                else {
                    timerCurrent = 0;
                }

            }
            else {
                timerCurrent = Time.realtimeSinceStartup - timerLast;
                timerCurrent = Mathf.Clamp(timerCurrent, 0, settings.animationTime);
            }

            if (timerCurrent > settings.animationTime)
            {
                Stop();
            }
            timer = timerCurrent;

		}
		
        return timer;
    }

    public void Play(int index)
    {
        if (animationFiles.Count < index){
            Debug.LogWarning("Tagarela: Invalid Range! Index is bigger than file number.");
            return;
        }

        TextAsset finder = animationFiles[index];
        if (finder != null)
        {
            PlayFile(finder);
        } else {
            Debug.LogWarning("Tagarela: Animation file index '" + index + "' not found!");
        }

    }

    public void Play(string fileName)
    {
        TextAsset finder = (TextAsset)animationFiles.Find(delegate(TextAsset t) { return t.name == fileName; });
        if (finder != null)
        {
            PlayFile(finder);
        } else {
            Debug.LogWarning("Tagarela: File '" + fileName + "' not found!");
        }
    }

    void PlayFile(TextAsset finder){
        editionMode = false;
        Clean();
        OpenFile(finder);
        StartTimer();
        BuildTimeline();
        //isPlaying = true;
        //if (audio != null) audio.Play();
    }

    public void OpenFile(TextAsset file)
    {
        Tagarela_loader LoadFile = new Tagarela_loader();
        settings = LoadFile.Load(file);
        audio.clip = null;
        audio.clip = audioFiles.Find(delegate(AudioClip a) { return a.name == settings.audioFile; });
    }

    public void Stop()
    {
        setTimer(0);
        basemesh.vertices = neutralMesh.vertices;
        basemesh.RecalculateBounds();
        isPlaying = false;
        if (audio.isPlaying) audio.Stop();
    }
    /*
    public void Update()
    {

        if (isPlaying && getTimer() > 0 || editionMode)
        {
            //Animate();
        }
    }
    */
    public void LateUpdate()
    {

        if (isPlaying && getTimer() > 0 || editionMode)
        {
            Animate();
        } 
    }

    public void Animate()
    {
        //Resources.UnloadUnusedAssets();

        if (morphFrom == null || morphFrom.Length == 0) morphFrom = neutralMesh.vertices;
        if (morphTo == null || morphTo.Length == 0) morphTo = timeline[0].morphTarget;

        Vector3[] blendMesh = neutralMesh.vertices;

        if (keyframeCurrent != keyframeNew)
        {
            keyframeCurrent = keyframeNew;

            if (keyframeCurrent == 0)
            {
                morphFrom = neutralMesh.vertices;
                morphTo = timeline[0].morphTarget;
            }
            else if (keyframeCurrent < timeline.Length - 1 && keyframeCurrent > 0)
            {
                morphFrom = timeline[keyframeCurrent - 1].morphTarget;
                morphTo = timeline[keyframeCurrent].morphTarget;
            }
            else if (keyframeCurrent == timeline.Length - 1)
            {
                morphFrom = timeline[keyframeCurrent - 1].morphTarget;
                morphTo = neutralMesh.vertices;
            }
        };

        //UPDATE KEYFRAME
        for (int i = 0; i < timeline.Length; i++)
        {
            if (i == 0 && getTimer() <= timeline[0].keyframeTime)
            {
                keyframeNew = 0;
            }
            else if (i > 0 && getTimer() > timeline[i - 1].keyframeTime)
            {
                keyframeNew = i;
            };
        }

        if (keyframeCurrent == 0)
        {
            keyframeCurrentTime = getTimer();
            keyframeDurationTime = timeline[keyframeCurrent].keyframeTime;
        }
        else if (keyframeCurrent > 0 && keyframeCurrent < timeline.Length)
        {
            keyframeCurrentTime = getTimer() - timeline[keyframeCurrent - 1].keyframeTime;
            keyframeDurationTime = timeline[keyframeCurrent].keyframeTime - timeline[keyframeCurrent - 1].keyframeTime;
        }

        //UPDATE MESH

        //float easing = EaseInOutQuint(keyframeCurrentTime, 0, 1, keyframeDurationTime);
        float easing = EaseInOutSine(keyframeCurrentTime, 0, 1, keyframeDurationTime);
        
        if (keyframeDurationTime == 0) easing = 1; //fix caso o primeiro keyframe for zero

        for (int i = 0; i < blendMesh.Length; i++)
        {
            blendMesh[i] = Vector3.Lerp(morphFrom[i], morphTo[i], easing);
        }

        basemesh.vertices = blendMesh;
        basemesh.RecalculateBounds();
        //basemesh.hideFlags = HideFlags.DontSave;
    }

    public void BuildTimeline()
    {
        keyValues = settings.keyframes.values;
        _SliderSettings = settings.keyframes.sliderSettings;
        listademesh = new List<Mesh>();
        for (int j = 0; j < settings.meshList.id.Length; j++)
        {
            for (int i = 0; i < morphTargets.Count; i++)
            {
                if (morphTargets[i].name == settings.meshList.description[j])
                {
                    //morphTargets[i].hideFlags = HideFlags.DontSave;
                    listademesh.Add(morphTargets[i]);
                }
            }

        }

        timeline = new Timeline[keyValues.Length + 1];

        v3_listademesh = new vertexTarget[listademesh.Count];
		
        Vector3[] v3_BaseMesh = neutralMesh.vertices;
        for (int j = 0; j < listademesh.Count; j++)
        {
            v3_listademesh[j] = new vertexTarget();
            v3_listademesh[j].target = new Vector3[neutralMesh.vertexCount];
            v3_listademesh[j].target = listademesh[j].vertices;

            for (int h = 0; h < neutralMesh.vertexCount; h++)
            {
                v3_listademesh[j].target[h] -= v3_BaseMesh[h];
            }
        }

        for (int i = 0; i < keyValues.Length; i++)
        {

            Vector3[] v3_temp = new Vector3[neutralMesh.vertexCount];

            v3_temp = neutralMesh.vertices;

            float[] slider = _SliderSettings[i];

            for (int j = 0; j < listademesh.Count; j++)
            {
                for (int h = 0; h < neutralMesh.vertexCount; h++)
                {
                    v3_temp[h] += v3_listademesh[j].target[h] * ((float)slider[j] / 100);
                }
            }

            timeline[i] = new Timeline(keyValues[i], v3_temp);

        }

        timeline[keyValues.Length] = new Timeline(settings.animationTime, neutralMesh.vertices);

        if (neutralMesh_temp == null)
        {
            neutralMesh_temp = Instantiate(neutralMesh) as Mesh;
        }
        
        neutralMesh_temp.vertices = neutralMesh.vertices;

        if (mainObject.GetComponent<MeshFilter>())
        {
            MeshFilter filter = mainObject.GetComponent<MeshFilter>();
            filter.sharedMesh.vertices = neutralMesh.vertices;
            if (filter.sharedMesh != null)
            {
                filter.sharedMesh = neutralMesh_temp;
            }
            filter.sharedMesh.name = "Tagarela";
            basemesh = filter.sharedMesh as Mesh;
            //basemesh.hideFlags = HideFlags.DontSave;
        }
        else if (mainObject.GetComponent<SkinnedMeshRenderer>())
        {
            SkinnedMeshRenderer filter = mainObject.GetComponent<SkinnedMeshRenderer>();
            filter.sharedMesh.vertices = neutralMesh.vertices;
            if (filter.sharedMesh != null)
            {
                filter.sharedMesh = neutralMesh_temp;
            }
            filter.sharedMesh.name = "Tagarela";
            basemesh = filter.sharedMesh as Mesh;
            //basemesh.hideFlags = HideFlags.DontSave;
        }

    }

    public void Clean()
    {
        audio.clip = null;

        keyValues = null;
        _SliderSettings = null;

        listademesh = null;
        v3_listademesh = null;

        isPlaying = false;
        editionMode = false;
        timer = 0;
        basemesh = null;

        timeline = null;

        keyframeCurrentTime = 0;
        keyframeDurationTime = 0;
        keyframeCurrent = 0;
        keyframeNew = 0;

        morphFrom = null;
        morphTo = null;

        settings = null;

    }

    float EaseInOutSine(float t, float b, float c, float d)
    {
        return -c / 2 * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
    }
    float EaseInOutCirc(float t, float b, float c, float d)
    {
        if ((t /= d / 2) < 1) return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;
        return c / 2 * (Mathf.Sqrt(1 - (t -= 2) * t) + 1) + b;
    }
    float EaseInOutQuint(float t, float b, float c, float d)
    {
        if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
        return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
    }
    float EaseNone(float t, float b, float c, float d)
    {
        return c * t / d + b;
    }

}