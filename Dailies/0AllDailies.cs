/*
name: All Dailies
description: Does all the avaiable dailies.
tags: all dailies, dailies, daily, all
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Nation/CoreNation.cs
//cs_include Scripts/Story/BattleUnder.cs
//cs_include Scripts/Story/Nation/CitadelRuins.cs
//cs_include Scripts/Story/DragonFableOrigins.cs
//cs_include Scripts/Dailies/LordOfOrder.cs
//cs_include Scripts/Story/Glacera.cs
//cs_include Scripts/Good/BLoD/CoreBLOD.cs
//cs_include Scripts/Story/Friendship.cs
//cs_include Scripts/Evil/SDKA/CoreSDKA.cs
//cs_include Scripts/Tools\BankAllItems.cs
using Skua.Core.Interfaces;
using Skua.Core.Options;

public class FarmAllDailies
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreDailies Daily = new();
    private LordOfOrder LOO = new();
    private GlaceraStory Glac = new();
    private CoreBLOD BLOD = new();
    private Friendship FR = new();
    private CoreSDKA CSDKA = new();
    //private BankAllItems BAI = new();

    public bool DontPreconfigure = true;
    public string OptionsStorage = "FarmAllDailies";
    public List<IOption> Options = new()
    {
        new Option<DailySet>("Select Dailies Set", "Dailies set: Recommended or All?", "only do the few that we recommend to make it a bit quicker?", DailySet.All),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.SetOptions();

        DoAllDailies(Bot.Config!.Get<DailySet>("Select Dailies Set"));

        Core.SetOptions(false);
    }

    public void DoAllDailies(DailySet Set = DailySet.All)
    {
        if (Set == DailySet.Recommended)
        {
            Core.Logger($"Doing selected set of dailies: Recommended");
            LOO.GetLoO();
            BLOD.UnlockMineCrafting();
            Daily.Pyromancer();
            Daily.DeathKnightLord();
            Daily.ShadowScytheClass();
            Daily.WheelofDoom();
            Daily.FreeDailyBoost();
            Daily.CollectorClass();
            Glac.FrozenTower();
            Daily.Cryomancer();
            Daily.EldersBlood();
            Daily.ShadowShroud();
            Daily.DagesScrollFragment();
            Daily.MineCrafting(new[] { "Aluminum", "Barium", "Gold", "Iron", "Copper", "Silver", "Platinum" }, 10, ToBank: true);
            Daily.CryptoToken();
            Core.Logger("Recommended Dailies finished!");
        }
        else if (Set == DailySet.All)
        {
            Core.Logger($"Doing selected set of dailies: All");
            LOO.GetLoO();
            BLOD.UnlockMineCrafting();
            Daily.Pyromancer();
            Daily.DeathKnightLord();
            Daily.ShadowScytheClass();
            Daily.WheelofDoom();
            Daily.FreeDailyBoost();
            Daily.CollectorClass();
            Glac.FrozenTower();
            Daily.Cryomancer();
            Daily.EldersBlood();
            Daily.MineCrafting(new[] { "Aluminum", "Barium", "Gold", "Iron", "Copper", "Silver", "Platinum" }, 10, ToBank: true);

            Daily.SparrowsBlood();
            Daily.BeastMasterChallenge();
            Daily.FungiforaFunGuy();
            CSDKA.UnlockHardCoreMetals();
            Daily.HardCoreMetals(new[] { "Arsenic", "Beryllium", "Chromium", "Palladium", "Rhodium", "Rhodium", "Thorium", "Mercury" }, 10, ToBank: true);
            Daily.GoldenInquisitor();
            Daily.BreakIntotheHoard(false, false);
            Daily.MadWeaponSmith();
            Daily.CyserosSuperHammer();
            Daily.BrightKnightArmor();
            Daily.GrumbleGrumble();
            Daily.MonthlyTreasureChestKeys();
            Daily.BallyhooAdRewards();
            Daily.PowerGem();
            Daily.DesignNotes();
            Daily.MoglinPets();
            FR.CompleteStory();
            Daily.Friendships();
            Core.Logger("\"All\" Dailies finished!");
        }
    }

    public enum DailySet
    {
        Recommended,
        All
    }
}
