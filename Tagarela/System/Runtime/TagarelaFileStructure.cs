//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using System.Collections;
using System.Collections.Generic;

public class TagarelaFileStructure
{
    public float animationTime;
    public int meshVertexCount;
    public string audioFile;

    public MeshList meshList;
    public Keyframes keyframes;

    public struct MeshList
    {
        public string[] id;
        public string[] description;
    }
    public struct Keyframes
    {
        public float[] values;
        public List<float[]> sliderSettings;
    }
}