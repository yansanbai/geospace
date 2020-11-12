using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
	public Vector3 Vertex1;
    public Vector3 Vertex2;
    public bool CanSelected;

    public Edge(Vector3 v1, Vector3 v2, bool CanSelected = true)
    {
        this.Vertex1 = v1;
        this.Vertex2 = v2;
        this.CanSelected = CanSelected;
    }

    public float Length()
    {
        return Vector3.Magnitude(Vertex1 - Vertex2);
    }
}