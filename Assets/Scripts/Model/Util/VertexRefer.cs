public struct VertexRefer
{
    public int id;

    public VertexRefer(int id)
    {
        this.id = id;
    }

    public override string ToString()
    {
        return "VertexRefer: " + id;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this == (VertexRefer)obj;
    }

    public override int GetHashCode()
    {
        return id;
    }

    public static bool operator ==(VertexRefer x, VertexRefer y)
    {
        return x.id == y.id;
    }
    
    public static bool operator !=(VertexRefer x, VertexRefer y)
    {
        return !(x == y);
    }

}
