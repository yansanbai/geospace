using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveMeasureOperation : Operation
{
    GeoController geoController;
    Geometry geometry;
    Measure measure;

    public RemoveMeasureOperation(GeoController geoController, Geometry geometry, Measure measure)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.geometry = geometry;
        this.measure = measure;
    }

    public override void Start()
    {
        geoController.RemoveMeasure(measure);

        geoController.EndOperation();
    }


    public override void End()
    {

    }

}
