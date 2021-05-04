using System;
using TexDrawLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(TEXDraw), typeof(TEXInputCursor), typeof(TEXInputLogger))]
[AddComponentMenu("TEXDraw/TEXInput")]
public class TEXInput : Selectable, IUpdateSelectedHandler, IDragHandler
{

    /// <summary>
    /// Starting range of selection. Changing this require texdraw.SetTextDirty(true) to be called
    /// </summary>
#if !TEXDRAW_DEBUG
    [HideInInspector]
#endif
    public int selectionStart;

    /// <summary>
    /// Length range of selection. Changing this require texdraw.SetTextDirty(true) to be called
    /// </summary>
#if !TEXDRAW_DEBUG
    [HideInInspector]
#endif
    public int selectionLength;

    [NonSerialized]
    public bool selectionReversed;

    /// <summary>
    /// Internal selection, refer when 'MouseDown'
    /// </summary>
    protected int selectionBegin
    {
        get
        {
            return selectionReversed ? selectionStart + selectionLength : selectionStart;
        }
        set
        {
            selectionStart = value;
            // this is beginning, right? so...
            selectionLength = 0;
            selectionReversed = false;
            cursor.hotState = false;
        }
    }

    /// <summary>
    /// Internal selection, refer during 'MouseMove'
    /// </summary>
    protected int selectionEnd
    {
        get
        {
            return selectionReversed ? selectionStart : selectionStart + selectionLength;
        }
        set
        {
            if ((value >= selectionBegin) ^ (selectionEnd >= selectionBegin))
                selectionReversed = !selectionReversed;

            if (selectionReversed)
            {
                selectionLength = selectionBegin - value;
                selectionStart = value;
            }
            else
            {
                selectionLength = value - selectionStart;
            }
            cursor.hotState = selectionLength > 0;
        }
    }

    /// <summary>
    /// Is the content can't be modified by user?
    /// </summary>
    [Tooltip("Is the content can't be modified?")]
    public bool readOnly = false;

    /// <summary>
    /// Do users should not be allowed to type TEXDraw specific syntax?
    /// </summary>
    [Tooltip("Do users should not be allow to type TEXDraw specific syntax?")]
    public bool escapeInput = true;

    /// <summary>
    /// Do users should not be allowed to alter TEXDraw specific syntax?
    /// </summary>
    [Tooltip("Do users should not be allowed to alter TEXDraw specific syntax?")]
    public bool escapeNavigation = true;

    [SerializeField]
    private TEXInputChangeEvent m_OnChange = new TEXInputChangeEvent();

    /// <summary>
    /// Event to whenever the text has changed by user
    /// </summary>
    public TEXInputChangeEvent onChange
    {
        get { return m_OnChange; }
        set { m_OnChange = value; }
    }

    /// <summary>
    /// Text that displayed, which is exact same as TEXDraw
    /// </summary>
    public string text { get { return tex.text; } set { tex.text = value; } }

    /// <summary>
    /// Portion of text that displayed
    /// </summary>
    public string selectedText
    {
        get
        {
            CheckSelection();
            return text.Substring(selectionStart, selectionLength);
        }
        set
        {
            CheckSelection();
            var start = selectionStart;
            var end = selectionLength;
            selectionLength = start + value.Length;
            selectionReversed = false;
            text = text.Substring(0, start) + value + text.Substring(start + end);
        }
    }

    /// <summary>
    /// Is selectionLength > 0?
    /// </summary>
    public bool hasSelection { get { return selectionLength > 0; } }

    internal TEXInputLogger logger { get { return GetComponent<TEXInputLogger>(); } }

    internal TEXInputCursor cursor { get { return GetComponent<TEXInputCursor>(); } }

    internal TEXDraw tex { get { return GetComponent<TEXDraw>(); } }

    private bool allowInput = false;

    /// <summary>
    /// Does the Event System put its focus to this input?
    /// </summary>
    public bool hasFocus { get { return allowInput; } }

    private Event m_ProcessingEvent = new Event();

    private void CheckSelection()
    {
        if (selectionStart + selectionLength > text.Length)
        {
            selectionStart = System.Math.Min(text.Length, selectionStart);
            selectionLength = System.Math.Min(text.Length, selectionStart + selectionLength) - selectionStart;
        }
    }

    // EVENTS =========

    public override void OnSelect(BaseEventData eventData)
    {
        allowInput = true;
        base.OnSelect(eventData);
        tex.SetTextDirty(true);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        allowInput = false;
        base.OnDeselect(eventData);
        tex.SetTextDirty(true);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        MousePointed(true, eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        MousePointed(false, eventData.position);
        base.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MousePointed(false, eventData.position);
    }

    public virtual void OnUpdateSelected(BaseEventData eventData)
    {

        if (allowInput && interactable)
        {
            var lastText = text;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                EventType type = m_ProcessingEvent.type;
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    if (!KeyPressed(m_ProcessingEvent))
                    {
                        allowInput = false;
                        var x = FindSelectableOnRight() ?? FindSelectableOnDown();
                        if (x)
                            EventSystem.current.SetSelectedGameObject(x.gameObject);
                        break;
                    }
                    else
                    {
                        cursor.hotState = cursor.hotState;
                        tex.SetTextDirty(true);
                    }
                }
                else if (type == EventType.ValidateCommand || type == EventType.ExecuteCommand)
                {
                    string commandName = m_ProcessingEvent.commandName;
                    switch (commandName)
                    {
                        case "SelectAll":
                            selectionBegin = 0;
                            selectionEnd = text.Length;
                            tex.SetTextDirty(true);
                            break;
                        case "UndoRedoPerformed":
                            if (Undo())
                                tex.SetTextDirty(true);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (text != lastText)
            {
                m_OnChange.Invoke(text);
            }
            eventData.Use();
        }
    }

    public void MousePointed(bool begin, Vector2 p)
    {
        if (logger.blocks.Count == 0) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, p, null, out p);

        var x = logger.GetNearestPosition(p);

        if (begin)
            selectionBegin = x;
        else
            selectionEnd = x;

        tex.SetTextDirty(true);
    }

    // true if continue
    protected bool KeyPressed(Event evt)
    {
        var shift = evt.shift;
        if (evt.control | evt.command)
        {
            switch (evt.keyCode)
            {
                case KeyCode.A:
                    RecordUndo();
                    selectionBegin = 0;
                    selectionEnd = selectionLength;
                    return true;
                case KeyCode.C:
                    GUIUtility.systemCopyBuffer = selectedText;
                    return true;
                case KeyCode.X:
                    GUIUtility.systemCopyBuffer = selectedText;
                    if (!readOnly)
                    {
                        RecordUndo();
                        selectedText = "";
                    }
                    return true;
                case KeyCode.V:
                    if (!readOnly)
                    {
                        RecordUndo();
                        selectedText = GUIUtility.systemCopyBuffer;
                    }
                    return true;
                default:
                    return true;
            }
        }
        switch (evt.keyCode)
        {
            case KeyCode.Backspace:
                if (!readOnly)
                {
                    RecordUndo();
                    Backspace(escapeNavigation);
                }
                return true;
            case KeyCode.Delete:
                if (!readOnly)
                {
                    RecordUndo();
                    Delete(escapeNavigation);
                }
                return true;
            case KeyCode.LeftArrow:
                Move(shift, MoveLeft);
                return true;
            case KeyCode.RightArrow:
                Move(shift, MoveRight);
                return true;
            case KeyCode.UpArrow:
                Move(shift, MoveUp);
                return true;
            case KeyCode.DownArrow:
                Move(shift, MoveDown);
                return true;
            case KeyCode.Tab:
            case KeyCode.Escape:
                return false;
            default:
                if (!readOnly)
                {
                    char c = evt.character;
                    if (c == '\0')
                    {
                        return true;
                    }
                    if (c == '\r' || c == '\u0003')
                    {
                        c = '\n';
                    }
                    RecordUndo();
                    Append(c);
                }
                return true;
        }
    }

    private void Append(char ch)
    {
        var s = escapeInput ? Escape(ch) : new string(ch, 1);
        selectedText = s;
        selectionBegin += s.Length;
        selectionReversed = false;
    }

    readonly static char[] brackets = new char[] { '{', '}' };

    private void Backspace(bool escape)
    {
        if (hasSelection)
        {
            selectedText = "";
            selectionBegin = selectionStart;
        }
        else if (selectionStart > 0)
        {
            if (!escape)
            {
                text = text.Remove(selectionBegin - 1, 1);
                selectionBegin--;
                return;
            }
            if (AttemptToMatchBrackets(true))
                return;
            var closest = logger.GetBlockBefore(selectionStart);
            if (closest.start >= selectionStart)
            {
                if (closest.index <= 0)
                    closest = new Block() { start = 0, length = selectionStart };
                else
                    closest = logger.GetPrevious(closest);
            }
            var length = selectionStart - closest.start;
            if (length > closest.length)
            {
                // try to not kill }, {
                var el = text.IndexOfAny(brackets, closest.end, selectionStart - closest.end);
                length = el >= 0 ? selectionStart - el - 1 : length - closest.length;
            }
            text = text.Remove(selectionStart - length, length);
            selectionBegin -= length;
        }
    }

    private void Delete(bool escape)
    {
        if (hasSelection)
        {
            selectedText = "";
            selectionBegin = selectionStart;
        }
        else if (selectionStart < text.Length)
        {
            if (!escape)
            {
                text = text.Remove(selectionBegin, 1);
                return;
            }
            if (AttemptToMatchBrackets(false))
                return;
            var closest = logger.GetBlockBefore(selectionStart);
            if (closest.index == -1)
            {
                closest = logger.GetNext(closest);
                closest.length = 0;
            }
            else if (closest.start < selectionStart)
            {
                if (closest.index >= logger.blocks.Count - 1)
                    closest = new Block() { start = selectionStart, length = text.Length - selectionStart };
                else
                    closest = logger.GetNext(closest);
            }
            var length = closest.end - selectionStart;
            if (length > closest.length)
            {
                // try to not kill }, {, ^, _, \
                var el = text.IndexOfAny(TexFormulaParser.preservedCharacters, selectionStart + 1, closest.end - selectionStart - 1);
                length = el >= 0 ? el - selectionStart : length - closest.length;
            }
            text = text.Remove(selectionStart, length);
        }
    }

    private void Move(bool shift, Func<int, int> mover)
    {
        var x = shift | hasSelection ? selectionEnd : selectionBegin;

        if (shift)
            selectionEnd = mover(x);
        else
            selectionBegin = mover(x);
    }

    private int MoveLeft(int x)
    {
        if (!escapeNavigation)
            return System.Math.Max(x - 1, 0);

        var closest = logger.GetBlockBefore(x);
        while (x > 0)
        {
            x--;
            if (closest.start > x)
                closest = logger.GetPrevious(closest);
            if (x == closest.start || x == closest.start + closest.length)
                break;
        }
        return x;
    }

    private int MoveRight(int x)
    {
        if (!escapeNavigation)
            return System.Math.Min(x + 1, text.Length);

        var closest = logger.GetBlockBefore(x);
        while (x < text.Length)
        {
            x++;
            if (closest.start + closest.length < x)
                closest = logger.GetNext(closest);
            if (x == closest.start || x == closest.start + closest.length)
                break;
        }
        return x;
    }

    private int MoveUp(int x)
    {
        var up = text.Take(x);
        var line = up.Count(c => c == '\n');
        if (line == 0) return x;

        var lines = up.Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').Select(cc => cc.i);
        var dist = x - lines.LastOrDefault();
        var pos = lines.ElementAtOrDefault(line - 2) + dist;
        if (escapeNavigation)
            return logger.GetBlockBefore(pos).start;
        else
            return pos;
    }

    private int MoveDown(int x)
    {
        var down = text.Skip(x);
        var line = down.Count(c => c == '\n');
        if (line == 0) return x;

        var lines = text.Take(x).Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').Select(cc => cc.i);
        var dist = x - (lines.LastOrDefault());
        var pos = x + down.Select((c, i) => new { c, i }).Where(cc => cc.c == '\n').First().i + dist;
        if (escapeNavigation)
            return logger.GetBlockBefore(pos).start;
        else
            return pos;
    }

    private struct UndoState
    {
        public string text;
        int selectionStart;
        int selectionLength;

        public UndoState(TEXInput input)
        {
            text = input.text;
            selectionStart = input.selectionStart;
            selectionLength = input.selectionLength;
        }

        public void Apply(TEXInput input)
        {
            input.text = text;
            input.selectionStart = selectionStart;
            input.selectionLength = selectionLength;
        }

        public static bool IsEqual(UndoState l, UndoState r)
        {
            return l.text == r.text && l.selectionLength == r.selectionLength
                && l.selectionStart == r.selectionStart;
        }
    }

    [NonSerialized]
    private List<UndoState> undoStack = new List<UndoState>();

    /// <summary>
    /// Record undo
    /// </summary>
    public void RecordUndo()
    {
        var st = new UndoState(this);
        if (undoStack.Count == 0 || !UndoState.IsEqual(st, undoStack[undoStack.Count - 1]))
        {
            undoStack.Add(st);
            // maybe the cap is too subjective
            if (undoStack.Count > 15)
                undoStack.RemoveAt(0);
        }
    }

    private bool Undo()
    {
        if (undoStack.Count > 0)
        {
            var now = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);

            if (now.text == text)
                return Undo();
            else
                now.Apply(this);
            return true;
        }
        return false;
    }

    private static Regex _escaper = new Regex(@"([\\{}^_])");

    public static string Escape(string s)
    {
        return _escaper.Replace(s, "\\$1");
    }

    public static string Escape(char s)
    {
        return TexFormulaParser.IsParserReserved(s) ? "\\" + s : new string(s, 1);
    }
    
    static readonly Regex _commandPattern = new Regex(@"(?:\\[A-Za-z]+|[\^_]{1,3})?(?:\[[^\[\]]*?\])?$");
    static readonly Regex _commandPattern2 = new Regex(@"\G(?:\\[A-Za-z]+|[\^_]{1,3})?(?:\[[^\[\]]*?\])?{");

    private bool AttemptToMatchBrackets(bool backspace)
    {
        CheckSelection();

        if (backspace && selectionStart > 0)
        {
            var c = text[selectionStart - 1];
            if (c == '{')
            {
                var m = LookupBracePairAhead(selectionStart - 1);
                if (m != selectionStart - 1)
                {
                    var s = selectionStart > 1 ? _commandPattern.Match(text, 0, selectionStart - 1).Length : 0;
                    var d = m - selectionStart;
                    selectionBegin -= s + 1;
                    selectionLength = d + 1 + s;
                }
                else
                {
                    selectionBegin--;
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
            else if (c == '}')
            {
                var m = LookupBracePairBehind(selectionStart - 1);
                if (m != selectionStart - 1)
                {
                    var s = m > 0 ? _commandPattern.Match(text, 0, m).Length : 0;
                    var d = selectionStart - m;
                    selectionBegin -= d + s;
                    selectionLength = d + s;
                }
                else
                {
                    selectionBegin--;
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
        }
        else if (!backspace && selectionStart < text.Length)
        {
            var c = text[selectionStart];
            if (c == '}')
            {
                var m = LookupBracePairBehind(selectionStart);
                if (m != selectionStart)
                {
                    var s = m > 0 ? _commandPattern.Match(text, 0, m).Length : 0;
                    var d = selectionStart - m;
                    selectionBegin -= d + s;
                    selectionLength = d + s + 1;
                }
                else
                {
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
            else if (c == '{' || c == '\\' || c == '^' || c == '_')
            {
                var q = 0;
                if (c != '{')
                {
                    var g = _commandPattern2.Match(text, selectionStart);
                    if (!g.Success) return false;
                    q = g.Length - 1;
                }
                var m = LookupBracePairAhead(selectionStart + q);
                if (m != selectionStart + q)
                {
                    var s = selectionStart > 2 ? _commandPattern.Match(text, 0, selectionStart - 1).Length : 0;
                    var d = m - selectionStart;
                    selectionBegin -= s;
                    selectionLength = d + s;
                }
                else
                {
                    text = text.Remove(selectionBegin, 1);
                }
                return true;
            }
        }
        return false;
    }

    private int LookupBracePairAhead(int position)
    {
        var t = text;
        var start = position;
        var shift = 0;
        do
        {
            if (position >= t.Length) return start;
            var c = t[position++];
            if (c == '\\') position++;
            else if (c == '{') shift++;
            else if (c == '}') shift--;
            else if (c == '\n') return start;
        } while (shift != 0);
        return position;
    }

    private int LookupBracePairBehind(int position)
    {
        var t = text;
        var start = position;
        var shift = 0;
        do
        {
            if (position < 0) return start;
            var c = t[position--];
            if (c == '\n') return start;
            else if (position >= 0 && t[position] == '\\')
            {
                var cont = 0;
                while (position > 0 && t[--position] == '\\')
                    cont++;
                if (t[position] == '\n')
                    return start;
                if (cont % 2 == 1) continue;
            }
            else if (c == '{') shift++;
            else if (c == '}') shift--;
        } while (shift != 0);
        return position + 1;
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        tex.SetTextDirty(true);
    }

    protected override void Reset()
    {
        base.Reset();
        transition = Transition.None;
    }
#endif

    [Serializable]
    internal struct Block
    {
        public int index;
        public int start;
        public int length;
        public int lineSeparator;

        public override string ToString()
        {
            return index + ":" + start + "-" + length;
        }

        public int end { get { return start + length; } }
    }
}

[Serializable]
public class TEXInputChangeEvent : UnityEvent<string>
{
}
