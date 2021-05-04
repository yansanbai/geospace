using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateBehaviour : MonoBehaviour
{
    const float LINE_RADIUS = 0.02f;
    const float POINT_LENGTH = 0.1f;
    const float POINT_SIGN_BIAS = 0.15f;
    const float AXIS_SIGN_BIAS = 0.2f;
    const float AXIS_SIGN_OFFSET = 0.2f;

    public Mesh ArrowMesh;

    protected static Mesh lineMesh;

    protected GeoCamera geoCamera;

    private GameObject axisWrapper, arrowWrapper, pointWrapper, signWrapper;


    private GameObject xAxis, yAxis, zAxis;

    private GameObject xArrow, yArrow, zArrow;
    private TextMesh xSign, ySign, zSign;

    private Vector3[] xPos, yPos, zPos;

    private GameObject[] xPoints, yPoints, zPoints;
    private GameObject[] Signx, Signy, Signz;
    private int sigx, sigy, sigz;
    private TextMesh[] xSigns, ySigns, zSigns;

    private int count;


    private float screenAspect = (float)Screen.width / (float)Screen.height;

    public void Init(GeoCamera camera, int size)
    {
        geoCamera = camera;
        geoCamera.OnRotate += OnCameraRotate;

        if (size <= 0)
            size = 2;
        if (size % 2 == 1)
            size += 1;


        if (lineMesh == null)
            lineMesh = LineMesh();

        count = size + 1;
        int offset = size / 2;


        axisWrapper = InitWrapper("Axis");
        arrowWrapper = InitWrapper("Arrow");
        pointWrapper = InitWrapper("Point");
        signWrapper = InitWrapper("Sign");

        // Axis
        MeshRenderer xAxisRender, yAxisRender, zAxisRender;
        xAxis = InitAxis("x", out xAxisRender);
        yAxis = InitAxis("y", out yAxisRender);
        zAxis = InitAxis("z", out zAxisRender);

        StyleManager.SetAxisXMaterial(xAxisRender);
        StyleManager.SetAxisYMaterial(yAxisRender);
        StyleManager.SetAxisZMaterial(zAxisRender);


        // Arrow
        MeshRenderer xArrowRender, yArrowRender, zArrowRender;
        xArrow = InitArrow("x", new Vector3(offset, 0, 0), new Vector3(0, 0, -90), out xArrowRender);
        yArrow = InitArrow("y", new Vector3(0, offset, 0), new Vector3(0, 0, 0), out yArrowRender);
        zArrow = InitArrow("z", new Vector3(0, 0, offset), new Vector3(90, 0, 0), out zArrowRender);

        StyleManager.SetArrowXMaterial(xArrowRender);
        StyleManager.SetArrowYMaterial(yArrowRender);
        StyleManager.SetArrowZMaterial(zArrowRender);

        Signx = new GameObject[size+1];
        Signy = new GameObject[size+1];
        Signz = new GameObject[size+1];
        sigx = sigy = sigz = 0;

        MeshRenderer xSignRender, ySignRender, zSignRender;
        xSign = InitSign("x", "x", new Vector3(offset, 0, 0), out xSignRender);
        ySign = InitSign("y", "y", new Vector3(0, offset, 0), out ySignRender);
        zSign = InitSign("z", "z", new Vector3(0, 0, offset), out zSignRender);

        StyleManager.SetAxisSignAttr(xSign);
        StyleManager.SetAxisSignAttr(ySign);
        StyleManager.SetAxisSignAttr(zSign);

        StyleManager.SetAxisSignXMaterial(xSignRender);
        StyleManager.SetAxisSignYMaterial(ySignRender);
        StyleManager.SetAxisSignZMaterial(zSignRender);

        // Point
        xPoints = new GameObject[size];
        yPoints = new GameObject[size];
        zPoints = new GameObject[size];

        xSigns = new TextMesh[size];
        ySigns = new TextMesh[size];
        zSigns = new TextMesh[size];

        xPos = new Vector3[size];
        yPos = new Vector3[size];
        zPos = new Vector3[size];

        InitPointsSigns("x", ref xPos, ref xPoints, ref xSigns, Vector3.right, size, StyleManager.SetAxisXMaterial, StyleManager.SetAxisSignXMaterial);
        InitPointsSigns("y", ref yPos, ref yPoints, ref ySigns, Vector3.up, size, StyleManager.SetAxisYMaterial, StyleManager.SetAxisSignYMaterial);
        InitPointsSigns("z", ref zPos, ref zPoints, ref zSigns, Vector3.forward, size, StyleManager.SetAxisZMaterial, StyleManager.SetAxisSignZMaterial);


        StyleManager.OnStyleChange += () =>
        {
            StyleManager.SetAxisXMaterial(xAxisRender);
            StyleManager.SetAxisYMaterial(yAxisRender);
            StyleManager.SetAxisZMaterial(zAxisRender);

            StyleManager.SetArrowXMaterial(xArrowRender);
            StyleManager.SetArrowYMaterial(yArrowRender);
            StyleManager.SetArrowZMaterial(zArrowRender);

            StyleManager.SetAxisSignAttr(xSign);
            StyleManager.SetAxisSignAttr(ySign);
            StyleManager.SetAxisSignAttr(zSign);

            StyleManager.SetAxisSignXMaterial(xSignRender);
            StyleManager.SetAxisSignYMaterial(ySignRender);
            StyleManager.SetAxisSignZMaterial(zSignRender);

            for (int i = 0; i < size; i++)
            {
                MeshRenderer pointMRX = xPoints[i].GetComponent<MeshRenderer>();
                StyleManager.SetAxisXMaterial(pointMRX);

                MeshRenderer pointMRY = yPoints[i].GetComponent<MeshRenderer>();
                StyleManager.SetAxisYMaterial(pointMRY);

                MeshRenderer pointMRZ = zPoints[i].GetComponent<MeshRenderer>();
                StyleManager.SetAxisXMaterial(pointMRZ);

                MeshRenderer signMRX = xSigns[i].GetComponent<MeshRenderer>();
                StyleManager.SetPointSignAttr(xSigns[i]);
                StyleManager.SetAxisSignXMaterial(signMRX);

                MeshRenderer signMRY = ySigns[i].GetComponent<MeshRenderer>();
                StyleManager.SetPointSignAttr(ySigns[i]);
                StyleManager.SetAxisSignYMaterial(signMRY);

                MeshRenderer signMRZ = zSigns[i].GetComponent<MeshRenderer>();
                StyleManager.SetPointSignAttr(zSigns[i]);
                StyleManager.SetAxisSignZMaterial(signMRZ);
            }

        };

    }

    delegate void SetMaterial(MeshRenderer renderer);

    public void AxisZHide() {
        zAxis.SetActive(false);
        zArrow.SetActive(false);
        foreach (GameObject point in zPoints) { 
            point.SetActive(false);
        }
        foreach (GameObject sign in Signz)
        {
            sign.SetActive(false);
        }

    }
    public void AxisZShow()
    {
        zAxis.SetActive(true);
        zArrow.SetActive(true);
        foreach (GameObject point in zPoints)
        {
            point.SetActive(true);
        }
        foreach (GameObject sign in Signz)
        {
            sign.SetActive(true);
        }

    }
    private void InitPointsSigns(string name, ref Vector3[] pos, ref GameObject[] points, ref TextMesh[] signs, Vector3 unit, int size, SetMaterial axisDelegate, SetMaterial signDelegate)
    {
        for (int i = 0; i < size / 2; i++)
        {
            float num = i + 1;
            pos[i] = unit * num;
            pos[size - i - 1] = unit * -num;

            MeshRenderer pointMRP, pointMRN, signMRP, signMRN;

            points[i] = InitPoint(name + num, pos[i], out pointMRP);
            points[size - i - 1] = InitPoint(name + -num, pos[size - i - 1], out pointMRN);

            axisDelegate(pointMRP);
            axisDelegate(pointMRN);

            signs[i] = InitSign(name, num.ToString(), pos[i], out signMRP);
            signs[size - i - 1] = InitSign(name, (-num).ToString(), pos[size - i - 1], out signMRN);

            signDelegate(signMRP);
            signDelegate(signMRN);
        }

        for (int i = 0; i < size; i++)
            StyleManager.SetPointSignAttr(signs[i]);

    }

    private void OnCameraRotate()
    {
        xAxis.transform.rotation = RotationOfDirection(Vector3.right);
        yAxis.transform.rotation = RotationOfDirection(Vector3.up);
        zAxis.transform.rotation = RotationOfDirection(Vector3.forward);

        Quaternion xQuaternion = RotationOfNormal(Vector3.right);
        Quaternion yQuaternion = RotationOfNormal(Vector3.up);
        Quaternion zQuaternion = RotationOfNormal(Vector3.forward);

        for (int i = 0; i < count - 1; i++)
        {
            xPoints[i].transform.rotation = xQuaternion;
            yPoints[i].transform.rotation = yQuaternion;
            zPoints[i].transform.rotation = zQuaternion;
        }

        for (int i = 0; i < count - 1; i++)
        {
            xSigns[i].transform.position = PositionOfDirection(xPos[i], Vector3.up, POINT_SIGN_BIAS);
            ySigns[i].transform.position = PositionOfDirection(yPos[i], Vector3.left, POINT_SIGN_BIAS);
            zSigns[i].transform.position = PositionOfDirection(zPos[i], Vector3.up, POINT_SIGN_BIAS);
        }

        // Arrow
        Ray xRay = new Ray(Vector3.zero, Vector3.left);
        Ray yRay = new Ray(Vector3.zero, Vector3.down);
        Ray zRay = new Ray(Vector3.zero, Vector3.back);

        Vector3 xArrowPos, yArrowPos, zArrowPos;

        geoCamera.IntersectRay(xRay, out xArrowPos);
        geoCamera.IntersectRay(yRay, out yArrowPos);
        geoCamera.IntersectRay(zRay, out zArrowPos);

        float offset = count / 2.0f;
        xArrowPos.x = Mathf.Min(offset, xArrowPos.x);
        yArrowPos.y = Mathf.Min(offset, yArrowPos.y);
        zArrowPos.z = Mathf.Min(offset, zArrowPos.z);

        xArrow.transform.position = xArrowPos;
        yArrow.transform.position = yArrowPos;
        zArrow.transform.position = zArrowPos;

        xArrowPos.x -= AXIS_SIGN_OFFSET;
        yArrowPos.y -= AXIS_SIGN_OFFSET;
        zArrowPos.z -= AXIS_SIGN_OFFSET;

        xSign.transform.position = PositionOfDirection(xArrowPos, Vector3.forward, AXIS_SIGN_BIAS);
        ySign.transform.position = PositionOfDirection(yArrowPos, Vector3.left, AXIS_SIGN_BIAS);
        zSign.transform.position = PositionOfDirection(zArrowPos, Vector3.up, AXIS_SIGN_BIAS);
    }


    private GameObject InitWrapper(string name)
    {
        GameObject wrapper = new GameObject(name);
        wrapper.transform.position = Vector3.zero;
        wrapper.transform.SetParent(transform);

        return wrapper;
    }

    private GameObject InitAxis(string name, out MeshRenderer meshRenderer)
    {
        GameObject axis = InitLine("axis " + name, out meshRenderer);
        axis.transform.SetParent(axisWrapper.transform);
        axis.transform.position = Vector3.zero;
        axis.transform.localScale = new Vector3(count, 1, 1);

        return axis;
    }

    private GameObject InitPoint(string name, Vector3 position, out MeshRenderer meshRenderer)
    {
        GameObject point = InitLine("point " + name, out meshRenderer);
        point.transform.SetParent(pointWrapper.transform);
        point.transform.position = position;
        point.transform.localScale = new Vector3(POINT_LENGTH, 1, 1);

        return point;
    }

    private GameObject InitLine(string name, out MeshRenderer meshRenderer)
    {
        GameObject line = new GameObject(name);
        MeshFilter meshFilter = line.AddComponent<MeshFilter>();
        meshRenderer = line.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = lineMesh;

        return line;
    }

    private GameObject InitArrow(string name, Vector3 position, Vector3 rotation, out MeshRenderer meshRenderer)
    {
        GameObject arrow = new GameObject("arrow " + name);
        arrow.transform.SetParent(arrowWrapper.transform);
        MeshFilter meshFilter = arrow.AddComponent<MeshFilter>();
        meshRenderer = arrow.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = ArrowMesh;

        arrow.transform.position = position;
        arrow.transform.eulerAngles = rotation;
        arrow.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);

        return arrow;
    }

    private TextMesh InitSign(string name, string text, Vector3 position, out MeshRenderer meshRenderer)
    {
        GameObject sign = new GameObject("sign " + name + text);
        if (name == "x")
        {
            Signx[sigx] = sign;
            sigx++;
        }
        else if (name == "y") {
            Signy[sigy] = sign;
            sigy++;
        }
        else if (name == "z")
        {
            Signz[sigz] = sign;
            sigz++;
        }
        sign.transform.SetParent(signWrapper.transform);
        TextMesh textMesh = sign.AddComponent<TextMesh>();
        meshRenderer = textMesh.GetComponent<MeshRenderer>();

        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = text;

        sign.transform.position = position;

        return textMesh;
    }

    private Quaternion RotationOfNormal(Vector3 normal)
    {
        Vector3 cameraView = -geoCamera.transform.position.normalized;
        Vector3 direction = Vector3.Cross(normal, cameraView);
        return RotationOfDirection(direction);
    }

    private Quaternion RotationOfDirection(Vector3 direction)
    {
        Vector3 tangent = direction.normalized;
        Vector3 cameraView = -geoCamera.transform.position.normalized;
        float distance = -Vector3.Dot(tangent, cameraView);
        Vector3 normal = (cameraView + tangent * distance).normalized;
        if (normal == Vector3.zero)
            normal = geoCamera.transform.TransformDirection(Vector3.right);
        Vector3 up = Vector3.Cross(tangent, normal);

        return Quaternion.LookRotation(normal, up);
    }

    protected Vector3 PositionOfDirection(Vector3 origin, Vector3 direction, float bias)
    {
        direction = geoCamera.transform.InverseTransformDirection(direction);
        direction.z = 0;
        direction = geoCamera.transform.TransformDirection(direction.normalized);

        return origin + bias * direction;
    }


    private Mesh LineMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);
        vertices[1] = new Vector3(-0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[2] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, 1, 0);
        vertices[3] = new Vector3(0.5f, 0, 0) + LINE_RADIUS * new Vector3(0, -1, 0);

        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(1, 0);

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();

        return mesh;
    }
    
}
