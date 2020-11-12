using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face 
{
    public Vector3[] Vertices;

    public bool Canselected;

    public FaceType faceType;

    public Face(Vector3[] vs, bool Canselected = true, FaceType faceType = FaceType.Normal)
    {
        this.Vertices = vs;
        this.Canselected = Canselected;
        this.faceType = faceType;
    }
}
