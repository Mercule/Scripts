/*
name: LoremasterREP
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
using Skua.Core.Interfaces;
public class LoremasterREP
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new CoreFarms();
    public CoreStory Story = new();
    public CoreAdvanced Adv = new();

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        //Adv.BestGear(GenericGearBoost.dmgAll);
        //Adv.BestGear(GenericGearBoost.rep);
        Farm.LoremasterREP();

        Core.SetOptions(false);
    }
}
