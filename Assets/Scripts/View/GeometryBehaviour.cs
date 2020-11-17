using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum GeometryShadeType
{
    Frame,
    Ray,
    Solid
}

public class GeometryBehaviour : MonoBehaviour
{
    GeoCamera geoCamera;
    Geometry geometry;

    GameObject vertexWrapper;
    GameObject edgeWrapper;
    GameObject faceWrapper;
    GameObject signWrapper;
    GameObject gizmoWrapper;
    GameObject lineWrapper;

    public Dictionary<GeoElement, ElementBehaviour> elementMap;
    Dictionary<GeoVertex, VertexBehaviour> vertexMap;
    Dictionary<GeoEdge, EdgeBehaviour> edgeMap;
    Dictionary<GeoFace, FaceBehaviour> faceMap;
    Dictionary<GeoCircle, CircleBehaviour> circleMap;
    Dictionary<GeoCircular, CircularBehaviour> circularMap;
    Dictionary<GeoLine, LineBehaviour> lineMap;
    Dictionary<int, SignBehaviour> signMap;

    Dictionary<Gizmo, GizmoBehaviour> gizmoMap;

    List<GeoElement> hideElements;

    public Action<int> OnElementDisplayChange;


    GeometryShadeType shadeType;

    // Mesh mesh;

    // MeshFilter meshFilter;
    // MeshRenderer meshRenderer;

    public void Init(GeoCamera camera)
    {
        geoCamera = camera;

        vertexWrapper = InitWrapper("Vertex");
        edgeWrapper = InitWrapper("Edge");
        faceWrapper = InitWrapper("Face");
        signWrapper = InitWrapper("Sign");
        gizmoWrapper = InitWrapper("Gizmo");
        lineWrapper = InitWrapper("Line");

        elementMap = new Dictionary<GeoElement, ElementBehaviour>();
        hideElements = new List<GeoElement>();

        vertexMap = new Dictionary<GeoVertex, VertexBehaviour>();
        edgeMap = new Dictionary<GeoEdge, EdgeBehaviour>();
        faceMap = new Dictionary<GeoFace, FaceBehaviour>();
        circleMap = new Dictionary<GeoCircle, CircleBehaviour>();
        circularMap = new Dictionary<GeoCircular, CircularBehaviour>();
        signMap = new Dictionary<int, SignBehaviour>();
        gizmoMap = new Dictionary<Gizmo, GizmoBehaviour>();
        lineMap=new Dictionary<GeoLine, LineBehaviour>();

    }

    public void InitGeometry(Geometry geo)
    {
        if (geometry != null && geometry.Type == GeometryType.Function)
            Clear(2);
        else
            Clear(3);

        geometry = geo;

        AddElements();
    }

    public GeometryType GetGeometryType() {
        if (geometry == null)
            return GeometryType.Common;
        return geometry.Type;
    }

    public void AddElements()
    {
        GeoVertex[] vertices = geometry.GeoVertices();
        GeoEdge[] edges = geometry.GeoEdges();
        GeoFace[] faces = geometry.GeoFaces();
        GeoCircle[] circles = geometry.GeoCircles();
        GeoCircular[] circulars = geometry.GeoCirculars();
        GeoLine[] lines = geometry.GeoLines();

        // New Geometry
        // mesh = GeometryToMesh();

        // GameObject geometryObject = new GameObject(geometry.Name);
        // geometryObject.transform.position = Vector3.zero;
        // geometryObject.transform.SetParent(transform);

        // meshFilter = geometryObject.AddComponent<MeshFilter>();
        // meshRenderer = geometryObject.AddComponent<MeshRenderer>();
        // meshFilter.mesh = mesh;
        // StyleManager.SetGeometryMaterial(meshRenderer);
        // StyleManager.OnStyleChange += () =>
        // {
        //     StyleManager.SetGeometryMaterial(meshRenderer);
        // };


        // New Vertex
        for (int i = 0; i < vertices.Length; i++)
            AddVertex(vertices[i]);

        // New Edge
        for (int i = 0; i < edges.Length; i++)
            AddEdge(edges[i]);

        // New Face
        for (int i = 0; i < faces.Length; i++)
            AddFace(faces[i]);

        // New Circles
        for (int i = 0; i < circles.Length; i++)
            AddCircle(circles[i]);

        // New Circulars
        for (int i = 0; i < circulars.Length; i++)
            AddCircular(circulars[i]);

        //New Lines
        for (int i = 0; i < lines.Length; i++)
            AddLine(lines[i]);

        // New Signs
        for (int i = 0; i < geometry.UnitCount(); i++)
            AddSign(i);


    }

    public void AddFaces() {
        GeoVertex[] vertices = geometry.GeoVertices();
        GeoEdge[] edges = geometry.GeoEdges();
        GeoFace[] faces = geometry.GeoFaces();
        // New Vertex
        for (int i = 0; i < vertices.Length; i++)
            AddVertex(vertices[i]);

        // New Edge
        for (int i = 0; i < edges.Length; i++)
            AddEdge(edges[i]);

        // New Face
        for (int i = 0; i < faces.Length; i++)
            AddFace(faces[i]);
    }


    public void Clear(int dimension)
    {
        geometry = null;
        StatusButton lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        if (dimension == 2)
        {
            geoCamera.Trigger2DRAnimation();
            lockButton.SetStatus(1);
        }
        else
        {
            geoCamera.TriggerCenterRAnimation();
            lockButton.SetStatus(0);
        }
        
        // Clear Element
        clearElements();

        // Clear Gizmos
        foreach (KeyValuePair<Gizmo, GizmoBehaviour> pair in gizmoMap)
            Destroy(pair.Value.gameObject);
        gizmoMap.Clear();
    }

    public void clearElements()
    {
        elementMap.Clear();

        hideElements.Clear();

        // Clear Circle
        foreach (KeyValuePair<GeoCircle, CircleBehaviour> pair in circleMap)
            Destroy(pair.Value.gameObject);
        circleMap.Clear();

        // Clear Circular
        foreach (KeyValuePair<GeoCircular, CircularBehaviour> pair in circularMap)
            Destroy(pair.Value.gameObject);
        circularMap.Clear();

        // Clear Vertex
        foreach (KeyValuePair<GeoVertex, VertexBehaviour> pair in vertexMap)
            Destroy(pair.Value.gameObject);
        vertexMap.Clear();

        // Clear Edge
        foreach (KeyValuePair<GeoEdge, EdgeBehaviour> pair in edgeMap)
            Destroy(pair.Value.gameObject);
        edgeMap.Clear();

        // Clear Face
        foreach (KeyValuePair<GeoFace, FaceBehaviour> pair in faceMap)
            Destroy(pair.Value.gameObject);
        faceMap.Clear();

        //Clear Line
        foreach (KeyValuePair<GeoLine, LineBehaviour> pair in lineMap)
            Destroy(pair.Value.gameObject);
        lineMap.Clear();

        // Clear Signs
        foreach (KeyValuePair<int, SignBehaviour> pair in signMap)
            Destroy(pair.Value.gameObject);
        signMap.Clear();
    }

    public void clearExtraElements() 
    {
        clearElements();
        AddElements();
    }

    private GameObject InitWrapper(string name)
    {
        GameObject wrapper = new GameObject(name);
        wrapper.transform.position = Vector3.zero;
        wrapper.transform.SetParent(transform);

        return wrapper;
    }

    #region GeoElement
    public Dictionary<GeoElement, ElementBehaviour> GetGeoElements()
    {
        return elementMap;
    }
    
    private void AddVertex(GeoVertex geoVertex)
    {
        GameObject vertexObject = new GameObject(geoVertex.ToString());
        vertexObject.transform.SetParent(vertexWrapper.transform);

        VertexBehaviour vertexBehaviour = vertexObject.AddComponent<VertexBehaviour>();
        vertexBehaviour.Init(geoVertex, geoCamera);
        vertexBehaviour.SetData(geometry.Vertex(geoVertex));

        vertexMap.Add(geoVertex, vertexBehaviour);
        elementMap.Add(geoVertex, vertexBehaviour);
    }

    private void AddEdge(GeoEdge geoEdge)
    {
        GameObject lineObject = new GameObject(geoEdge.ToString());
        lineObject.transform.SetParent(edgeWrapper.transform);

        EdgeBehaviour edgeBehaviour = lineObject.AddComponent<EdgeBehaviour>();
        edgeBehaviour.Init(geoEdge, geoCamera);

        edgeBehaviour.SetData(geometry.Edge(geoEdge), geometry.EdgeCenterDirection(geoEdge));

        edgeMap.Add(geoEdge, edgeBehaviour);
        elementMap.Add(geoEdge, edgeBehaviour);
    }

    private void AddFace(GeoFace geoFace)
    {
        GameObject planeObject = new GameObject(geoFace.ToString());
        planeObject.transform.SetParent(faceWrapper.transform);

        FaceBehaviour faceBehaviour = planeObject.AddComponent<FaceBehaviour>();
        faceBehaviour.Init(geoFace);

        faceBehaviour.SetData(geometry.Face(geoFace));

        faceMap.Add(geoFace, faceBehaviour);
        elementMap.Add(geoFace, faceBehaviour);
    }

    private void AddCircle(GeoCircle geoCircle)
    {
        GameObject planeObject = new GameObject(geoCircle.ToString());
        planeObject.transform.SetParent(faceWrapper.transform);

        CircleBehaviour circleBehaviour = planeObject.AddComponent<CircleBehaviour>();
        circleBehaviour.Init(geoCircle);

        circleBehaviour.SetData(geometry.Circle(geoCircle));

        circleMap.Add(geoCircle, circleBehaviour);
        elementMap.Add(geoCircle, circleBehaviour);
    }

    private void AddCircular(GeoCircular geoCircular)
    {
        GameObject planeObject = new GameObject(geoCircular.ToString());
        planeObject.transform.SetParent(faceWrapper.transform);

        CircularBehaviour circularBehaviour = planeObject.AddComponent<CircularBehaviour>();
        circularBehaviour.Init(geoCircular, geoCamera);

        circularBehaviour.SetData(geometry.Circular(geoCircular));

        circularMap.Add(geoCircular, circularBehaviour);
        elementMap.Add(geoCircular, circularBehaviour);
    }

    //generate function line
    private void AddLine(GeoLine geoLine)
    {
        GameObject planeObject = new GameObject(geoLine.ToString());
        planeObject.transform.SetParent(lineWrapper.transform);

        LineBehaviour lineBehaviour = planeObject.AddComponent<LineBehaviour>();
        lineBehaviour.Init(geoLine, geoCamera);

        //lineBehaviour.SetData(geometry.Circular(geoLine));

        lineMap.Add(geoLine, lineBehaviour);
        elementMap.Add(geoLine, lineBehaviour);
    }


    private void UpdateVertex(GeoVertex geoVertex)
    {
        VertexBehaviour vertexBehaviour = vertexMap[geoVertex];
        vertexBehaviour.SetData(geometry.Vertex(geoVertex));
    }

    private void UpdateEdge(GeoEdge geoEdge)
    {
        EdgeBehaviour edgeBehaviour = edgeMap[geoEdge];
        edgeBehaviour.SetData(geometry.Edge(geoEdge), geometry.EdgeCenterDirection(geoEdge));
    }

    private void UpdateFace(GeoFace geoFace)
    {
        FaceBehaviour faceBehaviour = faceMap[geoFace];
        faceBehaviour.SetData(geometry.Face(geoFace));
    }

    private void UpdateCircle(GeoCircle geoCircle)
    {
        CircleBehaviour circleBehaviour = circleMap[geoCircle];
        circleBehaviour.SetData(geometry.Circle(geoCircle));
    }

    private void UpdateCircular(GeoCircular geoCircular)
    {
        CircularBehaviour circularBehaviour = circularMap[geoCircular];
        circularBehaviour.SetData(geometry.Circular(geoCircular));
    }

    private void RemoveVertex(GeoVertex geoVertex)
    {
        VertexBehaviour vertexBehaviour = vertexMap[geoVertex];
        Destroy(vertexBehaviour.gameObject);
        vertexMap.Remove(geoVertex);
    }

    private void RemoveEdge(GeoEdge geoEdge)
    {
        EdgeBehaviour edgeBehaviour = edgeMap[geoEdge];
        Destroy(edgeBehaviour.gameObject);
        edgeMap.Remove(geoEdge);
    }

    private void RemoveFace(GeoFace geoFace)
    {
        FaceBehaviour faceBehaviour = faceMap[geoFace];
        Destroy(faceBehaviour.gameObject);
        faceMap.Remove(geoFace);
    }

    private void RemoveCircle(GeoCircle geoCircle)
    {
        CircleBehaviour circleBehaviour = circleMap[geoCircle];
        Destroy(circleBehaviour.gameObject);
        circleMap.Remove(geoCircle);
    }

    private void RemoveCircular(GeoCircular geoCircular)
    {
        CircularBehaviour circularBehaviour = circularMap[geoCircular];
        Destroy(circularBehaviour.gameObject);
        circularMap.Remove(geoCircular);
    }

    public int EdgeSize() {
        return edgeMap.Count;
    }

    public bool ContainsEdge(GeoEdge edge) {
        return edgeMap.ContainsKey(edge);
    }
    #endregion

    #region Sign

    public void AddSign(int id)
    {
        GameObject signObject = new GameObject("sign" + id);
        signObject.transform.SetParent(signWrapper.transform);

        SignBehaviour signBehaviour = signObject.AddComponent<SignBehaviour>();
        signBehaviour.Init(id, geoCamera);
        Vector3 pos = geometry.UnitVector(id);
        Vector3 center = geometry.Center();
        signBehaviour.SetData(pos, center);
        signBehaviour.SetSign(geometry.VertexSign(id));

        signMap.Add(id, signBehaviour);
    }

    public void RemoveSign(int id)
    {
        SignBehaviour signBehaviour = signMap[id];
        Destroy(signBehaviour.gameObject);
        signMap.Remove(id);
    }

    #endregion

    #region Gizmo
    public void AddGizmoRight(Gizmo gizmo)
    {
        if (!(gizmo is GizmoRight))
            return;
        GizmoRight gizmoRight = (GizmoRight)gizmo;

        GameObject gizmoObject = new GameObject("right");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        RightBehaviour rightBehaviour = gizmoObject.AddComponent<RightBehaviour>();
        rightBehaviour.Init(geoCamera);

        CornerRefer corner = gizmoRight.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        rightBehaviour.SetData(origin, dir1, dir2);

        gizmoMap.Add(gizmoRight, rightBehaviour);
    }

    public void AddGizmoCorner(Gizmo gizmo)
    {
        if (!(gizmo is GizmoCorner))
            return;
        GizmoCorner gizmoCorner = (GizmoCorner)gizmo;

        GameObject gizmoObject = new GameObject("corner");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        CornerBehavior cornerBehavior = gizmoObject.AddComponent<CornerBehavior>();
        cornerBehavior.Init(geoCamera);

        CornerRefer corner = gizmoCorner.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        cornerBehavior.SetData(origin, dir1, dir2);

        gizmoMap.Add(gizmoCorner, cornerBehavior);
    }

    public void AddGizmoLength(Gizmo gizmo)
    {
        if (!(gizmo is GizmoLength))
            return;
        GizmoLength gizmoLength = (GizmoLength)gizmo;

        GameObject gizmoObject = new GameObject("length");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        LengthBehaviour lengthBehaviour = gizmoObject.AddComponent<LengthBehaviour>();
        lengthBehaviour.Init(geoCamera);

        EdgeRefer edge = gizmoLength.edge;
        Vector3 center = geometry.Center();
        Vector3 vertex1 = geometry.UnitVector(edge.id1);
        Vector3 vertex2 = geometry.UnitVector(edge.id2);
        float length = geometry.EdgeLength(edge.id1, edge.id2);
        lengthBehaviour.SetData(center, vertex1, vertex2, length);

        gizmoMap.Add(gizmoLength, lengthBehaviour);
    }

    public void AddGizmoAngle(Gizmo gizmo)
    {
        if (!(gizmo is GizmoAngle))
            return;
        GizmoAngle gizmoAngle = (GizmoAngle)gizmo;

        GameObject gizmoObject = new GameObject("angle");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        AngleBehaviour angleBehaviour = gizmoObject.AddComponent<AngleBehaviour>();
        angleBehaviour.Init(geoCamera);

        CornerRefer corner = gizmoAngle.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        float angle = geometry.CornerAngle(corner.id1, corner.id2, corner.id3);
        angleBehaviour.SetData(origin, dir1, dir2, angle);

        gizmoMap.Add(gizmoAngle, angleBehaviour);
    }

    public void AddGizmoArea(Gizmo gizmo)
    {
        if (!(gizmo is GizmoArea))
            return;
        GizmoArea gizmoArea = (GizmoArea)gizmo;

        GameObject gizmoObject = new GameObject("area");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        AreaBehaviour areaBehaviour = gizmoObject.AddComponent<AreaBehaviour>();
        areaBehaviour.Init(geoCamera);

        FaceRefer face = gizmoArea.face;
        Vector3 center = geometry.Center();
        Vector3[] vectors = new Vector3[face.ids.Length];
        for (int i = 0; i < face.ids.Length; i++)
            vectors[i] = geometry.UnitVector(face.ids[i]);
        float area = geometry.FaceArea(face.ids);
        areaBehaviour.SetData(center, vectors, area);

        gizmoMap.Add(gizmoArea, areaBehaviour);
    }

    public void AddGizmoSurface(Gizmo gizmo)
    {
        if (!(gizmo is GizmoSurface))
            return;
        GizmoSurface gizmoSurface = (GizmoSurface)gizmo;

        GameObject gizmoObject = new GameObject("surface");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        SurfaceBehaviour surfaceBehaviour = gizmoObject.AddComponent<SurfaceBehaviour>();
        surfaceBehaviour.Init(geoCamera);

        FaceRefer face = gizmoSurface.face;
        if (face.ids.Length < 3) 
            return; 
        Vector3 center = geometry.Center();
        Vector3 v1 = geometry.UnitVector(face.ids[0]);
        Vector3 v2 = geometry.UnitVector(face.ids[1]);
        Vector3 v3 = geometry.UnitVector(face.ids[2]);
        float radius = v3.z - v2.z;
        int pointCount = 4;
        Vector3[] vectors = new Vector3[pointCount];
        float angledegree = 360.0f;
        float angleRad = Mathf.Deg2Rad * angledegree;
        float angleCur = angleRad;
        float angledelta = angleRad / pointCount;
        for (int i = 0; i < pointCount; i++) {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);
            vectors[i] = new Vector3(radius * cosA, v2.y, radius * sinA);
            angleCur -= angledelta;
        }

        string area = geometry.SurfaceArea(face.ids);
        surfaceBehaviour.SetData(center, vectors, area);

        gizmoMap.Add(gizmoSurface, surfaceBehaviour);
    }

    public void AddGizmoVolume(Gizmo gizmo)
    {
        if (!(gizmo is GizmoVolume))
            return;
        GizmoVolume gizmoVolume = (GizmoVolume)gizmo;

        GameObject gizmoObject = new GameObject("volume");
        gizmoObject.transform.SetParent(gizmoWrapper.transform);

        VolumeBehaviour volumeBehaviour = gizmoObject.AddComponent<VolumeBehaviour>();
        volumeBehaviour.Init(geoCamera);

        FaceRefer face = gizmoVolume.face;
        if (face.ids.Length < 3) 
            return;
        Vector3 center = geometry.Center();
        Vector3 v1 = geometry.UnitVector(face.ids[0]);
        Vector3 v2 = geometry.UnitVector(face.ids[1]);
        Vector3 v3 = geometry.UnitVector(face.ids[2]);
        float radius = v3.z - v2.z;
        int pointCount = 4;
        Vector3[] vectors = new Vector3[pointCount];
        float angledegree = 360.0f;
        float angleRad = Mathf.Deg2Rad * angledegree;
        float angleCur = angleRad;
        float angledelta = angleRad / pointCount;
        for (int i = 0; i < pointCount; i++) {
            float cosA = Mathf.Cos(angleCur);
            float sinA = Mathf.Sin(angleCur);
            vectors[i] = new Vector3(radius * cosA, v1.y, radius * sinA);
            angleCur -= angledelta;
        }

        string area = geometry.Volume(face.ids);
        volumeBehaviour.SetData(center, vectors, area);

        gizmoMap.Add(gizmoVolume, volumeBehaviour);
    }

    public void UpdateGizmoRight(Gizmo gizmo)
    {
        if (!(gizmo is GizmoRight))
            return;
        GizmoRight gizmoRight = (GizmoRight)gizmo;

        RightBehaviour rightBehaviour = (RightBehaviour)gizmoMap[gizmoRight];

        CornerRefer corner = gizmoRight.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        rightBehaviour.SetData(origin, dir1, dir2);
    }

    public void UpdateGizmoCorner(Gizmo gizmo)
    {
        if (!(gizmo is GizmoCorner))
            return;
        GizmoCorner gizmoCorner = (GizmoCorner)gizmo;

        CornerBehavior cornerBehavior = (CornerBehavior)gizmoMap[gizmoCorner];

        CornerRefer corner = gizmoCorner.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        cornerBehavior.SetData(origin, dir1, dir2);
    }

    public void UpdateGizmoLength(Gizmo gizmo)
    {
        if (!(gizmo is GizmoLength))
            return;
        GizmoLength gizmoLength = (GizmoLength)gizmo;

        LengthBehaviour lengthBehaviour = (LengthBehaviour)gizmoMap[gizmoLength];

        EdgeRefer edge = gizmoLength.edge;
        Vector3 center = geometry.Center();
        Vector3 vertex1 = geometry.UnitVector(edge.id1);
        Vector3 vertex2 = geometry.UnitVector(edge.id2);
        float length = geometry.EdgeLength(edge.id1, edge.id2);
        lengthBehaviour.SetData(center, vertex1, vertex2, length);
    }

    public void UpdateGizmoAngle(Gizmo gizmo)
    {
        if (!(gizmo is GizmoAngle))
            return;
        GizmoAngle gizmoAngle = (GizmoAngle)gizmo;

        AngleBehaviour angleBehaviour = (AngleBehaviour)gizmoMap[gizmoAngle];

        CornerRefer corner = gizmoAngle.corner;
        Vector3 origin = geometry.UnitVector(corner.id2);
        Vector3 dir1 = geometry.Direction(corner.id2, corner.id1);
        Vector3 dir2 = geometry.Direction(corner.id2, corner.id3);
        float angle = geometry.CornerAngle(corner.id1, corner.id2, corner.id3);
        angleBehaviour.SetData(origin, dir1, dir2, angle);
    }

    public void UpdateGizmoArea(Gizmo gizmo)
    {
        if (!(gizmo is GizmoArea))
            return;
        GizmoArea gizmoArea = (GizmoArea)gizmo;

        AreaBehaviour areaBehaviour = (AreaBehaviour)gizmoMap[gizmoArea];

        FaceRefer face = gizmoArea.face;
        Vector3 center = geometry.Center();
        Vector3[] vectors = new Vector3[face.ids.Length];
        for (int i = 0; i < face.ids.Length; i++)
            vectors[i] = geometry.UnitVector(face.ids[i]);
        float area = geometry.FaceArea(face.ids);
        areaBehaviour.SetData(center, vectors, area);
    }

    public void RemoveGizmoRight(Gizmo gizmo)
    {
        if (!(gizmo is GizmoRight))
            return;
        GizmoRight gizmoRight = (GizmoRight)gizmo;

        RightBehaviour rightBehaviour = (RightBehaviour)gizmoMap[gizmoRight];
        Destroy(rightBehaviour.gameObject);

        gizmoMap.Remove(gizmoRight);
    }

    public void RemoveGizmoCorner(Gizmo gizmo)
    {
        if (!(gizmo is GizmoCorner))
            return;
        GizmoCorner gizmoCorner = (GizmoCorner)gizmo;

        CornerBehavior cornerBehavior = (CornerBehavior)gizmoMap[gizmoCorner];
        Destroy(cornerBehavior.gameObject);

        gizmoMap.Remove(gizmoCorner);
    }

    public void RemoveGizmoLength(Gizmo gizmo)
    {
        if (!(gizmo is GizmoLength))
            return;
        GizmoLength gizmoLength = (GizmoLength)gizmo;

        LengthBehaviour lengthBehaviour = (LengthBehaviour)gizmoMap[gizmoLength];
        Destroy(lengthBehaviour.gameObject);

        gizmoMap.Remove(gizmoLength);
    }

    public void RemoveGizmoAngle(Gizmo gizmo)
    {
        if (!(gizmo is GizmoAngle))
            return;
        GizmoAngle gizmoAngle = (GizmoAngle)gizmo;

        AngleBehaviour angleBehaviour = (AngleBehaviour)gizmoMap[gizmoAngle];
        Destroy(angleBehaviour.gameObject);

        gizmoMap.Remove(gizmoAngle);
    }

    public void RemoveGizmoArea(Gizmo gizmo)
    {
        if (!(gizmo is GizmoArea))
            return;
        GizmoArea gizmoArea = (GizmoArea)gizmo;

        AreaBehaviour areaBehaviour = (AreaBehaviour)gizmoMap[gizmoArea];
        Destroy(areaBehaviour.gameObject);

        gizmoMap.Remove(gizmoArea);
    }

    public void RemoveGizmoSurface(Gizmo gizmo)
    {
        if (!(gizmo is GizmoSurface))
            return;
        GizmoSurface gizmoSurface = (GizmoSurface)gizmo;

        SurfaceBehaviour surfaceBehaviour = (SurfaceBehaviour)gizmoMap[gizmoSurface];
        Destroy(surfaceBehaviour.gameObject);

        gizmoMap.Remove(gizmoSurface);
    }

    public void RemoveGizmoVolume(Gizmo gizmo)
    {
        if (!(gizmo is GizmoVolume))
            return;
        GizmoVolume gizmoVolume = (GizmoVolume)gizmo;

        VolumeBehaviour volumeBehaviour = (VolumeBehaviour)gizmoMap[gizmoVolume];
        Destroy(volumeBehaviour.gameObject);

        gizmoMap.Remove(gizmoVolume);
    }
    #endregion

    #region Operation

    public void HighlightVertex(GeoVertex geoVertex, bool highlight)
    {
        vertexMap[geoVertex].SetHighlight(highlight);
    }

    public void HighlightEdge(GeoEdge geoEdge, bool highlight)
    {
        edgeMap[geoEdge].SetHighlight(highlight);
    }

    public void HighlightFace(GeoFace geoFace, bool highlight)
    {
        faceMap[geoFace].SetHighlight(highlight);
    }

    public void AddElement(GeoElement geoElement)
    {
        if (geoElement is GeoVertex)
            AddVertex((GeoVertex)geoElement);
        else if (geoElement is GeoEdge)
            AddEdge((GeoEdge)geoElement);
        else if (geoElement is GeoFace)
            AddFace((GeoFace)geoElement);
        else if (geoElement is GeoCircle)
            AddCircle((GeoCircle)geoElement);
        else if (geoElement is GeoCircular)
            AddCircular((GeoCircular)geoElement);
    }

    public void UpdateElement(GeoElement geoElement)
    {
        if (geoElement is GeoVertex)
            UpdateVertex((GeoVertex)geoElement);
        else if (geoElement is GeoEdge)
            UpdateEdge((GeoEdge)geoElement);
        else if (geoElement is GeoFace)
            UpdateFace((GeoFace)geoElement);
        else if (geoElement is GeoCircle)
            UpdateCircle((GeoCircle)geoElement);
        else if (geoElement is GeoCircular)
            UpdateCircular((GeoCircular)geoElement);
    }

    public void RemoveElement(GeoElement geoElement)
    {
        if (geoElement is GeoVertex)
            RemoveVertex((GeoVertex)geoElement);
        else if (geoElement is GeoEdge)
            RemoveEdge((GeoEdge)geoElement);
        else if (geoElement is GeoFace)
            RemoveFace((GeoFace)geoElement);
        else if (geoElement is GeoCircle)
            RemoveCircle((GeoCircle)geoElement);
        else if (geoElement is GeoCircular)
            RemoveCircular((GeoCircular)geoElement);
    }

    public void AddGizmo(Gizmo gizmo)
    {
        string methodName = "Add" + gizmo.name;
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method != null)
            method.Invoke(this, new System.Object[] { gizmo });
    }

    public void UpdateGizmo(Gizmo gizmo)
    {
        string methodName = "Update" + gizmo.name;
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method != null)
            method.Invoke(this, new System.Object[] { gizmo });
    }

    public void RemoveGizmo(Gizmo gizmo)
    {
        string methodName = "Remove" + gizmo.name;
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(methodName);
        if (method != null)
            method.Invoke(this, new System.Object[] { gizmo });
    }

    public void UpdateElements()
    {
        foreach (KeyValuePair<GeoVertex, VertexBehaviour> pair in vertexMap)
            UpdateVertex(pair.Key);

        foreach (KeyValuePair<GeoEdge, EdgeBehaviour> pair in edgeMap)
            UpdateEdge(pair.Key);

        foreach (KeyValuePair<GeoFace, FaceBehaviour> pair in faceMap)
            UpdateFace(pair.Key);
    }

    public void UpdateGizmos()
    {
        foreach (KeyValuePair<Gizmo, GizmoBehaviour> pair in gizmoMap)
            UpdateGizmo(pair.Key);
    }

    public void UpdateSignPosition(int id)
    {
        Vector3 pos = geometry.UnitVector(id);
        Vector3 center = geometry.Center();
        signMap[id].SetData(pos, center);
    }

    public void UpdateSignsPosition()
    {
        foreach (KeyValuePair<int, SignBehaviour> pair in signMap)
            UpdateSignPosition(pair.Key);
    }

    public void UpdateSignText(int id)
    {
        signMap[id].SetSign(geometry.VertexSign(id));
    }

    public void SignText(int id, string text)
    {
        signMap[id].SetSign(text);
    }

    public void SignState(int id, SignState state)
    {
        signMap[id].SetState(state);
    }

    public void GeometrySignChanged(int id)
    {
        signMap[id].SetSign(geometry.VertexSign(id));
    }

    public void GeometryElementColorChange(GeoElement geoElement, int color)
    {
        ElementBehaviour elementBehaviour = elementMap[geoElement];
        elementBehaviour.SetColorIndex(color);
    }

    public void GeometryElementStyleChange(GeoElement geoElement, int style)
    {
        ElementBehaviour elementBehaviour = elementMap[geoElement];
        elementBehaviour.SetStyleIndex(style);
    }

    public bool GeometryElementDiaplay(GeoElement geoElement)
    {
        ElementBehaviour elementBehaviour = elementMap[geoElement];
        return elementBehaviour.GetVisible();
    }

    public void GeometryElementDiaplayChange(GeoElement geoElement, bool visible)
    {
        ElementBehaviour elementBehaviour = elementMap[geoElement];
        elementBehaviour.SetVisible(visible);

        if (visible)
            hideElements.Remove(geoElement);
        else
            hideElements.Add(geoElement);

        if (OnElementDisplayChange != null)
            OnElementDisplayChange(hideElements.Count);
    }

    public void GeometryShowAllElements()
    {
        foreach (GeoElement geoElement in hideElements)
        {
            ElementBehaviour elementBehaviour = elementMap[geoElement];
            elementBehaviour.SetVisible(true);
        }

        hideElements.Clear();
    }

    public void SetShadeType(GeometryShadeType type)
    {
        shadeType = type;
        UpdateGeometryShade();
    }

    public void UpdateGeometryShade()
    {
        float alpha = 1f;
        bool interactable = true;
        switch (shadeType)
        {
            case GeometryShadeType.Frame:
                alpha = 0;
                interactable = false;
                break;
            case GeometryShadeType.Ray:
                alpha = 0.5f;
                break;
            case GeometryShadeType.Solid:
                alpha = 1f;
                break;
        }

        foreach (KeyValuePair<GeoFace, FaceBehaviour> pair in faceMap)
        {
            pair.Value.SetAlpha(alpha);
            pair.Value.SetInteractable(interactable);
        }
    }

    #endregion

    // private Mesh GeometryToMesh()
    // {
    //     Mesh mesh = new Mesh();
    //     GeoVertex[] vertices = geometry.GeoVertices();
    //     GeoFace[] faceIndices = geometry.GeoFaces();

    //     List<Vector3> meshVertices = new List<Vector3>();
    //     List<int> meshTriangles = new List<int>();
    //     for (int i = 0; i < faceIndices.Length; i++)
    //     {
    //         VertexUnit[] faceUnits = faceIndices[i].units;
    //         int startIndex = meshVertices.Count;
    //         for (int j = 0; j < faceUnits.Length; j++)
    //         {
    //             Vector3 point = faceUnits[j].position;
    //             meshVertices.Add(point);
    //         }
    //         for (int j = 1; j < faceUnits.Length - 1; j++)
    //         {
    //             meshTriangles.Add(startIndex);
    //             meshTriangles.Add(startIndex + j);
    //             meshTriangles.Add(startIndex + j + 1);
    //         }
    //     }

    //     mesh.vertices = meshVertices.ToArray();
    //     mesh.triangles = meshTriangles.ToArray();

    //     mesh.RecalculateNormals();
    //     return mesh;
    // }

}
