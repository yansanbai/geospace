using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    Vector3 point;

    // public Func<Ray, Vector3> OnMove;
    public Vector3 Point
    {
        get
        {
            return point;
        }
        set
        {
            point = value;
        }
    }

    public Vertex(float x, float y, float z)
    {
        this.Point = new Vector3(x, y, z);
    }

    public Vertex(Vector3 point)
    {
        this.Point = point;
    }

}
