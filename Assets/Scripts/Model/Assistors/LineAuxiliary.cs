using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAuxiliary : Auxiliary
{
    public EdgeRefer edge;

    public LineAuxiliary(int id1, int id2) : base()
    {
        edge = new EdgeRefer(id1, id2);
    }

    public override void InitWithGeometry(Geometry geometry)
    {
        try {
            VertexUnit unit1 = geometry.VertexUnit(edge.id1);
            VertexUnit unit2 = geometry.VertexUnit(edge.id2);
            units = new VertexUnit[] { };

            GeoEdge geoEdge = new GeoEdge(unit1, unit2);
            elements = new GeoElement[] { geoEdge };
            
            dependencies.Add(unit1);
            dependencies.Add(unit2);
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    public override void RemoveAuxiliary() {}
}

public class LineAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("连接");
        formInput.inputs[1] = new FormElement(2);
        formInput.inputs[2] = new FormText("作线段");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        if (formInput.inputs[1].ToString().Length < 2)
            return false;
        if (IsEdge(geometry, formElement))
            return false;
        string[] fields = formElement.fields;
        int i1 = geometry.SignVertex(fields[0]);
        int i2 = geometry.SignVertex(fields[1]);
        if(i1 == i2){
            return false;
        }
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
        LineAuxiliary auxiliary = new LineAuxiliary(i1, i2);

        return auxiliary;
    }
}

public class LineAuxiliaryState : AuxiliaryState
{
    new LineAuxiliary auxiliary;
    Geometry geometry;

    public LineAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is LineAuxiliary)
            this.auxiliary = (LineAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        int[] ids = new int[] {
            auxiliary.edge.id1,
            auxiliary.edge.id2,
        };
        return ids;
    }

    public override FormInput Title()
    {
        FormElement formElement = new FormElement(2);
        formElement.fields[0] = geometry.VertexSign(auxiliary.edge.id1);
        formElement.fields[1] = geometry.VertexSign(auxiliary.edge.id2);

        FormInput formInput = new FormInput(2);

        formInput.inputs[0] = new FormText("线段");
        formInput.inputs[1] = formElement;

        return formInput;
    }


}