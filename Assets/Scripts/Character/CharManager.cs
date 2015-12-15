using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class CharManager : MonoBehaviour {

	private static List<CharController> sm_objects = new List<CharController>(); // Private list for iterating over all objects
	private static Dictionary<int, CharController> sm_objectDictionary = new Dictionary<int, CharController>(); // Private dictionary for fast item fetching by ID. Use GetObject to access this.

	public static ReadOnlyCollection<CharController> Objects { get { return sm_objects.AsReadOnly(); } } // Public property exposes only read-only version of list to prevent modification

	public void OnLevelWasLoaded(int level) // Reset all containers and sm_curID when loading new scene
	{
		Reset();
	}

	private static void Reset()
	{
		sm_objectDictionary = new Dictionary<int, CharController>();
		sm_objects = new List<CharController>();
	}

	public static void Register(CharController item) // Add new object to objects list
	{
		sm_objects.Add(item);
		sm_objectDictionary.Add(item.ID, item);
	}

	public static void Unregister(int ID) // Remove object by ID.
	{
		var obj = GetObject(ID);
		if (obj == null)
			return;
		sm_objectDictionary.Remove(ID);
		sm_objects.Remove(obj);
	}

	public static CharController GetLocalPlayer()
	{
		for (int i = 0; i < Objects.Count; ++i)
		{
			var controller = Objects[i];
			if (controller.isLocalPlayer && controller.m_isPlayer)
				return controller;
		}
		return null;
	}

	public static CharController GetObject(int ID) // get mover associated with given ID
	{
		if (!sm_objectDictionary.ContainsKey(ID)) // if ID not found in dictionary, something's wrong
		{
			Debug.Log("Object ID not found from dictionary, ID: " + ID);
			return null;
		}
		return sm_objectDictionary[ID];
	}

}
