using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gizmo
{
    public string name;
    public abstract int[] DependentIds();
}


public class GizmoRight : Gizmo
{
    public CornerRefer corner;

    public GizmoRight(CornerRefer corner)
    {
        name = "GizmoRight";

        this.corner = corner;
    }

    public GizmoRight(int id1, int id2, int id3)
    {
        name = "GizmoRight";

        corner = new CornerRefer(id1, id2, id3);
    }

    public override int[] DependentIds()
    {
        return new int[] { corner.id1, corner.id2, corner.id3 };
    }

}

public class GizmoCorner : Gizmo
{
    public CornerRefer corner;

    public GizmoCorner(CornerRefer corner)
    {
        name = "GizmoCorner";

        this.corner = corner;
    }

    public GizmoCorner(int id1, int id2, int id3)
    {
        name = "GizmoCorner";

        corner = new CornerRefer(id1, id2, id3);
    }

    public override int[] DependentIds()
    {
        return new int[] { corner.id1, corner.id2, corner.id3 };
    }

}

public class GizmoLength : Gizmo
{
    public EdgeRefer edge;

    public GizmoLength(EdgeRefer edge)
    {
        name = "GizmoLength";

        this.edge = edge;
    }

    public GizmoLength(int id1, int id2)
    {
        name = "GizmoLength";

        edge = new EdgeRefer(id1, id2);
    }

    public override int[] DependentIds()
    {
        return new int[] { edge.id1, edge.id2 };
    }
}

public class GizmoAngle : Gizmo
{
    public CornerRefer corner;
    public float angle;

    public GizmoAngle(CornerRefer corner)
    {
        name = "GizmoAngle";

        this.corner = corner;
    }


    public GizmoAngle(int id1, int id2, int id3)
    {
        name = "GizmoAngle";

        corner = new CornerRefer(id1, id2, id3);
    }

    public override int[] DependentIds()
    {
        return new int[] { corner.id1, corner.id2, corner.id3 };
    }
}

public class GizmoArea : Gizmo
{
    public FaceRefer face;
    public GizmoArea(FaceRefer face)
    {
        name = "GizmoArea";

        this.face = face;
    }

    public GizmoArea(int[] ids)
    {
        name = "GizmoArea";

        face = new FaceRefer(ids);
    }

    public override int[] DependentIds()
    {
        return face.ids;
    }
}

public class GizmoSurface : Gizmo
{
    public FaceRefer face;
    public GizmoSurface(FaceRefer face)
    {
        name = "GizmoSurface";

        this.face = face;
    }

    public GizmoSurface(int[] ids)
    {
        name = "GizmoSurface";

        face = new FaceRefer(ids);
    }

    public override int[] DependentIds()
    {
        return face.ids;
    }
}

public class GizmoVolume : Gizmo
{
    public FaceRefer face;
    public GizmoVolume(FaceRefer face)
    {
        name = "GizmoVolume";

        this.face = face;
    }

    public GizmoVolume(int[] ids)
    {
        name = "GizmoVolume";

        face = new FaceRefer(ids);
    }

    public override int[] DependentIds()
    {
        return face.ids;
    }
}