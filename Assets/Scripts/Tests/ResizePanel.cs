using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace XANTools
{
    /// <summary>
    /// 拖动边缘，调整UI大小
    /// 使用说明
    /// 1、鼠标移动到UI的边缘，点击鼠标向内或向外拖动，实现调整UI大小
    /// 2、这里变化的 UI 的比例，你可以根据需要需改为 UI 宽高
    /// </summary>
    public class ResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        /// <summary>
        /// 边缘枚举
        /// </summary>
        public enum UIEdge
        {
            None,
            Left,
            Right,
            Down
        }

        /// <summary>
        /// 画布变化的大小限制值
        /// </summary>
        [Header("Resize 变化大小限制值")]
        public Vector3 minSize = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 maxSize = new Vector3(3.0f, 3.0f, 3.0f);

        [Header("边缘判断使用")]
        // 画布，用于鼠标转UI位置使用
        public Canvas canvas;
        // 边缘的图片变量
        public Transform Right_Image;
        public Transform Down_Image;
        public Transform Left_Image;

        // UI RectTransform
        private RectTransform panelRectTransform;
        // UI 变化前原始鼠标位置
        private Vector2 originalLocalPointerPosition;
        // UI 变化前原始比例
        private Vector3 originalScale;
        // 左上右下的区别判断
        private bool isUpLeft = false;
        // Resize 的变化平滑值
        private float floatResizeSmooth = 30;

        // 边缘判断处的位置变量
        Vector2 v2PointEnter;

        // 鼠标是否按下
        private bool isPinterDown = false;
        // 当前鼠标大概在什么位置
        private UIEdge uiEdge = UIEdge.None;

        // 边缘位置判断距离
        private float floatEdgeDistance = 100;





        public UnityEvent OnTestEnevt;
        [SerializeField]
        private UnityEvent OnTestEnevt2;



        void Awake()
        {
            // 初始化
            panelRectTransform = transform.GetComponent<RectTransform>();

            // 隐藏边缘图标
            SetActiveEdgeImage(false);
        }

        /// <summary>
        /// 鼠标在UI上按下的事件
        /// </summary>
        /// <param name="data"></param>
        public void OnPointerDown(PointerEventData data)
        {
            // 鼠标按下
            isPinterDown = true;
            // UI 原始比例
            originalScale = this.transform.localScale;

            // 鼠标位置转为对应 UI的坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);

            // 左上右下的区别判断
            if ((originalLocalPointerPosition.x - originalLocalPointerPosition.y) < 0)
            {
                isUpLeft = true;
            }
            else
            {
                isUpLeft = false;
            }
            // Debug.Log(GetType() + "/OnPointerDown()/originalLocalPointerPosition : " + originalLocalPointerPosition);
        }

        /// <summary>
        /// 拖拽实现Resize
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {
            // 停止边缘判断协程
            StopAllCoroutines();
            if (uiEdge == UIEdge.None)
                return;

            if (panelRectTransform == null)
                return;

            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out localPointerPosition);
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            Vector3 sizeDelta = originalScale;

            // 根据边缘不同进行不同的变化（根据需要调整）
            switch (uiEdge)
            {
                case UIEdge.None:
                    break;

                case UIEdge.Left:
                case UIEdge.Right:
                    // 仅在 X 方向变化
                    sizeDelta = originalScale + new Vector3(offsetToOriginal.x, 0, 0) * (isUpLeft == true ? -1 : 1) * 0.01f;
                    sizeDelta = new Vector3(
                            Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                            Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y),
                            Mathf.Clamp(sizeDelta.z, minSize.z, maxSize.z)
                    );
                    break;
                case UIEdge.Down:
                    // 仅在 Y 上变化
                    sizeDelta = originalScale + new Vector3(0, -offsetToOriginal.y, 0) * (isUpLeft == true ? -1 : 1) * 0.01f;
                    sizeDelta = new Vector3(
                            Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                            Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y),
                            Mathf.Clamp(sizeDelta.z, minSize.z, maxSize.z)
                    );
                    break;
                default:
                    break;
            }


            //Debug.Log(GetType() + "/OnDrag()/sizeDelta : " + sizeDelta);
            // 赋值产生变化
            panelRectTransform.localScale = Vector3.Lerp(panelRectTransform.localScale, sizeDelta, Time.deltaTime * floatResizeSmooth);
        }



        /// <summary>
        /// 鼠标进入UI的事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {

            //Debug.Log(" eventData.position:" + eventData.position);

            //把鼠标的的位置转为对应UI上的位置（这里可以省略，因为没有用到）
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out v2PointEnter);

            // 开启边缘检测
            StartCoroutine(edgeJudge());

            //Debug.Log(GetType()+ "/ OnPointerEnter() / v2PointEnter " + v2PointEnter);
            //Debug.Log(GetType()+ "/ OnPointerEnter() / panelRectTransform.rect.width " + panelRectTransform.rect.width);
            //Debug.Log(GetType()+ "/ OnPointerEnter() / panelRectTransform.rect.height " + panelRectTransform.rect.height);

        }

        /// <summary>
        /// 鼠标退出UI的事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log(GetType() + "/ OnPointerExit() / 退出 " );
            if (isPinterDown == false)
            {
                StopAllCoroutines();
                SetActiveEdgeImage(false);
                uiEdge = UIEdge.None;
            }

        }


        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            isPinterDown = false;
            StopAllCoroutines();
            SetActiveEdgeImage(false);
            uiEdge = UIEdge.None;
        }

        /// <summary>
        /// 设置边缘画的显隐
        /// </summary>
        /// <param name="isActive"></param>
        private void SetActiveEdgeImage(bool isActive)
        {
            Right_Image.gameObject.SetActive(isActive);
            Down_Image.gameObject.SetActive(isActive);
            Left_Image.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 判断鼠标在 Panel 的那个边缘
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private UIEdge GetCurrentEdge(Vector2 pos)
        {

            if ((pos.x - (-panelRectTransform.rect.width/2)) < (floatEdgeDistance / panelRectTransform.localScale.x))
            {
                return UIEdge.Left;
            }
            else if ((pos.y - (-panelRectTransform.rect.height/2)) < (floatEdgeDistance / panelRectTransform.localScale.y))
            {
                return UIEdge.Down;
            }
            else if ((panelRectTransform.rect.width/2 - pos.x) < (floatEdgeDistance / panelRectTransform.localScale.x))
            {
                return UIEdge.Right;
            }

            return UIEdge.None;
        }

        /// <summary>
        /// 边缘判断协程
        /// </summary>
        /// <returns></returns>
        IEnumerator edgeJudge()
        {
            yield return new WaitForEndOfFrame();
            while (true)
            {

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera, out v2PointEnter);
                Debug.Log("_pos:" + v2PointEnter);

                uiEdge = GetCurrentEdge(v2PointEnter);
                Debug.Log("edge:" + uiEdge.ToString());

                SetActiveEdgeImage(false);

                switch (uiEdge)
                {
                    case UIEdge.None:
                        SetActiveEdgeImage(false);
                        break;
                    case UIEdge.Left:
                        Left_Image.gameObject.SetActive(true);
                        Left_Image.localPosition = new Vector3(Left_Image.localPosition.x, Mathf.Clamp(v2PointEnter.y, -panelRectTransform.rect.height / 2, panelRectTransform.rect.height / 2),
                            Left_Image.localPosition.z);
                        break;
                    case UIEdge.Right:
                        Right_Image.gameObject.SetActive(true);
                        Right_Image.localPosition = new Vector3(Right_Image.localPosition.x, Mathf.Clamp(v2PointEnter.y, -panelRectTransform.rect.height / 2, panelRectTransform.rect.height / 2),
                            Right_Image.localPosition.z);


                        break;
                    case UIEdge.Down:
                        Down_Image.gameObject.SetActive(true);
                        //Debug.Log("panelRectTransform.rect.width:" + panelRectTransform.rect.width);
                        Down_Image.localPosition = new Vector3(Mathf.Clamp(v2PointEnter.x, -panelRectTransform.rect.width / 2, panelRectTransform.rect.width / 2),
                            Down_Image.localPosition.y, Down_Image.localPosition.z);


                        break;
                    default:
                        SetActiveEdgeImage(false);
                        break;
                }
                yield return new WaitForEndOfFrame();
            }

        }
    }
}


