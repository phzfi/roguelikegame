using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HealthPack : MonoBehaviour
{

    public int m_heals = 1;

    public void UseHealthPack()
    {
        var player = CharManager.GetLocalPlayer();
        var combatSystem = player.GetComponent<CombatSystem>();
        combatSystem.m_currentHp = Mathf.Min(combatSystem.m_maxHp, combatSystem.m_currentHp + m_heals);
        Destroy(gameObject);
    }
}
