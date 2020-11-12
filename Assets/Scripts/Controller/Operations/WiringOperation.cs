using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingOperation : Operation
{
    GeoController geoController;
    Geometry geometry;

    public WritingOperation(GeoController geoController, Geometry geometry)
    {
        CanRotateCamera = false;
        CanActiveElement = true;

        this.geoController = geoController;
        this.geometry = geometry;
    }

    public override void Start()
    {
        geoController.EndOperation();
    }


    public override void End()
    {

    }

}
