using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Implement
{
    private Geometry geometry;

    private List<Measure> measures;
    private Dictionary<VertexUnit, List<Measure>> vertexObservers;

    public Implement(Geometry geometry)
    {
        this.geometry = geometry;
        measures = new List<Measure>();
        vertexObservers = new Dictionary<VertexUnit, List<Measure>>();
    }

    public bool AddMeasure(Measure measure)
    {
        measures.Add(measure);
        AddObservers(measure);

        return true;
    }

    public bool RemoveMeasure(Measure measure)
    {
        RemoveObservers(measure);

        return measures.Remove(measure);
    }

    public HashSet<Measure> VertexMeasures(VertexUnit unit)
    {
        HashSet<Measure> measures = new HashSet<Measure>(VertexObservers(unit));

        return measures;
    }

    private List<Measure> VertexObservers(VertexUnit unit)
    {
        List<Measure> observers;
        if (vertexObservers.TryGetValue(unit, out observers))
            return observers;
        return new List<Measure>();
    }


    private void AddObservers(Measure measure)
    {
        foreach (VertexUnit unit in measure.dependencies)
            AddObserver(unit, measure);
    }

    private void RemoveObservers(Measure measure)
    {
        foreach (VertexUnit unit in measure.dependencies)
            RemoveObserver(unit, measure);
    }

    private void AddObserver(VertexUnit unit, Measure measure)
    {
        List<Measure> observers;
        vertexObservers.TryGetValue(unit, out observers);

        if (observers == null)
        {
            observers = new List<Measure>();
            vertexObservers[unit] = observers;
        }

        observers.Add(measure);
    }

    private void RemoveObserver(VertexUnit unit, Measure measure)
    {
        List<Measure> observers;
        vertexObservers.TryGetValue(unit, out observers);

        if (observers != null)
        {
            observers.Remove(measure);
        }
    }
}
