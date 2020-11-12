using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneVerticalAuxiliary : Auxiliary
{
    public VertexRefer vertex;
    public FaceRefer face;

    private string sign;

    public PlaneVerticalAuxiliary(int id1, int[] ids, string sign) : base()
    {
        vertex = new VertexRefer(id1);
        face = new FaceRefer(ids);
        this.sign = sign;
    }
    public override void InitWithGeometry(Geometry geometry)
    {
        VertexUnit unit1 = geometry.VertexUnit(vertex.id);

        int count = face.ids.Length;
        VertexUnit[] faceUnits = new VertexUnit[count];
        for (int i = 0; i < count; i++)
            faceUnits[i] = geometry.VertexUnit(face.ids[i]);

        VertexPlaneVertical unit2 = new VertexPlaneVertical(unit1, faceUnits);
        unit2.preferredSign = sign;
        geometry.VertexUnitSetId(unit2, 0);

        units = new VertexUnit[] { unit2 };


        GizmoRight gizmoRight = new GizmoRight(vertex.id, unit2.id, faceUnits[0].id);
        gizmos = new Gizmo[] { gizmoRight };

        GeoVertex geoVertex = new GeoVertex(unit2);
        GeoEdge geoEdge = new GeoEdge(unit1, unit2);
        elements = new GeoElement[] { geoVertex, geoEdge };


        dependencies.AddRange(faceUnits);
        dependencies.Add(unit1);
    }
    
    public override void RemoveAuxiliary() {}
}


public class PlaneVerticalAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(7);

        formInput.inputs[0] = new FormText("过点");
        formInput.inputs[1] = new FormElement(1);
        formInput.inputs[2] = new FormText("作平面");
        formInput.inputs[3] = new FormElement(-3);
        formInput.inputs[4] = new FormText("的垂线");
        formInput.inputs[5] = new FormText("交于点");
        formInput.inputs[6] = new FormElement(1);

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement1 = (FormElement)formInput.inputs[1];
        FormElement formElement2 = (FormElement)formInput.inputs[3];
        FormElement SignElement = (FormElement)formInput.inputs[6];

        if (!IsNewSign(geometry, SignElement))
            return false;

        bool judge = IsVertex(geometry, formElement1) && IsFace(geometry, formElement2);
        if (!judge)
            return false;
        return true;
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormElement formElement1 = (FormElement)formInput.inputs[1];
        FormElement formElement2 = (FormElement)formInput.inputs[3];
        int id1 = geometry.SignVertex(formElement1.fields[0]);
        string[] fields = formElement2.fields;
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);

        FormElement SignElement = (FormElement)formInput.inputs[6];
        string sign = Sign(SignElement);
        PlaneVerticalAuxiliary auxiliary = new PlaneVerticalAuxiliary(id1, ids, sign);

        return auxiliary;
    }
}

public class PlaneVerticalAuxiliaryState : AuxiliaryState
{
    new PlaneVerticalAuxiliary auxiliary;
    Geometry geometry;

    public PlaneVerticalAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is PlaneVerticalAuxiliary)
            this.auxiliary = (PlaneVerticalAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] faceIds = auxiliary.face.ids;
        int len = faceIds.Length;
        int[] ids = new int[len + 2];
        for (int i = 0; i < len; i++)
        {
            ids[i] = faceIds[i];
        }
        ids[len] = auxiliary.vertex.id;
        ids[len + 1] = auxiliary.units[0].id;
        return ids;
    }

    public override FormInput Title()
    {
        int[] faceIds = auxiliary.face.ids;
        int len = faceIds.Length;

        FormElement formElement1 = new FormElement(len);
        for (int i = 0; i < len; i++)
        {
            formElement1.fields[i] = geometry.VertexSign(faceIds[i]);
        }

        FormElement formElement2 = new FormElement(2);
        formElement2.fields[0] = geometry.VertexSign(auxiliary.vertex.id);
        formElement2.fields[1] = geometry.VertexSign(auxiliary.units[0].id);

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement1;
        formInput.inputs[1] = new FormText("的垂线");
        formInput.inputs[2] = formElement2;

        return formInput;
    }


}