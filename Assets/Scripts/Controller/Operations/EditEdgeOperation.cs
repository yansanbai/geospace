using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditEdgeOperation : Operation
{
    GeoController geoController;
    GeoCamera geoCamera;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    ActivePanel activePanel;
    ElementPanel elementPanel;
    GeoEdge geoEdge;

    public EditEdgeOperation(GeoController geoController, GeoCamera geoCamera, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, GeoEdge geoEdge)
    {
        CanRotateCamera = false;
        CanActiveElement = false;

        this.geoController = geoController;
        this.geoCamera = geoCamera;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.activePanel = geoUI.activePanel;
        this.elementPanel = geoUI.elementPanel;
        this.geoEdge = geoEdge;
    }

    public override void Start()
    {
        FormElement formElement = geoController.EdgeForm(geoEdge);
        activePanel.SetEdge(formElement);

        Vector2 position = geoCamera.WorldToViewPoint(geometry.EdgeCenter(geoEdge));
        elementPanel.SetPositionByAnchor(position);

        elementPanel.OnElementClickColor = (element, color) =>
        {
            geometry.SetElementColor(element, color);
            geometryBehaviour.GeometryElementColorChange(geoEdge, color);
        };

        elementPanel.OnElementClickStyle = (element, style) =>
        {
            geometry.SetElementStyle(element, style);
            geometryBehaviour.GeometryElementStyleChange(geoEdge, style);
        };

        elementPanel.OnElementVisible = () =>
        {
            return geometryBehaviour.GeometryElementDiaplay(geoEdge);
        };

        elementPanel.OnElementClickDisplay = (visible) =>
        {
            geometryBehaviour.GeometryElementDiaplayChange(geoEdge, visible);
        };

        elementPanel.OnElementClickDelete = () =>
        {
            if (geoEdge.isBased)
                return;

            elementPanel.Close();
            geoController.EndOperation();
            geoController.DeleteEdgeOperation(geoEdge);
        };

        elementPanel.OnClose = () =>
        {
            geoController.EndOperation();
        };


        elementPanel.SetEdge(geoEdge);
    }

    public override void End()
    {
        geometryBehaviour.HighlightEdge(geoEdge, false);
        activePanel.Clear();
    }
}