/*
name: YouMadBroBadge
description: null
tags: null
*/
//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/CoreAdvanced.cs
using Skua.Core.Interfaces;

public class YouMadBroBadge
{
    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new();
    public CoreAdvanced Adv = new();

    public void ScriptMain(IScriptInterface bot)
    {
        Core.BankingBlackList.AddRange(new[] { "Dragon Runestone", "Ice Vapor" });
        Core.SetOptions();

        Badge();

        Core.SetOptions(false);
    }

    public void Badge()
    {
        if (Core.HasWebBadge(badge))
        {
            Core.Logger($"Already have the {badge} badge");
            return;
        }

        Farm.AlchemyREP();
        Core.EquipClass(ClassType.Farm);
        while (!Bot.ShouldExit && !Core.HasWebBadge(badge))
        {
            Core.AddDrop("Ice Vapor");
            Core.AddDrop(11475); //dragon scale (2 items items have this name hence the id)
            Core.FarmingLogger("Dragon Scale", 30);
            Core.FarmingLogger("Ice Vapor", 2);
            // while (!Core.CheckInventory(11475, 30) || !Core.CheckInventory("Ice Vapor", 30)) //uncomment this, and comment the line below out, if ice vapor ever gets fixed from only needing 1 and it never being used.
            // while (!Core.CheckInventory(11475, 30) || !Core.CheckInventory("Ice Vapor", 2))
            //     Core.KillMonster("lair", "Enter", "Spawn", "*", isTemp: false, log: false);


            while (!Core.CheckInventory(11475, 30))
                Core.KillMonster("lair", "Hole", "Center", "*", isTemp: false, log: false);
            Core.KillMonster("lair", "Enter", "Spawn", "*", "Ice Vapor", 2, isTemp: false, log: false);

            //incase they fix icevapor not being used V
            // Core.KillMonster("lair", "Enter", "Spawn", "*", "Ice Vapor", 30, isTemp: false, log: false);

            if (!Core.CheckInventory("Dragon Runestone", 30))
            {
                Adv.BuyItem("alchemyacademy", 395, 62749, 30, 1, 8777);
                Core.BuyItem("alchemyacademy", 395, "Dragon Runestone", 30, 8844);
            }

            Core.Join("alchemy"); //maybe you have to be here to get the badge?

            Farm.AlchemyPacket("Dragon Scale", "Ice Vapor", trait: CoreFarms.AlchemyTraits.hOu, P2w: true);
        }
        Core.TrashCan("Dragon Scale", "Ice Vapor");
        Core.ToBank("Dragon Runestone", "Gold Voucher 100k");
    }
    // private string[] PotionsToSell = {"Life Potion", "Basic Crusader Elixir", "Basic Barrier Potion", "Basic Crusader Elixir", "Divine Elixir", "Barrier Potion", "Basic Barrier Potion", "Basic Divine Elixir", "Crusader Elixir"};
    private string badge = "You mad bro?";
}
