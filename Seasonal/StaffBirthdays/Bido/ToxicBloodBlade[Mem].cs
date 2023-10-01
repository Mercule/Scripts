/*
name: Toxic Blood Blade (Mem)
description: This script will complete the quest for Toxic Blood Blade.
tags: nation,bido,birthday,seasonal,another science experiment
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Nation/CoreNation.cs
using Skua.Core.Interfaces;

public class ToxicBloodBlade
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new();
    public CoreStory Story = new();
    public CoreAdvanced Adv = new();
    public CoreNation Nation = new();


    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        GetBlade();

        Core.SetOptions(false);
    }


    public void GetBlade()
    {
        if (Core.CheckInventory(55688) || !Core.IsMember)
            return;

        Core.EnsureAccept(7602);

        Nation.FarmUni13(2);
        Nation.SwindleBulk(100);
        Nation.FarmDarkCrystalShard(150);
        Nation.FarmTotemofNulgath(3);
        Nation.FarmBloodGem(40);

        Core.AddDrop("Concentrated Mana", "Bido's Appreciation");
        Core.AddDrop(49085, 55688);

        //Concentrated Mana x30
        Core.RegisterQuests(6979);
        Core.EquipClass(ClassType.Solo);
        while (!Bot.ShouldExit && !Core.CheckInventory("Concentrated Mana", 30) || !Core.CheckInventory(49085, 30))
            Core.HuntMonster("prison", "Piggy Drake", "Broken Piggy Bank", log: false);


        //Bido's Appreciation x50
        Core.EquipClass(ClassType.Solo);
        while (!Bot.ShouldExit && !Core.CheckInventory("Bido's Appreciation", 50))
        {
            Core.EnsureAccept(6980);
            Core.HuntMonster("well", "Gell Oh No", "Piece of Gell Oh No Perfectly Slushied", log: false);
            Core.HuntMonster("ashfallcamp", "Smoldur", "Smoldur's Shedded Scales", 4, log: false);
            Core.EnsureComplete(6980);
            Bot.Wait.ForPickup("Bido's Appreciation");
        }

        Core.EnsureComplete(7602);
        Bot.Wait.ForPickup(55688);
        Core.ToBank(49085);
        Core.ToBank("Concentrated Mana", "Bido's Appreciation");

    }
}
