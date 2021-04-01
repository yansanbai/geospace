using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function : Geometry
{
    private int index;
    private string currentfomula;
    private List<GeoLine> lines;
    public override void Init()
    {
        base.Init();

        Name = "Function";
        index = 0;
        lines = new List<GeoLine>();
        Type = GeometryType.Function;
    }

    public void SetWriting(Vector3[] pos,string fomula) {
        GeoLine line = new GeoLine(index, pos);
        currentfomula = fomula;
        lines.Add(line);
        AddGeoLine(line);
        index += 1;

        //增加极值点
        /*Vector3[] res = GetExtreme(pos);
        VertexSpace u0 = new VertexSpace(res[0].x, res[0].y, 0);
        VertexSpace u0x = new VertexSpace(res[0].x, 0, 0);
        VertexSpace u0y = new VertexSpace(0, res[0].y, 0);
        AddBaseVertex(u0);
        GeoVertex v0 = new GeoVertex(u0, true);
        AddGeoVertex(v0);
        GeoEdge e0x = new GeoEdge(u0, u0x, true);
        GeoEdge e0y = new GeoEdge(u0, u0y, true);
        AddGeoEdge(e0x);
        AddGeoEdge(e0y);

        VertexSpace u1 = new VertexSpace(res[1].x, res[1].y, 0);
        VertexSpace u1x = new VertexSpace(res[1].x, 0, 0);
        VertexSpace u1y = new VertexSpace(0, res[1].y, 0);
        AddBaseVertex(u1);
        GeoVertex v1 = new GeoVertex(u1, true);
        AddGeoVertex(v1);
        GeoEdge e1x = new GeoEdge(u1, u1x, true);
        GeoEdge e1y = new GeoEdge(u1, u1y, true);
        AddGeoEdge(e1x);
        AddGeoEdge(e1y);*/
    }

    private Vector3[] GetExtreme(Vector3[] pos) {
        float max = pos[0].y;
        float min = pos[0].y;
        int maxindex = 0;
        int minindex = 0;
        for (int i = 1; i < pos.Length; i++) {
            if (pos[i].y > max + 0.01)
            {
                max = pos[i].y;
                maxindex = i;
            }
            else if ((pos[i].y - max < 0.01)||(pos[i].y==max))
            {
                if (Mathf.Abs(pos[i].x) < Mathf.Abs(pos[maxindex].x))
                {
                    max = pos[i].y;
                    maxindex = i;
                }
            }
            if (pos[i].y < min-0.01)
            {
                    min = pos[i].y;
                    minindex = i;
            }
            else if (pos[i].y -min<0.01) {
                if (Mathf.Abs(pos[i].x) < Mathf.Abs(pos[minindex].x))
                {
                    min = pos[i].y;
                    minindex = i;
                }
            }
        }
        Vector3[] res = {new Vector3(pos[maxindex].x, pos[maxindex].y, 0), new Vector3(pos[minindex].x, pos[minindex].y, 0) };
        return res;
    }
    public string Getfomula() {
        return currentfomula;
    }
    public void Setfomula(string fomula)
    {
        this.currentfomula=fomula;
    }
    public int Getindex()
    {
        return index;
    }
    public List<GeoLine> Getline()
    {
        return lines;
    }
}

public class FunctionGeometryTool : GeometryTool
{
    private StatusButton lockButton;

    public override Geometry GenerateGeometry()
    {
        Function geo = new Function();
        geo.Constructor = new FunctionConstructor(geo);
        //geo.Assistor = new Assistor(geo);
        //geo.Implement = new Implement(geo);
        geo.Init();

/*        lockButton = GameObject.Find("LockButton").GetComponent<StatusButton>();
        lockButton.SetStatus(1);*/

        return geo;
    }
}

public class FunctionGeometryState : GeometryState
{
    new Function geometry;

    public FunctionGeometryState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is Function)
            this.geometry = (Function)geometry;
    }

    public override FormInput Title()
    {
        // add state
        FormInput formInput = new FormInput(1);
        formInput.inputs[0] = new FormText("函数");

        return formInput;
    }
}