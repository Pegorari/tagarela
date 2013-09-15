//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;

class TagarelaRulerUI
{
    public Rect SliderGroupRect;
    public float TotalValue;
    int StepValue;
    private Texture2D ponto = (Texture2D)Resources.Load("ponto", typeof(Texture));

    public TagarelaRulerUI(Rect SliderGroupRect, float TotalValue)
    {
        this.SliderGroupRect = SliderGroupRect;
        this.TotalValue = TotalValue;
        this.StepValue = 10;
    }

    public void Draw()
    {
        GUI.color = Color.white;
        GUI.DrawTexture(SliderGroupRect, ponto);

        float passo = Mathf.Abs(SliderGroupRect.width / TotalValue) * StepValue;
        int contador = 0;

        for (int i = 0; i < TotalValue; i++)
        {
            if (i % 5 == 0)
            {
                GUI.DrawTexture(new Rect(SliderGroupRect.x + (SliderGroupRect.width / TotalValue) * i, SliderGroupRect.y, 1, 6), ponto);
            }
            else
            {
                GUI.DrawTexture(new Rect(SliderGroupRect.x + (SliderGroupRect.width / TotalValue) * i, SliderGroupRect.y, 1, 3), ponto);
            }
        }

        for (int i = 0; i < TotalValue; i = i + StepValue)
        {
            Rect temp_rect = new Rect(SliderGroupRect.x + (passo * contador), SliderGroupRect.y - 14, 20, 15);
            GUI.color = Color.gray;
            GUI.Label(temp_rect, i.ToString(), EditorStyles.miniLabel);
            contador++;
        }

        Rect temp = new Rect(SliderGroupRect.xMax, SliderGroupRect.y - 14, 20, 15);
        GUI.Label(temp, TotalValue.ToString(), EditorStyles.miniLabel);

        //GUI.Label(new Rect(100, 400, teste.CalcSize().x, 100),"teste",teste);
    }
}
