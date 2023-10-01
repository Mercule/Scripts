/*
name: Beetle Quests and Rewards
description: This will complete the Beetle General Pet quest.
tags: quest, beetle, general, warlord, pet, staff, birthday, nulgath
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Nation/CoreNation.cs
//cs_include Scripts/Seasonal/StaffBirthdays/Nulgath/TempleSiege.cs
//cs_include Scripts/Nation/Various/DragonBlade[mem].cs
//cs_include Scripts/Seasonal/StaffBirthdays/Nulgath/TempleSiegeMerge.cs
//cs_include Scripts/Hollowborn/CoreHollowborn.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class BeetleQuests
{
    public static IScriptInterface Bot => IScriptInterface.Instance;
    private static CoreBots Core => CoreBots.Instance;
    private static CoreAdvanced Adv => new();
    private readonly TempleSiegeMerge TSM = new();
    private readonly CoreNation Nation = new();
    private readonly CoreHollowborn HB = new();


    public string OptionsStorage = "BeetleQuests";
    public bool DontPreconfigure = true;
    public List<IOption> Options = new()
    {
        CoreBots.Instance.SkipOptions,
        new Option<bool>("GetCosmetics", "Pet + Cosmetics?", "get the Cosmetics from both pets...? Or only get the 2 pets + the Armor", false),
        new Option<bool>("BankExtras", "Bank Cosmetics?", "Bank cosmetic rewards, or fill your inventory?", false)
    };

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.BankingBlackList.AddRange(new[] { "Beetle Warlord Pet", "Beetle General Pet", "Beetle EXP" });
        Core.SetOptions();

        if (Bot.Config!.Get<bool>("GetCosmetics"))
            Everything();
        else PetAndArmor();

        Core.SetOptions(false);
    }

    void Everything()
    {
        GeneralRewards();
        WarlordRewards();
    }

    void PetAndArmor()
    {
        GetBeetleWarlord();
        WarlordRewards("Void Beetle Warlord");
    }

    public void GetBeetleWarlord()
    {
        if (Core.CheckInventory(75663))
        {
            Core.Logger("\"Beetle Warlord Pet\" owned.");
            return;
        }

        TSM.BuyAllMerge("Beetle General Pet");

        if (!Core.CheckInventory(75663))
        {
            Core.AddDrop("Red Ant Pet", "Beetle EXP", "Beetle Warlord Pet");
            Core.EnsureAccept(9077);
            while (!Bot.ShouldExit && !Core.CheckInventory("Beetle EXP", 8))
            {
                Core.EnsureAccept(9076);
                Core.HuntMonster("giant", "Red Ant", "Red Ant Pet", isTemp: false);
                Nation.EssenceofNulgath(10);
                HB.FreshSouls(1, 10);
                Core.EnsureCompleteChoose(9076);
                Core.JumpWait();
            }
            Adv.BuyItem("tercessuinotlim", 1951, "Unmoulded Fiend Essence");
            Core.EnsureComplete(9077);
            Bot.Wait.ForPickup(75663);
        }
    }

    public void WarlordRewards(string? rewardItemName = null, int quant = 1)
    {
        GetBeetleWarlord();

        Core.AddDrop("Baby Chaos Dragon", "Reaper's Soul");
        List<ItemBase> RewardOptions = Core.EnsureLoad(9078).Rewards;

        if (!string.IsNullOrEmpty(rewardItemName))
        {
            ItemBase? rewardItem = RewardOptions.Find(item => item.Name.Equals(rewardItemName, StringComparison.OrdinalIgnoreCase));
            if (rewardItem == null)
            {
                Core.Logger("The specified reward item name is not available as a reward.");
                return;
            }

            Core.AddDrop(rewardItem.Name);
            int rewardItemQuantity = Bot.Inventory.GetItem(rewardItem.ID)?.Quantity ?? 0;

            if (rewardItemQuantity >= 1)
                return;

            while (rewardItemQuantity < quant)
            {
                Core.EnsureAccept(9078);
                Nation.FarmTotemofNulgath(1);
                Core.EquipClass(ClassType.Farm);
                Core.HuntMonster("dragonchallenge", "Chaos Dragon", "Baby Chaos Dragon", isTemp: false);
                Core.EquipClass(ClassType.Solo);
                Core.HuntMonster("thevoid", "Reaper", "Reaper's Soul", isTemp: false);
                Core.EnsureComplete(9078, rewardItem.ID);
                Core.ToBank(rewardItem.ID);

                rewardItemQuantity = Bot.Inventory.GetItem(rewardItem.ID)?.Quantity ?? 0;
            }
        }
        else
        {
            foreach (ItemBase item in RewardOptions)
            {
                int itemQuantity = Bot.Inventory.GetItem(item.ID)?.Quantity ?? 0;

                while (itemQuantity < item.MaxStack)
                {
                    Core.EnsureAccept(9078);
                    Nation.FarmTotemofNulgath(1);
                    Core.EquipClass(ClassType.Farm);
                    Core.HuntMonster("dragonchallenge", "Chaos Dragon", "Baby Chaos Dragon", isTemp: false);
                    Core.EquipClass(ClassType.Solo);
                    Core.HuntMonster("thevoid", "Reaper", "Reaper's Soul", isTemp: false);
                    Core.EnsureComplete(9078, item.ID);
                    Core.ToBank(item.ID);

                    itemQuantity = Core.CheckInventory(item.ID, toInv: false) ? Bot.Inventory.GetItem(item.ID)?.Quantity ?? 0 : item.MaxStack;
                }
            }
        }
    }

    public void GeneralRewards()
    {
        TSM.BuyAllMerge("Beetle General Pet");

        List<ItemBase> RewardOptions = Core.EnsureLoad(9076).Rewards;
        foreach (ItemBase item in RewardOptions)
            Core.AddDrop(item.Name);
        foreach (ItemBase item in RewardOptions)
        {
            while (!Bot.ShouldExit && !Core.CheckInventory(item.ID, toInv: false))
            {
                Core.EnsureAccept(9076);
                Core.HuntMonster("giant", "Red Ant", "Red Ant Pet", isTemp: false);
                Nation.EssenceofNulgath(10);
                HB.FreshSouls(1, 10);
                Core.EnsureComplete(9076, item.ID);
                Core.JumpWait();
                Core.ToBank(item.ID);
            }
            if (Bot.Config!.Get<bool>("BankExtras"))
                Core.ToBank(item.ID);
        }
    }
}