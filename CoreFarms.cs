/*
name: null
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Factions;
using Skua.Core.Models.Items;
using Skua.Core.Models.Monsters;
using Skua.Core.Models.Quests;

public class CoreFarms
{
    // [Can Change] Can you solo the boss without killing the ads
    public bool canSoloInPvP { get; set; } = true;

    // [Can Change] Use boosts on Gold farming
    public bool doGoldBoost { get; set; } = false;
    // [Can Change] Use boosts on Class farming
    public bool doClassBoost { get; set; } = false;
    // [Can Change] Use boosts on Reputation farming
    public bool doRepBoost { get; set; } = false;
    // [Can Change] Use boosts on Experience farming
    public bool doExpBoost { get; set; } = false;

    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.RunCore();
    }

    public void ToggleBoost(BoostType type, bool enabled = true)
    {
        if (enabled)
        {
            if (Core.CBOBool("doGoldBoost", out bool _doGoldBoost))
                doGoldBoost = _doGoldBoost;
            if (Core.CBOBool("doClassBoost", out bool _doClassBoost))
                doClassBoost = _doClassBoost;
            if (Core.CBOBool("doRepBoost", out bool _doRepBoost))
                doRepBoost = _doRepBoost;
            if (Core.CBOBool("doExpBoost", out bool _doExpBoost))
                doExpBoost = _doExpBoost;

            switch (type)
            {
                case BoostType.Gold:
                    if (!doGoldBoost || Bot.Boosts.UseGoldBoost || Bot.Player.Gold >= 100000000)
                        return;
                    Bot.Boosts.SetGoldBoostID();
                    Bot.Boosts.UseGoldBoost = true;
                    break;

                case BoostType.Class:
                    if (!doClassBoost || Bot.Boosts.UseClassBoost)
                        return;
                    Bot.Boosts.SetClassBoostID();
                    Bot.Boosts.UseClassBoost = true;
                    break;

                case BoostType.Reputation:
                    if (!doRepBoost || Bot.Boosts.UseReputationBoost)
                        return;
                    Bot.Boosts.SetReputationBoostID();
                    Bot.Boosts.UseReputationBoost = true;
                    break;

                case BoostType.Experience:
                    if (!doExpBoost || Bot.Boosts.UseExperienceBoost || Bot.Player.Level == 100)
                        return;
                    Bot.Boosts.SetExperienceBoostID();
                    Bot.Boosts.UseExperienceBoost = true;
                    break;
            }
            Bot.Boosts.Start();
        }
        else
        {
            switch (type)
            {
                case BoostType.Gold:
                    if (!Bot.Boosts.UseGoldBoost)
                        return;
                    Bot.Boosts.UseGoldBoost = false;
                    break;

                case BoostType.Class:
                    if (!Bot.Boosts.UseClassBoost)
                        return;
                    Bot.Boosts.UseClassBoost = false;
                    break;

                case BoostType.Reputation:
                    if (!Bot.Boosts.UseReputationBoost)
                        return;
                    Bot.Boosts.UseReputationBoost = false;
                    break;

                case BoostType.Experience:
                    if (!Bot.Boosts.UseExperienceBoost)
                        return;
                    Bot.Boosts.UseExperienceBoost = false;
                    break;
            }
            if (new[] { Bot.Boosts.UseGoldBoost, Bot.Boosts.UseClassBoost, Bot.Boosts.UseReputationBoost, Bot.Boosts.UseExperienceBoost }.All(on => !on))
                Bot.Boosts.Stop();
        }
    }

    #region Gold
    public void Gold(int quant = 100000000)
    {
        if (Bot.Player.Gold >= quant)
            return;

        ToggleBoost(BoostType.Gold);

        HonorHall(quant);
        BattleGroundE(quant);
        BerserkerBunny(quant);

        ToggleBoost(BoostType.Gold, false);
    }

    /// <summary>
    /// Farms Gold in HonorHall (members) with quests HonorHall Mobs and 61-75
    /// </summary>
    /// <param name="goldQuant">How much gold to farm</param>
    public void HonorHall(int goldQuant = 100000000)
    {
        if (!Core.IsMember || Bot.Player.Level < 61 || Bot.Player.Gold >= goldQuant)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        Core.Logger($"Farming {goldQuant} gold using HonorHall Method");

        Core.RegisterQuests(3992, 3993);
        while (!Bot.ShouldExit && Bot.Player.Gold < goldQuant)
        {
            Core.KillMonster("honorhall", "r1", "Center", "Ice Demon", "Battleground E Opponent Defeated", 10, log: false);
            Core.KillMonster("honorhall", "r1", "Center", "Ice Demon", "HonorHall Opponent Defeated", 10, log: false);
        }
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }

    /// <summary>
    /// Farms Gold in Battle Ground E with quests Level 46-60 and 61-75
    /// </summary>
    /// <param name="goldQuant">How much gold to farm</param>
    public void BattleGroundE(int goldQuant = 100000000)
    {
        if (Bot.Player.Gold >= goldQuant || Bot.Player.Level < 61)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        Core.Logger($"Farming {goldQuant} gold using BattleGroundE Method");

        Core.RegisterQuests(3991, 3992);
        while (!Bot.ShouldExit && Bot.Player.Gold < goldQuant)
        {
            Core.KillMonster("battlegrounde", "r2", "Center", "*", "Battleground E Opponent Defeated", 10, log: false);
            Core.KillMonster("battlegrounde", "r2", "Center", "*", "Battleground D Opponent Defeated", 10, log: false);
        }

        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }

    /// <summary>
    /// Farms Gold by selling Berserker Bunny
    /// </summary>
    /// <param name="goldQuant">How much gold to farm</param>
    public void BerserkerBunny(int goldQuant = 100000000, bool sell = true)
    {
        if (Bot.Player.Gold >= goldQuant)
            return;

        Core.AddDrop("Berserker Bunny");
        Core.EquipClass(ClassType.Solo);
        Core.SavedState();
        Core.Logger($"Farming {goldQuant}  using BerserkerBunny Method");

        Core.RegisterQuests(236);
        while (!Bot.ShouldExit && Bot.Player.Gold < goldQuant)
        {
            Core.HuntMonster("greenguardwest", "Big Bad Boar", "Were Egg", log: false);
            Bot.Wait.ForDrop("Berserker Bunny", 40);
            if (!sell)
                return;
            Bot.Sleep(Core.ActionDelay);
            Core.SellItem("Berserker Bunny");
        }
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }

    // <summary>
    // Farms Gold by Kill mobs in "darkwarlegion" for Badges and turning the quest in. (ignore the missign turning reqs.. its to quick)
    // </summary>
    // <param name="goldQuant">How much gold to farm</param>
    public void DarkWarLegion(int goldQuant = 100000000) //Slower then BattleGroundE
    {
        if (Bot.Player.Gold >= goldQuant)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        Core.Logger($"Farming {goldQuant}  using DarkWarLegion Method");

        Core.RegisterQuests(8584, 8585);
        while (!Bot.ShouldExit && Bot.Player.Gold < goldQuant)
            Core.KillMonster("darkwarlegion", "r2", "Left", "*", "Nation Badge", 5, log: false);
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }
    #endregion

    #region Experience
    public void Experience(int level = 100, bool rankUpClass = false)
    {
        if (Bot.Player.Level >= level && !rankUpClass)
            return;

        if (!rankUpClass)
            Core.EquipClass(ClassType.Farm);
        if (rankUpClass)
            ToggleBoost(BoostType.Class);

        ToggleBoost(BoostType.Experience);


        Core.RegisterQuests(4007);
        while (!Bot.ShouldExit && Bot.Player.Level < 10)
            Core.KillMonster("oaklore", "r3", "Left", "Bone Berserker", log: false);
        Core.CancelRegisteredQuests();

        //i swear this is active yearround for a black friday "locked" quest according to the wiki.. keep undead warrior as a backup.. undead giant is slower but will work till we find a fillin - perhaps sluethhouseinn? used to be good years ago
        if (Bot.Quests.IsAvailable(6979))
        {
            Core.EquipClass(ClassType.Solo);
            Core.RegisterQuests(6979);
            while (!Bot.ShouldExit && Bot.Player.Level < 30)
                Core.HuntMonster("prison", "Piggy Drake", log: false);
            Core.CancelRegisteredQuests();
        }
        else
        {
            UndeadGiantUnlock();
            Core.RegisterQuests(178);
            while (!Bot.ShouldExit && Bot.Player.Level < 30)
                Core.HuntMonster("swordhavenundead", "Undead Giant", log: false);
            Core.CancelRegisteredQuests();
        }

        FireWarxp(40);

        IcestormArena(level);

        if (rankUpClass)
            ToggleBoost(BoostType.Class, false);
        ToggleBoost(BoostType.Experience, false);

    }


    /// <summary>
    /// Farms level in Ice Storm Arena
    /// </summary>
    /// <param name="level">Desired level</param>
    public void IcestormArena(int level = 100, bool rankUpClass = false)
    {
        if (Bot.Player.Level >= level && !rankUpClass)
            return;

        if (!rankUpClass)
            Core.EquipClass(ClassType.Farm);
        if (rankUpClass)
            ToggleBoost(BoostType.Class);
        Core.ToggleAggro(true);
        Core.SavedState();

        //Between level 1 and 5
        while (NotYetLevel(5))
            Core.KillMonster("icestormarena", "r4", "Bottom", "*", log: false, publicRoom: true);

        //Between level 5 and 10
        while (NotYetLevel(10))
            Core.KillMonster("icestormarena", "r5", "Left", "*", log: false, publicRoom: true);

        //Between level 10 and 20
        while (NotYetLevel(20))
            Core.KillMonster("icestormarena", "r6", "Left", "*", log: false, publicRoom: true);

        //Between level 20 and 25
        if (NotYetLevel(25))
        {
            Core.RegisterQuests(6628);
            while (NotYetLevel(25))
                Core.KillMonster("icestormarena", "r7", "Left", "*", "Icewing Grunt Defeated", 3, log: false, publicRoom: true);
            Core.CancelRegisteredQuests();
        }

        //Between level 25 and 30
        while (NotYetLevel(30))
            Core.KillMonster("icestormarena", "r10", "Left", "*", log: false, publicRoom: true);

        //Between level 30 and 35
        if (NotYetLevel(35))
        {
            Core.RegisterQuests(6629);
            while (NotYetLevel(35))
                Core.KillMonster("icestormarena", "r11", "Left", "*", "Icewing Warrior Defeated", 3, log: false, publicRoom: true);
            Core.CancelRegisteredQuests();
        }

        //Between level 35 and 50
        while (NotYetLevel(50))
            Core.KillMonster("icestormarena", "r14", "Left", "*", log: false, publicRoom: true);

        //Between level 50 and 61(for BGE)
        while (NotYetLevel(61))
            Core.KillMonster("icestormarena", "r16", "Left", "*", log: false, publicRoom: true);

        //Between level 61 and 75
        if (NotYetLevel(75))
        {
            ToggleBoost(BoostType.Gold);
            Core.RegisterQuests(3991, 3992);
            while (NotYetLevel(75))
                Core.KillMonster("battlegrounde", "r2", "Center", "*", log: false, publicRoom: true);
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Gold, false);
        }

        //Between level 75 and 100
        while (NotYetLevel(100))
            Core.KillMonster(Core.IsMember ? "nightmare" : "icestormunder", Core.IsMember ? "r13" : "r2", Core.IsMember ? "left" : "Top", "*", log: false);

        Core.SavedState(false);
        Core.ToggleAggro(false);
        if (rankUpClass)
            ToggleBoost(BoostType.Class, false);

        bool NotYetLevel(int _level)
        {
            return !Bot.ShouldExit && (Bot.Player.Level < _level && Bot.Player.Level < level) || (Bot.Player.Level <= _level && rankUpClass && Bot.Player.CurrentClassRank != 10);
        }

    }

    /// <summary>
    /// Farms in Seven Circles War for level and items
    /// </summary>
    /// <param name="level">Desired level</param>
    public void SevenCirclesWar(int level = 100, int gold = 100000000)
    {
        if (Bot.Player.Level >= level && Bot.Player.Gold >= gold)
            return;

        if (!Core.isCompletedBefore(7979))
        {
            Core.Logger("Please use Scripts/Story/Legion/SevenCircles(War).cs in order to use the SevenCircles method");
            return;
        }

        Core.AddDrop("Essence of Wrath", "Souls of Heresy");
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        Core.Logger($"Farming {gold} gold using SCW Method");

        Core.RegisterQuests(7979, 7980, 7981);
        while (!Bot.ShouldExit && level == 101 ? Bot.Player.Gold < gold : Bot.Player.Level < level && Bot.Player.Gold < gold)
        {
            Core.KillMonster("sevencircleswar", "Enter", "Right", "*", "Wrath Guards Defeated", 12);
            Core.KillMonster("sevencircleswar", "Enter", "Right", "*", "War Medal", 5);
            Core.KillMonster("sevencircleswar", "Enter", "Right", "*", "Mega War Medal", 3);
        }
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }

    /// <summary>
    /// Farms level in FireWar Turnins
    /// </summary>
    /// <param name="level">Desired level</param>
    public void FireWarxp(int level)
    {
        if (Bot.Player.Level >= 60)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();

        Core.RegisterQuests(6294, 6295);
        while (!Bot.ShouldExit && Bot.Player.Level < level)
            Core.KillMonster("Firewar", "r2", "Right", "*", log: false);
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }
    #endregion

    #region Misc
    /// <summary>
    /// Farms the Black Knight Orb
    /// </summary>
    public void BlackKnightOrb()
    {
        if (Core.CheckInventory("Black Knight Orb"))
            return;

        Core.AddDrop("Black Knight Orb");
        Core.EnsureAccept(318);

        Core.HuntMonster("well", "Gell Oh No", "Black Knight Leg Piece");
        Core.HuntMonster("greendragon", "Greenguard Dragon", "Black Knight Chest Piece");
        Core.HuntMonster("deathgazer", "Deathgazer", "Black Knight Shoulder Piece");
        Core.HuntMonster("trunk", "GreenGuard Basilisk", "Black Knight Arm Piece");

        Core.EnsureComplete(318);
        Bot.Drops.Pickup("Black Knight Orb");
    }

    /// <summary>
    /// Kills the Restorers from /BludrutBrawl for "The Secret 4" item
    /// </summary>
    public void TheSecret4()
    {
        if (Core.CheckInventory("The Secret 4"))
            return;
        Core.EquipClass(ClassType.Solo);
        Core.JumpWait();
        while (!Bot.ShouldExit && !Core.CheckInventory("The Secret 4"))
        {
            while (!Bot.ShouldExit && Bot.Map.Name != "bludrutbrawl")
            {
                Bot.Sleep(5000);
                Core.JumpWait();
                Core.Join("bludrutbrawl", "Enter0", "Spawn");
            }

            Core.PvPMove(5, "Morale0C");
            Core.PvPMove(4, "Morale0B");
            Core.PvPMove(7, "Morale0A");
            Core.PvPMove(9, "Crosslower");
            Core.PvPMove(14, "Crossupper", 528, 255);

            Core.PvPMove(18, "Resource1A");
            Bot.Kill.Monster("(B) Defensive Restorer");
            Bot.Drops.Pickup("The Secret 4");
            Bot.Kill.Monster("(B) Defensive Restorer");
            Bot.Drops.Pickup("The Secret 4");

            Core.PvPMove(20, "Resource1B");
            Bot.Kill.Monster("(B) Defensive Restorer");
            Bot.Drops.Pickup("The Secret 4");
            Bot.Kill.Monster("(B) Defensive Restorer");
            Bot.Drops.Pickup("The Secret 4");

            while (!Bot.ShouldExit && Bot.Map.Name != "battleon")
            {
                Bot.Sleep(5000);
                Core.JumpWait();
                Core.Join("battleon");
            }
        }
    }

    /// <summary>
    /// Kills the Team B Captain in /BludrutBrawl for the desired item (Combat Trophy or Yoshino's Citrine)
    /// </summary>
    /// <param name="item">Name of the desired item</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="canSoloBoss">Whether you can solo the Boss without killing Restorers and Brawlers</param>
    public void BludrutBrawlBoss(string item = "Combat Trophy", int quant = 5000) //, bool canSoloBoss = true)
    {
        if (Core.CheckInventory(item, quant))
            return;

        Core.Logger("Kill ads is on by default now\n" +
        "as you get rewarded 10 rather then 3\n" +
        "Combat Trophies");


        // if (Core.CBOBool("PvP_SoloPvPBoss", out bool _canSoloBoss))
        //     canSoloBoss = !_canSoloBoss;

        Core.AddDrop(item);
        Bot.Drops.Add("The Secret 4", "Yoshino's Citrine");

        Core.EquipClass(ClassType.Solo);
        Core.FarmingLogger(item, quant);
        // Core.Logger($"Farming {quant} {item}. SoloBoss = {canSoloBoss}");
        Core.ConfigureAggro(false);

        // Bot.Events.PlayerDeath += PVPDeath;
        Core.EquipClass(ClassType.Solo);
        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            Core.Join("bludrutbrawl", "Enter0", "Spawn");
            Bot.Sleep(2500);

            Core.PvPMove(5, "Morale0C");
            Core.PvPMove(4, "Morale0B");
            Core.PvPMove(7, "Morale0A");
            Core.PvPMove(9, "Crosslower");

            // if (!canSoloBoss)
            // {
            Core.PvPMove(14, "Crossupper", 528, 255);
            Core.PvPMove(18, "Resource1A");

            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(9);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                Bot.Kill.Monster(10);
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(313))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("(B) Defensive Restorers killed, moving on.");
                    break;
                }
            }

            Core.PvPMove(20, "Resource1B");

            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(11);
                Bot.Wait.ForCombatExit();
                Bot.Kill.Monster(12);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(313))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("(B) Defensive Restorers killed, moving on.");
                    break;
                }
            }

            Core.PvPMove(21, "Resource1A", 124);
            Core.PvPMove(19, "Crossupper", 124);
            Core.PvPMove(17, "Crosslower", 488, 483);
            // }
            Core.PvPMove(15, "Morale1A", 862, 268);

            // if (!canSoloBoss)
            // {
            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(13);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(311))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("(B) Brawlers killed, moving on.");
                    break;
                }
            }
            // }

            Core.PvPMove(23, "Morale1B", 860, 267);

            // if (!canSoloBoss)
            // {
            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(14);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(311))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("(B) Brawlers killed, moving on.");
                    break;
                }
            }
            // }

            Core.PvPMove(25, "Morale1C", 857, 267);

            // if (!canSoloBoss)
            // {
            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(15);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(311))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("(B) Brawlers killed, moving on.");
                    break;
                }
            }
            // }

            Core.PvPMove(28, "Captain1", 528, 255);

            while (!Bot.ShouldExit)
            {
                Bot.Kill.Monster(16);
                Bot.Combat.CancelTarget();
                Bot.Wait.ForCombatExit();
                if (!Core.IsMonsterAlive(309))
                {
                    Bot.Combat.CancelTarget();
                    Core.Logger("Team B Captain killed, moving on.");
                    break;
                }
            }

            Bot.Wait.ForDrop(item, 40);
            Bot.Sleep(1500);
            Bot.Wait.ForPickup(item, 40);

            Core.Logger("Delaying exit");
            Bot.Sleep(7500);

            int i = 1;
            while (Bot.Map.Name != "battleon")
            {
                Core.Logger($"Attempting Exit {i++}.");
                Bot.Map.Join("battleon-999999");
                Bot.Sleep(1500);
                if (Bot.Map.Name != "battleon")
                    Core.Logger("Failed!? HOW.. try agian");
            }
            Core.Logger("Successful!");
        }

        foreach (string reward in new[] { "Yoshino's Citrine", "The Secret 4" })
        {
            if (item != reward && Bot.Inventory.Contains(reward))
                Core.ToBank(reward);
        }
    }

    public void BattleUnderB(string item = "Bone Dust", int quant = 1, bool isTemp = false)
    {
        if (Core.CheckInventory(item, quant))
            return;

        if (item == "Undead Energy" && !Core.isCompletedBefore(2084))
        {
            Core.Logger("Making it so undead energy can drop..");

            // 2066 - Reforging the Blinding Light
            if (!Core.isCompletedBefore(2066))
            {
                Core.EnsureAccept(2066);
                Core.BuyItem("doomwood", 276, "Blinding Light of Destiny Handle");
                Core.EnsureComplete(2066);
            }

            // 2067 - Secret Order of Undead Slayers
            if (!Core.isCompletedBefore(2067))
            {
                Core.EnsureAccept(2082);
                Core.BuyItem("doomwood", 276, "Bonegrinder Medal");
                Core.EnsureComplete(2082);
            }

            // 2082 - Essential Essences
            if (!Core.isCompletedBefore(2082))
            {
                Core.EnsureAccept(2082);
                Core.HuntMonster("battleunderb", "Skeleton Warrior", "Undead Essence", isTemp: false);
                Core.EnsureComplete(2082);
            }

            // 2083 - Bust some Dust
            if (!Core.isCompletedBefore(2083))
            {
                Core.EnsureAccept(2083);
                Core.HuntMonster("battleunderb", "Skeleton Warrior", "Bone Dust", isTemp: false);
                Core.EnsureComplete(2083);
            }

            // 2084 - A Loyal Follower
            if (!Core.isCompletedBefore(2084))
            {
                Core.EnsureAccept(2084);
                BoneSomeDust(100);
                Core.HuntMonster("timevoid", "Ephemerite", "Celestial Compass");
                Core.EnsureComplete(2084);
            }
        }

        Core.AddDrop(item);
        Core.FarmingLogger(item, quant);
        Core.EquipClass(ClassType.Farm);
        Core.KillMonster("battleunderb", "Enter", "Spawn", "*", item, quant, isTemp, log: false);
    }

    public void BoneSomeDust(int quant = 10500)
    {
        if (Core.CheckInventory("Spirit Orb", quant))
            return;

        Core.AddDrop("Bone Dust", "Undead Essence", "Undead Energy", "Spirit Orb");
        Core.EquipClass(ClassType.Farm);
        Core.FarmingLogger("Spirit Orb", quant);

        Core.RegisterQuests(2082, 2083);
        while (!Bot.ShouldExit && !Core.CheckInventory("Spirit Orb", quant))
            Core.KillMonster("battleunderb", "Enter", "Spawn", "Skeleton Warrior", log: false);
        Core.CancelRegisteredQuests();
    }

    #endregion

    #region Reputation
    public void GetAllRanks()
    {
        ToggleBoost(BoostType.Reputation);

        AegisREP();
        AlchemyREP();
        ArcangroveREP();
        BaconCatREP();
        if (Core.IsMember)
            BeastMasterREP();
        BlacksmithingREP();
        BladeofAweREP(farmBoA: false);
        if (Core.isSeasonalMapActive("birdswithharms"))
            BrethwrenREP();
        BrightoakREP();
        ChaosMilitiaREP();
        ChaosREP();
        ChronoSpanREP();
        CraggleRockREP();
        DiabolicalREP();
        DoomWoodREP();
        DreadfireREP();
        DreadrockREP();
        DruidGroveREP();
        DwarfholdREP();
        ElementalMasterREP();
        EmberseaREP();
        EternalREP();
        EtherStormREP();
        EvilREP();
        if (Core.isSeasonalMapActive("rainbow"))
            FaerieCourtREP();
        FishingREP();
        GlaceraREP();
        GoodREP();
        HollowbornREP();
        HorcREP();
        InfernalArmyREP();
        LoremasterREP();
        LycanREP();
        MonsterHunterREP();
        MysteriousDungeonREP();
        MythsongREP();
        NecroCryptREP();
        NorthpointeREP();
        PetTamerREP();
        RavenlossREP();
        SandseaREP();
        if (Core.IsMember)
            SkyguardREP();
        SomniaREP();
        SpellCraftingREP();
        SwordhavenREP();
        ThunderForgeREP();
        TreasureHunterREP();
        TrollREP();
        VampireREP();
        YokaiREP();
        //Death Pit scripts here because they take alot of time and kill script efficieny
        DeathPitBrawlREP();
        DeathPitArenaREP();

        ToggleBoost(BoostType.Reputation, false);
    }

    public void AegisREP(int rank = 10)
    {
        if (FactionRank("Aegis") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(4900, 4910, 4914); //Kick Some Can 4900, The Best You Can Buy 4910, Testing My Metal 4914
        while (!Bot.ShouldExit && FactionRank("Aegis") < rank)
        {
            Core.HuntMonster("skytower", "Seraphic Assassin", "Seraphic Assassin Dueled", 10, log: false);
            Core.HuntMonster("skytower", "Virtuous Warrior", "Warriors Dueled", 10, log: false);
            Core.HuntMonster("skytower", "Seraphic Assassin", "Assassins Handed To Them", 6, log: false);
            Core.HuntMonster("skytower", "Virtuous Warrior", "Warrior Butt Beaten", 6, log: false);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    /// <summary>
    /// Uses the specified parameters to make an Alchemy misture
    /// </summary>
    /// <param name="reagent1">The first reagent.</param>
    /// <param name="reagent2">The second reagent</param>
    /// <param name="rune">The rune to be used (AlchemyRunes.Gebo by default).</param>
    /// <param name="rank">The minimum rank to make the misture, use 0 for any rank.</param>
    /// <param name="loop">Whether loop till you run out of reagents</param>
    /// <param name="modifier">Some mistures have specific packet modifiers, default is Moose but you can find Man, mRe and others.</param>
    public void AlchemyPacket(string reagent1, string reagent2, AlchemyRunes rune = AlchemyRunes.Gebo, int rank = 0, bool loop = true, string modifier = "Moose", AlchemyTraits trait = AlchemyTraits.APw, bool P2w = false)
    {
        if (rank != 0 && FactionRank("Alchemy") < rank)
            AlchemyREP(rank);

        // Core.Join("Alchemy");
        if (!Core.CheckInventory(reagent1) || !Bot.Inventory.TryGetItem(reagent1, out var reg1))
        {
            Core.Logger("Something went wrong, you do not own " + reagent1, messageBox: true, stopBot: true);
            return;
        }
        if (!Core.CheckInventory(reagent2) || !Bot.Inventory.TryGetItem(reagent1, out var reg2))
        {
            Core.Logger("Something went wrong, you do not own " + reagent2, messageBox: true, stopBot: true);
            return;
        }

        int reagentid1 = reg1!.ID;
        int reagentid2 = reg2!.ID;

        if (reagent1 == "Dragon Scale")
            reg1!.ID = 11475;

        Core.Logger($"Reagents: [{reagent1}], [{reagent2}].");
        Core.Logger($"Rune: {rune}.");
        Core.Logger($"Modifier: {modifier}.");
        int i = 1;
        if (loop)
        {
            while (!Bot.ShouldExit && Core.CheckInventory(new[] { reagent1, reagent2 }))
            {
                if (P2w && !Core.CheckInventory("Dragon Runestone"))
                    break;
                Packet();
                Core.Logger($"Completed alchemy x{i++}");
            }
        }
        else Packet();
        //Times alchemy was fixed: TO FUCKING MANY I HATE ARTIX

        void Packet()
        {
            if (!P2w)
                Bot.Sleep(3500);
            else Bot.Sleep(Core.ActionDelay);
            if (P2w && Core.CheckInventory("Dragon Runestone"))
                Bot.Send.Packet($"%xt%zm%crafting%1%getAlchWait%{reagentid1}%{reagentid2}%true%Ready to Mix%{reagent1}%{reagent2}%{rune}%{trait}%");
            else if (!P2w)
                Bot.Send.Packet($"%xt%zm%crafting%1%getAlchWait%{reagentid1}%{reagentid2}%false%Ready to Mix%{reagent1}%{reagent2}%{rune}%{modifier}%");
            if (P2w)
                Bot.Sleep(2500);
            else Bot.Sleep(11000);
            if (P2w && Core.CheckInventory("Dragon Runestone"))
                Bot.Send.Packet($"%xt%zm%crafting%1%checkAlchComplete%{reagentid1}%{reagentid2}%true%Mix Complete%{reagent1}%{reagent2}%{rune}%{trait}%");

            else if (!P2w)
                Bot.Send.Packet($"%xt%zm%crafting%1%checkAlchComplete%{reagentid1}%{reagentid2}%false%Mix Complete%{reagent1}%{reagent2}%{rune}%{modifier}%");
        }

    }


    /// <summary>
    /// Uses the Jera:hOu in the alchemy packet for starting rep.
    /// to find the correct trait for a specific pot, goto /join alchemy with the correct reagents
    /// and open packet logger, enable it, start "help me", "use dragon stones"
    /// slect the level/type/kind(atk, int, spell, etc), and start it, grab the packet(copy it)
    /// make request with that and the Potion's name+itemid(from tools > grabber > inventory > grab)
    /// </summary>
    public enum AlchemyTraits
    {
        Dam = 0, // Potent Honor Potion (trait, itemID)
        APw = 1, // Potent Battle Elixir
        Luc = 2, // Fate Tonic
        Int = 3, // Sage Tonic
        SPw = 4, // Potent Malevolence Elixir    
        hOu = 5, // Healer Elixer / Potent Guard Potion / Unstable Healer Elixer // rep spam with jerra
        hRe = 6, // Potent Revitalize Elixi
        mRe = 7, // Potent Destruction Elixir
        End = 8, // Body Tonic
        Eva = 9, //
        Str = 10,
        Cri = 11,
        Dex = 12,
        Wis = 13
        //more to be added by request
    };

    public void DragonRunestone(int quant = 100)
    {
        if (Core.CheckInventory("Dragon Runestone", quant))
            return;

        Core.ToggleAggro(false);
        Core.FarmingLogger("Dragon Runestone", quant);

        Gold((100000 * (quant - Bot.Inventory.GetQuantity("Dragon Runestone"))) - Bot.Inventory.GetQuantity("Gold Voucher 100k"));
        Core.BuyItem("Alchemy", 395, "Gold Voucher 100k", (quant - Bot.Inventory.GetQuantity("Dragon Runestone")));
        Core.BuyItem("Alchemy", 395, "Dragon Runestone", quant, 8844);
    }

    public void AlchemyREP(int rank = 10, bool goldMethod = true)
    {
        if (FactionRank("Alchemy") >= rank)
            return;

        if (!Bot.Reputation.FactionList.Exists(f => f.Name == "Alchemy"))
        {
            Core.Logger("Getting Pre-Ranking XP");
            if (!Core.CheckInventory(new[] { 11478, 11475, 7132 }))
            {
                DragonRunestone(1);
                Core.BuyItem("alchemyacademy", 397, 11475, 2, 1232);
                Core.BuyItem("alchemyacademy", 397, 11478, 1, 1235);

                AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Jera, loop: false, trait: CoreFarms.AlchemyTraits.hOu, P2w: true);
            }
        }

        Core.AddDrop("Dragon Scale", "Ice Vapor");
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank} Alchemy");

        int i = 1;
        while (!Bot.ShouldExit && FactionRank("Alchemy") < rank)
        {
            if (goldMethod)
            {
                if (!Core.CheckInventory(new[] { 11478, 11475, 7132 }))
                {
                    DragonRunestone(25);
                    Core.BuyItem("alchemyacademy", 397, 11475, 20, 1232);
                    Core.BuyItem("alchemyacademy", 397, 11478, 10, 1235);
                }

                if (FactionRank("Alchemy") < 3)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Jera, trait: CoreFarms.AlchemyTraits.hOu, P2w: true);

                else if (FactionRank("Alchemy") < 5)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Uruz, trait: CoreFarms.AlchemyTraits.hOu, P2w: true);

                else if (FactionRank("Alchemy") < 8)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Fehu, trait: CoreFarms.AlchemyTraits.hOu, P2w: true);

                else if (FactionRank("Alchemy") >= 8)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Gebo, trait: CoreFarms.AlchemyTraits.hOu, P2w: true);
            }
            else
            {
                Core.EquipClass(ClassType.Farm);
                while (!Core.CheckInventory(11475, 30))
                    Core.KillMonster("lair", "Hole", "Center", "*", isTemp: false, log: false);
                Core.KillMonster("lair", "Enter", "Spawn", "*", "Ice Vapor", 30, isTemp: false, log: false);

                if (FactionRank("Alchemy") < 3)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Jera, trait: CoreFarms.AlchemyTraits.hOu, P2w: false);

                if (FactionRank("Alchemy") < 5)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Uruz, trait: CoreFarms.AlchemyTraits.hOu, P2w: false);

                if (FactionRank("Alchemy") < 8)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Fehu, trait: CoreFarms.AlchemyTraits.hOu, P2w: false);

                if (FactionRank("Alchemy") >= 8)
                    AlchemyPacket("Dragon Scale", "Ice Vapor", AlchemyRunes.Gebo, trait: CoreFarms.AlchemyTraits.hOu, P2w: false);
            }
            Core.Logger($"Iteration {i++} completed");
        }
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void ArcangroveREP(int rank = 10)
    {
        if (FactionRank("Arcangrove") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");
        Core.Logger("This will Do First Get all the quest\n" +
        "items for all 8 quests, then turn them in.");
        Bot.Sleep(2500);
        //Restore this v when 1.2.4 comes out to fix mobs
        // Core.RegisterQuests(794, 795, 796, 797, 798, 799, 800, 801); //A Necessary Sacrifice 794, Gorillaphant Poaching 795, Mustard and Pretzel Root 796, Thyme and a Half 797, Thistle Do Nicely 798, Pleased to Meat You 799, ArcanRobe 800, Ebony and Ivory Tusks 801
        while (!Bot.ShouldExit && FactionRank("Arcangrove") < rank)
        {
            Core.EnsureAccept(794, 795, 796, 797, 798, 799, 800, 801); //A Necessary Sacrifice 794, Gorillaphant Poaching 795, Mustard and Pretzel Root 796, Thyme and a Half 797, Thistle Do Nicely 798, Pleased to Meat You 799, ArcanRobe 800, Ebony and Ivory Tusks 801
            Core.HuntMonster("arcangrove", "Seed Spitter", "Spool of Arcane Thread", 10);
            Core.HuntMonster("arcangrove", "Seed Spitter", "Defeated Seed Spitter", 10);
            Core.HuntMonster("arcangrove", "Seed Spitter", "Bundle of Thyme", 10);
            Core.HuntMonster("arcangrove", "Seed Spitter", "Thistle", 5);
            Core.HuntMonster("arcangrove", "Seed Spitter", "Pretzel Root", 4);
            Core.HuntMonster("arcangrove", "Gorillaphant", "Lore-Strip Gorillaphant Steak", 8);
            Core.HuntMonster("arcangrove", "Gorillaphant", "Slain Gorillaphant", 7);
            Core.HuntMonster("arcangrove", "Gorillaphant", "Gorillaphant Tusk", 6);
            Core.HuntMonster("arcangrove", "Gorillaphant", "Batch of Mustard Seeds", 3);
            Core.EnsureComplete(794, 795, 796, 797, 798, 799, 800, 801); //A Necessary Sacrifice 794, Gorillaphant Poaching 795, Mustard and Pretzel Root 796, Thyme and a Half 797, Thistle Do Nicely 798, Pleased to Meat You 799, ArcanRobe 800, Ebony and Ivory Tusks 801
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void BaconCatREP(int rank = 10)
    {
        if (FactionRank("BaconCat") >= rank)
            return;

        if (!Bot.Quests.IsUnlocked(5120))
        {
            Core.Logger($"Quest [5120] \"Ziri Is Also Tough\", has yet to be completed, please run \"Farm/REP/BaconCatREP.cs\"", messageBox: true);
            return;
        }

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5112, 5120);
        while (!Bot.ShouldExit && FactionRank("BaconCat") < rank)
            Core.HuntMonster("baconcatlair", "Ice Cream Shark", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void BeastMasterREP(int rank = 10)
    {
        if (FactionRank("BeastMaster") >= rank)
            return;

        if (!Core.IsMember)
        {
            Core.Logger("Beast Master REP is Member-Only", messageBox: true);
            return;
        }

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        // Core.RegisterQuests(3757);
        while (!Bot.ShouldExit && FactionRank("BeastMaster") < rank)
        {
            Core.EnsureAccept(3757);
            Core.HuntMonster("pyramid", "Golden Scarab", "Gleaming Gems of Containment", 9, log: false);
            Core.HuntMonster("lair", "Golden Draconian", "Bright Binding of Submission", 8, log: false);
            Core.EnsureComplete(3757);
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void BlacksmithingREP(int rank = 10, bool UseGold = false, bool BulkFarmGold = false)
    {
        if (FactionRank("Blacksmithing") >= rank)
            return;

        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming rank {rank}");

        if (UseGold)
        {
            Core.Logger("using Gold Method");
            // Core.RegisterQuests(8737);
            while (!Bot.ShouldExit && FactionRank("Blacksmithing") < rank)
            {
                Core.EnsureAccept(8737);
                if (Bot.Player.Gold % 5000000 != 0 && BulkFarmGold)
                {
                    ToggleBoost(BoostType.Reputation, false);
                    Gold(1000000); //100m
                }
                else
                {
                    ToggleBoost(BoostType.Reputation, false);
                    Gold(5000000); //5m
                }
                ToggleBoost(BoostType.Reputation);
                Core.BuyItem("alchemyacademy", 2036, "Gold Voucher 500k", Bot.Player.Gold % 10 == 5000000 ? 10 : 1);
                Bot.Sleep(Core.ActionDelay);
                Core.EnsureCompleteMulti(8737);
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
            return;
        }

        Core.Logger("Using Non-Gold Method");
        // Core.Logger($"If you can't Solo SlugButter, Either use the Gold method or Run the AP Script (Found in: Good-ArchPaladin) as it can Solo the boss 👍");

        // Core.RegisterQuests(2777);
        while (!Bot.ShouldExit && FactionRank("Blacksmithing") < 4 && !UseGold)
        {
            Core.EnsureAccept(2777);
            Core.HuntMonster("greenguardeast", "Wolf", "Furry Lost Sock", 2, log: false);
            Core.HuntMonster("greenguardwest", "Slime", "Slimy Lost Sock", 5, log: false);
            Core.EnsureComplete(2777);
        }
        // Core.CancelRegisteredQuests();

        // Core.RegisterQuests(8736);
        while (!Bot.ShouldExit && FactionRank("Blacksmithing") < rank && !UseGold)
        {
            Core.EnsureAccept(8736);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("hydrachallenge", "Hydra Head 25", "Hydra Scale Piece", 75, isTemp: false, log: false);
            Core.HuntMonster("maul", "Creature Creation", "Creature Shard", isTemp: false, log: false);
            Core.HuntMonster("towerofdoom", "Dread Klunk", "Monster Trophy", 15, isTemp: false, log: false);
            Core.EnsureComplete(8736);
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void BladeofAweREP(int rank = 10, bool farmBoA = true)
    {
        if (farmBoA && !Core.CheckInventory(17585))
        {
            UnlockBoA();
            RepFarm(6);
            Core.BuyItem("museum", 631, 17585);
        }
        RepFarm(rank);

        void RepFarm(int rank)
        {
            if (FactionRank("Blade of Awe") >= rank)
                return;

            UnlockBoA();

            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            Core.EquipClass(ClassType.Farm);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(2935);
            while (!Bot.ShouldExit && FactionRank("Blade of Awe") < rank)
                Core.KillMonster("castleundead", "Enter", "Left", "Skeletal Viking", "Hilt Found!", 1, false, log: false);
            Core.CancelRegisteredQuests();
            Core.SavedState(false);
        }

        void UnlockBoA()
        {
            if (Core.isCompletedBefore(2937))
                return;

            if (!Core.isCompletedBefore(2933))
            {
                Core.Logger($"Doing Quest: [2933] - \"Find the Stonewrit!\"");
                Core.EquipClass(ClassType.Solo);
                Core.EnsureAccept(2933);
                Core.HuntMonster("j6", "Sketchy Dragon", "Stonewrit Found!", 1, false, log: false);
                Core.EnsureComplete(2933);
                Core.Logger($"Completed Quest: [2933] - \"Find the Stonewrit!\"");
            }
            else Core.Logger($"Already Completed: [2933] - \"Find the Stonewrit!\"");

            if (!Core.isCompletedBefore(2934))
            {
                Core.Logger($"Doing Quest: [2934] - \"Find the Handle!\"");
                Core.EquipClass(ClassType.Solo);
                Core.EnsureAccept(2934);
                Core.HuntMonster("gilead", "Fire Elemental", "Handle Found!", 1, false, log: false);
                Core.EnsureComplete(2934);
                Core.Logger($"Completed Quest: [2934] - \"Find the Handle!\"");
            }
            else Core.Logger($"Already Completed: [2934] - \"Find the Handle!\"");

            if (!Core.isCompletedBefore(2935))
            {
                Core.Logger($"Doing Quest: [2935] - \"Find the Hilt!\"");
                Core.EquipClass(ClassType.Farm);
                Core.EnsureAccept(2935);
                Core.HuntMonster("castleundead", "Skeletal Viking", "Hilt Found!", 1, false, log: false);
                Core.EnsureComplete(2935);
                Core.Logger($"Completed Quest: [2935] - \"Find the Hilt!\"");
            }
            else Core.Logger($"Already Completed: [2935] - \"Find the Hilt!\"");

            if (!Core.isCompletedBefore(2936))
            {
                Core.Logger($"Doing Quest: [2936] - \"Find the Blade!\"");
                Core.EquipClass(ClassType.Farm);
                Core.EnsureAccept(2936);
                Core.HuntMonster("hydra", "Hydra Head", "Blade Found!", 1, false, log: false);
                Core.EnsureComplete(2936);
                Core.Logger($"Completed Quest: [2936] - \"Find the Blade!\"");
            }
            else Core.Logger($"Already Completed: [2936] - \"Find the Blade!\"");

            if (!Core.isCompletedBefore(2937))
            {
                Core.Logger($"Doing Quest: [2937] - \"Find the Runes!\"");
                Core.EquipClass(ClassType.Solo);
                Core.EnsureAccept(2937);
                Core.KillEscherion("Runes Found!", log: false);
                Core.EnsureComplete(2937);
                Core.Logger($"Completed Quest: [2937] - \"Find the Runes!\"");
            }
            else Core.Logger($"Already Completed: [2937] - \"Find the Runes!\"");
        }
    }

    public void BrethwrenREP(int rank = 10)
    {
        if (FactionRank("Brethwren") >= rank || !Core.isSeasonalMapActive("birdswithharms"))
            return;

        if (!Bot.Quests.IsAvailable(8989))
        {
            Core.Logger("Quest not avaible for farm, run the complete Birds With Harms storyline script.");
            return;
        }

        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(8989);
        while (!Bot.ShouldExit && FactionRank("Brethwren") < rank)
            Core.HuntMonster("birdswithharms", "Turkonian", log: false);

        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }


    public void BrightoakREP(int rank = 10)
    {
        if (FactionRank("Brightoak") >= rank)
            return;

        if (!Bot.Quests.IsAvailable(4667))
        {
            Core.Logger("Quest not avaible for farm, do Brightoak saga till Elfhame [Unlocking the Guardian's Mouth]");
            return;
        }

        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(4667);
        Core.Join("elfhame", "Cut1", "Left");
        Core.ToggleAggro(false);

        while (!Bot.ShouldExit && FactionRank("Brightoak") < rank)
        {
            Bot.Map.GetMapItem(3984);
            Bot.Sleep(Core.ActionDelay);
            Bot.Wait.ForQuestComplete(4667, 20);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void ChaosMilitiaREP(int rank = 10)
    {
        if (FactionRank("Chaos Militia") >= rank)
            return;

        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5775); //Expect the Inquisitors 5775
        while (!Bot.ShouldExit && FactionRank("Chaos Militia") < rank)
            Core.HuntMonster("citadel", "Inquisitor Guard", log: false);
        Bot.Wait.ForQuestComplete(5775);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void ChaosREP(int rank = 10)
    {
        if (FactionRank("Chaos") >= rank)
            return;

        // if (Core.IsMember)
        // MembershipDues(MemberShipsIDS.Chaos, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(3594); //Embrace Your Chaos 3594
            while (!Bot.ShouldExit && FactionRank("Chaos") < rank)
                Core.KillMonster("mountdoomskull", "b1", "Left", "*", log: false);
            Bot.Wait.ForQuestComplete(3594);
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void ChronoSpanREP(int rank = 10)
    {
        if (FactionRank("ChronoSpan") >= rank)
            return;
        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.ChronoSpan, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(2204); //Do the 'do 2204
            while (!Bot.ShouldExit && FactionRank("ChronoSpan") < rank)
            {
                Core.EnsureAccept(2204); //Do the 'do 2204
                Core.KillMonster("thespan", "r6", "Left", "Moglin Ghost", "Tin of Ghost Dust", 2, log: false);
                Core.KillMonster("thespan", "r4", "Left", "Minx Fairy", "8 oz Fairy Glitter", 3, log: false);
                Core.KillMonster("thespan", "r4", "Left", "Tog", "Tog Fang", 4, log: false);
                Core.EnsureComplete(2204); //Do the 'do 2204
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void CraggleRockREP(int rank = 10)
    {
        if (FactionRank("CraggleRock") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");
        Core.AddDrop("Empowered Voidstone");
        Core.RegisterQuests(7277); //Star of the Sandsea 7277
        while (!Bot.ShouldExit && FactionRank("CraggleRock") < rank)
            Core.KillMonster("wanders", "r3", "Down", "Kalestri Worshiper", log: false);
        Bot.Wait.ForQuestComplete(7277);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DeathPitArenaREP(int rank = 10)
    {
        if (FactionRank("Death Pit Arena") >= rank)
            return;

        if (!Bot.Quests.IsAvailable(5154))
        {
            Core.Logger("Quest not available for farm, do the Death Pit Arena saga and unlock the quest [Pax Defeated]");
            return;
        }

        Core.EquipClass(ClassType.Solo);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5153);
        while (!Bot.ShouldExit && FactionRank("Death Pit Arena") < rank)
            Core.HuntMonster("deathpit", "General Hun'Gar", log: false);
        Bot.Wait.ForQuestComplete(5153);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DiabolicalREP(int rank = 10)
    {
        if (FactionRank("Diabolical") >= rank)
            return;

        if (!Bot.Quests.IsUnlocked(7877))
        {
            Core.EnsureAccept(7875);
            Core.HuntMonster("timevoid", "Unending Avatar", "Everlasting Scale");
            Core.EnsureComplete(7875);

            Core.EnsureAccept(7876);
            Core.HuntMonster($"twilightedge", "ChaosWeaver Warrior", "Chaotic Arachnid's Flesh");
            Core.EnsureComplete(7876);
        }

        Core.EquipClass(ClassType.Solo);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(7877);
        while (!Bot.ShouldExit && FactionRank("Diabolical") < rank)
            Core.HuntMonster("mudluk", "Tiger Leech", log: false);
        Bot.Wait.ForQuestComplete(7877);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DoomWoodREP(int rank = 10)
    {
        if (FactionRank("DoomWood") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(1151, 1152, 1153); //Minion Morale 1151, Shadowfall is DOOMed 1152, Grave-lyn Danger, 1153
        while (!Bot.ShouldExit && FactionRank("DoomWood") < rank)
            Core.KillMonster("shadowfallwar", "Garden1", "Left", "*", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DreadfireREP(int rank = 10)
    {
        if (FactionRank("Dreadfire") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5697); //Gather Crystals 5697
        while (!Bot.ShouldExit && FactionRank("Dreadfire") < rank)
            Core.KillMonster("dreadfire", "r13", "Bottom", "Arcane Crystal", log: false);
        Bot.Wait.ForQuestComplete(5697);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DreadrockREP(int rank = 10)
    {
        if (FactionRank("Dreadrock") >= rank)
            return;

        Core.AddDrop("Ghastly Dreadrock Blade");
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(4863, 4862, 4865, 4868); //Endurance Tesssssst 4863, Supply Run 4862, Ghastly Blades 4865, Glub, Glub, Glub 4868
        while (!Bot.ShouldExit && FactionRank("Dreadrock") < rank)
            Core.KillMonster("dreadrock", "r3", "Bottom", "*", log: false);
        Bot.Wait.ForQuestComplete(4868);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DruidGroveREP(int rank = 10)
    {
        if (FactionRank("Druid Grove") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(3049); //Help Professor Maedoc 3049
        while (!Bot.ShouldExit && FactionRank("Druid Grove") < rank)
            Core.HuntMonster("bloodtusk", "Crystal-Rock", log: false);
        Bot.Wait.ForQuestComplete(3049);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void DwarfholdREP(int rank = 10)
    {
        if (FactionRank("Dwarfhold") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Dwarfhold, rank);
        // else
        if (!Bot.Quests.IsUnlocked(320))
        {
            // Seven Sisters
            Core.EnsureAccept(319);
            Core.GetMapItem(56, 7, "tavern");
            Core.EnsureComplete(319);
        }

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        // Core.RegisterQuests(320, 321); //Warm and Furry 320, Shell Shock 321
        while (!Bot.ShouldExit && FactionRank("Dwarfhold") < rank)
        {
            Core.EnsureAccept(320, 321); //Warm and Furry 320, Shell Shock 321
            Core.KillMonster("pines", "Enter", "Right", "Pine Grizzly", "Bear Skin", 5, log: false);
            Core.KillMonster("pines", "Enter", "Right", "Red Shell Turtle", "Red Turtle Shell", 5, log: false);
            Core.EnsureComplete(new[] { 320, 321 }); //Warm and Furry 320, Shell Shock 321
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);

    }

    public void ElementalMasterREP(int rank = 10)
    {
        if (FactionRank("Elemental Master") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        if (!Core.isCompletedBefore(3052))
        {
            Core.EnsureAccept(3052);
            Core.GetMapItem(1921, 1, "dragonrune");
            Core.GetMapItem(1922, 1, "dragonrune");
            Core.GetMapItem(1923, 1, "dragonrune");
            Core.GetMapItem(1924, 1, "dragonrune");
            Core.EnsureComplete(3052);
        }
        
        // Define a dictionary to store the items and their quantities for each elemental
        Dictionary<string, Tuple<string, string, int, int>> elementalItems = new()
        {
            { "Water Elemental", Tuple.Create("Water Drop", "Water Core", 5, 1) },
            { "Fire Elemental", Tuple.Create("Flame", "Fire Core", 5, 1) },
            { "Wind Elemental", Tuple.Create("Breeze", "Air Core", 5, 1) },
            { "Earth Elemental", Tuple.Create("Stone", "Earth Core", 5, 1) }
        };

        while (!Bot.ShouldExit && FactionRank("Elemental Master") < rank)
        {
            Core.EnsureAccept(3298, 3050);

            foreach (var elementalEntry in elementalItems)
            {
                string elementalName = elementalEntry.Key;
                string firstItem = elementalEntry.Value.Item1;
                string secondItem = elementalEntry.Value.Item2;
                int dropQuantity = elementalEntry.Value.Item3;
                int coreQuantity = elementalEntry.Value.Item4;

                while (!Bot.ShouldExit && !Core.CheckInventory(firstItem, dropQuantity))
                    Core.HuntMonster("gilead", elementalName, firstItem, dropQuantity);
                Core.HuntMonster("gilead", elementalName, secondItem, coreQuantity);
            }

            Core.HuntMonster("gilead", "Mana Elemental", "Mana Core");
            Core.EnsureComplete(new[] { 3298, 3050 });
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void EmberseaREP(int rank = 10)
    {
        if (FactionRank("Embersea") >= rank)
            return;

        Core.EquipClass(ClassType.Solo);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        if (Core.isCompletedBefore(4135))
        {
            // Where There's Smoke...(200rep - faster)
            while (!Bot.ShouldExit && FactionRank("Embersea") < rank)
            {
                Core.EnsureAccept(4135);
                Core.GetMapItem(3248, 1, "feverfew");
                Core.EnsureComplete(4135);
                Bot.Wait.ForQuestComplete(4135);
            }
        }
        else
        {
            //  Slay the Blazebinders (500rep - 5 kills)
            Core.RegisterQuests(4228);
            while (!Bot.ShouldExit && FactionRank("Embersea") < rank)
                Core.HuntMonster("fireforge", "Blazebinder", log: false);
        }

        ToggleBoost(BoostType.Reputation, false);
        Core.CancelRegisteredQuests();
        Core.SavedState(false);
    }

    public void EternalREP(int rank = 10)
    {
        if (FactionRank("Eternal") >= rank)
            return;

        if (!Bot.Quests.IsAvailable(5198))
        {
            Core.Logger("Can't do farming quest [Sphynxes are Riddled with Gems] (/fourdpyramid)", messageBox: true);
            return;
        }

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        // Core.RegisterQuests(5198);
        while (!Bot.ShouldExit && FactionRank("Eternal") < rank)
        {
            Core.EnsureAccept(5198);
            Core.HuntMonsterMapID("fourdpyramid", 19, "White Gem", 2, log: false);
            Core.HuntMonsterMapID("fourdpyramid", 20, "Black Gem", 2, log: false);
            Core.EnsureComplete(5198);
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void EtherStormREP(int rank = 10)
    {
        if (FactionRank("Etherstorm") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Etherstorm, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(1721); //Defend Your Master! 1721
            while (!Bot.ShouldExit && FactionRank("Etherstorm") < rank)
            {
                Core.EnsureAccept(1721);
                Core.HuntMonster("etherwardes", "Water Dragon Warrior", "Water Dragon Tears", 3, log: false);
                Core.HuntMonster("etherwardes", "Fire Dragon Warrior", "Fire Dragon Flames", 3, log: false);
                Core.HuntMonster("etherwardes", "Air Dragon Warrior", "Air Dragon Breaths", 3, log: false);
                Core.HuntMonster("etherwardes", "Earth Dragon Warrior", "Earth Dragon Claws", 3, log: false);
                Core.EnsureComplete(1721);
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void EvilREP(int rank = 10)
    {
        if (FactionRank("Evil") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Evil, rank);
        // else
        {
            Core.ChangeAlignment(Alignment.Evil);
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(364); //Youthanize 364
            while (!Bot.ShouldExit && FactionRank("Evil") < 4)
                Core.HuntMonster("swordhavenbridge", "Slime", "Youthanize", log: false);
            Core.CancelRegisteredQuests();

            // Core.RegisterQuests(Core.IsMember ? 366 : 367); //Dangerous Decor 366, Bone-afide 367

            while (!Bot.ShouldExit && FactionRank("Evil") < rank)
            {
                if (Core.IsMember)
                {
                    Core.EnsureAccept(366);
                    Core.HuntMonster("sleuthhound", "Chair", "Chair", 4, log: false);
                    Core.HuntMonster("sleuthhound", "Table", "Table", 2, log: false);
                    Core.HuntMonster("sleuthhound", "Bookcase", "Bookcase", log: false);
                    Core.EnsureComplete(366);
                }
                else
                {
                    Core.EnsureAccept(367);
                    Core.HuntMonster("castleundead", "*", "Replacement Tibia", 6, log: false);
                    Core.HuntMonster("castleundead", "*", "Phalanges", 3, log: false);
                    Core.EnsureComplete(367);
                }
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void FishingREP(int rank = 10, bool shouldDerp = false, bool TrashBait = true, bool GetBoosts = true)
    {
        if (FactionRank("Fishing") >= rank)
        {
            Core.DebugLogger(this);
            if (TrashBait)
                Core.TrashCan("Fishing Bait", "Fishing Dynamite");
            return;
        }

        if (GetBoosts)
        {
            Core.Logger("Getting some boosters to make this quicker");
            GetBoost(10997, "REPUTATION Boost! (10 min)", 6, 1615, true);
        }

        ToggleBoost(BoostType.Reputation);
        Core.AddDrop("Fishing Bait", "Fishing Dynamite");
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        Core.Logger($"Farming rank {rank}");
        #region Prefarm xp
        Core.Logger("Pre-Ranking XP(This is Required)");
        Core.EnsureAccept(1682);
        Core.KillMonster("greenguardwest", "West4", "Right", "Slime", "Faith's Fi'shtick", 1, log: false);
        Core.EnsureComplete(1682);
        Core.Join("fishing");
        #endregion Prefarm xp

        while (!Bot.ShouldExit && FactionRank("Fishing") < 2)
        {
            Core.Logger("Fishing Rank < 2, gotta do bait first");
            GetBaitandDynamite(20, 0);
            Core.Logger($"Fishing With: Fishing Bait");
            Core.Logger($"0 Xp means a Failed Catch, common at lower Fishing (Non-Faction) ranks (Minigame)");

            int Bait = Bot.Inventory.GetQuantity("Fishing Bait");
            int FishAttempt = 1;
            while (!Bot.ShouldExit && Core.CheckInventory("Fishing Bait"))
            {
                Bot.Send.Packet("%xt%zm%FishCast%1%Net%30%");
                Bot.Sleep(10000);
                if (Bait != Bait - 1)
                    Core.Logger($"Cast: x{FishAttempt++}");
            }
        }

        while (!Bot.ShouldExit && FactionRank("Fishing") < rank && (!shouldDerp || !Core.HasAchievement(14)))
        {
            GetBaitandDynamite(0, 20);
            Core.Logger($"Fishing With: Dynamite");
            int Dyamnite = Bot.Inventory.GetQuantity("Fishing Dynamite");
            int FishAttempt = 1;
            while (!Bot.ShouldExit && Core.CheckInventory("Fishing Dynamite") && FactionRank("Fishing") < rank && (!shouldDerp || !Core.HasAchievement(14)))
            {
                Bot.Send.Packet($"%xt%zm%FishCast%1%Dynamite%30%");
                Bot.Sleep(3500);
                Core.SendPackets("%xt%zm%getFish%1%false%");
                if (Dyamnite != Dyamnite - 1)
                    Core.Logger($"Cast: x{FishAttempt++}");
            }
        }
        if (TrashBait)
            Core.TrashCan(new[] { "Fishing Bait", "Fishing Dynamite" });
        Core.SavedState(false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
    }

    public void GetBaitandDynamite(int FishingBaitQuant, int FishingDynamiteQuant)
    {
        if (Core.CheckInventory("Fishing Bait", FishingBaitQuant) && Core.CheckInventory("Fishing Dynamite", FishingDynamiteQuant))
        {
            Core.JumpWait();
            return;
        }

        if (FishingBaitQuant > 0)
        {
            Core.FarmingLogger("Fishing Bait", FishingBaitQuant);
            while (!Bot.ShouldExit && FishingBaitQuant != 0 && !Core.CheckInventory("Fishing Bait", FishingBaitQuant))
            {
                Core.EnsureAccept(1682);
                Core.KillMonster("greenguardwest", "West3", "Right", "Frogzard", "Fishing Bait", FishingBaitQuant, isTemp: false, log: false);
                Core.EnsureComplete(1682);
                Core.Logger($"Fishing Bait: {Bot.Inventory.GetQuantity("Fishing Bait")}/{FishingBaitQuant}");
            }
        }

        if (FishingDynamiteQuant > 0)
        {
            Core.FarmingLogger("Fishing Dynamite", FishingDynamiteQuant);
            while (!Bot.ShouldExit && FishingDynamiteQuant != 0 && !Core.CheckInventory("Fishing Dynamite", FishingDynamiteQuant))
            {
                Core.EnsureAccept(1682);
                Core.KillMonster("greenguardwest", "West4", "Right", "Slime", "Faith's Fi'shtick", 1, log: false);
                Core.EnsureComplete(1682);
                Core.Logger($"Fishing Dynamite: {Bot.Inventory.GetQuantity("Fishing Dynamite")}/{FishingDynamiteQuant}");
            }
        }

        Core.Logger("Returing to Fishing Map");
        Core.Join("fishing");
    }

    public void DeathPitBrawlREP(int rank = 10)
    {
        if (FactionRank("Death Pit Brawl") >= rank)
            return;

        Core.AddDrop("Death Pit Token");
        Core.EquipClass(ClassType.Solo);
        ToggleBoost(BoostType.Reputation);
        Core.ToggleAggro(false);

        Core.Logger($"Farming rank {rank}");

        if (!Core.isCompletedBefore(5155))
        {
            Core.EnsureAccept(5155);
            DeathPitToken();
            Core.EnsureComplete(5155);
        }
        if (!Core.isCompletedBefore(5156))
        {
            Core.EnsureAccept(5156);
            Core.EnsureComplete(5156);
        }
        Core.RegisterQuests(5156);
        if (!Core.isCompletedBefore(5157))
        {
            Core.EnsureAccept(5157);
            RunDeathPitBrawl();
            Core.EnsureComplete(5157);
        }
        Core.RegisterQuests(5157, 5165);

        while (!Bot.ShouldExit && FactionRank("Death Pit Brawl") < rank)
        {
            Core.ToggleAggro(false);
            RunDeathPitBrawl();
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.ToggleAggro(true);
    }

    void RunDeathPitBrawl(string item = "Death Pit Token")
    {
        while (Bot.Map.Name != "deathpitbrawl")
        {
            Core.Logger("Joining Brawl");
            Core.Join("DeathPitbrawl", "Enter0", "Spawn");
            Bot.Sleep(Core.ActionDelay);
        }

        int Move = 1;
        Core.PvPMove(5, "Morale0C", 228, 291);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(4, "Morale0B", 936, 397);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(7, "Morale0A", 946, 394);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(9, "Crosslower", 948, 400);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(14, "Crossupper", 903, 324);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(18, "Resource1A", 482, 295);
        Core.Logger($"Move: {Move++}, Restorers");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(9);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit(20);
            Bot.Kill.Monster(10);
            Bot.Wait.ForCombatExit(20);
            Bot.Combat.CancelTarget();
            if (!Core.IsMonsterAlive("Velm's Restorer"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("Velm's Restorers killed.");
                break;
            }
        }

        Core.PvPMove(20, "Resource1B", 938, 400);
        Core.Logger($"Move: {Move++}, Restorers");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(11);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit(20);
            Bot.Kill.Monster(12);
            Bot.Wait.ForCombatExit(20);
            Bot.Combat.CancelTarget();
            if (!Core.IsMonsterAlive("Velm's Restorer"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("Velm's Restorers killed.");
                break;
            }
        }


        Core.PvPMove(21, "Resource1A", 9, 435);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(19, "Crossupper", 461, 315);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(17, "Crosslower", 54, 339);
        Core.Logger($"Move: {Move++}");
        Core.PvPMove(15, "Morale1A", 522, 286);
        Core.Logger($"Move: {Move++}, Velm's Brawler");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(13);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit(20);
            if (!Core.IsMonsterAlive("Velm's Brawler"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("Velm's Brawlers killed.");
                break;
            }
        }


        Core.PvPMove(23, "Morale1B", 948, 403);
        Core.Logger($"Move: {Move++}, Velm's Brawler");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(14);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit(20);
            if (!Core.IsMonsterAlive("Velm's Brawler"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("Velm's Brawlers killed.");
                break;
            }
        }


        Core.PvPMove(25, "Morale1C", 945, 397);
        Core.Logger($"Move: {Move++}, Velm's Brawler");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(15);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit(20);
            if (!Core.IsMonsterAlive("Velm's Brawler"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("Velm's Brawlers killed.");
                break;
            }
        }


        Core.PvPMove(28, "Captain1", 943, 404);
        Core.Logger($"Move: {Move++}, General Velm (B)");

        while (!Bot.ShouldExit)
        {
            Bot.Kill.Monster(16);
            Bot.Combat.CancelTarget();
            Bot.Wait.ForCombatExit();
            if (!Core.IsMonsterAlive("General Velm (B)"))
            {
                Bot.Combat.CancelTarget();
                Core.Logger("General Velm (B) killed.");
                break;
            }
        }

        Bot.Sleep(5000);

        Bot.Wait.ForDrop(item, 40);
        Bot.Wait.ForPickup(item);

        Core.Logger("Delaying exit");
        Bot.Sleep(7500);

        while (Bot.Map.Name != "battleon")
        {
            int i = 0;
            Core.Logger($"Attemping Exit {i++}.");
            Bot.Map.Join("battleon-999999");
            Bot.Sleep(1500);
        }
    }

    public void DeathPitToken(string item = "Death Pit Token", int quant = 30, bool isTemp = false)
    {
        // Do not call this with registered quests, or it technically never exits.
        if (Core.CheckInventory(item, quant))
            return;
        Core.Logger($"Farming {quant} {item}");
        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
            RunDeathPitBrawl();
    }

    public void FaerieCourtREP(int rank = 10) // Seasonal
    {
        if (FactionRank("Faerie Court") >= rank)
            return;

        Core.JumpWait();
        Bot.Map.Join("rainbow");
        if (Bot.Map.Name != "rainbow")
        {
            Core.Logger("Can't level FaerieCourt, as it's seasonal");
            return;
        }

        Core.Logger($"Farming rank {rank}");
        ToggleBoost(BoostType.Reputation);
        Core.SavedState();

        Core.RegisterQuests(6775, 6779);
        while (!Bot.ShouldExit && FactionRank("Faerie Court") < rank)
        {
            if (FactionRank("Faerie Court") < 8)
            {
                Core.EquipClass(ClassType.Solo);
                Core.HuntMonster("rainbow", "Lucky Harms", "Four Leaf Clover", 3, log: false);
            }
            if (FactionRank("Faerie Court") >= 8)
            {
                Core.EquipClass(ClassType.Farm);
                Core.HuntMonster("faegrove", "Dark Sylphdrake", "Silver Sylph Feather", log: false);
            }
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void GlaceraREP(int rank = 10)
    {
        if (FactionRank("Glacera") >= rank)
            return;

        if (!Core.isCompletedBefore(5601))
        {
            Core.Logger("Farming Quests are not unlocked, Please run: \"Story/Glacera.cs\"");
            return;
        }

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        if (Core.FarmClass == "Generic")
            Core.Logger("FarmClass not set in CBO (options > corebot > tab 2),/n" +
            "so you'll be getting *very* low Rep Rates without a multi-target class.");

        Core.RegisterQuests(5597, 5598, 5599, 5600);
        while (!Bot.ShouldExit && FactionRank("Glacera") < rank)
        {
            while (!Bot.ShouldExit && !Core.CheckInventory("World Ender Medal", 10)
            && !Core.CheckInventory("Mega World Ender Medal", 5))
                Core.HuntMonster("icewindwar", "Frost Invader", log: false);

            while (!Bot.ShouldExit && !Core.CheckInventory("Frostspawn Medal", 10)
            && !Core.CheckInventory("Mega Frostspawn Medal", 5))
                Core.HuntMonster("icewindwar", "Glaceran Defender", log: false);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void GoodREP(int rank = 10)
    {
        if (FactionRank("Good") >= rank)
            return;

        Core.ChangeAlignment(Alignment.Good);
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(369); //That Hero Who Chases Slimes 369
        while (!Bot.ShouldExit && FactionRank("Good") < 4)
            Core.KillMonster("swordhavenbridge", "Bridge", "Left", "Slime", "Slime in a Jar", 6, log: false);

        Core.CancelRegisteredQuests();
        Core.RegisterQuests(Core.IsMember ? 371 : 372); //Rumble with Grumble 371, Tomb with a View 372
        while (!Bot.ShouldExit && FactionRank("Good") < rank)
            if (!Core.IsMember)
                Core.KillMonster("castleundead", "Enter", "Spawn", "*", log: false);
            else
                Core.HuntMonster("sewer", "Grumble", "Grumble's Fang", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);

    }

    public void HollowbornREP(int rank = 10)
    {
        if (FactionRank("Hollowborn") >= rank)
            return;

        Core.AddDrop("Hollow Soul");
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        //rep boost type crashes atm
        // ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        // Core.RegisterQuests(7553, 7555); //Get the Seeds 7553, Flex it! 7555
        while (!Bot.ShouldExit && FactionRank("Hollowborn") < rank)
        {
            Core.EnsureAccept(7553, 7555);
            Core.KillMonster("shadowrealm", "r2", "Left", "Gargrowl", "Darkseed", 8, log: false);
            Core.KillMonster("shadowrealm", "r2", "Left", "Shadow Guardian", "Shadow Medallion", 5, log: false);
            Core.EnsureComplete(7553); //Get the Seeds 7553
            Core.EnsureComplete(7555); // Flex it! 7555
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void HorcREP(int rank = 10)
    {
        if (FactionRank("Horc") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Horc, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(1265);
            while (!Bot.ShouldExit && FactionRank("Horc") < rank)
            {
                Core.EnsureAccept(1265);
                Core.HuntMonster("bloodtuskwar", "Chaotic Lemurphant", "Chaorrupted Eye", 3, log: false);
                Core.HuntMonster("bloodtuskwar", "Chaotic Horcboar", "Chaorrupted Tentacle", 5, log: false);
                Core.HuntMonster("bloodtuskwar", "Chaotic Chinchilizard", "Chaorrupted Tusk", 5, log: false);
                Core.EnsureComplete(1265);
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void LoremasterREP(int rank = 10)
    {
        if (FactionRank("Loremaster") >= rank)
            return;

        Experience(15);
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.EquipClass(ClassType.Farm);
        Core.RegisterQuests(7505); //Studying the Rogue 7505
        while (!Bot.ShouldExit && FactionRank("Loremaster") < rank)
        {

            if (Core.IsMember && FactionRank("Loremaster") >= 3)
                LoremasterREPAbove3();
            else
            {
                Core.HuntMonster("wardwarf", "Drow Assassin", "Poisoned Dagger", 4, log: false);
                Core.HuntMonster("wardwarf", "D'wain Jonsen", "Scroll: Opportunity's Strike", log: false);
            }
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);

        void LoremasterREPAbove3()
        {
            if (!Core.isCompletedBefore(3032)) //Need boat for this questsline (member only)
            {
                Core.Logger("Unlocking farming quest.");
                Core.EnsureAccept(3029); //Rosetta Stones 3029
                Core.HuntMonster("druids", "Void Bear", "Voidstone", 6, log: false);
                Core.EnsureComplete(3029);

                Core.EnsureAccept(3030); // Cull the Foot Soldiers 3030
                Core.HuntMonster("druids", "Void Larva", "Void Larvae Death Cry", 4, log: false);
                Core.EnsureComplete(3030);

                Core.EnsureAccept(3031); // Bad Vibes 3031
                Core.HuntMonster("druids", "Void Ghast", "Ghast's Death Cry", 4, log: false);
                Core.EnsureComplete(3031);
            }

            Core.EquipClass(ClassType.Solo);
            Core.RegisterQuests(3032); //Quite the Problem 3032
            while (!Bot.ShouldExit && FactionRank("Loremaster") < rank)
                Core.HuntMonster("druids", "Young Void Giant", log: false);
        }
    }

    public void LycanREP(int rank = 10)
    {
        if (FactionRank("Lycan") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Lycan, rank);
        // else
        {
            if (!Core.isCompletedBefore(537))
            {
                Core.Logger("Can't do farming quest [Sanguine] (/lycan)", messageBox: true);
                return;
            }

            Core.EquipClass(ClassType.Solo);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(537); //Sanguine 537
            while (!Bot.ShouldExit && FactionRank("Lycan") < rank)
                Core.HuntMonster("lycan", "Sanguine", "Sanguine Mask", log: false);
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void InfernalArmyREP(int rank = 10)
    {
        if (FactionRank("Infernal Army") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5707); // Practice Time 5707
        while (!Bot.ShouldExit && FactionRank("Infernal Army") < rank)
            Core.KillMonster("dreadfire", "r10", "Left", "Living Brimstone", "Living Brimstone Defeated", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void MonsterHunterREP(int rank = 10)
    {
        if (FactionRank("Monster Hunter") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5849, 5850); //Capture the Misshapen 5849, Defeat the Parasites 5850
        if (!Core.isCompletedBefore(5850))
        {
            Core.Logger("Unlocking farming quest.");
            Core.KillMonster("pilgrimage", "r5", "Left", "SpiderWing", "Spiderwing Captured", 4, log: false);
            Core.KillMonster("pilgrimage", "r5", "Left", "Urstrix", "Urstrix Captured", 4, log: false);
        }

        while (!Bot.ShouldExit && FactionRank("Monster Hunter") < rank)
        {
            Core.KillMonster("pilgrimage", "r5", "Left", "SpiderWing", "Spiderwing Captured", 4, log: false);
            Core.KillMonster("pilgrimage", "r5", "Left", "Urstrix", "Urstrix Captured", 4, log: false);
            Core.KillMonster("pilgrimage", "r5", "Left", "Ravenous Parasite", "Ravenous Parasites Slain", 7, log: false);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void MysteriousDungeonREP(int rank = 10)
    {
        if (FactionRank("Mysterious Dungeon") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        if (!Core.isCompletedBefore(5429))
        {
            Core.EnsureAccept(5428);
            Core.GetMapItem(4803, 1, "cursedshop");
            Core.EnsureComplete(5428);
        }

        Core.RegisterQuests(5429); //Lamps, Paintings and Chairs, oh my! 5429
        while (!Bot.ShouldExit && FactionRank("Mysterious Dungeon") < rank)
            Core.HuntMonster("cursedshop", "Antique Chair", "Antique Chair Defeated", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void MythsongREP(int rank = 10)
    {
        if (FactionRank("Mythsong") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Mythsong, rank);
        // else

        if (!Bot.Quests.IsUnlocked(4829))
        {
            Core.Logger("Can't do farming quest (Do Lord of Chaos Kimberly)", messageBox: true);
            return;
        }

        Core.EquipClass(ClassType.Solo);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(4829); //Sugar, Sugar 4829
        while (!Bot.ShouldExit && FactionRank("Mythsong") < rank)
            Core.HuntMonster("beehive", "Stinger", "Honey Gathered", 10, log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void NecroCryptREP(int rank = 10)
    {
        if (FactionRank("Necro Crypt") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(3048); //Help Professor Mueran 3048
        while (!Bot.ShouldExit && FactionRank("Necro Crypt") < rank)
            Core.HuntMonster("castleundead", "Skeletal Viking", "Old Bone", 25, log: false);
        Bot.Wait.ForQuestComplete(3048);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void NorthpointeREP(int rank = 10)
    {
        if (FactionRank("Northpointe") >= rank)
            return;
        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Northpointe, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            ToggleBoost(BoostType.Reputation);
            Core.SavedState();
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(4027); //Sage Advice 4027
            while (!Bot.ShouldExit && FactionRank("Northpointe") < rank)
                Core.HuntMonster("northpointe", "Grim Stalker", "Bunch of Sage", 10, log: false);
            Bot.Wait.ForQuestComplete(4027);
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void PetTamerREP(int rank = 10)
    {
        if (FactionRank("Pet Tamer") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(5261);
        while (!Bot.ShouldExit && FactionRank("Pet Tamer") < rank)
            Core.KillMonster("greenguardwest", "West7", "Down", "Mogzard", "Mogzard Captured", log: false);
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void RavenlossREP(int rank = 10)
    {
        if (FactionRank("Ravenloss") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Ravenloss, rank);
        // else
        {
            if (!Bot.Quests.IsAvailable(3445))
            {
                Core.Logger("Quest Locked Run: \"Story/RavenlossSaga.cs\"", messageBox: true);
                return;
            }
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(3445); //Slay the Spiderkin 3445
            while (!Bot.ShouldExit && FactionRank("Ravenloss") < rank)
                Core.HuntMonster("twilightedge", "ChaosWeaver Mage", "ChaosWeaver Slain", 10, log: false);
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void SandseaREP(int rank = 10)
    {
        if (FactionRank("Sandsea") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Sandsea, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(916, 917, 919, 921, 922); //Dissertations Bupers Camel 916, Crafty Creepers: A Favorite of Mine 917, Parched Pets 919, Oasis Ornaments 921, The Power of Pomade 922
            while (!Bot.ShouldExit && FactionRank("Sandsea") < rank)
            {
                Core.EnsureAccept(916, 917, 919, 921, 922);
                Core.HuntMonster("sandsea", "Bupers Camel", "Bupers Camel Document", 10, log: false);
                Core.HuntMonster("sandsea", "Bupers Camel", "Barrel of Desert Water", 10, log: false);
                Core.HuntMonster("sandsea", "Bupers Camel", "Flexible Camel Spit", 7, log: false);
                Core.HuntMonster("sandsea", "Bupers Camel", "Oasis Jewelry Piece", 4, log: false);
                Core.HuntMonster("sandsea", "Bupers Camel", "Camel Skull", 2, log: false);
                Core.HuntMonster("sandsea", "Cactus Creeper", "Sandsea Cotton", 8, log: false);
                Core.HuntMonster("sandsea", "Cactus Creeper", "Cactus Creeper Head", 8, log: false);
                Core.EnsureComplete(916, 917, 919, 921, 922);
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void SkyguardREP(int rank = 10)
    {
        if (FactionRank("Skyguard") >= rank)
            return;

        if (!Core.IsMember)
        {
            Core.Logger("Skyguard REP is Member-Only", messageBox: true);
            return;
        }

        MembershipDues(MemberShipsIDS.Skyguard, rank);

        //Core.RegisterQuests(1016);
        //while (!Bot.ShouldExit && FactionRank("Skyguard") < rank)
        //{
        //    Core.HuntMonster("gilead", "Water Elemental", "Bucket of Water", 5, log: false);
        //    Core.HuntMonster("gilead", "Wind Elemental", "Beaker of Air", 5, log: false);
        //}
        //Core.CancelRegisteredQuests();
    }

    public void SomniaREP(int rank = 10)
    {
        if (FactionRank("Somnia") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        // Core.RegisterQuests(7665, 7666, 7669);
        while (!Bot.ShouldExit && FactionRank("Somnia") < rank)
        {
            Core.EnsureAccept(7665, 7666, 7669);
            Core.HuntMonster("somnia", "Nightspore", "Dream Truffle", 8, log: false);
            Core.HuntMonster("somnia", "Orpheum Elemental", "Orphium Ore", 8, log: false);
            Core.HuntMonster("somnia", "Dream Larva", "Dreamsilk", 5, log: false);
            Core.EnsureComplete(7665, 7666, 7669);
        }
        // Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void SpellCraftingREP(int rank = 10)
    {
        if (FactionRank("SpellCrafting") >= rank)
            return;

        Core.AddDrop("Mystic Quills", "Mystic Parchment");
        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        if (FactionRank("SpellCrafting") == 0)
        {
            Core.EnsureAccept(2260);
            Core.GetMapItem(1920, 1, "dragonrune");
            Core.HuntMonster("castleundead", "Skeletal Warrior", "Arcane Parchment", 13, log: false);
            Core.EnsureComplete(2260);
        }

        if (FactionRank("SpellCrafting") < 4)
        {
            Core.HuntMonster("mobius", "Slugfit", "Mystic Quills", 10, false);
            Core.BuyItem("dragonrune", 549, "Ember Ink", 50);
            while (!Bot.ShouldExit && Core.CheckInventory("Ember Ink") && FactionRank("SpellCrafting") < 4)
                Core.ChainComplete(2299);
        }
        while (!Bot.ShouldExit && FactionRank("SpellCrafting") < rank)
        {
            Core.HuntMonster("underworld", "Skull Warrior", "Mystic Parchment", 10, false);
            Core.BuyItem("dragonrune", 549, "Hallow Ink", 50);
            while (!Bot.ShouldExit && Core.CheckInventory("Hallow Ink") && FactionRank("SpellCrafting") < rank)
                Core.ChainComplete(2322);
        }
        Core.SellItem("Ember Ink", all: true);
        Core.SellItem("Hallow Ink", all: true);

        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void SwordhavenREP(int rank = 10)
    {
        if (FactionRank("Swordhaven") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Swordhaven, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(3065, 3066, 3067, 3070, 3085, 3086, 3087);
            while (!Bot.ShouldExit && FactionRank("Swordhaven") < rank)
            {
                Core.EnsureAccept(3065, 3066, 3067, 3070, 3085, 3086, 3087);
                Core.HuntMonster("castle", "Castle Spider", "Eradicated Arachnid", 10, log: false);
                Core.HuntMonster("castle", "Castle Spider", "Castle Spider Silk", 8, log: false);
                Core.HuntMonster("castle", "Castle Spider", "Castle Spider Silk Yarn", 2, log: false);
                Core.HuntMonster("castle", "Castle Wraith", "Castle Wraith Defeated", 10, log: false);
                Core.HuntMonster("castle", "Castle Wraith", "Jarred Wraith", 5, log: false);
                Core.HuntMonster("castle", "Castle Wraith", "Castle Wraith Wool", 2, log: false);
                Core.HuntMonster("castle", "Gargoyle", "Stony Plating", 6, log: false);
                Core.HuntMonster("castle", "Gargoyle", "Gargoyle Gems", 2, log: false);
                Core.HuntMonster("castle", "Dungeon Fiend", "Dungeon Fiend Hair Bow", 5, log: false);
                Core.HuntMonster("castle", "Dungeon Fiend", "Dungeon Fiend Bow Tie", 5, log: false);
                Core.HuntMonster("castle", "Dungeon Fiend", "Dungeon Fiend Textiles", 2, log: false);
                Core.EnsureComplete(new[] { 3065, 3066, 3067, 3070, 3085, 3086, 3087 });
            }
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void ThunderForgeREP(int rank = 10)
    {
        if (FactionRank("ThunderForge") >= rank)
            return;
        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Thunderforge, rank);
        // else
        {
            if (!Bot.Quests.IsAvailable(2733))
            {
                Core.Logger("Quest not avaible for farm, do ThunderForge saga till Deathpits [The Chaos Eye of Vestis]");
                return;
            }
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");
            if (!Core.IsMember)
            {
                Core.EquipClass(ClassType.Solo);
                Core.RegisterQuests(2733, 2734);
                while (!Bot.ShouldExit && FactionRank("ThunderForge") < rank)
                    Core.HuntMonster("deathpits", "Wrathful Vestis", "Vestis's Chaos Eye", log: false);
            }
            else
            {
                Core.EquipClass(ClassType.Farm);
                Core.RegisterQuests(2734, 2735, 2736, 2737);
                while (!Bot.ShouldExit && FactionRank("ThunderForge") < rank)
                    Core.HuntMonster("deathpits", "Rotting Darkblood", log: false);
            }
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void TreasureHunterREP(int rank = 10)
    {
        if (FactionRank("TreasureHunter") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(6593);
        while (!Bot.ShouldExit && FactionRank("TreasureHunter") < rank)
        {
            Core.HuntMonster("stalagbite", "Balboa", "Super Specific Rock", log: false);
            Bot.Wait.ForQuestComplete(6593);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void TrollREP(int rank = 10)
    {
        if (FactionRank("Troll") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Troll, rank);
        // else
        {
            Core.EquipClass(ClassType.Farm);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            // Core.RegisterQuests(1263);
            while (!Bot.ShouldExit && FactionRank("Troll") < rank)
            {
                Core.EnsureAccept(1263);
                Core.HuntMonster("bloodtuskwar", "Chaotic Lemurphant", "Chaorrupted Eye", 3, log: false);
                Core.HuntMonster("bloodtuskwar", "Chaotic Horcboar", "Chaorrupted Tentacle", 5, log: false);
                Core.HuntMonster("bloodtuskwar", "Chaotic Chinchilizard", "Chaorrupted Tusk", 5, log: false);
                Core.EnsureComplete(1263);
            }
            // Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void VampireREP(int rank = 10)
    {
        if (FactionRank("Vampire") >= rank)
            return;

        // if (Core.IsMember)
        //     MembershipDues(MemberShipsIDS.Vampire, rank);
        // else
        {
            if (!Bot.Quests.IsUnlocked(522))
            {
                Core.Logger("Can't do farming quest [Twisted Paw] (/safiria)", messageBox: true);
                return;
            }
            Core.EquipClass(ClassType.Solo);
            Core.SavedState();
            ToggleBoost(BoostType.Reputation);
            Core.Logger($"Farming rank {rank}");

            Core.RegisterQuests(522);
            Core.RemoveDrop("Old Moon");
            while (!Bot.ShouldExit && FactionRank("Vampire") < rank)
            {
                Core.HuntMonster("safiria", "Twisted Paw", "Twisted Paw's Head", log: false);
                Bot.Wait.ForQuestComplete(522);
            }
            Core.CancelRegisteredQuests();
            ToggleBoost(BoostType.Reputation, false);
            Core.SavedState(false);
        }
    }

    public void YokaiREP(int rank = 10)
    {
        if (FactionRank("Yokai") >= rank)
            return;

        Core.EquipClass(ClassType.Farm);
        ToggleBoost(BoostType.Reputation);
        Core.SavedState();
        Core.Logger($"Farming rank {rank}");

        Core.RegisterQuests(383);
        while (!Bot.ShouldExit && FactionRank("Yokai") < rank)
        {
            Core.HuntMonster("dragonkoiz", "Pockey Chew", "Piece of Pockey", 3, log: false);
            Bot.Wait.ForQuestComplete(383);
        }
        Core.CancelRegisteredQuests();
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    public void SwagTokenA(int quant = 100)
    {
        if (!Core.IsMember || Core.CheckInventory("Super-Fan Swag Token A", quant))
            return;

        Core.AddDrop("Super-Fan Swag Token A", "Super-Fan Swag Token B", "Super-Fan Swag Token C");
        Core.EquipClass(ClassType.Farm);

        while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token A", quant))
        {
            Core.FarmingLogger($"Super-Fan Swag Token A", quant);

            /*
            //uncomment after 1.2.4 release:
             while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token C", 200))
              Core.KillMonster("collectorlab", "r2", "Right", "*", log: false);
            */

            //Leave this for 1.2.3 till 1.2.4 comes out.
            Core.RegisterQuests(1310, 1312, 1313, 1314);
            Core.Logger("without a farm class / multi-target class some mobs may not be killed --sorry");
            while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token C", 200))
                Core.KillMonster("collectorlab", "r2", "Left", "*", log: false);
            Core.CancelRegisteredQuests();

            Core.Join("Collection", "Begin", "Spawn");
            Core.BuyItem("collection", 325, "Super-Fan Swag Token B", 20);
            Bot.Shops.Load(325);
            Bot.Shops.BuyItem("Super-Fan Swag Token A");
        }
        Core.CancelRegisteredQuests();
    }

    public void SwagTokenAF2p(int quant = 100)
    {
        if (Core.CheckInventory("Super-Fan Swag Token A", quant))
            return;

        Core.AddDrop("Super-Fan Swag Token A", "Super-Fan Swag Token B", "Super-Fan Swag Token C");
        Core.EquipClass(ClassType.Farm);

        Core.RegisterQuests(1304, 1307);
        Core.FarmingLogger($"Super-Fan Swag Token A", quant);
        while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token A", quant))
        {
            Core.KillMonster("terrarium", "Enter", "Spawn", "Dustbunny of DOOM", "Super-Fan Swag Token D", 500, isTemp: false);

            //Check if shop is loaded into Cache
            Core.Join("Collection", "Begin", "Spawn");
            if (!Bot.Shops.IsLoaded)
                Bot.Shops.Load(325);

            //Token Buying
            while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token C", 10) && Core.CheckInventory("Super-Fan Swag Token D", 10))
                Bot.Shops.BuyItem("Super-Fan Swag Token C");
            while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token B", 20) && Core.CheckInventory("Super-Fan Swag Token C", 10))
                Bot.Shops.BuyItem("Super-Fan Swag Token B");
            while (!Bot.ShouldExit && !Core.CheckInventory("Super-Fan Swag Token A", quant) && Core.CheckInventory("Super-Fan Swag Token B", 20))
                Bot.Shops.BuyItem("Super-Fan Swag Token A");
        }
        Core.CancelRegisteredQuests();
    }

    public void MembershipDues(MemberShipsIDS faction, int rank = 10)
    {
        if (FactionRank(faction.ToString()) >= rank)
            return;

        Core.SavedState();
        ToggleBoost(BoostType.Reputation);
        int i = 1;
        while (FactionRank($"{faction}") < 10)
        {
            if (Core.IsMember)
                SwagTokenA(1);
            else SwagTokenAF2p(1);
            Core.ChainComplete((int)faction);
            Core.Logger($"Completed x{i++}");
        }
        ToggleBoost(BoostType.Reputation, false);
        Core.SavedState(false);
    }

    private void UndeadGiantUnlock()
    {
        Core.Logger("Checking if farming quest is unlocked.");
        if (!Core.isCompletedBefore(178))
        {
            Core.EnsureAccept(183);
            Core.HuntMonster("portalundead", "Skeletal Fire Mage", "Defeated Fire Mage", 4, log: false);
            Core.EnsureComplete(183);
            Core.EnsureAccept(176);
            Core.HuntMonster("swordhavenundead", "Skeletal Soldier", "Slain Skeletal Soldier", 10, log: false);
            Core.EnsureComplete(176);
            Core.EnsureAccept(177);
            Core.HuntMonster("swordhavenundead", "Skeletal Ice Mage", "Frozen Bonehead", 8, log: false);
            Core.EnsureComplete(177);
        }
    }

    public void GetBoost(int itemID, string boostName, int BoostQuant, int quest, bool doOnce = false)
    {
        ItemBase? boostItem = Core.EnsureLoad(quest)?.Rewards.Find(x => x.Name == boostName);

        if (boostItem != null && Core.CheckInventory(boostItem?.Name, BoostQuant) && !doOnce) return;

        if (FactionRank("Fishing") < 2)
            FishingREP(2, false, false, false);
        else
        {
            Core.Logger("Pre-Ranking XP(This is Required)");
            Core.EnsureAccept(1682);
            Core.KillMonster("greenguardwest", "West4", "Right", "Slime", "Faith's Fi'shtick", 1, log: false);
            Core.EnsureComplete(1682);
        }

        Core.FarmingLogger(boostName, BoostQuant); // Use boostName directly
        Core.AddDrop("Fishing Dynamite", boostItem?.Name ?? "DefaultItemName");
        Core.EquipClass(ClassType.Farm);

        string itemName = boostItem?.Name ?? (Core.EnsureLoad(quest)?.Rewards.Find(x => x.ID == itemID)?.Name ?? "DefaultItemName");
        while (!Bot.ShouldExit && (boostItem == null || (boostItem != null && !Core.CheckInventory(boostItem?.Name, BoostQuant)) || doOnce))
        {
            // Unlock the quest if not completed before (only for quest 1615)
            if (!Core.isCompletedBefore(1615))
            {
                Core.EnsureAccept(1614);
                GetFish(10850, 30, 1614);
                Core.HuntMonster("swordhaven", "Slime", "Slime Sauce", log: false);
                Core.EnsureComplete(1614);
            }

            Core.EnsureAccept(quest);

            GetFish(itemID, quest == 1614 ? 30 : 5, quest);

            if (quest == 1614)
                Core.KillMonster("greenguardwest", "West4", "Right", "Slime", "Slime Sauce");
            else if (quest == 1615)
                Core.HuntMonster("Greenguardwest", "Frogzard", "Greenguard Seal", log: false);
            Bot.Wait.ForPickup(quest == 1614 ? "Slime Sauce" : "Greenguard Seal");

            Core.EnsureComplete(quest);
        }

        if (!doOnce) Core.TrashCan(new[] { "Fishing Bait", "Fishing Dynamite" });
        Core.CancelRegisteredQuests();
    }

    void GetFish(int itemID, int quant, int quest)
    {
        if (Core.CheckInventory(itemID, quant)) return;

        ItemBase? reward = Core.EnsureLoad(quest)?.Rewards.Find(x => x.ID == itemID);
        if (reward != null)
            Core.FarmingLogger(reward.Name, quant);

        while (!Bot.ShouldExit && !Core.CheckInventory(itemID, quant))
        {
            if (!Core.CheckInventory("Fishing Dynamite"))
                GetBaitandDynamite(0, 20);

            Core.Join("fishing");

            while (!Bot.ShouldExit && Core.CheckInventory("Fishing Dynamite") && !Core.CheckInventory(itemID, quant))
            {
                int CurrentDynamiteQuant = Bot.Inventory.GetQuantity("Fishing Dynamite");
                Bot.Send.Packet($"%xt%zm%FishCast%1%Dynamite%30%");
                Bot.Sleep(3500);
                Bot.Wait.ForTrue(() => CurrentDynamiteQuant == CurrentDynamiteQuant - 1, 20);
                Core.SendPackets("%xt%zm%getFish%1%false");
                Core.Logger($"Dynamite: {Bot.Inventory.GetQuantity("Fishing Dynamite")} Fish: {Bot.TempInv.GetQuantity(itemID)}/{quant}");
            }
        }
    }

    public int FactionRank(string faction) => Bot.Reputation.GetRank(faction);

    #endregion
}

public enum BoostIDs
{
    DailyXP60 = 19189,
    XP20 = 22448,
    XP60 = 27552,
    DoomClass60 = 19761,
    Class20 = 22447,
    Class60 = 27555,
    DoomREP60 = 19762,
    REP20 = 22449,
    REP60 = 27553,
    DoomGold60 = 19763,
    Gold20 = 22450,
    Gold60 = 27554
}

public enum AlchemyRunes
{
    Dragon,
    Jera,
    Uruz,
    Fehu,
    Gebo
}

public enum MemberShipsIDS
{
    Dwarfhold = 1317,
    Good = 1318,
    Evil = 1319,
    Yokai = 1320,
    Vampire = 1321,
    Lycan = 1322,
    Mythsong = 1323,
    Arcangrove = 1324,
    Sandsea = 1325,
    Skyguard = 1326,
    DoomWood = 1327,
    Troll = 1328,
    Horc = 1329,
    Etherstorm = 4340,
    ChronoSpan = 4341,
    Thunderforge = 4342,
    Swordhaven = 4343,
    Chaos = 4344,
    Northpointe = 4345,
    Embersea = 4346,
    Ravenloss = 4347
}
