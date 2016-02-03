using UnityEngine;
using System.Collections;

// This is a dummy class that should be replaced by real game logic.
public class DummyGameLogic : Singleton<DummyGameLogic>
{
    protected DummyGameLogic() {}

    public void Inititialize() { Debug.Log("GameLogic is Initialized"); }
    public void AddPlayer() { Debug.Log("GameLogic added player"); }
    public void ReconnectPlayer(int playerId) { Debug.Log("GameLogic added player"); }
    public void ExecuteMovement() { Debug.Log("Excute movement"); }
    public void ExecuteAttack() { Debug.Log("Execute attack"); }
}
