//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class TagarelaTimelineUI
{
    public List<TLkeyframe> keyframeSet = new List<TLkeyframe>();
    public int selectedIndex = 0;
    public float selectedValue = -1;
    public Rect timeLineRect;
    public float totalValue;
    public bool init = false;
    public bool isKeyframeSetChanged = false; //info to pass to editor, to call an update on editor, after the new or cloned keyframe
    public TagarelaMorphTarget morphSliders;
    private GUIStyle styleTimeline;
    private Texture2D imgTimeline;

    public bool isDragging = false, refresh = false, enabled = true;

    public TagarelaTimelineUI(float totalValue)
    {
        timeLineRect = new Rect(0, 0, 100, 20);
        //TimeLineRect = new Rect(0, 0, 100, 40);
        this.totalValue = totalValue;

        imgTimeline = TagarelaFileManager.LoadImageResource("timeline.png");
        imgTimeline.hideFlags = HideFlags.DontSave;
        styleTimeline = new GUIStyle();
        styleTimeline.normal.background = imgTimeline;
        styleTimeline.border = new RectOffset(2, 2, 2, 2);
        styleTimeline.padding = new RectOffset(0, 0, 0, 0);
        styleTimeline.margin = new RectOffset(0, 0, 0, 0);
    }

    public void AddKeyframe(float Value, TagarelaMorphTarget Objeto)
    {
        bool keyframe_exists = false;
        for (int i = 0; i < keyframeSet.Count; i++)
        {
            TLkeyframe temp_keyframe = (TLkeyframe)keyframeSet[i];
            if (Value == temp_keyframe.value) keyframe_exists = true;
        }
        if (keyframe_exists)
        {
            EditorUtility.DisplayDialog("Error!", "Already exists a keyframe at position 0", "ok");
        }
        else
        {
            keyframeSet.Add(new TLkeyframe(Value, totalValue, timeLineRect, Objeto));

            TLkeyframe _keyframe = (TLkeyframe)keyframeSet[keyframeSet.Count-1];
            selectedValue = _keyframe.value;
            morphSliders = _keyframe.morphSliders;
            keyframeSet.Sort(new ordenador());

            isKeyframeSetChanged = true;
        }
    }

    public void RemoveKeyframe(int index)
    {
        if (index < keyframeSet.Count)
        {
            keyframeSet.RemoveAt(index);

            if (index > keyframeSet.Count - 1) index = keyframeSet.Count - 1;
            if (keyframeSet.Count > 0)
            {
                TLkeyframe _keyframe = (TLkeyframe)keyframeSet[index];
                selectedValue = _keyframe.value;
                selectedIndex = index;
                morphSliders = _keyframe.morphSliders;
                keyframeSet.Sort(new ordenador());
                isKeyframeSetChanged = true;
            }


        }
    }

    /*
    public void SetTargetToKeyframe(int Index, TagarelaMorphTarget Objeto)
    {

        TLkeyframe temp_keyframe = (TLkeyframe)KeyframeSet[Index];

        TagarelaMorphTarget CurrentObjeto = new TagarelaMorphTarget();


        CurrentObjeto = temp_keyframe.morphSliders;

        //popula com os que não tiver 
        for (int i = 0; i < Objeto.id.Count; i++)
        {
            if (!CurrentObjeto.id.Contains((string)Objeto.id[i]))
            {
                CurrentObjeto.id.Add((string)Objeto.id[i]);
                CurrentObjeto.sliderValue.Add((float)Objeto.sliderValue[i]);
            };
        }

        //remove the old ones
        for (int i = 0; i < CurrentObjeto.id.Count; i++)
        {
            if (!Objeto.id.Contains((string)CurrentObjeto.id[i]))
            {
                CurrentObjeto.id.RemoveAt(i);
                CurrentObjeto.sliderValue.RemoveAt(i);
            };
        }

        temp_keyframe.morphSliders = (TagarelaMorphTarget)CurrentObjeto;
        KeyframeSet[Index] = (TLkeyframe)temp_keyframe;

    } */

    public void UpdateSelection()
    {
        if (keyframeSet.Count > 0)
        {
            keyframeSet.Sort(new ordenador());
            TLkeyframe _keyframe = (TLkeyframe)keyframeSet[0];
            selectedIndex = 0;
            selectedValue = _keyframe.value;
            morphSliders = _keyframe.morphSliders;
            _keyframe.state = KeyframeState.selected;
        }
        init = true;
    }

    public void Draw(){
        Draw(timeLineRect);
    }

    public void Draw(Rect rect)
    {
        rect = new Rect(rect.x, rect.y, Mathf.Max(imgTimeline.width, rect.width), Mathf.Max(imgTimeline.height, rect.height));
        timeLineRect = rect;
        GUI.color = Color.white;

        GUI.Box(timeLineRect, "", styleTimeline);
        Rect dragLimit = new Rect(0, 0, Screen.width, Screen.height);

        if (!init) UpdateSelection();

        refresh = false;

        for (int index = 0; index < keyframeSet.Count; index++)
        {
            TLkeyframe _keyframe = (TLkeyframe)keyframeSet[index];
            _keyframe.timeLineRect = new Rect(timeLineRect.x + styleTimeline.border.left, timeLineRect.y + styleTimeline.border.top, timeLineRect.width - styleTimeline.border.horizontal, timeLineRect.height);
            _keyframe.iconRect.y = timeLineRect.y + 2; //top position of the icons
            _keyframe.totalValue = totalValue;
            if (enabled)
            {
                if (selectedValue == _keyframe.value && !isDragging)
                {
                    selectedIndex = index;
                    morphSliders = _keyframe.morphSliders;
                    _keyframe.state = KeyframeState.selected;
                }

                //SELECT SLIDER
                if ((Event.current.type == EventType.mouseDown) && (!isDragging) && (selectedIndex != index) && (_keyframe.iconRect.Contains(Event.current.mousePosition)))
                {
                    
                    for (int index2 = 0; index2 < keyframeSet.Count; index2++)
                    {
                        TLkeyframe temp_keyframe = (TLkeyframe)keyframeSet[index2];
                        if (index2 != index) temp_keyframe.state = KeyframeState.normal;
                    }
                    _keyframe.state = KeyframeState.selected;
                    selectedValue = _keyframe.value;
                    selectedIndex = index;
                    morphSliders = _keyframe.morphSliders;
                    refresh = true;
                }

                //START DRAG
                if (!isDragging && Event.current.type == EventType.mouseDrag && selectedIndex == index && _keyframe.iconRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;

                    //DRAG TO CLONE
                    if (Event.current.shift)
                    {
                        TagarelaMorphTarget morphInstance = new TagarelaMorphTarget();
                        morphInstance.id = _keyframe.morphSliders.id.GetRange(0,_keyframe.morphSliders.id.ToArray().Length);
                        morphInstance.sliderValue = _keyframe.morphSliders.sliderValue.GetRange(0, _keyframe.morphSliders.sliderValue.ToArray().Length);
                        AddKeyframe(selectedValue + 0.0001f, morphInstance);
                        isKeyframeSetChanged = true;
                    }
                }

                //UPDATE DRAG
                if (isDragging && selectedIndex == index)
                {
                    //if (EventType.mouseDown())
                    selectedIndex = index;
                    selectedValue = _keyframe.value;
                    morphSliders = _keyframe.morphSliders;
                    _keyframe.state = KeyframeState.drag;
                    refresh = true;
                }

                if (selectedIndex == index)
                {
                    GUI.color = GUI.contentColor;
                    //_keyframe.Value = EditorGUI.Slider(new Rect(TimeLineRect.xMax + 5, TimeLineRect.y, 50, 16), _keyframe.Value, 0f, totalValue);
                    //_keyframe.Value = SelectedValue;
                }

                //ROLLOVER
                if (selectedIndex != index && !isDragging && _keyframe.state != KeyframeState.drag)
                {

                    if (_keyframe.iconRect.Contains(Event.current.mousePosition))
                    {
                        //Refresh = true;
                        _keyframe.state = KeyframeState.over;
                    }
                    else
                    {
                        _keyframe.state = KeyframeState.normal;
                    }
                }

                //STOP DRAG
                if (Event.current.type == EventType.mouseUp || !dragLimit.Contains(Event.current.mousePosition) && isDragging)
                {
                    if (selectedValue == _keyframe.value)
                    {
                        _keyframe.state = KeyframeState.selected;
                        isDragging = false;
                        keyframeSet.Sort(new ordenador());
                    }
                }

            }
            else
            {
                _keyframe.state = KeyframeState.normal;
                //SelectedIndex = -1;
                //SelectedValue = -1;
            }

            //DRAW SLIDER
            _keyframe.draw();
        }

        //if (Refresh) GetWindow(typeof(TagarelaEditor)).Repaint();
    }

    public class TLkeyframe
    {
        private Vector2 drag_pos;
        Color seta_cor = Color.black;
        public bool repaint = false;
        public float value, totalValue;

        Texture2D seta_img = TagarelaFileManager.LoadImageResource("seta.png");

        public Rect iconRect, timeLineRect;

        public bool canDrag = false;
        public TagarelaMorphTarget morphSliders;
        
        public KeyframeState state = KeyframeState.normal;

        public TLkeyframe(float value, float totalValue, Rect timeLineRect, TagarelaMorphTarget morphSliders)
        {
            this.value = value;
            this.totalValue = totalValue;
            this.timeLineRect = timeLineRect;
            this.morphSliders = morphSliders;

            seta_img.hideFlags = HideFlags.DontSave;
            iconRect = new Rect(0, timeLineRect.y, seta_img.width, seta_img.height);

        }

        void Init()
        {
            //repaint = true;
        }
        public void draw()
        {
            //repaint = false;

            //CENTER IMAGE
            float fix_timeline_gap = timeLineRect.x - (iconRect.width / 2);

            switch (state)
            {

                case KeyframeState.normal:
                    canDrag = false;
                    EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                    seta_cor = Color.gray;
                    drag_pos.x = (Event.current.mousePosition.x - iconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.over:
                    canDrag = false;
                    EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                    seta_cor = Color.white;
                    drag_pos.x = (Event.current.mousePosition.x - iconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.selected:
                    canDrag = true;
                    EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                    seta_cor = new Color(255f/255f,185f/255f,55f/255f);
                    drag_pos.x = (Event.current.mousePosition.x - iconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.drag:
                    if (canDrag)
                    {
                        EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.SlideArrow);
                        seta_cor = new Color(190f / 255f, 250f / 255f, 100f / 255f);
                        value = ((Event.current.mousePosition.x - drag_pos.x) / timeLineRect.width) * totalValue;
                    }
                    break;
            }


            if (value > totalValue) value = totalValue;
            if (value < 0) value = 0f;

            iconRect.x = (value / totalValue) * timeLineRect.width + fix_timeline_gap;
            //Debug.Log("kf--> " + IconRect.x + " | totalValue " + totalValue + " | Value " + Value + " | TimeLineRect.width " + TimeLineRect.width);

            GUIContent img_content = new GUIContent();
            img_content.tooltip = value.ToString();

            GUI.color = seta_cor;
            GUI.DrawTexture(new Rect(iconRect.x, iconRect.y, iconRect.width, iconRect.height), seta_img);
            if (state != KeyframeState.drag) GUI.Label(new Rect(iconRect.x, iconRect.y, iconRect.width, iconRect.height), img_content);
            GUI.color = GUI.contentColor;

            //repaint = true;

        }

    }

    public class ordenador : IComparer<TLkeyframe>
    {
        //static TLkeyframe s1;
        //static TLkeyframe s2;

        public int Compare(TLkeyframe a, TLkeyframe b)
        {
            if (a.value > b.value)
                return 1;
            else if (a.value < b.value)
                return -1;
            else
                return 0;
        }
    }

    public enum KeyframeState
    {
        normal,
        over,
        selected,
        drag
    }

}
