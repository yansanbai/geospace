using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XANTools
{
    /// <summary>
    /// 拖拽移动UI的大小
    /// 是哟ing说明
    /// 1、点击 UI
    /// 2、拖拽UI，UI的大小就会更改变化
    /// 3、变化会在限定大小内变化
    /// </summary>
    public class PointerDragDragUIToResize : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("调整的最小值")]
        public Vector2 minSize = new Vector2(50, 50);
        [Header("调整的最大值")]
        public Vector2 maxSize = new Vector2(500, 500);

        // UI 的 RectTransform
        private RectTransform panelRectTransform;

        // 鼠标按下的时候鼠标位置转到UI的位置的大小
        private Vector2 originalLocalPointerPosition;

        // 鼠标按下的时候UI原来的大小
        private Vector2 originalSizeDelta;

        // 判断鼠标的点击位置，是左上还是右上(让变化合理（鼠标向内drag是变小，向外drag是变大）)
        private bool isUpLeft = false;

        void Awake()
        {
            panelRectTransform = transform.GetComponent<RectTransform>();

        }

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            // 记录按下鼠标变化前的大小
            originalSizeDelta = panelRectTransform.sizeDelta;

            // 记录按下鼠标的位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);

            // 判断鼠标的点击位置(对角线法)
            if ((originalLocalPointerPosition.x - originalLocalPointerPosition.y) < 0)
            {
                isUpLeft = true;

            }
            else
            {
                isUpLeft = false;

            }
        }

        /// <summary>
        /// 拖拽事件
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            // UI 的安全校验
            if (panelRectTransform == null)
                return;

            // Drag 变化的变量
            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out localPointerPosition);

            // Drag 变化值于鼠标按下拖拽前的值之间的差值
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;

            // UI RectTransform 差值变化 （注意左上和右下的区别对待）
            Vector2 sizeDelta = originalSizeDelta + new Vector2(offsetToOriginal.x, -offsetToOriginal.y) * (isUpLeft == true ? -1 : 1);
            sizeDelta = new Vector2(
                    Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                    Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
            );

            // 赋值，实现大小调整
            panelRectTransform.sizeDelta = sizeDelta;
        }


    }
}
