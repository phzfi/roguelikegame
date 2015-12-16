using UnityEngine;
using System.Collections;

public struct Vector3i
{
	public override string ToString()
	{
		return string.Format("({0}, {1}, {2})", x, y, z);
	}

	public static Vector3i Zero { get { return new Vector3i(0, 0, 0); } }
	public static Vector3i One { get { return new Vector3i(1, 1, 1); } }

	public int x;
	public int y;
	public int z;

	public Vector3i(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3i(Vector3 vec)
	{
		this.x = (int)vec.x;
		this.y = (int)vec.y;
		this.z = (int)vec.z;
	}

	public float Distance(Vector3i vec)
	{
		float dx = x - vec.x;
		float dy = y - vec.y;
		float dz = z - vec.z;
		return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
	}

	public float Length()
	{
		return Mathf.Sqrt(x * x + y * y + z * z);
	}

	public float SqrLength()
	{
		return x * x + y * y + z * z;
	}

	public int GridDistance(Vector3i vec)
	{
		return Mathf.Abs(vec.x - x) + Mathf.Abs(vec.y - y);
	}

	public int GridLength()
	{
		return Mathf.Abs(x) + Mathf.Abs(y);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Vector3i))
			return false;
		var gp = (Vector3i)obj;
		return gp.x == x && gp.y == y && gp.z == z;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
	}

	public static Vector3i operator +(Vector3i vec1, Vector3i vec2)
	{
		return new Vector3i { x = vec1.x + vec2.x, y = vec1.y + vec2.y, z = vec1.z + vec2.z };
	}

	public static Vector3i operator -(Vector3i vec1, Vector3i vec2)
	{
		return new Vector3i { x = vec1.x - vec2.x, y = vec1.y - vec2.y, z = vec1.z - vec2.z };
	}

	public static Vector3i operator *(Vector3i vec1, Vector3i vec2)
	{
		return new Vector3i { x = vec1.x * vec2.x, y = vec1.y * vec2.y, z = vec1.z * vec2.z };
	}

	public static Vector3 operator /(Vector3i vec1, Vector3i vec2)
	{
		return new Vector3 { x = (float)vec1.x / (float)vec2.x, y = (float)vec1.y / (float)vec2.y, z = (float)vec1.z / (float)vec2.z };
	}

	public static Vector3i operator +(Vector3i vec, int sca)
	{
		return new Vector3i { x = vec.x + sca, y = vec.y + sca, z = vec.z + sca };
	}

	public static Vector3i operator -(Vector3i vec, int sca)
	{
		return new Vector3i { x = vec.x - sca, y = vec.y - sca, z = vec.z - sca };
	}

	public static Vector3i operator *(Vector3i vec, int sca)
	{
		return new Vector3i { x = vec.x * sca, y = vec.y * sca, z = vec.z * sca };
	}

	public static Vector3 operator /(Vector3i vec, int sca)
	{
		return new Vector3 { x = (float)vec.x / (float)sca, y = (float)vec.y / (float)sca, z = (float)vec.z / (float)sca };
	}

	public static bool operator ==(Vector3i vec1, Vector3i vec2)
	{
		return vec1.Equals(vec2);
	}

	public static bool operator !=(Vector3i vec1, Vector3i vec2)
	{
		return !vec1.Equals(vec2);
	}

}
