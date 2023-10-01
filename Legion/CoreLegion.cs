/*
name: null
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
using Skua.Core.Interfaces;
using Skua.Core.Models;

public class CoreLegion
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreFarms Farm = new();
    private CoreStory Story = new();
    private CoreAdvanced Adv = new();

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.RunCore();
    }

    public string[] legionMedals =
    {
        "Legion Round 1 Medal",
        "Legion Round 2 Medal",
        "Legion Round 3 Medal",
        "Legion Round 4 Medal"
    };

    public void EmblemofDage(int quant = 500)
    {
        if (Core.CheckInventory("Emblem of Dage", quant))
            return;

        if (!Core.CheckInventory("Legion Round 4 Medal"))
            LegionRound4Medal();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.gold);

        Core.FarmingLogger("Emblem of Dage", quant);
        Core.AddDrop("Legion Seal", "Gem of Mastery", "Emblem of Dage");
        Core.RegisterQuests(4742);
        while (!Bot.ShouldExit && !Core.CheckInventory("Emblem of Dage", quant))
        {
            Core.HuntMonster("shadowblast", "Carnage", "Gem of Mastery", 1, false, false);
            Core.KillMonster("shadowblast", "r10", "Left", "*", "Legion Seal", 25, false);
            Bot.Wait.ForPickup("Emblem of Dage");
        }
        Core.CancelRegisteredQuests();
    }

    public void DarkToken(int quant = 600)
    {
        if (Core.CheckInventory("Dark Token", quant))
            return;

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Human);

        Core.AddDrop("Dark Token");
        Core.FarmingLogger("Dark Token", quant);
        Core.RegisterQuests(6248, 6249, 6251);
        while (!Bot.ShouldExit && !Core.CheckInventory("Dark Token", quant))
        {
            Core.HuntMonster("seraphicwardage", "Seraphic Commander", log: false);
        }
        Core.CancelRegisteredQuests();
    }

    public void DiamondTokenofDage(int quant = 300)
    {
        if (Core.CheckInventory("Diamond Token of Dage", quant))
            return;

        if (!Core.CheckInventory("Legion Round 4 Medal"))
            LegionRound4Medal();
        if (!Core.CheckInventory("Legion Token", 50))
            FarmLegionToken(50);

        Core.FarmingLogger("Diamond Token of Dage", quant);
        Core.AddDrop("Diamond Token of Dage", "Legion Token");
        Core.RegisterQuests(4743);
        while (!Bot.ShouldExit && !Core.CheckInventory("Diamond Token of Dage", quant))
        {
            if (!Core.CheckInventory("Defeated Makai", 25))
            {
                Core.EquipClass(ClassType.Farm);
                Core.KillMonster("tercessuinotlim", "m2", "Bottom", "Dark Makai", "Defeated Makai", 25, false);
                Core.Jump();
                Core.Join("aqlesson");
            }
            Core.EquipClass(ClassType.Solo);
            //Adv.BestGear(RacialGearBoost.Chaos);
            Core.KillMonster("aqlesson", "Frame9", "Right", "Carnax", "Carnax Eye", publicRoom: true);
            Core.HuntMonster("deepchaos", "Kathool", "Kathool Tentacle", publicRoom: true);

            //More then one item of the same name as drop btoh temp and non-temp.
            while (!Bot.ShouldExit && !Core.CheckInventory(33257))
                Core.KillMonster("dflesson", "r12", "Right", "Fluffy the Dracolich", publicRoom: true);

            //Adv.BestGear(RacialGearBoost.Dragonkin);
            Core.HuntMonster("lair", "Red Dragon", "Red Dragon's Fang");
            //Adv.BestGear(RacialGearBoost.Human);
            Core.HuntMonster("bloodtitan", "Blood Titan", "Blood Titan's Blade", publicRoom: true);
            Bot.Drops.Pickup("Legion Token", "Diamond Token of Dage");
        }
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Farms Legion Round 4 Medal in Shadow Blast Arena
    /// </summary>
    public void LegionRound4Medal()
    {
        if (Core.CheckInventory("Legion Round 4 Medal"))
            return;

        Core.AddDrop(legionMedals);
        Core.Logger("Farming Legion Round 4 Medal");
        Core.Join("shadowblast");
        //Adv.BestGear(GenericGearBoost.dmgAll);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Round 4 Medal"))
        {
            if (!Core.CheckInventory("Legion Round 1 Medal") &&
                !Core.CheckInventory("Legion Round 2 Medal") &&
                !Core.CheckInventory("Legion Round 3 Medal"))
            {
                Core.EnsureAccept(4738);
                Core.HuntMonster("shadowblast", "Caesaristhedark", "Nation Rookie Defeated", 5, true);
                Core.HuntMonster("shadowblast", "Shadowrise Guard", "Shadowscythe Rookie Defeated", 5, true);
                Core.EnsureComplete(4738);
                Bot.Wait.ForDrop("Legion Round 1 Medal");
                Core.Logger("Medal 1 acquired");
            }

            if (Core.CheckInventory("Legion Round 1 Medal"))
            {
                Core.EnsureAccept(4739);
                Core.HuntMonster("shadowblast", "Carnage", "Nation Veteran Defeated", 7, true);
                Core.HuntMonster("shadowblast", "Doombringer", "Shadowscythe Veteran Defeated", 7, true);
                Core.EnsureComplete(4739);
                Bot.Wait.ForDrop("Legion Round 2 Medal");
                Core.Logger("Medal 2 acquired");
            }

            if (Core.CheckInventory("Legion Round 2 Medal"))
            {
                Core.EnsureAccept(4740);
                Core.HuntMonster("shadowblast", "Minotaurofwar", "Nation Elite Defeated", 10, true);
                Core.HuntMonster("shadowblast", "Draconic Doomknight", "Shadowscythe Elite Defeated", 10, true);
                Core.EnsureComplete(4740);
                Bot.Wait.ForDrop("Legion Round 3 Medal");
                Core.Logger("Medal 3 acquired");
            }

            if (Core.CheckInventory("Legion Round 3 Medal"))
            {
                Core.EnsureAccept(4741);
                Core.HuntMonster("shadowblast", "Thanatos", "Thanatos Vanquished", 1, true);
                Core.EnsureComplete(4741);
                Bot.Wait.ForDrop("Legion Round 4 Medal");
                Core.Logger("Medal 4 acquired");
            }
        }
    }

    public void ApprovalAndFavor(int quantApproval = 5000, int quantFavor = 5000)
    {
        if (Core.CheckInventory("Dage's Approval", quantApproval) && Core.CheckInventory("Dage's Favor", quantFavor))
            return;

        Core.AddDrop("Dage's Approval", "Dage's Favor");

        bool shouldLog = true;
        if (quantApproval > 0 && quantFavor > 0)
        {
            Core.Logger($"Farming Dage's Approval ({Bot.Inventory.GetQuantity("Dage's Approval")}/{quantApproval}) " +
                            $"and Dage's Favor ({Bot.Inventory.GetQuantity("Dage's Favor")}/{quantFavor})");
            shouldLog = false;
        }

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Undead);

        Core.KillMonster("evilwardage", "r8", "Left", "*", "Dage's Approval", quantApproval, false, shouldLog);
        Core.KillMonster("evilwardage", "r8", "Left", "*", "Dage's Favor", quantFavor, false, shouldLog);
    }

    public void BoneSigil(int quant = 1)
    {
        if (Core.CheckInventory("Bone Sigil", quant))
            return;

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Undead);

        Core.FarmingLogger("Bone Sigil", quant);
        Core.AddDrop("Bone Sigil");
        Core.RegisterQuests(6739);
        while (!Bot.ShouldExit && !Core.CheckInventory("Bone Sigil", quant))
        {
            Core.HuntMonster("legionarena", "Legion Gladiator", "Legion Grunt Defeated", 5);
            Bot.Wait.ForPickup("Bone Sigil");
        }
        Core.CancelRegisteredQuests();
    }

    public void SoulForgeHammer()
    {
        if (Core.CheckInventory("SoulForge Hammer"))
            return;

        Core.EquipClass(ClassType.Solo);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.AddDrop("SoulForge Hammer");
        Core.EnsureAccept(2741);
        Core.HuntMonster("forest", "Zardman Grunt", "Zardman's StoneHammer", isTemp: false);
        if (Core.CheckInventory(319))
            Core.BuyItem("swordhaven", 179, "Iron Hammer");
        else Core.HuntMonster("battleundera", "Skeletal Warrior", "Iron Hammer", isTemp: false);
        Core.HuntMonster("bludrut", "Rock Elemental", "Elemental Rock Hammer", isTemp: false);
        Core.EnsureComplete(2741);
        Bot.Wait.ForPickup("SoulForge Hammer");
    }

    #region LegionTokens
    public void FarmLegionToken(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant))
            return;

        JoinLegion();

        LTBrightParagon(quant);
        LTArcaneParagon(quant);
        LTShogunParagon(quant);
        LTMountedParagonPet(quant);
        LTThanatosParagon(quant);
        LTAscendedParagon(quant);
        LTDreadnaughtParagon(quant);
        LTFestiveParagonDracolichRider(quant);
        LTHolidayParagon(quant);
        LTHardCoreParagon(quant);
        LTUW3017(quant);
        LTInfernalLegionBetrayal(quant);
        LTFirstClassEntertainment(quant, true, 4, true);
        LTDreadrock(quant);
    }

    public void LTHardCoreParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Hardcore Paragon Pet"))
            return;
        Core.BankingBlackList.Add("Legion Token");
        Core.EquipClass(ClassType.Solo);
        //Adv.BestGear(RacialGearBoost.Chaos);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop(Core.QuestRewards(3393, 3394));

        if (!Bot.Quests.IsDailyComplete(3394))
        {
            Core.EnsureAccept(3394);
            Core.HuntMonster("chaosboss", "Ultra Chaos Warlord", "Chaorrupted Dark Fire", 20, isTemp: false);
            Core.EnsureComplete(3394);
        }

        Core.RegisterQuests(3393);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
            Adv.BoostHuntMonster("doomvault", "Binky", "Dark Unicorn Rib", isTemp: false, log: false);
        Core.CancelRegisteredQuests();
        Core.ToBank(Core.QuestRewards(3393, 3394));

    }

    public void LTInfernalLegionBetrayal(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Infernal Caladbolg"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");

        Core.RegisterQuests(Core.CheckInventory("Shogun Paragon Pet") ? new[] { 3722, 5755 } : new[] { 3722 });
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("fotia", "*", "Betrayer Extinguished", 5);
            Core.HuntMonster("evilwardage", "Dreadfiend of Nulgath", "Fiend Felled", 2);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTUW3017(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("UW3017 Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(5738);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("underworld", "Bloodfiend", "Foreign Weapon", 20);
            Core.HuntMonster("underworld", "Bloodfiend", "Foreign Equipment", 20);
            Core.HuntMonster("underworld", "Bloodfiend", "Unknown Substance", 20);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTHolidayParagon(int quant = 25000)
    {

        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Holiday Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(3256);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("prison", "King Alteon's Knight", "Spirit of Loyalty", 6);
            Core.HuntMonster("battlewedding", "Silver Knight", "Spirit of Love", 6);
            Core.HuntMonster("lycan", "Lycan Knight", "Spirit of Good Will", 6);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTFestiveParagonDracolichRider(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Festive Paragon Dracolich Rider"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(3968, 3969);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("frozenruins", "Frost Fangbeast", "Frost Heart", 10);
            Core.HuntMonster("frozenruins", "Frost Fangbeast", "Blanket", 6);
            Core.HuntMonster("frozenruins", "Frost Fangbeast", "Light", 6);
            Core.HuntMonster("frozenruins", "Frost Fangbeast", "Pail of Water", 6);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTBrightParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Bright Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token", "Legion Token Pile");
        Core.RegisterQuests(4704, 4703);
        Core.ConfigureAggro();
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.KillMonster("brightfortress", "r3", "Right", "*", "Badge of Loyalty", 10);
            Core.KillMonster("brightfortress", "r3", "Right", "*", "Badge of Corruption", 8);
            Core.KillMonster("brightfortress", "r3", "Right", "*", "Twisted Light Token", 6);
        }
        Core.ConfigureAggro(false);
        Core.CancelRegisteredQuests();
    }

    public void LTShogunParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant)
            || (!Core.CheckInventory("Shogun Paragon Pet") && !Core.CheckInventory("Paragon Fiend Quest Pet") && !Core.CheckInventory("Paragon Ringbearer") && !Core.CheckInventory("Shogun Dage Pet")))
            return;

        JoinLegion();

        int QuestID = 0;
        if (Core.CheckInventory("Shogun Paragon Pet"))
            QuestID = 5755;
        else if (Core.CheckInventory("Shogun Dage Pet"))
            QuestID = 5756;
        else if (Core.CheckInventory("Paragon Fiend Quest Pet"))
        {
            if (Bot.Inventory.GetItem("Paragon Fiend Quest Pet")!.ID == 47578)
                QuestID = 6750;
            else if (Bot.Inventory.GetItem("Paragon Fiend Quest Pet")!.ID == 47614)
                QuestID = 6756;
        }
        else if (Core.CheckInventory("Paragon Ringbearer"))
            QuestID = 7073;

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(QuestID);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
            Core.HuntMonster("fotia", "*", log: false);
        Core.CancelRegisteredQuests();
    }

    public void LTFirstClassEntertainment(int quant = 25000, bool onlyWithParty = false, int partySize = 4, bool ReturnIfNoPeople = false)
    {
        if (Core.CheckInventory("Legion Token", quant))
            return;

        JoinLegion();

        Core.Join("legionarena", publicRoom: true);
        if (Bot.Map.PlayerCount < partySize && onlyWithParty)
        {
            Core.Join("legionarena", ignoreCheck: true, publicRoom: true);
            if (ReturnIfNoPeople && Bot.Map.PlayerCount < partySize)
                return;
            while (!Bot.ShouldExit && Bot.Map.PlayerCount < partySize) { }
            Core.Logger($"Party gathered [{Bot.Map.PlayerNames!.Count}/{partySize}]");
        }

        Core.EquipClass(ClassType.Solo);
        //Adv.BestGear(RacialGearBoost.Undead);

        Core.FarmingLogger("Legion Token", quant);
        Core.RegisterQuests(6742, 6743);
        Core.AddDrop("Legion Token", "Bone Sigil");
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
            Core.KillMonster("legionarena", "Boss", "Left", "Legion Fiend Rider", log: false);
        Core.CancelRegisteredQuests();
        Core.ToBank("Bone Sigil");
    }

    public void LTDreadrock(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant))
            return;

        JoinLegion();
        Adv.BuyItem("underworld", 216, "Undead Champion");

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Human);

        Core.FarmingLogger("Legion Token", quant);
        Core.RegisterQuests(4849);
        Core.AddDrop("Legion Token");
        Core.ConfigureAggro();
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
            Core.KillMonster("dreadrock", "r3", "Bottom", "*", "Dreadrock Enemy Recruited", 6, log: false);
        Core.ConfigureAggro(false);
        Core.CancelRegisteredQuests();
    }

    public void LTArcaneParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Arcane Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Dragonkin);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token", "Granite Dracolich Soul", "Tempest Dracolich Soul", "Deluge Dracolich Soul", "Inferno Dracolich Soul");
        Core.RegisterQuests(4896);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("dragonheart", "Granite Dracolich", "Granite Dracolich Soul", 4, isTemp: false);
            Core.HuntMonster("dragonheart", "Tempest Dracolich", "Tempest Dracolich Soul", 4, isTemp: false);
            Core.HuntMonster("dragonheart", "Inferno Dracolich", "Inferno Dracolich Soul", 4, isTemp: false);
            Core.HuntMonster("dragonheart", "Deluge Dracolich", "Deluge Dracolich Soul", 4, isTemp: false);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTThanatosParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Thanatos Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(RacialGearBoost.Undead);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(4100);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
            Core.KillMonster("dragonheart", "r6", "Right", "Zombie Dragon", "Elemental Dragon Soul", 20);
        Core.CancelRegisteredQuests();
    }

    public void LTDreadnaughtParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Paragon Dreadnaught Pet"))
            return;

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(5741);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("laken", "Augmented Guard", "Stolen Guard", 5);
            Core.HuntMonster("laken", "Cyborg Dog", "Stolen Dog", 6);
            Core.HuntMonster("laken", "Mad Scientist", "Taken Axe", 10);
        }
        Core.CancelRegisteredQuests();
    }

    public void LTMountedParagonPet(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Mounted Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(5604);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("frozentower", "Ice Wolf", "Giant Coal Lump", 10);
            Core.HuntMonster("frozentower", "Ice Wolf", "Small Coal Lump", 8);
        }
        Core.CancelRegisteredQuests();

    }

    public void LTAscendedParagon(int quant = 25000)
    {
        if (Core.CheckInventory("Legion Token", quant) || !Core.CheckInventory("Ascended Paragon Pet"))
            return;

        JoinLegion();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Legion Token", quant);
        Core.AddDrop("Legion Token");
        Core.RegisterQuests(2747);
        while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
        {
            Core.HuntMonster("tournament", "Lord Brentan", "Lord Brentan's Regal Blade");
            Core.HuntMonster("tournament", "Roderick", "Roderick's Chaotic Bane");
            Core.HuntMonster("tournament", "Knight of Thorns", "Knight of Thorns' Sword");
            Core.HuntMonster("tournament", "Johann Wryce", "Platinum of Johann Wryce");
            Core.HuntMonster("tournament", "Khai Khaldun", "Khai Khaldun's Scimitar");
        }
        Core.CancelRegisteredQuests();
    }

    #endregion

    public void JoinLegion()
    {
        if (Core.isCompletedBefore(793))
            return;

        if (!Core.isCompletedBefore(792))
            Farm.BludrutBrawlBoss(quant: 200);

        Core.BuyItem("underworld", 215, "Undead Warrior");
        var SellUW = Bot.ShowMessageBox(
            "Do you want the bot to sell the \"Undead Warrior\" armor after it has succesfully joined the legion. This will return 1080 AC to you",
            "Sell \"Undead Warrior\"?",
            true);


        // Undead Champion Initiation
        if (!Story.QuestProgression(789))
        {
            Core.EnsureAccept(789);
            Core.HuntMonster("greenguardwest", "Black Knight", "Black Knight's Eternal Contract", isTemp: false, log: false);
            Core.EnsureComplete(789);
        }

        // Mourn the Soldiers
        if (!Story.QuestProgression(790))
        {
            Core.EnsureAccept(790);
            Core.HuntMonster("dwarfhold", "Chaos Drow", "Chaos Drow slain");
            Core.HuntMonster("swordhavenundead", "Skeletal Soldier", "Skeletal Soldier slain");
            Core.HuntMonster("pirates", "Fishman Soldier", "Fishman Soldier slain");
            Core.HuntMonster("willowcreek", "Dwakel Soldier", "Dwakel Soldier slain");
            Core.EnsureComplete(790);
        }

        // Understanding Undead Champions
        if (!Story.QuestProgression(791))
        {
            Core.EnsureAccept(791);
            Core.HuntMonster("battleunderb", "Undead Champion", "Ravaged Champion Soul", 80, isTemp: false, log: false);
            Core.EnsureComplete(791);
        }

        // Player vs Power
        Story.ChainQuest(792);

        // Fail to the King
        Story.KillQuest(793, "prison", "King Alteon's Knight");

        if (SellUW == true)
            Core.SellItem("Undead Warrior", all: true);
        Adv.BuyItem("underworld", 216, "Undead Champion");
    }

    public void ObsidianRock(int quant = 10)
    {
        if (Core.CheckInventory("Obsidian Rock", quant))
            return;

        SoulForgeHammer();

        Core.EquipClass(ClassType.Farm);
        //Adv.BestGear(GenericGearBoost.dmgAll);

        Core.FarmingLogger("Obsidian Rock", quant);
        Core.AddDrop("Obsidian Rock");
        if (!Core.IsMember)
        {
            Core.Logger("Using Non-Member Method");
            Bot.Quests.UpdateQuest(1542);
        }
        else Core.Logger("Using Members Method");

        Core.RegisterQuests(2742);
        while (!Bot.ShouldExit && !Core.CheckInventory("Obsidian Rock", quant))
        {
            if (Core.IsMember)
                Core.HuntMonster("hydra", "Fire Imp", "Obsidian Deposit", 10, log: false);
            else Core.KillMonster("firestorm", "r8", "Left", "Firestorm Hatchling", "Obsidian Deposit", 10, log: false);

            Bot.Wait.ForPickup("Obsidian Rock");
        }
        Core.CancelRegisteredQuests();
    }

    public void DagePvP(int TrophyQuant, int TechniqueQuant, int ScrollQuant, bool canSoloBoss = true)
    {
        if (Core.CheckInventory("Legion Combat Trophy", TrophyQuant) &&
            Core.CheckInventory("Technique Observed", TechniqueQuant) &&
            Core.CheckInventory("Sword Scroll Fragment", ScrollQuant))
            return;

        if (Core.CBOBool("PvP_SoloPvPBoss", out bool _canSoloBoss))
            canSoloBoss = !_canSoloBoss;

        // Bot.Options.RestPackets = true;
        // Bot.Events.PlayerDeath += PVPDeath;

        Core.AddDrop("Legion Combat Trophy", "Technique Observed", "Sword Scroll Fragment");
        Core.EquipClass(ClassType.Solo);
        // Core.Logger($"Farming {quant} {item}. SoloBoss = {canSoloBoss}");

        while (!Bot.ShouldExit &&
                !Core.CheckInventory("Legion Combat Trophy", TrophyQuant) ||
                !Core.CheckInventory("Technique Observed", TechniqueQuant) ||
                !Core.CheckInventory("Sword Scroll Fragment", ScrollQuant))
        {
            if (TrophyQuant > 0)
                Core.Logger($"Trophy: {Bot.Inventory.GetQuantity("Legion Combat Trophy")} / {TrophyQuant}");
            if (TechniqueQuant > 0)
                Core.Logger($"Technique: {Bot.Inventory.GetQuantity("Technique Observed")} / {TechniqueQuant}");
            if (ScrollQuant > 0)
                Core.Logger($"Fragment: {Bot.Inventory.GetQuantity("Sword Scroll Fragment")} / {ScrollQuant}");

            Core.Join("dagepvp", "Enter0", "Spawn", ignoreCheck: true);
            Bot.Sleep(2500);

            Core.PvPMove(1, "r2", 475, 269);
            Core.PvPMove(4, "r4", 963, 351);
            Core.PvPMove(7, "r5", 849, 177);
            Core.PvPMove(9, "r6", 937, 389);

            if (!Core.CheckInventory("Sword Scroll Fragment", ScrollQuant))
            {
                Core.PvPMove(11, "r7", 513, 286);
                Core.PvPMove(15, "r10", 832, 347);

                Core.FarmingLogger("Sword Scroll Fragment", ScrollQuant);

                while (!Bot.ShouldExit)
                {
                    Bot.Kill.Monster(17);
                    Bot.Wait.ForCombatExit();
                    Bot.Kill.Monster(18);
                    Bot.Wait.ForCombatExit();
                    if (!Core.IsMonsterAlive("Blade Master"))
                    {
                        Bot.Combat.CancelTarget();
                        Core.Logger("Blade Masters killed, moving on.");
                        break;
                    }
                }

                Core.PvPMove(20, "r11", 943, 391);

                while (!Bot.ShouldExit)
                {
                    Bot.Kill.Monster(19);
                    Bot.Wait.ForCombatExit();
                    Bot.Kill.Monster(20);
                    Bot.Wait.ForCombatExit();
                    if (!Core.IsMonsterAlive("Blade Master"))
                    {
                        Bot.Combat.CancelTarget();
                        Core.Logger("Blade Masters killed, moving on.");
                        break;
                    }
                }

                Core.PvPMove(21, "r10", 9, 397);
                Core.PvPMove(19, "r7", 7, 392);
                Core.PvPMove(14, "r6", 482, 483);
            }

            Core.PvPMove(12, "r12", 758, 338);

            while (!Bot.ShouldExit && !canSoloBoss)
            {
                Bot.Kill.Monster(21);
                Bot.Wait.ForCombatExit();
                Bot.Kill.Monster(22);
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive("Legion Guard"))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("Blade Masters killed, moving on.");
                    break;
                }
            }

            Core.PvPMove(23, "r13", 933, 394);

            while (!Bot.ShouldExit && !canSoloBoss)
            {
                Bot.Kill.Monster(23);
                Bot.Wait.ForCombatExit();
                Bot.Kill.Monster(24);
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive("Legion Guard"))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("Legion Guards killed, moving on.");
                    break;
                }
            }

            Core.PvPMove(25, "r14", 846, 181);

            while (!Bot.ShouldExit && !canSoloBoss)
            {
                Bot.Kill.Monster(25);
                Bot.Wait.ForCombatExit();
                Bot.Kill.Monster(26);
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive("Legion Guard"))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("Legion Guards killed, moving on.");
                    break;
                }
            }

            Core.PvPMove(28, "r15", 941, 348);

            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(27);
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive("Dage the Evil"))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("Dage the Evil, getting trophies.");
                    break;
                }
            }

            Bot.Wait.ForDrop("Legion Combat Trophy", 40);
            Bot.Sleep(Core.ActionDelay);
            Bot.Wait.ForPickup("Legion Combat Trophy");

            Core.Logger("Delaying exit");
            Bot.Sleep(7500);

            while (Bot.Map.Name != "battleon")
            {
                int i = 1;
                Core.Logger($"Attemping Exit {i++}.");
                Core.Join("battleon-999999");
                Bot.Sleep(1500);
            }
        }
    }
    // Bot.Events.PlayerDeath -= PVPDeath;


    // private void PVPDeath()
    // {
    //     Core.DebugLogger(this);
    //     Bot.Wait.ForCellChange("Enter0");
    //     Core.DebugLogger(this);

    //     Core.Logger("Player Died in PvP, resetting");
    //     Core.DebugLogger(this);
    //     while (!Bot.ShouldExit && !Bot.Player.Alive)
    //     {
    //         Core.DebugLogger(this);
    //         Bot.Wait.ForTrue(() => Bot.Player.Alive, 20);
    //     }
    //     Core.DebugLogger(this);
    //     Bot.Sleep(2500);
    //     Core.DebugLogger(this);
    //     Bot.Map.Join("battleon-999999");
    //     Core.DebugLogger(this);
    //     BludrutBrawlBoss();
    //     Core.DebugLogger(this);
    // }
}