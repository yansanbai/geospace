using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditVertexOperation : Operation
{
    GeoController geoController;
    StateController stateController;

    GeoCamera geoCamera;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    ActivePanel activePanel;
    ElementPanel elementPanel;

    GeoVertex geoVertex;

    public EditVertexOperation(GeoController geoController, StateController stateController, GeoCamera geoCamera, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, GeoVertex geoVertex)
    {
        CanRotateCamera = false;
        CanActiveElement = false;

        this.geoController = geoController;
        this.stateController = stateController;
        this.geoCamera = geoCamera;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.activePanel = geoUI.activePanel;
        this.elementPanel = geoUI.elementPanel;
        this.geoVertex = geoVertex;
    }

    public override void Start()
    {

        geometryBehaviour.HighlightVertex(geoVertex, true);

        FormElement formElement = geoController.VertexForm(geoVertex);
        activePanel.SetVertex(formElement);

        Vector3 worldPosition = geometry.UnitVector(geoVertex.Id);
        Vector2 position = geoCamera.WorldToViewPoint(worldPosition);
        elementPanel.SetPositionByAnchor(position);


        elementPanel.OnElementClickColor = (element, color) =>
        {
            geometry.SetElementColor(element, color);
            geometryBehaviour.GeometryElementColorChange(geoVertex, color);
        };

        elementPanel.OnElementClickStyle = (element, style) =>
        {
            geometry.SetElementStyle(element, style);
            geometryBehaviour.GeometryElementStyleChange(geoVertex, style);
        };

        elementPanel.OnElementVisible = () =>
        {
            return geometryBehaviour.GeometryElementDiaplay(geoVertex);
        };

        elementPanel.OnElementClickDisplay = (visible) =>
        {
            geometryBehaviour.GeometryElementDiaplayChange(geoVertex, visible);
        };

        // Sign
        int vertexId = geoVertex.Id;
        string newSign = geometry.VertexSign(vertexId);

        elementPanel.OnSignButtonChange = (open) =>
        {
            if (open)
                geometryBehaviour.SignState(vertexId, SignState.Highlight);
            else
            {
                geometry.SetVertexSign(vertexId, newSign);
                geometryBehaviour.SignState(vertexId, SignState.Normal);
                geometryBehaviour.UpdateSignText(vertexId);

                stateController.RefreshStateCellById(vertexId);
            }
        };

        elementPanel.OnSignDefault = () =>
        {
            return geometry.VertexSign(vertexId);
        };

        elementPanel.OnSignInputChanged = (sign) =>
        {
            newSign = sign;
            geometryBehaviour.SignText(vertexId, sign);
        };

        elementPanel.OnSignInputValidate = (sign) =>
        {
            bool valid = sign != "" && geometry.CanSetVertexSign(vertexId, sign);
            geometryBehaviour.SignState(vertexId, valid ? SignState.Valid : SignState.Error);
            return valid;
        };


        elementPanel.OnElementClickDelete = () =>
        {
            if (geoVertex.isBased)
                return;

            elementPanel.Close();
            geoController.EndOperation();
            geoController.DeleteVertexOperation(geoVertex);
        };

        elementPanel.OnElementClickCoordinate =() =>
        {
            if (!geoVertex.isSpace)
            return;

             elementPanel.Close();
            geoController.EndOperation();
            geoController.VertexCoordinateOperation(geoVertex);
        };


        elementPanel.OnClose = () =>
        {
            geoController.EndOperation();
        };

        // Final
        elementPanel.SetVertex(geoVertex);
    }

    public override void End()
    {
        geometryBehaviour.HighlightVertex(geoVertex, false);
        activePanel.Clear();
    }
}