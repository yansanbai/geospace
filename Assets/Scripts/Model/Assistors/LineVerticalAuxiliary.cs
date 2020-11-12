using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineVerticalAuxiliary : Auxiliary
{
    public VertexRefer vertex;
    public EdgeRefer edge;

    private string sign;

    public LineVerticalAuxiliary(int id1, int id2, int id3, string sign) : base()
    {
        vertex = new VertexRefer(id1);
        edge = new EdgeRefer(id2, id3);
        this.sign = sign;
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        VertexUnit unit1 = geometry.VertexUnit(vertex.id);
        VertexUnit unit2 = geometry.VertexUnit(edge.id1);
        VertexUnit unit3 = geometry.VertexUnit(edge.id2);

        VertexLineVertical unit4 = new VertexLineVertical(unit1, unit2, unit3);
        unit4.preferredSign = sign;
        geometry.VertexUnitSetId(unit4, 0);

        units = new VertexUnit[] { unit4 };


        GizmoRight gizmoRight = new GizmoRight(vertex.id, unit4.id, edge.id2);
        gizmos = new Gizmo[] { gizmoRight };

        GeoVertex geoVertex = new GeoVertex(unit4);
        GeoEdge geoEdge = new GeoEdge(unit1, unit4);
        elements = new GeoElement[] { geoVertex, geoEdge };

        dependencies.Add(unit1);
        dependencies.Add(unit2);
        dependencies.Add(unit3);

    }

    public override void RemoveAuxiliary() {}
}

public class LineVerticalAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(7);

        formInput.inputs[0] = new FormText("过点");
        formInput.inputs[1] = new FormElement(1);
        formInput.inputs[2] = new FormText("作线段");
        formInput.inputs[3] = new FormElement(2);
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
        {
            return false;
        }

        bool judge = IsVertex(geometry, formElement1) && IsEdge(geometry, formElement2);
        if (!judge)
        {
            return false;
        }
        return true;
    }

    public override Auxiliary GenerateAuxiliary(Geometry geometry, FormInput formInput)
    {
        //Debug.Log("generate");
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
        {
            //Debug.Log("invalid");
            return null;
        }

        FormElement formElement1 = (FormElement)formInput.inputs[1];
        FormElement formElement2 = (FormElement)formInput.inputs[3];
        int id1 = geometry.SignVertex(formElement1.fields[0]);
        int id2 = geometry.SignVertex(formElement2.fields[0]);
        int id3 = geometry.SignVertex(formElement2.fields[1]);

        FormElement SignElement = (FormElement)formInput.inputs[6];
        string sign = Sign(SignElement);
        LineVerticalAuxiliary auxiliary = new LineVerticalAuxiliary(id1, id2, id3, sign);

        return auxiliary;
    }

}

public class LineVerticalAuxiliaryState : AuxiliaryState
{
    new LineVerticalAuxiliary auxiliary;
    Geometry geometry;

    public LineVerticalAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is LineVerticalAuxiliary)
            this.auxiliary = (LineVerticalAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            auxiliary.vertex.id,
            auxiliary.edge.id1,
            auxiliary.edge.id2,
            auxiliary.units[0].id,
        };
        return ids;
    }

    public override FormInput Title()
    {
        FormElement formElement1 = new FormElement(2);
        formElement1.fields[0] = geometry.VertexSign(auxiliary.edge.id1);
        formElement1.fields[1] = geometry.VertexSign(auxiliary.edge.id2);

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