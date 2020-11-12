using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPanel : MonoBehaviour
{
    public Action OnCenterButtonClick;
    public Action OnZoomInButtonClick;
    public Action OnZoomOutButtonClick;
    public Action OnUpButtonClick;
    public Action OnDownButtonClick;

    private Button centerButton;
    private Button zoomInButton;
    private Button zoomOutButton;
    private Button upButtom;
    private Button downButtom;

    public void Init()
    {
        centerButton = transform.Find("Center/button").GetComponent<Button>();
        centerButton.onClick.AddListener(() =>
        {
            if (OnCenterButtonClick != null)
                OnCenterButtonClick();
        });


        zoomInButton = transform.Find("ZoomIn/button").GetComponent<Button>();
        zoomInButton.onClick.AddListener(() =>
        {
            if (OnZoomInButtonClick != null)
                OnZoomInButtonClick();
        });

        zoomOutButton = transform.Find("ZoomOut/button").GetComponent<Button>();
        zoomOutButton.onClick.AddListener(() =>
        {
            if (OnZoomOutButtonClick != null)
                OnZoomOutButtonClick();
        });

        upButtom = transform.Find("Up/button").GetComponent<Button>();
        upButtom.onClick.AddListener(() =>
        {
            if (OnUpButtonClick != null)
                OnUpButtonClick();
        });

        downButtom = transform.Find("Down/button").GetComponent<Button>();
        downButtom.onClick.AddListener(() =>
        {
            if (OnDownButtonClick != null)
                OnDownButtonClick();
        });

    }

    public void SetCenterButtonActive(bool active)
    {
        centerButton.interactable = active;
    }
}
