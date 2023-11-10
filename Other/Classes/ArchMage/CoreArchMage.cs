/*
name: null
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Farm/BuyScrolls.cs
//cs_include Scripts/Good/BLoD/CoreBLOD.cs
//cs_include Scripts/Story/BattleUnder.cs
//cs_include Scripts/Story/ShadowsOfWar/CoreSoW.cs
//cs_include Scripts/Story/QueenofMonsters/CoreQOM.cs
//cs_include Scripts/ShadowsOfWar/CoreSoWMats.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Skills;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class CoreArchMage
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreFarms Farm = new();
    private CoreAdvanced Adv = new();
    private BuyScrolls Scroll = new();
    private CoreBLOD BLOD = new();
    private CoreQOM QOM = new();
    private CoreSoW SoW = new();
    private CoreSoWMats SOWM = new();

    public bool DontPreconfigure = true;
    public string OptionsStorage = "ArchMage";
    public List<IOption> Options = new()
    {
        new Option<bool>("lumina_elementi", "Lumina Elementi", "Todo the last quest or not, for the 51% wep(takes awhileand will require aditional boss items.) [On by default]", true),
        new Option<bool>("cosmetics", "Get Cosmetics", "Gets the cosmetic rewards (redoes quests if you don't have them, disable to just get ArchMage and the weapon) [On by default]", true),
        new Option<bool>("army", "Armying?", "use when running on 4 accounts at once only, will probably get out of sync.) [Off by default]", false),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(RequiredItems.Concat(BossDrops).ToArray());
        Core.SetOptions();

        GetAM();

        Core.SetOptions(false);
    }

    public void GetAM(bool rankUpClass = true)
    {
        bool cosmetics = Bot.Config!.Get<bool>("cosmetics");
        bool lumina = Bot.Config!.Get<bool>("lumina_elementi");
        army = Bot.Config!.Get<bool>("army");

        if (Core.CheckInventory("ArchMage", toInv: false))
        {
            if (!lumina)
            {
                if (!cosmetics)
                {
                    Core.Logger("You own \"ArchMage\", farm complete.");
                    return;
                }
                else if (Core.CheckInventory(Cosmetics, toInv: false))
                {
                    Core.Logger("You own \"ArchMage\" and the extra cometics, farm complete.");
                    return;
                }
            }
            else if (Core.CheckInventory("Providence", toInv: false))
            {
                if (!cosmetics)
                {
                    Core.Logger("You own \"ArchMage\" and \"Providence\", farm complete.");
                    return;
                }
                else if (Core.CheckInventory(Cosmetics, toInv: false))
                {
                    Core.Logger("You own \"ArchMage\", \"Providence\", and the extra cometics, farm complete.");
                    return;
                }
            }
        }

        if (army)
            Core.Logger("Armying Set to True, Please have all accounts logged in and Following this Acc using the Tools > Butler.cs");
        Bot.Drops.Add(RequiredItems.Concat(BossDrops).Concat(Cosmetics).ToArray());

        Core.Logger("The bot will now farm all requierments for ArchMage");
        SoW.CompleteCoreSoW();
        QOM.TheReshaper();

        Farm.SpellCraftingREP();
        Farm.EmberseaREP();
        Farm.ChaosREP();
        Farm.GoodREP();
        Farm.EvilREP();
        Farm.EtherStormREP();
        Farm.LoremasterREP();

        Farm.Experience(100);

        Core.Logger("Requirements complete");

        if (!Core.CheckInventory("ArchMage"))
        {
            Core.EnsureAccept(8918);
            Core.Logger($"ArchMage: Cosmetics = {cosmetics}");

            BookOfMagus();
            BookOfFire(cosmetics);
            BookOfIce(cosmetics);
            BookOfAether(cosmetics);
            BookOfArcana(cosmetics);

            Core.ToBank(Cosmetics);
            BossItemCheck(250, "Elemental Binding");

            Core.Unbank("Book of Magus", "Book of Fire", "Book of Ice", "Book of Aether", "Book of Arcana", "Elemental Binding");
            Core.EnsureComplete(8918);

            Bot.Wait.ForPickup("ArchMage");
            Core.ToBank(Cosmetics);

            if (rankUpClass)
                Adv.RankUpClass("ArchMage");
        }

        if (lumina)
            LuminaElementi();
    }

    public void LuminaElementi(bool standalone = false)
    {
        if (standalone || Bot.Config!.Get<bool>("cosmetics") ?
                Core.CheckInventory(Core.EnsureLoad(8919).Rewards.Select(x => x.ID).ToArray(), toInv: false) :
                Core.CheckInventory("Providence", toInv: false))
            return;

        if (Bot.Quests.IsUnlocked(8919))
            GetAM(false);

        Core.EnsureAccept(8919);
        Core.Logger("Doing the extra quest for the 51% weapon \"Providence\"");

        BookOfArcana();
        UnboundTome(30);
        BossItemCheck(2500, "Elemental Binding");

        Core.EquipClass(ClassType.Farm);
        SOWM.PrismaticSeams();

        Core.FarmingLogger("Unbound Thread", 100);
        //Fallen Branches 8869
        Core.RegisterQuests(8869);
        Core.AddDrop("Unbound Thread", "Providence");
        while (!Bot.ShouldExit && !Core.CheckInventory("Unbound Thread", 100))
        {
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("DeadLines", "Frenzied Mana", "Captured Mana", 8);
            Core.HuntMonster("DeadLines", "Shadowfall Warrior", "Armor Scrap", 8);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("DeadLines", "Eternal Dragon", "Eternal Dragon Scale");
            Bot.Wait.ForPickup("Unbound Thread");
        }
        Core.CancelRegisteredQuests();

        Core.EnsureComplete(8919);
        Bot.Wait.ForPickup("Providence");
        Core.Logger("Weapon obtained: \"Providence\" [51% damage to all]");
    }

    #region Books
    public void BookOfMagus()
    {
        //Book of Magus: Incantation
        if (Core.CheckInventory("Book of Magus"))
            return;

        Core.FarmingLogger("Book of Magus", 1);
        UnboundTome(1);
        Core.EnsureAccept(8913);
        BLOD.GetBlindingWeapon(WeaponOfDestiny.Mace);
        BLOD.BrilliantAura(200);

        Scroll.BuyScroll(Scrolls.Mystify, 50);
        SOWM.PrismaticSeams(250);

        Core.HuntMonster("noxustower", "Lightguard Caster", "Mortal Essence", 100, false);
        Core.HuntMonster("portalmazec", "Pactagonal Knight", "Orthogonal Energy", 150, false);

        Core.EquipClass(ClassType.Solo);
        Core.HuntMonster("timeinn", "Ezrajal", "Celestial Magia", 50, false);

        Core.EnsureComplete(8913);
        Bot.Wait.ForPickup("Book of Magus");
        Core.ToBank(BLOD.BLoDItems);

    }

    public void BookOfFire(bool Extras = false)
    {
        //Book of Fire: Immolation
        if (Extras ?
                Core.CheckInventory(new[] { "Book of Fire", "Arcane Floating Sigil", "Sheathed Archmage's Staff" }, toInv: false) :
                Core.CheckInventory("Book of Fire"))
            return;

        Core.FarmingLogger("Book of Fire", 1);

        UnboundTome(1);
        Core.EnsureAccept(8914);

        Scroll.BuyScroll(Scrolls.FireStorm, 50);

        Core.EquipClass(ClassType.Farm);
        Core.HuntMonster("fireavatar", "Shadefire Cavalry", "ShadowFire Wisps", 200, false);
        Core.HuntMonster("fotia", "Femme Cult Worshiper", "Spark of Life", 200, false);
        Core.HuntMonster("mafic", "*", "Emblazoned Basalt", 200, false);

        Core.EquipClass(ClassType.Solo);
        Core.KillMonster("underlair", "r6", "left", "Void Draconian", "Dense Dragon Crystal", 200, false);

        Core.EnsureComplete(8914);
        Bot.Wait.ForPickup("Book of Fire");
        Core.ToBank(Cosmetics);
    }

    public void BookOfIce(bool Extras = false)
    {
        if (Extras ?
                Core.CheckInventory(new[] { "Book of Ice", "Archmage's Cowl", "Archmage's Cowl and Locks" }, toInv: false) :
                Core.CheckInventory("Book of Ice"))
            return;

        Core.FarmingLogger("Book of Ice", 1);

        UnboundTome(1);
        Core.EnsureAccept(8915);

        Scroll.BuyScroll(Scrolls.Frostbite, 50);

        Core.EquipClass(ClassType.Solo);
        while (!Bot.ShouldExit && !Core.CheckInventory("Ice Diamond", 100))
        {
            Core.EnsureAccept(7279);
            Core.HuntMonster("kingcoal", "Snow Golem", "Frozen Coal", 10);
            Core.EnsureComplete(7279);
            Bot.Wait.ForPickup("Ice Diamond");
        }
        Core.HuntMonster("icepike", "Chained Kezeroth", "Rimeblossom", 100, false);
        Core.HuntMonster("icepike", "Karok the Fallen", "Starlit Frost", 100, false);
        Core.HuntMonster("icedungeon", "Shade of Kyanos", "Temporal Floe", 100, false);

        Core.EnsureComplete(8915);
        Bot.Wait.ForPickup("Book of Ice");
        Core.ToBank(Cosmetics);

    }

    public void BookOfAether(bool Extras = false)
    {
        if (Extras ?
                Core.CheckInventory(new[] { "Book of Aether", "Archmage's Staff" }, toInv: false) :
                Core.CheckInventory("Book of Aether"))
            return;

        Core.FarmingLogger("Book of Aether", 1);

        BossItemCheck(1, "Void Essentia", "Vital Exanima", "Everlight Flame");

        UnboundTome(1);
        Core.EnsureAccept(8916);

        Scroll.BuyScroll(Scrolls.Eclipse, 50);

        Core.EquipClass(ClassType.Solo);
        Core.HuntMonster("streamwar", "Second Speaker", "A Fragment of the Beginning", isTemp: false);
        // Core.HuntMonster("fireavatar", "Avatar Tyndarius", "Everlight Flame", isTemp: false); //1% Drop Rate
        Core.EnsureComplete(8916);
        Bot.Wait.ForPickup("Book of Aether");
        Core.ToBank(Cosmetics);

    }

    public void BookOfArcana(bool Extras = false)
    {
        if (Extras ?
                Core.CheckInventory(new[] { "Book of Arcana", "Archmage's Robes" }, toInv: false) :
                Core.CheckInventory("Book of Arcana") && !Extras)
            return;

        Core.FarmingLogger("Book of Arcana", 1);

        BossItemCheck(1, "The Mortal Coil", "The Divine Will", "Insatiable Hunger", "Undying Resolve", "Calamitous Ruin");

        UnboundTome(1);
        Core.EnsureAccept(8917);

        Scroll.BuyScroll(Scrolls.EtherealCurse, 50);

        Core.EnsureComplete(8917);
        Bot.Wait.ForPickup("Book of Arcana");
        Core.ToBank(Cosmetics);
    }

    #endregion

    #region Materials
    public void MysticScribingKit(int quant)
    {
        if (Core.CheckInventory(73327, quant))
            return;

        Core.FarmingLogger("Mystic Scribing Kit", quant);
        Core.AddDrop("Mystic Scribing Kit");

        while (!Bot.ShouldExit && !Core.CheckInventory("Mystic Scribing Kit", quant))
        {
            Core.EnsureAccept(8909);

            Core.EquipClass(ClassType.Farm);
            Core.FarmingLogger("Mystic Quills", 49);
            Core.FarmingLogger("Mystic Shards", 49);


            if (!Core.isCompletedBefore(3052))
            {
                Core.EnsureAccept(3052);
                Core.GetMapItem(1921, 1, "dragonrune");
                Core.GetMapItem(1922, 1, "dragonrune");
                Core.GetMapItem(1923, 1, "dragonrune");
                Core.GetMapItem(1924, 1, "dragonrune");
                Core.EnsureComplete(3052);
            }
            Core.RegisterQuests(3050);
            while (!Bot.ShouldExit && !Core.CheckInventory(new[] { "Mystic Shards", "Mystic Quills" }, 49))
            {
                Core.HuntMonster("gilead", "Water Elemental", "Water Core", log: false);
                Core.HuntMonster("gilead", "Fire Elemental", "Fire Core", log: false);
                Core.HuntMonster("gilead", "Wind Elemental", "Air Core", log: false);
                Core.HuntMonster("gilead", "Earth Elemental", "Earth Core", log: false);
                Core.HuntMonster("gilead", "Mana Elemental", "Mana Core", log: false);
            }

            //Incase they swap it back again:

            // Core.RegisterQuests(3298);
            // while (!Bot.ShouldExit && !Core.CheckInventory(new[] { "Mystic Shards", "Mystic Quills" }, 49))
            // {
            //     Core.HuntMonster("gilead", "Water Elemental", "Water Drop", 5, log: false);
            //     Core.HuntMonster("gilead", "Fire Elemental", "Flame", 5, log: false);
            //     Core.HuntMonster("gilead", "Wind Elemental", "Breeze", 5, log: false);
            //     Core.HuntMonster("gilead", "Earth Elemental", "Stone", 5, log: false);
            // }

            Core.CancelRegisteredQuests();

            Core.EquipClass(ClassType.Solo);
            if (!Core.CheckInventory("Semiramis Feather"))
            {
                Core.AddDrop("Semiramis Feather");
                Core.EnsureAccept(6286);
                Core.HuntMonster("guardiantree", "Terrane", "Terrane Defeated");
                Core.EnsureComplete(6286);
                Bot.Wait.ForPickup("Semiramis Feather");
            }
            Core.HuntMonster("deepchaos", "Kathool", "Mystic Ink", isTemp: false);

            Core.EnsureComplete(8909);
            Bot.Wait.ForPickup("Mystic Scribing Kit");
        }
    }

    public void PrismaticEther(int quant)
    {
        if (Core.CheckInventory(73333, quant))
            return;

        if (!Bot.Quests.IsUnlocked(8910))
            MysticScribingKit(1);

        Core.FarmingLogger("Prismatic Ether", quant);
        Core.AddDrop("Prismatic Ether");
        Core.EquipClass(ClassType.Solo);
        while (!Bot.ShouldExit && !Core.CheckInventory("Prismatic Ether", quant))
        {
            Core.EnsureAccept(8910);
            Core.KillNulgathFiendShard("Infernal Ether");
            Core.HuntMonster("celestialarenad", "Aranx", "Celestial Ether", isTemp: false);
            Core.HuntMonster("eternalchaos", "Eternal Drakath", "Chaotic Ether", isTemp: false);
            Core.KillMonster("shadowattack", "Boss", "Left", "Death", "Mortal Ether", isTemp: false);
            Core.HuntMonster("gaiazor", "Gaiazor", "Vital Ether", isTemp: false);
            Core.EnsureComplete(8910);
            Bot.Wait.ForPickup("Prismatic Ether");
        }
    }

    public void ArcaneLocus(int quant)
    {
        if (Core.CheckInventory(73339, quant))
            return;

        if (!Bot.Quests.IsUnlocked(8911))
            PrismaticEther(1);

        Core.FarmingLogger("Arcane Locus", quant);
        Core.AddDrop(73339);

        while (!Bot.ShouldExit && !Core.CheckInventory(73339, quant))
        {
            Core.EnsureAccept(8911);
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("natatorium", "Anglerfish", "Sea Locus", isTemp: false);
            Core.EquipClass(ClassType.Solo);
            Bot.Sleep(2500);
            Core.Logger("cutscene happens when joining some maps, give the bot a sec to realise its not broke :P");
            Bot.Sleep(2500);
            Core.KillMonster("skytower", "r13", "Bottom", "*", "Sky Locus", isTemp: false);
            Core.HuntMonster("elemental", "Mana Golem", "Prime Locus Attunement", 30, isTemp: false);
            Core.HuntMonster("ectocave", "Ektorax", "Earth Locus", isTemp: false);
            Core.HuntMonster("drakonnan", "Drakonnan", "Fire Locus", isTemp: false);

            Core.EnsureComplete(8911);
            Bot.Wait.ForPickup(73339);
        }
    }

    public void UnboundTome(int quant)
    {
        int unboundTomesNeeded = Core.CheckInventory("Unbound Tome") ? quant - Bot.Inventory.GetQuantity("Unbound Tome") : quant;

        if (unboundTomesNeeded <= 0)
            return;

        if (!Bot.Quests.IsUnlocked(8912))
            ArcaneLocus(1);

        Core.FarmingLogger("Unbound Tome", unboundTomesNeeded);
        Core.AddDrop("Unbound Tome");

        while (!Bot.ShouldExit && !Core.CheckInventory("Unbound Tome", unboundTomesNeeded))
        {
            Core.EnsureAccept(8912);
            MysticScribingKit(unboundTomesNeeded);
            PrismaticEther(unboundTomesNeeded);
            ArcaneLocus(unboundTomesNeeded);
            Farm.DragonRunestone(30);
            Adv.BuyItem("darkthronehub", 1308, "Exalted Paladin Seal");
            Adv.BuyItem("shadowfall", 89, "Forsaken Doom Seal");

            Core.EnsureComplete(8912);
            Bot.Wait.ForPickup("Unbound Tome");
        }
    }

    #endregion

    private void BossItemCheck(int quant = 1, params string[] Items)
    {
        foreach (string item in Items)
        {
            if (Core.CheckInventory(item))
                continue;

            switch (item)
            {
                case "Void Essentia":
                    Item("voidflibbi", "Flibbitiestgibbet", item, quant);
                    break;

                case "Vital Exanima":
                    Core.BossClass();
                    Adv.KillUltra("dage", "Boss", "Right", "Dage the Evil", item, isTemp: false);
                    break;

                case "Everlight Flame":
                    Core.BossClass();
                    Adv.KillUltra("fireavatar", "r9", "Left", "Avatar Tyndarius", item, isTemp: false);
                    break;

                case "Calamitous Ruin":
                    if (army)
                    {
                        Bot.Events.RunToArea += DarkCarnaxMove;
                        Core.Logger("You might need to babysit this one due to the laser");
                        Adv.KillUltra("darkcarnax", "Boss", "Right", "Nightmare Carnax", "Calamitous Ruin", isTemp: false);
                        Bot.Events.RunToArea -= DarkCarnaxMove;
                    }
                    else Item("darkcarnax", "Nightmare Carnax", item, quant);
                    break;

                case "The Mortal Coil":
                    Core.BossClass();
                    Adv.KillUltra("tercessuinotlim", "Boss2", "Right", "Nulgath", item, isTemp: false);
                    break;

                case "The Divine Will":
                    Item("celestialpast", "Azalith", item, quant);
                    break;

                case "Insatiable Hunger":
                    Item("voidnightbane", "Nightbane", item, quant);
                    break;

                case "Undying Resolve":
                    Bot.Quests.UpdateQuest(8732);
                    Item("theworld", "Encore Darkon", item, quant);
                    break;

                case "Elemental Binding":
                    Item("archmage", "Prismata", item, quant);
                    break;
            }
        }

        void Item(string map, string monster, string item, int quant)
        {
            if (army)
                Core.HuntMonster(map, monster, item, quant, isTemp: false);
            else Core.Logger($"{item} x{quant} not found, it can be farmed (with an army) from \"{monster}\" in /{map.ToLower()}", stopBot: true);
        }
    }

    //For Nightmare Carnax
    private void DarkCarnaxMove(string zone)
    {
        switch (zone.ToLower())
        {
            case "a":
                //Move to the right
                Bot.Player.WalkTo(Bot.Random.Next(600, 930), Bot.Random.Next(380, 475));
                Bot.Sleep(2500);
                break;
            case "b":
                //Move to the left
                Bot.Player.WalkTo(Bot.Random.Next(25, 325), Bot.Random.Next(380, 475));
                Bot.Sleep(2500);
                break;
            default:
                //Move to the center
                Bot.Player.WalkTo(Bot.Random.Next(325, 600), Bot.Random.Next(380, 475));
                Bot.Sleep(2500);
                break;
        }
    }


    private readonly string[] RequiredItems = {
        "ArchMage",
        "Providence",
        "Mystic Scribing Kit",
        "Prismatic Ether",
        "Arcane Locus",
        "Unbound Tome",
        "Book of Magus",
        "Book of Fire",
        "Book of Ice",
        "Book of Aether",
        "Book of Arcana",
        "Arcane Sigil",
        "Archmage"
    };
    private string[] BossDrops = {
        "Void Essentia",
        "Vital Exanima",
        "Everlight Flame",
        "Calamitous Ruin",
        "The Mortal Coil",
        "The Divine Will",
        "Insatiable Hunger",
        "Undying Resolve",
        "Elemental Binding"
    };
    private string[] Cosmetics = {
        "Arcane Sigil",
        "Arcane Floating Sigil",
        "Sheathed Archmage's Staff",
        "Archmage's Cowl",
        "Archmage's Cowl and Locks",
        "Archmage's Staff",
        "Archmage's Robes",
        "Divine Mantle",
        "Divine Veil",
        "Divine Veil and Locks",
        "Prismatic Floating Sigil",
        "Sheathed Providence",
        "Prismatic Sigil",
        "Astral Mantle"
    };
    private bool army = false;


}
