public struct CornerRefer
{
    public int id1;
    public int id2;
    public int id3;

    public CornerRefer(int id1, int id2, int id3)
    {
        this.id1 = id1;
        this.id2 = id2;
        this.id3 = id3;
    }

    public void SwapIndex()
    {
        int tmp = id3;
        id3 = id1;
        id1 = tmp;
    }

    public override string ToString()
    {
        return "CornerRefer: " + id1 + " " + id2 + " " + id3;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this == (CornerRefer)obj;
    }

    public override int GetHashCode()
    {
        return id1 + id2 + id3;
    }

    public static bool operator ==(CornerRefer x, CornerRefer y)
    {
        return (x.id1 == y.id1 && x.id2 == y.id2 && x.id3 == y.id3) || (x.id1 == y.id3 && x.id2 == y.id2 && x.id3 == y.id1);
    }

    public static bool operator !=(CornerRefer x, CornerRefer y)
    {
        return !(x == y);
    }

}