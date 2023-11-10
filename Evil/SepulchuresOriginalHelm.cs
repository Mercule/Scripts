/*
name: SepulchuresOriginalHelm
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/Story/LordsofChaos/Core13LoC.cs
//cs_include Scripts/Story/Doomwood/CoreDoomwood.cs
//cs_include Scripts/Story/ThroneofDarkness/CoreToD.cs

using Skua.Core.Interfaces;

public class SepulchuresOriginalHelm
{
    public IScriptInterface Bot => IScriptInterface.Instance;

    public CoreBots Core => CoreBots.Instance;
    public CoreAdvanced Adv = new();
    public CoreStory Story = new();
    public CoreFarms Farm = new();
    public CoreDailies Daily = new();
    public Core13LoC LOC = new();
    public CoreToD TOD = new();
    public CoreDoomwood DW = new();
    public string[] GravelynsDoomFireTokenItems = { "Empowered Essence", "Gravelyn's Blessing", "Painful Memory Bubble", "Burning Passion Flame", "Father's Sorrowful Tear", "Gravelyn's DoomFire Token", "Necrotic Sword of Doom", "Sepulchure's DoomKnight Armor" };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(new[] { "Necrotic Sword of Doom", "Sepulchure's DoomKnight Armor" });
        Core.SetOptions();

        DoAll();

        Core.SetOptions(false);
    }

    public void DoAll()
    {
        if (Core.CheckInventory("Sepulchure's Original Helm"))
            return;

        Story.PreLoad(this);

        Core.AddDrop("Sepulchure's Original Helm");
        if (Core.CheckInventory("Sepulchure's DoomKnight Armor"))
        {
            Core.BuyItem("shadowfall", 1642, "Sepulchure's Original Helm");
            return;
        }

        Core.ChangeAlignment(Alignment.Evil);
        Farm.EvilREP(10);
        Farm.Experience(70);
        DW.AQWZombies();
        TOD.MysteriousDungeon();
        LOC.Hero();
        if (!Core.CheckInventory(new[] { "Lore's Champion Seal", "Gravelyn's DoomFire Token", "Royal ShadowScythe Blade" }))
        {
            GravelynsDoomFireToken();
            RoyalShadowScytheBlade();
            Core.BuyItem(Bot.Map.Name, 993, "Lore's Champion Seal");
        }
        Core.ChainComplete(6555);
        Bot.Wait.ForPickup("Sepulchure's Original Helm");
        Core.SellItem("Royal ShadowScythe Blade");
    }

    public void GravelynsDoomFireToken()
    {
        if (Core.CheckInventory(37033))
            return;

        Core.AddDrop(GravelynsDoomFireTokenItems);

        while (!Bot.ShouldExit && !Core.CheckInventory(37033))
        {
            Core.EnsureAccept(5461);
            // Check for "Gravelyn's Blessing"
            if (!Core.CheckInventory(37034))
            {
                if (Core.CheckInventory("Necrotic Sword of Doom"))
                    Core.ChainComplete(5455);
                else if (Core.CheckInventory("Sepulchure's DoomKnight Armor"))
                    Core.ChainComplete(5456);
                else
                {
                    // Handle the case when none of the above conditions match
                    Farm.EvilREP(10);
                    Core.EnsureAccept(5457);
                    while (!Bot.ShouldExit && !Core.CheckInventory(37039))
                        Core.HuntMonsterMapID("necrodungeon", 9);
                    Core.EnsureComplete(5457);
                    Bot.Wait.ForPickup(37034);
                }
            }

            // Check for "Painful Memory Bubble"
            if (!Core.CheckInventory("Painful Memory Bubble"))
            {
                Core.EnsureAccept(5458);
                Core.KillMonster("swordhavenfalls", "r10", "Left", 1295, "Doomed Memories");
                Core.EnsureComplete(5458);
                Bot.Wait.ForPickup("Painful Memory Bubble");
            }

            // Check for "Burning Passion Flame"
            if (!Core.CheckInventory("Burning Passion Flame"))
            {
                Core.EnsureAccept(5459);
                Core.HuntMonster("shadowstrike", "Sepulchuroth", "Sepulchuroth's Undying Flame");
                Core.EnsureComplete(5459);
                Bot.Wait.ForPickup("Burning Passion Flame");
            }

            // Check for "Father's Sorrowful Tear"
            if (!Core.CheckInventory("Father's Sorrowful Tear"))
            {
                Core.EnsureAccept(5460);
                Core.HuntMonster("Shadowfall", "Shadow of the Past", "Father's Anger");
                Core.EnsureComplete(5460);
                Bot.Wait.ForPickup("Father's Sorrowful Tear");
            }

            // Kill monsters for "Empowered Essence"
            Core.KillMonster("shadowrealmpast", "Enter", "Spawn", "*", "Empowered Essence", 13, isTemp: false);

            Core.EnsureComplete(5461);
            Bot.Wait.ForPickup(37033);
        }
    }

    public void RoyalShadowScytheBlade()
    {
        if (Core.CheckInventory("Royal ShadowScythe Blade"))
            return;

        Farm.Gold(1000000);
        Farm.EvilREP(10);
        Core.BuyItem("shadowfall", 1639, "Royal ShadowScythe Blade", shopItemID: 26666);
    }

}
