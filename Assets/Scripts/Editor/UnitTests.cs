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
        var inventory = testPlayer.AddComponent<TestInventory>();

        itemToAdd.AddComponent<Item>();
        
        Assert.That(inventory.CanAddItem(itemToAdd));
    }

    [Test]
    public void TestTooManyItemsPickup()
    {
        var inventory = testPlayer.AddComponent<TestInventory>();

        itemToAdd.AddComponent<Item>();

        for (int i = 0; i < inventory.m_maxItems; ++i)
        {
            inventory.AddItem(itemToAdd);
        }
        Assert.That(!inventory.CanAddItem(itemToAdd));
    }

    [Test]
    public void TestPickupGold()
    {
        var inventory = testPlayer.AddComponent<TestInventory>();
        var coin = itemToAdd.AddComponent<Item>();
        coin.m_name = "Coins";
        inventory.AddItem(itemToAdd);
        int coins = inventory.m_coins;
        Assert.AreEqual(coins, coins++);
    }

    [Test]
    public void TestPlayerTakeDamage()
    {
        var combatSystem = testPlayer.AddComponent<CombatSystem>();

        int hp = 5;

        combatSystem.m_currentHp = hp;

        combatSystem.ChangeHP(2);

        Assert.AreEqual(combatSystem.m_currentHp, hp - 2);
    }

    [Test]
    public void TestHealthpack()
    {
        var cs = testPlayer.AddComponent<CombatSystem>();
        int maxhp = 5;
        cs.m_maxHp = maxhp;
        cs.m_currentHp = maxhp-1;
        int oldhp = cs.m_currentHp;
        var healthpack = itemToAdd.AddComponent<TestHealthpack>();
        healthpack.UseHealthPack(testPlayer);
        Assert.AreEqual(cs.m_currentHp, oldhp + healthpack.m_heals);

    }

    [Test]
    public void TestEquipping()
    {
        var item = itemToAdd.AddComponent<Item>();
        item.m_vitality = 5;
        item.m_strength = 3;
        var equipment = testPlayer.AddComponent<TestEquipment>();
        var inv = testPlayer.AddComponent<TestInventory>();
        inv.m_items.Add(itemToAdd);
        equipment.EquipItem(itemToAdd, testPlayer);
        Assert.AreEqual(equipment.m_playerStrength, item.m_strength);
        Assert.AreEqual(equipment.m_playerVitality, item.m_vitality);
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
