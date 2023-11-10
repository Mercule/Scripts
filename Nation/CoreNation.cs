/*
name: null
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs

using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Models.Quests;

public class CoreNation
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private readonly CoreFarms Farm = new();

    //CanChange: If enabled will sell the "Voucher of Nulgath" item during farms if it's not needed.
    bool sellMemVoucher = true;
    //CanChange: If enabled will do "Swindles Return Policy" passively during "Supplies To Spin The Wheels of Fate".
    bool returnPolicyDuringSupplies = true;

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.RunCore();
    }

    /// <summary>
    /// Crag & Bamboozle name in game
    /// </summary>
    public string CragName => "Crag &amp; Bamboozle";

    /// <summary>
    /// All principal drops from Nulgath
    /// </summary>
    public string[] bagDrops =
    {
        "Dark Crystal Shard",
        "Diamond of Nulgath",
        "Gem of Nulgath",
        "Tainted Gem",
        "Unidentified 10",
        "Unidentified 13",
        "Unidentified 24",
        "Voucher of Nulgath",
        "Voucher of Nulgath (non-mem)",
        "Essence of Nulgath",
        "Unidentified 25",
        "Totem of Nulgath",
        "Fiend Token",
        "Blood Gem of the Archfiend",
        "Emblem of Nulgath",
        "Receipt of Swindle",
        "Bone Dust",
        "Nulgath's Approval",
        "Archfiend's Favor",
        "Unidentified 34"
    };

    public string[] SuppliesRewards =
    {
    "Tainted Gem",
    "Dark Crystal Shard",
    "Diamond of Nulgath",
    "Voucher of Nulgath",
    "Voucher of Nulgath (non-mem)",
    "Gem of Nulgath",
    "Unidentified 10"
    };

    /// <summary>
    /// Drops from the bosses that used to give acess to tercess
    /// </summary>
    public string[] tercessBags = { "Bone Dust" };

    /// <summary>
    /// List of Betrayal Blades
    /// </summary>
    public string[] betrayalBlades =
    {
        "1st Betrayal Blade of Nulgath",
        "2nd Betrayal Blade of Nulgath",
        "3rd Betrayal Blade of Nulgath",
        "4th Betrayal Blade of Nulgath",
        "5th Betrayal Blade of Nulgath",
        "6th Betrayal Blade of Nulgath",
        "7th Betrayal Blade of Nulgath",
        "8th Betrayal Blade of Nulgath"
    };

    /// <summary>
    /// Shadow Blast Arena medals
    /// </summary>
    public string[] nationMedals =
    {
        "Nation Round 1 Medal",
        "Nation Round 2 Medal",
        "Nation Round 3 Medal",
        "Nation Round 4 Medal"
    };

    public string[] Receipt =
    {
        "Unidentified 1",
        "Unidentified 6",
        "Unidentified 9",
        "Unidentified 16",
        "Unidentified 20",
        "Receipt of Swindle",
        "Dark Crystal Shard",
        "Diamond of Nulgath",
        "Gem of Nulgath",
        "Blood Gem of the Archfiend"
    };

    /// <summary>
    /// Misc items to accept during Bloody Chaos if turned on
    /// </summary>
    public string[] BloodyChaosSupplies =
    {
        "Tainted Gem",
        "Dark Crystal Shard",
        "Diamond of Nulgath",
        "Voucher of Nulgath",
        "Voucher of Nulgath (non-mem)",
        "Unidentified 10",
        "Unidentified 13",
        "Gem of Nulgath",
        "Relic of Chaos"
    };

    public string[] SwindlesReturn =
    {
        "Unidentified 1",
        "Unidentified 6",
        "Unidentified 9",
        "Unidentified 16",
        "Unidentified 20",
    };

    public string[] SwindlesReturnRewards =
    {
        "Tainted Gem",
        "Dark Crystal Shard",
        "Diamond of Nulgath",
        "Gem of Nulgath",
        "Blood Gem of the Archfiend",
        "Receipt of Swindle"
    };

    public string Uni(int nr)
        => $"Unidentified {nr}";

    /// <summary>
    /// Does Essence of Defeat Reagent quest for Dark Crystal Shards
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void EssenceofDefeatReagent(int quant = 1000)
    {
        if (Core.CheckInventory("Dark Crystal Shard", quant))
            return;

        Core.AddDrop(tercessBags.Concat(bagDrops).ToArray());
        Core.FarmingLogger("Dark Crystal Shard", quant);

        Core.RegisterQuests(570);
        while (!Bot.ShouldExit && !Core.CheckInventory("Dark Crystal Shard", quant))
        {
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("faerie", "Aracara", "Aracara's Fang", isTemp: false, log: false);
            Core.HuntMonster("hydra", "Hydra Head", "Hydra Scale", isTemp: false, log: false);
            Core.KillVath("Strand of Vath's Hair", 1, isTemp: false);
            Core.HuntMonster("yokaiwar", "O-dokuro's Head", "O-dokuro's Tooth", isTemp: false, log: false);
            Core.KillEscherion("Escherion's Chain", publicRoom: true);

            Core.EquipClass(ClassType.Farm);
            Core.KillMonster("tercessuinotlim", "m2", "Bottom", "Dark Makai", "Defeated Makai", 50, false, log: false);
            Core.JumpWait();

            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("djinn", "Tibicenas", "Tibicenas' Chain", publicRoom: true, log: false);
            Bot.Wait.ForPickup("Dark Crystal Shard");
        }
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Does NWNO from Nulgath's Birthday Gift/Bounty Hunter's Drone Pet
    /// </summary>
    /// <param name=item>Desired item to get</param>
    /// <param name="quant">Desired quantity to get</param>
    public void NewWorldsNewOpportunities(string item = "Any", int quant = 1)
    {
        if (Core.CheckInventory(item, quant) || (!Core.CheckInventory("Nulgath's Birthday Gift") && !Core.CheckInventory("Bounty Hunter's Drone Pet")))
            return;

        if (item != "Any")
        {
            Core.AddDrop(item);
            Core.FarmingLogger(item, quant);
        }
        Core.AddDrop(bagDrops);
        Core.EquipClass(ClassType.Farm);

        Core.RegisterQuests(Core.CheckInventory("Bounty Hunter's Drone Pet") ? 6183 : 6697);
        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant) && !Bot.Inventory.IsMaxStack(item))
        {
            if (!Core.CheckInventory("Slugfit Horn", 5) || !Core.CheckInventory("Cyclops Horn", 3))
            {
                Core.JoinSWF("mobius", "ChiralValley/town-Mobius-21Feb14.swf");
                Core.KillMonster("mobius", "Slugfit", "Bottom", "Slugfit", "Slugfit Horn", 5, log: false);
                Core.KillMonster("mobius", "Slugfit", "Bottom", "Cyclops Warlord", "Cyclops Horn", 3, log: false);
            }
            Core.KillMonster("tercessuinotlim", "m2", "Top", "Dark Makai", "Makai Fang", 5);
            Core.KillMonster("hydra", "Rune2", "Left", "Fire Imp", "Imp Flame", 3, log: false);
            Core.HuntMonster("greenguardwest", "Big Bad Boar", "Wereboar Tusk", 2, log: false);

            if (item != "Any")
                Bot.Wait.ForPickup(item);

            Core.Logger($"{item}: {Bot.Inventory.GetQuantity(item)}/{quant}");
        }
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Farm Diamonds from Evil War Nul quests (does Member one if possible)
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void DiamondEvilWar(int quant = 1000)
    {
        if (Core.CheckInventory("Diamond of Nulgath", quant))
            return;

        Core.AddDrop("Legion Blade", "Dessicated Heart", "Diamond of Nulgath");
        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming {quant} Diamonds");
        int i = 1;
        Core.Join("evilwarnul");

        while (!Bot.ShouldExit && !Core.CheckInventory("Diamond of Nulgath", quant))
        {
            if (Core.IsMember)
                Core.EnsureAccept(2221);
            else
                Core.EnsureAccept(2219);
            Core.HuntMonster("evilwarnul", "Blade Master", "Legion Blade", isTemp: false, log: false);
            Core.HuntMonster("evilwarnul", "Blade Master", "Dessicated Heart", 20, false, log: false);
            Core.HuntMonster("underworld", "Skull Warrior", "Legion Helm", 5, log: false);
            Core.HuntMonster("underworld", "Skull Warrior", "Undead Skull", 3, log: false);
            Core.HuntMonster("underworld", "Skull Warrior", "Legion Champion Medal", 5, log: false);
            if (Core.IsMember)
                Core.EnsureComplete(2221);
            else
                Core.EnsureComplete(2219);
            Bot.Drops.Pickup("Diamond of Nulgath");
            Core.Logger($"Completed x{i++}");
            if (Bot.Inventory.IsMaxStack("Diamond of Nulgath"))
                Core.Logger("Max Stack Hit.");
            else Core.Logger($"Diamond of Nulgath: {Bot.Inventory.GetQuantity("Diamond of Nulgath")}/{quant}");
        }
    }

    /// <summary>
    /// Farms Approvals and Favors in Evil War Nul
    /// </summary>
    /// <param name="quantApproval">Desired quantity for Approvals, 5000 = max stack</param>
    /// <param name="quantFavor">Desired quantity for Favors, 5000 = max stack</param>
    public void ApprovalAndFavor(int quantApproval = 5000, int quantFavor = 5000)
    {
        if (Core.CheckInventory("Nulgath's Approval", quantApproval) && Core.CheckInventory("Archfiend's Favor", quantFavor))
            return;

        Core.AddDrop("Nulgath's Approval", "Archfiend's Favor");

        bool shouldLog = true;
        if (quantApproval > 0 && quantFavor > 0)
        {
            Core.Logger($"Farming Nulgath's Approval ({Bot.Inventory.GetQuantity("Nulgath's Approval")}/{quantApproval}) " +
                            $"and Archfiend's Favor ({Bot.Inventory.GetQuantity("Archfiend's Favor")}/{quantFavor})");
            shouldLog = false;
        }

        Core.EquipClass(ClassType.Farm);

        Core.KillMonster("evilwarnul", "r2", "Down", "*", "Nulgath's Approval", quantApproval, false, shouldLog);
        Core.KillMonster("evilwarnul", "r2", "Down", "*", "Archfiend's Favor", quantFavor, false, shouldLog);
    }

    /// <summary>
    /// Farms specific item with Swindles Return Policy quest
    /// </summary>
    /// <param name="item">Desired Item</param>
    /// <param name="quant">Desired Item quantity</param>
    public void SwindleReturn(string? item = null, int quant = 1000)
    {
        ItemBase? Item = Core.EnsureLoad(7551).Rewards.Find(x => x.Name == item);

        if (Item == null || Core.CheckInventory(Item.Name, quant))
            return;

        Core.AddDrop(Receipt);

        sellMemVoucher = Core.CBOBool("Nation_SellMemVoucher", out bool _sellMemVoucher) && _sellMemVoucher;

        Core.FarmingLogger(Item.Name, quant);

        while (!Bot.ShouldExit && !Core.CheckInventory(Item.Name, quant))
        {
            Core.EnsureAccept(7551);
            Supplies("Unidentified 1");
            Supplies("Unidentified 6");
            Supplies("Unidentified 9");
            Supplies("Unidentified 16");
            Supplies("Unidentified 20");
            ResetSindles();
            string[] locations = new[] { "tercessuinotlim", Core.IsMember ? "Nulgath" : "evilmarsh" };
            string location = locations[new Random().Next(locations.Length)];
            string cell = location == "tercessuinotlim" ? (new Random().Next(2) == 0 ? "m1" : "m2") : "Field1";
            Core.KillMonster(location, cell, "Left", "Dark Makai", "Dark Makai Rune");
            Core.EnsureComplete(7551, Item.ID);
            if (Item.Name != "Voucher of Nulgath" && sellMemVoucher)
                Core.SellItem("Voucher of Nulgath", all: true);

            Core.Logger(Bot.Inventory.IsMaxStack(Item.Name) ? "Max Stack Hit." : $"{Item.Name}: {Bot.Inventory.GetQuantity(Item.Name)}/{quant}");
        }
    }

    /// <summary>
    /// Farms Tainted Gem with Swindle Bulk quest.
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void SwindleBulk(int quant = 1000)
    {
        if (Core.CheckInventory("Tainted Gem", quant))
            return;

        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming {quant} Tainted Gems");

        int i = 1;
        Core.AddDrop("Cubes", "Tainted Gem");
        Core.AddDrop(bagDrops);

        int cubeKillCount = quant % 25 == 0 ? 500 : 25;
        int snowGolemKillCount = quant % 25 == 0 ? 6 : 1;

        while (!Bot.ShouldExit && !Core.CheckInventory("Tainted Gem", quant))
        {
            int questId = quant % 25 == 0 ? 7817 : 569;
            Core.EnsureAccept(questId);
            Core.KillMonster("boxes", "Fort2", "Left", "*", "Cubes", cubeKillCount, false, log: false);
            Core.KillMonster("mountfrost", "War", "Left", "Snow Golem", "Ice Cubes", snowGolemKillCount, log: false);
            Core.EnsureComplete(questId);

            Bot.Drops.Pickup("Tainted Gem");
            Core.Logger($"Completed x{i++}");

            if (Bot.Inventory.IsMaxStack("Tainted Gem"))
                Core.Logger("Max Stack Hit.");
            else
                Core.Logger($"Tainted Gem: {Bot.Inventory.GetQuantity("Tainted Gem")}/{quant}");
        }
    }

    /// <summary>
    /// Farms specified items or a specific item in the specified location.
    /// </summary>
    /// <param name="item">The item to farm. If null, it farms a list of rewards.</param>
    /// <param name="quant">Desired quantity, 1000 = max stack.</param>
    public void FarmContractExchage(string? item = null, int quant = 1)
    {
        if (!Core.CheckInventory("Drudgen the Assistant") || (item != null && Core.CheckInventory(item, quant)))
        {
            if (!Core.CheckInventory("Drudgen the Assistant"))
                Core.Logger("Missing \"Drudgen the Assistant\"");
            return;
        }

        string?[] rewards = { "Tainted Gem", "Dark Crystal Shard", "Gem of Nulgath", "Blood Gem of the Archfiend" };

        Core.EquipClass(ClassType.Farm);
        Core.AddDrop(Core.QuestRewards(870));

        if (item != null)
        {
            ItemBase? Reward = Bot.Quests.EnsureLoad(870)?.Rewards.Find(x => x.Name == item);
            Core.FarmingLogger(Reward.Name, quant > 1 ? quant : Reward.MaxStack);
            while (!Bot.ShouldExit && !Core.CheckInventory(Reward.Name, quant > 1 ? quant : Reward.MaxStack))
            {
                switch (Reward.Name)
                {
                    case "Tainted Gem":
                        Supplies("Diamond of Nulgath", 45);
                        ContractExchange(ChooseReward.TaintedGem, quant > 1 ? quant : Reward.MaxStack);
                        break;
                    case "Dark Crystal Shard":
                        Supplies("Diamond of Nulgath", 45);
                        ContractExchange(ChooseReward.DarkCrystalShard, quant > 1 ? quant : Reward.MaxStack);
                        break;
                    case "Gem of Nulgath":
                        Supplies("Diamond of Nulgath", 45);
                        ContractExchange(ChooseReward.GemofNulgath, quant > 1 ? quant : Reward.MaxStack);
                        break;
                    case "Blood Gem of the Archfiend":
                        Supplies("Diamond of Nulgath", 45);
                        ContractExchange(ChooseReward.BloodGemoftheArchfiend, quant > 1 ? quant : Reward.MaxStack);
                        break;
                    // Add more cases for other rewards
                    default:
                        Core.Logger("Default case");
                        break;
                }
            }
        }
        else
        {
            foreach (string? thing in rewards)
            {
                ItemBase? Reward = Bot.Quests.EnsureLoad(870)?.Rewards.Find(item => item.Name == thing);
                Core.FarmingLogger(Reward.Name, quant);
                while (!Bot.ShouldExit && !Core.CheckInventory(Reward.Name, quant > 1 ? quant : Reward.MaxStack))
                {
                    switch (Reward.Name)
                    {
                        case "Tainted Gem":
                            Supplies("Diamond of Nulgath", 45);
                            ContractExchange(ChooseReward.TaintedGem, quant > 1 ? quant : Reward.MaxStack);
                            break;
                        case "Dark Crystal Shard":
                            Supplies("Diamond of Nulgath", 45);
                            ContractExchange(ChooseReward.DarkCrystalShard, quant > 1 ? quant : Reward.MaxStack);
                            break;
                        case "Gem of Nulgath":
                            Supplies("Diamond of Nulgath", 45);
                            ContractExchange(ChooseReward.GemofNulgath, quant > 1 ? quant : Reward.MaxStack);
                            break;
                        case "Blood Gem of the Archfiend":
                            Supplies("Diamond of Nulgath", 45);
                            ContractExchange(ChooseReward.BloodGemoftheArchfiend, quant > 1 ? quant : Reward.MaxStack);
                            break;
                        // Add more cases for other rewards
                        default:
                            Core.Logger("Default case");
                            break;
                    }
                }
            }
        }
    }




    /// <summary>
    /// Farms Emblem of Nulgath in Shadow Blast Arena
    /// </summary>
    /// <param name="quant">Desired quantity, 500 = max stack</param>
    public void EmblemofNulgath(int quant = 500)
    {
        if (Core.CheckInventory("Emblem of Nulgath", quant))
            return;

        if (!Core.CheckInventory("Nation Round 4 Medal"))
            NationRound4Medal();

        Core.AddDrop("Fiend Seal", "Gem of Domination", "Emblem of Nulgath");
        Core.AddDrop(bagDrops);
        Core.EquipClass(ClassType.Farm);
        Core.FarmingLogger("Emblem of Nulgath", quant);

        Core.RegisterQuests(4748);
        while (!Bot.ShouldExit && !Core.CheckInventory("Emblem of Nulgath", quant))
        {
            Core.HuntMonster("shadowblast", "Shadowrise Guard", "Gem of Domination", 1, false, false);
            Core.HuntMonster("shadowblast", "Legion Fenrir", "Fiend Seal", 25, false, false);
        }
    }

    /// <summary>
    /// Farms the required medals for Nation Round 4 in Shadow Blast Arena.
    /// </summary>
    public void NationRound4Medal()
    {
        foreach (string medal in new[] { "Nation Round 1 Medal", "Nation Round 2 Medal", "Nation Round 3 Medal", "Nation Round 4 Medal" })
        {
            if (Core.CheckInventory(medal))
            {
                Core.Logger($"\"{medal}\" owned.");
            }
            else
            {
                switch (medal)
                {
                    // The Nation Needs YOU!
                    case "Nation Round 1 Medal":
                        Core.EnsureAccept(4744);
                        Core.HuntMonster("shadowblast", "Legion AirStrike", "Legion Rookie Defeated", 5);
                        Core.HuntMonster("shadowblast", "Shadowrise Guard", "Shadowscythe Rookie Defeated", 5);
                        Core.EnsureComplete(4744);
                        break;

                    // Show Me More, Nation-Noob
                    case "Nation Round 2 Medal":
                        Core.EnsureAccept(4745);
                        Core.HuntMonster("shadowblast", "Legion Fenrir", "Legion Veteran Defeated", 7);
                        Core.HuntMonster("shadowblast", "Doombringer", "Shadowscythe Veteran Defeated", 7);
                        Core.EnsureComplete(4745);
                        break;

                    // For the Nation!
                    case "Nation Round 3 Medal":
                        Core.EnsureAccept(4746);
                        Core.HuntMonster("shadowblast", "Legion Cannon", "Legion Elite Defeated", 10);
                        Core.HuntMonster("shadowblast", "Draconic Doomknight", "Shadowscythe Elite Defeated", 10);
                        Core.EnsureComplete(4746);
                        break;

                    // Nulgath Likes Your Style
                    case "Nation Round 4 Medal":
                        Core.EnsureAccept(4747);
                        Core.HuntMonster("shadowblast", "Grimlord Boss", "Grimlord Vanquished");
                        Core.EnsureComplete(4747);
                        break;
                }

                Bot.Drops.Pickup(medal);
                Core.Logger($"Medal {medal} acquired");
            }
        }
    }

    /// <summary>
    /// Farms Totem of Nulgath/Gem of Nulgath with Voucher Item: Totem of Nulgath quest
    /// </summary>
    /// <param name="reward">Which reward to pick (totem or gem)</param>
    public void VoucherItemTotemofNulgath(ChooseReward reward = ChooseReward.TotemofNulgath)
    {
        if (!Core.CheckInventory("Voucher of Nulgath (non-mem)"))
            FarmVoucher(false);

        Core.AddDrop("Gem of Nulgath", "Totem of Nulgath");
        Core.AddDrop(bagDrops);
        Core.Logger($"Reward selected: {reward}");
        Core.EnsureAccept(4778);

        EssenceofNulgath();
        if (!Bot.Quests.CanComplete(4778))
            EssenceofNulgath(65);
        Core.EnsureComplete(4778, (int)reward);
        Bot.Drops.Pickup("Gem of Nulgath");
        Bot.Drops.Pickup("Totem of Nulgath");
    }

    /// <summary>
    /// Farms Essences of Nulgath from Dark Makais in Tercessuinotlim
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack</param>
    public void EssenceofNulgath(int quant = 60)
    {
        if (Core.CheckInventory("Essence of Nulgath", quant))
            return;

        Core.AddDrop("Essence of Nulgath");
        Core.EquipClass(ClassType.Farm);
        Core.KillMonster("tercessuinotlim", "m2", "Bottom", "Dark Makai", "Essence of Nulgath", quant, false);
        Core.JumpWait();
    }

    /// <summary>
    /// Does Nulgath Larvae quest for the desired item
    /// </summary>
    /// <param name="item">Desired item name</param>
    /// <param name="quant">Desired item quantity</param>
    public void NulgathLarvae(string? item = null, int quant = 1)
    {
        Bot.Drops.Add(bagDrops);
        Bot.Drops.Add("Mana Energy for Nulgath");
        if (item != null)
        {
            Core.FarmingLogger(item, quant);
            while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
            {
                Core.EnsureAccept(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                Core.EquipClass(ClassType.Solo);
                Core.HuntMonster("elemental", "Mana Golem", "Mana Energy for Nulgath", 13, isTemp: false);

                Core.EquipClass(ClassType.Farm);
                while (!Bot.ShouldExit && !Core.CheckInventory(item, quant) && Core.CheckInventory("Mana Energy for Nulgath"))
                {
                    Core.EnsureAccept(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                    Core.HuntMonster("elemental", "Mana Falcon", "Charged Mana Energy for Nulgath", 5);
                    Core.EnsureComplete(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                    Bot.Wait.ForPickup(item);
                }
            }
        }
        else
        {
            foreach (string Drop in bagDrops)
            {
                ItemBase? drop = Core.EnsureLoad(Bot.Quests.IsAvailable(2568) ? 2568 : 2566).Rewards.Find(x => x.Name == Drop);
                if (drop == null)
                    continue;

                Core.FarmingLogger(drop.Name, drop.MaxStack);

                while (!Bot.ShouldExit && !Core.CheckInventory(drop.Name, drop.MaxStack))
                {
                    Core.EnsureAccept(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                    Core.EquipClass(ClassType.Solo);
                    Core.HuntMonster("elemental", "Mana Golem", "Mana Energy for Nulgath", 13, isTemp: false);
                    Core.EquipClass(ClassType.Farm);

                    Core.EquipClass(ClassType.Farm);
                    while (!Bot.ShouldExit && !Core.CheckInventory(drop.Name, drop.MaxStack) && Core.CheckInventory("Mana Energy for Nulgath"))
                    {
                        Core.EnsureAccept(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                        Core.HuntMonster("elemental", "Mana Falcon", "Charged Mana Energy for Nulgath", 5);
                        Core.EnsureComplete(Bot.Quests.IsAvailable(2568) ? 2568 : 2566);
                        Bot.Wait.ForPickup(drop.Name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Does Supplies to Spin the Wheel of Chance for the desired item with the best method available.
    /// </summary>
    /// <param name="item">Desired item name.</param>
    /// <param name="quant">Desired item quantity.</param>
    public void Supplies(string? item = null, int quant = 1)
    {
        bool sellMemVoucher = Core.CBOBool("Nation_SellMemVoucher", out bool _sellMemVoucher) && _sellMemVoucher;
        bool returnPolicyDuringSupplies = Core.CBOBool("Nation_ReturnPolicyDuringSupplies", out bool _returnSupplies) && _returnSupplies;

        Core.RegisterQuests(2857);
        Core.EquipClass(ClassType.Solo);

        Core.AddDrop((item != null ? new[] { item } : Enumerable.Empty<string>()).Concat(SuppliesRewards.Concat(sellMemVoucher ? new[] { "Voucher of Nulgath" } : Enumerable.Empty<string>()).Append("Relic of Chaos")).ToArray());

        if (item == null)
        {
            foreach (string Thing in SuppliesRewards)
            {
                var rewards = Core.EnsureLoad(2857).Rewards;
                ItemBase? Item = rewards.Find(x => x.Name == Thing);

                if (Core.CheckInventory(CragName))
                    BambloozevsDrudgen(Item.Name, Item.MaxStack);
                else
                {    // Find the corresponding item in quest rewards

                    while (!Bot.ShouldExit && Item != null && !Core.CheckInventory(Item.Name, Item.MaxStack))
                    {
                        Core.KillEscherion(Item.Name, Item.MaxStack, log: false);

                        if (item != "Voucher of Nulgath" && _sellMemVoucher && Core.CheckInventory("Voucher of Nulgath"))
                        {
                            while (!Bot.ShouldExit && (Bot.Player.HasTarget || Bot.Player.InCombat) && Bot.Player.Cell != "Enter")
                            {
                                Bot.Combat.CancelTarget();
                                Bot.Wait.ForCombatExit();
                                Core.Jump("Enter", "Spawn");
                                Bot.Sleep(Core.ActionDelay);
                            }

                            Bot.Wait.ForPickup("Voucher of Nulgath");
                            Core.SellItem("Voucher of Nulgath", all: true);
                            Bot.Wait.ForItemSell();
                        }
                    }
                }
            }
        }
        else // Handle the case when item is not null
        {
            if (Core.CheckInventory(CragName))
                BambloozevsDrudgen(item, quant);
            else
            {
                while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
                {
                    Core.KillEscherion(item, quant, log: false);

                    if (item != "Voucher of Nulgath" && _sellMemVoucher && Core.CheckInventory("Voucher of Nulgath"))
                    {
                        Core.JumpWait();

                        while (!Bot.ShouldExit && (Bot.Player.HasTarget || Bot.Player.InCombat) && Bot.Player.Cell != "Enter")
                        {
                            Bot.Combat.CancelTarget();
                            Bot.Wait.ForCombatExit();
                            Core.Jump("Enter", "Spawn");
                        }

                        Bot.Wait.ForPickup("Voucher of Nulgath");
                        Core.SellItem("Voucher of Nulgath", all: true);
                        Bot.Wait.ForItemSell();
                    }
                }
            }
        }

        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Does "The Assistant" quest for the desired item.
    /// </summary>
    /// <param name="item">Desired item name. Pass null to farm all available drops.</param>
    /// <param name="quant">Desired item quantity.</param>
    /// <param name="farmGold">Whether to farm gold (default: true).</param>
    /// <param name="Reward">Swindles Return Policy quest reward (default: None).</param>
    public void TheAssistant(string? item = null, int quant = 1000, bool farmGold = true, SwindlesReturnReward Reward = SwindlesReturnReward.None)
    {
        if (item != null && Core.CheckInventory(item, quant))
            return;

        // List of available drops for "The Assistant" quest
        string[] selectedDrops = item != null ? new string[] { item } : bagDrops[..^11];
        Core.AddDrop(selectedDrops);

        // Check if return policy and sell voucher are active
        sellMemVoucher = Core.CBOBool("Nation_SellMemVoucher", out bool _sellMemVoucher) && _sellMemVoucher;
        returnPolicyDuringSupplies = Core.CBOBool("Nation_ReturnPolicyDuringSupplies", out bool _returnSupplies) && _returnSupplies;

        Core.Logger(returnPolicyDuringSupplies ? "Return Policy During Supplies: true" : "Return Policy During Supplies: false");
        Core.Logger($"Sell Voucher of Nulgath: {sellMemVoucher}");


        string[]? rPDSuni = null;
        if (returnPolicyDuringSupplies)
        {
            rPDSuni = new[] { Uni(1), Uni(6), Uni(9), Uni(16), Uni(20) };
            Core.AddDrop(rPDSuni);
            Core.AddDrop("Blood Gem of Nulgath");
        }

        // Register the "Swindles Return Policy" quest if specified
        if (_returnSupplies && Reward == SwindlesReturnReward.None)
            Core.RegisterQuests(7551);

        // Farm all drops if 'item' is null
        if (item == null)
        {
            Core.Logger("Null method");
            foreach (string Thing in selectedDrops)
            {
                // Find the corresponding item in quest rewards
                var rewards = Core.EnsureLoad(2859).Rewards;
                ItemBase? Item = rewards.Find(x => x.Name == Thing);

                if (Item == null)
                    continue;

                // Continue farming until the desired item quantity is obtained
                while (!Bot.ShouldExit && !Core.CheckInventory(Item.Name, Item.MaxStack))
                {
                    LogItemQuant2(Item, Item.MaxStack);
                    if (farmGold)
                        Farm.Gold(1000000);

                    Core.EnsureAccept(2859);
                    Core.BuyItem("yulgar", 41, "War-Torn Memorabilia", 10);
                    Core.EnsureCompleteMulti(2859);

                    // Process "Swindles Return Policy" quest if return policy is active
                    if (Core.CheckInventory(rPDSuni) && _returnSupplies)
                    {
                        var rewards2 = Core.EnsureLoad(7551).Rewards;
                        ItemBase? Item2 = rewards2.Find(x => x.ID == Item.ID);

                        if (Item2 == null)
                            continue;
                        ResetSindles();

                        Core.FarmingLogger(Item2.Name, Item2.MaxStack);
                        Core.EnsureAccept(7551);
                        string[] locations = new[] { "tercessuinotlim", Core.IsMember ? "Nulgath" : "evilmarsh" };
                        string location = locations[new Random().Next(locations.Length)];
                        string cell = location == "tercessuinotlim" ? (new Random().Next(2) == 0 ? "m1" : "m2") : "Field1";
                        Core.KillMonster(location, cell, "Left", "Dark Makai", "Dark Makai Rune");

                        if (Reward != SwindlesReturnReward.None)
                            Core.EnsureComplete(7551, Item2.ID);
                    }
                }
            }
            Core.CancelRegisteredQuests();
        }
        else
        {
            Core.Logger("Non-null method");
            // Continue farming the specified item until the desired quantity is obtained
            while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
            {
                LogItemQuant(item, quant);
                if (farmGold)
                    Farm.Gold(1000000);

                Core.EnsureAccept(2859);
                Core.BuyItem("yulgar", 41, "War-Torn Memorabilia", 10);
                Bot.Wait.ForItemBuy(40);
                Core.EnsureCompleteMulti(2859);

                // Process "Swindles Return Policy" quest if return policy is active
                if (Core.CheckInventory(rPDSuni) && _returnSupplies)
                {
                    var rewards2 = Core.EnsureLoad(7551).Rewards;
                    ItemBase? Item2 = rewards2.Find(x => x.ID == (int)Reward);

                    if (Item2 == null)
                        continue;
                    ResetSindles();

                    Core.FarmingLogger(Item2.Name, Item2.MaxStack);
                    Core.EnsureAccept(7551);

                    string[] locations = new[] { "tercessuinotlim", Core.IsMember ? "Nulgath" : "evilmarsh" };
                    string location = locations[new Random().Next(locations.Length)];
                    string cell = location == "tercessuinotlim" ? (new Random().Next(2) == 0 ? "m1" : "m2") : "Field1";
                    Core.KillMonster(location, cell, "Left", "Dark Makai", "Dark Makai Rune");


                    if (Reward != SwindlesReturnReward.None)
                        Core.EnsureComplete(7551, (int)Reward);
                }
            }

        }
    }

    public void ResetSindles()
    {
        Core.EnsureAccept(7551);
        Bot.Wait.ForQuestAccept(7551);
        Core.AbandonQuest(7551);
        Core.EnsureAccept(7551);
    }

    /// <summary>
    /// Logs the quantity of the specified item after a time interval.
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired item quantity</param>
    void LogItemQuant(string item, int quant)
    {
        // Check if the specified item is in inventory
        if (!Core.CheckInventory(item))
            return;

        // Get the initial quantity of the item in the inventory
        int startQuant = Bot.Inventory.GetQuantity(item);

        // Wait for a short period (e.g., 1.5 seconds) to allow the item quantity to change
        // (e.g., after completing a quest, the quantity might increase)
        Bot.Sleep(1500);

        // Get the current quantity of the item in the inventory
        int currentQuant = Bot.Inventory.GetQuantity(item);

        // If the quantity changes or increases during the interval, log the updated quantity
        if (currentQuant != startQuant || currentQuant > startQuant)
        {
            Core.FarmingLogger(item, quant);

            // Wait for a short period again (optional)
            Bot.Sleep(1500);
        }
    }

    /// <summary>
    /// Logs the quantity of the specified item object after a time interval.
    /// </summary>
    /// <param name="item">Item object</param>
    /// <param name="quant">Desired item quantity</param>
    void LogItemQuant2(ItemBase item, int maxStack)
    {
        // Check if the specified item is in inventory
        if (!Core.CheckInventory(item.Name))
            return;

        // Get the initial quantity of the item in the inventory
        int startQuant = item.Quantity;

        // Wait for a short period (e.g., 1.5 seconds) to allow the item quantity to change
        // (e.g., after completing a quest, the quantity might increase)
        Bot.Sleep(1500);

        // Get the current quantity of the item in the inventory
        int currentQuant = item.Quantity;

        // If the quantity changes or increases during the interval, log the updated quantity
        if (currentQuant > startQuant)
        {
            Core.FarmingLogger(item.Name, item.MaxStack);

            // Wait for a short period again (optional)
            Bot.Sleep(1500);
        }
    }

    /// <summary>
    /// Does the "Bamblooze vs. Drudgen" quest for the desired item.
    /// </summary>
    /// <param name="item">Desired item name</param>
    /// <param name="quant">Desired item quantity</param>
    public void BambloozevsDrudgen(string? item = null, int quant = 1)
    {
        if (!Core.CheckInventory(CragName) || Core.CheckInventory(item, quant))
            return;

        Core.AddDrop("Relic of Chaos", "Tainted Core");
        Core.AddDrop(string.IsNullOrEmpty(item) ? bagDrops : new string[] { item });


        bool hasOBoNPet = Core.IsMember && Core.CheckInventory("Oblivion Blade of Nulgath") &&
                          Bot.Inventory.Items.Any(obon => obon.Category == Skua.Core.Models.Items.ItemCategory.Pet && obon.Name == "Oblivion Blade of Nulgath");
        if (hasOBoNPet || Core.CheckInventory("Oblivion Blade of Nulgath Pet (Rare)"))
            Core.AddDrop("Tainted Soul");

        bool returnPolicyDuringSupplies = Core.CBOBool("Nation_ReturnPolicyDuringSupplies", out bool _returnSupplies);
        bool sellMemVoucher = Core.CBOBool("Nation_SellMemVoucher", out bool _sellMemVoucher);

        Core.Logger($"Sell Voucher of Nulgath: {_sellMemVoucher}");

        if (_returnSupplies)
            Core.AddDrop(Uni(1), Uni(6), Uni(9), Uni(16), Uni(20));

        Dictionary<string, int> rewardItemIds = new()
        {
            ["Dark Crystal Shard"] = 123,
            ["Diamond of Nulgath"] = 456,
            ["Gem of Nulgath"] = 789,
            ["Tainted Gem"] = 101,
            ["Unidentified 10"] = 202
        };

        if (!string.IsNullOrEmpty(item))
            Core.FarmingLogger(item, quant);

        // Choose the appropriate quest based on pet availability
        if (Core.CheckInventory("Oblivion Blade of Nulgath Pet (Rare)") && Core.IsMember)
            Core.RegisterQuests(2857, 609, 599);
        else if (hasOBoNPet)
            Core.RegisterQuests(2857, 609, 2561);
        else
            Core.RegisterQuests(2857, 609);

        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            Core.KillMonster("evilmarsh", "End", "Left", "Tainted Elemental", log: false);

            if (item != "Voucher of Nulgath" && _sellMemVoucher && Core.CheckInventory("Voucher of Nulgath"))
            {
                while (!Bot.ShouldExit && (Bot.Player.HasTarget || Bot.Player.InCombat) && Bot.Player.Cell != "Enter")
                {
                    Bot.Combat.CancelTarget();
                    Bot.Wait.ForCombatExit();
                    Core.Jump("Enter", "Spawn");
                    Bot.Sleep(Core.ActionDelay);
                }

                Bot.Drops.Pickup("Voucher of Nulgath");
                Core.SellItem("Voucher of Nulgath", all: true);
                Bot.Wait.ForItemSell();
            }


            if (returnPolicyDuringSupplies && Core.CheckInventory(new[] { Uni(1), Uni(6), Uni(9), Uni(16), Uni(20) }))
            {
                ResetSindles();
                string[] locations = new[] { "tercessuinotlim", Core.IsMember ? "Nulgath" : "evilmarsh" };
                string location = locations[new Random().Next(locations.Length)];
                string cell = location == "tercessuinotlim" ? (new Random().Next(2) == 0 ? "m1" : "m2") : "Field1";
                Core.KillMonster(location, cell, "Left", "Dark Makai", "Dark Makai Rune");

                if (item != null && rewardItemIds.TryGetValue(item, out int itemId))
                    Core.EnsureCompleteMulti(7551, itemId);
                else
                {
                    foreach (var rewardItemPair in rewardItemIds)
                    {
                        string rewardItem = rewardItemPair.Key;
                        itemId = rewardItemPair.Value;

                        if (!Bot.Inventory.IsMaxStack(rewardItem))
                        {
                            Core.EnsureCompleteMulti(7551, itemId);
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Does the "AssistingDrudgen" Quest for Fiend Tokens (and other possible drops).
    /// Requires either "Drudgen the Assistant" or "Twin Blade of Nulgath" to accept.
    /// </summary>
    /// <param name="item">Desired item name</param>
    /// <param name="quant">Desired item quantity</param>
    public void AssistingDrudgen(string item = "Any", int quant = 1)
    {
        if (Core.CheckInventory(item, quant) || !Core.CheckInventory("Drudgen the Assistant") || !Core.CheckInventory("Twin Blade of Nulgath") || !Bot.Player.IsMember)
            return;

        if (!Bot.Quests.IsAvailable(3826))
        {
            Core.Logger("Quest \"Seal of Light\"[Daily] is not available yet today.");
            return;
        }

        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            Core.EnsureAccept(5816);
            Core.HuntMonster("willowcreek", "Hidden Spy", "The Secret 1", isTemp: false);
            EssenceofNulgath(20);
            ApprovalAndFavor(50, 50);
            Core.KillMonster("boxes", "Fort2", "Left", "*", "Cubes", 50, false);
            Core.KillMonster("shadowblast", "r13", "Left", "*", "Fiend Seal", 10, false);
            Bot.Quests.UpdateQuest(3824);
            if (Bot.Quests.IsAvailable(3826) && !Core.CheckInventory(25026))
            {
                Core.EnsureAccept(3826);
                Core.HuntMonster("alteonbattle", "*", "Seal of Light");
                Core.EnsureComplete(3826);
            }
            Core.EnsureComplete(5816);
        }
    }

    /// <summary>
    /// Completes the Feed the Fiend quest to obtain the specified item.
    /// </summary>
    /// <param name="item">The item to obtain (default: "Fiend Token").</param>
    /// <param name="quant">The quantity of the item to obtain (default: 30).</param>
    public void FeedtheFiend(string item = "Fiend Token", int quant = 30)
    {
        // Check if the desired item is already in inventory or if the player is not a member
        if (Core.CheckInventory(item, quant) || !Core.IsMember)
            return;

        // Update and register the necessary quests
        Bot.Quests.UpdateQuest(2215);
        Core.RegisterQuests(3053);

        // Equip the appropriate class for the quest
        Core.EquipClass(ClassType.Solo);

        // Continue the quest until the desired item and quantity are obtained
        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            // Hunt monsters to complete the quest
            FarmDiamondofNulgath(1);
            Core.HuntMonster("lair", "Red Dragon", "Dragon Fiend Gem", 13, isTemp: false);
            Core.KillMonster("battleunderd", "r5", "Left", "Glacial Horror", "Glacial Bones", 3, isTemp: false);
            Core.HuntMonster("dreammaze", "Screamfeeder", "Screamfeeder Heart", isTemp: false);
        }

        // Wait for the item to be picked up and cancel any registered quests
        Bot.Wait.ForPickup(item);
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Completes the Void Knight Sword Quest to obtain the specified item.
    /// </summary>
    /// <param name="item">The item to obtain (default: "Any").</param>
    /// <param name="quant">The quantity of the item to obtain (default: 1).</param>
    public void VoidKightSwordQuest(string item = "Any", int quant = 1)
    {
        // Check if the desired item is already in inventory or if the required items are missing
        if (Core.CheckInventory(item, quant) || (!Core.CheckInventory(38275) && !Core.CheckInventory(38254)))
            return;

        // Add drops based on the provided item or bag drops
        Core.AddDrop(bagDrops);
        Core.AddDrop(item);

        // Check if the player should sell the Voucher of Nulgath
        _ = Core.CBOBool("Nation_SellMemVoucher", out bool _sellMemVoucher) && _sellMemVoucher;

        if (item != "Any")
            Core.FarmingLogger(item, quant);

        // Register the appropriate quest based on the available items
        Core.RegisterQuests(Core.CheckInventory(38275) ? 5662 : 5659);

        // Continue the quest until the desired item and quantity are obtained
        while (!Bot.ShouldExit && !Core.CheckInventory(item, quant))
        {
            // Equip the Solo class and hunt monsters for quest completion
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("mobius", "Slugfit", "Slugfit Horn", 5);
            Core.HuntMonster("faerie", "Aracara", "Aracara Silk");

            // Equip the Farm class and hunt monsters for quest completion
            Core.EquipClass(ClassType.Farm);
            Core.KillMonster("tercessuinotlim", "m2", "Top", "Dark Makai", "Makai Fang", 5);
            Core.HuntMonster("hydra", "Fire Imp", "Imp Flame", 3);
            Core.HuntMonster("battleunderc", "Crystalized Jellyfish", "Aquamarine of Nulgath", 3, false);

            // Pick up any dropped items
            Bot.Drops.Pickup(bagDrops);
        }

        // Cancel any registered quests once the desired items are obtained
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Do Diamond Exchange quest 1 time, if farmDiamond is true, will farm 15 Diamonds before if needed
    /// </summary>
    /// <param name="farmDiamond">Whether or not farm Diamonds</param>
    public void DiamondExchange(bool farmDiamond = true)
    {
        if ((!Core.CheckInventory("Diamond of Nulgath", 15) && !farmDiamond) || !Core.CheckInventory(CragName))
            return;

        Core.AddDrop("Diamond of Nulgath");

        if (farmDiamond)
            BambloozevsDrudgen("Diamond of Nulgath", 15);
        Core.EnsureAccept(869);
        string[] locations = new[] { "tercessuinotlim", Core.IsMember ? "Nulgath" : "evilmarsh" };
        string location = locations[new Random().Next(locations.Length)];
        string cell = location == "tercessuinotlim" ? (new Random().Next(2) == 0 ? "m1" : "m2") : "Field1";
        Core.KillMonster(location, cell, "Left", "Dark Makai", "Dark Makai Sigil", log: false);

        Core.EnsureComplete(869);
        Core.Logger("Completed");
    }

    /// <summary>
    /// Do Contract Exchange quest 1 time, if <paramref name="farmUni13"/> is true, will farm Uni 13 before if needed
    /// </summary>
    /// <param name="reward">Desired reward</param>
    /// <param name="farmUni13">Whether or not farm Uni 13</param>
    public void ContractExchange(ChooseReward reward, int quant, bool farmUni13 = true)
    {
        if ((!Core.CheckInventory("Unidentified 13") && !farmUni13) || !Core.CheckInventory("Drudgen the Assistant"))
        {
            if ((!Core.CheckInventory("Unidentified 13") && !farmUni13))
                Core.Logger($"{farmUni13} is probably set to false, please have a dev change it");
            if (!Core.CheckInventory("Drudgen the Assistant"))
                Core.Logger("Missing \"Drudgen the Assistant\"");
            return;
        }


        Core.AddDrop(bagDrops);
        Core.EquipClass(ClassType.Solo);
        Core.FarmingLogger(reward.ToString(), quant);
        while (!Bot.ShouldExit && !Core.CheckInventory(reward.ToString(), quant))
        {
            if (farmUni13 && !Core.CheckInventory("Unidentified 13"))
                FarmUni13(3);
            Core.EnsureAccept(870);
            Core.KillMonster("tercessuinotlim", "m4", "Top,", "Shadow of Nulgath", "Blade Master Rune", log: false);
            Core.EnsureComplete(870, (int)reward);
            Core.Logger($"Exchanged for {reward}");
        }
    }

    /// <summary>
    /// Does Swindles Dirt-y Deeds Done Dirt Cheap quest, only use if you have /TowerofDoom10 completed and a good solo class
    /// </summary>
    /// <param name="quant"></param>
    public void DirtyDeedsDoneDirtCheap(int quant = 1000)
    {
        if (Core.CheckInventory("Unidentified 10", quant))
            return;

        Core.AddDrop("Emerald Pickaxe", "Seraphic Grave Digger Spade", "Unidentified 10", "Receipt of Swindle", "Blood Gem of the Archfiend");

        if (!Core.CheckInventory("Emerald Pickaxe"))
            Core.KillEscherion("Emerald Pickaxe");

        if (!Core.CheckInventory("Seraphic Grave Digger Spade"))
            Core.KillMonster("legioncrypt", "r1", "Top", "Gravedigger", "Seraphic Grave Digger Spade", isTemp: false, log: false);
        Core.EquipClass(ClassType.Solo);
        int i = 1;
        while (!Bot.ShouldExit && !Core.CheckInventory("Unidentified 10", quant))
        {
            Core.EnsureAccept(7818);
            Core.HuntMonster("towerofdoom10", "Slugbutter", "Slugbutter Digging Advice", publicRoom: true, log: false);
            Core.HuntMonster("crownsreach", "Chaos Tunneler", "Chaotic Tunneling Techniques", 2, log: false);
            Core.HuntMonster("downward", "Crystal Mana Construct", "Crystalized Corporate Digging Secrets", 3, log: false);
            Core.EnsureComplete(7818);
            Core.Logger($"Completed x{i++}");
            if (Bot.Inventory.IsMaxStack("Unidentified 10"))
                Core.Logger("Max Stack Hit.");
            else Core.Logger($"Unidentified 10: {Bot.Inventory.GetQuantity("Unidentified 10")}/{quant}");
        }
    }

    /// <summary>
    /// Farms Unidentified 13 with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 13 = max stack</param>
    public void FarmUni13(int quant = 13)
    {
        if (Core.CheckInventory("Unidentified 13", quant))
            return;

        Core.AddDrop("Unidentified 13");
        quant = quant > 13 ? 13 : quant;

        if (Core.CheckInventory(CragName))
            while (!Bot.ShouldExit && !Core.CheckInventory("Unidentified 13", quant))
                DiamondExchange();
        NewWorldsNewOpportunities("Unidentified 13", quant); //1minute turning  = 1x guaranteed
        VoidKightSwordQuest("Unidentified 13", quant);
        NulgathLarvae("Unidentified 13", quant);
    }

    /// <summary>
    /// Farms Unidentified 10 with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void FarmUni10(int quant = 1000)
    {
        if (Core.CheckInventory("Unidentified 10", quant))
            return;

        Core.AddDrop("Unidentified 10");

        BambloozevsDrudgen("Unidentified 10", quant);
        NulgathLarvae("unidentified 10", quant);
    }

    /// <summary>
    /// Farms Dark Crystal Shard with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void FarmDarkCrystalShard(int quant = 1000)
    {
        if (Core.CheckInventory("Dark Crystal Shard", quant))
            return;

        Core.AddDrop("Dark Crystal Shard");
        FarmContractExchage("Dark Crystal Shard", quant);
        NewWorldsNewOpportunities("Dark Crystal Shard", quant); //1minute turning  = 1x guaranteed
        VoidKightSwordQuest("Dark Crystal Shard", quant);
        Supplies("Dark Crystal Shard", quant); //xx:xx time turnin = 10% chance
        EssenceofDefeatReagent(quant);
    }

    /// <summary>
    /// Farms Diamond of Nulgath with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 1000 = max stack</param>
    public void FarmDiamondofNulgath(int quant = 1000)
    {
        if (Core.CheckInventory("Diamond of Nulgath", quant))
            return;

        Core.AddDrop("Diamond of Nulgath");

        VoidKightSwordQuest("Diamond of Nulgath", quant);
        if (!Core.CheckInventory(new[] { CragName }))
            DiamondEvilWar(quant);
        else Supplies("Diamond of Nulgath", quant);
    }

    /// <summary>
    /// Farms Fiend Tokens using various methods.
    /// </summary>
    /// <param name="quant">Desired quantity of Fiend Tokens, 30 = default stack size.</param>
    public void FarmFiendToken(int quant = 30)
    {
        // Check if Fiend Tokens are already in inventory
        if (Core.CheckInventory("Fiend Token", quant))
            return;

        // Try different quest methods to obtain Fiend Tokens
        VoidKightSwordQuest("Fiend Token", quant);
        AssistingDrudgen("Fiend Token", quant);
        FeedtheFiend();
    }

    /// <summary>
    /// Farms Gem of Nulgath with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 300 = max stack</param>
    public void FarmGemofNulgath(int quant = 1000)
    {
        if (Core.CheckInventory("Gem of Nulgath", quant))
            return;

        FarmContractExchage("Gem of Nulgath", quant);
        Core.AddDrop("Gem of Nulgath");
        VoidKightSwordQuest("Gem of Nulgath", quant);
        while (!Bot.ShouldExit && !Core.CheckInventory("Gem of Nulgath", quant))
            VoucherItemTotemofNulgath(ChooseReward.GemofNulgath);
    }

    /// <summary>
    /// Farms Blood Gem of the Archfiend with the best method available
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack</param>
    public void FarmBloodGem(int quant = 100)
    {
        if (Core.CheckInventory("Blood Gem of the Archfiend", quant))
            return;

        Core.AddDrop("Blood Gem of the Archfiend");

        FarmContractExchage("Blood Gem of the Archfiend", quant);
        NewWorldsNewOpportunities("Blood Gem of the Archfiend", quant);
        VoidKightSwordQuest("Blood Gem of the Archfiend", quant);
        BloodyChaos(quant, true);
        KisstheVoid(quant);
    }

    public void FarmTaintedGem(int quant = 100)
    {
        if (Core.CheckInventory("Tainted Gem", quant))
            return;

        Core.AddDrop("Tainted Gem");
        FarmContractExchage("Tainted Gem", quant);
        ForgeTaintedGems(quant);
        Supplies("Tainted Gem", quant);
    }

    /// <summary>
    /// Completes the lair questline to unlock Nation mats if not completed.
    /// </summary>
    public void DragonSlayerReward()
    {
        int[] questIds = { 165, 166, 167, 168, 169 };
        string[] questMonsterNames = { "Water Draconian", "Hole", "Ledge", "Red Dragon", "Hole" };
        string[] questMonsterItems = { "Dragonslayer Veteran Medal", "Dragonslayer Sergeant Medal", "Dragonslayer Captain Medal", "Dragonslayer Marshal Medal", "Wisp of Dragonspirit" };
        int[] requiredQuantities = { 8, 8, 8, 8, 12 };
        ClassType[] questClasses = { ClassType.Farm, ClassType.Farm, ClassType.Farm, ClassType.Solo, ClassType.Farm };

        for (int i = 0; i < questIds.Length; i++)
        {
            int questId = questIds[i];
            string monsterName = questMonsterNames[i];
            string monsterItem = questMonsterItems[i];
            int requiredQuantity = requiredQuantities[i];
            ClassType questClass = questClasses[i];

            // Check if the quest is already completed
            if (Core.isCompletedBefore(questId))
                continue;

            // Equip the required class for the quest
            Core.EquipClass(questClass);

            // Accept the quest and hunt the required monster
            Core.EnsureAccept(questId);
            Core.HuntMonster("lair", monsterName, monsterItem, requiredQuantity);
            Core.EnsureComplete(questId);
        }
    }

    /// <summary>
    /// Farms Totem of Nulgath with the best method available.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack.</param>
    public void FarmTotemofNulgath(int quant = 100)
    {
        // Check if Totem of Nulgath is already in inventory
        if (Core.CheckInventory("Totem of Nulgath", quant))
            return;

        NewWorldsNewOpportunities("Totem of Nulgath", quant);
        VoidKightSwordQuest("Totem of Nulgath", quant);

        // Continue farming until desired quantity is reached
        while (!Bot.ShouldExit && !Core.CheckInventory("Totem of Nulgath", quant))
        {
            // Complete the Voucher Item: Totem of Nulgath quest with the TotemofNulgath reward
            VoucherItemTotemofNulgath(ChooseReward.TotemofNulgath);

            if (Bot.Inventory.IsMaxStack("Totem of Nulgath"))
                Core.Logger("Max Stack Hit.");
            else
                Core.Logger($"Totem of Nulgath: {Bot.Inventory.GetQuantity("Totem of Nulgath")}/{quant}");
        }
    }

    /// <summary>
    /// Do Bloody Chaos quest for Blood Gems.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack.</param>
    /// <param name="relic">Indicates if Relic of Chaos supplies are used.</param>
    public void BloodyChaos(int quant = 100, bool relic = false)
    {
        // Check if Blood Gem Of The Archfiend is already in inventory or player level is below 80
        if (Core.CheckInventory("Blood Gem Of The Archfiend", quant) || Bot.Player.Level < 80)
            return;

        // Add drops for the quest
        Core.AddDrop("Blood Gem of the Archfiend", "Hydra Scale Piece");
        if (relic)
            Core.AddDrop(BloodyChaosSupplies);

        // Log the farming for Blood Gem Of The Archfiend
        Core.FarmingLogger("Blood Gem Of The Archfiend", quant);

        // Register the quest depending on whether Relic of Chaos supplies are used
        Core.RegisterQuests(relic ? new[] { 7816, 2857 } : new[] { 7816 });

        // Continue farming until the desired quantity is reached
        while (!Bot.ShouldExit && !Core.CheckInventory("Blood Gem Of The Archfiend", quant))
        {
            // Equip Solo class and kill Escherion
            Core.EquipClass(ClassType.Solo);
            Core.KillEscherion("Escherion's Helm", isTemp: false);

            // Equip Solo class and kill Vath
            Core.EquipClass(ClassType.Solo);
            Core.KillVath("Shattered Legendary Sword of Dragon Control", isTemp: false);

            // Equip Solo class and hunt Hydra Head 85
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("hydrachallenge", "Hydra Head 85", "Hydra Scale Piece", 200, false);
        }

        // Cancel the registered quests after farming is completed
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Do Kiss the Void quest for Blood Gems.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack</param>
    public void KisstheVoid(int quant = 100)
    {
        if (Core.CheckInventory("Blood Gem of the Archfiend", quant))
            return;

        Core.AddDrop("Tendurrr The Assistant", "Fragment of Chaos", "Blood Gem of the Archfiend");
        Core.EquipClass(ClassType.Farm);
        Core.Logger($"Farming {quant} Blood Gems");

        int i = 1;

        while (!Bot.ShouldExit && !Core.CheckInventory("Blood Gem of the Archfiend", quant))
        {
            Core.EnsureAccept(3743);

            if (!Core.CheckInventory("Tendurrr The Assistant"))
            {
                Core.KillMonster("tercessuinotlim", "m2", "Bottom", "Dark Makai", "Tendurrr The Assistant", isTemp: false, log: false);
                Core.JumpWait();
            }

            Core.HuntMonster("lair", "Water Draconian", "Fragment of Chaos", 80, false, log: false);
            Core.KillMonster("evilwarnul", "r13", "Left", "Legion Fenrir", "Broken Betrayal Blade", 8, log: false);
            Core.EnsureComplete(3743);

            Bot.Wait.ForPickup("Blood Gem of the Archfiend");
            Core.Logger($"Completed x{i++}");

            if (Bot.Inventory.IsMaxStack("Blood Gem of the Archfiend"))
                Core.Logger("Max Stack Hit.");
            else
                Core.Logger($"Blood Gem of the Archfiend: {Bot.Inventory.GetQuantity("Blood Gem of the Archfiend")}/{quant}");
        }
    }

    /// <summary>
    /// Farms Gemstone Receipt of Nulgath with specific quantities.
    /// </summary>
    /// <param name="quant">Desired quantity of Gemstone Receipt of Nulgath</param>
    public void GemStoneReceiptOfNulgath(int quant = 10)
    {
        const int demandingApprovalQuest = 4917;
        const int receiptOfNulgathQuest = 4924;
        const int dwoboCoinQuestMember = 4798;
        const int receiptItemId = 33451;

        if (!Core.IsMember)
        {
            Core.Logger("This quest requires membership to be able to accept it.");
            return;
        }

        if (Core.CheckInventory("Gemstone Receipt of Nulgath", quant))
            return;

        Core.AddDrop("Gemstone Receipt of Nulgath", "Receipt of Nulgath");

        while (!Bot.ShouldExit && !Core.CheckInventory("Gemstone Receipt of Nulgath", quant))
        {
            Core.EnsureAccept(demandingApprovalQuest);

            FarmUni13(3);
            Farm.VampireREP();

            if (!Core.CheckInventory(receiptItemId))
            {
                DwoboCoin(100, dwoboCoinQuestMember);
                Core.BuyItem("crashedruins", 1212, receiptItemId);
            }

            Core.EnsureAccept(receiptOfNulgathQuest);
            ApprovalAndFavor(0, 100);
            Core.EquipClass(ClassType.Farm);
            Core.HuntMonster("Extinction", "Control Panel", "Coal", 15, isTemp: false, log: false);
            DwoboCoin(10, dwoboCoinQuestMember);
            EssenceofNulgath(10);
            Core.BuyItem("Tercessuinotlim", 68, "Blade of Affliction");
            Core.EnsureComplete(receiptOfNulgathQuest);
            Bot.Wait.ForPickup("Receipt of Nulgath");

            FarmVoucher(member: true);
            FarmVoucher(member: false);
            EssenceofNulgath(100);
            FarmTotemofNulgath(1);
            Core.EquipClass(ClassType.Solo);
            Core.HuntMonster("ShadowfallWar", "Bonemuncher", "Ultimate Darkness Gem", 5, isTemp: false);
            Core.EnsureComplete(demandingApprovalQuest);
            Bot.Wait.ForPickup("Gemstone Receipt of Nulgath");
        }
    }

    /// <summary>
    /// Farms Dwobo Coins with the specified quest and quantity.
    /// </summary>
    /// <param name="quant">Desired quantity of Dwobo Coins</param>
    /// <param name="questId">Quest ID for the Dwobo Coin quest</param>
    public void DwoboCoin(int quant, int questId)
    {
        Core.RegisterQuests(questId);

        while (!Bot.ShouldExit && !Core.CheckInventory("Dwobo Coin", quant))
        {
            int unluckyExplorerCount = Core.IsMember ? 8 : 10;
            int spacetimeAnomalyCount = Core.IsMember ? 5 : 7;

            Core.KillMonster("crashruins", "r2", "Left", "Unlucky Explorer", "Ancient Treasure", unluckyExplorerCount, log: false);
            Core.KillMonster("crashruins", "r2", "Left", "Spacetime Anomaly", "Pieces of Future Tech", spacetimeAnomalyCount, log: false);
            Core.HuntMonster("crashruins", "Cluckmoo Idol", "Idol Heart", log: false);
        }

        Bot.Wait.ForPickup("Dwobo Coin");
        Core.CancelRegisteredQuests();
    }

    /// <summary>
    /// Farm Gemstones of Nulgath for specified quantities
    /// </summary>
    /// <param name="bloodStone">Desired quantity of Bloodstone of Nulgath</param>
    /// <param name="quartz">Desired quantity of Quartz of Nulgath</param>
    /// <param name="tanzanite">Desired quantity of Tanzanite of Nulgath</param>
    /// <param name="uniGemStone">Desired quantity of Unidentified Gemstone of Nulgath</param>
    public void GemStonesOfnulgath(int bloodStone = 100, int quartz = 100, int tanzanite = 100, int uniGemStone = 1)
    {
        const int gemstonesForNulgathQuest = 4918;
        const int skeletalWarriorQuest1 = 374;
        const int skeletalWarriorQuest2 = 375;
        const int boneTerrorQuest = 376;
        const int unidentifiedWeaponQuest = 377;

        if (!Core.CheckInventory("Gemstone of Nulgath") && !Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it.");
            return;
        }

        FarmUni13(1);
        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 4");

        Core.AddDrop("Gem of Nulgath", "Bloodstone of Nulgath", "Quartz of Nulgath", "Tanzanite of Nulgath", "Unidentified Gemstone of Nulgath");

        while (!Bot.ShouldExit && (!Core.CheckInventory("Bloodstone of Nulgath", bloodStone)
                                || !Core.CheckInventory("Quartz of Nulgath", quartz)
                                || !Core.CheckInventory("Tanzanite of Nulgath", tanzanite)
                                || !Core.CheckInventory("Unidentified Gemstone of Nulgath", uniGemStone)))
        {
            Core.EnsureAccept(gemstonesForNulgathQuest);

            if (!Core.isCompletedBefore(boneTerrorQuest))
            {
                if (!Core.isCompletedBefore(skeletalWarriorQuest1))
                {
                    Core.EnsureAccept(skeletalWarriorQuest1);
                    Core.HuntMonster("battleundera", "Skeletal Warrior", "Yara's Ring", log: false);
                    Core.EnsureComplete(skeletalWarriorQuest1);
                }

                if (!Core.isCompletedBefore(skeletalWarriorQuest2))
                {
                    Core.EnsureAccept(skeletalWarriorQuest2);
                    Core.HuntMonster("battleundera", "Skeletal Warrior", "Skeletal Claymore", 6, log: false);
                    Core.HuntMonster("battleundera", "Skeletal Warrior", "Bony Chestplate", 3, log: false);
                    Core.EnsureComplete(skeletalWarriorQuest2);
                }

                if (!Core.isCompletedBefore(boneTerrorQuest))
                {
                    Core.EnsureAccept(boneTerrorQuest);
                    Core.HuntMonster("battleundera", "Bone Terror", "Bone Terror's Head", log: false);
                    Core.EnsureComplete(boneTerrorQuest);
                }
            }

            while (!Bot.ShouldExit && !Core.CheckInventory("Yara's Sword"))
            {
                Core.AddDrop("Yara's Sword");
                Core.EnsureAccept(unidentifiedWeaponQuest);
                Core.HuntMonster("battleundera", "Skeletal Warrior", "Unidentified Weapon", log: false);
                Core.EnsureComplete(unidentifiedWeaponQuest);
            }

            Core.HuntMonster("Twilight", "Abaddon", "Balor's Cruelty", isTemp: false, log: false);
            Core.HuntMonster("ShadowfallWar", "Bonemuncher", "Ultimate Darkness Gem", isTemp: false, log: false);
            Core.EnsureComplete(gemstonesForNulgathQuest);
        }
    }




    /// <summary>
    /// [Member] Does Forge Tainted Gems for Nulgath [Quest] to get You Tainted Gems with your specific quantities
    /// </summary>
    /// <param name="quant">Desired quantity of Tainted Gems</param>
    public void ForgeTaintedGems(int quant = 1000)
    {
        const int forgeTaintedGemsQuest = 4919;

        if (!Core.CheckInventory("Gemstone of Nulgath") && !Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it.");
            return;
        }

        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 5");

        Core.AddDrop("Tainted Gem", "Unidentified Gemstone of Nulgath");

        while (!Bot.ShouldExit && !Core.CheckInventory("Tainted Gem", quant))
        {
            Core.EnsureAccept(forgeTaintedGemsQuest);
            FarmGemofNulgath(1);
            GemStonesOfnulgath(0, 1, 1, 0);
            Core.EnsureComplete(forgeTaintedGemsQuest);
        }
    }


    /// <summary>
    /// [Member] Forges Dark Crystal Shards for Nulgath [Quest] to obtain Dark Crystal Shards with specific quantities.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack</param>
    public void ForgeDarkCrystalShards(int quant = 1000)
    {
        if (!Core.CheckInventory("Gemstone of Nulgath") && !Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it");
            return;
        }

        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 5");

        Core.AddDrop("Dark Crystal Shards", "Unidentified Gemstone of Nulgath");

        while (!Bot.ShouldExit && !Core.CheckInventory("Dark Crystal Shards", quant))
        {
            // Forge Dark Crystal Shards for Nulgath [Member] 4920
            Core.EnsureAccept(4920);
            FarmGemofNulgath(1);
            GemStonesOfnulgath(0, 5, 2, 0);
            Core.EnsureComplete(4920);
        }
    }


    /// <summary>
    /// [Member] Forges Diamonds for Nulgath [Quest] to obtain Diamonds for Nulgath with specific quantities.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack.</param>
    public void ForgeDiamondsOfNulgath(int quant = 1000)
    {
        if (!Core.CheckInventory("Gemstone of Nulgath") && !Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it");
            return;
        }

        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 5");

        Core.AddDrop("Diamonds for Nulgath", "Unidentified Gemstone of Nulgath");

        while (!Bot.ShouldExit && !Core.CheckInventory("Diamonds for Nulgath", quant))
        {
            // Forge Diamonds for Nulgath [Member] 4921
            Core.EnsureAccept(4921);
            FarmGemofNulgath(1);
            GemStonesOfnulgath(0, 2, 0, 0);
            Core.EnsureComplete(4921);
        }
    }



    /// <summary>
    /// [Member] Forges Blood Gems for Nulgath [Quest] to obtain Blood Gem of the Archfiend with specific quantities.
    /// </summary>
    /// <param name="quant">Desired quantity, 100 = max stack.</param>
    public void ForgeBloodGems(int quant = 100)
    {
        if (!Core.CheckInventory("Gemstone of Nulgath") && !Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it");
            return;
        }

        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 5");

        Core.AddDrop("Blood Gem of the Archfiend", "Unidentified Gemstone of Nulgath");

        while (!Bot.ShouldExit && !Core.CheckInventory("Blood Gem of the Archfiend", quant))
        {
            // Forge Blood Gems for Nulgath [Member] 4922
            Core.EnsureAccept(4922);
            FarmGemofNulgath(7);
            GemStonesOfnulgath(3, 5, 0, 0);
            Core.EnsureComplete(4922);
        }
    }



    /// <summary>
    /// [Member] Carves a Uni Gemstone [Quest] to obtain specific items.
    /// </summary>
    /// <param name="item">Desired item name.</param>
    /// <param name="quant">Desired item quantity.</param>
    public void CarveUniGemStone(string? item = null, int quant = 1000)
    {
        string[] questDrops = { "Tainted Gem", "Dark Crystal Shard", "Diamond of Nulgath", "Gem of Nulgath", "Blood Gem of the Archfiend" };

        // Check if the player is a member and has the desired items or item.
        if (!Core.IsMember)
        {
            Core.Logger("This quest requires you to have Gemstone of Nulgath and membership to be able to accept it");
            return;
        }

        if ((item == null && Core.CheckInventory(questDrops, quant)) || (item != null && Core.CheckInventory(item, quant)))
            return;

        // Required items
        Core.KillMonster("tercessuinotlim", "m4", "Right", "Shadow of Nulgath", "Hadean Onyx of Nulgath", isTemp: false);
        GemStoneReceiptOfNulgath(1);
        Supplies("Unidentified 5");

        if (item != null)
            Core.AddDrop(item);
        else
            Core.AddDrop(questDrops);

        while (!Bot.ShouldExit && (item == null || !Core.CheckInventory(item, quant)))
        {
            // Carve the Unidentified Gemstone [Member] 4923
            Core.EnsureAccept(4923);
            Core.HuntMonster("WillowCreek", "Hidden Spy", "The Secret 1", isTemp: false);
            FarmGemofNulgath(7);
            GemStonesOfnulgath(1, 3, 1, 1);

            static int GetItemIdByName(string? itemName) => itemName switch
            {
                "Dark Crystal Shard" => 4770,
                "Diamond of Nulgath" => 4771,
                "Gem of Nulgath" => 6136,
                "Blood Gem of the Archfiend" => 22332,
                "Tainted Gem" => 4769,
                _ => -1,
            };

            int itemId = GetItemIdByName(item);
            if (itemId != -1)
            {
                Core.EnsureComplete(4923, itemId);
            }
            else
            {
                Core.EnsureCompleteChoose(4923); // Complete the quest without specifying item ID
            }

            if (item != null)
                Core.Logger(Bot.Inventory.IsMaxStack(item) ? "Max Stack Hit." : $"{item}: {Bot.Inventory.GetQuantity(item)}/{quant}");
        }
    }



    /// <summary>
    /// Farms gold through Leery Contract exchange.
    /// </summary>
    /// <param name="quant">Desired gold quantity.</param>
    public void LeeryExchangeGold(int quant = 100000000)
    {
        // Check if the player is a member or already has the desired gold quantity.
        if (!Core.IsMember || Bot.Player.Gold >= quant)
            return;

        // Add Unidentified 13 to the drops list.
        Core.AddDrop("Unidentified 13");

        // Toggle Gold Boost and register the required quest.
        Farm.ToggleBoost(BoostType.Gold);
        Core.RegisterQuests(554);

        // Continue farming until the desired gold quantity is reached.
        while (!Bot.ShouldExit && Bot.Player.Gold < quant)
        {
            // Farm Unidentified 13 for the exchange.
            FarmUni13(13);

            // Hunt the specified monster to exchange Unidentified 13 for gold.
            while (Core.CheckInventory("Unidentified 13"))
                Core.HuntMonster("underworld", "Undead Legend", "Undead Legend Rune", log: false);
        }

        // Cancel the registered quest and disable Gold Boost.
        Core.CancelRegisteredQuests();
        Farm.ToggleBoost(BoostType.Gold, false);
    }


    /// <summary>
    /// Hires Nulgath Larvae.
    /// </summary>
    public void HireNulgathLarvae()
    {
        // Check if Nulgath Larvae is already in inventory or the player is not a member.
        if (Core.CheckInventory("Nulgath Larvae") || !Core.IsMember)
            return;

        // Add Nulgath Larvae to the drops list.
        Core.AddDrop("Nulgath Larvae");

        // Accept the required quest.
        Core.EnsureAccept(867);

        // Farm the required vouchers for the quest.
        FarmVoucher(true);

        // Hunt the specified monster to complete the quest.
        Core.HuntMonster("underworld", "Undead Legend", "Undead Legend Rune", log: false);

        // Ensure the quest is completed and wait for the pet pickup.
        Core.EnsureComplete(867);
        Bot.Wait.ForPickup("Nulgath Larvae");
    }



    /// <summary>
    /// Swindles Bilk method
    /// </summary>
    /// <param name="item">Desired item name</param>
    /// <param name="quantity">Desired item quantity</param>
    public void SwindlesBilk(string item)
    {
        if (string.IsNullOrEmpty(item))
        {
            throw new ArgumentException($"'{nameof(item)}' cannot be null or empty.", nameof(item));
        }

        string[] rPDSuni = new[] { Uni1(1), Uni1(6), Uni1(9), Uni1(16), Uni1(20) };
        Core.AddDrop(rPDSuni);
        Core.AddDrop("Blood Gem of Nulgath");
    }

    private static string Uni1(int nr) => $"Unidentified {nr}";

    /// <summary>
    /// Farms Voucher of Nulgath (member or not) with the best method available
    /// </summary>
    /// <param name="member">If true will farm Voucher of Nulgath; false Voucher of Nulgath (nom-mem)</param>
    public void FarmVoucher(bool member)
    {
        if ((Core.CheckInventory("Voucher of Nulgath (non-mem)") && !member) || (Core.CheckInventory("Voucher of Nulgath") && member))
            return;

        Core.AddDrop(member ? "Voucher of Nulgath" : "Voucher of Nulgath (non-mem)");

        BambloozevsDrudgen(member ? "Voucher of Nulgath" : "Voucher of Nulgath (non-mem)");
        NewWorldsNewOpportunities(member ? "Voucher of Nulgath" : "Voucher of Nulgath (non-mem)");
        VoidKightSwordQuest(member ? "Voucher of Nulgath" : "Voucher of Nulgath (non-mem)");
        Supplies(member ? "Voucher of Nulgath" : "Voucher of Nulgath (non-mem)");
    }

}

public enum ChooseReward
{
    TaintedGem = 4769,
    DarkCrystalShard = 4770,
    DiamondofNulgath = 4771,
    GemofNulgath = 6136,
    BloodGemoftheArchfiend = 22332,
    TotemofNulgath = 5357
}

public enum SwindlesReturnReward
{
    Tainted_Gem = 4769,
    Dark_Crystal_Shard = 4770,
    Diamond_of_Nulgath = 4771,
    Gem_of_Nulgath = 6136,
    Blood_Gem_of_the_Archfiend = 22332,
    None = 0
};
