using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterPointAuxiliary : Auxiliary
{
    public FaceRefer face;

     private string sign;

    public CenterPointAuxiliary(int[] ids, string sign) : base()
    {
        face = new FaceRefer(ids);
           this.sign = sign;
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        int count = face.ids.Length;
        VertexUnit[] vertexUnits = new VertexUnit[count];
        for (int i = 0; i < count; i++)
            vertexUnits[i] = geometry.VertexUnit(face.ids[i]);

        VertexPlaneCenter unit = new VertexPlaneCenter(vertexUnits);
        unit.preferredSign = sign;
        geometry.VertexUnitSetId(unit, 0);
        units = new VertexUnit[] { unit };

        GeoVertex geoVertex = new GeoVertex(unit);
        elements = new GeoElement[] { geoVertex };

        dependencies.AddRange(vertexUnits);
    }

    public override void RemoveAuxiliary() {}
}

public class CenterPointAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(4);

        formInput.inputs[0] = new FormText("作平面");
        formInput.inputs[1] = new FormElement(-3);
        formInput.inputs[2] = new FormText("的重心");
        formInput.inputs[3] = new FormElement(1);

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        FormElement SignElement = (FormElement)formInput.inputs[3];

        if (!IsNewSign(geometry, SignElement))
            return false;
        if (!IsFace(geometry, formElement))
            return false;

        return true;
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormElement formElement = (FormElement)formInput.inputs[1];
        string[] fields = formElement.fields;
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);

        FormElement SignElement = (FormElement)formInput.inputs[3];
        string sign = Sign(SignElement);

        CenterPointAuxiliary auxiliary = new CenterPointAuxiliary(ids, sign);

        return auxiliary;
    }
}

public class CenterPointAuxiliaryState : AuxiliaryState
{
    new CenterPointAuxiliary auxiliary;
    Geometry geometry;

    public CenterPointAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is CenterPointAuxiliary)
            this.auxiliary = (CenterPointAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] faceIds = auxiliary.face.ids;
        int len = faceIds.Length;
        int[] ids = new int[len + 1];
        for (int i = 0; i < len; i++)
        {
            ids[i] = faceIds[i];
        }
        ids[len] = auxiliary.units[0].id;
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

        FormElement formElement2 = new FormElement(1);
        formElement2.fields[0] = geometry.VertexSign(auxiliary.units[0].id);

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement1;
        formInput.inputs[1] = new FormText("重心");
        formInput.inputs[2] = formElement2;

        return formInput;
    }

}