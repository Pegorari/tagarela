//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari
/*
Based on "Save and Load from XML" routine
Author: Zumwalt
http://www.unifycommunity.com/wiki/index.php?title=Save_and_Load_from_XML
*/

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using UnityEngine;

class Tagarela_loader
{
    private TagarelaFileStructure FileData = new TagarelaFileStructure();
    private string _data;

    public TagarelaFileStructure Load(TextAsset file)
    {

        TextAsset asset = file;
        _data = asset.text;
        if (_data.ToString() != "")
        {
            FileData = (TagarelaFileStructure)DeserializeObject(_data);
        }
        return FileData;

    }

    //*************************************************** 
    // Saving ...
    // **************************************************    

    /* The following metods came from the referenced URL */
    byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    // Here we deserialize it back into its original form 
    object DeserializeObject(string pXmlizedString)
    {
        XmlSerializer xs = new XmlSerializer(typeof(TagarelaFileStructure));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        return xs.Deserialize(memoryStream);
    }

    // SaveData is our custom class that holds our defined objects we want to store in XML format 

}