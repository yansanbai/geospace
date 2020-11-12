using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class StatusButton : MonoBehaviour
{
    public Sprite[] Images;
    public Action<int, int> OnStatusChange;

    Button button;
    // Image image;
    int status;
    float statusCount;

    public void Init()
    {
        button = GetComponent<Button>();
        // image = GetComponent<Image>();
        statusCount = Images.Length;

        button.onClick.AddListener(HandleClick);

    }

    public void HandleClick()
    {
        int next = NextStatus();
        SetStatus(next);
    }

    public void SetStatus(int next)
    {
        if (OnStatusChange != null)
            OnStatusChange(status, next);
        StatusChanged(next);
    }

    private int NextStatus()
    {
        if (status == statusCount - 1)
            return 0;
        return status + 1;
    }

    private bool StatusChanged(int s)
    {
        if (s < 0 || s >= statusCount)
            return false;

        status = s;
        button.image.sprite = Images[status];

        return true;
    }
}
