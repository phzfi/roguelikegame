using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Equipment : NetworkBehaviour
{
    
    public List<GameObject> m_equipment;

    [SyncVar]
    public int m_playerStrength = 0;
    [SyncVar]
    public int m_playerAgility = 0;
    [SyncVar]
    public int m_playerIntelligence = 0;
    [SyncVar]
    public int m_playerVitality = 0;

    void Start()
    {
        m_equipment = new List<GameObject>();
    }

	public RangedAttack GetRangedAttack()
	{
		for(int i = 0; i < m_equipment.Count; ++i)
		{
			var item = m_equipment[i].GetComponent<RangedAttack>();
			if (item != null)
				return item;
		}
		return null;
	}

}
