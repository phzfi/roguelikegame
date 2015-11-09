using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ItemManager : MonoBehaviour {

    private static List<Item> sm_allItems = new List<Item>(); // Private lists for iterating over all items
    private static List<Item> sm_itemsOnMap = new List<Item>();

    private static Dictionary<int, Item> sm_allItemsDictionary = new Dictionary<int, Item>(); // Private dictionaries for fast item fetching by ID. Use GetItem and GetItemOnMap to access these.
    private static Dictionary<int, Item> sm_itemsOnMapDictionary = new Dictionary<int, Item>();

    public static ReadOnlyCollection<Item> AllItems { get { return sm_allItems.AsReadOnly(); } } // Public properties expose only read-only version of lists to prevent modification
    public static ReadOnlyCollection<Item> ItemsOnMap { get { return sm_itemsOnMap.AsReadOnly(); } }

    private static int sm_curID = 0; // TODO: will not stay synchronized if client and server have different amounts of objects, eg. when not synchronizing unseen objects

    public static void OnLevelWasLoaded(int level) // reset all containers and sm_curID when loading new scene
    {
        Reset();
    }

    public static void Register(Item item, out int ID, bool onMap = true)
    {
        // Add new item, return item ID.

        ID = sm_curID++;

        sm_allItems.Add(item);
        sm_allItemsDictionary.Add(ID, item);

        if(onMap)
        {
            sm_itemsOnMap.Add(item);
            sm_itemsOnMapDictionary.Add(ID, item);
        }
    }
    
    public static void Unregister(int ID)
    {
        // Remove item from all containers by ID.

        var obj = GetItem(ID);
        if (obj == null)
            return;
        sm_allItemsDictionary.Remove(ID);
        sm_allItems.Remove(obj);

        if (GetItemOnMap(ID) != null) // if item on map, remove it from there too
        {
            sm_itemsOnMapDictionary.Remove(ID);
            sm_itemsOnMap.Remove(obj);
        }
    }

    public static void UnregisterFromMap(int ID)
    {
        // Remove item from map containers by ID.

        var obj = GetItemOnMap(ID);
        if (obj == null)
            return;
        sm_itemsOnMapDictionary.Remove(ID);
        sm_itemsOnMap.Remove(obj);
    }

    public static Item GetItem(int ID)
    {
        if (!sm_allItemsDictionary.ContainsKey(ID)) // if ID not found in dictionary, something's wrong
        {
            Debug.Log("Item ID not found from dictionary, ID: " + ID);
            return null;
        }
        return sm_allItemsDictionary[ID];
    }

    public static Item GetItemOnMap(int ID)
    {
        if (!sm_itemsOnMapDictionary.ContainsKey(ID)) // if ID not found in dictionary, item's not on map
            return null;

        return sm_itemsOnMapDictionary[ID];
    }

    public static void OrderPickup(PickupOrder order)
    {
        var mover = MovementManager.GetObject(order.m_playerID);
        var item = GetItemOnMap(order.m_itemID);
        item.Pickup(mover.gameObject);
    }

    private static void Reset()
    {
        sm_allItems = new List<Item>();
        sm_itemsOnMap = new List<Item>();

        sm_allItemsDictionary = new Dictionary<int, Item>();
        sm_itemsOnMapDictionary = new Dictionary<int, Item>();

        sm_curID = 0;
    }
}
