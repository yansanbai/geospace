using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circular
{
    // Cylinder has 4 Vertices and Cone has 3 Vertices.
    public Vector3[] Vertices;
    public float radius1;
    public float radius2;
    public CircularType type;
    public int circleCount;

    public Circular(Vector3[] vs, float radius1, float radius2, CircularType type)
    {
        this.Vertices = vs;
        this.radius1 = radius1;
        this.radius2 = radius2;
        this.type = type;
        int count = 0;

        if (Vertices.Length == 3)
            count = 1;
        else if (Vertices.Length == 4)
            count = 2;

        if (Vertices[0].z != 0)
            count++;
        if (Vertices[1].z != 0)
            count++;
        this.circleCount = count;
    }

    public bool IsNormalCircular()
    {
        if (Vertices.Length == 3)
        {
            return Vertices[0].y + Vertices[1].y == 0 && Vertices[0].z == Vertices[1].z && Vertices[1].y == Vertices[2].y;
        }
        else if (Vertices.Length == 4)
        {
            return Vertices[0].y + Vertices[1].y == 0 &&  Vertices[0].z == Vertices[1].z && Vertices[1].y == Vertices[2].y && Vertices[2].z == Vertices[3].z && Vertices[0].y == Vertices[3].y;
        }
        else
        {
            return false;
        }
    }
}
