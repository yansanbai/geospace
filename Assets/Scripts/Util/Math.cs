using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Range<T>
{
    public T min;
    public T max;

    public Range(T min, T max) : this()
    {
        this.min = min;
        this.max = max;
    }
}

public static class Math
{

    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static float Mod(float x, int m)
    {
        return (x % m + m) % m;
    }


    public static bool InRange(float x, float a, float b)
    {
        return x >= a && x <= b;
    }

    public static bool OutRange(float x, float a, float b)
    {
        return x < a || x > b;
    }



    public static bool AboutEquals(float a, float b)
    {
        // float epsilon = Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)) * 1E-10F;
        return Mathf.Approximately(a, b);
    }

    public static bool AboutEquals(float a, float b, float epsilon)
    {
        return Mathf.Abs(a - b) <= epsilon;
    }

    public static bool AboutEqualsZero(float a)
    {
        return AboutEquals(a, 0, 1E-6F);
    }
}
