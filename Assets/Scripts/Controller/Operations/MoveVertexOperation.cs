using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveVertexOperation : Operation
{
    StateController stateController;

    GeoCamera geoCamera;
    Geometry geometry;
    GeometryBehaviour geometryBehaviour;
    VertexUnit vertex;
    VertexBehaviour vertexBehaviour;

    InputPanel inputPanel;


    bool snap;

    HashSet<GeoElement> observeElements;
    HashSet<Gizmo> observeGizmos;

    FormInput formInput;

    FormNum formNumX, formNumY, formNumZ;


    public MoveVertexOperation(StateController stateController, GeoCamera geoCamera, Geometry geometry, GeometryBehaviour geometryBehaviour, GeoUI geoUI, VertexUnit vertex, VertexBehaviour vertexBehaviour, bool snap)
    {
        CanRotateCamera = false;
        CanActiveElement = false;

        this.stateController = stateController;

        this.geoCamera = geoCamera;
        this.geometry = geometry;
        this.geometryBehaviour = geometryBehaviour;
        this.inputPanel = geoUI.inputPanel;

        this.vertex = vertex;
        this.vertexBehaviour = vertexBehaviour;

        this.snap = snap;
    }

    public override void Start()
    {
        vertexBehaviour.SetActive(true);
        vertexBehaviour.OnDragVertex = MoveVertex;

        VertexUnit[] vertices = geometry.VerticesOfMoveVertex(vertex);

        observeElements = new HashSet<GeoElement>();
        observeGizmos = new HashSet<Gizmo>();

        foreach (VertexUnit v in vertices)
        {
            HashSet<GeoElement> elementsSet = geometry.VertexUnitObserveElements(v.id);
            observeElements.UnionWith(elementsSet);

            HashSet<Gizmo> gizmosSet = geometry.VertexUnitObserveGizmos(v.id);
            observeGizmos.UnionWith(gizmosSet);
        }

        string sign = geometry.VertexSign(vertex.id);
        Vector3 position = vertex.Position();

        formInput = new FormInput(8);
        formNumX = new FormNum(position.x);
        formNumY = new FormNum(position.y);
        formNumZ = new FormNum(position.z);

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

        inputPanel.SetFormForMessage(formInput);
    }

    public override void End()
    {
        vertexBehaviour.SetActive(false);
        vertexBehaviour.OnDragVertex = null;
        inputPanel.Clear();
    }

    private void MoveVertex(Ray ray)
    {
        geometry.MoveVertex(vertex, ray, geoCamera.transform, snap);
        geometry.RefreshVertexNormals();
        geometry.RefreshCenter();

        geometryBehaviour.UpdateSignsPosition();

        stateController.RefreshStateCellById(vertex.id);

        foreach (GeoElement element in observeElements)
        {
            geometryBehaviour.UpdateElement(element);
        }

        foreach (Gizmo gizmo in observeGizmos)
        {
            geometryBehaviour.UpdateGizmo(gizmo);
        }

        Vector3 position = vertex.Position();
        formNumX.num = position.x;
        formNumY.num = position.y;
        formNumZ.num = position.z;

        inputPanel.RefreshForm(formInput);
    }
}