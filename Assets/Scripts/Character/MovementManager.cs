using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MovementManager : MonoBehaviour {

    //TODO: don't use GameObject here
    public static NavGridScript sm_grid;

    private static List<SimpleCharacterMovement> sm_objects = new List<SimpleCharacterMovement>(); // Private list for iterating over all objects
    private static Dictionary<int, SimpleCharacterMovement> sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>(); // Private dictionary for fast item fetching by ID. Use GetObject to access this.

    public static ReadOnlyCollection<SimpleCharacterMovement> Objects { get { return sm_objects.AsReadOnly(); } } // Public property exposes only read-only version of list to prevent modification

    public void Start()
    {
        sm_grid = GameObject.FindObjectOfType<NavGridScript>();
    }

    public void OnLevelWasLoaded(int level) // Reset all containers and sm_curID when loading new scene
    {
        Reset();
    }

    public static void Register(SimpleCharacterMovement item) // Add new object, return object ID.
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

    public static SimpleCharacterMovement GetObject(int ID) // get mover associated with given ID
    {
        if (!sm_objectDictionary.ContainsKey(ID)) // if ID not found in dictionary, something's wrong
        {
            Debug.Log("Object ID not found from dictionary, ID: " + ID);
            return null;
        }
        return sm_objectDictionary[ID];
    }

    public static void InputMoveOrder(Vector3 target) // when mouse is clicked, send move order for all local player objects (should be only one)
    {
        for(int i = 0; i < sm_objects.Count; ++i)
        {
            var mover = sm_objects[i];
            if(mover.m_syncer.IsLocalPlayer())
                SyncManager.AddMoveOrder(target, mover.ID);
        }
    }

    public static void OrderMove(MoveOrder order) // pass move order to the mover it is associated with by ID
    {
        var mover = GetObject(order.m_moverID);
        mover.MoveCommand(order.m_target);
    }

    public static void OrderMoveVisualize(MoveOrder order) // pass move visualization order to the mover it is associated with by ID
	{
        var mover = GetObject(order.m_moverID);
        mover.VisualizeMove(order.m_target);
    }

    public static void RunServerTurn() // run server side game logic for all movers, eg. walk along the path determined by path finding
    {
        for (int i = 0; i < sm_objects.Count; ++i)
        {
            var mover = sm_objects[i];
            bool moved = mover.TakeStep();
            
            if (moved)
            {
                mover.m_syncer.SyncPosition(mover.m_gridPos);
                SyncManager.AddMoveVisualizationOrder(mover.m_worldPos, mover.ID);
            }
        }
    }

    private static void Reset()
    {
        sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>();
        sm_objects = new List<SimpleCharacterMovement>();
    }
}
