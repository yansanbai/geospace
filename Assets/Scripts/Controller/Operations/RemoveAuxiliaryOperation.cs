using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAuxiliaryOperation : Operation
{
    GeoController geoController;
    Geometry geometry;
    Auxiliary auxiliary;

    public RemoveAuxiliaryOperation(GeoController geoController, Geometry geometry, Auxiliary auxiliary)
    {
        CanRotateCamera = true;
        CanActiveElement = true;

        this.geoController = geoController;
        this.geometry = geometry;
        this.auxiliary = auxiliary;
    }

    public override void Start()
    {
        HashSet<VertexUnit> vertices = geometry.Assistor.AuxiliaryTotalObserveVertices(auxiliary);

        SortedSet<Auxiliary> oberveAuxiliaries = new SortedSet<Auxiliary>(
            Comparer<Auxiliary>.Create((x, y) => y.id - x.id)
        );

        SortedSet<Measure> oberveMeasures = new SortedSet<Measure>(
            Comparer<Measure>.Create((x, y) => y.id - x.id)
        );

        foreach (VertexUnit unit in vertices)
        {
            oberveAuxiliaries.UnionWith(geometry.Assistor.VertexAuxiliaries(unit));
            oberveMeasures.UnionWith(geometry.Implement.VertexMeasures(unit));
        }
        
        oberveAuxiliaries.Add(auxiliary);

        foreach (Auxiliary oAuxiliary in oberveAuxiliaries)
            geoController.RemoveAuxiliary(oAuxiliary);
        foreach (Measure oMeasure in oberveMeasures)
            geoController.RemoveMeasure(oMeasure);

        geoController.RefreshGeometryBehaviour();

        geoController.EndOperation();
    }

    public override void End()
    {

    }




}
