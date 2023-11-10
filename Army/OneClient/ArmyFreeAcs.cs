/*
name: Army Free 500 accs
description: the 500 free acs quest
tags: acs, free, thefamily, army.
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Army/CoreArmyLite.cs
using Skua.Core.Interfaces;

public class ArmyFreeAcs
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreArmyLite Army = new();
    private CoreFarms Farm = new();
    public CoreStory Story = new();

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.SetOptions();

        FreeAcs();

        Core.SetOptions(false);
    }

    public void FreeAcs()
    {
        Core.OneTimeMessage("Only for army", "This is intended for use with an army, not for solo players.");

        while (!Bot.ShouldExit && Army.doForAll())
        {

            if (Story.QuestProgression(9444))
            {
                Core.Logger("Quest not avaible / is already completed.");
            }
            else
            {
                Core.EnsureAccept(9444);
                Core.HuntMonster("eventhub", "Agitated Orb", "Free ACs... and Yogurt");
                Core.EnsureComplete(9444);
            }
        }
    }
}


#region Preious years (just copy and paste, then comment out)

#region 2023
// while (Army.doForAll())
//         {

//             if (!Bot.Quests.IsAvailable(9444))
//             {
//                 Core.Logger("Quest not avaible / is already completed.");
//                 return;
//             }

//             Core.EnsureAccept(9444);
//             Core.HuntMonster("eventhub", "Agitated Orb", "Free ACs... and Yogurt");
//             Core.EnsureComplete(9444);
//         }
#endregion 2023

#endregion Preious years (just copy and paste, then comment out)
