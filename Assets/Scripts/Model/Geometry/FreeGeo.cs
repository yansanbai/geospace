using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeGeo : Geometry
{
    public override void Init()
    {
        base.Init();

        Name = "FreeGeo";
        Type = GeometryType.Common;
    }
}

public class FreeGeometryTool : GeometryTool
{
    public override Geometry GenerateGeometry()
    {
        FreeGeo geo = new FreeGeo();
        geo.Assistor = new Assistor(geo);
        geo.Implement = new Implement(geo);
        geo.Init();

        return geo;
    }
}

public class FreeGeometryState : GeometryState
{
    new FreeGeo geometry;

    public FreeGeometryState(Tool tool, Geometry geometry) : base(tool, geometry)
    {
        if (geometry is FreeGeo)
            this.geometry = (FreeGeo)geometry;
    }

    public override FormInput Title()
    {

        FormInput formInput = new FormInput(1);
        formInput.inputs[0] = new FormText("自由构建");

        return formInput;
    }
}