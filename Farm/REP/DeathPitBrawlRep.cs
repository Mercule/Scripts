/*
name: DeathPitBrawlRep
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/Story/ThroneofDarkness/CoreToD.cs
//cs_include Scripts/CoreAdvanced.cs

using Skua.Core.Interfaces;
public class DeathPitBrawlREP
{
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new CoreFarms();
    public CoreToD CoreToD = new();
    public CoreAdvanced Adv = new();

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        DoRep();

        Core.SetOptions(false);
    }

    public void DoRep()
    {
        CoreToD.DeathPitPVP();
        //Adv.BestGear(GenericGearBoost.dmgAll);
        //Adv.BestGear(GenericGearBoost.rep);
        Farm.DeathPitBrawlREP();
        
    }
}
