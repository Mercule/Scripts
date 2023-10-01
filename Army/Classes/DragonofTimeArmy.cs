/*
name: Dragon of Time Army
description: sues an army to get the dragon of time class
tags: dragon of time, class, army
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/Good/BLoD/CoreBLOD.cs
//cs_include Scripts/Darkon/CoreDarkon.cs
//cs_include Scripts/Other/Weapons/GoldenBladeOfFate.cs
//cs_include Scripts/Other/Weapons/PinkBladeofDestruction.cs
//cs_include Scripts/Story/Doomwood/CoreDoomwood.cs
//cs_include Scripts/Story/QueenofMonsters/CoreQoM.cs
//cs_include Scripts/Story/ThroneofDarkness/CoreToD.cs
//cs_include Scripts/Story/7DeadlyDragons/Core7DD.cs
//cs_include Scripts/Other/MysteriousEgg.cs
//cs_include Scripts/Story/BattleUnder.cs
//cs_include Scripts/Story/Summer2015AdventureMap/CoreSummer.cs
//cs_include Scripts/Story/Borgars.cs
//cs_include Scripts/Story/ElegyofMadness(Darkon)/CoreAstravia.cs
//cs_include Scripts/Army/CoreArmyLite.cs
using Skua.Core.Interfaces;
using Skua.Core.Options;
using Skua.Core.Models.Items;
using Skua.Core.Models.Quests;
using Skua.Core.Models.Monsters;

public class DoTArmy
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new();
    public CoreAdvanced Adv = new();
    public CoreStory Story = new();
    public CoreDarkon Darkon = new();
    public GoldenBladeOfFate GBoF = new();
    public PinkBladeOfDestruciton PBoD = new();
    public CoreQOM QOM = new();
    public CoreToD TOD = new();
    public MysteriousEgg Egg = new();
    public CoreSummer Coll = new();
    public Borgars Borgars = new();
    private CoreArmyLite Army = new();

    private static CoreBots sCore = new();
    private static CoreArmyLite sArmy = new();


    public string OptionsStorage = "DoTArmy";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        new Option<bool>("sellToSync", "Sell to Sync", "Sell items to make sure the army stays syncronized.\nIf off, there is a higher chance your army might desyncornize", false),
        sArmy.player1,
        sArmy.player2,
        sArmy.player3,
        sArmy.player4,
        sArmy.player5,
        sArmy.player6,
        sArmy.packetDelay,
        CoreBots.Instance.SkipOptions
    };

    public string[] QuestRewards =
    {
        //Q1
        "Dragon of Time Helm",
        "Dragon of Time Ponytail",
        // Core.CheckInventory(QuestRewards[^2], toInv: false);
        //Q2
        "Dragon of Time Runes",
        "Dragon of Time Wings",
        "Dragon of Time Tail",
        // Core.CheckInventory(QuestRewards[^5], toInv: false);
        //Q3
        "Dragon of Time FangBlade",
        "Dual Dragon of Time FangBlades",
        // Core.CheckInventory(QuestRewards[^7], toInv: false);
        //Q4
        "Dragon of Time Armor",
        // Core.CheckInventory(QuestRewards[^8], toInv: false);
        //Q5
        "Dragon of Time Daggers",
        "Dragon of Time Cleaver",
        // Core.CheckInventory(QuestRewards[^10], toInv: false);
        //Q6
        "Ascended Dragon of Time Runes",
        "Runes Of Time",
        // Core.CheckInventory(QuestRewards[^12], toInv: false);
        //Q7
        "Dragon of Time Reaper",
        "Dragon of Time WingBlade",
        // Core.CheckInventory(QuestRewards[^14], toInv: false);
        //Q8
        "Ascended Dragon of Time",
        // Core.CheckInventory(QuestRewards[^15], toInv: false);
        //Q9
        "Dragon of Time",
        "Ascended Dragon of Time Morph",
        "Ascended Dragon of Time Wings",
        "Dragon of Time Horns",
        // Core.CheckInventory(QuestRewards[^19], toInv: false);
        //Q10/Extra Quest
        "Dragon of Time Horns + Ponytail",
        "Dragon of Time Wings + Tail"
        // Core.CheckInventory(QuestRewards[^22], toInv: false);
    };

    public string[] Quest1 =
    {
        "Lost Hieroglyphic",
        "Myths of Lore",
        "Historia Page",
        "Frost King's Story",
        "Your Own Memories"
    };

    public string[] Quest2 =
    {
      "Desoloth's Destructive Aura",
      "Nythera's Patience",
      "Goregold's Luck",
      "Victorious's Dignity",
      "Trigoras's Tenacity"
    };

    public string[] Quest3 =
    {
        "Golden Blade of Fate (Sword)",
        "Pink Blade of Destruction",
        "Cross-Era Stabilizer",
        "Chronomancer's Codex",
        "Timestream String"
    };

    public string[] Quest4 =
    {
        "Time Loop Broken",
        "Anomaly Silenced",
        "Chronolord Stopped",
        "Is This a Wormhole?"
    };

    public string[] Quest5 =
    {
        "Dimensional Dragon Portal",
        "Brutal Slash Studied",
        "Epic Hydra Fang"
    };

    public string[] Quest6 =
    {
        "Dimensional Dragon Portal",
        "Brutal Slash Studied",
        "Epic Hydra Fang"
    };

    public string[] Quest7 =
    {
        "Sword of Voids",
        "Darkon's Receipt",
        "Semiramis Feather",
        "Cross-Dimensional Weapons",
        "Starlight Singularity",
        "Collectible Collector"
    };

    public string[] Quest8 =
    {
        "Unyielding Slime",
        "Omnipotent Cells",
        "Dragon's Plasma",
        "Chaotic Invertebrae",
        "Cryostatic Essence",
        "Salvaged Chaos Dragon Biomass"
    };

    public string[] Quest9 =
    {
        "Fire Essence",
        "Akriloth's Flametongue",
        "Immortal Embers",
        "Ashes from the Void Realm"
    };

    public string[] Quest10 =
    {
        "Mysterious Egg",
        "Conquered Past",
        "Slugbutter Trophy",
        "Icewing's Laurel"
    };


    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(QuestRewards);
        Core.SetOptions(disableClassSwap: true);

        DoT(Bot.Config!.Get<bool>("sellToSync"));

        Core.SetOptions(false);
    }


    public void DoT(bool doExtra = true)
    {
        // if ((!doExtra && Core.CheckInventory("Dragon of Time")) || (doExtra && Core.CheckInventory(QuestRewards, toInv: false)))
        //     return;

        Core.OneTimeMessage("Only for army", "This is intended for use with an army, not for solo players.");

        Bot.Events.PlayerAFK += PlayerAFK;
        /*
        ********************************************************************************
        *****************************PREQuests Zone*************************************
        ********************************************************************************
        */
        Farm.LoremasterREP(4);
        GBoF.GetGBoF();
        PBoD.GetPBoD();
        Farm.Experience(75);
        Darkon.FarmReceipt(100);
        QOM.TheReshaper();
        Coll.Collector();
        TOD.ShiftingPyramid();
        if (!Core.CheckInventory("Blade of Awe"))
            Farm.BladeofAweREP(6, true);
        Egg.GetMysteriousEgg();
        Borgars.StoryLine();
        /*
        ********************************************************************************
        **********************************FINISH****************************************
        ********************************************************************************
        */

        Bot.Drops.Add(QuestRewards);
        DoQuest1();
        DoQuest2();
        DoQuest3();
        DoQuest4();
        DoQuest5();
        DoQuest6();
        DoQuest7();
        DoQuest8();
        DoQuest9();
        if (doExtra)
            DoQuest10();

        // List<string> PreQuestInv = Bot.Inventory.Items.Select(x => x.Name).ToList();
        // Quest QuestData = Core.EnsureLoad(7716);
        // Core.TrashCan(QuestData.Requirements.Where(x => !x.Temp).Select(y => y.Name).ToArray());
        // Core.ToBank(Bot.Inventory.Items.Select(x => x.Name).ToList().Except(PreQuestInv).ToArray());
    }

    public void DoQuest1()
    {
        if (Core.CheckInventory(QuestRewards[0..2], toInv: false))
            foreach (string reward in QuestRewards[0..2])
                Army.waitForParty("whitemap", reward);

        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[0..2], toInv: false))
        {
            // Acquiring Ancient Secrets 7716
            Core.EnsureAccept(7716);

            Bot.Quests.UpdateQuest(4614);
            Core.EquipClass(ClassType.Farm);
            ArmyHunt("mummies", new[] { "Mummy" }, "Lost Hieroglyphic", ClassType.Solo, false, 30);

            ArmyHunt("timelibrary", new[] { "Training Globe", "Tog", "Moglin Ghost" }, "Historia Page", ClassType.Solo, false, 100);

            ArmyHunt("kingcoal", new[] { "Frost King" }, "Frost King's Story", ClassType.Solo);

            Core.EquipClass(ClassType.Solo);
            Core.KillMonster("baconcatyou", "Enter", "Spawn", "*", "Your Own Memories", isTemp: false);

            Core.BuyItem("librarium", 651, "Myths of Lore");

            Core.EnsureComplete(7716);
            Core.Logger($"Quest 1: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest2()
    {
        if (Core.CheckInventory(QuestRewards[3..5], toInv: false))
            foreach (string reward in QuestRewards[3..5])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[3..5], toInv: false))
        {
            Core.EnsureAccept(7717);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("dragonchallenge", new[] { "Desoloth the Final" }, "Desoloth's Destructive Aura", ClassType.Solo);

            Bot.Quests.UpdateQuest(899);
            ArmyHunt("blindingsnow", new[] { "Nythera" }, "Nythera's Patience", ClassType.Solo);

            Core.AddDrop("Key of Greed");
            ArmyHunt("greed", new[] { "Goregold" }, "Goregold's Luck", ClassType.Solo);

            ArmyHunt("darkplane", new[] { "Victorious" }, "Victorious's Dignity", ClassType.Solo);

            ArmyHunt("trigoras", new[] { "Trigoras" }, "Trigoras's Tenacity", ClassType.Solo, false, 3);

            Core.EnsureComplete(7717);
            Core.Logger($"Quest 2: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest3()
    {
        if (Core.CheckInventory(QuestRewards[6..7], toInv: false))
            foreach (string reward in QuestRewards[6..7])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[6..7], toInv: false))
        {
            Core.EnsureAccept(7718);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("underworld", new[] { "Laken" }, "Cross-Era Stabilizer", ClassType.Solo);

            if (!Core.CheckInventory("Chronomancer's Codex"))
            {
                Core.EquipClass(ClassType.Solo);
                ArmyHunt("mqlesson", new[] { "Dragonoid" }, "Dragonoid of Hours", ClassType.Solo);
                Core.EquipClass(ClassType.Solo);
                ArmyHunt("timespace", new[] { "Chaos Lord Iadoa" }, "Chronomancer's Codex", ClassType.Solo);
            }

            ArmyHunt("arena", new[] { "Timestream Rider" }, "Timestream String", ClassType.Solo, false, 100);

            Core.EnsureComplete(7718);
            Core.Logger($"Quest 3: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest4()
    {
        if (Core.CheckInventory(QuestRewards[7..8], toInv: false))
            foreach (string reward in QuestRewards[7..8])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[7..8], toInv: false))
        {
            Core.EnsureAccept(7719);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("cathedral", new[] { "Incarnation of Time" }, "Time Loop Broken", ClassType.Solo);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("ubear", new[] { "Cornholio" }, "Is This a Wormhole?", ClassType.Solo);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("portalwar", new[] { "Chronorysa", "Tempus Larva", "Time Wraith" }, "Anomaly Silenced", ClassType.Solo, false, 100);

            Core.EquipClass(ClassType.Solo);
            ArmyHunt("portalmaze", new[] { "ChronoLord" }, "Chronolord Stopped", ClassType.Solo, false, 50);

            Core.EnsureComplete(7719);
            Core.Logger($"Quest 4: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest5()
    {
        if (Core.CheckInventory(QuestRewards[9..10], toInv: false))
            foreach (string reward in QuestRewards[9..10])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[9..10], toInv: false))
        {
            Core.EnsureAccept(7720);

            ArmyHunt("lairdefend", new[] { "Dragon Summoner" }, "Dimensional Dragon Portal", ClassType.Solo, false, 2);

            ArmyHunt("bosschallenge", new[] { "Grievous Inbunche" }, "Brutal Slash Studied", ClassType.Solo, false, 10);

            ArmyHunt("hydrachallenge", new[] { "Hydra Head 90" }, "Epic Hydra Fang", ClassType.Solo, false, 125);
            Core.EnsureComplete(7720);
            Core.Logger($"Quest 5: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest6()
    {
        if (Core.CheckInventory(QuestRewards[11..12], toInv: false))
            foreach (string reward in QuestRewards[11..12])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[11..12], toInv: false))
        {
            Core.EnsureAccept(7721);

            ArmyHunt("ivoliss", new[] { "Ivoliss" }, "Sword of Voids", ClassType.Solo);

            if (!Core.CheckInventory("Semiramis Feather"))
            {
                Core.AddDrop("Semiramis Feather");
                // Take Down Terrane 6286
                Core.EnsureAccept(6286);
                ArmyHunt("guardiantree", new[] { "Terrane" }, "Terrane Defeated", ClassType.Solo, isTemp: true);
                Core.EnsureComplete(6286);
                Bot.Wait.ForPickup("Semiramis Feather");
            }

            ArmyHunt("aqw3d", new[] { "Nightlocke Axe", "Nightlocke Blade", "Nightlocke Staff" }, "Cross-Dimensional Weapons", ClassType.Solo, false, 300);

            if (!Core.CheckInventory("Starlight Singularity"))
            {
                Core.AddDrop("Starlight Singularity");
                // Serpent of the Stars 5186
                Core.EnsureAccept(5186);
                ArmyHunt("whitehole", new[] { "Mehensi Serpent" }, "Mehen Slain", ClassType.Solo, isTemp: true);
                Core.EnsureComplete(5186);
                Bot.Wait.ForPickup("Starlight Singularity");
            }

            Core.BuyItem("collection", 325, "Collectible Collector");
            Bot.Wait.ForPickup("Collectible Collector");

            Core.EnsureComplete(7721);

            Core.Logger($"Quest 6: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest7()
    {
        if (Core.CheckInventory(QuestRewards[13..14], toInv: false))
            foreach (string reward in QuestRewards[13..14])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[13..14], toInv: false))
        {
            Core.EnsureAccept(7722);

            ArmyHunt("moonlab", new[] { "Slime Mold" }, "Unyielding Slime", ClassType.Farm, false, 300);

            ArmyHunt("bosschallenge", new[] { "Mutated Void Dragon" }, "Omnipotent Cells", ClassType.Solo, false, 20);

            ArmyHunt("underlair", new[] { "ArchFiend Dragonlord" }, "Dragon's Plasma", ClassType.Solo, false, 20);

            ArmyHunt("chaoskraken", new[] { "Chaos Kraken" }, "Chaotic Invertebrae", ClassType.Solo, false, 20);
            Bot.Quests.UpdateQuest(9, 159);

            ArmyHunt("towerofdoom9", new[] { "Dread Fang" }, "Cryostatic Essence", ClassType.Farm, false, 20);

            ArmyHunt("castleroof", new[] { "Ultra Chaos Dragon" }, "Salvaged Chaos Dragon Biomass", ClassType.Solo, false, 20);

            Core.EnsureComplete(7722);
            Core.Logger($"Quest 7: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest8()
    {
        if (Core.CheckInventory(QuestRewards[14..15], toInv: false))
            foreach (string reward in QuestRewards[14..15])
                Army.waitForParty("whitemap", reward);

        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[14..15], toInv: false))
        {

            Core.EnsureAccept(7723);

            ArmyHunt("volcano", new[] { "Fire Imp" }, "Fire Essence", ClassType.Farm, false, 3000);

            ArmyHunt("charredplains", new[] { "Akriloth" }, "Akriloth's Flametongue", ClassType.Solo, false, 100);

            ArmyHunt("ultraphedra", new[] { "Ultra Phedra" }, "Immortal Embers", ClassType.Solo, false, 50);

            ArmyHunt("thevoid", new[] { "Reaper" }, "Ashes from the Void Realm", ClassType.Solo, false, 50);

            Core.EnsureComplete(7723);
            Core.Logger($"Quest 8: 🖕");
            Bot.Wait.ForPickup("*");
        }
    }

    public void DoQuest9()
    {
        if (Core.CheckInventory(QuestRewards[16..19], toInv: false))
            foreach (string reward in QuestRewards[16..19])
                Army.waitForParty("whitemap", reward);


        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[16..19], toInv: false))
        {
            Core.EnsureAccept(7724);

            Bot.Quests.UpdateQuest(3880);
            ArmyHunt("chaoslord", 1770, "Conquered Past", ClassType.Solo, false);

            Bot.Quests.UpdateQuest(10, 159);
            ArmyHunt("towerofdoom10", new[] { "Slugbutter" }, "Slugbutter Trophy", ClassType.Solo, false, 100);

            ArmyHunt("icewing", new[] { "Warlord Icewing" }, "Icewing's Laurel", ClassType.Solo, false, 30);

            Core.EnsureComplete(7724);
            Core.Logger($"Quest 9: 🖕");
            Bot.Wait.ForPickup("Dragon of Time");
            Adv.RankUpClass("Dragon of Time");
        }
    }

    public void DoQuest10()
    {
        if (Core.CheckInventory(QuestRewards[20..22], toInv: false))
            foreach (string reward in QuestRewards[20..22])
                Army.waitForParty("whitemap", reward);

        else Core.Logger("Quest already complete / Items owned, butlering[hopefully]");

        List<string> PreQuestInv = Bot.Inventory.Items.Select(x => x.Name).ToList();

        while (!Bot.ShouldExit && !Core.CheckInventory(QuestRewards[20..22], toInv: false))
        {
            Core.EnsureAccept(7725);

            if (!Core.CheckInventory("Borgar"))
            {
                Core.AddDrop("Burger Buns");
                while (!Bot.ShouldExit && !Core.CheckInventory("Burger Buns", 5))
                {
                    // Burglinster's Revenge 7522
                    Core.EnsureAccept(7522);
                    ArmyHunt("borgars", new[] { "Burglinster" }, "Burglinster Cured", ClassType.Solo);
                    Core.EnsureComplete(7522);
                    Bot.Wait.ForPickup("Burger Buns");
                }
            }
            Core.BuyItem("borgars", 1884, 54650, shopItemID: 7387);

            Core.EnsureCompleteChoose(7725, QuestRewards);
            Bot.Wait.ForPickup("*");
            Core.ToBank(Bot.Inventory.Items.Select(x => x.Name).ToList().Except(PreQuestInv).ToArray());
        }
    }

    //                                   ▒▒▒▒▒▒▒▒▒▒▒▒▒▒░░                    
    //                               ▓▓▓▓████████████████▓▓▓▓▒▒              
    //                           ▓▓▓▓████░░░░░░░░░░░░░░░░██████▓▓            
    //                         ▓▓████░░░░░░░░░░░░░░░░░░░░░░░░░░████          
    //                       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██        
    //                     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██      
    //                   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██      
    //                 ▓▓██░░░░░░▓▓██░░  ░░░░░░░░░░░░░░░░░░░░▓▓██░░  ░░██    
    //               ▓▓██░░░░░░░░██████░░░░░░░░░░░░░░░░░░░░░░██████░░░░░░██  
    //               ▓▓██░░░░░░░░██████▓▓░░░░░░██░░░░██░░░░░░██████▓▓░░░░██  
    //             ▓▓██▒▒░░░░░░░░▓▓████▓▓░░░░░░████████░░░░░░▓▓████▓▓░░░░░░██
    //           ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░██░░░░██░░░░░░░░░░░░░░░░░░░░██
    //           ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //           ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //         ░░▓▓▒▒░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //         ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //         ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //         ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██
    //       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██    
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░Script Made for Potatos ░░░░░░░░░░░░░░██    
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██    
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██    
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██    
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //   ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //     ░░▓▓▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██  
    //       ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██░░  
    //         ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██    
    //           ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██      
    //           ▓▓██░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██        
    //             ▓▓████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██          
    //               ▓▓▓▓████████░░░░░░░░░░░░░░░░░░░░░░░░████████░░          
    //               ░░░░▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░   

    void ArmyHunt(string map, string[] monsters, string item, ClassType classType, bool isTemp = false, int quant = 1)
    {
        Core.PrivateRooms = true;
        Core.PrivateRoomNumber = Army.getRoomNr();

        if (Bot.Config!.Get<bool>("sellToSync"))
            Army.SellToSync(item, quant);

        Core.AddDrop(item);
        Army.waitForParty(map);

        Core.EquipClass(classType);
        Core.FarmingLogger(item, quant);

        Army.SmartAggroMonStart(map, monsters);

        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            if (monsters == new[] { "Hydra Head 90" })
            {
                Core.Logger("Swapping classes to 1 of the 3\n" +
                ">> so that we can be sure you arent doing multi targeting\n" +
                ">> as itd fuck it up");
                
                foreach (string Class in new[] { "StoneCrusher", "Lord of Order", "Void Highlord" })
                    if (Core.CheckInventory(Class))
                        Core.Equip(Class);

                while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
                    Bot.Combat.Attack("*");
                break;
            }

            else if (monsters == new[] { "Tigoras" })
            {
                Core.KillTrigoras(item, quant, 1, isTemp);
                break;
            }

            else if (monsters != new[] { "Tigoras" } || monsters != new[] { "Hydra Head 90" })
                Bot.Combat.Attack("*");
        }
        Core.JumpWait();
        Army.AggroMonStop(true);

        while (!Bot.ShouldExit && Bot.Player.InCombat)
        {
            Core.JumpWait();
            Bot.Sleep(2500);
        }
        Army.waitForParty(map, item);
    }

    void ArmyHunt(string map, int monsterID, string item, ClassType classType, bool isTemp = false, int quant = 1)
    {
        Core.PrivateRooms = true;
        Core.PrivateRoomNumber = Army.getRoomNr();

        Monster? monster = Bot.Monsters.CurrentMonsters?.Find(m => m.ID == monsterID);

        if (Bot.Config!.Get<bool>("sellToSync"))
            Army.SellToSync(item, quant);

        Core.AddDrop(item);
        Army.waitForParty(map);

        Core.EquipClass(classType);
        Core.FarmingLogger(item, quant);

        Army.SmartAggroMonStart(map, monster!.ToString());

        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
            Bot.Combat.Attack("*");

        Core.JumpWait();
        Army.AggroMonStop(true);

        while (!Bot.ShouldExit && Bot.Player.InCombat)
        {
            Core.JumpWait();
            Bot.Sleep(2500);
        }
        Army.waitForParty(map, item);
    }


    public void PlayerAFK()
    {
        Core.Logger("Anti-AFK engaged");
        Bot.Sleep(1500);
        Bot.Send.Packet("%xt%zm%afk%1%false%");
    }
}
