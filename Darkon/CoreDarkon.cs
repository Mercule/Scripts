/*
name: null
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Story/ElegyofMadness(Darkon)/CoreAstravia.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;

public class CoreDarkon
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreAdvanced Adv = new();
    private CoreAstravia Astravia => new();

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.RunCore();
    }

    public void FarmReceipt(int Quantity = 222) => FirstErrand(Quantity);

    public void FirstErrand(int Quantity = 222)
    {
        if (Core.CheckInventory("Darkon's Receipt", Quantity))
            return;

        Core.AddDrop("Darkon's Receipt");
        Core.FarmingLogger("Darkon's Receipt", Quantity);
        Core.EquipClass(ClassType.Farm);

        Core.RegisterQuests(7324);
        while (!Bot.ShouldExit && !Core.CheckInventory("Darkon's Receipt", Quantity))
            Core.HuntMonster("portalmaze", "Jurassic Monkey", "Banana", 22, false, log: false);
        Core.CancelRegisteredQuests();
    }

    public void SecondErrand(int Quantity = 222, bool escapeWhile = false)
    {
        if (Core.CheckInventory("Darkon's Receipt", Quantity))
            return;

        bool EnoughPeople = false;
        Core.AddDrop("Darkon's Receipt");
        Core.FarmingLogger("Darkon's Receipt", Quantity);
        Core.EquipClass(ClassType.Solo);
        Bot.Quests.UpdateQuest(2954);
        Core.Join("doomvault", "r5", "Left");

        Core.RegisterQuests(7325);
        while (!Bot.ShouldExit && !Core.CheckInventory("Darkon's Receipt", Quantity))
        {
            if (Bot.Map.Name.ToLower() == "doomvault")
            {
                while (!Bot.ShouldExit && Bot.Player.Cell != "r5")
                {
                    Core.Jump("r5", "Left");
                    Bot.Sleep(5000);
                }
                if (Bot.Map.PlayerCount >= 3)
                    EnoughPeople = true;
                else EnoughPeople = false;
            }

            if (!EnoughPeople && !Core.IsMember && escapeWhile)
            {
                Core.CancelRegisteredQuests();
                return;
            }

            if (!EnoughPeople && Core.IsMember)
                Core.HuntMonster("ultravoid", "Ultra Kathool", "Ingredients?", 22, false, publicRoom: true, log: false);
            else Adv.KillUltra("doomvault", "r5", "Left", "Binky", "Ingredients?", 22, false, publicRoom: true, log: false);

            Bot.Wait.ForPickup("Darkon's Receipt");
        }
        Core.CancelRegisteredQuests();
    }

    public void ThirdErrand(int Quantity = 222)
    {
        if (Core.CheckInventory("Darkon's Receipt", Quantity))
            return;

        Core.AddDrop("Darkon's Receipt");
        Core.FarmingLogger("Darkon's Receipt", Quantity);
        Core.EquipClass(ClassType.Solo);

        Core.RegisterQuests(7326);
        while (!Bot.ShouldExit && !Core.CheckInventory("Darkon's Receipt", Quantity))
        {
            Core.HuntMonster("tercessuinotlim", "Nulgath", "Nulgath's mask", 1, false, publicRoom: true);
            Bot.Wait.ForPickup("Darkon's Receipt");
        }
        Core.CancelRegisteredQuests();
    }

    public void Teeth(int Quantity = 300)
    {
        if (Core.CheckInventory("Teeth", Quantity))
            return;

        Core.AddDrop("Teeth");
        Astravia.Eridani();
        Core.FarmingLogger("Teeth", Quantity);
        Core.EquipClass(ClassType.Solo);

        Core.RegisterQuests(7780);
        while (!Bot.ShouldExit && (!Core.CheckInventory("Teeth", Quantity)))
        {
            Core.HuntMonster("eridani", "Wolf-Like Creature", "Tooth", 28, false, log: false);
            Bot.Options.AttackWithoutTarget = true;
            Core.KillMonster("eridani", "r4", "Left", "Creature 15", "Wisdom Tooth", 4, false, log: false);
            Bot.Options.AttackWithoutTarget = false;
            Bot.Wait.ForPickup("Teeth");
        }
        Core.CancelRegisteredQuests();
    }

    public void LasGratitude(int Quantity = 300)
    {
        if (Core.CheckInventory("La's Gratitude", Quantity))
            return;

        Core.AddDrop("La's Gratitude");
        Astravia.Astravia();
        Core.FarmingLogger("La's Gratitude", Quantity);
        Core.EquipClass(ClassType.Solo);

        Core.RegisterQuests(8001);
        while (!Bot.ShouldExit && (!Core.CheckInventory("La's Gratitude", Quantity)))
        {
            Core.HuntMonster("astravia", "Creature 27", "Broken Dog Tag", 20, log: false);
            Core.HuntMonster("astravia", "Creature 27", "Intact Dog Tag", 5, log: false);
            Bot.Wait.ForPickup("La's Gratitude");
        }
        Core.CancelRegisteredQuests();
    }

    public void AstravianMedal(int Quantity = 300)
    {
        if (Core.CheckInventory("Astravian Medal", Quantity))
            return;

        Core.AddDrop("Astravian Medal");
        Astravia.AstraviaCastle();
        Core.FarmingLogger("Astravian Medal", Quantity);

        Core.RegisterQuests(8257);
        while (!Bot.ShouldExit && (!Core.CheckInventory("Astravian Medal", Quantity)))
        {
            Core.EquipClass(ClassType.Farm);
            Bot.Options.AttackWithoutTarget = true;
            Core.HuntMonster("astraviacastle", "Creature 27", "Defaced Portrait", 10, log: false);
            Core.HuntMonster("astraviacastle", "Creature 20", "Smashed Sculpture", 4, log: false);
            Bot.Options.AttackWithoutTarget = false;
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("astraviacastle", "The Sun", "Burned Banana", log: false);
            Bot.Wait.ForPickup("Astravian Medal");
        }
        Core.CancelRegisteredQuests();
    }

    public void AMelody(int Quantity = 300)
    {
        if (Core.CheckInventory("A Melody", Quantity))
            return;

        Core.AddDrop("A Melody");
        Astravia.AstraviaJudgement();
        Core.FarmingLogger("A Melody", Quantity);
        Core.RegisterQuests(8396);
        while (!Bot.ShouldExit && (!Core.CheckInventory("A Melody", Quantity)))
        {
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("astraviajudge", "Trumpeter", "Brass", 10, log: false);
            Core.HuntMonster("astraviajudge", "Hand", "Sinew", 10, log: false);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("astraviajudge", "La", "Knight's Favor", log: false);
            Bot.Wait.ForPickup("A Melody");
        }
        Core.CancelRegisteredQuests();
    }

    public void BanditsCorrespondence(int Quantity = 3000)
    {
        if (Core.CheckInventory("Bandit's Correspondence", Quantity))
            return;

        Core.AddDrop("Bandit's Correspondence");
        Astravia.EridaniPast();
        Core.FarmingLogger("Bandit's Correspondence", Quantity);

        Core.RegisterQuests(8531);
        while (!Bot.ShouldExit && !Core.CheckInventory("Bandit's Correspondence", Quantity))
        {

            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("eridanipast", "Bandit", "Bandit Contraband", 12, log: false);
            Core.HuntMonster("eridanipast", "Dog", "Dogs Confiscated", 12, log: false);

            Core.EquipClass(ClassType.Solo);
            Core.HuntMonsterMapID("eridanipast", 19, "Seraphic Sparred", log: false);
            Bot.Wait.ForPickup("Bandit's Correspondence");
        }
        Core.CancelRegisteredQuests();
    }

    public void SukisPrestiege(int Quantity = 300)
    {
        if (Core.CheckInventory("Suki's Prestige", Quantity))
            return;

        Core.AddDrop("Suki's Prestige");
        Astravia.CompleteCoreAstravia();
        Core.FarmingLogger("Suki's Prestige", Quantity);

        Core.RegisterQuests(8602);
        while (!Bot.ShouldExit && (!Core.CheckInventory("Suki's Prestige", Quantity)))
        {
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("astraviapast", "Regulus", "Regulus' Rematch Won", log: false);
            Core.HuntMonster("astraviapast", "Titania", "Titania's Rematch Won", log: false);
            Core.HuntMonster("astraviapast", "Aurola", "Aurola's Rematch Won", log: false);
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("astraviapast", "Astravian Soldier", "Soldiers Trained", 8, log: false);
            Bot.Wait.ForPickup("Suki's Prestige");
        }
        Core.CancelRegisteredQuests();
    }

    public void AncientRemnant(int Quantity = 300)
    {
        if (Core.CheckInventory("Ancient Remnant", Quantity))
            return;

        Core.AddDrop("Ancient Remnant");
        Astravia.FirstObservatory();
        Core.FarmingLogger("Ancient Remnant", Quantity);

        Core.RegisterQuests(8641);
        while (!Bot.ShouldExit && !Core.CheckInventory("Ancient Remnant", Quantity))
        {
            Core.JumpWait();
            Core.EquipClass(ClassType.Farm);
            Core.KillMonster("firstobservatory", "r7", "Left", "Ancient Creature", "Creature Samples", 6);
            Core.KillMonster("firstobservatory", "r6", "Left", "Ancient Turret", "Turret Pieces", 12);
            Core.JumpWait();
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster($"firstobservatory", "Empress’ Finger", "Alprecha Observed");
            Bot.Wait.ForPickup("Ancient Remnant");
        }
        Core.CancelRegisteredQuests();
    }

    public void WheelofFortune(int FlowerQuantity = 1000, int ScaleQuantity = 1000)
    {
        if (Core.CheckInventory("Mourning Flower", FlowerQuantity) && Core.CheckInventory("Jus Divinum Scale", ScaleQuantity))
            return;

        bool shouldLog = true;
        if (FlowerQuantity > 0 && ScaleQuantity > 0)
        {
            Core.Logger($"Farming Mourning Flower ({Bot.Inventory.GetQuantity("Mourning Flower")}/{FlowerQuantity}) " +
                            $"and Jus Divinum Scale ({Bot.Inventory.GetQuantity("Jus Divinum Scale")}/{ScaleQuantity})");
            shouldLog = false;
        }

        Core.AddDrop("Mourning Flower", "Jus Divinum Scale");

        Astravia.GenesisGarden();

        Core.RegisterQuests(8688);
        while (!Bot.ShouldExit && !Core.CheckInventory("Mourning Flower", FlowerQuantity)
        || !Core.CheckInventory("Jus Divinum Scale", ScaleQuantity))
        {
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("genesisgarden", "Long-eared Beast", "Beast Subject", 7, shouldLog);
            Core.HuntMonster("genesisgarden", "Undead Humanoid", "Humanoid Subject", 7, shouldLog);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("genesisgarden", "Ancient Mecha", "Replacement Parts", 7, shouldLog);
        }
        Core.CancelRegisteredQuests();
    }

    public void UnfinishedMusicalScore(int Quantity = 300)
    {
        if (Core.CheckInventory("Unfinished Musical Score", Quantity))
            return;

        Bot.Quests.UpdateQuest(8733);
        Core.EquipClass(ClassType.Solo);

        Core.HuntMonster("theworld", "Encore Darkon", "Unfinished Musical Score", Quantity, false);
    }
}
