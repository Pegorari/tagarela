﻿/*============================================================
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

    public List<SkinnedMeshRenderer> smrTotal;
    public int smrTotalBlendShapesCount;
    private blendTarget[] morphTargets;

    public GameObject mainObject;

    //public List<Mesh> morphTargets = new List<Mesh>();
    public List<TextAsset> animationFiles = new List<TextAsset>();
    public List<AudioClip> audioFiles = new List<AudioClip>();

    private float[] morphOriginal, morphFrom, morphTo;
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
        public float[] morphValue;

        public Timeline(float keyframeCurrentTime, float[] keyframeMorphValue)
        {
            this.keyframeTime = keyframeCurrentTime;
            this.morphValue = keyframeMorphValue;
        }
    }


    private class blendTarget
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public int blendShapeIndex;
        public void SetValue(float value)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value);
        }
        //public int[] index;
    }


    public void Awake()
    {
        audio.Stop();
        instance = this;
        FindBlendShapes();
    }

    public void FindBlendShapes()
    {
        SkinnedMeshRenderer[] smr = GetComponents<SkinnedMeshRenderer>();
        SkinnedMeshRenderer[] smr_children = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        smrTotal = new List<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer sm in smr)
        {
            if (sm.sharedMesh.blendShapeCount > 0)
            {
                smrTotal.Add(sm);
            }
        }

        foreach (SkinnedMeshRenderer sm in smr_children)
        {
            if (sm.sharedMesh.blendShapeCount > 0)
            {
                smrTotal.Add(sm);
            }
        }

        smrTotalBlendShapesCount = 0;
        if (smrTotal.Count > 0)
        {
            foreach (SkinnedMeshRenderer smrItem in smrTotal)
            {
                smrTotalBlendShapesCount += smrItem.sharedMesh.blendShapeCount;
            }
        }

    }

    public void Reset()
    {
        //basemesh.RecalculateBounds();
        if (timeline != null)
        {
            //morphFrom = neutralMesh.vertices;
            morphFrom = timeline[0].morphValue;
            morphTo = timeline[0].morphValue;
        }
    }

    public void Repair()
    {
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
            audio.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            audio.Play();
        }
    }

    public void PreviewAnimation(float newTime)
    {
        if (timer == newTime) keyframeCurrent = -1;

        if (!editionMode)
        {
            editionMode = true;
        }

        timer = Mathf.Clamp(newTime, 0f, settings.animationTime);
        LateUpdate();
    }

    public void StartTimer()
    {
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
        }
        else
        {
            timerLast = Time.realtimeSinceStartup - newTimer;
            timerCurrent = Time.realtimeSinceStartup;
            timer = timerCurrent;
        }
    }

    public float getTimer()
    {
        if (isPlaying)
        {

            if (settings.audioFile != null && audio.clip != null)
            {
                if (audio.isPlaying)
                {
                    timerCurrent = audio.time;
                    timerCurrent = Mathf.Clamp(timerCurrent, 0, audio.clip.length);
                }
                else
                {
                    timerCurrent = 0;
                }

            }
            else
            {
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
        if (animationFiles.Count < index)
        {
            Debug.LogWarning("Tagarela: Invalid Range! Index is bigger than file number.");
            return;
        }

        TextAsset finder = animationFiles[index];
        if (finder != null)
        {
            PlayFile(finder);
        }
        else
        {
            Debug.LogWarning("Tagarela: Animation file index '" + index + "' not found!");
        }

    }

    public void Play(string fileName)
    {
        TextAsset finder = (TextAsset)animationFiles.Find(delegate(TextAsset t) { return t.name == fileName; });
        if (finder != null)
        {
            PlayFile(finder);
        }
        else
        {
            Debug.LogWarning("Tagarela: File '" + fileName + "' not found!");
        }
    }

    void PlayFile(TextAsset file)
    {
        editionMode = false;
        Clean();
        OpenFile(file);
        BuildTimeline();
        StartTimer();
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
        isPlaying = false;
        if (audio.isPlaying) audio.Stop();
    }

    public void LateUpdate()
    {
        if (isPlaying && getTimer() > 0 || editionMode)
        {
            Animate();
        }
    }

    public void Animate()
    {

        if (morphFrom == null || morphFrom.Length == 0) morphFrom = morphOriginal;
        if (morphTo == null || morphTo.Length == 0) morphTo = timeline[0].morphValue;

        //Vector3[] blendMesh = neutralMesh.vertices;

        if (keyframeCurrent != keyframeNew)
        {
            keyframeCurrent = keyframeNew;

            if (keyframeCurrent == 0)
            {
                morphFrom = new float[smrTotalBlendShapesCount];
                morphTo = timeline[0].morphValue;
            }
            else if (keyframeCurrent < timeline.Length - 1 && keyframeCurrent > 0)
            {
                morphFrom = timeline[keyframeCurrent - 1].morphValue;
                morphTo = timeline[keyframeCurrent].morphValue;
            }
            else if (keyframeCurrent == timeline.Length - 1)
            {
                morphFrom = timeline[keyframeCurrent - 1].morphValue;
                morphTo = timeline[keyframeCurrent].morphValue;
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
        float easing = EaseInOutSine(keyframeCurrentTime, 0, 1, keyframeDurationTime);

        if (keyframeDurationTime == 0) easing = 1; //fix caso o primeiro keyframe for zero

        float[] slider = timeline[keyframeCurrent].morphValue;

        for (int j = 0; j < slider.Length; j++)
        {
            morphTargets[j].SetValue(Mathf.Lerp(morphFrom[j], morphTo[j], easing));
        }

    }

    public void BuildTimeline()
    {


        timeline = new Timeline[settings.keyframes.values.Length + 1];

        morphOriginal = new float[smrTotalBlendShapesCount];
        morphTargets = new blendTarget[smrTotalBlendShapesCount];
        int indexCounter = 0;
        for (int i = 0; i < smrTotal.Count; i++)
        {
            for (int j = 0; j < smrTotal[i].sharedMesh.blendShapeCount; j++)
            {
                morphOriginal[indexCounter] = smrTotal[i].GetBlendShapeWeight(j);
                morphTargets[indexCounter] = new blendTarget();
                morphTargets[indexCounter].skinnedMeshRenderer = smrTotal[i];
                morphTargets[indexCounter].blendShapeIndex = j;
                indexCounter++;
            }
        }

        for (int i = 0; i < settings.keyframes.values.Length; i++)
        {
            float[] slider = settings.keyframes.sliderSettings[i];
            timeline[i] = new Timeline(settings.keyframes.values[i], slider);
        }

        //float[] neutralSliders = new float[smrTotalBlendShapesCount];
        timeline[settings.keyframes.values.Length] = new Timeline(settings.animationTime, morphOriginal);

    }

    public void Clean()
    {
        audio.clip = null;

        isPlaying = false;
        editionMode = false;
        timer = 0;

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