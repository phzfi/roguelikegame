using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public interface IOutputOrder
{
    void ExecuteOrder(ClientSyncManager manager);
    void SetTurnQueuePosition(int index);
    networkingMsgType GetMessageType();
}


public class OutputConnectMessage : MessageBase
{
    public Vector2i m_mapSize = new Vector2i();
}

public class OutputMessage : MessageBase
{
    protected void SetInterfaceToOrders<T>(List<IOutputOrder> list, ref T[] array)
    {
        array = new T[list.Count];
        for (int i = 0; i < list.Count; ++i)
            array[i] = (T)list[i];
    }

    public void SetOrdersToInterface<T>(ref List<IOutputOrder> list, T[] array)
    {
        for (int i = 0; i < array.Length; ++i)
            list.Add((IOutputOrder)array[i]);
    }
}

public class VisibilityOutputMessage : OutputMessage
{
    public void SetOrders(List<IOutputOrder> inputs)
    {
        SetInterfaceToOrders<VisibilityOutputOrder>(inputs, ref m_orders);
    }

    public VisibilityOutputOrder[] m_orders;
}

[Serializable]
public struct VisibilityOutputOrder : IOutputOrder
{
    public void ExecuteOrder(ClientSyncManager manager)
    {
        manager.ExecuteVisibilityOrder(this);
    }

    public void SetTurnQueuePosition(int index)
    {
        m_turnQueuePosition = index;
    }

    public networkingMsgType GetMessageType()
    {
        return networkingMsgType.visibilityOutputMessage;
    }

    public void SetData(int numberOfTiles, 
        List<Vector2i> gridPositions,
        List<int> tileTypes,
        List<int> visualizationIndices)
    {
        Debug.Assert(gridPositions.Count == numberOfTiles);
        Debug.Assert(tileTypes.Count == numberOfTiles);
        Debug.Assert(visualizationIndices.Count == numberOfTiles);

        m_numberOfTiles = numberOfTiles;
        m_gridPostions = gridPositions.ToArray();
        m_mapTiles = tileTypes.ToArray();
        m_visualizationIndices = visualizationIndices.ToArray();
    }

    [SerializeField]
    public int m_turnQueuePosition;
    [SerializeField]
    public int m_numberOfTiles;
    [SerializeField]
    public Vector2i[] m_gridPostions;
    [SerializeField]
    public int[] m_mapTiles;
    [SerializeField]
    public int[] m_visualizationIndices;
}
