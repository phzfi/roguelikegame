using UnityEngine;
using System.Collections;

public struct Vector2i
{
	public override string ToString()
	{
		return string.Format("({0}, {1})", x, y);
	}

	public static Vector2i Zero { get { return new Vector2i(0, 0); } }
	public static Vector2i One { get { return new Vector2i(1, 1); } }

	public int x;
	public int y;

	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2i(Vector2 vec)
	{
		this.x = (int)vec.x;
		this.y = (int)vec.y;
	}

	public float Distance(Vector2i vec)
	{
		float dx = x - vec.x;
		float dy = y - vec.y;
		return Mathf.Sqrt(dx * dx + dy * dy);
	}

	public float Length()
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public float SqrLength()
	{
		return x * x + y * y;
	}

	public int GridDistance(Vector2i vec)
	{
		return Mathf.Abs(vec.x - x) + Mathf.Abs(vec.y - y);
	}

	public int GridLength()
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
		return new Vector2 { x = (float)vec1.x / (float)vec2.x, y = (float)vec1.y / (float)vec2.y };
	}

	public static Vector2i operator +(Vector2i vec, int sca)
	{
		return new Vector2i { x = vec.x + sca, y = vec.y + sca };
	}

	public static Vector2i operator -(Vector2i vec, int sca)
	{
		return new Vector2i { x = vec.x - sca, y = vec.y - sca };
	}

	public static Vector2i operator *(Vector2i vec, int sca)
	{
		return new Vector2i { x = vec.x * sca, y = vec.y * sca };
	}

	public static Vector2 operator /(Vector2i vec, int sca)
	{
		return new Vector2 { x = (float)vec.x / (float)sca, y = (float)vec.y / (float)sca };
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
