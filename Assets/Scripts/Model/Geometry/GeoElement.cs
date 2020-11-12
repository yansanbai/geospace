using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//所有位置元素的父类
public abstract class GeoElement
{
    public string name;
    public int color;
    public int style;

    public bool isBased;

    public GeoElement(int color, int style, bool isBased = false)
    {
        this.color = color;
        this.style = style;
        this.isBased = isBased;
    }

    public abstract void AddObserveElements();

    public abstract void RemoveObserveElements();
}

public class GeoVertex : GeoElement
{
    private VertexUnit vertex;
    public bool isFixed;
    public bool isSpace;

    public GeoVertex(VertexUnit vertex, bool isBased = false) : base(0, 0, isBased)
    {
        name = "Vertex";

        this.vertex = vertex;

        this.isFixed = vertex.isFixed;
        this.isSpace = vertex is VertexSpace;
    }

    public int Id
    {
        get
        {
            return vertex.id;
        }
    }

    public Vertex Vertex()
    {
        return new Vertex(vertex.Position());
    }

    public VertexUnit VertexUnit() {
        return vertex;
    }

    public override string ToString()
    {
        return string.Format("vertex {0}", vertex.id);
    }

    public override void AddObserveElements()
    {
        vertex.AddObserveElement(this);
    }

    public override void RemoveObserveElements()
    {
        vertex.RemoveObserveElement(this);
    }
}

public class GeoEdge : GeoElement
{
    private VertexUnit vertex1;
    private VertexUnit vertex2;
    private bool CanSelected;
    public GeoEdge(VertexUnit vertex1, VertexUnit vertex2, bool CanSelected = false, bool isBased = false) : base(0, 0, isBased)
    {
        name = "Edge";

        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        this.CanSelected = CanSelected;
    }

    public int Id1
    {
        get
        {
            return vertex1.id;
        }
    }

    public int Id2
    {
        get
        {
            return vertex2.id;
        }
    }

    public Edge Edge()
    {
        return new Edge(vertex1.Position(), vertex2.Position(), CanSelected);
    }

    public override string ToString()
    {
        return string.Format("edge {0} {1}", vertex1.id, vertex2.id);
    }

    public override void AddObserveElements()
    {
        vertex1.AddObserveElement(this);
        vertex2.AddObserveElement(this);
    }

    public override void RemoveObserveElements()
    {
        vertex1.RemoveObserveElement(this);
        vertex2.RemoveObserveElement(this);
    }
}

public enum FaceType {
    Normal,
    SpreadRectangle,
    SpreadCylinderCircle,
    SpreadConeCircle,
    SpreadFan,
}

public class GeoFace : GeoElement
{
    private VertexUnit[] vertices;

    public bool Canselected;

    public FaceType faceType;

    private int[] ids;

    public GeoFace(VertexUnit[] vertices, bool Canselected = true, FaceType faceType = FaceType.Normal, bool isBased = false) : base(0, 0, isBased)
    {
        name = "Face";

        this.vertices = vertices;

        this.Canselected = Canselected;
        
        this.faceType = faceType;

        this.ids = vertices.Select(v => v.id).ToArray();
    }

    // public int VertexCount()
    // {
    //     return vertices.Length;
    // }

    // public int Id(int i)
    // {
    //     return vertices[i].id;
    // }

    public int[] Ids
    {
        get
        {
            return ids;
        }
    }
    public VertexUnit[] get_vertices()
    {
        
            return vertices;
        
    }

    public Face Face()
    {
        return new Face(vertices.Select(v => v.Position()).ToArray(), Canselected, faceType);
    }

    public override string ToString()
    {
        return string.Format("face {0}", string.Join(" ", ids));
    }


    public override void AddObserveElements()
    {
        foreach (VertexUnit vertex in vertices)
            vertex.AddObserveElement(this);
    }

    public override void RemoveObserveElements()
    {
        foreach (VertexUnit vertex in vertices)
            vertex.RemoveObserveElement(this);
    }
}


public class GeoCircle : GeoElement
{
    private VertexUnit vertex;
    private float radius; 
    private CircleDirection direction; 
    private bool displayFace; 
    private FaceType faceType; 

    public GeoCircle(VertexUnit vertex, float radius, CircleDirection direction = CircleDirection.Y, bool displayFace = false, FaceType faceType = FaceType.Normal, bool isBased = false) : base(0, 0, isBased)
    {
        name = "Circle";

        this.vertex = vertex;
        this.radius = radius;
        this.direction = direction;
        this.displayFace = displayFace;
        this.faceType = faceType;
    }

    public Circle Circle()
    {
        return new Circle(vertex.Position(), radius, direction, displayFace, faceType);
    }

    public override string ToString()
    {
        return string.Format("circular");
    }

    public override void AddObserveElements()
    {
        vertex.AddObserveElement(this);
    }

    public override void RemoveObserveElements()
    {
        vertex.RemoveObserveElement(this);
    }
}

public enum CircularType
{
    Cylinder,//圆柱
    Cone,//圆锥
}

public class GeoCircular : GeoElement
{
    private VertexUnit[] vertexs;
    private float radius1; 
    private float radius2; 
    private CircularType type;

    public GeoCircular(VertexUnit[] vertexs, float radius1, float radius2, CircularType type, bool isBased = false) : base(0, 0, isBased)
    {
        name = "Circular";

        this.vertexs = vertexs;
        this.radius1 = radius1;
        this.radius2 = radius2;
        this.type = type;
    }

    public Circular Circular()
    {
        return new Circular(vertexs.Select(v => v.Position()).ToArray(), radius1, radius2, type);
    }

    public override string ToString()
    {
        return string.Format("circular");
    }

    public override void AddObserveElements()
    {
        foreach (VertexUnit vertex in vertexs)
            vertex.AddObserveElement(this);
    }

    public override void RemoveObserveElements()
    {
        foreach (VertexUnit vertex in vertexs)
            vertex.RemoveObserveElement(this);
    }
}