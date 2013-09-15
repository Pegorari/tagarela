//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari
/*
Based on "Save and Load from XML" routine
Author: Zumwalt
http://www.unifycommunity.com/wiki/index.php?title=Save_and_Load_from_XML
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

static class TagarelaFileManager
{
    private static string _FileLocation, _FileName;
    private static TagarelaFileStructure FileData = new TagarelaFileStructure();
    private static string _data;

    #region SaveData
    public static TagarelaFileStructure Load(TextAsset file)
    {
        _data = file.text;
        if (_data.ToString() != "")
        {
            FileData = (TagarelaFileStructure)DeserializeObject(_data);
        }
        return FileData;
    }
    #endregion

    //*************************************************** 
    // Saving ...
    // **************************************************
    public static bool NewFile(string filename, int MeshVertexCount)
    {
        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath + "/Tagarela/System/Animations/";
        _FileName = filename + ".xml";

        // we need soemthing to store the information into 
        FileData = new TagarelaFileStructure();
        FileData.animationTime = 0f;
        FileData.audioFile = "";
        FileData.meshVertexCount = MeshVertexCount;

        // Time to creat our XML!
        _data = SerializeObject(FileData);
        // This is the final resulting XML from the serialization process
        return CreateXML();
    }

    //*************************************************** 
    // Saving ...
    // **************************************************
    public static bool Save(string filename, List<Mesh> meshList, List<TagarelaTimelineUI.TLkeyframe> KeyframeSet, int MeshVertexCount, string AudioFile, float TotalTime)
    {

        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath + "/Tagarela/System/Animations/";
        _FileName = filename + ".xml";

        // we need soemthing to store the information into 
        FileData = new TagarelaFileStructure();

        FileData.animationTime = TotalTime;
        FileData.audioFile = AudioFile;
        FileData.meshVertexCount = MeshVertexCount;

        string[] tempSaveData_Id = new string[meshList.Count];
        string[] tempSaveData_Description = new string[meshList.Count];
        for (int i = 0; i < meshList.Count; i++)
        {
            tempSaveData_Id[i] = meshList[i].name;
            tempSaveData_Description[i] = meshList[i].name;
        }
        FileData.meshList.id = tempSaveData_Id;
        FileData.meshList.description = tempSaveData_Description;

        FileData.keyframes.sliderSettings = new List<float[]>();
        FileData.keyframes.values = new float[KeyframeSet.Count];
        for (int i = 0; i < KeyframeSet.Count; i++)
        {
            //UI_TimeLine.TLkeyframe temp_keyframe = (UI_TimeLine.TLkeyframe);
            FileData.keyframes.sliderSettings.Add(KeyframeSet[i].morphSliders.sliderValue.ToArray());
            FileData.keyframes.values[i] = KeyframeSet[i].value;
            //FileData.keyframes.values[i] = (temp_keyframe.Value);
        }

        //FileData.Keyframes._single_keyframe = 

        // Time to creat our XML!
        _data = SerializeObject(FileData);
        // This is the final resulting XML from the serialization process
        return CreateXML();

    }

    //*************************************************** 
    // Saving ...
    // **************************************************
    public static TagarelaFileStructure UpdateSettings(List<Mesh> meshList, List<TagarelaTimelineUI.TLkeyframe> KeyframeSet, int MeshVertexCount, string AudioFile, float TotalTime)
    {
        // we need soemthing to store the information into 
        FileData = new TagarelaFileStructure();

        FileData.animationTime = TotalTime;
        FileData.audioFile = AudioFile;
        FileData.meshVertexCount = MeshVertexCount;

        string[] tempSaveData_Id = new string[meshList.Count];
        string[] tempSaveData_Description = new string[meshList.Count];
        for (int i = 0; i < meshList.Count; i++)
        {
            tempSaveData_Id[i] = meshList[i].name;
            tempSaveData_Description[i] = meshList[i].name;
        }
        FileData.meshList.id = tempSaveData_Id;
        FileData.meshList.description = tempSaveData_Description;

        FileData.keyframes.sliderSettings = new List<float[]>();
        FileData.keyframes.values = new float[KeyframeSet.Count];
        for (int i = 0; i < KeyframeSet.Count; i++)
        {
            //UI_TimeLine.TLkeyframe temp_keyframe = (UI_TimeLine.TLkeyframe);
            FileData.keyframes.sliderSettings.Add(KeyframeSet[i].morphSliders.sliderValue.ToArray());
            FileData.keyframes.values[i] = KeyframeSet[i].value;
            //FileData.keyframes.values[i] = (temp_keyframe.Value);
        }

        return FileData;

    }


    /* The following metods came from the referenced URL */
    private static string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    private static byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    // Here we serialize our SaveData object of FileData 
    private static string SerializeObject(object pObject)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(typeof(TagarelaFileStructure));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    // Here we deserialize it back into its original form 
    private static object DeserializeObject(string pXmlizedString)
    {
        XmlSerializer xs = new XmlSerializer(typeof(TagarelaFileStructure));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        return xs.Deserialize(memoryStream);
    }

    // Finally our save and load methods for the file itself 
    private static bool CreateXML()
    {

        StreamWriter writer;
        FileInfo t = new FileInfo(_FileLocation + _FileName);
        if (t.Exists) t.Delete();
        writer = t.CreateText();
        writer.Write(_data);
        writer.Close();
        AssetDatabase.Refresh();
        return true;

    }

    // DELETE POR NOME
    public static bool Delete(string filename)
    {
        return DeleteFile(filename);
    }
    // DELETE POR OBJETO
    public static bool Delete(Object file)
    {
        return DeleteFile(file.name);
    }

    private static bool DeleteFile(string filename)
    {
        // Where we want to save and load to and from 
        _FileLocation = Application.dataPath + "/Tagarela/System/Animations/";
        _FileName = filename + ".xml";
        FileInfo t = new FileInfo(_FileLocation + _FileName);
        if (t.Exists) t.Delete();
        AssetDatabase.Refresh();
        return true;
    }

    // convert bitmap to jpeg
	public static Texture2D LoadImageResource(string res){
        //System.Drawing.Bitmap search = (System.Drawing.Bitmap)TagarelaEditorDLL.Properties.Resources.ResourceManager.GetObject(res.Split('.')[0]);
        //return TagarelaFileManager.BitmapToTexture2D(search);
        Texture2D returnTexture = AssetDatabase.LoadMainAssetAtPath("Assets/Tagarela/System/Editor/Images/"+res) as Texture2D;
        //returnTexture.hideFlags = HideFlags.DontSave;
        return returnTexture;
    }

    /*
    public static Texture2D BitmapToTexture2D(System.Drawing.Bitmap bitmapOriginal)
    {
        // convert gif to bitmap
        System.Drawing.Bitmap bitmap2 = new System.Drawing.Bitmap(bitmapOriginal);
        System.Drawing.Size mySize = bitmap2.Size;

        // convert bitmap to jpeg
        MemoryStream memStream = new MemoryStream();
        bitmap2.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
        byte[] bytes = memStream.ToArray();
        Texture2D myTexture = new Texture2D(mySize.Width, mySize.Height);

        // load jpeg into Texture2D
        myTexture.LoadImage(bytes);
        return myTexture;
    }
    */
}