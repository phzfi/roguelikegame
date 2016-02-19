using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public interface IInputOrder
{
    void ExecuteOrder(GameLogic logic, int playerIndex);
}


public class InputConnectMessage : MessageBase
{
    public int m_playerIndex;
    public int m_connectionIndex;
}

public class InputMessage : MessageBase
{
    protected void SetInterfaceToOrders<T>(List<IInputOrder> list, ref T[] array)
    {
        array = new T[list.Count];
        for (int i = 0; i < list.Count; ++i)
            array[i] = (T)list[i];
    }

    public void SetOrdersToInterface<T>(ref List<IInputOrder> list, T[] array)
    {
        for (int i = 0; i < array.Length; ++i)
            list.Add((IInputOrder)array[i]);
    }
}


public class MovementInputMessage : InputMessage
{
    public void SetOrders(List<IInputOrder> inputs)
    {
        SetInterfaceToOrders<MovementInputOrder>(inputs, ref m_orders);
    }

    public MovementInputOrder[] m_orders;
}


public struct MovementInputOrder : IInputOrder
{
    public void ExecuteOrder(GameLogic logic, int playerIndex)
    {
        logic.PlayerMovement(this, playerIndex);
    }

    public void SetData(Vector2i targetPosition)
    {
        m_targetGridPos = targetPosition;
    }

    [SerializeField]
    private Vector2i m_targetGridPos;
}
