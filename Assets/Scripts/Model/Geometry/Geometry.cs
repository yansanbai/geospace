using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum GeometryType //物体类型
{
    Common,
    General,
    Cubio,
    TriPyd,
    ResolvedBody,
    Function
}

public abstract class Geometry
{
    public string Name;
    public GeometryType Type;

    private Dictionary<int, VertexUnit> vertices; //dictionary键值对映射
    private Dictionary<int, VertexUnit> baseVertices;
    private Dictionary<int, Vector3> normals;

    private int vertexId;
    private List<GeoVertex> geoVertices;
    private List<GeoEdge> geoEdges;
    private List<GeoFace> geoFaces;
    private List<GeoCircle> geoCircles;
    private List<GeoCircular> geoCirculars;
    private List<GeoLine> geoLines;

    private Vector3 center;

    private Dictionary<string, int> signVertexMap;
    private Dictionary<int, string> vertexSignMap;

    private List<Gizmo> gizmos;

    public Constructor Constructor;
    public Assistor Assistor;
    public Implement Implement;

    public virtual void Init()
    {
        vertexId = 0;
        vertices = new Dictionary<int, VertexUnit>();
        baseVertices = new Dictionary<int, VertexUnit>();
        normals = new Dictionary<int, Vector3>();

        geoVertices = new List<GeoVertex>();
        geoEdges = new List<GeoEdge>();
        geoFaces = new List<GeoFace>();
        geoCircles = new List<GeoCircle>();
        geoCirculars = new List<GeoCircular>();
        geoLines = new List<GeoLine>();

        signVertexMap = new Dictionary<string, int>();
        vertexSignMap = new Dictionary<int, string>();

        gizmos = new List<Gizmo>();
    }

    protected void InitDatas()
    {
        RefreshVertexNormals();
        RefreshCenter();
    }


    #region Unit

    public int UnitCount()
    {
        return vertices.Count;
    }

    public Vector3 UnitVector(int id)
    {
        try {
            return vertices[id].Position();
        } catch {
            return new Vector3(0, 0, 0);
        }
    }

    public VertexUnit[] VertexUnits()
    {
        return vertices.Values.ToArray();
    }

    public VertexUnit[] BaseVertexUnits()
    {
        return baseVertices.Values.ToArray();
    }

    public void SetUnitVector(int id, Vector3 vector)
    {
        VertexUnit unit = vertices[id];
        unit.SetPosition(vector);

        foreach (VertexUnit dependency in unit.dependencies)
            dependency.RefreshPosition();
    }

    public VertexUnit VertexUnit(int id)
    {
        return vertices[id];
    }

    public HashSet<GeoElement> VertexUnitObserveElements(int id)
    {
        List<GeoElement> list = vertices[id].TotalObserveElements();
        return new HashSet<GeoElement>(list);
    }

    public HashSet<Gizmo> VertexUnitObserveGizmos(int id)
    {
        List<Gizmo> list = vertices[id].TotalObserveGizmos();
        return new HashSet<Gizmo>(list);
    }

    public void VertexUnitSetId(VertexUnit unit, int offset)
    {
        unit.id = vertexId + offset;
    }

    protected void AddBaseVertex(VertexUnit unit)
    {
        unit.isBase = true;
        VertexUnitSetId(unit, 0);
        AddVertexUnit(unit);
        baseVertices[unit.id] = unit;
    }

    public void AddVertexUnit(VertexUnit unit)
    {
        if (unit.id != vertexId)
        {
            Debug.LogWarning("Error Id: " + unit.id + "  " + vertexId);
            return;
        }

        vertices[vertexId] = unit;
        normals[vertexId] = Vector3.zero;

        string sign = AutoSign(vertexId);
        vertexSignMap[vertexId] = sign;
        signVertexMap[sign] = vertexId;

        unit.AddDependencies();

        vertexId++;

        // preferredSign
        if (unit.preferredSign != null)
            SetVertexSign(unit.id, unit.preferredSign);
    }

    public void RemoveVertexUnit(int id)
    {
        VertexUnit unit = vertices[id];
        if (unit.isBase)
            return;
        vertices.Remove(id);

        string sign = vertexSignMap[id];
        signVertexMap.Remove(sign);
        vertexSignMap.Remove(id);

        unit.RemoveDependencies();

        // foreach (VertexUnit dependency in unit.dependencies)// TODO
        //     RemoveVertexUnit(dependency.id);
    }



    #endregion


    #region GeoElement

    public GeoVertex[] GeoVertices()
    {
        return geoVertices.ToArray();
    }

    public GeoEdge[] GeoEdges()
    {
        return geoEdges.ToArray();
    }

    public GeoFace[] GeoFaces()
    {
        return geoFaces.ToArray();
    }

    public GeoCircle[] GeoCircles()
    {
        return geoCircles.ToArray();
    }

    public GeoCircular[] GeoCirculars()
    {
        return geoCirculars.ToArray();
    }

    public GeoLine[] GeoLines()
    {
        return geoLines.ToArray();
    }


    public void AddElement(GeoElement element)
    {
        if (element is GeoVertex)
            AddGeoVertex((GeoVertex)element);
        else if (element is GeoEdge)
            AddGeoEdge((GeoEdge)element);
        else if (element is GeoFace)
            AddGeoFace((GeoFace)element);
        else if (element is GeoCircle)
            AddGeoCircle((GeoCircle)element);
        else if (element is GeoCircular)
            AddGeoCircular((GeoCircular)element);
        else if (element is GeoLine)
            AddGeoLine((GeoLine)element);
    }

    public void Clear() {
        vertexId = 0;
        geoVertices.Clear();
        geoEdges.Clear();
        geoFaces.Clear();
        geoCircles.Clear();
        geoCirculars.Clear();
        geoLines.Clear();
        vertices.Clear();
    }


    public void AddGeoVertex(GeoVertex vertex)
    {
        geoVertices.Add(vertex);
        vertex.AddObserveElements();
    }

    public void AddGeoEdge(GeoEdge edge)
    {
        geoEdges.Add(edge);
        edge.AddObserveElements();
    }

    public void AddGeoFace(GeoFace face)
    {
        geoFaces.Add(face);
        face.AddObserveElements();
    }

    public void AddGeoCircle(GeoCircle circle)
    {
        geoCircles.Add(circle);
        circle.AddObserveElements();
    }

    public void AddGeoCircular(GeoCircular circular)
    {
        geoCirculars.Add(circular);
        circular.AddObserveElements();
    }

    public void RefreshGeoEdges()
    {
        List<GeoEdge> newEdges = new List<GeoEdge>();
        foreach (GeoEdge edge in newEdges) 
        {
            RemoveGeoEdge(edge);
        }
    }

    public void AddGeoLine(GeoLine line)
    {
        geoLines.Add(line);
        line.AddObserveElements();
    }

    public void RemoveElement(GeoElement element)
    {

        if (element is GeoVertex)
            RemoveGeoVertex((GeoVertex)element);
        else if (element is GeoEdge)
            RemoveGeoEdge((GeoEdge)element);
        else if (element is GeoFace)
            RemoveGeoFace((GeoFace)element);
        else if (element is GeoCircle)
            RemoveGeoCircle((GeoCircle)element);
        else if (element is GeoCircular)
            RemoveGeoCircular((GeoCircular)element);
        else if (element is GeoLine)
            RemoveGeoLine((GeoLine)element);
    }

    public void RemoveGeoVertex(GeoVertex vertex)
    {
        geoVertices.Remove(vertex);
        vertex.RemoveObserveElements();
    }

    public void RemoveGeoEdge(GeoEdge edge)
    {
        geoEdges.Remove(edge);
        edge.RemoveObserveElements();
    }

    public void RemoveGeoFace(GeoFace face)
    {
        geoFaces.Remove(face);
        face.RemoveObserveElements();
    }

    public void RemoveGeoCircle(GeoCircle circle)
    {
        geoCircles.Remove(circle);
        circle.RemoveObserveElements();
    }

    public void RemoveGeoCircular(GeoCircular circular)
    {
        geoCirculars.Remove(circular);
        circular.RemoveObserveElements();
    }

    public void RemoveGeoLine(GeoLine line)
    {
        geoLines.Remove(line);
        line.RemoveObserveElements();
    }

    public void SetElementColor(GeoElement element, int i)
    {
        element.color = i;
    }

    public void SetElementStyle(GeoElement element, int i)
    {
        element.style = i;
    }

    public void SetSpreadFaceStyle(GeoElement element, int i)
    {
        GeoFace geoFace;
        if (element is GeoFace) 
            geoFace = (GeoFace) element;
        else
            return;
        this.SetElementColor(element, i);
        GeometryBehaviour geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        switch (geoFace.faceType) {
            case FaceType.Normal:
                break;
            case FaceType.SpreadRectangle:
            case FaceType.SpreadFan:
                for (int index = 0; index < geoCirculars.Count; index++) {
                    GeoCircular circular = geoCirculars[0];
                    this.SetElementColor(circular, i);
                    geometryBehaviour.GeometryElementColorChange(circular, i);
                }
                break;
            case FaceType.SpreadCylinderCircle:
            case FaceType.SpreadConeCircle:
                Vector3[] vectors = geoFace.Face().Vertices;
                if (vectors.Length == 0)
                    break;
                Vector3 v = (vectors[0] + vectors[vectors.Length / 2]) / 2;
                Dictionary<GeoElement, ElementBehaviour> elementMap = geometryBehaviour.GetGeoElements();
                GeoFace face;
                foreach (GeoElement key in elementMap.Keys)
                {
                    if (key is GeoFace){
                        face = (GeoFace) key;
                        if (geoFace.faceType == face.faceType) {
                            if (face.faceType == FaceType.SpreadCylinderCircle) {
                                vectors = face.Face().Vertices;
                                if (vectors.Length == 0)
                                    continue;
                                Vector3 tmp = (vectors[0] + vectors[vectors.Length / 2]) / 2;
                                if ((tmp.y > 0 && v.y > 0) || (tmp.y < 0 && v.y < 0)) {
                                    this.SetElementColor(face, i);
                                    geometryBehaviour.GeometryElementColorChange(face, i); 
                                }
                            }
                            if (face.faceType == FaceType.SpreadConeCircle) {
                                this.SetElementColor(face, i);
                                geometryBehaviour.GeometryElementColorChange(face, i);  
                            }
                        }
                    }
                }
                break;
        }
    }

    #endregion

    #region Gizmos

    public Gizmo[] Gizmos()
    {
        return gizmos.ToArray();
    }

    public void AddGizmo(Gizmo gizmo)
    {
        gizmos.Add(gizmo);

        foreach (int id in gizmo.DependentIds())
            AddObserveGizmo(id, gizmo);
    }

    public void RemoveGizmo(Gizmo gizmo)
    {
        foreach (int id in gizmo.DependentIds())
            RemoveObserveGizmo(id, gizmo);

        gizmos.Remove(gizmo);
    }

    private void AddObserveGizmo(int id, Gizmo gizmo)
    {
        VertexUnit unit;
        vertices.TryGetValue(id, out unit);
        if (unit == null)
            return;
        unit.AddObserveGizmo(gizmo);
    }

    private void RemoveObserveGizmo(int id, Gizmo gizmo)
    {
        VertexUnit unit;
        vertices.TryGetValue(id, out unit);
        if (unit == null)
            return;
        unit.RemoveObserveGizmo(gizmo);
    }


    #endregion

    public Vertex Vertex(GeoVertex vertex)
    {
        return vertex.Vertex();
    }

    public Edge Edge(GeoEdge edge)
    {
        return edge.Edge();
    }

    public Face Face(GeoFace face)
    {
        return face.Face();
    }

    public Circle Circle(GeoCircle circle)
    {
        return circle.Circle();
    }

    public Circular Circular(GeoCircular circular)
    {
        return circular.Circular();
    }
    public Line Line(GeoLine line)
    {
        return line.Line();
    }

    public Vector3 Direction(int from, int to)
    {
        Vector3 dir = UnitVector(to) - UnitVector(from);
        return dir.normalized;
    }

    public float EdgeLength(int id1, int id2)
    {
        Vector3 v1 = UnitVector(id1);
        Vector3 v2 = UnitVector(id2);
        float length = Vector3.Magnitude(v1 - v2);

        return length;
    }

    public float CornerAngle(int id1, int id2, int id3)
    {
        Vector3 v1 = UnitVector(id1);
        Vector3 v2 = UnitVector(id2);
        Vector3 v3 = UnitVector(id3);
        float angle = Vector3.Angle(v1 - v2, v3 - v2);

        return angle;
    }

    public float FaceArea(int[] ids)
    {
        int count = ids.Length;
        if (count <= 2)
            return 0;

        float area = 0;
        for (int i = 1; i < count - 1; i++)
        {
            Vector3 v1 = UnitVector(ids[0]);
            Vector3 v2 = UnitVector(ids[i]);
            Vector3 v3 = UnitVector(ids[i + 1]);

            float a = Vector3.Magnitude(v1 - v2);
            float b = Vector3.Magnitude(v1 - v3);
            float c = Vector3.Magnitude(v2 - v3);

            float s = (a + b + c) / 2.0f;
            area += Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        return area;
    }

    public String SurfaceArea(int[] ids)
    {
        int count = ids.Length;
        float surface = 0;
        if (count < 3) {
            return surface.ToString(UIConstants.AreaFormat);
        }
        Vector3 v1 = UnitVector(ids[0]);
        Vector3 v2 = UnitVector(ids[1]);
        Vector3 v3 = UnitVector(ids[2]);
        if (count == 3)
        {
            float radius = v3.z - v2.z;
            float height = v1.y - v2.y;
            float circleArea = radius * radius;
            float sideRadius = Mathf.Sqrt(radius * radius + circleArea * circleArea);
            float sideArea = radius * sideRadius;
            surface = sideArea + circleArea;
        }
        else if (count == 4)
        {
            Vector3 v4 = UnitVector(ids[3]);
            float radius = v4.z - v1.z;
            float height = v1.y - v2.y;
            float circleArea = radius * radius;
            float sideArea = 2 * radius * height;
            surface = sideArea + 2 * circleArea;
        }
        return surface.ToString(UIConstants.AreaFormat) + " π";
    }

    public String Volume(int[] ids)
    {
        int count = ids.Length;
        float volume = 0;
        if (count < 3)
        {
            return volume.ToString();
        }
        Vector3 v1 = UnitVector(ids[0]);
        Vector3 v2 = UnitVector(ids[1]);
        Vector3 v3 = UnitVector(ids[2]);
        float radius = v3.z - v2.z;
        float height = v1.y - v2.y;
        volume = radius * radius * height;
        if (count == 3)
        {
            volume /= 3;
        }
        return volume.ToString(UIConstants.AreaFormat) + " π";
    }

    public bool IsEdge(int id1, int id2)
    {
        foreach (GeoEdge geoEdge in geoEdges)
        {
            if ((id1 == geoEdge.Id1 && id2 == geoEdge.Id2) ||
                (id1 == geoEdge.Id2 && id2 == geoEdge.Id1))
                return true;
        }

        return false;
    }

    public bool IsFace(int[] ids)
    {
        SortedSet<int> idSet = new SortedSet<int>(ids);
        foreach (GeoFace geoFace in geoFaces)
        {
            SortedSet<int> set = new SortedSet<int>(geoFace.Ids);
            if (SortedSet<int>.CreateSetComparer().Equals(set, idSet))
                return true;
        }
        return false;
    }

    #region Normal

    private Vector3 FaceNormal(GeoFace geoFace)
    {
        int id1 = geoFace.Ids[0];
        int id2 = geoFace.Ids[1];
        int id3 = geoFace.Ids[2];
        Vector3 dir1 = UnitVector(id1) - UnitVector(id2);
        Vector3 dir2 = UnitVector(id1) - UnitVector(id3);

        Vector3 normal = Vector3.Cross(dir1, dir2).normalized;
        return normal;
    }

    public void RefreshVertexNormals()
    {
        foreach (KeyValuePair<int, VertexUnit> pair in vertices)
        {
            normals[pair.Key] = Vector3.zero;
        }

        foreach (GeoFace geoFace in geoFaces)
        {
            Vector3 faceNormal = FaceNormal(geoFace);

            foreach (int id in geoFace.Ids)
            {
                normals[id] = normals[id] + faceNormal;
            }
        }

        foreach (KeyValuePair<int, VertexUnit> pair in vertices)
        {
            normals[pair.Key] = normals[pair.Key].normalized;
        }
    }

    public Vector3 VertexNormal(GeoVertex vertex)
    {
        return normals[vertex.Id];
    }

    public Vector3 EdgeNormal(GeoEdge edge)
    {
        return (normals[edge.Id1] + normals[edge.Id2]).normalized;
    }

    #endregion

    #region Center

    public void RefreshCenter()
    {
        Vector3 total = Vector3.zero;
        int count = baseVertices.Count;

        if (count == 0)
            center = Vector3.zero;

        else
        {
            for (int i = 0; i < count; i++)
            {
                total += baseVertices[i].Position();
            }
            center = total / count;
        }
    }


    public Vector3 Center()
    {
        return center;
    }

    public Vector3 VertexCenterDirection(GeoVertex vertex)
    {
        Vector3 v = UnitVector(vertex.Id);
        v = v - center;
        return v.normalized;
    }

    public Vector3 EdgeCenterDirection(GeoEdge edge)
    {
        Vector3 v = EdgeCenter(edge);
        v = v - center;
        return v.normalized;
    }

    public Vector3 FaceCenterDirection(GeoFace face)
    {
        Vector3 v = FaceCenter(face);
        v = v - center;
        return v.normalized;
    }

    public Vector3 EdgeCenter(GeoEdge edge)
    {
        Vector3 v1 = UnitVector(edge.Id1);
        Vector3 v2 = UnitVector(edge.Id2);

        return (v1 + v2) / 2;
    }

    public Vector3 FaceCenter(GeoFace face)
    {
        Vector3 total = Vector3.zero;
        int[] ids = face.Ids;

        foreach (int id in ids)
        {
            total += UnitVector(id);
        }

        return total / ids.Length;
    }

    #endregion

    public virtual void MoveVertex(VertexUnit vertex, Ray ray, Transform camera, bool snap)
    {
        vertex.Move(ray, camera, snap);
    }

    public virtual VertexUnit[] VerticesOfMoveVertex(VertexUnit vertex)
    {
        return new VertexUnit[] { vertex };
    }

    #region Sign

    private List<string> restSigns = new List<string>();

    private string AutoSign(int id)
    {
        string sign = DefaultSign(id);
        if (CanSetVertexSign(id, sign))
            return sign;
        else
            return restSigns.First();
    }

    private void AddOldSign(string sign)
    {
        int oldId = SignId(sign);
        if (oldId <= vertexId)
        {
            restSigns.Add(sign);
            restSigns.OrderBy((x) => x);
        }
    }

    private void RemoveOldSign(string sign)
    {
        restSigns.Remove(sign);
    }

    private string DefaultSign(int id)
    {
        // 26 * 10
        if (id >= 260)
            Debug.LogError("Sign Error: " + id);

        string sign = System.Char.ConvertFromUtf32(id % 26 + 'A');
        if (id >= 26)
            sign += System.Char.ConvertFromUtf32(Mathf.FloorToInt(id / 26) + '0');

        return sign;
    }

    public int SignId(string sign)
    {
        if (sign.Length < 1 || sign.Length > 2)
            return -1;
        int i = sign[0] - 'A';
        int j = 0;
        if (sign.Length == 2)
            j = sign[1] - '0';
        return j * 26 + i;
    }

    public string VertexSign(int id)
    {
        if (vertexSignMap.ContainsKey(id))
            return vertexSignMap[id];
        return "";
    }

    public int SignVertex(string sign)
    {
        if (signVertexMap.ContainsKey(sign))
            return signVertexMap[sign];
        return -1;
    }


    public void SetVertexSign(int id, string sign)
    {
        bool canSign = CanSetVertexSign(id, sign);
        if (canSign)
        {
            if (vertexSignMap.ContainsKey(id))
            {
                string oldSign = vertexSignMap[id];
                signVertexMap.Remove(oldSign);
                AddOldSign(oldSign);
            }
            vertexSignMap[id] = sign;
            signVertexMap[sign] = id;
            RemoveOldSign(sign);

            // Debug.Log("can sign: " + index + "   " + sign);
        }
    }

    public bool CanSetVertexSign(int id, string sign)
    {
        if (sign == null || sign == "")
            return false;
        bool containSign = signVertexMap.ContainsKey(sign);

        if (containSign)
        {
            int oldId = signVertexMap[sign];
            return oldId == id;
        }
        else
        {
            return true;
        }

    }

    #endregion
}

public abstract class GeometryTool
{
    public abstract Geometry GenerateGeometry();
}

public abstract class GeometryState : State
{
    public Geometry geometry;
    public GeometryState(Tool tool) : base(tool)
    {

    }

    public GeometryState(Tool tool, Geometry geometry) : base(tool)
    {
        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        VertexUnit[] units = geometry.BaseVertexUnits();
        int[] ids = new int[units.Length];
        for (int i = 0; i < units.Length; i++)
            ids[i] = units[i].id;

        return ids;
    }
}