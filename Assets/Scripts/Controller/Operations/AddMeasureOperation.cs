using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMeasureOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;
    Tool tool;

    MeasureTool measureTool;
    FormInput writeInput;
    public AddMeasureOperation(GeoController geoController, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, Tool tool)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.stateController = stateController;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;
        this.tool = tool;

        Type type = Type.GetType(tool.Name + "MeasureTool");
        if (type != null)
            measureTool = (MeasureTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (measureTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }
        if (writeInput == null)
        {
            FormInput formInput = measureTool.FormInput();
            if (formInput != null)
            {
                inputPanel.SetFormForInput(formInput);

                inputPanel.OnValidate = (form) =>
                {
                    return measureTool.ValidateInput(geometry, form);
                };

                inputPanel.OnClickSubmit = (form) =>
                {
                    AddMeasure(geometry, form);
                };

                inputPanel.OnClickCancel = (form) =>
                {
                    geoController.EndOperation();
                };
            }
            else
            {
                AddMeasure(geometry, null);
            }
        }
        else {
            AddMeasure(geometry, writeInput);
        }
    }

    public void AddMeasure(Geometry geometry, FormInput form)
    {
        Measure measure = measureTool.GenerateMeasure(geometry, form);
        if (measure == null)
        {
            return;
        }
        measure.InitWithGeometry(geometry);


        bool result = geometry.Implement.AddMeasure(measure);

        if (result)
        {
            AddState(measure);

            Gizmo[] gizmos = measure.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.AddGizmo(gizmo);
                    geometryBehaviour.AddGizmo(gizmo);
                }
            }

            else
            {
                // TODO
            }
            // foreach(FormItem item in form.inputs) 
            // {
            // Debug.Log(item);
            // }
            geoController.EndOperation();

        };


    }

    public override void End()
    {
        inputPanel.Clear();
    }

    public override void OnClickElement(GeoElement element)
    {
        FormElement form = null;
        if (element is GeoVertex)
            form = geoController.VertexForm((GeoVertex)element);
        else if (element is GeoEdge)
            form = geoController.EdgeForm((GeoEdge)element);
        else if (element is GeoFace)
            form = geoController.FaceForm((GeoFace)element);
        if (form != null)
            inputPanel.InputFields(form.fields);
    }

    public void SetWriteInput(FormInput writeInput)
    {
        this.writeInput = writeInput;
    }

    private void AddState(Measure measure)
    {
        Type type = Type.GetType(tool.Name + "MeasureState");
        if (type != null)
        {
            MeasureState measureState = (MeasureState)Activator.CreateInstance(type, tool, measure, geometry);
            measureState.OnClickDelete = () => geoController.RemoveMeasureOperation(measure);

            stateController.AddMeasureState(measureState);
        }
    }

}
