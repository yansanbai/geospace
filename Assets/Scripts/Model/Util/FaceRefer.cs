using System.Collections;
using System.Collections.Generic;

public struct FaceRefer
{
    public int[] ids;

    public FaceRefer(int[] ids)
    {
        this.ids = ids;
    }

    public override string ToString()
    {
        return "FaceRefer: " + string.Join(" ", ids);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this == (FaceRefer)obj;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (int id in ids)
            hash += id;
        return hash;
    }

    public static bool operator ==(FaceRefer x, FaceRefer y)
    {
        SortedSet<int> xSet = new SortedSet<int>(x.ids);
        SortedSet<int> ySet = new SortedSet<int>(y.ids);
        if (SortedSet<int>.CreateSetComparer().Equals(xSet, ySet))
            return true;
        return false;
    }

    public static bool operator !=(FaceRefer x, FaceRefer y)
    {
        return !(x == y);
    }
}
