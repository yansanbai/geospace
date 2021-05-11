using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


public enum GeoState
{
    Normal,
    MoveVertex,
    EditVertex,
    EditEdge,
    EditFace,
    Condition,
    Auxiliary,
    Measure,
    Writing
}

public class GeoUI
{
    public NavPanel navPanel;
    public ToolPanel toolPanel;
    public StatePanel statePanel;
    public InputPanel inputPanel;
    public ElementPanel elementPanel;
    public ActivePanel activePanel;
    public CameraPanel cameraPanel;
    public WritingPanel writingPanel;
    public RecognizePanel recognizePanel;
}


public class GeoController : MonoBehaviour
{

    GeoUI geoUI;//UI
    Geometry geometry;//主物体
    GeoState state;
    GeometryBehaviour geometryBehaviour;
    NavigationBehaviour navigationBehaviour;//导航
    GridBehaviour gridBehaviour;
    CoordinateBehaviour coordinateBehaviour;

    GeoCamera geoCamera;
    NavCamera navCamera;

    StateController stateController;
    ToolController toolController;
    RecognizeController recognizeController;

    bool isCameraRotate;
    bool canCameraRotate;

    bool isSnapToGrid;
    GeometryShadeType shadeType;

    Operation currentOperation;

    const int MIN_WINDOW_WIDTH = 800;
    const int MIN_WINDOW_HEIGHT = 600;

    void Start()
    {
        // 检测分辨率
        int screen_width = UnityEngine.Screen.width;
        int screen_height = UnityEngine.Screen.height;
        Debug.Log("屏幕宽："+screen_width);
        Debug.Log("屏幕高："+screen_height);
        if(screen_width < MIN_WINDOW_WIDTH && screen_height < MIN_WINDOW_HEIGHT){
            Debug.Log("当前分辨率不符合要求");
            // TODO: 弹出对话框：“分辨率不符合要求，即将退出”
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        navigationBehaviour = GameObject.Find("/3D/Navigation").GetComponent<NavigationBehaviour>();
        gridBehaviour = GameObject.Find("/3D/Grid").GetComponent<GridBehaviour>();
        coordinateBehaviour = GameObject.Find("/3D/Coordinate").GetComponent<CoordinateBehaviour>();

        geoCamera = GameObject.Find("/3D/GeoCamera").GetComponent<GeoCamera>();
        navCamera = GameObject.Find("/3D/NavCamera").GetComponent<NavCamera>();

        InitTouchSystem();
        InitView();
        InitUI();

        geoCamera.InitDefault();

        stateController = GetComponent<StateController>();
        stateController.Init(geoUI.statePanel);

        toolController = GetComponent<ToolController>();
        toolController.Init(geoUI.toolPanel);

        recognizeController = GetComponent<RecognizeController>();
        recognizeController.Init(geoUI.writingPanel);

        ClearGeometry(3);
    }

    void InitUI()
    {
        geoUI = new GeoUI();

        Transform canvasBack = GameObject.Find("/UI/CanvasBack").transform;
        Transform canvasFront = GameObject.Find("/UI/CanvasFront").transform;

        NavPanel navPanel = canvasBack.Find("NavPanel").GetComponent<NavPanel>();
        navPanel.OnWritingButtonClick = HandleClickWritingButton;
        navPanel.OnShadeButtonClick = HandleClickShadeButton;
        navPanel.OnLockButtonClick = HandleClickLockButton;
        navPanel.OnDisplayButtonClick = HandleClickDisplayButton;
        navPanel.OnCoordinateButtonClick = HandleClickCoordinateButton;
        navPanel.OnGridButtonClick = HandleClickGridButton;
        //Speech Recognition
        navPanel.OnSnapButtonClick = HandleClickSnapButton;
        navPanel.OnVoiceButtonClick = HandleClickVoiceButton;
        geometryBehaviour.OnElementDisplayChange = HandleElementDisplayChange;
        navPanel.Init();

        ToolPanel toolPanel = canvasBack.Find("ToolPanel").GetComponent<ToolPanel>();
        toolPanel.Init();
        toolPanel.OnClickTool = HandleClickTool;

        StatePanel statePanel = canvasBack.Find("StatePanel").GetComponent<StatePanel>();
        statePanel.Init();

        InputPanel inputPanel = canvasBack.Find("InputPanel").GetComponent<InputPanel>();
        inputPanel.Init();

        ElementPanel elementPanel = canvasFront.Find("ElementPanel").GetComponent<ElementPanel>();
        elementPanel.Init();

        ActivePanel activePanel = canvasFront.Find("ActivePanel").GetComponent<ActivePanel>();
        activePanel.Init();

        RecognizePanel recognizePanel = canvasFront.Find("RecognizePanel").GetComponent<RecognizePanel>();
        recognizePanel.Init();

        CameraPanel cameraPanel = canvasFront.Find("CameraPanel").GetComponent<CameraPanel>();
        cameraPanel.OnCenterButtonClick = HandleClickCenterButton;
        cameraPanel.OnZoomInButtonClick = HandleClickZoomInButton;
        cameraPanel.OnZoomOutButtonClick = HandleClickZoomOutButton;
        cameraPanel.OnUpButtonClick = HandleClickUpButton;
        cameraPanel.OnDownButtonClick = HandleClickDownButton;
        cameraPanel.Init();
        cameraPanel.SetCenterButtonActive(false);

        geoUI.navPanel = navPanel;
        geoUI.toolPanel = toolPanel;
        geoUI.statePanel = statePanel;
        geoUI.inputPanel = inputPanel;
        geoUI.elementPanel = elementPanel;
        geoUI.activePanel = activePanel;
        geoUI.cameraPanel = cameraPanel;
        geoUI.recognizePanel = recognizePanel;

        WritingPanel writingPanel = canvasFront.Find("WritingPanel").GetComponent<WritingPanel>();
        writingPanel.Init(geoUI, this);
        geoUI.writingPanel = writingPanel;
    }

    void InitTouchSystem()
    {
        // Pan
        PanRecognizer panRecognizer = new PanRecognizer();

        panRecognizer.gestureCanTrigger = () =>
        {
            bool isInViewport = geoCamera.IsInViewport();
            if (!isInViewport)
                return false;

            if (state == GeoState.Normal)
                return canCameraRotate;
            if (currentOperation != null)
                return currentOperation.CanRotateCamera;
            else
                return false;
        };
        panRecognizer.gestureBeginEvent += (r) =>
        {
            isCameraRotate = true;
            geoUI.activePanel.Clear();
        };

        panRecognizer.gestureRecognizedEvent += (r) =>
        {
            Vector2 delta = -(r.deltaPosition) * 0.1f;
            geoCamera.Rotate(delta);
        };

        panRecognizer.gestureEndEvent += (r) =>
        {
            isCameraRotate = false;
        };

        TouchSystem.addRecognizer(panRecognizer);
    }

    void InitView()
    {
        geoCamera.Init();
        navCamera.Init(geoCamera);
        geometryBehaviour.Init(geoCamera);
        navigationBehaviour.Init(geoCamera);
        gridBehaviour.Init(geoCamera, 10);
        coordinateBehaviour.Init(geoCamera, 10);
    }

    private void SetState(GeoState newState)
    {
        state = newState;
    }

    public void EndWriting()
    {
        SetState(GeoState.Normal);
    }

    public GeoState GetState()
    {
        return state;
    }

    public bool IsCameraRotate()
    {
        return isCameraRotate;
    }

    public void ClearGeometry(int dimension)
    {

        this.geometry = null;

        toolController.SetIsGeometry(false);
        toolController.SetToolType(GeometryType.Common);
        toolController.RefreshToolPanel();

        stateController.ClearStates();

        geometryBehaviour.Clear(dimension);
    }

    public void SetGeometry(Geometry geometry)
    {
        if (geometry.Type == GeometryType.Function)
        {
            ClearGeometry(2);
            coordinateBehaviour.AxisZHide();
        }
        else {
            ClearGeometry(3);
            coordinateBehaviour.AxisZShow();
        }

        this.geometry = geometry;

        geometryBehaviour.InitGeometry(geometry);
        geometryBehaviour.SetShadeType(shadeType);

        toolController.SetIsGeometry(true);
        toolController.SetToolType(geometry.Type);
        toolController.RefreshToolPanel();

        geoUI.navPanel.SetDisplayButtonStatus(0);
    }

    public void RefreshGeometryBehaviour()
    {
        geometryBehaviour.UpdateElements();
        geometryBehaviour.UpdateSignsPosition();
    }

    public void RemoveAuxiliary(Auxiliary auxiliary)
    {

        bool result = geometry.Assistor.RemoveAuxiliary(auxiliary);

        if (result)
        {
            stateController.RemoveAuxiliaryState(auxiliary);

            VertexUnit[] units = auxiliary.units;
            GeoElement[] elements = auxiliary.elements;

            foreach (GeoElement element in elements)
                geometry.RemoveElement(element);

            foreach (VertexUnit unit in units)
                geometry.RemoveVertexUnit(unit.id);

            foreach (GeoElement element in elements)
                geometryBehaviour.RemoveElement(element);

            foreach (VertexUnit unit in units)
                geometryBehaviour.RemoveSign(unit.id);

            Gizmo[] gizmos = auxiliary.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }

        }
    }

    public void RemoveMeasure(Measure measure)
    {
        bool result = geometry.Implement.RemoveMeasure(measure);

        if (result)
        {
            stateController.RemoveMeasureState(measure);

            Gizmo[] gizmos = measure.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }
        }
    }

    public void RemoveCondition(Condition condition)
    {
        bool result = geometry.Constructor.RemoveCondition(condition);
        if (result)
        {
            stateController.RemoveConditionState(condition);

            Gizmo[] gizmos = condition.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.RemoveGizmo(gizmo);
                    geometryBehaviour.RemoveGizmo(gizmo);
                }
            }
            if (condition is FunctionCondition) {
                Function geo = (Function)geometry;
                FunctionCondition func = (FunctionCondition)condition;
                geometryBehaviour.RemoveElement(geo.Getline()[func.index-1]);
            }
        }
    }

    #region EventHandle

    public void HandleClickCenterButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(false);
        geoCamera.TriggerCenterZMAnimation();
    }

    public void HandleClickZoomInButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerZoomAnimation(-0.5f);
    }

    public void HandleClickZoomOutButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerZoomAnimation(0.5f);
    }

    public void HandleClickUpButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerMoveYAnimation(0.5f);
    }

    public void HandleClickDownButton()
    {
        geoUI.cameraPanel.SetCenterButtonActive(true);
        geoCamera.TriggerMoveYAnimation(-0.5f);
    }

    public void HandleClickTool(ToolGroupType type, Tool tool)
    {
        switch (type)
        {
            case ToolGroupType.Geometry:
                GeometryOperation(tool);
                break;
            case ToolGroupType.Condition:
                AddConditionOperation(tool);
                break;
            case ToolGroupType.Auxiliary:
                AddAuxiliaryOperation(tool);
                break;
            case ToolGroupType.Measure:
                AddMeasureOperation(tool);
                break;
        }
    }
    public void HandleClickWritingButton(int i)
    {
        Debug.Log("按下书写按钮");
        if (geoUI.writingPanel != null)
        {
            if (i == 1)
            {
                Debug.Log(state);
                currentOperation = new WritingOperation(this, geometry);
                currentOperation.Start();

                geoUI.writingPanel.OpenWritingPanel(geometry);

                SetState(GeoState.Writing);
            }
            else
            {
                SetState(GeoState.Normal);
                geoUI.writingPanel.Clear();
            }
        }
    }

    public void HandleClickShadeButton(int i)
    {

        shadeType = (GeometryShadeType)i;
        geometryBehaviour.SetShadeType(shadeType);
    }

    public void HandleClickLockButton(int i)
    {
        canCameraRotate = i == 0;
    }

    public void HandleClickDisplayButton(int i)
    {
        if (i == 0)
            geometryBehaviour.GeometryShowAllElements();
    }

    public void HandleClickCoordinateButton(int i)
    {
        coordinateBehaviour.gameObject.SetActive(i == 0);
    }

    public void HandleClickGridButton(int i)
    {
        gridBehaviour.gameObject.SetActive(i == 0);
    }

    public void HandleClickSnapButton(int i)
    {
        isSnapToGrid = i == 0;
    }

    public void HandleClickVoiceButton(int i)
    {
        // Debug.Log("Speech Recognition");
        // geoUI.navPanel.SetSpeechButtonStatus(1);
    }

    public void HandleElementDisplayChange(int i)
    {
        geoUI.navPanel.SetDisplayButtonStatus(i > 0 ? 1 : 0);
    }

    public string HandleRecognizeResult(string base64)
    {
        return recognizeController.GetRecognizeResult(base64);
    }

    public void HandleRecognizeFomula(string base64)
    {
        recognizeController.GetRecognizeFomula(base64);
    }

    public void HandleGetPosition(string latex)
    {
        recognizeController.GetPositon(latex);
    }

    public void HandleRecognizeChange(string latex)
    {
        Function func = (Function)this.geometry;
        string currentfomula = func.Getfomula();
        //string currentfomula = "y=x+1";
        recognizeController.GetRecognizeChange(latex, currentfomula);
    }

/*    public string HandleRecognizeTick(string base64)
    {
        return recognizeController.GetRecognizeTick(base64);
    }*/

    public void ClickVertex(GeoVertex vertex)
    {
        if (currentOperation == null)
            EditVertexOperation(vertex);
        else
            currentOperation.OnClickElement(vertex);
    }

    public void ClickEdge(GeoEdge edge)
    {
        if (currentOperation == null)
            EditEdgeOperation(edge);
        else
            currentOperation.OnClickElement(edge);
    }

    public void ClickFace(GeoFace face)
    {
        if (currentOperation == null)
            EditFaceOperation(face);
        else
            currentOperation.OnClickElement(face);
    }

    public void ClickSpreadFace(GeoFace face)
    {
        int color = face.color == 0 ? 1 : 0;
        geometry.SetSpreadFaceStyle(face, color);
        geometryBehaviour.GeometryElementColorChange(face, color);
    }

    public void HoverVertex(GeoVertex vertex, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover)
        {
            FormElement formElement = VertexForm(vertex);
            geoUI.activePanel.SetVertex(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightVertex(vertex, isHover);
    }

    public void HoverEdge(GeoEdge edge, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover)
        {
            FormElement formElement = EdgeForm(edge);
            geoUI.activePanel.SetEdge(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightEdge(edge, isHover);
    }

    public void HoverFace(GeoFace face, bool isHover)
    {
        if (isCameraRotate)
            return;
        if (currentOperation != null && !currentOperation.CanActiveElement)
            return;
        if (isHover && face.faceType == FaceType.Normal)
        {
            FormElement formElement = FaceForm(face);
            geoUI.activePanel.SetFace(formElement);
        }
        else
            geoUI.activePanel.Clear();

        geometryBehaviour.HighlightFace(face, isHover);
    }

    #endregion

    #region Common

    public FormElement VertexForm(GeoVertex vertex)
    {
        string s = geometry.VertexSign(vertex.Id);
        FormElement formElement = new FormElement(1, new string[] { s });
        return formElement;
    }

    public FormElement EdgeForm(GeoEdge edge)
    {
        string s1 = geometry.VertexSign(edge.Id1);
        string s2 = geometry.VertexSign(edge.Id2);
        FormElement formElement = new FormElement(2, new string[] { s1, s2 });
        return formElement;
    }

    public FormElement FaceForm(GeoFace face)
    {
        int count = face.Ids.Length;
        string[] signs = new string[count];

        for (int i = 0; i < count; i++)
        {
            signs[i] = geometry.VertexSign(face.Ids[i]);
        }

        FormElement formElement = new FormElement(count, signs);
        return formElement;
    }

    #endregion


    #region Operation

    public void EndOperation()
    {
        if (currentOperation == null)
            return;
        currentOperation.End();
        currentOperation = null;
        SetState(GeoState.Normal);
    }

    public void GeometryOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;

        currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
        currentOperation.Start();
    }

    private string StringToBinary(string str)
    {
        byte[] data = Encoding.Unicode.GetBytes(str);
        StringBuilder sb = new StringBuilder(data.Length * 8);
        foreach (byte item in data)
        {
            sb.Append(Convert.ToString(item, 2).PadLeft(8, '0'));
        }
        return sb.ToString();
    }

    public void Classify(String str)
    {
        try
        {
            str = str.ToUpper();
            str = str.Replace(" ", "");
            /*
            正方体/立方体ABCD-A1B1C1D1
            三棱锥P-ABC
            旋转体  矩形旋转体、三角形旋转体  旋转、展开
            测量长、宽、高  
            作空间中一点A(1,1,1)
            作两点、线段中点              作AB中点C
            线段描点                     取AB上一点C
            作平面重心                   作面ABC..重心M
            连接两点                     连接MN 
            连接点作平面                 连接ABC..作平面
            过点作线段垂线               过点A作线段MN垂线交线段于点P
            过点作平面垂线               过点A作面ABC..垂线交面于点Q
            测量长度、角度、面积          ...的长度/角度/面积
             */
            if (!(str.IndexOf("旋转体") != -1) && str.IndexOf("旋转") != -1)
            {
                Debug.Log("旋转");
                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[0];
                //Debug.Log(tool.Type);  //reslovedBody
                SetState(GeoState.Auxiliary);

                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                currentOperation.Start();
            }
            else if (str.IndexOf("展开") != -1)
            {
                Debug.Log("展开");
                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[1];
                //Debug.Log(tool.Type);  //reslovedBody
                SetState(GeoState.Auxiliary);

                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                currentOperation.Start();
            }
            else if (str.Length < 3)
            {
                Debug.Log("can not recognize!");
            }
            else if (str.IndexOf("正方体") != -1 || str.IndexOf("立方体") != -1)
            {
                int tar = -1;
                if (str.IndexOf("正方体") != -1)
                {
                    tar = str.IndexOf("正方体");
                }
                else
                {
                    tar = str.IndexOf("立方体");
                }
                
                Debug.Log("正方体");
                Tool tool = geoUI.toolPanel.toolGroups[0].Tools[0];
                currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
                String list = "";
                if (str.Length > 3)
                {
                    //Debug.Log(str.Substring(3));
                    int itemCount = 0;
                    for (int i = tar+3; i < str.Length; i++)
                        {
                            String item = str.Substring(i, 1);
                            if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                            {
                                if (i == str.Length - 1)
                                {
                                    list += item;
                                    itemCount++;
                                }
                                else
                                {
                                    if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                                    {
                                        list += str.Substring(i, 2);
                                        itemCount++;
                                        if (i != str.Length - 2)
                                        {
                                            list += " ";
                                        }
                                        i++;
                                    }
                                    else
                                    {
                                        list += item + " ";
                                        itemCount++;
                                    }
                                }
                            }
                    }//for
                    if (itemCount == 8)
                    {
                            Debug.Log(list);
                            GeometryOperation opt = (GeometryOperation)currentOperation;
                            opt.ReSetSign(list);
                    }
                    else
                    {
                            // default name
                    }
              
                    
                }

                currentOperation.Start();

            }
            else if (str.IndexOf("三棱锥") != -1 || str.IndexOf("四面体") != -1)
            {
                Debug.Log("三棱锥");
                int index = -1;
                if (str.IndexOf("三棱锥") != -1) {
                    index = str.IndexOf("三棱锥");
                }
                else{
                    index = str.IndexOf("四面体");
                }
         
                Tool tool = geoUI.toolPanel.toolGroups[0].Tools[1];
                currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
                String list = "";

                if (str.Length > 3)
                {
                    Debug.Log(str.Substring(3));
                    String triName = str.Substring(3);
                    int itemCount = 0;
                    if (triName.Length >= 4)
                    {
                        for (int i = index+3; i < triName.Length; i++)
                        {
                            String item = triName.Substring(i, 1);
                            if (Regex.IsMatch(triName.Substring(i, 1), @"^[A-Za-z]+$"))
                            {
                                if (i == triName.Length - 1)
                                {
                                    list += item;
                                    itemCount++;
                                }
                                else
                                {
                                    if (Regex.IsMatch(triName.Substring(i + 1, 1), @"^[0-9]*$"))
                                    {
                                        list += triName.Substring(i, 2);
                                        itemCount++;
                                        if (i != triName.Length - 2)
                                        {
                                            list += " ";
                                        }
                                        i++;
                                    }
                                    else
                                    {
                                        list += item + " ";
                                        itemCount++;
                                    }
                                }
                            }
                        }
                        if (itemCount == 4)
                        {
                            Debug.Log(list);
                            GeometryOperation opt = (GeometryOperation)currentOperation;
                            opt.ReSetSign(list);
                        }
                        else
                        {
                            // default name
                        }
                    }
                    else
                    {
                        // default name
                    }
                }

                currentOperation.Start();

            }
            else if (str.IndexOf("旋转体") != -1)
            {
                Debug.Log("旋转体");
                Tool tool = geoUI.toolPanel.toolGroups[0].Tools[2];
                currentOperation = new GeometryOperation(this, toolController, stateController, tool, geometryBehaviour);
                currentOperation.Start();
                if (str.IndexOf("矩形旋转体") != -1 || str.IndexOf("长方形旋转体") != -1)
                {
                    Tool tool1 = geoUI.toolPanel.toolGroups[1].Tools[0];
                    Debug.Log(tool1.Description);
                    //Debug.Log(tool1.Type);

                    SetState(GeoState.Condition);

                    currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool1);
                    currentOperation.Start();

                }
                else if (str.IndexOf("三角形旋转体") != -1)
                {
                    Tool tool1 = geoUI.toolPanel.toolGroups[1].Tools[1];
                    Debug.Log(tool1.Description);
                    //Debug.Log(tool1.Type);

                    SetState(GeoState.Condition);

                    currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool1);
                    currentOperation.Start();
                }
            }
            else if (str.IndexOf("空间一点") != -1 || str.IndexOf("空间1点") != -1)
            {
                Debug.Log("取空间一点");

            }
            else if (str.IndexOf("中点") != -1)
            {
                Debug.Log("中点");
                String line = "";
                int itemCount = 0;
                for (int i = 0; i < str.IndexOf("中点"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            line += str.Substring(i, 1) + str.Substring(i + 1, 1);
                            i++;
                        }
                        else
                        {
                            line += str.Substring(i, 1);
                        }

                        if (itemCount == 2)
                        {
                            break;
                        }
                        else
                        {
                            line += " ";
                        }
                    }
                }
                String point = "";
                for (int i = str.IndexOf("中点") + 2; i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point += str.Substring(i, 1) + str.Substring(i + 1, 1);
                        }
                        else
                        {
                            point += str.Substring(i, 1);
                        }
                        break;
                    }
                }

                FormText text1 = new FormText("作");
                String[] eles = line.Split(' ');
                FormElement ele1 = new FormElement(eles.Length);
                for (int i = 0; i < eles.Length; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("中点");
                FormElement ele2 = new FormElement(1);
                ele2.fields[0] = point;

                FormInput writeInput = new FormInput(4);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;
                writeInput.inputs[3] = ele2;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[1];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if ((str.IndexOf("线段") != -1 && (str.IndexOf("一点") != -1 || str.IndexOf("1点") != -1)) ||
                ((str.IndexOf("线段") != -1) && str.IndexOf("的点") != -1) ||
                ((str.IndexOf("作") != -1 || str.IndexOf("做") != -1) && (str.IndexOf("一点") != -1 || str.IndexOf("的点") != -1) && !(str.IndexOf("空间") != -1)))
            {
                Debug.Log("取线段一点");

                String line = "";
                int itemCount = 0;
                for (int i = 0; i < str.IndexOf("点")-1; i++)  // -1, due to "1点"
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                        }
                        else
                        {
                            line += str.Substring(i, 1) + " ";
                        }

                        if (itemCount == 2)
                        {
                            break;
                        }

                    }
                }
                if (itemCount < 2)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                String point = "";
                for (int i = str.IndexOf("点") + 1; i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point += str.Substring(i, 1) + str.Substring(i + 1, 1);
                        }
                        else
                        {
                            point += str.Substring(i, 1);
                        }
                        break;
                    }
                }

                Debug.Log(line + " * " + point);
                FormText text1 = new FormText("作线段");
                String[] eles = line.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("的点");
                FormElement ele2 = new FormElement(1);
                ele2.fields[0] = point;
                //Debug.Log(ele2.fields[0]);
                FormInput writeInput = new FormInput(4);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;
                writeInput.inputs[3] = ele2;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[2];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("重心") != -1 || str.IndexOf("中心") != -1)
            {
                Debug.Log("重心");
                int tar = -1;
                if (str.IndexOf("重心") != -1)
                {
                    tar = str.IndexOf("重心");
                }
                else {
                    tar = str.IndexOf("中心");
                }

                String face = "";
                int itemCount = 0;
                for (int i = 0; i < tar; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            face += str.Substring(i, 1) + str.Substring(i + 1, 1);
                        }
                        else
                        {
                            face += str.Substring(i, 1);
                        }

                        face += " ";
                    }
                }
                if (itemCount < 3)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                String point = "";
                for (int i = tar+2; i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point += str.Substring(i, 1) + str.Substring(i + 1, 1);
                        }
                        else
                        {
                            point += str.Substring(i, 1);
                        }
                        break;
                    }
                }

                FormText text1 = new FormText("作平面");
                String[] eles = face.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("的重心");
                FormElement ele2 = new FormElement(1);
                ele2.fields[0] = point;
                FormInput writeInput = new FormInput(4);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;
                writeInput.inputs[3] = ele2;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[3];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("连接") != -1 && !(str.IndexOf("面") != -1))
            {
                Debug.Log("连接两点");

                String line = "";
                int itemCount = 0;
                for (int i = str.IndexOf("连接"); i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                        }
                        else
                        {
                            line += str.Substring(i, 1) + " ";
                        }
                        if (itemCount == 2)
                        {
                            break;
                        }
                    }
                }
                if (itemCount < 2)
                {
                    Debug.Log("can not recognize!");
                    return;
                }
                Debug.Log(line);
                FormText text1 = new FormText("连接");
                String[] eles = line.Split(' ');
                FormElement ele1 = new FormElement(eles.Length);
                for (int i = 0; i < eles.Length; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("作线段");

                FormInput writeInput = new FormInput(3);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[4];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("垂线") != -1 && str.IndexOf("线段") != -1)
            {
                Debug.Log("线段垂线");
                String point1 = "";
                for (int i = 0; i < str.IndexOf("线段"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point1 += str.Substring(i, 2);
                            //Debug.Log(point1 + "---");
                            break;
                        }
                        else
                        {
                            point1 += str.Substring(i, 1);
                            break;
                        }
                    }
                }

                String line = "";
                int itemCount = 0;
                for (int i = str.IndexOf("线段"); i < str.IndexOf("垂线"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            line += str.Substring(i, 2) + " ";
                            i++;
                        }
                        else
                        {
                            line += str.Substring(i, 1) + " ";
                        }
                        if (itemCount == 2)
                        {
                            break;
                        }
                    }
                }
                if (itemCount < 2)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                String point2 = "";
                for (int i = str.IndexOf("垂线"); i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point2 += str.Substring(i, 2);
                            i++;
                        }
                        else
                        {
                            point2 += str.Substring(i, 1);
                        }
                        break;
                    }
                }

                FormText text1 = new FormText("过点");
                //Debug.Log(text1);
                FormElement ele1 = new FormElement(1);
                ele1.fields[0] = point1;
                //Debug.Log(ele1);
                FormText text2 = new FormText("作线段");
                //Debug.Log(text2);
                String[] eles = line.Split(' ');
                FormElement ele2 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele2.fields[i] = eles[i];
                    //Debug.Log(eles[i] +"*");
                }
                FormText text3 = new FormText("的垂线");
                //Debug.Log(text3);
                FormText text4 = new FormText("交于点");
                //Debug.Log(text4);
                FormElement ele3 = new FormElement(1);
                ele3.fields[0] = point2;
                //Debug.Log(point2);

                FormInput writeInput = new FormInput(7);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;
                writeInput.inputs[3] = ele2;
                writeInput.inputs[4] = text3;
                writeInput.inputs[5] = text4;
                writeInput.inputs[6] = ele3;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[5];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();

            }
            else if (str.IndexOf("垂线") != -1 && str.IndexOf("平面") != -1)
            {
                Debug.Log("面垂线");

                String point1 = "";
                for (int i = 0; i < str.IndexOf("平面"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point1 += str.Substring(i, 2);
                            //Debug.Log(point1 + "---");
                            break;
                        }
                        else
                        {
                            point1 += str.Substring(i, 1);
                            break;
                        }
                    }
                }

                String face = "";
                int itemCount = 0;
                for (int i = str.IndexOf("平面") + 2; i < str.IndexOf("垂线"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            face += str.Substring(i, 2) + " ";
                            i++;
                        }
                        else
                        {
                            face += str.Substring(i, 1) + " ";
                        }

                    }
                }
                if (itemCount < 3)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                String point2 = "";
                for (int i = str.IndexOf("垂线") + 2; i < str.Length; i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            point2 += str.Substring(i, 2);
                            i++;
                        }
                        else
                        {
                            point2 += str.Substring(i, 1);
                        }
                        break;
                    }
                }

                FormText text1 = new FormText("过点");
                //Debug.Log(text1);
                FormElement ele1 = new FormElement(1);
                ele1.fields[0] = point1;
                //Debug.Log(ele1);
                FormText text2 = new FormText("作平面");
                //Debug.Log(text2);
                String[] eles = face.Split(' ');
                FormElement ele2 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele2.fields[i] = eles[i];
                    //Debug.Log(eles[i] +"*");
                }
                FormText text3 = new FormText("的垂线");
                //Debug.Log(text3);
                FormText text4 = new FormText("交于点");
                //Debug.Log(text4);
                FormElement ele3 = new FormElement(1);
                ele3.fields[0] = point2;
                //Debug.Log(point2);

                FormInput writeInput = new FormInput(7);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;
                writeInput.inputs[3] = ele2;
                writeInput.inputs[4] = text3;
                writeInput.inputs[5] = text4;
                writeInput.inputs[6] = ele3;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[6];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if ((str.IndexOf("作平面") != -1 || str.IndexOf("做平面") != -1)|| str.IndexOf("连接") != -1)
            {
                Debug.Log("连接点作平面");
                
                String face = "";
                int itemCount = 0;

                int tar = -1;
                if ((str.IndexOf("作平面") != -1 || str.IndexOf("做平面") != -1) && str.IndexOf("连接") == -1)
                {
                    tar = str.IndexOf("平面");
                    for (int i = tar+2; i < str.Length; i++)
                    {
                        if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                        {
                            itemCount++;
                            if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                            {
                                face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                            }
                            else
                            {
                                face += str.Substring(i, 1) + " ";
                            }
                        }
                    }
                }
                else
                {
                    tar = str.IndexOf("连接");
                    for (int i = tar + 2; i < str.Length; i++)
                    {
                        if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                        {
                            itemCount++;
                            if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                            {
                                face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                            }
                            else
                            {
                                face += str.Substring(i, 1) + " ";
                            }
                        }
                    }
                }
                
                if (itemCount < 3)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                FormText text1 = new FormText("连接");
                String[] eles = face.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("作平面");
                FormInput writeInput = new FormInput(3);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;

                Tool tool = geoUI.toolPanel.toolGroups[2].Tools[7];
                SetState(GeoState.Auxiliary);


                currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddAuxiliaryOperation opt = (AddAuxiliaryOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("长度") != -1)
            {
                Debug.Log("测量长度");

                String line = "";
                int itemCount = 0;
                for (int i = 0; i < str.IndexOf("长度"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            line += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                        }
                        else
                        {
                            line += str.Substring(i, 1) + " ";
                        }
                        if (itemCount == 2)
                        {
                            break;
                        }
                    }
                }
                if (itemCount < 2)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                FormText text1 = new FormText("线段");
                String[] eles = line.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("的长度");
                FormInput writeInput = new FormInput(3);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;

                Tool tool = geoUI.toolPanel.toolGroups[3].Tools[0];
                SetState(GeoState.Measure);

                currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("角度") != -1)
            {
                Debug.Log("测量角度");

                String angle = "";
                int itemCount = 0;
                for (int i = 0; i < str.IndexOf("角度"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            angle += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                        }
                        else
                        {
                            angle += str.Substring(i, 1) + " ";
                        }

                        if (itemCount == 3)
                        {
                            break;
                        }
                    }
                }
                if (itemCount < 3)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                Debug.Log("测量" + angle + "角度");

                FormText text1 = new FormText("∠");
                String[] eles = angle.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("的角度");
                FormInput writeInput = new FormInput(3);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;

                Tool tool = geoUI.toolPanel.toolGroups[3].Tools[1];
                SetState(GeoState.Measure);

                currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else if (str.IndexOf("面积") != -1)
            {
                Debug.Log("测量面积");

                String face = "";
                int itemCount = 0;
                for (int i = 0; i < str.IndexOf("面积"); i++)
                {
                    if (Regex.IsMatch(str.Substring(i, 1), @"^[A-Za-z]+$"))
                    {
                        itemCount++;
                        if (i != str.Length - 1 && Regex.IsMatch(str.Substring(i + 1, 1), @"^[0-9]*$"))
                        {
                            face += str.Substring(i, 1) + str.Substring(i + 1, 1) + " ";
                        }
                        else
                        {
                            face += str.Substring(i, 1) + " ";
                        }
                    }
                }
                if (itemCount < 3)
                {
                    Debug.Log("can not recognize!");
                    return;
                }

                FormText text1 = new FormText("平面");
                String[] eles = face.Split(' ');
                FormElement ele1 = new FormElement(eles.Length - 1);
                for (int i = 0; i < eles.Length - 1; i++)
                {
                    ele1.fields[i] = eles[i];
                }
                FormText text2 = new FormText("的面积");
                FormInput writeInput = new FormInput(3);
                writeInput.inputs[0] = text1;
                writeInput.inputs[1] = ele1;
                writeInput.inputs[2] = text2;

                Tool tool = geoUI.toolPanel.toolGroups[3].Tools[2];
                SetState(GeoState.Measure);

                currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
                AddMeasureOperation opt = (AddMeasureOperation)currentOperation;
                opt.SetWriteInput(writeInput);
                currentOperation.Start();
            }
            else
            {
                Debug.Log("analysis error :" + str);
                //if (str.Contains("连接"))
                //{
                //  Debug.Log("************");
                //}
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }


    public void MoveVertexOperation(GeoVertex geoVertex, VertexBehaviour vertexBehaviour)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.MoveVertex);

        VertexUnit unit = geometry.VertexUnit(geoVertex.Id);

        currentOperation = new MoveVertexOperation(stateController, geoCamera, geometry, geometryBehaviour, geoUI, unit, vertexBehaviour, isSnapToGrid);
        currentOperation.Start();
    }

    public void EditVertexOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        currentOperation = new EditVertexOperation(this, stateController, geoCamera, geometry, geometryBehaviour, geoUI, geoVertex);
        currentOperation.Start();
    }

    public void EditEdgeOperation(GeoEdge geoEdge)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditEdge);

        currentOperation = new EditEdgeOperation(this, geoCamera, geometry, geometryBehaviour, geoUI, geoEdge);
        currentOperation.Start();
    }

    public void EditFaceOperation(GeoFace geoFace)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditFace);

        currentOperation = new EditFaceOperation(this, geoCamera, geometry, geometryBehaviour, geoUI, geoFace);
        currentOperation.Start();
    }


    public void VertexCoordinateOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        VertexUnit vertexUnit = geometry.VertexUnit(geoVertex.Id);

        currentOperation = new VertexCoordinateOperation(this, stateController, geometry, geometryBehaviour, geoUI, vertexUnit);
        currentOperation.Start();
    }


    public void DeleteVertexOperation(GeoVertex geoVertex)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditVertex);

        currentOperation = new DeleteVertexOperation(this, geometry, geoVertex);
        currentOperation.Start();
    }

    public void DeleteEdgeOperation(GeoEdge geoEdge)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditEdge);

        currentOperation = new DeleteEdgeOperation(this, geometry, geoEdge);
        currentOperation.Start();
    }

    public void DeleteFaceOperation(GeoFace geoFace)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.EditFace);

        currentOperation = new DeleteFaceOperation(this, geometry, geoFace);
        currentOperation.Start();
    }

    public void AddConditionOperation(Tool tool)
    {
        if (tool.Name != "Free")
        {
            if (state != GeoState.Normal)
                return;
            SetState(GeoState.Condition);
        }
        currentOperation = new AddConditionOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void AddAuxiliaryOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Auxiliary);

        currentOperation = new AddAuxiliaryOperation(this, geoCamera, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void AddMeasureOperation(Tool tool)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Measure);

        currentOperation = new AddMeasureOperation(this, stateController, geometry, geometryBehaviour, geoUI, tool);
        currentOperation.Start();
    }

    public void ClearGeometryOperation(Geometry geometry)
    {
        if (state != GeoState.Normal)
            return;

        currentOperation = new ClearGeometryOperation(this);
        currentOperation.Start();
    }

    public void RemoveConditionOperation(Condition condition)
    {
        FunctionCondition fun = (FunctionCondition)condition;
        if (!(condition is FunctionCondition))
        {
            if (state != GeoState.Normal)
                return;
            SetState(GeoState.Condition);
        }

        currentOperation = new RemoveConditionOperation(this, geometry, condition);
        currentOperation.Start();
    }

    public void RemoveAuxiliaryOperation(Auxiliary auxiliary)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Auxiliary);

        currentOperation = new RemoveAuxiliaryOperation(this, geometry, auxiliary);
        currentOperation.Start();
    }

    public void RemoveMeasureOperation(Measure measure)
    {
        if (state != GeoState.Normal)
            return;
        SetState(GeoState.Measure);

        currentOperation = new RemoveMeasureOperation(this, geometry, measure);
        currentOperation.Start();
    }

    #endregion

    public void ChangeConditionState(Sprite sprite) {
        StateCell cell = stateController.GetConditionState();

        cell.SetImage(sprite);
    }
}
