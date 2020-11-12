using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum KeyboardEventType
{
    keyDown,
    KeyUp,
    down
}

public class KeyboardSystem : MonoBehaviour
{
    private static bool applicationIsQuitting = false;

    #region Single


    private static KeyboardSystem _instance = null;
    public static KeyboardSystem instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            if (!_instance)
            {
                // check if available
                _instance = FindObjectOfType(typeof(KeyboardSystem)) as KeyboardSystem;

                // create a new one
                if (!_instance)
                {
                    var obj = new GameObject("KeyboardSystem");
                    _instance = obj.AddComponent<KeyboardSystem>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    #endregion

    Array allKeyCodes;

    private KeyEvent<KeyCode> onKey = new KeyEvent<KeyCode>();
    private KeyEvent<KeyCode> onKeyDown = new KeyEvent<KeyCode>();
    private KeyEvent<KeyCode> onKeyUp = new KeyEvent<KeyCode>();

    public class KeyEvent<KeyCode> : UnityEvent<KeyCode> { }

    void Awake()
    {
        allKeyCodes = System.Enum.GetValues(typeof(KeyCode));
    }

    void Update()
    {

        foreach (KeyCode tempKey in allKeyCodes)
        {

            if (Input.GetKeyDown(tempKey))
                sendEvent(tempKey, KeyboardEventType.keyDown);

            if (Input.GetKeyUp(tempKey))
                sendEvent(tempKey, KeyboardEventType.KeyUp);

            if (Input.GetKey(tempKey))
                sendEvent(tempKey, KeyboardEventType.down);

        }
    }

    void OnDestroy()
    {
        applicationIsQuitting = true;
    }

    public static KeyEvent<KeyCode> OnKey
    {
        get { return instance.onKey; }
        set { instance.onKey = value; }
    }

    public static KeyEvent<KeyCode> OnKeyUp
    {
        get { return instance.onKeyUp; }
        set { instance.onKeyUp = value; }
    }

    public static KeyEvent<KeyCode> OnKeyDown
    {
        get { return instance.onKeyDown; }
        set { instance.onKeyDown = value; }
    }

    void sendEvent(KeyCode keycode, KeyboardEventType evType)
    {

        if (evType == KeyboardEventType.keyDown)
            OnKey.Invoke(keycode);
        if (evType == KeyboardEventType.KeyUp)
            OnKeyUp.Invoke(keycode);
        if (evType == KeyboardEventType.down)
            OnKeyDown.Invoke(keycode);
    }
}

