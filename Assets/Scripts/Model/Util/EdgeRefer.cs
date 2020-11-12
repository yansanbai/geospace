public struct EdgeRefer
{
    public int id1;
    public int id2;
    
    public EdgeRefer(int id1, int id2)
    {
        this.id1 = id1;
        this.id2 = id2;
    }

    public void SwapIndex()
    {
        int tmp = id2;
        id2 = id1;
        id1 = tmp;
    }

    public override string ToString()
    {
        return "EdgeRefer: " + id1 + " " + id2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this == (EdgeRefer)obj;
    }

    public override int GetHashCode()
    {
        return id1 + id2;
    }

    public static bool operator ==(EdgeRefer x, EdgeRefer y)
    {
        return (x.id1 == y.id1 && x.id2 == y.id2) || (x.id1 == y.id2 && x.id2 == y.id1);
    }
    public static bool operator !=(EdgeRefer x, EdgeRefer y)
    {
        return !(x == y);
    }

}
