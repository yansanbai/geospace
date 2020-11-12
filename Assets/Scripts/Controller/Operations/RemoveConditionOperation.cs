using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveConditionOperation : Operation
{
    GeoController geoController;
    Geometry geometry;
    Condition condition;

    public RemoveConditionOperation(GeoController geoController, Geometry geometry, Condition condition)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.geometry = geometry;
        this.condition = condition;
    }

    public override void Start()
    {
        geoController.RemoveCondition(condition);
        if (condition is RectangleCondition || condition is TriangleCondition) {
            geoController.ClearGeometry(3);
        }

        geoController.EndOperation();
    }

    public override void End()
    {

    }
}
