using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexCoordinateOperation : Operation
{
    GeoController geoController;
    StateController stateController;


    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    InputPanel inputPanel;

    VertexUnit vertexUnit;

    public VertexCoordinateOperation(GeoController geoController, StateController stateController, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, VertexUnit vertexUnit)
    {
        CanRotateCamera = true;
        CanActiveElement = false;

        this.geoController = geoController;
        this.stateController = stateController;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;
        this.vertexUnit = vertexUnit;
    }


    public override void Start()
    {
        FormInput formInput;

        string sign = geometry.VertexSign(vertexUnit.id);
        Vector3 position = vertexUnit.Position();


        formInput = new FormInput(8);
        FormNum formNumX = new FormNum(position.x);
        FormNum formNumY = new FormNum(position.y);
        FormNum formNumZ = new FormNum(position.z);

        formNumX.format = UIConstants.PointFormat;
        formNumY.format = UIConstants.PointFormat;
        formNumZ.format = UIConstants.PointFormat;

        FormElement formElement = new FormElement(1);
        formElement.fields[0] = sign;

        formInput.inputs[0] = formElement;
        formInput.inputs[1] = new FormText("(");
        formInput.inputs[2] = formNumX;
        formInput.inputs[3] = new FormText(",");
        formInput.inputs[4] = formNumY;
        formInput.inputs[5] = new FormText(",");
        formInput.inputs[6] = formNumZ;
        formInput.inputs[7] = new FormText(")");

        inputPanel.SetFormForInput(formInput);

        inputPanel.OnValidate = (form) =>
        {
            return !formNumX.isEmpty && !formNumY.isEmpty && !formNumZ.isEmpty;
        };

        inputPanel.OnClickSubmit = (form) =>
        {
            float x = formNumX.num;
            float y = formNumY.num;
            float z = formNumZ.num;

            Vector3 newPosition = new Vector3(x, y, z);

            geometry.SetUnitVector(vertexUnit.id, newPosition);

            stateController.RefreshStateCellById(vertexUnit.id);

            geometryBehaviour.UpdateElements();
            geometryBehaviour.UpdateSignsPosition();
            geometryBehaviour.UpdateGizmos();

            geoController.EndOperation();
        };

        inputPanel.OnClickCancel = (form) =>
        {
            geoController.EndOperation();
        };

    }

    public override void End()
    {
        inputPanel.Clear();
    }

}
