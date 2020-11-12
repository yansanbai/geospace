using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
	public Vector3 Vertex1;
    public Vector3 Vertex2;
    public Vector3 Vertex3;

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.Vertex1 = v1;
        this.Vertex2 = v2;
        this.Vertex3 = v3;
    }
}