using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAuxiliary : Auxiliary
{
    public float x, y, z;

    private string sign;

    public PointAuxiliary(float x, float y, float z, string sign) : base()
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.sign = sign;
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        VertexSpace unit = new VertexSpace(x, y, z);
        unit.preferredSign = sign;
        geometry.VertexUnitSetId(unit, 0);

        units = new VertexUnit[] { unit };

        GeoVertex geoVertex = new GeoVertex(unit);

        elements = new GeoElement[] { geoVertex };
    }

    public override void RemoveAuxiliary() {}
}

public class PointAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(9);

        formInput.inputs[0] = new FormText("作点");
        formInput.inputs[1] = new FormElement(1);
        formInput.inputs[2] = new FormText("(");
        formInput.inputs[3] = new FormNum();
        formInput.inputs[4] = new FormText(",");
        formInput.inputs[5] = new FormNum();
        formInput.inputs[6] = new FormText(",");
        formInput.inputs[7] = new FormNum();
        formInput.inputs[8] = new FormText(")");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement SignElement = (FormElement)formInput.inputs[1];
        FormNum formNum1 = (FormNum)formInput.inputs[3];
        FormNum formNum2 = (FormNum)formInput.inputs[5];
        FormNum formNum3 = (FormNum)formInput.inputs[7];

        if (!IsNewSign(geometry, SignElement))
            return false;

        return IsPointCoordinate(geometry, formNum1) && IsPointCoordinate(geometry, formNum2) && IsPointCoordinate(geometry, formNum3);
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        FormNum formNum1 = (FormNum)formInput.inputs[3];
        FormNum formNum2 = (FormNum)formInput.inputs[5];
        FormNum formNum3 = (FormNum)formInput.inputs[7];

        FormElement SignElement = (FormElement)formInput.inputs[1];
        string sign = Sign(SignElement);
        PointAuxiliary auxiliary = new PointAuxiliary(formNum1.num, formNum2.num, formNum3.num, sign);

        return auxiliary;
    }
}

public class PointAuxiliaryState : AuxiliaryState
{
    new PointAuxiliary auxiliary;
    Geometry geometry;

    public PointAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is PointAuxiliary)
            this.auxiliary = (PointAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            auxiliary.units[0].id,
        };
        return ids;
    }

    public override FormInput Title()
    {
        FormElement formElement = new FormElement(1);
        formElement.fields[0] = geometry.VertexSign(auxiliary.units[0].id);

        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = formElement;

        return formInput;
    }


}