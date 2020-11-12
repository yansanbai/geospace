using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WritingPanel : MonoBehaviour
{
    RectTransform penWrapper;
    RecognizePanel recognizePanel;
    NavPanel navPanel;
    PenBehaviour penBehaviour;

    public void Init(GeoUI geoUI, GeoController geoController)
    {
        Clear();

        RectTransform writingPanel = (RectTransform)transform;
        penWrapper = transform.Find("Wrapper").GetComponent<RectTransform>();
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, writingPanel.rect.width);
        penWrapper.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, writingPanel.rect.height);
        Debug.Log("writingPanel width:" + writingPanel.rect.width + ", height:" + writingPanel.rect.height);
        penBehaviour = transform.GetComponentInChildren<PenBehaviour>();
        penBehaviour.Init(geoUI, geoController);
        recognizePanel = geoUI.recognizePanel;
        navPanel = geoUI.navPanel;
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        if (recognizePanel != null)
            recognizePanel.Clear();
    }

    public void OpenWritingPanel(Geometry geometry)
    {
        gameObject.SetActive(true);
        StatusButton lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        lockButton.SetStatus(1);
        recognizePanel.showRecognizePanel();
        penBehaviour.SetDrawing(false);
        penBehaviour.SetGeometry(geometry);

        if (geometry is ResolvedBody)
        {
            ResolvedBody resolvedBody = (ResolvedBody)geometry;
            if (!resolvedBody.shapeSetted)
            {
                NavAxisBehaviour axis = GameObject.Find("X").GetComponent<NavAxisBehaviour>();
                PointerEventData data = new PointerEventData(EventSystem.current);
                axis.OnPointerClick(data);
                penBehaviour.SetDrawing(true);
                penBehaviour.SetGeometry(geometry);
                return;
            }
        }
    }

    public void OnDestroy()
    {
    }

}
