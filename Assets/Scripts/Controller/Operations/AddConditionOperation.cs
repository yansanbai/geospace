using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddConditionOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;
    StatePanel statePanel;
    Tool tool;

    ConditionTool conditionTool;


    public AddConditionOperation(GeoController geoController, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, Tool tool)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.stateController = stateController;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;
        this.statePanel = geoUI.statePanel;
        this.tool = tool;

        Type type = Type.GetType(tool.Name + "ConditionTool");
        if (type != null)
            conditionTool = (ConditionTool)Activator.CreateInstance(type);
    }

    public override void Start()
    {
        if (conditionTool == null)
        {
            Debug.LogWarning(tool.Name + " Error!");
            geoController.EndOperation();
            return;
        }

        FormInput formInput = conditionTool.FormInput();
        if (formInput != null)
        {
;            inputPanel.SetFormForInput(conditionTool.FormInput());

            inputPanel.OnValidate = (form) =>
            {
                return conditionTool.ValidateInput(geometry, form);
            };

            inputPanel.OnClickSubmit = (form) =>
            {
                geoController.record.SetForm(form);
                geoController.records.Add(geoController.record);
                addCondition(geometry, form);
            };

            inputPanel.OnClickCancel = (form) =>
            {
                geoController.EndOperation();
            };
        } else {
            addCondition(geometry, null);
        }
    }

    public void addCondition(Geometry geometry, FormInput form)
    {
        
        Condition condition = conditionTool.GenerateCondition(geometry, form);

        bool result = geometry.Constructor.AddCondition(condition);
        if (result)
        {
            AddState(condition);
            geometryBehaviour.UpdateElements();
            geometryBehaviour.UpdateSignsPosition();
            geometryBehaviour.UpdateGizmos();

            Gizmo[] gizmos = condition.gizmos;
            if (gizmos != null)
            {
                foreach (Gizmo gizmo in gizmos)
                {
                    geometry.AddGizmo(gizmo);
                    geometryBehaviour.AddGizmo(gizmo);
                }
            }

            stateController.RefreshStateCells();
        }
        else
        {
            // TODO
        }

        geoController.EndOperation();
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

    private void AddState(Condition condition)
    {
        Type type = Type.GetType(tool.Name + "ConditionState");
        if (type != null)
        {
            ConditionState conditionState = (ConditionState)Activator.CreateInstance(type, tool, condition, geometry);
            //删除函数？
            conditionState.OnClickDelete = () => geoController.RemoveConditionOperation(condition);

            conditionState.OnElementHighlight = () =>
            {
                //获取当前state对应的geoline，让behaviour调用
                FreeCondition free = (FreeCondition)condition;
                Function func = (Function)geometry;
                geometryBehaviour.HighlightLine(func.Getline()[free.index-1]);
                func.Setfomula(func.Getline()[free.index - 1].Fomula());
            };
            stateController.AddConditionState(conditionState);
        }
    }

}