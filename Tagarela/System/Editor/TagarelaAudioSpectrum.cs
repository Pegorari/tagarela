//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;

public class TagarelaAudioSpectrum
{

    // width = 512; height = 256;
    public enum PreviewType
    {
        bar, wave, both
    }
    public static Texture2D CreatePreview(AudioClip aud, int width, int height, Color color, PreviewType previewType)
    {

        int step = Mathf.CeilToInt((aud.samples * aud.channels) / width);
        float[] samples = new float[aud.samples * aud.channels];


        //workaround to prevent the error in the function getData when Audio Importer loadType is "compressed in memory"
        string path = AssetDatabase.GetAssetPath(aud);
        AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
        AudioImporterLoadType audioLoadTypeBackup = audioImporter.loadType;
        audioImporter.loadType = AudioImporterLoadType.StreamFromDisc;
        AssetDatabase.ImportAsset(path);

        //getData after the loadType changed
        aud.GetData(samples, 0);

        //restore the loadType
        audioImporter.loadType = audioLoadTypeBackup;
        AssetDatabase.ImportAsset(path);


        Texture2D img = new Texture2D(width, height, TextureFormat.RGBA32, false);

        if (previewType == PreviewType.wave)
        {

            Color[] xy = new Color[width * height];
            for (int x = 0; x < width * height; x++)
            {
                xy[x] = new Color(0, 0, 0, 0);
                //xy[x] = new Color(0, 1, 0, 0.2f);
            }

            img.SetPixels(xy);

            int i = 0;
            while (i < width)
            {
                int barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
                int add = samples[i * step] > 0 ? 1 : -1;
                for (int j = 0; j < barHeight; j++)
                {
                    img.SetPixel(i, Mathf.FloorToInt(height / 2) - (Mathf.FloorToInt(barHeight / 2) * add) + (j * add), color);
                }
                ++i;

            }

            img.Apply();
        }
        else if (previewType == PreviewType.bar)
        {
            img = new Texture2D(width, 1, TextureFormat.RGBA32, false);
            int i = 0;
            while (i < width)
            {
                //int barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
                //int add = samples[i * step] > 0 ? 1 : -1;
                float colorIntensity = Mathf.Clamp(Mathf.Abs(samples[i * step]) * 10f, 0, 1);
                Color colorReturn = new Color(color.r / colorIntensity, color.g / colorIntensity, color.b / colorIntensity, colorIntensity / 4f);
                img.SetPixel(i, 0, colorReturn);
                ++i;
            }
            img.Apply();

        }
        else if (previewType == PreviewType.both)
        {
            /*
            Color[] xy = new Color[width * height];
            for (int x = 0; x < width * height; x++)
            {
                xy[x] = new Color(1, 0, 0, 1);
                //xy[x] = new Color(0, 1, 0, 0.2f);
            }
            img.SetPixels(xy);
            */
            int i = 0;
            while (i < width)
            {
                int barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
                int add = samples[i * step] > 0 ? 1 : -1;

                float colorIntensity = Mathf.Clamp(Mathf.Abs(samples[i * step]) * 10f, 0, 1);

                Color colorReturn = new Color(color.r / colorIntensity, color.g / colorIntensity, color.b / colorIntensity, colorIntensity / 6f);


                for (int j = 0; j < height; j++)
                {
                    img.SetPixel(i, j, colorReturn);                    
                }

                for (int j = 0; j < barHeight; j++)
                {
                    img.SetPixel(i, Mathf.FloorToInt(height / 2) - (Mathf.FloorToInt(barHeight / 2) * add) + (j * add), color);
                }
                ++i;

            }

            img.Apply();

        }
        return img;
    }

}

