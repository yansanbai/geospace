using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryOperation : Operation
{
    GeoController geoController;
    ToolController toolController;
    StateController stateController;

    Tool tool;

    GeometryTool geometryTool;

    Geometry geometry;

    GeometryBehaviour geometryBehaviour;

    String[] signs;
    public GeometryOperation(GeoController geoController, ToolController toolController, StateController stateController, Tool tool,GeometryBehaviour geometryBehaviour)
    {
        this.geoController = geoController;
        this.toolController = toolController;
        this.stateController = stateController;

        this.tool = tool;
        this.geometryBehaviour = geometryBehaviour;

        Type type = Type.GetType(tool.Name + "GeometryTool");
        if (type != null)
            geometryTool = (GeometryTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (geometryTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }

        geometry = geometryTool.GenerateGeometry();

        //Debug.Log(geometry.VertexUnit(0));
        //Debug.Log(geometry.VertexSign(0));
        //Debug.Log(signs.Length);
        if (signs != null) {
            if (signs.Length == 8)
            {
                geometry.SetVertexSign(0, signs[0]);
                geometry.SetVertexSign(1, signs[1]);
                geometry.SetVertexSign(2, signs[2]);
                geometry.SetVertexSign(3, signs[3]);
                geometry.SetVertexSign(4, signs[4]);
                geometry.SetVertexSign(5, signs[5]);
                geometry.SetVertexSign(6, signs[6]);
                geometry.SetVertexSign(7, signs[7]);
            }
            else if (signs.Length == 4) {
                geometry.SetVertexSign(3, signs[0]);
                geometry.SetVertexSign(0, signs[1]);
                geometry.SetVertexSign(1, signs[2]);
                geometry.SetVertexSign(2, signs[3]);
            }
        }

        geoController.SetGeometry(geometry);

        AddState(geometry);

        geoController.EndOperation();
    }

    public override void End()
    {

    }

    private void AddState(Geometry geometry)
    {
        Type type = Type.GetType(tool.Name + "GeometryState");
        if (type != null)
        {
            GeometryState geometryState = (GeometryState)Activator.CreateInstance(type, tool, geometry);
            geometryState.OnClickDelete = () => geoController.ClearGeometryOperation(geometry);

            stateController.AddGeometryState(geometryState);
        }
    }

    public void ReSetSign(String list) {
        this.signs = list.Split(' ');
    }
}
