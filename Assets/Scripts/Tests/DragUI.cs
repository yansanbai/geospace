
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private GeoController geoController;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        geoController = GameObject.Find("GeoController").GetComponent<GeoController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("开始拖拽");
        //geoController.HandleClickLockButton(1);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //geoController.HandleClickLockButton(1);
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out pos);
        rectTransform.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("结束拖拽");
        //geoController.HandleClickLockButton(0);
    }

}
