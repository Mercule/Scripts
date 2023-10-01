/*
name: Shadowflame Finale Merge
description: This bot will farm the items belonging to the selected mode for the Shadowflame Finale Merge [2156] in /ruinedcrown
tags: shadowflame, finale, merge, ruinedcrown, defender, defenders, crest, horn, horned, skull, wing, spear, enchanted, warrior, mage, healer, rogue, rogues, mortal
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/ShadowsOfWar/CoreSoWMats.cs
//cs_include Scripts/Story/ShadowsOfWar/CoreSoW.cs
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class ShadowflameFinaleMerge
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new();
    public CoreStory Story = new();
    public CoreAdvanced Adv = new();
    public static CoreAdvanced sAdv = new();
    private CoreSoWMats SOWM = new();
    public CoreSoW SoW = new();

    public List<IOption> Generic = sAdv.MergeOptions;
    public string[] MultiOptions = { "Generic", "Select" };
    public string OptionsStorage = sAdv.OptionsStorage;
    // [Can Change] This should only be changed by the author.
    //              If true, it will not stop the script if the default case triggers and the user chose to only get mats
    private bool dontStopMissingIng = false;

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.BankingBlackList.AddRange(new[] { "Willpower", "ShadowFlame Warrior", "ShadowFlame Mage", "ShadowFlame Healer", "ShadowFlame Rogue", "ShadowFlame Rogue's Mask", "ShadowFlame Rogue's Mortal Locks", "ShadowFlame Rogue's Locks"});
        Core.SetOptions();

        BuyAllMerge();
        Core.SetOptions(false);
    }

    public void BuyAllMerge(string? buyOnlyThis = null, mergeOptionsEnum? buyMode = null)
    {
        SoW.RuinedCrown();
        //Only edit the map and shopID here
        Adv.StartBuyAllMerge("ruinedcrown", 2156, findIngredients, buyOnlyThis, buyMode: buyMode);

        #region Dont edit this part
        void findIngredients()
        {
            ItemBase req = Adv.externalItem;
            int quant = Adv.externalQuant;
            int currentQuant = req.Temp ? Bot.TempInv.GetQuantity(req.Name) : Bot.Inventory.GetQuantity(req.Name);
            if (req == null)
            {
                Core.Logger("req is NULL");
                return;
            }

            switch (req.Name)
            {
                default:
                    bool shouldStop = !Adv.matsOnly || !dontStopMissingIng;
                    Core.Logger($"The bot hasn't been taught how to get {req.Name}." + (shouldStop ? " Please report the issue." : " Skipping"), messageBox: shouldStop, stopBot: shouldStop);
                    break;
                #endregion

                case "Willpower":
                    SOWM.Willpower(quant);
                    break;

                case "ShadowFlame Healer":
                case "ShadowFlame Warrior":
                case "ShadowFlame Mage":
                    Core.EquipClass(ClassType.Farm);
                    Core.HuntMonster("ruinedcrown", "Mana-Burdened Mage", req.Name, isTemp: false);
                    break;

                case "ShadowFlame Rogue":
                case "ShadowFlame Rogue's Mask":
                case "ShadowFlame Rogue's Locks":
                case "ShadowFlame Rogue's Mortal Locks":
                    Core.EquipClass(ClassType.Farm);
                    Core.HuntMonster("ruinedcrown", "Mana-Burdened Minion", req.Name, isTemp: false);
                    break;

            }
        }
    }

    public List<IOption> Select = new()
    {
        new Option<bool>("70606", "ShadowFlame Defender", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender\" ?", false),
        new Option<bool>("70607", "ShadowFlame Defender's Crest", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Crest\" ?", false),
        new Option<bool>("70608", "ShadowFlame Defender's Hair", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Hair\" ?", false),
        new Option<bool>("70609", "ShadowFlame Defender's Horn", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Horn\" ?", false),
        new Option<bool>("70611", "ShadowFlame Defender's Horned Skull", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Horned Skull\" ?", false),
        new Option<bool>("70612", "ShadowFlame Defender's Wing", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Wing\" ?", false),
        new Option<bool>("70616", "ShadowFlame Defender's Spear", "Mode: [select] only\nShould the bot buy \"ShadowFlame Defender's Spear\" ?", false),
        new Option<bool>("71601", "Enchanted ShadowFlame Warrior", "Mode: [select] only\nShould the bot buy \"Enchanted ShadowFlame Warrior\" ?", false),
        new Option<bool>("71602", "Enchanted ShadowFlame Mage", "Mode: [select] only\nShould the bot buy \"Enchanted ShadowFlame Mage\" ?", false),
        new Option<bool>("71603", "Enchanted ShadowFlame Healer", "Mode: [select] only\nShould the bot buy \"Enchanted ShadowFlame Healer\" ?", false),
        new Option<bool>("71604", "Enchanted ShadowFlame Rogue", "Mode: [select] only\nShould the bot buy \"Enchanted ShadowFlame Rogue\" ?", false),
        new Option<bool>("71605", "Enchanted Rogue's Mask", "Mode: [select] only\nShould the bot buy \"Enchanted Rogue's Mask\" ?", false),
        new Option<bool>("71606", "Enchanted Rogue's Mortal Locks", "Mode: [select] only\nShould the bot buy \"Enchanted Rogue's Mortal Locks\" ?", false),
        new Option<bool>("71607", "Enchanted Rogue's Locks", "Mode: [select] only\nShould the bot buy \"Enchanted Rogue's Locks\" ?", false),
    };
}
