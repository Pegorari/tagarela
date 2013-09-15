//TAGARELA LIP SYNC SYSTEM
//Copyright (c) 2013 Rodrigo Pegorari

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class TagarelaTimelineSegmentUI
{
    public List<TLkeyframe> KeyframeSet = new List<TLkeyframe>();
    public int SelectedIndex = 0;
    public float SelectedValue = 0;
    public Rect TimeLineRect;
    public float totalValue;
    public bool Init = false;
    public bool isDragging = false, refresh = false, enabled = true, active = false;

    private Texture2D icoA;
    private Texture2D icoB;

    public TagarelaTimelineSegmentUI(float TotalValue)
    {
        TimeLineRect = new Rect(0, 0, 100, 20);
        this.totalValue = TotalValue;

        icoA = TagarelaFileManager.LoadImageResource("icoSegmentA.png");
        icoA.hideFlags = HideFlags.DontSave;
        
        icoB = TagarelaFileManager.LoadImageResource("icoSegmentB.png");
        icoB.hideFlags = HideFlags.DontSave;
        
        KeyframeSet.Add(new TLkeyframe(0f, TotalValue, TimeLineRect,new Rect(0,0,icoA.width,icoA.height)));
        KeyframeSet.Add(new TLkeyframe(TotalValue, TotalValue, TimeLineRect,new Rect(0,0,icoA.width,icoA.height)));
    }

    public void AddKeyframe(float Value)
    {
        bool keyframe_exists = false;
        for (int i = 0; i < KeyframeSet.Count; i++)
        {
            TLkeyframe temp_keyframe = (TLkeyframe)KeyframeSet[i];
            if (Value == temp_keyframe.Value) keyframe_exists = true;
        }
        if (keyframe_exists)
        {
            EditorUtility.DisplayDialog("Erro!", "Já existe um keyframe aqui!", "ok");
        }
        else
        {
            KeyframeSet.Add(new TLkeyframe(Value, totalValue, TimeLineRect, new Rect(0, 0, icoA.width, icoA.height)));
            SelectedValue = Value;
        }
    }

    public void UpdateSelection()
    {
        if (KeyframeSet.Count > 0)
        {
            KeyframeSet.Sort(new ordenador());
            TLkeyframe _keyframe = (TLkeyframe)KeyframeSet[0];
            SelectedIndex = 0;
            SelectedValue = _keyframe.Value;
            _keyframe.state = KeyframeState.selected;
        }
        Init = true;
    }

    public void Draw(Rect rect)
    {
        TimeLineRect = rect;
        GUI.color = Color.white;
        //GUI.Box(TimeLineRect, "", EditorStyles.toolbarTextField);
        Rect DragLimit = new Rect(0, 0, Screen.width, Screen.height);

        if (!Init) UpdateSelection();

        refresh = false;

        for (int index = 0; index < KeyframeSet.Count; index++)
        {
            TLkeyframe _keyframe = (TLkeyframe)KeyframeSet[index];

            _keyframe.TimeLineRect = TimeLineRect;
            //_keyframe.TimeLineRect.y = TimeLineRect.y + 20;
            _keyframe.IconRect.y = TimeLineRect.y - 18;
            _keyframe.TotalValue = totalValue;

            if (enabled)
            {
                if (SelectedValue == _keyframe.Value && !isDragging)
                {
                    SelectedIndex = index;
                    _keyframe.state = KeyframeState.selected;
                }

                //SELECT SLIDER
                if ((Event.current.type == EventType.mouseDown) && (!isDragging) && (SelectedIndex != index) && (_keyframe.IconRect.Contains(Event.current.mousePosition)))
                {
                    for (int index2 = 0; index2 < KeyframeSet.Count; index2++)
                    {
                        TLkeyframe temp_keyframe = (TLkeyframe)KeyframeSet[index2];
                        if (index2 != index) temp_keyframe.state = KeyframeState.normal;
                    }
                    _keyframe.state = KeyframeState.selected;
                    SelectedValue = _keyframe.Value;
                    SelectedIndex = index;
                    refresh = true;
                }

                //START DRAG
                if (!isDragging && Event.current.type == EventType.mouseDrag && SelectedIndex == index && _keyframe.IconRect.Contains(Event.current.mousePosition))
                {
                    isDragging = true;
                    if (Event.current.shift)
                    {
                        Debug.Log("duplicar");
                    }
                }

                //UPDATE DRAG
                if (isDragging && SelectedIndex == index)
                {
                    //if (EventType.mouseDown())
                    SelectedIndex = index;
                    SelectedValue = _keyframe.Value;
                    _keyframe.state = KeyframeState.drag;
                    refresh = true;
                }

                if (SelectedIndex == index)
                {
                    GUI.color = GUI.contentColor;
                    _keyframe.Value = SelectedValue;
                    //_keyframe.Value = EditorGUI.Slider(new Rect(TimeLineRect.xMax + 5, TimeLineRect.y, 50, 16), _keyframe.Value, 0f, TotalValue);
                }

                //ROLLOVER
                if (SelectedIndex != index && !isDragging && _keyframe.state != KeyframeState.drag)
                {
                    if (_keyframe.IconRect.Contains(Event.current.mousePosition))
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
                if (Event.current.type == EventType.mouseUp || !DragLimit.Contains(Event.current.mousePosition) && isDragging)
                {
                    if (SelectedValue == _keyframe.Value)
                    {
                        _keyframe.state = KeyframeState.selected;
                        isDragging = false;
                        KeyframeSet.Sort(new ordenador());
                    }
                }

            }
            else
            {
                _keyframe.state = KeyframeState.normal;
                SelectedIndex = -1;
                //SelectedValue = 0;
            }

            //DRAW SLIDER
            _keyframe.draw();
        }


        TLkeyframe _keyframe_0 = (TLkeyframe)KeyframeSet[0];
        TLkeyframe _keyframe_1 = (TLkeyframe)KeyframeSet[1];

        if (_keyframe_0.Value < _keyframe_1.Value)
        {
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x - 1, TimeLineRect.y - 14, _keyframe_0.IconRect.width, _keyframe_0.IconRect.height), icoA);
            GUI.DrawTexture(new Rect(_keyframe_1.IconRect.x - 1, TimeLineRect.y - 14, _keyframe_1.IconRect.width, _keyframe_1.IconRect.height), icoB);
        }
        else
        {
            GUI.DrawTexture(new Rect(_keyframe_1.IconRect.x - 1, TimeLineRect.y - 14, _keyframe_1.IconRect.width, _keyframe_1.IconRect.height), icoA);
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x - 1, TimeLineRect.y - 14, _keyframe_0.IconRect.width, _keyframe_0.IconRect.height), icoB);
        }

        if (_keyframe_0.Value + _keyframe_1.Value != totalValue)
        {
            active = true;

            /*
            GUI.color = new Color(250f / 255f, 100f / 255f, 40f / 255f, 0.5f);
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x + 6, TimeLineRect.y + 1, (_keyframe_1.IconRect.x - _keyframe_0.IconRect.x), 39), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x + 6, TimeLineRect.y + 43, (_keyframe_1.IconRect.x - _keyframe_0.IconRect.x), 7), EditorGUIUtility.whiteTexture);
            */

            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(TimeLineRect.xMin, TimeLineRect.y + 1, (_keyframe_0.IconRect.x - TimeLineRect.xMin)+6, 39), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(_keyframe_1.IconRect.x + 6, TimeLineRect.y + 1, (TimeLineRect.xMax - _keyframe_1.IconRect.x)-6, 39), EditorGUIUtility.whiteTexture);

            GUI.color = new Color(130f / 255f, 170f / 255f, 30f / 255f, 0.2f);
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x + 6, TimeLineRect.y + 43, (_keyframe_1.IconRect.x - _keyframe_0.IconRect.x), 11), EditorGUIUtility.whiteTexture);

            GUI.color = new Color(130f/255f, 170f/255f, 30f/255f, 1f);
            GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x + 6, TimeLineRect.y + 41, (_keyframe_1.IconRect.x - _keyframe_0.IconRect.x), 2), EditorGUIUtility.whiteTexture);

        }
        else {
            active = false;
        }
        GUI.color = Color.white;

        //GUI.DrawTexture(new Rect(_keyframe_0.IconRect.x + 3, _keyframe_0.IconRect.y + 5, 2, 5), EditorGUIUtility.whiteTexture);
        //GUI.DrawTexture(new Rect(_keyframe_1.IconRect.x + 3, _keyframe_0.IconRect.y + 5, 2, 5), EditorGUIUtility.whiteTexture);
        //if (Refresh) GetWindow(typeof(TagarelaEditor)).Repaint();

    }

    public class TLkeyframe
    {

        private Vector2 drag_pos;
        Color seta_cor = Color.black;
        public bool repaint = false;
        public float Value, TotalValue;

        public Rect IconRect, TimeLineRect;

        public bool canDrag = false;

        public KeyframeState state = KeyframeState.normal;

        public TLkeyframe(float Value, float TotalValue, Rect TimeLineRect, Rect _IconRect)
        {
            this.Value = Value;
            this.TotalValue = TotalValue;
            this.TimeLineRect = TimeLineRect;
            IconRect = new Rect(0, TimeLineRect.y, _IconRect.width, _IconRect.height);
        }

        void Init()
        {
            //repaint = true;
        }
        public void draw()
        {
            repaint = false;

            //CENTER IMAGE
            float fix_timeline_gap = TimeLineRect.x - IconRect.width / 2;
            switch (state)
            {

                case KeyframeState.normal:
                    canDrag = false;
                    EditorGUIUtility.AddCursorRect(IconRect, MouseCursor.Link);
                    seta_cor = Color.white;
                    drag_pos.x = (Event.current.mousePosition.x - IconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.over:
                    canDrag = false;
                    EditorGUIUtility.AddCursorRect(IconRect, MouseCursor.Link);
                    seta_cor = Color.red;
                    drag_pos.x = (Event.current.mousePosition.x - IconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.selected:
                    canDrag = true;
                    EditorGUIUtility.AddCursorRect(IconRect, MouseCursor.Link);
                    seta_cor = Color.white;
                    drag_pos.x = (Event.current.mousePosition.x - IconRect.x) + fix_timeline_gap;
                    break;

                case KeyframeState.drag:
                    if (canDrag)
                    {
                        EditorGUIUtility.AddCursorRect(IconRect, MouseCursor.SlideArrow);
                        seta_cor = Color.red;
                        Value = ((Event.current.mousePosition.x - drag_pos.x) / TimeLineRect.width) * TotalValue;
                    }
                    break;
            }


            if (Value > TotalValue)
            {
                Value = TotalValue;
            }
            if (Value < 0) Value = 0f;

            IconRect.x = (Value / TotalValue) * TimeLineRect.width + fix_timeline_gap;
            //Debug.Log(IconRect.x);

            GUIContent img_content = new GUIContent();
            img_content.tooltip = Value.ToString();

            GUI.color = seta_cor;

            if (state != KeyframeState.drag) GUI.Label(new Rect(IconRect.x, IconRect.y, IconRect.width, IconRect.height), img_content);
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
            if (a.Value > b.Value)
                return 1;
            else if (a.Value < b.Value)
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
