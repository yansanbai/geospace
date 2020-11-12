using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assistor
{
    private Geometry geometry;

    private List<Auxiliary> auxiliaries;
    private Dictionary<VertexUnit, List<Auxiliary>> vertexObservers;
    private Dictionary<VertexUnit, Auxiliary> vertexAuxiliaryMap;
    private Dictionary<GeoElement, Auxiliary> elementAuxiliaryMap;

    public Assistor(Geometry geometry)
    {
        this.geometry = geometry;
        auxiliaries = new List<Auxiliary>();
        vertexObservers = new Dictionary<VertexUnit, List<Auxiliary>>();
        vertexAuxiliaryMap = new Dictionary<VertexUnit, Auxiliary>();
        elementAuxiliaryMap = new Dictionary<GeoElement, Auxiliary>();
    }

    public bool AddAuxiliary(Auxiliary auxiliary)
    {
        foreach (Auxiliary curAuxiliary in auxiliaries) 
        {
            if (curAuxiliary is SpreadAuxiliary && auxiliary is SpreadAuxiliary) {
                return false;
            }
            if (curAuxiliary is SpinAuxiliary && auxiliary is SpinAuxiliary) {
                return false;
            }
        }
        auxiliaries.Add(auxiliary);
        AddObservers(auxiliary);


        return true;
    }

    public bool RemoveAuxiliary(Auxiliary auxiliary)
    {
        RemoveObservers(auxiliary);
        auxiliary.RemoveAuxiliary();

        return auxiliaries.Remove(auxiliary);
    }

    public HashSet<VertexUnit> AuxiliaryTotalObserveVertices(Auxiliary auxiliary)
    {
        HashSet<VertexUnit> vertices = new HashSet<VertexUnit>(auxiliary.units);

        foreach (VertexUnit unit in auxiliary.units)
        {
            List<Auxiliary> obervers = VertexObservers(unit);
            foreach (Auxiliary item in obervers)
                vertices.UnionWith(AuxiliaryTotalObserveVertices(item));
        }

        return vertices;
    }

    public HashSet<VertexUnit> VertexTotalObserveVertices(VertexUnit unit)
    {
        Auxiliary auxiliary = vertexAuxiliaryMap[unit];
        return AuxiliaryTotalObserveVertices(auxiliary);

        // HashSet<VertexUnit> vertices = new HashSet<VertexUnit>();

        // List<Auxiliary> observers = VertexObservers(unit);

        // foreach (Auxiliary a in observers)
        // {
        //     vertices.UnionWith(a.units);
        //     foreach (VertexUnit item in a.units)
        //         vertices.UnionWith(VertexTotalObserveVertices(item));
        // }

        // return vertices;

    }

    // public HashSet<Auxiliary> VertexTotalObserveAuxiliaries(VertexUnit unit)
    // {
    //     HashSet<Auxiliary> obervers = new HashSet<Auxiliary>();
    //     List<Auxiliary> auxiliaries = VertexObservers(unit);

    //     foreach (Auxiliary auxiliary in auxiliaries)
    //         foreach (VertexUnit item in auxiliary.units)
    //             obervers.UnionWith(VertexTotalObserveAuxiliaries(item));

    //     obervers.UnionWith(auxiliaries);
    //     obervers.Add(vertexAuxiliaryMap[unit]);
    //     return obervers;
    // }


    // public HashSet<Auxiliary> VertexAuxiliaries(VertexUnit unit)
    // {
    //     HashSet<Auxiliary> auxiliaries = new HashSet<Auxiliary>(VertexObservers(unit));
    //     Auxiliary dependency;
    //     vertexAuxiliaryMap.TryGetValue(unit, out dependency);
    //     if (dependency != null)
    //         auxiliaries.Add(dependency);
    //     return auxiliaries;
    // }

    // public HashSet<Auxiliary> AuxiliaryTotalObservers(Auxiliary auxiliary)
    // {
    //     List<Auxiliary> observers = AuxiliaryObservers(auxiliary);
    //     HashSet<Auxiliary> total = new HashSet<Auxiliary>(observers);

    //     foreach (Auxiliary item in observers)
    //         total.UnionWith(AuxiliaryTotalObservers(item));

    //     total.Add(auxiliary);

    //     return total;
    // }

    // private List<Auxiliary> AuxiliaryObservers(Auxiliary auxiliary)
    // {
    //     List<Auxiliary> total = new List<Auxiliary>();
    //     foreach (VertexUnit unit in auxiliary.units)
    //     {
    //         List<Auxiliary> observers = VertexObservers(unit);
    //         if (observers != null)
    //             total.AddRange(observers);
    //     }
    //     return total;
    // }

    public Auxiliary ElementAuxiliary(GeoElement element)
    {
        return elementAuxiliaryMap[element];
    }

    public List<Auxiliary> VertexAuxiliaries(VertexUnit unit)
    {
        List<Auxiliary> observers = VertexObservers(unit);
        observers.Add(vertexAuxiliaryMap[unit]);

        return observers;
    }


    private List<Auxiliary> VertexObservers(VertexUnit unit)
    {
        List<Auxiliary> observers;
        if (vertexObservers.TryGetValue(unit, out observers))
            return observers;
        return new List<Auxiliary>();
    }


    private void AddObservers(Auxiliary auxiliary)
    {
        foreach (VertexUnit unit in auxiliary.dependencies)
            AddObserver(unit, auxiliary);

        foreach (VertexUnit unit in auxiliary.units)
            vertexAuxiliaryMap.Add(unit, auxiliary);

        foreach (GeoElement element in auxiliary.elements)
            elementAuxiliaryMap.Add(element, auxiliary);
    }

    private void RemoveObservers(Auxiliary auxiliary)
    {
        foreach (VertexUnit unit in auxiliary.dependencies)
            RemoveObserver(unit, auxiliary);

        foreach (VertexUnit unit in auxiliary.units)
            vertexAuxiliaryMap.Remove(unit);
        
        foreach (GeoElement element in auxiliary.elements)
            elementAuxiliaryMap.Remove(element);
    }

    private void AddObserver(VertexUnit unit, Auxiliary auxiliary)
    {
        List<Auxiliary> observers;
        vertexObservers.TryGetValue(unit, out observers);

        if (observers == null)
        {
            observers = new List<Auxiliary>();
            vertexObservers[unit] = observers;
        }

        observers.Add(auxiliary);
    }

    private void RemoveObserver(VertexUnit unit, Auxiliary auxiliary)
    {
        List<Auxiliary> observers;
        vertexObservers.TryGetValue(unit, out observers);

        if (observers != null)
        {
            observers.Remove(auxiliary);

            if (observers.Count == 0)
                vertexObservers.Remove(unit);
        }
    }

}
