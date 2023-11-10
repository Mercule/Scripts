/*
name: All accs bank all
description: banks all items on all accs in the "thefamily.txt" file.
tags: bank, army, all
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreDailies.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Army/CoreArmyLite.cs
//cs_include Scripts/Tools/BankAllItems.cs
using Skua.Core.Interfaces;

public class ArmyBankAllItems
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private BankAllItems BAI = new();
    private CoreArmyLite Army = new();
    
    public void ScriptMain(IScriptInterface Bot)
    {
        Core.SetOptions();

        AllBankAll();

        Core.SetOptions(false);
    }

    public void AllBankAll()
    {
        while (!Bot.ShouldExit && Army.doForAll())
            BAI.BankAll();

    }
}