using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class Pen
{
    List<Vector3> points;
    int id;

    public Pen(int id)
    {
        points = new List<Vector3>();
        this.id = id;
    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    public List<Vector3> GetPoints()
    {
        return points;
    }

    public override string ToString()
    {
        return string.Format("pen {0}", this.id);
    }
}

public class Word
{
    List<Pen> pens;
    List<Vector2> points;
    int id;

    public Word(int id, List<Pen> pens)
    {
        this.id = id;
        this.pens = pens;
        points = new List<Vector2>();
        pens.ForEach(pen =>
        {
            pen.GetPoints().ForEach(p =>
            {
                points.Add(new Vector2(p.x, p.y));
            });
        });
    }

    public List<Pen> GetPens()
    {
        return this.pens;
    }

    public List<Vector2> GetPoints()
    {
        return points;
    }

    public override string ToString()
    {
        return string.Format("word {0}", this.id);
    }
}

public class PenBehaviour : ElementBehaviour
{
    const string LAYER = "UI";
    const float MAX_TIME = 2f;
    const float ValidDiff = 0.4f;
    // const float WorldValidDiff 

    LineRenderer lineRenderer;
    int i;
    Pen pen;
    List<Pen> pens;
    List<Word> words;
    Camera curCamera;
    int PenCount = 0;
    int WordCount = 0;
    RectTransform penWrapper;
    float waitTime;

    float width;
    float height;
    Vector2 xRange;
    Vector2 yRange;
    Vector3 startPoint;
    string recognizeResult;

    public Action<FormInput> OnClickSubmit;
    public Action<FormInput> OnClickCancel;
    Button btnSubmit;
    Button btnCancel;

    Dictionary<Pen, GameObject> penMap;
    RecognizePanel recognizePanel;
    NavPanel navPanel;

    List<Vector3> ShapePositions;

    bool Drawing;
    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    GeoController geoController;

    const int RENDER_WIDTH_16_9 = 2560;
    const int RENDER_HEIGHT_16_9 = 1440;
    const int RENDER_WIDTH_16_10 = 2560;
    const int RENDER_HEIGHT_16_10 = 1600;

    // Used to fix scale
    const int RENDER_WIDTH = 2560;
    int screen_width;
    int screen_height;
    float ratio = 0f;
    float factor_x = 0f;
    float factor_y = 0f;
    int offset = 240;
    Vector3 delta;
    Pen prePen;

    //fomula
    Vector3[] positions;
    string image;


    public void Init(GeoUI geoUI, GeoController geoController)
    {
        recognizePanel = geoUI.recognizePanel;
        navPanel = geoUI.navPanel;
        geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        this.geoController = geoController;
    }

    public void InitScaleParameters()
    {
        screen_width = UnityEngine.Screen.width;
        screen_height = UnityEngine.Screen.height;
        ratio = (float)screen_width / screen_height;
        factor_x = (float)RENDER_WIDTH / screen_width;
        factor_y = (float)RENDER_WIDTH / ratio / screen_height;
        delta = new Vector3(-(-startPoint.x * factor_x + offset), 0, 0);
        Debug.Log("screen:(" + screen_width + "," + screen_height + "), " + "fx:" + factor_x + ", fy:" + factor_y + ", delta:" + delta.x);
    }

    public void SetDrawing(bool Drawing)
    {
        this.Drawing = Drawing;
        if (Drawing)
        {
            ShapePositions = new List<Vector3>();
        }
    }

    public void SetGeometry(Geometry geometry)
    {
        this.geometry = geometry;
    }

    public void Start()
    {
        penMap = new Dictionary<Pen, GameObject>();
        pens = new List<Pen>();
        words = new List<Word>();

        lineRenderer = transform.GetComponent<LineRenderer>();
        InitRenderer(lineRenderer);
        penWrapper = (RectTransform)transform;
        width = penWrapper.rect.width;
        height = penWrapper.rect.height;
        Debug.Log("penWrapper width:" + penWrapper.rect.width + ", height:" + penWrapper.rect.height);
        xRange = new Vector2(0, width);
        yRange = new Vector2(-height, 0);
        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].name == "UICameraFront")
                curCamera = cameras[i];
        }
        startPoint = curCamera.WorldToScreenPoint(penWrapper.parent.position) - new Vector3(width, 0, 1);
        // fix scale bugs
        InitScaleParameters();

        btnSubmit = transform.parent.Find("ButtonSubmit").GetComponent<Button>();
        btnCancel = transform.parent.Find("ButtonCancel").GetComponent<Button>();
        btnSubmit.onClick.AddListener(ClickSubmit);
        btnCancel.onClick.AddListener(ClickCancel);

        visiable = true;
        waitTime = 0;
        recognizeResult = "";
    }

    private void InitRenderer(LineRenderer renderer)
    {
        renderer.startWidth = 1f;
        renderer.endWidth = 1f;
        renderer.useWorldSpace = false;
        SetColorIndex(renderer, 0);
        SetStyleIndex(renderer, 0);
    }

    private bool IsValidPoint(Vector3 vector3)
    {
        if (vector3.x < xRange.x || vector3.x > xRange.y || vector3.y < yRange.x || vector3.y > yRange.y)
        {
            return false;
        }
        return true;
    }

    void Update()
    {
        /*if (!Drawing)
        {
            waitTime += Time.deltaTime;
            if (waitTime > MAX_TIME && pens.Count > 0)
            {
                if (geometry!=null&&geometry.Type == GeometryType.Function)
                {
                    AddFomula();
                }
                else
                {
                    AddWord();
                }
                waitTime = 0;
            }
        }*/
        //生成新笔迹
        if (Input.GetMouseButtonDown(0))
        {
            pen = new Pen(PenCount++);
            i = 0;
            Init(pen);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 point = new Vector3(0, 0, 0);
            if (Drawing) {
                point = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0) - startPoint;
                Vector3 fixPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                fixPoint = ScaleHandwritingPoint(fixPoint);
                if (!pen.GetPoints().Contains(fixPoint))
                {
                    pen.AddPoint(fixPoint);
                    SetData(i, fixPoint);
                    i++;
                }
            } else {
                point = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                point = ScaleHandwritingPoint(point);
                if (!pen.GetPoints().Contains(point))
                {
                    pen.AddPoint(point);
                    SetData(i, point);
                    i++;
                }
            }
            waitTime = 0;
        }
        if (Input.GetMouseButtonUp(0))
        {
            //添加笔迹并检查是否是√或O
            AddPen(pen);
            int res=CheckPen(pen);
            if (res == 1)
            {
                AddFomula();
            }
            else if (res == 2) 
            {
                Debug.Log("测试到圈");
                AddChange();
            }
        }
    }

    // 按照屏幕比率进行放缩，得到新的对应的笔迹点
    private Vector3 ScaleHandwritingPoint(Vector3 point)
    {
        // 获取当前屏幕显示分辨率，判断屏幕比例
        // delta = ( -(startPoint.x * factor_x - 实际左下角.x), 0, 0 )
        //       = ( -(startPoint.x * factor_x + 240), 0, 0 )
        point = new Vector3(point.x * factor_x, point.y * factor_y, 0) - new Vector3(startPoint.x * factor_x, startPoint.y * factor_y, 0);
        point = point + delta;
        return point;
    }

    // 反向还原被放缩的点，用于绘制旋转体
    private Vector3 ReductionScalePoint(Vector3 point) {
        point = point - delta;
        point = point + new Vector3(startPoint.x * factor_x, startPoint.y * factor_y, 0);
        point = new Vector3(point.x / factor_x, point.y / factor_y, 0);
        return point;
    }

    private Vector3 ScreenPositionToAxis(Vector3 mousePosition)
    {
        // TODO: 坐标偏移
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Vector3 position = new Vector3(0, 0, 0);
        Plane screenPlane = new Plane(-ray.direction, position);

        float distance = 0;
        if (screenPlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        // Normalize
        if (Mathf.Abs(position.z) <= ValidDiff)
        {
            position.z = 0;
        }

        return position;
    }

    private void AddPen(Pen pen)
    {
        if (penMap.ContainsKey(pen))
            return;
        if (pen.GetPoints().Count <= 1)
            return;
        pens.Add(pen);
        GameObject penObject = new GameObject(pen.ToString());
        penObject.layer = LayerMask.NameToLayer(LAYER); ;
        RectTransform rect = penObject.AddComponent<RectTransform>();
        penObject.transform.SetParent(penWrapper.transform);
        rect.position = new Vector3(0, 0, 1);
        rect.localScale = new Vector3(1, 1, 1);

        rect.pivot = new Vector2(0, 1);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, penWrapper.rect.width);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, penWrapper.rect.height);
        LineRenderer curLineRenderer = penObject.AddComponent<LineRenderer>();
        InitRenderer(curLineRenderer);
        curLineRenderer.positionCount = pen.GetPoints().Count;
        curLineRenderer.SetPositions(pen.GetPoints().ToArray());
        curLineRenderer.Simplify(1);
        penMap.Add(pen, penObject);

        if (Drawing)
        {
            List<Vector3> points = pen.GetPoints();
            Vector3 worldStart = points[0];
            Vector3 worldEnd = points[points.Count - 1];
            worldStart = ReductionScalePoint(worldStart);
            worldEnd = ReductionScalePoint(worldEnd);
            if (!IsValidDrawPoint(worldStart) || !IsValidDrawPoint(worldEnd))
            {
                return;
            }
            Vector3 start = ScreenPositionToAxis(worldStart);
            Vector3 end = ScreenPositionToAxis(worldEnd);


            foreach (Vector3 position in new Vector3[] { start, end })
            {
                bool IsNew = true;
                foreach (Vector3 point in ShapePositions)
                {
                    IsNew = !IsSamePoint(position, point);
                    if (!IsNew)
                    {
                        break;
                    }
                }
                if (IsNew)
                {
                    ShapePositions.Add(position);
                }
            }
        }
    }
    private int CheckPen(Pen pen) {
        List<Vector2> points = new List<Vector2>();
        pen.GetPoints().ForEach(p =>
       {
            points.Add(new Vector2(p.x, p.y));
        });
        int border = 20;
        int Thickness = 10;
        int left = (int)penWrapper.rect.width;
        int right = 0;
        int top = (int)penWrapper.rect.height;
        int bottom = 0;
        points.ForEach(point =>
        {
            point += new Vector2(startPoint.x, startPoint.y);
            left = Mathf.Min(left, (int)point.x);
            right = Mathf.Max(right, (int)point.x);
            top = Mathf.Min(top, (int)point.y);
            bottom = Mathf.Max(bottom, (int)point.y);
        });
        int width = right - left + Thickness + 2 * border;
        int height = bottom - top + Thickness + 2 * border;

        Texture2D png = new Texture2D(width, height);
        List<Vector3> positions = pen.GetPoints();
        for (int i = 0; i < positions.Count - 1; i++)
        {
            Vector2 start = new Vector2(positions[i].x, positions[i].y) + new Vector2(startPoint.x, startPoint.y);
            Vector2 end = new Vector2(positions[i + 1].x, positions[i + 1].y) + new Vector2(startPoint.x, startPoint.y);
            List<Vector2> betweenPoints = GetPointsBetweenStartAndEnd(start, end);
            foreach (Vector2 point in betweenPoints)
            {
                for (int m = 0; m < Thickness; m++)
                {
                    for (int n = 0; n < Thickness; n++)
                    {
                        png.SetPixel((int)point.x - left + m + border, (int)point.y - top + n + border, new Color(0, 0, 0, 1));
                    }
                }
            }
        }
        string base64 = System.Convert.ToBase64String(png.EncodeToPNG());
        string contents = Application.dataPath + "/temp";
        string pngName = "image";
        byte[] bytes = png.EncodeToJPG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(contents + "/" + pngName + ".jpg", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
/*        string res = geoController.HandleRecognizeTick(base64);
        if (res=="tick") return 1;
        else if (res == "circle") return 2;
        else return 0;*/
        if (PenCount == 8) return 2;
        return 0;
    }

    private bool IsValidDrawPoint(Vector3 point)
    {
        int left = 240;
        int right = (int)(left + penWrapper.rect.width);
        int top = 0;
        int bottom = 180;
        if (point.x >= right - 180 && point.x <= right && point.y >= top && point.y <= bottom)
        {
            return false;
        }
        return true;
    }

    private bool IsSamePoint(Vector3 point1, Vector3 point2)
    {
        Vector3 diff = point1 - point2;
        if (diff.sqrMagnitude < new Vector3(ValidDiff, ValidDiff, ValidDiff).sqrMagnitude)
        {
            return true;
        }
        return false;
    }

    private void AddWord()
    {
        Word word = new Word(WordCount++, new List<Pen>(pens));
        words.Add(word);
        pens.Clear();
        string base64= PointsToBitmap(word).Replace(" ", "");
        string res = geoController.HandleRecognizeResult(base64);
        if (res == "")
            res = "空";

        recognizePanel.AddWord(res);
        recognizeResult = recognizePanel.GetWords() + res;
    }

    private void AddFomula()
    {
        List<Pen> pen1=pens;
        pen1.Remove(pen);
        Word word = new Word(WordCount++, new List<Pen>(pen1));
        words.Add(word);
        pens.Clear();
        string base64 = PointsToBitmap(word).Replace(" ", "");
        string res;
        Vector3[] pos;
        string img;
        geoController.HandleRecognizeFomula(base64, out res, out pos, out img);
        positions = pos;
        image = img;
        if (res == "")
            res = "空";
        recognizePanel.AddWord(res);
        recognizeResult = recognizePanel.GetWords() + res;
        if(res!="空")
            recognizePanel.AddImage(image);

    }

    private void AddChange()
    {
        List<Pen> pen1 = pens;
        pen1.Remove(pen);
        Word word = new Word(WordCount++, new List<Pen>(pen1));
        words.Add(word);
        pens.Clear();
        string base64 = PointsToBitmap(word).Replace(" ", "");
        string res;
        Vector3[] pos;
        string img;
        geoController.HandleRecognizeChange(base64, out res, out pos, out img);
        positions = pos;
        image = img;
        if (res == "")
            res = "空";

        recognizePanel.AddWord(res);
        recognizeResult = recognizePanel.GetWords() + res;

    }



    private string PointsToBitmap(Word word)
    {
        List<Vector2> points = word.GetPoints();
        int border = 20;
        int Thickness = 10;
        int left = (int)penWrapper.rect.width;
        int right = 0;
        int top = (int)penWrapper.rect.height;
        int bottom = 0;
        points.ForEach(point =>
        {
            point += new Vector2(startPoint.x, startPoint.y);
            left = Mathf.Min(left, (int)point.x);
            right = Mathf.Max(right, (int)point.x);
            top = Mathf.Min(top, (int)point.y);
            bottom = Mathf.Max(bottom, (int)point.y);
        });
        int width = right - left + Thickness + 2 * border;
        int height = bottom - top + Thickness + 2 * border;

        Texture2D png = new Texture2D(width, height);
        foreach (Pen pen in word.GetPens())
        {
            List<Vector3> positions = pen.GetPoints();
            for (int i = 0; i < positions.Count - 1; i++)
            {
                Vector2 start = new Vector2(positions[i].x, positions[i].y) + new Vector2(startPoint.x, startPoint.y);
                Vector2 end = new Vector2(positions[i + 1].x, positions[i + 1].y) + new Vector2(startPoint.x, startPoint.y);
                List<Vector2> betweenPoints = GetPointsBetweenStartAndEnd(start, end);
                foreach (Vector2 point in betweenPoints)
                {
                    for (int m = 0; m < Thickness; m++)
                    {
                        for (int n = 0; n < Thickness; n++)
                        {
                            png.SetPixel((int)point.x - left + m + border, (int)point.y - top + n + border, new Color(0, 0, 0, 1));
                        }
                    }
                }
            }
        }
        string base64 = System.Convert.ToBase64String(png.EncodeToPNG());
        string contents = Application.dataPath + "/temp";
        string pngName = "image";
        byte[] bytes = png.EncodeToPNG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(contents + "/" + pngName + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
        /*if (geometry!=null&&geometry.Type == GeometryType.Function)
        {
            string res;
            Vector3[] pos;
            string img;
            geoController.HandleRecognizeFomula(base64,out res,out pos,out img);
            positions = pos;
            image = img;
            return res;
        }
        else
        {
            return geoController.HandleRecognizeResult(base64);
        }*/
        return base64;
    }

    private List<Vector2> GetPointsBetweenStartAndEnd(Vector2 start, Vector2 end)
    {
        List<Vector2> linePoint = new List<Vector2>();
        Vector2 pointMaxX = new Vector2();
        Vector2 pointMinX = new Vector2();
        if (Mathf.Max(start.x, end.x) == start.x)
        {
            pointMaxX = start;
            pointMinX = end;
        }
        else
        {
            pointMaxX = end;
            pointMinX = start;
        }
        double k = ((double)(pointMinX.y - pointMaxX.y)) / (pointMinX.x - pointMaxX.x);
        for (int i = (int)pointMinX.x; i <= pointMaxX.x; i++)
        {
            double y = k * (i - pointMinX.x) + pointMinX.y;
            linePoint.Add(new Vector2(i, (int)y));
        }
        for (int i = (int)pointMinX.y; i <= pointMaxX.y; i++)
        {
            double x = (i - pointMinX.y) / k + pointMinX.x;
            linePoint.Add(new Vector2((int)x, i));
        }
        return linePoint;
    }

    private bool AddShape()
    {
        if (geometry is ResolvedBody)
        {
            ResolvedBody resolvedBody = (ResolvedBody)geometry;
            if (resolvedBody.shapeSetted)
            {
                return false;
            }
            switch (ShapePositions.Count)
            {
                case 3:
                    resolvedBody.SetTriangle(ShapePositions.ToArray());
                    break;
                case 4:
                    resolvedBody.SetRectangle(ShapePositions.ToArray());
                    break;
                default:
                    return false;
            }
        }
        geometryBehaviour.InitGeometry(geometry);
        return true;
    }

    public void Init(Pen pen)
    {
        lineRenderer.SetPositions(pen.GetPoints().ToArray());
    }

    public void SetData(int i, Vector3 point)
    {
        lineRenderer.positionCount = (i + 1);
        lineRenderer.SetPosition(i, point);
    }

    public void SetColorIndex(LineRenderer renderer, int index)
    {
        base.SetColorIndex(index);
        StyleManager.SetLineProperty(renderer, colorIndex);
    }

    public void SetStyleIndex(LineRenderer renderer, int index)
    {
        base.SetStyleIndex(index);
        renderer.sharedMaterial = ConfigManager.EdgeStyle[styleIndex].Material;
    }

    private void Clear()
    {
        prePen = null;
        foreach (KeyValuePair<Pen, GameObject> pair in penMap)
            Destroy(pair.Value);
        penMap.Clear();

        StatusButton lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        geoController.EndWriting();
        lockButton.SetStatus(0);
        recognizeResult = "";
        navPanel.SetWritingButtonStatus(0);
    }

    private void ClickSubmit()
    {
        transform.parent.gameObject.SetActive(false);
        String command = recognizePanel.GetWords();
        recognizePanel.Clear();
        Clear();
        if (Drawing)
        {
            AddShape();
        }
        else
        {
            if (geometry != null && geometry.Type == GeometryType.Function)
            {
                Function function = (Function)geometry;
                function.SetWriting(positions,command);
                geometryBehaviour.AddLine(function.Getline()[function.Getindex()-1]);
                Tool tool = new Tool();
                tool.Name = "Free";
                geoController.AddConditionOperation(tool);
            }
            else
            {
                GameObject.Find("GeoController").GetComponent<GeoController>().Classify(command);
            }
        }
    }

    private void ClickCancel()
    {
        Debug.Log("取消");
        transform.parent.gameObject.SetActive(false);
        recognizePanel.Clear();
        Clear();
    }
}
