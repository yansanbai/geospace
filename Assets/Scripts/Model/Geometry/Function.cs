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
    }

    public string Getfoluma() {
        return currentfomula;
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