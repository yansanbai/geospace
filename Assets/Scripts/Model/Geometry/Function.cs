using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function : Geometry
{
    public override void Init()
    {

        base.Init();

        Name = "Function";
        Type = GeometryType.Function;

    }

    public void SetWriting(Vector3[] pos) {

        GeoLine line = new GeoLine(pos);
        AddGeoLine(line);
    }
}

public class FunctionGeometryTool : GeometryTool
{
    private StatusButton lockButton;

    public override Geometry GenerateGeometry()
    {
        Function geo = new Function();
        //geo.Constructor = new ResolvedBodyConstructor(geo);
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