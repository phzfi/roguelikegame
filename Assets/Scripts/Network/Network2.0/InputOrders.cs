using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

abstract public class InputOrder
{
    abstract public void ExecuteOrder();
}

public class InputConnectMessage : MessageBase
{
    public int m_playerIndex;
    public int m_connectionIndex;
}

public class InputOrderMessage : MessageBase
{
    public InputOrderMessage(InputOrder order, bool clearStack = false)
    {
        m_order = order;
        m_clearStack = clearStack;
    }

    public InputOrder m_order;
    public bool m_clearStack;
}

public class MovementInputOrder : InputOrder
{
    MovementInputOrder(Vector2i targetPosition)
    {
        m_targetGridPos = targetPosition;
    }
    private Vector2i m_targetGridPos;

    public override void ExecuteOrder()
    {
        //DummyGameLogic.Instance.ExecuteMovement();
    }
}