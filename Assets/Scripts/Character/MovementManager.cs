using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MovementManager : MonoBehaviour {

    //TODO: don't use GameObject here
    public static NavGridScript sm_grid;

    private static List<SimpleCharacterMovement> sm_objects = new List<SimpleCharacterMovement>(); // Private list for iterating over all objects
    private static Dictionary<int, SimpleCharacterMovement> sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>(); // Private dictionary for fast item fetching by ID. Use GetObject to access this.
    private static int sm_curID = 0; // TODO: will not stay synchronized if client and server have different amounts of objects, eg. when not synchronizing unseen objects

    public static ReadOnlyCollection<SimpleCharacterMovement> Objects { get { return sm_objects.AsReadOnly(); } } // Public property exposes only read-only version of list to prevent modification

    public void Start()
    {
        sm_grid = NavGridScript.Instance;
    }

    public void OnLevelWasLoaded(int level) // Reset all containers and sm_curID when loading new scene
    {
        Reset();
    }

    public static void Register(SimpleCharacterMovement item, out int ID) // Add new object, return object ID.
    {
        ID = sm_curID++;

        sm_objects.Add(item);
        sm_objectDictionary.Add(ID, item);
    }

    public static void Unregister(int ID) // Remove object by ID.
    {
        var obj = GetObject(ID);
        if (obj == null)
            return;
        sm_objectDictionary.Remove(ID);
        sm_objects.Remove(obj);
    }

    public static SimpleCharacterMovement GetObject(int ID)
    {
        if (!sm_objectDictionary.ContainsKey(ID)) // if ID not found in dictionary, something's wrong
        {
            Debug.Log("Object ID not found from dictionary, ID: " + ID);
            return null;
        }
        return sm_objectDictionary[ID];
    }

    public static void InputMoveOrder(Vector3 target)
    {
        for(int i = 0; i < sm_objects.Count; ++i)
        {
            var mover = sm_objects[i];
            if(mover.m_syncer.IsLocalPlayer())
                SyncManager.AddMoveOrder(target, mover.ID);
        }
    }

    public static void OrderMove(MoveOrder order)
    {
        var mover = GetObject(order.m_moverID);
        mover.MoveCommand(order.m_target);
    }

    public static void OrderMoveVisualize(MoveOrder order)
    {
        var mover = GetObject(order.m_moverID);
        mover.VisualizeMove(order.m_target);
    }

    public static void RunServerTurn()
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
        sm_curID = 0;
        sm_objectDictionary = new Dictionary<int, SimpleCharacterMovement>();
        sm_objects = new List<SimpleCharacterMovement>();
    }
}
