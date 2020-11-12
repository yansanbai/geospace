using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAuxiliary : Auxiliary
{
    public FaceRefer face;

    public PlaneAuxiliary(int[] ids) : base()
    {
        face = new FaceRefer(ids);
    }
    public override void InitWithGeometry(Geometry geometry)
    {
        int count = face.ids.Length;
        VertexUnit[] faceUnits = new VertexUnit[count];
        for (int i = 0; i < count; i++)
            faceUnits[i] = geometry.VertexUnit(face.ids[i]);

        units = new VertexUnit[] { };


        List<GeoElement> elementList = new List<GeoElement>();
        GeoFace geoFace = new GeoFace(faceUnits);
        elementList.Add(geoFace);

        for (int i = 0; i < count; i++)
        {
            int id1 = face.ids[i];
            int index2 = (i + 1) % count;
            int id2 = face.ids[index2];
            if (!geometry.IsEdge(id1, id2))
                elementList.Add(new GeoEdge(faceUnits[i], faceUnits[index2]));
        }

        elements = elementList.ToArray();


        dependencies.AddRange(faceUnits);
    }

    public override void RemoveAuxiliary() {}
}

public class PlaneAuxiliaryTool : AuxiliaryTool
{
    public override FormInput FormInput()
    {
        FormInput formInput = new FormInput(3);

        formInput.inputs[0] = new FormText("连接");
        formInput.inputs[1] = new FormElement(-3);
        formInput.inputs[2] = new FormText("作平面");

        return formInput;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        FormElement formElement = (FormElement)formInput.inputs[1];
        if (IsFace(geometry, formElement))
            return false;
        string[] fields = formElement.fields;
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);
        if (!isPlane(ids, geometry))
        {
            Debug.Log("无法构成平面");
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
        int[] ids = new int[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            ids[i] = geometry.SignVertex(fields[i]);
        

        PlaneAuxiliary auxiliary = new PlaneAuxiliary(ids);

        return auxiliary;
    }

    public bool isPlane(int[] ids, Geometry geometry) {
        Vector3 A = new Vector3();
        Vector3 B = new Vector3();
        Vector3 C = new Vector3();
        for (int i = 0; i < ids.Length - 2; i++)
        {
            A = geometry.UnitVector(ids[i]);
            B = geometry.UnitVector(ids[i+1]);
            C = geometry.UnitVector(ids[i+2]);

            //if (((B.x - A.x) / (C.x - A.x) == (B.y - A.y) / (C.y - A.y)) && ((B.x - A.x) / (C.x - A.x) == (B.z - A.z) / (C.z - A.z)))
            if (!ThreePointsCollinear(A, B, C))
            {
                return true;
            }
        }
        return false;
    }
    public bool ThreePointsCollinear(Vector3 A, Vector3 B, Vector3 C)
    {
        double edge_A = PointsDistance(A, B);
        double edge_B = PointsDistance(B, C);
        double edge_C = PointsDistance(A, C);
        double p = 0.5 * (edge_A + edge_B + edge_C);
        if (p * (p - edge_A) * (p - edge_B) * (p - edge_C) == 0) //area==0
            return true;
        return false;
    }
    public double PointsDistance(Vector3 A, Vector3 B)
    {
        var x1 = A.x - B.x;
        var y1 = A.y - B.y;
        var z1 = A.z - B.z;
        return System.Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1);
    }
}

public class PlaneAuxiliaryState : AuxiliaryState
{
    new PlaneAuxiliary auxiliary;
    Geometry geometry;

    public PlaneAuxiliaryState(Tool tool, Auxiliary auxiliary, Geometry geometry) : base(tool, auxiliary)
    {
        if (auxiliary is PlaneAuxiliary)
            this.auxiliary = (PlaneAuxiliary)auxiliary;

        this.geometry = geometry;
    }

    public override int[] DependVertices()
    {
        return auxiliary.face.ids;
    }

    public override FormInput Title()
    {
        int[] faceIds = auxiliary.face.ids;
        int len = faceIds.Length;

        FormElement formElement = new FormElement(len);
        for (int i = 0; i < len; i++)
        {
            formElement.fields[i] = geometry.VertexSign(faceIds[i]);
        }

        FormInput formInput = new FormInput(2);

        formInput.inputs[0] = new FormText("平面");
        formInput.inputs[1] = formElement;

        return formInput;
    }


}