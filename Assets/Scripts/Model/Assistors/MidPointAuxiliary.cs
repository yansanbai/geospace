using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidPointAuxiliary : Auxiliary
{
    public EdgeRefer edge;

    private string sign;

    public MidPointAuxiliary(int id1, int id2, string sign) : base()
    {
        edge = new EdgeRefer(id1, id2);
        this.sign = sign;
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        VertexUnit unit1 = geometry.VertexUnit(edge.id1);
        VertexUnit unit2 = geometry.VertexUnit(edge.id2);

        VertexLineFixed unit3 = new VertexLineFixed(unit1, unit2, 0.5f);
        unit3.preferredSign = sign;
        geometry.VertexUnitSetId(unit3, 0);
        units = new VertexUnit[] { unit3 };


        List<GeoElement> elementList = new List<GeoElement>();

        GeoVertex geoVertex = new GeoVertex(unit3);
        elementList.Add(geoVertex);

        // if (!geometry.IsEdge(id, id1))
        //     elementList.Add(new GeoEdge(id, id1));

        // if (!geometry.IsEdge(id, id2))
        //     elementList.Add(new GeoEdge(id, id2));

        elements = elementList.ToArray();

        dependencies.Add(unit1);
        dependencies.Add(unit2);

    }

    public override void RemoveAuxiliary() {}
}

public class MidPointAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(4);

        formInput.inputs[0] = new FormText("作");
        formInput.inputs[1] = new FormElement(2);
        formInput.inputs[2] = new FormText("的中点");
        formInput.inputs[3] = new FormElement(1);

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        FormElement SignElement = (FormElement)formInput.inputs[3];

        if (!IsNewSign(geometry, SignElement))
            return false;
        if (!IsTwoVertex(geometry, formElement))
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
        int i1 = geometry.SignVertex(fields[0]);
        int i2 = geometry.SignVertex(fields[1]);

        FormElement SignElement = (FormElement)formInput.inputs[3];
        string sign = Sign(SignElement);
        MidPointAuxiliary auxiliary = new MidPointAuxiliary(i1, i2, sign);

        return auxiliary;
    }
}

public class MidPointAuxiliaryState : AuxiliaryState
{
    new MidPointAuxiliary auxiliary;
    Geometry geometry;

    public MidPointAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is MidPointAuxiliary)
            this.auxiliary = (MidPointAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
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

        FormElement formElement2 = new FormElement(1);
        formElement2.fields[0] = geometry.VertexSign(auxiliary.units[0].id);

        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = formElement1;
        formInput.inputs[1] = new FormText("中点");
        formInput.inputs[2] = formElement2;

        return formInput;
    }


}