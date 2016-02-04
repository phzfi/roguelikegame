using UnityEngine;
using System.Collections;

public class GlobalParameter<T>
{
	protected string m_name;
	protected T m_value;

	protected GlobalParameter(string name, T defaultValue)
	{
		m_name = name;
		m_value = Load(defaultValue);
	}

	public T Get()
	{
		return m_value;
	}

	public void Set(T value)
	{
		m_value = value;
		Store(value);
	}

	public T Value
	{
		get { return m_value; }
		set { Set(value); }
	}

	public static implicit operator T(GlobalParameter<T> parameter)
	{
		return parameter.m_value;
	}

	virtual public T Load(T defaultValue)
	{
		throw new System.NotImplementedException("GlobalParameter: Load not implemented for type: " + typeof(T));
	}

	virtual public void Store(T value)
	{
		throw new System.NotImplementedException("GlobalParameter: Store not implemented for type: " + typeof(T));
	}
}
// ---------------------------------------------------------------------

public class GlobalFloat : GlobalParameter<float>
{
	public GlobalFloat(string name, float defaultValue) : base(name, defaultValue) { }

	public override float Load(float defaultValue)
	{
		return PlayerPrefs.GetFloat(m_name, defaultValue);
	}

	public override void Store(float value)
	{
		PlayerPrefs.SetFloat(m_name, value);
	}
}
// ---------------------------------------------------------------------

public class GlobalInt : GlobalParameter<int>
{
	public GlobalInt(string name, int defaultValue) : base(name, defaultValue) { }

	public override int Load(int defaultValue)
	{
		return PlayerPrefs.GetInt(m_name, defaultValue);
	}

	public override void Store(int value)
	{
		PlayerPrefs.SetInt(m_name, value);
	}
}
// ---------------------------------------------------------------------

public class GlobalString : GlobalParameter<string>
{
	public GlobalString(string name, string defaultValue) : base(name, defaultValue) { }

	public override string Load(string defaultValue)
	{
		return PlayerPrefs.GetString(m_name, defaultValue);
	}

	public override void Store(string value)
	{
		PlayerPrefs.SetString(m_name, value);
	}
}
// ---------------------------------------------------------------------

public class GlobalBool : GlobalParameter<bool>
{
	public GlobalBool(string name, bool defaultValue) : base(name, defaultValue) { }

	public override bool Load(bool defaultValue)
	{
		return PlayerPrefs.GetInt(m_name, defaultValue ? 1 : 0) != 0;
	}

	public override void Store(bool value)
	{
		PlayerPrefs.SetInt(m_name, value ? 1 : 0);
	}
}
// ---------------------------------------------------------------------

public class GlobalVector2i : GlobalParameter<Vector2i>
{
	public GlobalVector2i(string name, Vector2i defaultValue) : base(name, defaultValue) { }

	public override Vector2i Load(Vector2i defaultValue)
	{
		int x = PlayerPrefs.GetInt(m_name + ".x", defaultValue.x);
		int y = PlayerPrefs.GetInt(m_name + ".y", defaultValue.y);
		return new Vector2i(x, y);
	}

	public override void Store(Vector2i value)
	{
		PlayerPrefs.SetInt(m_name + ".x", value.x);
		PlayerPrefs.SetInt(m_name + ".y", value.y);
	}
}
// ---------------------------------------------------------------------

public class GlobalVector3i : GlobalParameter<Vector3i>
{
	public GlobalVector3i(string name, Vector3i defaultValue) : base(name, defaultValue) { }

	public override Vector3i Load(Vector3i defaultValue)
	{
		int x = PlayerPrefs.GetInt(m_name + ".x", defaultValue.x);
		int y = PlayerPrefs.GetInt(m_name + ".y", defaultValue.y);
		int z = PlayerPrefs.GetInt(m_name + ".z", defaultValue.y);
		return new Vector3i(x, y, z);
	}

	public override void Store(Vector3i value)
	{
		PlayerPrefs.SetInt(m_name + ".x", value.x);
		PlayerPrefs.SetInt(m_name + ".y", value.y);
		PlayerPrefs.SetInt(m_name + ".z", value.z);
	}
}
// ---------------------------------------------------------------------

public class GlobalVector2 : GlobalParameter<Vector2>
{
	public GlobalVector2(string name, Vector2 defaultValue) : base(name, defaultValue) { }

	public override Vector2 Load(Vector2 defaultValue)
	{
		float x = PlayerPrefs.GetFloat(m_name + ".x", defaultValue.x);
		float y = PlayerPrefs.GetFloat(m_name + ".y", defaultValue.y);
		return new Vector2(x, y);
	}

	public override void Store(Vector2 value)
	{
		PlayerPrefs.SetFloat(m_name + ".x", value.x);
		PlayerPrefs.SetFloat(m_name + ".y", value.y);
	}
}
// ---------------------------------------------------------------------

public class GlobalVector3 : GlobalParameter<Vector3>
{
	public GlobalVector3(string name, Vector3 defaultValue) : base(name, defaultValue) { }

	public override Vector3 Load(Vector3 defaultValue)
	{
		float x = PlayerPrefs.GetFloat(m_name + ".x", defaultValue.x);
		float y = PlayerPrefs.GetFloat(m_name + ".y", defaultValue.y);
		float z = PlayerPrefs.GetFloat(m_name + ".z", defaultValue.y);
		return new Vector3(x, y, z);
	}

	public override void Store(Vector3 value)
	{
		PlayerPrefs.SetFloat(m_name + ".x", value.x);
		PlayerPrefs.SetFloat(m_name + ".y", value.y);
		PlayerPrefs.SetFloat(m_name + ".z", value.z);
	}
}
// ---------------------------------------------------------------------