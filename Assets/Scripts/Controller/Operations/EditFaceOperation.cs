using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditFaceOperation : Operation
{
    GeoController geoController;
    GeoCamera geoCamera;

    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    ActivePanel activePanel;
    ElementPanel elementPanel;

    GeoFace geoFace;

    public EditFaceOperation(GeoController geoController, GeoCamera geoCamera, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, GeoFace geoFace)
    {
        CanRotateCamera = false;
        CanActiveElement = false;

        this.geoController = geoController;
        this.geoCamera = geoCamera;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.activePanel = geoUI.activePanel;
        this.elementPanel = geoUI.elementPanel;
        this.geoFace = geoFace;
    }

    public override void Start()
    {
        FormElement formElement = geoController.FaceForm(geoFace);
        activePanel.SetFace(formElement);

        Vector2 position = geoCamera.WorldToViewPoint(geometry.FaceCenter(geoFace));
        elementPanel.SetPositionByAnchor(position);

        elementPanel.OnElementClickColor = (element, color) =>
        {
            geometry.SetElementColor(element, color);
            geometryBehaviour.GeometryElementColorChange(geoFace, color);
        };

        elementPanel.OnElementClickStyle = (element, style) =>
        {
            geometry.SetElementStyle(element, style);
            geometryBehaviour.GeometryElementStyleChange(geoFace, style);
        };

        elementPanel.OnElementVisible = () =>
        {
            return geometryBehaviour.GeometryElementDiaplay(geoFace);
        };

        elementPanel.OnElementClickDisplay = (visible) =>
        {
            geometryBehaviour.GeometryElementDiaplayChange(geoFace, visible);
        };

        elementPanel.OnElementClickDelete = () =>
        {
            if (geoFace.isBased)
                return;

            elementPanel.Close();
            geoController.EndOperation();
            geoController.DeleteFaceOperation(geoFace);
        };


        elementPanel.OnClose = () =>
        {
            geoController.EndOperation();
        };

        elementPanel.SetFace(geoFace);
    }

    public override void End()
    {
        geometryBehaviour.HighlightFace(geoFace, false);
        activePanel.Clear();
    }
}