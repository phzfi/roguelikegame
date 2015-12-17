using UnityEngine;
using System.Collections;
using NUnit.Framework;
using NSubstitute;

public class UnitTests
{
    GameObject itemToAdd = GameObject.CreatePrimitive(PrimitiveType.Cube);
    GameObject testPlayer = GameObject.CreatePrimitive(PrimitiveType.Cube);


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
    public void TestMultiplyVector2i()
    {
        Vector2i a = new Vector2i(5, 3);
        Vector2i b = new Vector2i(-2, 4);

        Assert.That(a * b == new Vector2i(-10, 12));

    }

    [Test]
    public void TestSumVector2i()
    {
        Vector2i a = new Vector2i(5, 3);
        Vector2i b = new Vector2i(-2, 4);

        Assert.That(a + b == new Vector2i(3, 7));
    }

    [Test]
    public void TestDifferenceVector2i()
    {
        Vector2i a = new Vector2i(5, 3);
        Vector2i b = new Vector2i(-2, 4);

        Assert.That(a - b == new Vector2i(7, -1));
    }

}
