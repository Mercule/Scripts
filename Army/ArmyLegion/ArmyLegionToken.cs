/*
name: Army Legion Token
description: Uses an army, and method select to farm Legion tokens
tags: army, legion, legion tokens, dreadrock, pet, bright paragon pet, shogun paragon pet, thanatos paragon pet, arcane paragon pet, paragon dreadnaught pet, munted paragon pet, ascended paragon pet, festive paragon dracolich rider, holiday paragon pet, uw3017 pet, infernal caladbolg, hardcore paragon pet, legion arena
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/Legion/CoreLegion.cs
//cs_include Scripts/CoreStory.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Models.Monsters;
using Skua.Core.Models.Quests;
using Skua.Core.Options;

public class ArmyLegionToken
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreAdvanced Adv => new();
    private CoreArmyLite Army = new();
    public CoreLegion Legion = new();

    private static CoreArmyLite sArmy = new();

    public string OptionsStorage = "ArmyLegionToken";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        new Option<Method>("Method", "Which method to get LTs?", "Choose your method", Method.Dreadrock),
        sArmy.player1,
        sArmy.player2,
        sArmy.player3,
        sArmy.player4,
        sArmy.player5,
        sArmy.player6,
        sArmy.packetDelay,
        CoreBots.Instance.SkipOptions
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.Add("Legion Token");
        Core.SetOptions(disableClassSwap: false);

        Setup(Bot.Config.Get<Method>("Method"), 25001);

        Core.SetOptions(false);
    }

    public void Setup(Method Method, int quant = 25000)
    {
        Core.OneTimeMessage("Only for army", "This is intended for use with an army, not for solo players.");

        Legion.JoinLegion();


        switch (Method.ToString())
        {
            case "Dreadrock":
                Adv.BuyItem("underworld", 216, "Undead Champion");

                questIDs = new() { 4849 };
                monNames = new() { "Void Mercenary", "Fallen Hero", "Hollow Wraith", "Shadowknight", "Legion Sentinel" };
                drops = new() { "Legion Token" };
                map = "dreadrock";
                classType = ClassType.Farm;

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;

            case "Shogun_Paragon_Pet":
                if (!Core.CheckInventory("Shogun Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(RacialGearBoost.Elemental);

                monNames.Clear();
                monNames.AddRange(new[] { "Fotia Elemental", "Fotia Spirit" });

                drops.Clear();
                drops.Add("Legion Token");

                map = "fotia";
                classType = ClassType.Farm;
                Adv.SmartEnhance(Core.FarmClass);

                int paragonFiendPetID = -1;
                bool hasInfernalCaladbolg = Core.CheckInventory("Infernal Caladbolg");
                bool hasShogunParagonPet = Core.CheckInventory("Shogun Paragon Pet");
                bool hasShogunDagePet = Core.CheckInventory("Shogun Dage Pet");
                bool hasParagonFiendPet = Core.CheckInventory("Paragon Fiend Quest Pet");

                if (hasInfernalCaladbolg)
                    questIDs = new() { 3722, 5755 };
                else if (hasShogunParagonPet)
                    questIDs = new() { 5755 };
                else if (hasShogunDagePet)
                    questIDs = new() { 5756 };
                else if (hasParagonFiendPet)
                {
                    paragonFiendPetID = Bot.Inventory.GetItem("Paragon Fiend Quest Pet")!.ID;
                    if (paragonFiendPetID == 47578)
                        questIDs = new() { 6750 };
                    else if (paragonFiendPetID == 47614)
                        questIDs = new() { 6756 };
                }
                else if (Core.CheckInventory("Paragon Ringbearer"))
                    questIDs = new() { 7073 };

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;



            case "Thanatos_Paragon_Pet":
                if (!Core.CheckInventory("Thanatos Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(RacialGearBoost.Dragonkin);
                questIDs = new() { 4100 };
                monNames = new() { "Zombie Dragon" };
                drops = new() { "Legion Token" };
                map = "dragonheart";
                classType = ClassType.Farm;
                Adv.SmartEnhance(Core.FarmClass);

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;

            case "Bright_Paragon_Pet":
                if (!Core.CheckInventory("Bright Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                questIDs = new() { 4704 };
                monNames = new() { "Skeletal Ice mage" };
                drops = new() { "Legion Token" };
                map = "brightfortress";
                classType = ClassType.Farm;
                Adv.SmartEnhance(Core.FarmClass);

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;

            case "Arcane_Paragon_Pet":
                if (!Core.CheckInventory("Arcane Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(RacialGearBoost.Dragonkin);
                Core.EquipClass(ClassType.Farm);
                Adv.SmartEnhance(Core.FarmClass);

                Core.RegisterQuests(4896);
                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    GetItem("dragonheart", "Granite Dracolich", "Granite Dracolich Soul", 4, isTemp: false);
                    GetItem("dragonheart", "Tempest Dracolich", "Tempest Dracolich Soul", 4, isTemp: false);
                    GetItem("dragonheart", "Inferno Dracolich", "Inferno Dracolich Soul", 4, isTemp: false);
                    GetItem("dragonheart", "Deluge Dracolich", "Deluge Dracolich Soul", 4, isTemp: false);
                }
                break;

            case "Paragon_Dreadnaught_Pet":
                if (!Core.CheckInventory("Paragon Dreadnaught Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                Core.EquipClass(ClassType.Farm);
                Adv.SmartEnhance(Core.FarmClass);

                Core.RegisterQuests(5741);
                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    GetItem("laken", "Augmented Guard", "Stolen Guard", 5);
                    GetItem("laken", "Cyborg Dog", "Stolen Dog", 6);
                    GetItem("laken", "Mad Scientist", "Taken Axe", 10);
                }
                break;

            case "Mounted_Paragon_Pet":
                if (!Core.CheckInventory("Mounted Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                questIDs = new() { 5604 };
                monNames = new() { "Ice Wolf" };
                drops = new() { "Legion Token" };
                map = "frozentower";
                classType = ClassType.Farm;
                Adv.SmartEnhance(Core.FarmClass);

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;

            case "Ascended_Paragon_Pet":
                if (!Core.CheckInventory("Ascended Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                Core.EquipClass(ClassType.Solo);
                Adv.SmartEnhance(Core.SoloClass);

                Core.RegisterQuests(2747);
                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    GetItem("tournament", "Lord Brentan", "Lord Brentan's Regal Blade", 1);
                    GetItem("tournament", "Roderick", "Roderick's Chaotic Bane", 1);
                    GetItem("tournament", "Knight of Thorns", "Knight of Thorns' Sword", 1);
                    GetItem("tournament", "Johann Wryce", "Platinum of Johann Wryce", 1);
                    GetItem("tournament", "Khai Kaldun", "Khai Kaldun's Scimitar", 1);
                }
                break;

            case "Festive_Paragon_Dracolich_Rider":
                if (!Core.CheckInventory("Festive Paragon Dracolich Rider") || !Core.isSeasonalMapActive("frozenruins"))
                    Core.Logger("Pet not owned / Seasonal Map not active, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                questIDs = new() { 3969 };
                monNames = new() { "Frost Fangbeast" };
                drops = new() { "Legion Token" };
                map = "frozenruins";
                classType = ClassType.Solo;
                Adv.SmartEnhance(Core.SoloClass);

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;

            case "Holiday_Paragon_Pet":
                if (!Core.CheckInventory("Holiday Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                Core.EquipClass(ClassType.Farm);
                Adv.SmartEnhance(Core.FarmClass);

                Core.RegisterQuests(3256);
                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    GetItem("prison", "King Alteon's Knight", "Spirit of Loyalty", 6);
                    GetItem("battlewedding", "Silver Knight", "Spirit of Love", 6);
                    GetItem("lycan", "Lycan Knight", "Spirit of Love", 6);
                }
                break;

            case "UW3017_Pet":
                if (!Core.CheckInventory("UW3017 Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(GenericGearBoost.dmgAll);
                questIDs = new() { 5738 };
                monNames = new() { "Bloodfiend" };
                drops = new() { "Legion Token" };
                map = "underworld";
                classType = ClassType.Farm;
                Adv.SmartEnhance(Core.FarmClass);

                RunAggro();
                break;

            case "Infernal_Caladbolg":
                if (!Core.CheckInventory("Infernal Caladbolg"))
                    Core.Logger("Sword not owned, stopping", stopBot: true);

                //Adv.BestGear(RacialGearBoost.Elemental);
                Core.EquipClass(ClassType.Farm);
                Adv.SmartEnhance(Core.FarmClass);

                if (Core.CheckInventory("Shogun Paragon Pet"))
                    questIDs = new() { 3722, 5755 };
                else questIDs = new() { 3722 };


                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    if (Core.CheckInventory("Shogun Paragon Pet"))
                    {
                        GetItem("fotia", "*", "Nothing Heard", 10);
                        GetItem("fotia", "*", "Nothing to See", 10);
                        GetItem("fotia", "*", "Area Secured and Quiet", 10);
                    }

                    GetItem("fotia", "Fotia Elemental", "Betrayer Extinguished", 5);
                    GetItem("evilwardage", "Dreadfiend of Nulgath", "Fiend Felled", 2);
                }
                break;

            case "Hardcore_Paragon_Pet":
                if (!Core.CheckInventory("Hardcore Paragon Pet"))
                    Core.Logger("Pet not owned, stopping", stopBot: true);

                //Adv.BestGear(!Bot.Quests.IsUnlocked(793) ? RacialGearBoost.Undead : RacialGearBoost.Chaos);
                questIDs = new() { !Bot.Quests.IsUnlocked(793) ? 3393 : 3394 };
                monNames = new() { !Bot.Quests.IsUnlocked(793) ? "Binky" : "Ultra Chaos Warlord" };
                drops = new() { "Legion Token" };
                map = !Bot.Quests.IsUnlocked(793) ? "doomvault" : "chaosboss";
                classType = ClassType.Solo;
                Adv.SmartEnhance(Core.SoloClass);

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                {
                    if (!Bot.Quests.IsUnlocked(793))
                        RunAggro(true);
                    else RunAggro();
                }
                break;

            default:
                //Adv.BestGear(RacialGearBoost.Undead);
                questIDs = new() { 6742, 6743 };
                monNames = new() { "Legion Fiend Rider" };
                drops = new() { "Legion Token", "Bone Sigil" };
                map = "legionarena";
                classType = ClassType.Solo;
                Adv.SmartEnhance(Core.SoloClass);

                Adv.BuyItem("underworld", 216, "Undead Champion");

                while (!Bot.ShouldExit && !Core.CheckInventory("Legion Token", quant))
                    Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);
                break;
        }
    }

    public void GetItem(string map = "", string? monster = null, string? item = null, int quant = 1, bool isTemp = true, bool Binky = false)
    {
        Core.PrivateRooms = true;
        Core.PrivateRoomNumber = Army.getRoomNr();
        Bot.Events.PlayerAFK += PlayerAFK;

        while (true)
        {
            if (Binky)
                Core.HuntMonster("Doomvault", "Binky", item);
            else
                Army.RunGeneratedAggroMon(map, monNames, questIDs, classType, drops);

            if (Bot.ShouldExit || Binky || Core.CheckInventory(item, quant))
                break;
        }

        Army.AggroMonStop(true);
        Core.CancelRegisteredQuests();
        Bot.Events.PlayerAFK -= PlayerAFK;

        void PlayerAFK()
        {
            Core.Logger("Anti-AFK engaged");
            Bot.Sleep(1500);
            Bot.Send.Packet("%xt%zm%afk%1%false%");
        }
    }


    private void RunAggro(bool Binky = false) => GetItem();

    public enum Method
    {
        Dreadrock = 0,
        Bright_Paragon_Pet = 1,
        Shogun_Paragon_Pet = 2,
        Thanatos_Paragon_Pet = 3,
        Arcane_Paragon_Pet = 4,
        Paragon_Dreadnaught_Pet = 5,
        Mounted_Paragon_Pet = 6,
        Ascended_Paragon_Pet = 7,
        Festive_Paragon_Dracolich_Rider = 8,
        Holiday_Paragon_Pet = 9,
        UW3017_Pet = 10,
        Infernal_Caladbolg = 11,
        Hardcore_Paragon_Pet = 12,
        Legion_Arena = 13,
    }

    private List<int> questIDs = new() { };
    private List<string> monNames = new() { };
    private List<string> drops = new() { };
    private string map = "";
    private ClassType classType;
}
