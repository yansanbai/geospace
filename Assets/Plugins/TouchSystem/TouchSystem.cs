using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSystem : MonoBehaviour
{
    public int maxTouches = 2;

    public bool canContinuousRecognize = true;

    public bool isGetMouse = false;

    // private
    private TSTouch[] touchesCache;
    private List<TSTouch> touches = new List<TSTouch>(2);
    private List<AbstractRecognizer> recognizers = new List<AbstractRecognizer>(5);
    private AbstractRecognizer triggeRecognizer;
    private bool canRecognize = true;
    
    void Awake()
    {
        touchesCache = new TSTouch[maxTouches];
        for (int i = 0; i < maxTouches; i++)
        {
            touchesCache[i] = new TSTouch(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateTouches();
    }

    private void OnApplicationQuit()
    {
        _instance = null;
        Destroy(gameObject);
    }

    #region Single
    private static TouchSystem _instance = null;
    public static TouchSystem instance
    {
        get
        {
            if (!_instance)
            {
                // check if available
                _instance = FindObjectOfType(typeof(TouchSystem)) as TouchSystem;

                // create a new one
                if (!_instance)
                {
                    var obj = new GameObject("TouchSystem");
                    _instance = obj.AddComponent<TouchSystem>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    #endregion

    #region Public
    public static void addRecognizer(AbstractRecognizer recognizer)
    {
        instance.recognizers.Add(recognizer);

        if (recognizer.zIndex > 0)
        {
            _instance.recognizers.Sort();
            _instance.recognizers.Reverse();
        }
    }

    public static void removeRecognizer(AbstractRecognizer recognizer)
    {
        if (_instance == null)
            return;

        if (!_instance.recognizers.Contains(recognizer))
        {
            Debug.LogError("Trying to remove recognizer that has not been added: " + recognizer);
            return;
        }

        recognizer.reset();
        instance.recognizers.Remove(recognizer);
    }

    public static void removeAllRecognizers()
    {
        if (_instance == null)
            return;

        instance.recognizers.Clear();
    }

    public static void resetRecoginer()
    {
        if (_instance == null)
            return;

        instance.triggeRecognizer = null;
        if (instance.canContinuousRecognize)
        {
            instance.canRecognize = true;
        }
    }

    #endregion

    #region Private
    private void updateTouches()
    {

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WEBGL
        if (isGetMouse)
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButton(0))
                touches.Add(touchesCache[0].updateByMouse());
#endif

        if (Input.touchCount > 0)
        {
            int touchLength = Mathf.Min(Input.touchCount, maxTouches);
            for (var i = 0; i < touchLength; i++)
            {
                Touch touch = Input.touches[i];
                if (touch.fingerId < maxTouches)
                {
                    touchesCache[i].updateByTouch(touch);
                    touches.Add(touchesCache[i]);
                }
            }
        }

        // recognizers
        if (touches.Count > 0 && recognizers.Count > 0)
        {

            if (canRecognize && triggeRecognizer == null)
            {
                for (var i = 0; i < recognizers.Count; i++)
                {
                    if (recognizers[i].canTrigger(touches))
                    {
                        triggeRecognizer = recognizers[i];
                        canRecognize = false;
                    }
                }
            }
            if (triggeRecognizer != null)
            {
                triggeRecognizer.recognizeTouches(touches);
            }
        }
        else
        {
            triggeRecognizer = null;
            canRecognize = true;
        }

        // post
        if (touches.Count > 0)
        {

            touches.Clear();
        }
    }
    #endregion
}