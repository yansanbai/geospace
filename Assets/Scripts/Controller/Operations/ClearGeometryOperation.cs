using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearGeometryOperation : Operation
{
    GeoController geoController;

    public ClearGeometryOperation(GeoController geoController)
    {
        this.geoController = geoController;
    }

     public override void Start()
    {
        geoController.ClearGeometry(3);

        geoController.EndOperation();
    }

    public override void End()
    {

    }

}
