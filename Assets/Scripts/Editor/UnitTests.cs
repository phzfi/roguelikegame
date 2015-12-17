using UnityEngine;
using System.Collections;
using NUnit.Framework;
using NSubstitute;

public class UnitTests
{
    GameObject itemToAdd = GameObject.CreatePrimitive(PrimitiveType.Cube);
    GameObject testPlayer = GameObject.CreatePrimitive(PrimitiveType.Cube);

    [Test]
    public void TestInventory()
    {
        Inventory inv = new Inventory();
        Assert.That(inv != null);
    }


    [Test]
    public void TestOneItemPickup()
    {
        var inventory = testPlayer.AddComponent<Inventory>();

        itemToAdd.AddComponent<Item>();
        
        Assert.That(inventory.CanAddItem(itemToAdd));
    }

    [Test]
    public void TestTooManyItemsPickup()
    {
        var inventory = testPlayer.AddComponent<Inventory>();

        itemToAdd.AddComponent<Item>();

        for(int i = 0; i < 5; ++i)
        {
            inventory.AddItem(itemToAdd);
        }
        Assert.That(!inventory.CanAddItem(itemToAdd));
    }

    [Test]
    public void TestPlayerTakeDamage()
    {
        var combatSystem = testPlayer.AddComponent<CombatSystem>();

        int hp = 5;

        combatSystem.m_currentHp = hp;

        combatSystem.GetHit(2);

        Assert.That(combatSystem.m_currentHp == hp - 2);
    }

    [Test]
    public void TestPlayerDie()
    {
        var combatSystem = testPlayer.AddComponent<CombatSystem>();

        int hp = 5;
        combatSystem.m_currentHp = hp;
        combatSystem.GetHit(5);
        Assert.That(testPlayer.activeInHierarchy);
    }

}
