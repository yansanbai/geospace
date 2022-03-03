using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VertexUnit
{
    public int id;
    public bool isFixed = false;
    public bool isBase = false;

    protected Vector3 position;

    private List<GeoElement> observeElements = new List<GeoElement>();
    private List<Gizmo> observeGizmos = new List<Gizmo>();
    public List<VertexUnit> dependencies = new List<VertexUnit>(); //依赖自己的顶点列表

    public string preferredSign;

    public VertexUnit()
    {
        position = Vector3.zero;
    }

    public VertexUnit(Vector3 position)
    {
        this.position = position;
    }

    public VertexUnit(Vector3 position, bool isFixed)
    {
        this.position = position;
        this.isFixed = isFixed;
    }

    public VertexUnit(float x, float y, float z)
    {
        position = new Vector3(x, y, z);
    }

    public abstract void AddDependencies();
    public abstract void RemoveDependencies();

    public void AddDependency(VertexUnit vertex)
    {
        if (dependencies.Contains(vertex))
            return;
        dependencies.Add(vertex);
    }

    public void RemoveDependency(VertexUnit vertex)
    {
        dependencies.Remove(vertex);
    }

    public void AddObserveElement(GeoElement element)
    {
        if (observeElements.Contains(element))
            return;
        observeElements.Add(element);
    }

    public void RemoveObserveElement(GeoElement element)
    {
        observeElements.Remove(element);
    }

    public void AddObserveGizmo(Gizmo gizmo)
    {
        if (observeGizmos.Contains(gizmo))
            return;
        observeGizmos.Add(gizmo);
    }

    public void RemoveObserveGizmo(Gizmo gizmo)
    {
        observeGizmos.Remove(gizmo);
    }

    protected void PositionChanged()
    {
        foreach (VertexUnit vertex in dependencies)
            vertex.RefreshPosition();
    }

    public virtual void Move(Ray ray, Transform camera, bool snap = false)
    {
        PositionChanged();
    }

    public virtual void SetPosition(Vector3 position)
    {
        this.position = position;
        PositionChanged();
    }


    protected bool SnapPosition(Vector3 position, out Vector3 snapPosition) //捕捉位置
    {
        snapPosition = position;
        snapPosition.x = Mathf.Round(position.x);
        snapPosition.y = Mathf.Round(position.y);
        snapPosition.z = Mathf.Round(position.z);

        if (Vector3.Magnitude(snapPosition - position) <= 0.2f)
            return true;
        return false;
    }

    public Vector3 Position()
    {
        return position;
    }

    public virtual void RefreshPosition() //刷新位置
    {
        PositionChanged();
    }

    public List<GeoElement> TotalObserveElements()
    {
        List<GeoElement> total = new List<GeoElement>(observeElements);
        foreach (VertexUnit dependency in dependencies)
        {
            List<GeoElement> list = dependency.TotalObserveElements();
            total.AddRange(list);
        }
        return total;
    }

    public List<Gizmo> TotalObserveGizmos()
    {
        List<Gizmo> total = new List<Gizmo>(observeGizmos);
        foreach (VertexUnit dependency in dependencies)
        {
            List<Gizmo> list = dependency.TotalObserveGizmos();
            total.AddRange(list);
        }

        return total;
    }

    public override string ToString()
    {
        return this.GetType().ToString() + "  " + id + "  " + position;
    }
}

public class VertexSpace : VertexUnit //空间中的点
{
    public VertexSpace(Vector3 position) : base(position)
    {

    }

    public VertexSpace(float x, float y, float z) : base(x, y, z)
    {

    }

    public override void AddDependencies()
    {

    }

    public override void RemoveDependencies()
    {

    }

    public override void Move(Ray ray, Transform camera, bool snap)
    {
        Plane screenPlane = new Plane(-ray.direction, position);

        float distance = 0;
        if (screenPlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        if (snap)
        {
            Vector3 snapPosition;
            if (SnapPosition(position, out snapPosition))
                position = snapPosition;
        }

        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        base.RefreshPosition();
    }

}

public class VertexFace : VertexUnit  //面上的点
{
    protected Vector3 faceNormal;

    public VertexFace(float x, float y, float z, Vector3 faceNormal) : base(x, y, z)
    {
        this.faceNormal = faceNormal;
    }

    public VertexFace(Vector3 position, Vector3 faceNormal) : base(position)
    {
        this.faceNormal = faceNormal;
    }

    public override void AddDependencies()
    {

    }
    public override void RemoveDependencies()
    {

    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        Plane facePlane = new Plane(faceNormal, position);

        float distance = 0;
        if (facePlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        if (snap)
        {
            Vector3 snapPosition;
            if (SnapPosition(position, out snapPosition))
            {
                float snapDistance = facePlane.GetDistanceToPoint(snapPosition);
                if (Math.AboutEqualsZero(snapDistance))
                    position = snapPosition;
            }
        }

        base.PositionChanged();

    }

    public override void RefreshPosition()
    {
        Plane facePlane = new Plane(faceNormal, position);
        Ray ray = new Ray(position, faceNormal);

        float distance = 0;
        if (facePlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        base.RefreshPosition();
    }
}

public class VertexResolvedBody: VertexUnit {   //分解体？
    protected Vector3 faceNormal;

    float signX;
    float signY;
    float signZ;

    public Vector3 pos;

    public VertexResolvedBody(float x, float y, float z, Vector3 faceNormal) : base(x, y, z)
    {
        this.faceNormal = faceNormal;
        signX = Mathf.Sign(x);
        signY = Mathf.Sign(y);
        signZ = Mathf.Sign(z);
        pos = new Vector3(x, y, z);
    }

    public VertexResolvedBody(Vector3 position, Vector3 faceNormal) : base(position)
    {
        this.faceNormal = faceNormal;
    }

    public override void AddDependencies()
    {

    }
    public override void RemoveDependencies()
    {

    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        Plane facePlane = new Plane(faceNormal, position);

        float distance = 0;
        if (facePlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
            RestrictPosition();
        }

        if (snap)
        {
            Vector3 snapPosition;
            if (SnapPosition(position, out snapPosition))
            {
                float snapDistance = facePlane.GetDistanceToPoint(snapPosition);
                if (Math.AboutEqualsZero(snapDistance))
                    position = snapPosition;
            }
        }

        base.PositionChanged();

    }

    public override void RefreshPosition()
    {
        Plane facePlane = new Plane(faceNormal, position);
        Ray ray = new Ray(position, faceNormal);

        float distance = 0;
        if (facePlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
        }

        base.RefreshPosition();
    }

    public override void SetPosition(Vector3 position)
    {
        this.position = position;
        RestrictPosition();
        base.PositionChanged();
    }

    public void SetAbsPosition(Vector3 position)
    {
        position.x = signY * Mathf.Abs(position.x);
        position.y = signY * Mathf.Abs(position.y);
        position.z = signZ * Mathf.Abs(position.z);

        SetPosition(position);
    }

    private void RestrictPosition()
    {
        if (signX > 0 && position.x < 0)
            position.x = 0;
        if (signX < 0 && position.x > 0)
            position.x = 0;

        if (signY > 0 && position.y < 0)
            position.y = 0;
        if (signY < 0 && position.y > 0)
            position.y = 0;

        if (signZ > 0 && position.z < 0)
            position.z = 0;
        if (signZ < 0 && position.z > 0)
            position.z = 0;
    }
}


public class VertexCuboid : VertexUnit   //长方体的点
{
    float signX;
    float signY;
    float signZ;

    public VertexCuboid(float x, float y, float z) : base(x, y, z)
    {
        signX = Mathf.Sign(x);
        signY = Mathf.Sign(y);
        signZ = Mathf.Sign(z);
    }

    public override void AddDependencies()
    {

    }
    public override void RemoveDependencies()
    {

    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        Plane screenPlane = new Plane(-ray.direction, position);

        float distance = 0;
        if (screenPlane.Raycast(ray, out distance))
        {
            position = ray.GetPoint(distance);
            RestrictPosition();
        }

        if (snap)
        {
            Vector3 snapPosition;
            if (SnapPosition(position, out snapPosition))
                position = snapPosition;
        }

        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        base.RefreshPosition();
    }

    public override void SetPosition(Vector3 position)
    {
        this.position = position;
        RestrictPosition();
        base.PositionChanged();
    }

    public void SetAbsPosition(Vector3 position)
    {
        position.x = signX * Mathf.Abs(position.x);
        position.y = signY * Mathf.Abs(position.y);
        position.z = signZ * Mathf.Abs(position.z);

        SetPosition(position);
    }

    private void RestrictPosition()
    {
        if (signX > 0 && position.x < 0)
            position.x = 0;
        if (signX < 0 && position.x > 0)
            position.x = 0;

        if (signY > 0 && position.y < 0)
            position.y = 0;
        if (signY < 0 && position.y > 0)
            position.y = 0;

        if (signZ > 0 && position.z < 0)
            position.z = 0;
        if (signZ < 0 && position.z > 0)
            position.z = 0;
    }
}

public class VertexLineFixed : VertexUnit   //线上固定点
{
    // dependencies
    private VertexUnit dVertex1;
    private VertexUnit dVertex2;
    private float ratio;

    public VertexLineFixed(VertexUnit dVertex1, VertexUnit dVertex2, float ratio)
    {
        isFixed = true;

        this.dVertex1 = dVertex1;
        this.dVertex2 = dVertex2;
        this.ratio = ratio;

        RefreshPosition();
    }

    public override void AddDependencies()
    {
        dVertex1.AddDependency(this);
        dVertex2.AddDependency(this);
    }

    public override void RemoveDependencies()
    {
        dVertex1.RemoveDependency(this);
        dVertex2.RemoveDependency(this);
    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        position = (dVertex2.Position() - dVertex1.Position()) * ratio + dVertex1.Position();
        base.RefreshPosition();
    }
}

public class VertexLineVertical : VertexUnit   //线的垂线的垂足
{
    // dependencies
    private VertexUnit dVertex1; // vertex  垂线顶点
    private VertexUnit dVertex2; // edge   线段起点
    private VertexUnit dVertex3; // edge   线段终点

    public VertexLineVertical(VertexUnit dVertex1, VertexUnit dVertex2, VertexUnit dVertex3)
    {
        isFixed = true;

        this.dVertex1 = dVertex1;
        this.dVertex2 = dVertex2;
        this.dVertex3 = dVertex3;

        RefreshPosition();
    }

    public override void AddDependencies()
    {
        dVertex1.AddDependency(this);
        dVertex2.AddDependency(this);
        dVertex3.AddDependency(this);
    }

    public override void RemoveDependencies()
    {
        dVertex1.RemoveDependency(this);
        dVertex2.RemoveDependency(this);
        dVertex3.RemoveDependency(this);
    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        Vector3 dir23 = dVertex2.Position() - dVertex3.Position();
        Vector3 dir13 = dVertex1.Position() - dVertex3.Position();
        float len = Vector3.Dot(dir13, dir23.normalized);
        position = dVertex3.Position() + len * dir23.normalized;
        base.RefreshPosition();
    }
}

public class VertexPlaneVertical : VertexUnit  //面的垂线的垂足
{
    // dependencies
    private VertexUnit dVertex1; // vertex
    private VertexUnit[] dVertices; // face

    public VertexPlaneVertical(VertexUnit dVertex1, VertexUnit[] dVertices)
    {
        isFixed = true;

        this.dVertex1 = dVertex1;

        if (dVertices.Length <= 2)
            dVertices = new VertexUnit[3];

        this.dVertices = dVertices;

        RefreshPosition();
    }

    public override void AddDependencies()
    {
        dVertex1.AddDependency(this);
        foreach (VertexUnit unit in dVertices)
            unit.AddDependency(this);
    }

    public override void RemoveDependencies()
    {
        dVertex1.RemoveDependency(this);
        foreach (VertexUnit unit in dVertices)
            unit.RemoveDependency(this);
    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        base.PositionChanged();
    }

    public override void RefreshPosition()
    {

        Vector3 dir1 = dVertices[1].Position() - dVertices[0].Position();
        Vector3 dir2 = dVertices[2].Position() - dVertices[0].Position();
        Vector3 normal = Vector3.Cross(dir1, dir2);
        // position = Vector3.ProjectOnPlane(dVertex1.Position(), normal);
        Plane plane = new Plane(normal, dVertices[0].Position());
        position = plane.ClosestPointOnPlane(dVertex1.Position());
        base.RefreshPosition();
    }
}

public class VertexLine : VertexUnit  //线段上的点
{
    // dependencies
    private VertexUnit dVertex1;
    private VertexUnit dVertex2;
    private float ratio;

    public VertexLine(VertexUnit dVertex1, VertexUnit dVertex2, float ratio)
    {
        isFixed = false;

        this.dVertex1 = dVertex1;
        this.dVertex2 = dVertex2;
        this.ratio = ratio;

        RefreshPosition();

    }

    public override void AddDependencies()
    {
        dVertex1.AddDependency(this);
        dVertex2.AddDependency(this);
    }

    public override void RemoveDependencies()
    {
        dVertex1.RemoveDependency(this);
        dVertex2.RemoveDependency(this);
    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {

        Vector3 d1 = dVertex2.Position() - dVertex1.Position();
        Vector3 d2 = -d1;
        Vector3 pDir = camera.InverseTransformDirection(d1);
        pDir.z = 0;
        pDir = camera.TransformDirection(pDir);
        Plane plane = new Plane(pDir, ray.origin);
        Ray ray1 = new Ray(dVertex1.Position(), d1);
        Ray ray2 = new Ray(dVertex2.Position(), d2);

        float enter1, enter2;
        bool isCast1 = plane.Raycast(ray1, out enter1);
        bool isCast2 = plane.Raycast(ray2, out enter2);
        if (isCast1 && isCast2)
        {
            ratio = enter1 / d1.magnitude;
        }
        else if (isCast1)
        {
            ratio = 1;
        }
        else if (isCast2)
        {
            ratio = 0;
        }
        RefreshPosition();

        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        position = (dVertex2.Position() - dVertex1.Position()) * ratio + dVertex1.Position();
        base.RefreshPosition();
    }
}

public class VertexPlaneCenter : VertexUnit  //面的重心
{
    // dependencies
    private VertexUnit[] dVertices;

    public VertexPlaneCenter(VertexUnit[] dVertices)
    {
        isFixed = true;

        if (dVertices.Length <= 2)
            dVertices = new VertexUnit[3];

        this.dVertices = dVertices;
        RefreshPosition();

    }

    public override void AddDependencies()
    {
        foreach (VertexUnit dVertex in dVertices)
            dVertex.AddDependency(this);
    }

    public override void RemoveDependencies()
    {
        foreach (VertexUnit dVertex in dVertices)
            dVertex.RemoveDependency(this);
    }

    public override void Move(Ray ray, Transform camera, bool snap = false)
    {
        base.PositionChanged();
    }

    public override void RefreshPosition()
    {
        Vector3 center = Vector3.zero;
        foreach (VertexUnit dVertex in dVertices)
            center += dVertex.Position();
        position = center / dVertices.Length;
        base.RefreshPosition();
    }
}