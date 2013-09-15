//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#region MorphTarget
class TagarelaMorphTarget
{
    public List<float> sliderValue;
    public List<string> id;
    public void Populate(List<Mesh> origin_MeshList)
    {
        sliderValue = new List<float>();
        id = new List<string>();
        for (int i = 0; i < origin_MeshList.Count; i++)
        {
            sliderValue.Add(0f);
            id.Add(origin_MeshList[i].name);
        }
    }
    public void Populate(TagarelaMorphTarget origin_morphTargetList)
    {
        sliderValue = new List<float>();
        id = new List<string>();
        for (int i = 0; i < origin_morphTargetList.sliderValue.Count; i++)
        {
            sliderValue.Add(origin_morphTargetList.sliderValue[i]);
            id.Add(origin_morphTargetList.id[i]);
        }
    }

}
#endregion