using UnityEngine;
using System.Collections;


public struct Vector2i
{

    public override string ToString()
    {
        return string.Format("[GridPosition {0}, {1}]", x, y);
    }

    public static Vector2i None = new Vector2i { x = -1, y = -1 };

    public int x;
    public int y;

    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int Distance(Vector2i vec)
    {
        return Mathf.Abs(vec.x - x) + Mathf.Abs(vec.y - y);
    }

    public int Length()
    {
        return Mathf.Abs(x) + Mathf.Abs(y);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vector2i))
            return false;
        var gp = (Vector2i)obj;
        return gp.x == x && gp.y == y;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public static Vector2i operator +(Vector2i vec1, Vector2i vec2)
    {
        return new Vector2i { x = vec1.x + vec2.x, y = vec1.y + vec2.y };
    }

    public static Vector2i operator -(Vector2i vec1, Vector2i vec2)
    {
        return new Vector2i { x = vec1.x - vec2.x, y = vec1.y - vec2.y };
    }

    public static Vector2i operator *(Vector2i vec1, Vector2i vec2)
    {
        return new Vector2i { x = vec1.x * vec2.x, y = vec1.y * vec2.y };
    }

    public static Vector2 operator /(Vector2i vec1, Vector2i vec2)
    {
        return new Vector2 { x = (float)vec1.x / vec2.x, y = (float)vec1.y / vec2.y };
    }

    public static bool operator ==(Vector2i vec1, Vector2i vec2)
    {
        return vec1.Equals(vec2);
    }

    public static bool operator !=(Vector2i vec1, Vector2i vec2)
    {
        return !vec1.Equals(vec2);
    }

}
