/*
name: Abyssal Zone
description: This script completes the storyline in /abyssalzone.
tags: age,of,ruin,saga,story,quest,zone,abyssal
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreStory.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/Story\ShadowsOfWar\CoreSoW.cs
//cs_include Scripts/Story\AgeOfRuin\CoreAOR.cs
using Skua.Core.Interfaces;

public class AbyssalZone
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreAOR AOR = new();

    public void ScriptMain(IScriptInterface Bot)
    {
        Core.SetOptions();

        AOR.AbyssalZone();
        Core.SetOptions(false);
    }
}
