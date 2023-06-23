﻿/*
name: null
description: null
tags: null
*/
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Skua.Core.Interfaces;
using Skua.Core.Models;
using Skua.Core.Models.Items;
using Skua.Core.Models.Monsters;
using Skua.Core.Models.Quests;
using Skua.Core.Models.Servers;
using Skua.Core.Models.Shops;
using Skua.Core.Models.Skills;
using Skua.Core.Options;
using Skua.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class CoreBots
{
    #region Declerations
    // [Can Change] Delay between common actions, 700 is the safe number
    public int ActionDelay { get; set; } = 700;
    // [Can Change] Delay used to get out of combat, 1600 is the safe number
    public int ExitCombatDelay { get; set; } = 1600;
    // [Can Change] Delay between jumping rooms after hunting a monster, increase if you think it is jumping too much
    public int HuntDelay { get; set; } = 1000;
    // [Can Change] How many tries to accept/complete the quest will be sent
    public int AcceptandCompleteTries { get; set; } = 20;
    // [Can Change] How many quests the bot should be able to have loaded at once
    public int LoadedQuestLimit { get; set; } = 150;
    // [Can Change] Whether the bots should also log in AQW's chat
    public bool LoggerInChat { get; set; } = true;
    // [Can Change] When enabled, no message boxes will be shown unless absolutely necessary
    public bool ForceOffMessageboxes { get; set; } = false;
    // [Can Change] Whether the bots will use private rooms
    public bool PrivateRooms { get; set; } = true;
    // [Can Change] What private room number the bot should use, if > 99999 it will pick a random room
    public int PrivateRoomNumber { get; set; } = 100000;
    // [Can Change] Use public rooms if the enemy is tough
    public bool PublicDifficult { get; set; } = false;
    // [Can Change] If StopLocations.Custom is selected, where to go
    public string CustomStopLocation { get; set; } = "whitemap";
    // [Can Change] Whether the player should rest after killing a monster
    public bool ShouldRest { get; set; } = false;
    // [Can Change] Whether the bot should attempt to clean your inventory by banking Misc. AC Items before starting the bot
    public bool BankMiscAC { get; set; } = false;
    public bool BankUnenhancedACGear { get; set; } = false;
    // [Can Change] Whether you want anti lag features (lag killer, invisible monsters, set to 10 FPS)
    public bool AntiLag { get; set; } = true;
    // [Can Change] Name of your soloing class
    public string SoloClass { get; set; } = "Generic";
    // [Can Change] Mode of soloing class, if it has multiple. 
    public ClassUseMode SoloUseMode { get; set; } = ClassUseMode.Base;
    // [Can Change] Whether you wish to equip solo equipment
    public bool SoloGearOn { get; set; } = true;
    // [Can Change] Names of your soloing equipment
    public string[] SoloGear { get; set; } = Array.Empty<string>();
    // [Can Change] Name of your farming class
    public string FarmClass { get; set; } = "Generic";
    // [Can Change] Mode of farming class, if it has multiple. 
    public ClassUseMode FarmUseMode { get; set; } = ClassUseMode.Base;
    // [Can Change] Whether you wish to equip farm equipment
    public bool FarmGearOn { get; set; } = true;
    // [Can Change] Names of your farming equipment
    public string[] FarmGear { get; set; } = Array.Empty<string>();
    // [Can Change] Some Sagas use the hero alignment to give extra reputation, change to your desired rep (Alignment.Evil or Alignment.Good).
    public int HeroAlignment { get; set; } = (int)Alignment.Evil;

    private static CoreBots? _instance;
    public static CoreBots Instance => _instance ??= new CoreBots();
    private IScriptInterface Bot => IScriptInterface.Instance;

    private const string DiscordLink = "https://discord.gg/CKKbk2zr3p";

    #endregion

    #region Start/Stop

    /// <summary>
    /// Set common bot options to desired value
    /// </summary>
    /// <param name="changeTo">Value the options will be changed to</param>
    public void SetOptions(bool changeTo = true, bool disableClassSwap = false)
    {
        // These things need to be set and checked before anything else
        if (changeTo)
        {
            Bot.Events.ScriptStopping += CrashDetector;
            SkuaVersionChecker("1.2.2.1");

            loadedBot = Bot.Manager.LoadedScript.Replace("\\", "/").Split("/Scripts/").Last().Replace(".cs", "");
            Logger($"Bot Started [{loadedBot}]");
            if (Bot.Config != null && Bot.Config.Options.Contains(SkipOptions) && !Bot.Config.Get<bool>(SkipOptions))
                Bot.Config.Configure();

            if (!Bot.Player.LoggedIn)
            {
                if (Bot.Servers.CachedServers.Any())
                {
                    Logger("Auto Login triggered");
                    try
                    {
                        if (!Bot.Servers.EnsureRelogin(Bot.Options.ReloginServer ?? Bot.Servers.CachedServers[0]?.Name ?? "Twilly"))
                            Logger("Please log-in before starting the bot.\nIf you are already logged in but are receiving this message regardless, please re-install CleanFlash", messageBox: true, stopBot: true);
                        Bot.Sleep(5000);
                    }
                    catch
                    {
                        Logger("Please log-in before starting the bot.\nIf you are already logged in but are receiving this message regardless, please re-install CleanFlash", messageBox: true, stopBot: true);
                    }
                }
                else Logger("Please log-in before starting the bot.\nIf you are already logged in but are receiving this message regardless, please re-install CleanFlash", messageBox: true, stopBot: true);
            }

            ReadCBO();
        }

        // Common Options
        Bot.Options.PrivateRooms = false;
        Bot.Options.AttackWithoutTarget = false;
        Bot.Options.SafeTimings = changeTo;
        Bot.Options.RestPackets = changeTo && ShouldRest;
        Bot.Options.AutoRelogin = true;
        Bot.Options.InfiniteRange = changeTo;
        Bot.Options.SkipCutscenes = changeTo;
        Bot.Options.QuestAcceptAndCompleteTries = AcceptandCompleteTries;
        Bot.Drops.RejectElse = changeTo;
        Bot.Lite.UntargetDead = changeTo;
        Bot.Lite.UntargetSelf = changeTo;
        Bot.Lite.ReacceptQuest = false;
        Bot.Lite.DisableRedWarning = true;
        Bot.Lite.CharacterSelectScreen = false;

        CollectData(changeTo);

        // These things need to be taken care of too, but less priority
        if (changeTo)
        {
            SetOptionsAsync();

            Bot.Options.HuntDelay = HuntDelay;

            Bot.Bank.Open();
            Bot.Bank.Loaded = true;
            if (BankMiscAC)
                BankACMisc();
            if (BankUnenhancedACGear)
                BankACUnenhancedGear();

            EquipmentBeforeBot.AddRange(Bot.Inventory.Items.Where(i => i.Equipped).Select(x => x.Name));
            currentClass = ClassType.None;
            usingSoloGeneric = SoloClass.ToLower() == "generic";
            usingFarmGeneric = FarmClass.ToLower() == "generic";
            EquipClass(disableClassSwap ? ClassType.None : ClassType.Solo);

            Bot.Events.ScriptStopping += StopBotEvent;

            // Alive Check handling
            Bot.Events.MapChanged += CleanKilledMonstersList;
            Bot.Events.MonsterKilled += KilledMonsterListener;
            Bot.Events.ExtensionPacketReceived += RespawnListener;

            Bot.Drops.Start();

            Logger("Bot Configured");

            // Bunch of things that are done in the background and you dont need the bot to wait for 
            void SetOptionsAsync()
            {
                Task.Run(() =>
                {
                    Task.Run(() =>
                    {
                        if (OneTimeMessage("discordV11",
                                "Our discord server was recently deleted again (March 29th 2023), click yes if you wish to (re-)join the server",
                                true, true, true))
                            Process.Start("explorer", DiscordLink);
                    });

                    // Butler directory cleaning
                    if (Directory.Exists(ButlerLogDir))
                    {
                        if (File.Exists(ButlerLogPath()))
                            File.Delete(ButlerLogPath());

                        string[] files = Directory.GetFiles(ButlerLogDir);
                        if (files.Any(x => x.Contains("~!") && x.Split("~!").First() == Username().ToLower()))
                            File.Delete(files.First(x => x.Contains("~!") && x.Split("~!").First() == Username().ToLower()));
                    }

                    // Making sure its set and wont change
                    IsMember = Bot.Player.IsMember;

                    // AFK Handler
                    Bot.Send.Packet("%xt%zm%afk%1%false%");
                    Bot.Sleep(ActionDelay);
                    bool TimerRunning = false;
                    //int afkCount = 0;
                    //Bot.Events.PlayerAFK += eventAFK;

                    //void eventAFK()
                    //{
                    //    afkCount++;
                    //    int localCount = afkCount;
                    //    Bot.Sleep(300000);
                    //    if (Bot.Player.AFK && afkCount == localCount)
                    //    {
                    //        Bot.Options.AutoRelogin = true;
                    //        Bot.Servers.Logout();
                    //    }
                    //}
                    Bot.Handlers.RegisterHandler(5000, b =>
                    {
                        if (b.Player.AFK && !TimerRunning)
                        {
                            TimerRunning = true;
                            Bot.Sleep(300000);
                            if (b.Player.AFK)
                            {
                                b.Options.AutoRelogin = true;
                                b.Servers.Logout();
                            }
                            TimerRunning = false;
                        }
                    }, "AFK Handler");

                    // Settin Loaded Quest Limiter
                    Bot.Handlers.RegisterHandler(3000, b =>
                    {
                        if (Bot.Quests.Tree.Count() > LoadedQuestLimit)
                        {
                            Bot.Flash.SetGameObject("world.questTree", new ExpandoObject());
                        }
                    }, "Quest-Limit Handler");

                    // Prison Detector
                    if (loadedBot.Replace("\\", "/") != "Tools/Butler")
                    {
                        Bot.Events.MapChanged += PrisonDetector;
                        void PrisonDetector(string map)
                        {
                            if (map.ToLower() == "prison" && !joinedPrison && !prisonListernerActive)
                            {
                                prisonListernerActive = true;
                                Bot.Options.AutoRelogin = false;
                                Bot.Servers.Logout();
                                string message = "You were teleported to /prison by someone other than the bot. We disconnected you and stopped the bot out of precaution.\n" +
                                                 "Be ware that you might have received a ban, as this is a method moderators use to see if you're botting." +
                                                 (!PrivateRooms || PrivateRoomNumber < 1000 || PublicDifficult ? "\nGuess you should have stayed out of public rooms!" : String.Empty);
                                Logger(message);
                                Bot.ShowMessageBox(message, "Unauthorized joining of /prison detected!", "Oh fuck!");
                                Bot.Stop(true);
                            }
                        }
                    }


                    // Anti-lag option
                    if (AntiLag)
                    {
                        Bot.Options.LagKiller = true;
                        Bot.Flash.SetGameObject("stage.frameRate", 10);
                        if (!Bot.Flash.GetGameObject<bool>("ui.monsterIcon.redX.visible"))
                            Bot.Flash.CallGameFunction("world.toggleMonsters");
                    }

                    // Identity Protection
                    Bot.Options.CustomName = "SKUA BOT";
                    Bot.Options.CustomGuild = "HTTPS://AUQW.TK/";

                    // Holiday Handlers
                    AprilFools();

                    //Fucking with specific people
                    UserSpecificMessages();
                });
            }
        }
    }

    public List<string> BankingBlackList = new();
    private List<string> EquipmentBeforeBot = new();
    private bool joinedPrison = false;
    private bool prisonListernerActive = false;
    public string loadedBot = String.Empty;

    /// <summary>
    /// Stops the bot and moves you back to /Battleon
    /// </summary>
    private bool StopBot(bool crashed)
    {
        CancelRegisteredQuests();
        AbandonQuest(Bot.Quests.Active.Select(x => x.ID).ToArray());
        StopBotAsync();
        Bot.Handlers.Clear();

        if (Bot.Player.LoggedIn)
        {
            JumpWait();
            Bot.Sleep(ActionDelay);

            if (!string.IsNullOrWhiteSpace(CustomStopLocation))
            {
                string _stopLoc = CustomStopLocation.Trim().ToLower();
                if (new[] { "home", "house" }.Contains(_stopLoc))
                {
                    if (Bot.House.Items.Any(h => h.Equipped))
                    {
                        string? toSend = null;
                        Bot.Events.ExtensionPacketReceived += modifyPacket;
                        Bot.Send.Packet($"%xt%zm%house%1%{Username()}%");
                        Task.Run(() =>
                        {
                            Bot.Wait.ForMapLoad("house");
                            if (Bot.Wait.ForTrue(() => toSend != null, 20))
                                Bot.Send.ClientPacket(toSend!, "json");
                            Bot.Events.ExtensionPacketReceived -= modifyPacket;
                            for (int i = 0; i < 7; i++)
                                Bot.Send.ClientServer(" ", "");
                        });

                        void modifyPacket(dynamic packet)
                        {
                            string type = packet["params"].type;
                            dynamic data = packet["params"].dataObj;
                            if ((type is not null and "json") && (data.houseData is not null))
                            {
                                toSend = $"{{\"t\":\"xt\",\"b\":{{\"r\":-1,\"o\":{{\"cmd\":\"moveToArea\",\"areaName\":\"house\",\"uoBranch\":{JsonConvert.SerializeObject(data.uoBranch)},\"strMapFileName\":\"{data.strMapFileName}\",\"intType\":\"1\",\"monBranch\":[],\"houseData\":{Regex.Replace(JsonConvert.SerializeObject(data.houseData), Username(), "Skua user", RegexOptions.IgnoreCase)},\"sExtra\":\"\",\"areaId\":{data.areaId},\"strMapName\":\"house\"}}}}}}";
                                Bot.Events.ExtensionPacketReceived -= modifyPacket;
                            }
                        }
                    }
                    else Bot.Send.Packet($"%xt%zm%cmd%1%tfer%{Username()}%whitemap-{PrivateRoomNumber}%");
                }
                else if (new[] { "off", "disabled", "disable", "stop", "same", "currentmap", "bot.map.currentmap", String.Empty }
                    .Any(m => m == _stopLoc))
                {
                    // Nothing happens
                }
                else Bot.Send.Packet($"%xt%zm%cmd%1%tfer%{Username()}%{_stopLoc}-{PrivateRoomNumber}%");

                if (EquipmentBeforeBot.Any())
                    Equip(EquipmentBeforeBot.ToArray());
            }
        }

        if (crashed)
            Logger("Bot stopped due to a crash.");
        else if (!Bot.Player.LoggedIn)
        {
            if (Bot.Options.AutoRelogin)
            {
                Task.Run(async () =>
                {
                    //DL_Enable();
                    DebugLogger(this);
                    await Bot.Manager.RestartScriptAsync();
                    if (Bot.Player.LoggedIn)
                        return;
                    Logger("Bot stopped due to Auto-Relogin failure.");
                });
            }
            else Logger("Bot stopped due to player logout.");
        }
        else Logger("Bot stopped successfully.");

        GC.KeepAlive(Instance);
        return scriptFinished;

        void StopBotAsync()
        {
            Task.Run(() =>
            {
                SavedState(false);

                if (AntiLag)
                {
                    Bot.Options.SetFPS = 60;
                    if (Bot.Flash.GetGameObject<bool>("ui.monsterIcon.redX.visible"))
                        Bot.Flash.CallGameFunction("world.toggleMonsters");
                }

                Bot.Options.CustomName = Username().ToUpper();
                string? guild = Bot.Flash.GetGameObject<string>("world.myAvatar.objData.guild.Name");
                Bot.Options.CustomGuild = guild != null ? $"< {guild} >" : String.Empty;

                if (File.Exists(ButlerLogPath()))
                    File.Delete(ButlerLogPath());
            });
        }
    }
    private bool scriptFinished = true;

    private bool StopBotEvent(Exception? e)
    {
        SetOptions(false);
        return StopBot(e != null);
    }

    private bool CrashDetector(Exception? e)
    {
        if (e == null || e is OperationCanceledException)
            return scriptFinished;

        string eSlice = e.Message + "\n" + e.InnerException;
        List<string> logs = Ioc.Default.GetRequiredService<ILogService>().GetLogs(LogType.Script);
        logs = logs.Skip(logs.Count() > 5 ? (logs.Count() - 5) : logs.Count()).ToList();
        if (Bot.ShowMessageBox("A crash has been detected, please fill in the report form (prefilled):\n\n" + eSlice,
                               "Script Crashed", "Open Form", "Close Window").Text == "Open Form")
        {
            string url = "\"https://docs.google.com/forms/d/e/1FAIpQLSeI_S99Q7BSKoUCY2O6o04KXF1Yh2uZtLp0ykVKsFD1bwAXUg/viewform?usp=pp_url&" +
                "entry.2118425091=Bug+Report&" +
               $"entry.290078150={Bot.Manager.LoadedScript.Split("Scripts").Last().Replace('/', '\\').Substring(1).Replace(".cs", "")}&" +
                "entry.1803231651=It+stopped+at+the+wrong+time+(crash)&" +
               $"entry.1954840906={logs.Join("%0A")}&" +
               $"entry.285894207={eSlice}&\"";
            url = url.Replace("\r\n", "%0A").Replace("\n", "").Replace(" ", "%20");

            Process p = new();
            p.StartInfo.FileName = "rundll32";
            p.StartInfo.Arguments = "url,OpenURL " + url;
            p.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System).Split('\\').First() + "\\";
            p.Start();

            Logger("Thank you for reporting the crash. Below you will find the information you will need to report, in case it isn't being auto filled");

        }
        else Logger("A crash has occurred. Please report it in the form with the details below");

        Bot.Log("--------------------------------------");
        Logger("Last 5 Logs:");
        Bot.Log(logs.Join('\n'));
        Bot.Log("--------------------------------------");
        Logger("Crash (Debug)");
        Bot.Log(eSlice);
        Bot.Log("--------------------------------------");

        return false;
    }

    public List<string> GetLogs(LogType type = LogType.Script)
        => (_logService ??= Ioc.Default.GetRequiredService<ILogService>()).GetLogs(LogType.Script);
    private ILogService? _logService;

    public void ScriptMain(IScriptInterface Bot)
    {
        RunCore();
    }

    #endregion

    #region Inventory, Bank and Shop
#nullable enable

    /// <summary>
    /// Check the Bank, Inventory and Temp Inventory for the item
    /// </summary>
    /// <param name="item">Name of the item</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="toInv">Whether or not send the item to Inventory</param>
    /// <returns>Returns whether the item exists in the desired quantity in the bank and inventory</returns>
    public bool CheckInventory(string? item, int quant = 1, bool toInv = true)
    {
        if (item == null)
            return true;

        if (Bot.TempInv.Contains(item, quant))
            return true;

        if (Bot.Inventory.Contains(item, quant))
            return true;

        if (Bot.Bank.Contains(item))
        {
            if (toInv)
                Unbank(item);

            if ((toInv && Bot.Inventory.GetQuantity(item) >= quant) ||
               (!toInv && Bot.Bank.TryGetItem(item, out InventoryItem? _item) && _item != null && _item.Quantity >= quant))
                return true;
        }

        if (Bot.House.Contains(item))
            return true;

        return false;
    }

    /// <summary>
    /// Checks the Bank and Inventory for the item with it's ID
    /// </summary>
    /// <param name="itemID">ID of the item to be checked</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="toInv">Whether or not send the item to Inventory</param>
    /// <returns>Returns whether the item exists in the desired quantity in the Bank and Inventory</returns>
    public bool CheckInventory(int? itemID, int quant = 1, bool toInv = true)
    {
        if (itemID == null)
            return true;
        int _itemID = (int)itemID;

        if (Bot.TempInv.Contains(_itemID, quant))
            return true;

        if (Bot.Inventory.Contains(_itemID, quant))
            return true;

        if (Bot.Bank.Contains(_itemID))
        {
            if (toInv)
                Unbank(_itemID);

            if ((toInv && Bot.Inventory.GetQuantity(_itemID) >= quant) ||
               (!toInv && Bot.Bank.TryGetItem(_itemID, out InventoryItem? _item) && _item != null && _item.Quantity >= quant))
                return true;
        }

        if (Bot.House.Contains(_itemID))
            return true;

        return false;
    }

    /// <summary>
    /// Check if the Bank/Inventory has at least 1 of all listed items
    /// </summary>
    /// <param name="itemNames">Array of names of the items to be checked</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="any">If any of the items exist, returns true</param>
    /// <param name="toInv">Whether or not send the item to Inventory</param>
    /// <returns>Returns whether all the items exist in the Bank or Inventory</returns>
    public bool CheckInventory(string[]? itemNames, int quant = 1, bool any = false, bool toInv = true)
    {
        if (itemNames == null || !itemNames.Any())
            return true;

        foreach (string name in itemNames)
        {
            if (CheckInventory(name, quant, toInv))
            {
                if (any)
                    return true;
                else
                    continue;
            }

            if (!any)
                return false;
        }

        return !any;
    }

    /// <summary>
    /// Checks the Bank and Inventory for the item with it's ID
    /// </summary>
    /// <param name="itemIDs">Array of IDs of the items to be checked</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="any">If any of the items exist, returns true</param>
    /// <param name="toInv">Whether or not send the item to Inventory</param>
    /// <returns>Returns whether the item exists in the desired quantity in the Bank and Inventory</returns>
    public bool CheckInventory(int[]? itemIDs, int quant = 1, bool any = false, bool toInv = true)
    {
        if (itemIDs == null || !itemIDs.Any())
            return true;

        foreach (int id in itemIDs)
        {
            if (CheckInventory(id, quant, toInv))
            {
                if (any)
                    return true;
                else
                    continue;
            }

            if (!any)
                return false;
        }

        return !any;
    }

    public void CheckSpaces(ref int counter, params string[] items)
    {
        int count = 0;
        foreach (string s in items)
        {
            if (CheckInventory(s, toInv: false))
                count++;
        }
        if (Bot.Inventory.FreeSlots < (items.Count() - count))
            Logger($"Not enough free slots, please clear {(items.Count() - count)} slot" + ((items.Count() - count) > 1 ? "s" : ""), messageBox: true, stopBot: true);
    }

    /// <summary>
    /// Move items from bank to inventory
    /// </summary>
    /// <param name="items">Items to move</param>
    public void Unbank(params string[] items)
    {
        if (items == null || items.Length == 0)
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();

        foreach (string item in items)
        {
            if (Bot.Bank.Contains(item))
            {
                Bot.Sleep(ActionDelay);
                if (Bot.Inventory.FreeSlots == 0 && Bot.Inventory.Slots != 0 && Bot.Inventory.UsedSlots <= Bot.Inventory.Slots)
                    Logger($"Your inventory is full ({Bot.Inventory.UsedSlots}/{Bot.Inventory.Slots}), please clean it and restart the bot", messageBox: true, stopBot: true);

                if (!Bot.Bank.EnsureToInventory(item))
                {
                    Logger($"Failed to unbank {item}, skipping it", messageBox: true);
                    continue;
                }
                Logger($"{item} moved from bank");
            }
        }
    }

    /// <summary>
    /// Move items from bank to inventory
    /// </summary>
    /// <param name="items">Items to move</param>
    public void Unbank(params int[] items)
    {
        if (items == null)
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();

        foreach (int item in items)
        {
            if (Bot.Bank.Contains(item))
            {
                Bot.Sleep(ActionDelay);
                if (Bot.Inventory.FreeSlots == 0 && Bot.Inventory.Slots != 0 && Bot.Inventory.UsedSlots <= Bot.Inventory.Slots)
                    Logger($"Your inventory is full ({Bot.Inventory.UsedSlots}/{Bot.Inventory.Slots}), please clean it and restart the bot", messageBox: true, stopBot: true);

                if (!Bot.Bank.EnsureToInventory(item))
                {
                    Logger($"Failed to unbank {Bot.Bank.GetItem(item)?.Name ?? item.ToString()}, skipping it", messageBox: true);
                    continue;
                }
                Logger($"{Bot.Inventory.GetItem(item)?.Name ?? item.ToString()} moved from bank");
            }
        }
    }

    /// <summary>
    /// Move items from inventory to bank
    /// </summary>
    /// <param name="items">Items to move</param>
    public void ToBank(params string?[]? items)
    {
        if (items == null || !items.Any(x => x != null))
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();
        foreach (string? item in items)
        {
            if (item == null || item == SoloClass || item == FarmClass)
                continue;
            if (Bot.Inventory.IsEquipped(item))
            {
                Logger("Can't bank an equipped item");
                continue;
            }
            if (Bot.Inventory.Contains(item))
            {
                if (!Bot.Inventory.EnsureToBank(item))
                {
                    Logger($"Failed to bank {item}, skipping it");
                    continue;
                }
                Logger($"{item} moved to bank");
            }
        }
    }

    public void ToHouseBank(params string?[]? items)
    {
        if (items == null || !items.Any(x => x != null))
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();

        foreach (string? item in items)
        {
            if (item == null || item == SoloClass || item == FarmClass)
                continue;
            if (Bot.House.IsEquipped(item))
            {
                Logger("Can't bank an equipped item");
                continue;
            }

            if (Bot.House.Contains(item))
            {
                if (!Bot.House.EnsureToBank(item))
                {
                    Logger($"Failed to bank {item}, skipping it");
                    continue;
                }
                Logger($"{item} moved to house bank");
            }
        }
    }

    public void ToHouseBank(params int[]? items)
    {
        if (items == null || !items.Any())
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();

        foreach (int item in items)
        {
            if (item == 0)
                continue;
            if (Bot.House.IsEquipped(item))
            {
                Logger("Can't bank an equipped item");
                continue;
            }

            if (Bot.House.Contains(item))
            {
                if (!Bot.House.EnsureToBank(item))
                {
                    Logger($"Failed to bank {item}, skipping it");
                    continue;
                }
                Logger($"{item} moved to house bank");
            }
        }
    }

    /// <summary>
    /// Move items from inventory to bank
    /// </summary>
    /// <param name="items">Items to move</param>
    public void ToBank(params int[]? items)
    {
        if (items == null || !items.Any())
            return;

        JumpWait();

        if (Bot.Flash.GetGameObject("ui.mcPopup.currentLabel") != "\"Bank\"")
            Bot.Bank.Open();

        foreach (int item in items)
        {
            if (item == 0)
                continue;
            if (Bot.Inventory.IsEquipped(item))
            {
                Logger("Can't bank an equipped item");
                continue;
            }
            if (Bot.Inventory.Contains(item))
            {
                string name = Bot.Inventory.GetItem(item)?.Name ?? $"[{item}]";
                if (!Bot.Inventory.EnsureToBank(item))
                {
                    Logger($"Failed to bank {name}, skipping it");
                    continue;
                }
                Logger($"{name} moved to bank");
            }
        }
    }

    /// <summary>
    /// Buys a item till you have the desired quantity
    /// </summary>
    /// <param name="map">Map of the shop</param>
    /// <param name="shopID">ID of the shop</param>
    /// <param name="itemName">Name of the item</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="shopQuant">How many items you get for 1 buy</param>
    /// <param name="shopItemID">Use this for Merge shops that has 2 or more of the item with the same name and you need the second/third/etc., be aware that it will re-log you after to prevent ghost buy. To get the ShopItemID use the built in loader of Skua</param>
    public void BuyItem(string map, int shopID, string itemName, int quant = 1, int shopItemID = 0)
    {
        if (CheckInventory(itemName, quant))
            return;
        _CheckInventorySpace();

        ShopItem? item = parseShopItem(GetShopItems(map, shopID).Where(x => shopItemID == 0 ? x.Name.ToLower() == itemName.ToLower() : x.ShopItemID == shopItemID).ToList(), shopID, itemName, shopItemID);
        _BuyItem(map, shopID, item, quant);
    }

    /// <summary>
    /// Buys a item till it have the desired quantity
    /// </summary>
    /// <param name="map">Map of the shop</param>
    /// <param name="shopID">ID of the shop</param>
    /// <param name="itemID">ID of the item</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="shopQuant">How many items you get for 1 buy</param>
    /// <param name="shopItemID">Use this for Merge shops that has 2 or more of the item with the same name and you need the second/third/etc., be aware that it will relog you after to prevent ghost buy. To get the ShopItemID use the built in loader of Skua</param>
    public void BuyItem(string map, int shopID, int itemID, int quant = 1, int shopItemID = 0)
    {
        if (CheckInventory(itemID, quant))
            return;
        _CheckInventorySpace();

        ShopItem? item = parseShopItem(GetShopItems(map, shopID).Where(x => shopItemID == 0 ? x.ID == itemID : x.ShopItemID == shopItemID).ToList(), shopID, itemID.ToString(), shopItemID);
        _BuyItem(map, shopID, item, quant);
    }

    public void _BuyItem(string map, int shopID, ShopItem? item, int quant)
    {
        int buy_quant;

        if (item == null || (buy_quant = _CalcBuyQuantity(item, quant)) == 0 || !_canBuy(shopID, item, buy_quant))
            return;

        Join(map);
        Bot.Wait.ForMapLoad(map);
        JumpWait();
        Bot.Events.ExtensionPacketReceived += RelogRequieredListener;

        dynamic sItem = new ExpandoObject();
        dynamic objData = getData(item.ID, item.ShopItemID);
        sItem = objData;
        sItem.iSel = objData;
        sItem.iQty = buy_quant;
        sItem.iSel.iQty = buy_quant;
        sItem.accept = 1;

        Bot.Sleep(ActionDelay);

        if (Bot.Options.SafeTimings)
            Bot.Wait.ForActionCooldown(GameActions.BuyItem);
        Bot.Flash.CallGameFunction("world.sendBuyItemRequestWithQuantity", JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(sItem))!);
        if (Bot.Options.SafeTimings)
            Bot.Wait.ForItemBuy();
        Bot.Sleep(ActionDelay);

        Bot.Events.ExtensionPacketReceived -= RelogRequieredListener;

        if (buy_quant > quant && (CheckInventory(item.Name, buy_quant)))
        {
            // Sell spares
            // This only occurs when you buy sth with stack limits, but want less then the stack limit.
            int sell_quant = buy_quant - quant;
            SellItem(item.Name, quant);
            Logger($"Bought {buy_quant} {item.Name}, sold {sell_quant}, now at {quant} {item.Name}", "BuyItem");
        }
        else if (CheckInventory(item.Name, quant))
        {
            Logger($"Bought {buy_quant} {item.Name}, now at {quant} {item.Name}", "BuyItem");
        }
        else
            Logger($"Failed at buying {buy_quant}/{quant} {item.Name}", "BuyItem");

        void RelogRequieredListener(dynamic packet)
        {
            string type = packet["params"].type;
            dynamic data = packet["params"].dataObj;
            if (type == "json")
            {
                string str = data.strMessage;
                switch (str)
                {
                    case "Item is not buyable. Item Inventory full. Re-login to syncronize your real bag slot amount.":
                        Logger("Inventory de-sync (AE Issue) detected, reloggin so the bot can continue");
                        Relogin();
                        break;
                }
            }
        }

        dynamic getData(int itemID, int shopItemID = 0)
        {
            dynamic[]? shopItems = Bot.Flash.GetGameObject<dynamic[]>("world.shopinfo.items");
            if (shopItems != null)
            {
                foreach (dynamic i in shopItems)
                {
                    if (i == null || i!.ItemID == null || i!.ItemID != itemID ||
                       (shopItemID != 0 ? (i!.ShopItemID == null || i!.ShopItemID != shopItemID) : false))
                        continue;
                    return i!;
                }
            }
            Logger($"Failed to find the shopItemData for itemID {itemID} in {shopID}" + reinstallCleanFlash, "BuyItem");
            return null!;
        }

        bool _canBuy(int shopID, ShopItem? item, int buy_quant)
        {
            if (item == null)
                return false;

            //Achievement Check
            int achievementID = Bot.Flash.GetGameObject<int>("world.shopinfo.iIndex");
            string? io = Bot.Flash.GetGameObject<string>("world.shopinfo.sField");
            if (achievementID > 0 && io != null && !HasAchievement(achievementID, io))
            {
                Logger($"Cannot buy {item.Name} from {shopID} because you dont have achievement {achievementID} of category {io}.", "CanBuy");
                return false;
            }

            //Member Check
            if (item.Upgrade && !IsMember)
            {
                Logger($"Cannot buy {item.Name} from {shopID} because you aren't a member.", "CanBuy");
                return false;
            }

            //Required-Item Check
            int reqItemID = Bot.Flash.GetGameObject<int>("world.shopinfo.reqItems");
            if (reqItemID > 0 && !CheckInventory(reqItemID))
            {
                Logger($"Cannot buy {item.Name} from {shopID} because you dont have the requiered item needed to buy stuff from the shop, itemID: {reqItemID}", "CanBuy");
                return false;
            }

            //Quest Check
            string? questName = Bot.Flash.GetGameObject<List<dynamic>>("world.shopinfo.items")?.Find(d => d.ItemID == item.ID)?.sQuest;
            if (!String.IsNullOrEmpty(questName))
            {
                var v = JsonConvert.DeserializeObject<List<QuestData>?>(File.ReadAllText(ClientFileSources.SkuaQuestsFile));
                if (v != null)
                {
                    List<int> ids = v.Where(x => x.Name == questName).Select(q => q.ID).ToList();
                    if (ids.Any())
                    {
                        List<Quest> quests = EnsureLoad(ids.Where(q => !isCompletedBefore(q)).ToArray());
                        if (quests.Any())
                        {
                            string s = String.Empty;
                            quests.ForEach(q => s += $"[{q.ID}] |");
                            bool one = quests.Count == 1;
                            Logger($"Cannot buy {item.Name} from {shopID} because you havn't completed the {(one ? "" : "one of ")}following quest{(one ? "" : "s")}: \"{questName}\" {s[..^2]}", "CanBuy");
                            return false;
                        }
                    }
                }
            }

            //Rep check
            if (!String.IsNullOrEmpty(item.Faction) && item.Faction != "None")
            {
                int reqRank = PointsToLevel(item.RequiredReputation);
                if (reqRank > Bot.Reputation.GetRank(item.Faction))
                {
                    Logger($"Cannot buy {item.Name} from {shopID} because you dont have rank {reqRank} {item.Faction}.", "CanBuy");
                    return false;
                }
            }

            //Merge item check
            int itemCount = item.Quantity == 0 ? 1 : item.Quantity;
            int buy_count = (int)Math.Ceiling((decimal)buy_quant / (decimal)(itemCount));
            if (item.Requirements.Any())
            {
                foreach (ItemBase req in item.Requirements)
                {
                    if (CheckInventory(req.ID, req.Quantity))
                        continue;

                    Bot.Drops.Pickup(req.ID);
                    Bot.Wait.ForPickup(req.ID);

                    int total_quant = buy_count * req.Quantity;

                    if (GetShopItems(map, shopID).Any(x => req.ID == x.ID))
                        BuyItem(map, shopID, req.ID, total_quant);

                    if (!CheckInventory(req.ID, total_quant))
                    {
                        if (CheckInventory(req.ID))
                        {
                            Logger($"Cannot buy {item.Name} from {shopID}.", "CanBuy");
                            Logger($"You own {Bot.Inventory.GetQuantity(req.ID)}x {req.Name}.", "CanBuy");
                            Logger($"You need {total_quant}.", "CanBuy");

                            return false;
                        }
                        Logger($"Cannot buy {item.Name} from {shopID} because {req.Name} is missing.", "CanBuy");
                        return false;
                    }
                }
            }

            if (item.Cost > 0)
            {
                //Gold check
                if (!item.Coins)
                {
                    int total_gold_cost = buy_count * item.Cost;
                    if (total_gold_cost > 100000000)
                    {
                        Logger($"Cannot buy more than 100 mil worth of items.", "CanBuy");
                        return false;
                    }
                    else if (total_gold_cost > Bot.Player.Gold)
                    {
                        Logger($"Cannot buy {item.Name} from {shopID}.", "CanBuy");
                        Logger($"You own {Bot.Inventory.GetQuantity(item.ID)}x {item.Name}.", "CanBuy");
                        Logger($"You need {Bot.Inventory.GetQuantity(item.ID) + buy_count}.", "CanBuy");
                        Logger($"You are missing {total_gold_cost - Bot.Player.Gold} gold to buy enough.", "CanBuy");
                        return false;
                    }
                }
                //AC costing check
                else
                {
                    int total_ac_cost = buy_count * item.Cost;
                    if (Bot.ShowMessageBox(
                            $"The bot is about to buy \"{item.Name}\" {buy_count} times, which costs {total_ac_cost} AC, do you accept this?",
                            "Warning: Costs AC!", true)
                            != true)
                    {
                        Logger($"Cannot buy {item.Name} from {shopID} because you didn't allow the bot to buy the item", "CanBuy");
                        return false;
                    }
                    else if (Bot.Flash.GetGameObject<int>("world.myAvatar.objData.intCoins") < total_ac_cost)
                    {
                        Logger($"Cannot buy {item.Name} from {shopID} because you are missing {Bot.Flash.GetGameObject<int>("world.myAvatar.objData.intCoins") - total_ac_cost} ACs", "CanBuy");
                        return false;
                    }
                }

            }
            return true;
        }
    }

    private void _CheckInventorySpace()
    {
        if (Bot.Inventory.Slots != 0 && Bot.Inventory.FreeSlots <= 0)
        {
            int prefCount = Bot.Inventory.UsedSlots;
            Logger($"Your inventory is very full [{prefCount}/{Bot.Inventory.Slots}], the bot will now clean it a bit before continueing.", "BuyItem");
            BankACMisc();
            if (Bot.Inventory.FreeSlots <= 0)
                Logger($"Banked {(prefCount - Bot.Inventory.UsedSlots)} items but it still wasn't enough. Please clean the rest of your inventory manually. Stopping the bot.", "BuyItem", true, true);
        }
    }

    private int _CalcBuyQuantity(ShopItem item, int requestedQuant, bool old = false)
    {
        if (requestedQuant > item.MaxStack)
        {
            Logger($"Attempting to buy more than {item.MaxStack} of {item.Name}. The developer needs to fix the calling script.", "BuyItem");
            Bot.Stop();
        }

        // requestQuant <= max stack.
        // No clamp checks needed, as Buys already asserts current quantity is less.
        int buy_quant;
        if ((buy_quant = requestedQuant - Bot.Inventory.GetQuantity(item.Name)) % item.Quantity != 0)
        {
            int diff = item.Quantity - (buy_quant % item.Quantity);
            SellItem(item.Name, Bot.Inventory.GetQuantity(item.Name) - diff);
            buy_quant += diff;
        }
        return buy_quant;
    }


    public int PointsToLevel(int points) => RepCPLevel.First(kvp => points <= kvp.Value).Key;

    private Dictionary<int, int> RepCPLevel = new()
    {
        { 1, 0 },
        { 2, 900 },
        { 3, 3600 },
        { 4, 10000 },
        { 5, 22500 },
        { 6, 44100 },
        { 7, 78400 },
        { 8, 129600 },
        { 9, 202500 },
        { 10, 302500 },
    };

    /// <summary>
    /// Sells a item till you have the desired quantity
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="all">Set to true if you wish to sell all the items</param>
    public void SellItem(string itemName, int quant = 0, bool all = false)
    {
        if (!(quant > 0 ? CheckInventory(itemName, quant) : CheckInventory(itemName)) || !Bot.Inventory.TryGetItem(itemName, out var item))
            return;
        JumpWait();

        if (!all)
        {
            // Inv quant >= current quantity.
            if (Bot.Options.SafeTimings)
                Bot.Wait.ForActionCooldown(GameActions.SellItem);
            Bot.Send.Packet($"%xt%zm%sellItem%{Bot.Map.RoomID}%{item!.ID}%{item!.Quantity - quant}%{item!.CharItemID}%");
            if (Bot.Options.SafeTimings)
                Bot.Wait.ForItemSell();

            Bot.Sleep(ActionDelay);
            return;
        }
        else Bot.Shops.SellItem(itemName);

        Logger($"{(all ? string.Empty : quant.ToString())} {itemName} sold");
    }

    public List<ShopItem> GetShopItems(string map, int shopID)
    {
        Bot.Wait.ForTrue(() => Bot.Shops.ID == shopID, () =>
        {
            Join(map);
            Bot.Shops.Load(shopID);
            Bot.Sleep(ActionDelay);
        }, 20, 1000);

        if (Bot.Shops.ID != shopID || Bot.Shops.Items == null)
        {
            Bot.ShowMessageBox("Failed to load shop the shop and get it's data" + reinstallCleanFlash, "Shop Data Loading Failed");
            return new();
        }
        return Bot.Shops.Items;
    }

    public ShopItem? parseShopItem(List<ShopItem> shopItem, int shopID, string itemNameID, int shopItemID = 0)
    {
        if (shopItem.Count == 0)
        {
            Logger($"Item {itemNameID} not found in shop {shopID}.");
            return null;
        }
        else if (shopItem.Count > 1)
        {
            if (shopItemID > 0)
            {
                if (!shopItem.Any(x => x.ShopItemID == shopItemID))
                {
                    Logger($"Item {itemNameID} with ShopItemID {shopItemID} was not in {shopID}. The developer needs to correct the Shop Item ID.");
                    return null;
                }
                return shopItem.First(x => x.ShopItemID == shopItemID);
            }
            Logger($"Multiple items found with the name {itemNameID} in shop {shopID}. The developer needs to specify the Shop Item ID.");
            return null;
        }

        return shopItem.First();
    }

    public void GhostItem(int ID, string name = "Ghost Item", int quantity = 1, bool temp = false, ItemCategory category = ItemCategory.Unknown, string? description = null, int level = 1, params (string, object)[] extraInfo)
    {
        if (temp ? (Bot.TempInv.Contains(ID) && Bot.TempInv.Contains(name)) : (Bot.Inventory.Contains(ID) && Bot.Inventory.Contains(name)))
            return;

        dynamic item = new ExpandoObject();

        item.ItemID = ID;
        item.sName = name;
        item.sDesc = description == null ? "A Ghost Item that mimics Item ID: " + ID : description;

        item.iLvl = level;
        if (quantity != 0) // This allows for ghost items without taking up slots, but it'll not work for bypasses
        {
            item.iQty = quantity;
            item.iStk = quantity > 0 ? quantity : 1;
        }

        item.sType = category == ItemCategory.Unknown ? "Item" : category.ToString();
        #region icon switch
        item.sIcon = (category) switch
        {
            ItemCategory.Sword => "iwsword",
            ItemCategory.Axe => "iwaxe",
            ItemCategory.Dagger => "iwdagger",
            ItemCategory.Gun or ItemCategory.HandGun or ItemCategory.Rifle or ItemCategory.Whip => "iwgun",
            ItemCategory.Bow => "iwbow",
            ItemCategory.Mace => "iwmace",
            ItemCategory.Gauntlet => "iwclaws",
            ItemCategory.Polearm => "iwpolearm",
            ItemCategory.Staff => "iwstaff",
            ItemCategory.Wand => "iwwand",

            ItemCategory.Class => "iiclass",
            ItemCategory.Armor => "iwarmor",
            ItemCategory.Helm => "iihelm",
            ItemCategory.Cape => "iicape",
            ItemCategory.Pet => "iipet",

            ItemCategory.Amulet or ItemCategory.Necklace => "iin1",
            // Ground Rune
            ItemCategory.Misc => "imr2",

            ItemCategory.House => "ihhouse",
            ItemCategory.WallItem => "ihwall",
            ItemCategory.FloorItem => "ihfloor",

            ItemCategory.Enhancement => "none",

            //Default (Unknown, Note, Resource, Item, ServerUse)
            _ => "iibag",
        };
        #endregion
        // Add enhancements property for enhancable equipment

        item.bEquip = 0;
        item.bStaff = 0;

        // Adding / modifying based on extra info
        var _item = item as IDictionary<string, object>;
        foreach (var info in extraInfo)
            _item![info.Item1] = info.Item2;
        //if (item.sLink is not null && item.sFile is not null)
        //    item.bSCP = false;

        // Yes it needs to call 'item', not '_item', they are linked in memory
        Bot.Flash.CallGameFunction("world.myAvatar.addItem", item);
    }

    /// <summary>
    /// Removes the specified items from players inventory (Banks AC items)
    /// </summary>
    /// <param name="items">Items to Trash/Bank</param>
    public void TrashCan(params string[] items)
    {
        JumpWait();
        foreach (string item in items)
        {
            if (!Bot.Inventory.TryGetItem(item, out var TrashItem) || TrashItem == null)
                continue;

            if (!TrashItem.Coins)
            {
                Logger($"Trashed: \"{TrashItem}\" x{TrashItem.Quantity}");
                Bot.Send.Packet($"%xt%zm%removeItem%{Bot.Map.RoomID}%{TrashItem.ID}%{Bot.Player.ID}%{TrashItem.Quantity}%");
            }
            else ToBank(item);
        }
    }

    #endregion

    #region Drops

    /// <summary>
    /// Adds drops to the pickup list, un-bank the items.
    /// </summary>
    /// <param name="items">Items to add</param>
    public void AddDrop(params string[] items)
    {
        if (items == null || items.Length == 0)
            return;
        Unbank(items);
        Bot.Drops.Add(items);
    }

    /// <summary>
    /// Adds drops to the pickup list, un-bank the items.
    /// </summary>
    /// <param name="items">Items to add</param>
    public void AddDrop(params int[] items)
    {
        Unbank(items);
        Bot.Drops.Add(items);
    }

    /// <summary>
    /// Removes drops from the pickup list.
    /// </summary>
    /// <param name="items">Items to remove</param>
    public void RemoveDrop(params string[] items)
    {
        Bot.Drops.Remove(items);
    }

    /// <summary>
    /// Removes drops from the pickup list.
    /// </summary>
    /// <param name="items">Items to remove</param>
    public void RemoveDrop(params int[] items)
    {
        Bot.Drops.Remove(items);
    }

    #endregion

    #region Quest
    private CancellationTokenSource? questCTS = null;
    /// <summary>
    /// This will register quests to be completed while doing something else, i.e. while in combat.
    /// If it has quests already registered, it will cancel them first and then register the new quests.
    /// </summary>
    /// <param name="questIDs">ID of the quests to be completed.</param>
    public void RegisterQuests(params int[] questIDs)
    {
        if (questCTS is not null)
            CancelRegisteredQuests();

        // Defining all the lists to be used=
        List<Quest> questData = EnsureLoad(questIDs);
        Dictionary<Quest, int> chooseQuests = new();
        Dictionary<Quest, int> nonChooseQuests = new();

        foreach (Quest q in questData)
        {
            bool shouldBreak = false;
            // Removing quests that you can't accept
            foreach (ItemBase req in q.AcceptRequirements)
            {
                if (!CheckInventory(req.Name))
                {
                    Logger($"Missing requirement {req.Name} for \"{q.Name}\" [{q.ID}]");
                    shouldBreak = true;
                    break;
                }
            }
            if (shouldBreak)
                break;

            // Separating the quests into choose and non-choose
            if (q.SimpleRewards.Any(r => r.Type == 2))
                chooseQuests.Add(q, 0);
            else
                nonChooseQuests.Add(q, 0);
        }

        registeredQuests = questIDs;
        EnsureAccept(questIDs);
        questCTS = new();
        Task.Run(async () =>
        {
            while (!Bot.ShouldExit && !questCTS.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(ActionDelay);

                    // Quests that dont need a choice
                    foreach (KeyValuePair<Quest, int> kvp in nonChooseQuests)
                    {
                        if (!Bot.Quests.IsInProgress(kvp.Key.ID))
                            EnsureAccept(kvp.Key.ID);
                        if (Bot.Quests.CanCompleteFullCheck(kvp.Key.ID))
                        {
                            int amountTurnedIn = EnsureCompleteMulti(kvp.Key.ID);
                            if (amountTurnedIn == 0)
                                continue;
                            await Task.Delay(ActionDelay);
                            EnsureAccept(kvp.Key.ID);
                            nonChooseQuests[kvp.Key] = nonChooseQuests[kvp.Key] + amountTurnedIn;
                            Logger($"Quest completed x{nonChooseQuests[kvp.Key]} times: [{kvp.Key.ID}] \"{kvp.Key.Name}\"");
                        }
                    }

                    // Quests that need a choice
                    foreach (KeyValuePair<Quest, int> kvp in chooseQuests)
                    {
                        if (!Bot.Quests.IsInProgress(kvp.Key.ID))
                            EnsureAccept(kvp.Key.ID);

                        if (Bot.Quests.CanCompleteFullCheck(kvp.Key.ID))
                        {
                            // Finding the list of items you dont have yet.
                            List<SimpleReward> simpleRewards =
                                kvp.Key.SimpleRewards.Where(r => r.Type == 2 &&
                                    !CheckInventory(r.ID, toInv: false)).ToList();

                            // If you have at least 1 of each item, start finding items that you dont have max stack of yet
                            if (simpleRewards.Count == 0)
                            {
                                List<int> matches = kvp.Key.Rewards.Where(x => !CheckInventory(x.ID, x.MaxStack, toInv: false)).Select(i => i.ID).ToList();
                                simpleRewards =
                                    kvp.Key.SimpleRewards.Where(r => r.Type == 2 && matches.Contains(r.ID)).ToList();
                            }
                            if (simpleRewards.Count == 0)
                            {
                                EnsureComplete(kvp.Key.ID);
                                await Task.Delay(ActionDelay);
                                EnsureAccept(kvp.Key.ID);
                                continue;
                            }

                            Bot.Drops.Add(kvp.Key.Rewards.Where(x => simpleRewards.Any(t => t.ID == x.ID)).Select(i => i.Name).ToArray());
                            EnsureComplete(kvp.Key.ID, simpleRewards.First().ID);
                            await Task.Delay(ActionDelay);
                            EnsureAccept(kvp.Key.ID);
                            Logger($"Quest completed x{chooseQuests[kvp.Key]++} times: [{kvp.Key.ID}] \"{kvp.Key.Name}\" (Got \"{kvp.Key.Rewards.First(x => x.ID == simpleRewards.First().ID).Name}\")");
                        }
                    }
                }
                catch
                {

                }
            }
            questCTS = null;
        });
    }

    /// <summary>
    /// Cancels the current registered quests.
    /// </summary>
    public void CancelRegisteredQuests()
    {
        questCTS?.Cancel();
        Bot.Wait.ForTrue(() => questCTS == null, 30);
        AbandonQuest(registeredQuests!);
        registeredQuests = null;
    }
    private int[]? registeredQuests = null;

    /// <summary>
    /// Ensures you are out of combat before accepting the quest
    /// </summary>
    /// <param name="questID">ID of the quest to accept</param>
    public bool EnsureAccept(int questID)
    {
        Quest QuestData = EnsureLoad(questID);

        if (QuestData.Upgrade && !IsMember)
            Logger($"\"{QuestData.Name}\" [{questID}] is member-only, stopping the bot.", stopBot: true);

        if (Bot.Quests.IsInProgress(questID))
            return true;
        if (questID <= 0)
            return false;

        Bot.Drops.Add(QuestData.Requirements.Where(x => !x.Temp).Select(y => y.Name).ToArray());
        Bot.Sleep(ActionDelay);
        return Bot.Quests.EnsureAccept(questID);
    }

    /// <summary>
    /// Accepts all the quests given
    /// </summary>
    /// <param name="questIDs">IDs of the quests</param>
    public void EnsureAccept(params int[] questIDs)
    {
        List<Quest> QuestData = EnsureLoad(questIDs);
        foreach (Quest quest in QuestData)
        {
            if (quest.Upgrade && !IsMember)
                Logger($"\"{quest.Name}\" [{quest.ID}] is member-only, stopping the bot.", stopBot: true);

            if (Bot.Quests.IsInProgress(quest.ID) || quest.ID <= 0)
                continue;

            Bot.Drops.Add(quest.Requirements.Where(x => !x.Temp).Select(y => y.Name).ToArray());
            Bot.Sleep(ActionDelay);
            Bot.Quests.EnsureAccept(quest.ID);
        }
    }

    /// <summary>
    /// Completes the quest with a choose-able reward item
    /// </summary>
    /// <param name="questID">ID of the quest to complete</param>
    /// <param name="itemID">ID of the choose-able reward item</param>
    public bool EnsureComplete(int questID, int itemID = -1)
    {
        if (questID <= 0)
            return false;
        Bot.Sleep(ActionDelay);
        return Bot.Quests.EnsureComplete(questID, itemID);
    }

    /// <summary>
    /// Completes all the quests given but doesn't support quests with choose-able rewards
    /// </summary>
    /// <param name="questIDs">IDs of the quests</param>
    public void EnsureComplete(params int[] questIDs)
    {
        Bot.Quests.EnsureComplete(questIDs);
    }

    /// <summary>
    /// Completes a quest and choose any item from it that you don't have (automatically accepts the drop)
    /// </summary>
    /// <param name="questID">ID of the quest</param>
    /// <param name="itemList">List of the items to get, if you want all just let it be null</param>
    public bool EnsureCompleteChoose(int questID, string[]? itemList = null)
    {
        if (questID <= 0)
            return false;
        Bot.Sleep(ActionDelay);
        Quest quest = EnsureLoad(questID);
        if (quest is not null)
        {
            foreach (ItemBase item in quest.Rewards)
            {
                if (!CheckInventory(item.Name, toInv: false)
                    && (itemList == null || (itemList != null && itemList.Contains(item.Name))))
                {
                    bool completed = Bot.Quests.EnsureComplete(questID, item.ID);
                    Bot.Drops.Pickup(item.Name);
                    Bot.Wait.ForPickup(item.Name);
                    return completed;
                }
            }
        }
        else
        {
            Logger($"Failed to load Quest {questID}, EnsureCompleteChoose failed");
            return false;
        }
        Logger($"Could not complete the quest {questID}. Maybe all items are already in your inventory");
        return false;
    }

    /// <summary>
    /// Completes the quest with a choose-able reward item
    /// </summary>
    /// <param name="questID">ID of the quest to complete</param>
    /// <param name="amount">Amount of times you want it to turn in the quest, -1 is maximum amount possible.</param>
    /// <param name="itemID">ID of the choose-able reward item</param>
    public int EnsureCompleteMulti(int questID, int amount = -1, int itemID = -1)
    {
        var q = EnsureLoad(questID);

        int turnIns = 0;
        if (q.Once || !String.IsNullOrEmpty(q.Field))
            turnIns = 1;
        else
        {
            int possibleTurnin = Bot.Flash.CallGameFunction<int>("world.maximumQuestTurnIns", questID);
            turnIns = possibleTurnin > amount && amount > 0 ? amount : possibleTurnin;
            if (turnIns == 0)
                return 0;
        }
        Bot.Flash.CallGameFunction("world.tryQuestComplete", questID, itemID, false, turnIns);
        if (Bot.Options.SafeTimings)
            Bot.Wait.ForQuestComplete(questID);
        return !Bot.Quests.IsInProgress(questID) ? turnIns : 0;
    }

    public Quest EnsureLoad(int questID)
    {
        var toReturn = Bot.Quests.Tree.Find(x => x.ID == questID) ?? _EnsureLoad1() ?? _EnsureLoad2();
        if (toReturn == null)
        {
            Bot.Quests.Load(questID);
            toReturn = Bot.Quests.Tree.Find(x => x.ID == questID) ?? _EnsureLoad1() ?? _EnsureLoad2();

            if (toReturn == null)
            {
                toReturn = EnsureLoadFromFile(questID).Result?.FirstOrDefault();

                if (toReturn == null)
                {
                    Logger($"Failed to get the Quest Object for questID {questID}" + reinstallCleanFlash, "EnsureLoad A.0", messageBox: true, stopBot: true);
                    return new();
                }
            }
        }

        return toReturn;

        Quest? _EnsureLoad1()
        {
            Bot.Sleep(ActionDelay);
            Bot.Wait.ForTrue(() => Bot.Quests.Tree.Contains(x => x.ID == questID), () => Bot.Quests.Load(questID), 20);
            return Bot.Quests.Tree.Find(q => q.ID == questID)!;
        }
        Quest? _EnsureLoad2()
        {
            Bot.Sleep(ActionDelay);
            return Bot.Quests.EnsureLoad(questID);
        }
    }

    public List<Quest> EnsureLoad(params int[] questIDs)
    {
        List<Quest>? quests = Bot.Quests.Tree.Where(x => questIDs.Contains(x.ID)).ToList();
        if (quests.Count == questIDs.Length)
            return quests;

        List<int> missing = questIDs.Where(x => !quests.Any(y => y.ID == x)).ToList();
        Bot.Sleep(ActionDelay);
        for (int i = 0; i < missing.Count; i = i + 30)
        {
            Bot.Quests.Load(missing.ToArray()[i..(missing.Count > i ? missing.Count : i + 30)]);
            Bot.Sleep(1500);
        }
        Bot.Wait.ForTrue(() => questIDs.All(id => Bot.Quests.Tree.Any(q => q.ID == id)), 20);

        List<Quest>? toReturn = Bot.Quests.Tree.Where(x => questIDs.Contains(x.ID)).ToList();
        if (toReturn == null || !toReturn.Any())
        {
            toReturn = EnsureLoadFromFile(questIDs).Result;
            if (toReturn == null || !toReturn.Any())
            {
                Logger($"Failed to get the Quest Object for questIDs {String.Join(" | ", questIDs)}" + reinstallCleanFlash, "EnsureLoad B.4", messageBox: true, stopBot: true);
                return new();
            }
        }

        return toReturn;
    }

    private async Task<List<Quest>?> EnsureLoadFromFile(params int[] questIDs)
    {
        List<Quest>? toReturn;
        //First try local Quest.txt file(if its not too old)
        if (File.GetLastWriteTime(ClientFileSources.SkuaQuestsFile).Subtract(DateTime.Now).TotalDays < 14 && LoadLocal())
            return toReturn!;

        // Otherwise try file on Github
        toReturn = (OnlineQuestsFile ??=
                        JsonConvert.DeserializeObject<List<QuestData>?>(
                            GetRequest("https://raw.githubusercontent.com/BrenoHenrike/Scripts/Skua/QuestData.json")))?
                    .Where(q => questIDs.Contains(q.ID)).Select(q => toQuest(q)).ToList();
        if (toReturn != null && toReturn.Any() && questIDs.All(q => toReturn.Any(x => x.ID == q)))
            return toReturn;

        // If Github failed, manually update the quest file 
        await UpdateQuestFile();
        if (LoadLocal())
            return toReturn!;

        // Failure
        Logger($"Failed to get the Quest Object for questIDs {String.Join(" | ", questIDs)}", "EnsureLoad C.0", messageBox: true, stopBot: true);
        return null;

        bool LoadLocal()
        {
            toReturn = (LocalQuestsFile ??= JsonConvert.DeserializeObject<List<QuestData>?>(File.ReadAllText(ClientFileSources.SkuaQuestsFile)))?
                .Where(q => questIDs.Contains(q.ID)).Select(q => toQuest(q)).ToList();
            return (toReturn != null && toReturn.Any() && questIDs.All(q => toReturn.Any(x => x.ID == q)));
        }


        Quest toQuest(QuestData data)
        {
            return new Quest()
            {
                ID = data.ID,
                Slot = data.Slot,
                Value = data.Value,
                Name = data.Name,
                Description = String.Empty, // Not found in QuestData
                EndText = String.Empty, // Not found in QuestData
                Once = data.Once,
                Field = data.Field,
                Index = data.Index,
                Upgrade = data.Upgrade,
                Level = data.Level,
                RequiredClassID = data.RequiredClassID,
                RequiredClassPoints = data.RequiredClassPoints,
                RequiredFactionId = data.RequiredFactionId,
                RequiredFactionRep = data.RequiredFactionRep,
                Gold = data.Gold,
                XP = data.XP,
                Status = null!, // Not found in QuestData
                //Active is based on Status being NULL or not
                AcceptRequirements = data.AcceptRequirements,
                //Requirements cant be writen to
                Rewards = data.Rewards,
                SimpleRewards = data.SimpleRewards,
            };
        }
        async Task UpdateQuestFile()
        {
            CancellationTokenSource? _loaderCTS;
            _loaderCTS = new();
            List<QuestData> questData =
                await (LoaderService ??= Ioc.Default.GetRequiredService<IQuestDataLoaderService>())
                .UpdateAsync("Quests.txt", false, null, _loaderCTS.Token);
            _loaderCTS.Dispose();
            _loaderCTS = null;
        }
    }
    private List<QuestData>? LocalQuestsFile;
    private List<QuestData>? OnlineQuestsFile;
    private IQuestDataLoaderService? LoaderService;

    public void AbandonQuest(params int[] questIDs)
    {
        if (questIDs == null || questIDs.Length == 0)
            return;

        foreach (var q in EnsureLoad(questIDs))
        {
            if (q == null || !q.Active)
                continue;
            Bot.Flash.CallGameFunction("world.abandonQuest", q.ID);
            Bot.Wait.ForTrue(() => !EnsureLoad(q.ID).Active, 20);
        }
    }

    public string[] QuestRewards(params int[] questIDs)
    {
        if (questIDs.Length == 0)
            return Array.Empty<string>();
        List<string> toReturn = new();
        foreach (var q in EnsureLoad(questIDs))
        {
            if (q.Rewards == null || q.Rewards.Count() == 0)
                continue;
            toReturn.AddRange(q.Rewards.Select(i => i.Name));
        }
        return toReturn.ToArray();
    }

    /// <summary>
    /// Accepts and then completes the quest, used inside a loop
    /// </summary>
    /// <param name="questID">ID of the quest</param>
    /// <param name="itemID">ID of the choose-able reward item</param>
    public void ChainComplete(int questID, int itemID = -1)
    {
        Bot.Quests.EnsureAccept(questID);
        Bot.Sleep(ActionDelay);
        Bot.Quests.EnsureComplete(questID, itemID);
    }

    /// <param name="QuestID">ID of the quest</param>
    public bool isCompletedBefore(int QuestID)
    {
        Quest? QuestData = EnsureLoad(QuestID);
        try
        {
            return QuestData.Slot < 0 || Bot.Flash.CallGameFunction<int>("world.getQuestValue", QuestData.Slot) >= QuestData.Value;
        }
        catch
        {
            QuestData = Bot.Quests.EnsureLoad(QuestID);
            return QuestData?.Slot < 0 || Bot.Flash.CallGameFunction<int>("world.getQuestValue", QuestData!.Slot) >= QuestData.Value;
        }
    }

    #endregion

    #region Kill

    /// <summary>
    /// Joins a map, jump & set the spawn point and kills the specified monster
    /// </summary>
    /// <param name="map">Map to join</param>
    /// <param name="cell">Cell to jump to</param>
    /// <param name="pad">Pad to jump to</param>
    /// <param name="monster">Name of the monster to kill</param>
    /// <param name="item">Item to kill the monster for, if null will just kill the monster 1 time</param>
    /// <param name="quant">Desired quantity of the item</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    /// <param name="log">Whether it will log that it is killing the monster</param>
    public void KillMonster(string map, string cell, string pad, string monster, string? item = null, int quant = 1, bool isTemp = true, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;
        if (!isTemp && item != null)
            AddDrop(item);

        Join(map, cell, pad, publicRoom: publicRoom);
        Jump(cell, pad);

        if (item == null)
        {
            if (log)
                Logger($"Killing {monster}");
            ToggleAggro(true);
            Bot.Kill.Monster(monster);
            ToggleAggro(false);
            Rest();
        }
        else _KillForItem(monster, item, quant, isTemp, log: log);
    }

    /// <summary>
    /// Kills a monster using it's ID
    /// </summary>
    /// <param name="map">Map to join</param>
    /// <param name="cell">Cell to jump to</param>
    /// <param name="pad">Pad to jump to</param>
    /// <param name="monsterID">ID of the monster</param>
    /// <param name="item">Item to kill the monster for, if null will just kill the monster 1 time</param>
    /// <param name="quant">Desired quantity of the item</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    /// <param name="log">Whether it will log that it is killing the monster</param>
    public void KillMonster(string map, string cell, string pad, int monsterID, string? item = null, int quant = 1, bool isTemp = true, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;
        if (!isTemp && item != null)
            AddDrop(item);

        Join(map, cell, pad, publicRoom: publicRoom);
        Jump(cell, pad);

        Monster? monster = Bot.Monsters.CurrentMonsters?.Find(m => m.ID == monsterID);
        if (monster == null)
        {
            Logger($"Monster [{monsterID}] not found. Something is wrong. Stopping bot", messageBox: true, stopBot: true);
            return;
        }

        if (item == null)
        {
            if (log)
                Logger($"Killing {monster}");
            ToggleAggro(true);
            Bot.Kill.Monster(monster);
            ToggleAggro(false);
            Rest();
        }
        else _KillForItem(monster.Name, item, quant, isTemp, log: log);
    }

    /// <summary>
    /// Joins a map and hunt for the monster
    /// </summary>
    /// <param name="map">Map to join</param>
    /// <param name="monster">Name of the monster to kill</param>
    /// <param name="item">Item to hunt the monster for, if null will just hunt & kill the monster 1 time</param>
    /// <param name="quant">Desired quantity of the item</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void HuntMonster(string map, string monster, string? item = null, int quant = 1, bool isTemp = true, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;

        Join(map, publicRoom: publicRoom);

        if (item == null)
        {
            if (log)
                Logger($"Hunting {monster}");
            Bot.Hunt.Monster(monster);
            Rest();
        }
        else
        {
            if (!isTemp)
                AddDrop(item);
            if (log)
                Logger($"Hunting {monster} for {item}, ({dynamicQuant(item, isTemp)}/{quant}) [Temp = {isTemp}]");

            while (!Bot.ShouldExit && (isTemp ? !Bot.TempInv.Contains(item, quant) : !CheckInventory(item, quant)))
            {
                if (!Bot.Combat.StopAttacking)
                {
                    Bot.Hunt.Monster(monster);
                }
                Bot.Sleep(ActionDelay);
                Rest();
            }
        }
    }

    /// <summary>
    /// Kills a monster using it's MapID
    /// </summary>
    /// <param name="map">Map to join</param>
    /// <param name="cell">Cell to jump to</param>
    /// <param name="pad">Pad to jump to</param>
    /// <param name="monsterID">ID of the monster</param>
    /// <param name="item">Item to kill the monster for, if null will just kill the monster 1 time</param>
    /// <param name="quant">Desired quantity of the item</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    /// <param name="log">Whether it will log that it is killing the monster</param>
    public void HuntMonsterMapID(string map, int monsterMapID, string? item = null, int quant = 1, bool isTemp = true, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;

        Join(map, publicRoom: publicRoom);

        Monster? monster = Bot.Monsters.MapMonsters?.Find(m => m.MapID == monsterMapID);
        if (monster == null)
        {
            Logger($"Failed to find monsterMapID {monsterMapID} in {map}");
            return;
        }
        Jump(monster.Cell, "Left");

        if (item == null)
        {
            if (log)
                Logger($"Killing {monster.Name}");
            Bot.Kill.Monster(monster);
            Rest();
        }
        else
        {
            if (!isTemp)
                AddDrop(item);
            if (log)
                Logger($"Killing {monster.Name} for {item}, ({dynamicQuant(item, isTemp)}/{quant}) [Temp = {isTemp}]");

            while (!Bot.ShouldExit && (isTemp ? !Bot.TempInv.Contains(item, quant) : !CheckInventory(item, quant)))
            {
                if (!Bot.Combat.StopAttacking)
                    Bot.Combat.Attack(monster);
                Bot.Sleep(ActionDelay);
                Rest();
            }
        }
    }

    /// <summary>
    /// Kill Escherion for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void KillEscherion(string? item = null, int quant = 1, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;

        // DebugLogger(this);
        Join("escherion", "Boss", "Left", publicRoom: publicRoom);

        if (item != null)
            FarmingLogger(item, quant);

        if (item == null)
        {
            // DebugLogger(this);
            if (log)
                Logger("Killing Escherion");
            while (!Bot.ShouldExit && IsMonsterAlive("Escherion"))
            {
                // DebugLogger(this);
                while (!Bot.ShouldExit && IsMonsterAlive("Staff of Inversion"))
                    Bot.Kill.Monster("Staff of Inversion");
                Bot.Combat.Attack("Escherion");
                Bot.Sleep(1000);
            }
        }
        else
        {
            if (!isTemp)
                AddDrop(item);
            if (log)
                Logger($"Killing Escherion for {item} ({dynamicQuant(item, isTemp)}/{quant}) [Temp = {isTemp}]");
            while (!Bot.ShouldExit && !CheckInventory(item, quant))
            {
                while (!Bot.ShouldExit && Bot.Player.Cell != "Boss")
                {
                    // DebugLogger(this);
                    Jump("Boss", "Left");
                    Bot.Sleep(ActionDelay);
                }
                // DebugLogger(this);
                while (!Bot.ShouldExit && IsMonsterAlive("Staff of Inversion"))
                    Bot.Kill.Monster("Staff of Inversion");
                Bot.Combat.Attack("Escherion");
                Bot.Sleep(1000);
            }
            // DebugLogger(this);
        }
    }

    /// <summary>
    /// Kill Vath for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void KillVath(string? item = null, int quant = 1, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;

        Join("stalagbite", "r2", "Left");

        if (item == null)
        {
            if (log)
                Logger("Killing Vath");
            while (!Bot.ShouldExit && IsMonsterAlive("Vath"))
                _killVath();
        }
        else
        {
            if (!isTemp)
                AddDrop(item);
            if (log)
                Logger($"Killing Vath for {item} ({dynamicQuant(item, isTemp)}/{quant}) [Temp = {isTemp}]");
            while (!Bot.ShouldExit && !CheckInventory(item, quant))
                _killVath();
        }

        void _killVath()
        {
            if (IsMonsterAlive("Stalagbite"))
                Bot.Kill.Monster("Stalagbite");
            Bot.Combat.Attack("Vath");
            Bot.Sleep(1000);
        }
    }

    /// <summary>
    /// Kill Kitsune for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void KillKitsune(string? item = null, int quant = 1, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (item != null && (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant)))
            return;

        Join("kitsune", "Boss", "Left");
        Bot.Events.ExtensionPacketReceived += KitsuneListener;

        if (item == null)
        {
            if (log)
                Logger("Killing Kitsune");
            while (!Bot.ShouldExit && IsMonsterAlive("Kitsune"))
                Bot.Combat.Attack("Kitsune");
        }
        else
        {
            if (!isTemp)
                AddDrop(item);
            if (log)
                Logger($"Killing Kitsune for {item} ({dynamicQuant(item, isTemp)}/{quant}) [Temp = {isTemp}]");
            while (!Bot.ShouldExit && !CheckInventory(item, quant))
                Bot.Combat.Attack("Kitsune");
        }
        Bot.Events.ExtensionPacketReceived -= KitsuneListener;

        void KitsuneListener(dynamic packet)
        {
            string type = packet["params"].type;
            dynamic data = packet["params"].dataObj;
            if (type is not null and "json")
            {
                string cmd = data.cmd.ToString();
                switch (cmd)
                {
                    case "ct":
                        if (data.a is not null)
                        {
                            foreach (var a in data.a)
                            {
                                if (a is null)
                                    continue;

                                if (a.aura is not null && (string)a.aura["nam"] is "Shapeshifted")
                                {
                                    Bot.Combat.StopAttacking = ((string)a.cmd)[^0] == '+';
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Kill Vath for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    /// <param name="Phase">Which phase of the boss to kill>
    public void KillTrigoras(string item, int quant = 1, int Phase = 1, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant))
            return;

        EquipClass(ClassType.Solo);
        Join("trigoras");

        while (!Bot.ShouldExit && !CheckInventory(item, quant))
        {
            Jump(Phase == 1 ? "r4" : "r4a", "Left");
            Bot.Combat.Attack("trigoras");
            Bot.Wait.ForCellChange(Phase == 1 ? "r4a" : "Enter");
        }
        Bot.Wait.ForCellChange(Phase == 1 ? "r4a" : "Enter");
        JumpWait();
    }

    /// <summary>
    /// Kill DoomKitten for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void KillDoomKitten(string item, int quant = 1, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant))
            return;

        string[] DOTClasses = {
            "ShadowStalker of Time",
            "ShadowWeaver of Time",
            "ShadowWalker of Time",
            "Infinity Knight",
            "Interstellar Knight",
            "Void Highlord",
            "Dragon of Time",
            "Timeless Dark Caster",
            "Frostval Barbarian",
            "Blaze Binder",
            "DeathKnight",
            "DragonSoul Shinobi",
            "Shadow Dragon Shinobi",
            "Legion Revenant",
        };

        if (!DOTClasses.Any(c => CheckInventory(c)))
        {
            Bot.Log("--------------------------------");
            Logger("Possible classes for DoomKitten:");
            DOTClasses.ForEach(l => Logger(l));
            Bot.Log("--------------------------------");

            Logger($"\'Damage over Time\' class / VHL not found. See the logs to see suggestions. Please get one and run the bot agian. Stopping.", messageBox: true, stopBot: true);
        }

        if (CheckInventory("Shadow Dragon Shinobi") || CheckInventory("DragonSoul Shinobi"))
        {
            Logger("Due to the nature of this class and the hit range of the kitten, this is basicly RNG gl!");
            Equip(CheckInventory("Shadow Dragon Shinobi") ? "Shadow Dragon Shinobi" : "DragonSoul Shinobi");
            //tested Skillset is working properly and can get a kill.
            Bot.Skills.StartAdvanced("4H>50 | 3M<70S | 2H<50 M>70S | 1H>50", 150);
        }
        else Bot.Skills.StartAdvanced(DOTClasses.ToList().First(c => Bot.Inventory.Contains(c)), true, ClassUseMode.Base);
        HuntMonster("doomkitten", "Doomkitten", item, quant, isTemp, log, publicRoom);
    }

    /// <summary>
    /// Kill Xiang for the desired item
    /// </summary>
    /// <param name="item">Item name</param>
    /// <param name="quant">Desired quantity</param>
    /// <param name="ultra">Fight the ultra variant</param>
    /// <param name="isTemp">Whether the item is temporary</param>
    public void KillXiang(string item, int quant = 1, bool ultra = false, bool isTemp = false, bool log = true, bool publicRoom = false)
    {
        if (isTemp ? Bot.TempInv.Contains(item, quant) : CheckInventory(item, quant))
            return;

        JumpWait();

        if (CheckInventory("Dragon of Time"))
            Bot.Skills.StartAdvanced("Dragon of Time", true, ClassUseMode.Solo);
        else if (CheckInventory("Healer (Rare)"))
            Bot.Skills.StartAdvanced("Healer (Rare)", true, ClassUseMode.Base);
        else if (CheckInventory("Healer"))
            Bot.Skills.StartAdvanced("Healer", true, ClassUseMode.Base);

        KillMonster("mirrorportal", ultra ? "r6" : "r4", "Right", ultra ? "Ultra Xiang" : "Chaos Lord Xiang", item, quant, isTemp, log, publicRoom);
    }

    public void _KillForItem(string name, string item, int quantity, bool isTemp = false, bool rejectElse = false, bool log = true)
    {
        if (log)
            Logger($"Killing {name} for {item}, ({dynamicQuant(item, isTemp)}/{quantity}) [Temp = {isTemp}]");

        ToggleAggro(true);
        while (!Bot.ShouldExit && !CheckInventory(item, quantity))
        {
            if (!Bot.Combat.StopAttacking)
                Bot.Combat.Attack(name);
            Bot.Sleep(ActionDelay);
            if (rejectElse)
                Bot.Drops.RejectExcept(item);
        }
        ToggleAggro(false);
        Bot.Sleep(ActionDelay);
        Rest();
    }

    public bool IsMonsterAlive(Monster? mon)
        => mon != null && (mon.Alive || !KilledMonsters.Contains(mon.MapID));
    public bool IsMonsterAlive(string monsterName)
        => Bot.Monsters.CurrentMonsters.Where(m => m.Name == monsterName).Any(m => IsMonsterAlive(m));
    public bool IsMonsterAlive(int monsterID)
        => Bot.Monsters.CurrentMonsters.Where(m => m.ID == monsterID).Any(m => IsMonsterAlive(m));
    public bool IsMonsterAlive(int monsterMapID, bool useMapID)
        => IsMonsterAlive(Bot.Monsters.CurrentMonsters.Find(m => m.MapID == monsterMapID));

    private List<int> KilledMonsters = new();
    private void CleanKilledMonstersList(string map)
        => KilledMonsters.Clear();
    private void KilledMonsterListener(int monsterMapID)
        => KilledMonsters.Add(monsterMapID);
    private void RespawnListener(dynamic packet)
    { //%xt%respawnMon%-1%12% (monster map ID is 12 in this example)
        string type = packet["params"].type;
        dynamic data = packet["params"].dataObj;
        if (type is not null and "str")
        {
            string cmd = data[0];
            switch (cmd)
            {
                case "respawnMon":
                    KilledMonsters.RemoveAll(id => id == (int)data[2]);
                    break;
            }
        }
    }


    #endregion

    #region Utility

    // Whether the player is Member (set to true if neccessary during setOptions)
    public bool IsMember = false;

    public string Username()
    {
        try
        {
            return Bot.Flash.GetGameObject("sfc.myUserName")![1..^1];
        }
        catch
        {
            return Bot.Player.Username;
        }
    }


    /// <summary>
    /// Logs a line of text to the script log with time, method from where it's called and a message
    /// </summary>
    public void Logger(string message = "", [CallerMemberName] string caller = "", bool messageBox = false, bool stopBot = false)
    {
        Bot.Log($"[{DateTime.Now:HH:mm:ss}] ({caller})  {message}");
        if (LoggerInChat && Bot.Player.LoggedIn)
            Bot.Send.ClientModerator(message.Replace('[', '(').Replace(']', ')'), caller);
        if (messageBox & !ForceOffMessageboxes)
            Message(message, caller);
        if (stopBot)
        {
            scriptFinished = false;
            Bot.Stop(true);
        }
    }

    public void FarmingLogger(string item, int quant, [CallerMemberName] string caller = "")
        => Logger($"Farming {item} ({Bot.Inventory.GetQuantity(item)}/{quant})", caller);

    public void DebugLogger(object _this, string? marker = null, [CallerMemberName] string? caller = null, [CallerLineNumber] int lineNumber = 0)
    {
        if (!DL_Enabled || ((DL_MarkerFilter == null ? false : DL_MarkerFilter != marker) || (DL_CallerFilter == null ? false : DL_CallerFilter != caller)))
            return;

        string _class = _this.GetType().ToString();
        string[] compiledScript = CompiledScript();

        int compiledClassLine = Array.IndexOf(compiledScript, compiledScript.First(line => line.Trim() == $"public class {_class}")) + 1;
        string[] currentScript = File.ReadAllLines(Bot.Manager.LoadedScript);
        string[]? includedScript = null;

        bool inCurrentScript = false;
        if (currentScript.Any(line => line.Trim() == $"public class {_class}"))
            inCurrentScript = true;
        else
        {
            foreach (string cs in currentScript.Where(x => x.StartsWith("//cs_include")).ToArray())
            {
                List<string> pathParts = new() { ClientFileSources.SkuaDIR };
                pathParts.AddRange(cs.Replace("//cs_include ", "").Replace("\\", "/").Split('/'));
                includedScript = File.ReadAllLines(Path.Combine(pathParts.ToArray()));

                if (includedScript.Any(line => line.Trim() == $"public class {_class}"))
                    break;
            }
        }

        if (!inCurrentScript && includedScript == null)
        {
            Logger("includedScript is NULL", "DEBUG LOGGER");
            return;
        }

        int count = 0;
        int lastIndex = compiledClassLine;

        foreach (string l in compiledScript[compiledClassLine..Array.FindIndex(compiledScript, compiledClassLine, l => l == "}")])
        {
            if (!l.Contains("DebugLogger(this"))
                continue;

            count++;
            lastIndex = Array.FindIndex(compiledScript, lastIndex + 1, _l => _l.Trim() == l.Trim());
            if (lastIndex + 1 == lineNumber)
                break;
        }

        int count2 = 0;
        int lastIndex2 = -1;
        string[] selectedScript = inCurrentScript || includedScript == null ? currentScript : includedScript;
        foreach (string l in selectedScript)
        {
            if (!l.Contains("DebugLogger(this"))
                continue;

            count2++;
            lastIndex2 = Array.FindIndex(selectedScript, lastIndex2 + 1, _l => _l.Trim() == l.Trim());

            if (count == count2)
                break;
        }

        Logger($"{marker}{(String.IsNullOrEmpty(marker) ? null : " | ")}{_class} => {caller}, line {lastIndex2 + 1}", "DEBUG LOGGER");
    }
    private bool DL_Enabled { get; set; } = false;
    public string? DL_CallerFilter { get; set; } = null;
    public string? DL_MarkerFilter { get; set; } = null;
    public void DL_Enable()
    {
        DL_Enabled = true;
        LoggerInChat = false;
    }
    public string[] CompiledScript() => Bot.Manager.CompiledScript.Split(
                                                                new string[] { "\r\n", "\r", "\n" },
                                                                StringSplitOptions.None);

    /// <summary>
    /// Creates a Message Box with the desired text and caption
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="caption">Title of the box</param>
    public void Message(string message, string caption)
    {
        Bot.Handlers.RegisterOnce(1, (Bot) => Bot.ShowMessageBox(message, caption));
    }

    public int dynamicQuant(string item, bool isTemp) => isTemp ? Bot.TempInv.GetQuantity(item) : Bot.Inventory.GetQuantity(item);

    public void ConfigureAggro(bool status = true)
    {
        Logger("Configuring aggro");
        last_aggro_status = status;
    }

    public void ToggleAggro(bool enable)
    {
        if (enable)
        {
            if (last_aggro_status)
            {
                // If was previously aggro when untoggled
                // Set aggro back and flip last aggro
                //Logger("Flipping aggro to False");
                last_aggro_status = false;
                Bot.Options.AggroMonsters = true;
            }
            else
                return;
        }
        else
        {
            if (!Bot.Options.AggroMonsters)
                return;
            else
            {
                // If currently aggro, set last aggro to true
                // and flip current aggro status
                //Logger("Flipping aggro to False");
                last_aggro_status = true;
                Bot.Options.AggroMonsters = false;
            }
        }
    }
    private bool last_aggro_status = false;

    /// <summary>
    /// Send a packet to the server the desired amount of times
    /// </summary>
    /// <param name="packet">Packet to send</param>
    /// <param name="times">How many times to send</param>
    public void SendPackets(string packet, int times = 1, bool toClient = false)
    {
        for (int i = 0; i < times; i++)
        {
            if (toClient)
                Bot.Send.ClientPacket(packet);
            else
                Bot.Send.Packet(packet);
            Bot.Sleep(ActionDelay * 2);
        }
    }

    /// <summary>
    /// Rest the player until full if ShouldRest = true
    /// </summary>
    public void Rest()
    {
        if (ShouldRest)
            Bot.Player.Rest(true);
    }

    /// <summary>
    /// Logs the player out and then in again to the same server. Disables Options.AutoRelogin temporarily 
    /// </summary>
    public void Relogin()
    {
        if (Bot.Options.ReloginServer == null || !Bot.Servers.EnsureRelogin(Bot.Options.ReloginServer))
        {
            var servers = Bot.Servers.GetServers(true).Result;
            if (servers.Count() == 0)
                Logger("Failed to relogin: could not fetch server details" + (Bot.Options.ReloginServer == null ? '.' : " or the find the server you've set in Options > Game."), messageBox: true, stopBot: true);
            Bot.Servers.EnsureRelogin(servers.First(s => s.Name != "Class Test Realm").Name);
        }
    }

    /// <summary>
    /// Checks, and prompts for the latest Skua Version
    /// <param name="targetVersion">Current Skua Version to Check against</param>
    /// </summary>
    private void SkuaVersionChecker(string targetVersion)
    {
        if (Bot.Version == null || Version.Parse(targetVersion).CompareTo(Bot.Version) <= 0)
            return;

        if (Bot.ShowMessageBox($"This script requires Skua {targetVersion} or above, " +
        "click OK to open the download page of the latest release", "Outdated Skua detected", "OK").Text == "OK")
            Process.Start("explorer", "https://github.com/BrenoHenrike/Skua/releases/latest");
        Logger($"This script requires Skua {targetVersion} or above. Stopping the script", messageBox: true, stopBot: true);
    }

    ClassType currentClass = ClassType.None;
    bool usingSoloGeneric = false;
    bool usingFarmGeneric = false;
    /// <summary>
    /// Equips either the FarmClass or SoloClass from the CanChange section at the top of CoreBots 
    /// </summary>
    /// <param name="classToUse">Type "ClassType." and then either Farm or Solo in order to select which type it should swap too</param>
    public void EquipClass(ClassType classToUse)
    {
        if (currentClass == classToUse && Bot.Skills.TimerRunning)
            return;

        currentClass = classToUse;

        switch (classToUse)
        {
            case ClassType.Farm:
                if (_equipClass(usingFarmGeneric, FarmClass, FarmUseMode, FarmGearOn, FarmGear))
                    return;
                break;

            case ClassType.Solo:
                if (_equipClass(usingSoloGeneric, SoloClass, SoloUseMode, SoloGearOn, SoloGear))
                    return;
                break;
        }
        Bot.Skills.StartAdvanced(Bot.Player.CurrentClass?.Name ?? "generic", false);

        bool _equipClass(bool usingGeneric, string className, ClassUseMode classMode, bool useEquipment, string[] equipment)
        {
            if (usingGeneric)
                return false;

            if (!CheckInventory(className))
            {
                Logger("You do not own " + className);
                return false;
            }

            if (useEquipment && equipment.Any())
            {
                Bot.Sleep((int)(ActionDelay * 1.5));
                Equip(equipment);
            }

            className = className.Trim().ToLower();

            logEquip = false;
            Equip(Bot.Inventory.Items.First(x => x.Name.ToLower() == className && x.Category == ItemCategory.Class).ID);
            logEquip = true;

            Bot.Skills.StartAdvanced(className, false, classMode);
            return true;
        }
    }
    private bool logEquip = true;

    public void Equip(params string[] gear)
    {
        if (gear == null || gear.Length == 0)
            return;

        JumpWait();

        foreach (string item in gear)
        {
            if (String.IsNullOrEmpty(item) || String.IsNullOrWhiteSpace(item))
                continue;

            if (!Bot.Inventory.IsEquipped(item))
            {
                if (!CheckInventory(item))
                {
                    if (!Bot.ShouldExit)
                        Logger($"Equipping Failed: \"{item}\" not found in Inventory or Bank");
                    continue;
                }
                if (!Bot.Inventory.TryGetItem(item, out var _item))
                {
                    if (!Bot.ShouldExit)
                        Logger($"Equipping Failed: Could not parse \"{item}\" from your inventory");
                    continue;
                }
                _Equip(_item);
            }
        }
    }

    public void Equip(params int[] gear)
    {
        if (gear == null || gear.Length == 0)
            return;

        JumpWait();

        foreach (int item in gear)
        {
            if (item <= 0)
                continue;

            if (!Bot.Inventory.IsEquipped(item))
            {
                if (!CheckInventory(item))
                {
                    Logger($"Equipping Failed: \"{item}\" not found in Inventory or Bank");
                    continue;
                }
                if (!Bot.Inventory.TryGetItem(item, out var _item))
                {
                    Logger($"Equipping Failed: Could not parse \"{item}\" from your inventory");
                    continue;
                }
                _Equip(_item);
            }
        }
    }

    private void _Equip(InventoryItem? item)
    {
        if (item == null)
        {
            Logger($"Equipping Failed: Parsed object for \"{item}\" is null");
            return;
        }

        switch (item.CategoryString.ToLower())
        {
            case "item": // Consumables
                dynamic dItem = new ExpandoObject();
                dItem.ItemID = item.ID;
                dItem.sLink = Bot.Flash.GetGameObject<string>($"world.invTree.{item.ID}.sLink");
                dItem.sES = item.ItemGroup;
                dItem.sType = item.CategoryString;
                dItem.sIcon = Bot.Flash.GetGameObject<string>($"world.invTree.{item.ID}.sIcon");
                dItem.sFile = Bot.Flash.GetGameObject<string>($"world.invTree.{item.ID}.sFile");
                dItem.bUpg = item.Upgrade ? 1 : 0;
                dItem.sDesc = item.Description;
                dItem.bEquip = item.Equipped ? 1 : 0;
                dItem.sName = item.Name;
                dItem.sMeta = item.Meta;

                Bot.Flash.CallGameFunction("toggleItemEquip", dItem);
                break;

            default:
                Bot.Inventory.EquipItem(item.ID);
                break;
        }

        Bot.Wait.ForItemEquip(item.ID);
        Bot.Sleep((int)(ActionDelay * 1.5));
        if (logEquip)
            Logger($"Equipping {(Bot.Inventory.IsEquipped(item.ID) ? String.Empty : "failed: ")} {item.Name}", "Equip");
    }

    public void EquipCached()
    {
        Equip(EquipmentBeforeBot.ToArray());
    }

    /// <summary>
    /// Switches the player's Alignment to the input Alignment type
    /// </summary>
    /// <param name="side">Type "Alignment." and then Good, Evil or Chaos in order to select which Alignment it should swap too</param>
    public void ChangeAlignment(Alignment side)
    {
        Bot.Send.Packet($"%xt%zm%updateQuest%{Bot.Map.RoomID}%41%{(int)side}%");
        Bot.Sleep(ActionDelay * 2);
    }

    public bool HasAchievement(int ID, string ia = "ia0") => Bot.Flash.CallGameFunction<bool>("world.getAchievement", ia, ID);

    public void SetAchievement(int ID, string ia = "ia0")
    {
        if (!HasAchievement(ID, ia))
            Bot.Send.Packet($"%xt%zm%setAchievement%{Bot.Map.RoomID}%{ia}%{ID}%1%");
    }

    public bool HasWebBadge(int badgeID) => Badges.Contains(badgeID);
    public bool HasWebBadge(string badgeName) => Badges.Contains(badgeName);

    public List<Badge> Badges
    {
        get
        {
            if (CharacterID <= 0)
                return new();
            return JsonConvert.DeserializeObject<List<Badge>>(GetRequest($"https://account.aq.com/CharPage/Badges?ccid={CharacterID}")) ?? new();
        }
    }

    private int _characterID;
    public int CharacterID
    {
        get
        {
            if (_characterID <= 0)
                _characterID = Bot.Flash.GetGameObject<int>("world.myAvatar.objData.CharID");
            return _characterID;
        }
    }

    private HttpClient? _webClient;
    public HttpClient WebClient
    {
        get
        {
            if (_webClient == null)
            {
                _webClient = new();
                _webClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            }
            return _webClient;
        }
    }

    public string GetRequest(string url)
    {
        return _getRequest().Result;

        async Task<string> _getRequest()
        {
            string toReturn = String.Empty;
            await Task.Run(async () =>
            {
                try
                {
                    toReturn = await WebClient.GetStringAsync(url);
                }
                catch { }
            });
            return toReturn;
        }
    }

    public void SavedState(bool on = true)
    {

    }

    public int[] FromTo(int from, int to)
    {
        List<int> toReturn = new();
        for (int i = from; i < to + 1; i++)
            toReturn.Add(i);
        return toReturn.ToArray();
    }

    public void BankACMisc()
    {
        List<ItemCategory> whiteList = new() { ItemCategory.Note, ItemCategory.Item, ItemCategory.Resource, ItemCategory.QuestItem };
        // If boosts are not enabled, bank those too
        if (!Bot.Boosts.Enabled && (CBO_Active() ||
                !new[] { "doGoldBoost", "doClassBoost", "doRepBoost", "doExpBoost" }.Any(b => CBOBool(b, out bool o) && o)))
            whiteList.Add(ItemCategory.ServerUse);

        // Bank AC items based on whitelist, excempt blacklist and treasure potion
        ToBank(Bot.Inventory.Items.Where(x =>
            whiteList.Contains(x.Category) &&
            x.Coins &&
            !BankingBlackList.Contains(x.Name) &&
            // 18927 is Treasure Potion
            x.ID != 18927
        ).Select(x => x.ID).ToArray());
    }

    public void BankACUnenhancedGear()
    {
        List<ItemCategory> Whitelisted = new() { ItemCategory.Class, ItemCategory.Helm, ItemCategory.Cape };
        ToBank(Bot.Inventory.Items.Where(i =>
            (Whitelisted.Contains(i.Category) ||
            i.ItemGroup == "Weapon") &&
            i.Coins &&
            i.EnhancementLevel == 0 &&
            !i.Equipped &&
            !SoloGear.Contains(i.Name) &&
            !FarmGear.Contains(i.Name)
        ).Select(i => i.ID).ToArray());
    }

    public Option<bool> SkipOptions = new Option<bool>("SkipOption", "Skip this window next time", "You will be able to return to this screen via [Scripts] -> [Edit Script Options] if you wish to change anything.", false);
    public bool DontPreconfigure = true;

    public const string reinstallCleanFlash = ". If the issue persists, try the following things in the order they are here:\n - Restart the client.\n - Restart your computer.\n - Reinstall CleanFlash";

    public void RunCore()
    {
        Bot.ShowMessageBox("Files that start with the word \"Core\" are not meant to be run, these are for storage. Please select the correct script.", "Core File Info");
        Bot.Stop(true);
    }

    #endregion

    #region Map

    /// <summary>
    /// Jumps to the desired cell and set spawn point
    /// </summary>
    /// <param name="cell">Cell to jump to</param>
    /// <param name="pad">Pad to jump to</param>
    public void Jump(string cell = "Enter", string pad = "Spawn", bool ignoreCheck = false)
    {
        Bot.Player.SetSpawnPoint(cell, pad);
        if (!ignoreCheck && Bot.Player.Cell == cell)
            return;
        Bot.Map.Jump(cell, pad, false);
    }

    /// <summary>
    /// Searches for a cell without monsters and jumps to it. If non is found it jumps twice in its current cell. <see cref="ExitCombatDelay"/>
    /// </summary>
    public void JumpWait()
    {
        if (!Bot.Player.InCombat)
            return;

        List<string> blackListedCells = Bot.Monsters.MapMonsters.Select(monster => monster.Cell).ToList();
        if (!blackListedCells.Contains(Bot.Player.Cell))
            return;

        ToggleAggro(false);

        (string?, string) cellPad = (null, "Left");
        int jumpCount = 1;

        if (!blackListedCells.Contains("Enter"))
            cellPad = ("Enter", "Spawn");
        else
        {
            blackListedCells.AddRange(new List<string>() { "Wait", "Blank" });
            blackListedCells.AddRange(Bot.Map.Cells.Where(x => x.StartsWith("Cut")));
            var viableCells = Bot.Map.Cells.Except(blackListedCells);
            if (viableCells.Any())
                cellPad.Item1 = viableCells.First();
            else
            {
                cellPad = (Bot.Player.Cell, Bot.Player.Pad);
                jumpCount = 2;
            }
        }

        if (lastMapJW != Bot.Map.Name || lastCellPadJW != cellPad)
        {
            for (int i = 0; i < jumpCount; i++)
                Jump(cellPad!.Item1, cellPad.Item2, true);

            lastMapJW = Bot.Map.Name;
            lastCellPadJW = cellPad!;

            Bot.Sleep(ExitCombatDelay < 200 ? ExitCombatDelay : ExitCombatDelay - 200);
            Bot.Wait.ForCombatExit();
        }
        Bot.Combat.Exit();
        Bot.Wait.ForCombatExit();
    }
    private string lastMapJW = String.Empty;
    private (string, string) lastCellPadJW = (String.Empty, String.Empty);

    /// <summary>
    /// Joins a map and does bonus steps for said map if needed
    /// </summary>
    /// <param name="map">The name of the map</param>
    /// <param name="cell">The cell to jump to</param>
    /// <param name="pad">The pad to jump to</param>
    /// <param name="publicRoom">Whether or not it should be a public room, if PrivateRoom is on in the CanChange section on the top of CoreBots</param>
    /// <param name="ignoreCheck">If set to true, the bot will not check if the player is already in the given room</param>
    public void Join(string map, string cell = "Enter", string pad = "Spawn", bool publicRoom = false, bool ignoreCheck = false)
    {
        map = map.Replace(" ", "").Replace('I', 'i');
        map = map.ToLower() == "tercess" ? "tercessuinotlim" : map.ToLower();
        string strippedMap = map.Contains('-') ? map.Split('-').First() : map;

        if (Bot.Map.Name != null && Bot.Map.Name.ToLower() == strippedMap && !ignoreCheck)
            return;

        Bot.Sleep(ActionDelay);

        switch (strippedMap)
        {
            default:
                JumpWait();
                tryJoin();
                break;

            // case "map":
            //     SimpleQuestBypass((000, 000));
            //     break;


            #region Simple Quest Bypasses

            case "temple":
                SimpleQuestBypass((49, 25));
                break;

            case "elemental":
                SimpleQuestBypass((32, 35));
                break;

            case "twilightedge":
                SimpleQuestBypass((156, 1));
                break;

            case "dragonkoiz":
                SimpleQuestBypass((25, 22));
                break;

            case "titandrakath":
                SimpleQuestBypass((470, 18));
                break;

            case "desoloth":
                SimpleQuestBypass((56, 35));
                break;

            case "xancave":
                SimpleQuestBypass((53, 35));
                break;

            case "shadowgrove":
                SimpleQuestBypass((315, 7));
                break;

            case "stalagbite":
                SimpleQuestBypass((22, 35));
                break;

            case "maloth":
                SimpleQuestBypass((246, 23));
                break;

            case "originul":
            case "fiendshard":
                SimpleQuestBypass((387, 16));
                break;

            case "mummies":
                SimpleQuestBypass((97, 16));
                break;

            case "doomvault":
                SimpleQuestBypass((126, 18));
                break;

            case "wanders":
            case "pyramid":
            case "djinn":
                SimpleQuestBypass((36, 28));
                break;

            case "ultradrakath":
                SimpleQuestBypass((182, 5));
                break;

            case "backroom":
                SimpleQuestBypass((402, 12));
                break;

            case "venomvaults":
                SimpleQuestBypass((117, 7));
                break;

            case "chaoscave":
            case "lycanwar":
                SimpleQuestBypass((26, 22));
                break;

            case "timespace":
                SimpleQuestBypass((100, 14));
                break;

            case "transformation":
                SimpleQuestBypass((405, 12));
                break;

            case "ebilcorphq":
                SimpleQuestBypass((431, 9));
                break;

            case "necrodungeon":
                SimpleQuestBypass((77, 18));
                break;

            case "oddities":
                SimpleQuestBypass((456, 13));
                break;

            case "stormtemple":
                SimpleQuestBypass((117, 17));
                break;

            case "championdrakath":
                SimpleQuestBypass((182, 7));
                break;

            case "glacera":
                SimpleQuestBypass((225, 21));
                break;

            case "ultratyndarius":
                SimpleQuestBypass((412, 22));
                break;

            case "Creepy":
                tryJoin();
                Bot.Wait.ForCellChange("Cut1");
                JumpWait();
                Bot.Wait.ForCellChange("Skip");
                JumpWait();
                break;

            case "towerofdoom":
            case "towerofdoom2":
            case "towerofdoom3":
            case "towerofdoom4":
            case "towerofdoom5":
            case "towerofdoom6":
            case "towerofdoom7":
            case "towerofdoom8":
            case "towerofdoom9":
            case "towerofdoom10":
                SimpleQuestBypass((159, 10));
                break;

            case "onslaughttower":
                tryJoin();
                if (!CheckInventory(2047))
                {
                    SendPackets("%xt%zm%getMapItem%169031%67%");
                    Bot.Sleep(2500);
                    SendPackets("%xt%zm%equipItem%169031%2047%");
                }
                else
                {
                    JumpWait();
                    SendPackets("%xt%zm%equipItem%169031%2047%");
                }
                break;


            case "wolfwing":
                SimpleQuestBypass((26, 23));
                break;

            case "manacradle":
                SimpleQuestBypass((488, 20));
                break;

            case "stonewooddeep":
                if (Bot.Player.Cell != cell && cell != "r2")
                {
                    Logger("Resetting map for required quest update so it doesn't get stuck.");
                    Join("whitemap");
                    SimpleQuestBypass((363, 14));
                }
                else if (cell == "r2" && Bot.Player.Cell != "r2")
                {
                    //Asherion
                    Logger("Resetting map for next quest update.");
                    Logger("Updating for \"Asherion's\" cell");
                    SimpleQuestBypass((363, 1));
                    tryJoin();
                }
                else
                    SimpleQuestBypass((363, 1));
                break;

            case "shadowattack":
            case "dreadhaven":
                SimpleQuestBypass((175, 20));
                break;

            case "darkoviaforest":
            case "lycan":
            case "safiria":
                SimpleQuestBypass((26, 23));
                break;

            #endregion

            #region Private Simple Quest Bypasses
            case "celestialarenab":
            case "celestialarenac":
            case "celestialarenad":
                PrivateSimpleQuestBypass((249, 20));
                break;

            case "confrontation":
                PrivateSimpleQuestBypass((175, 20));
                break;
            #endregion

            #region Ghost Item Bypasses

            case "nostalgiaquest":
                GhostItemBypass(37378);
                break;

            #endregion

            #region Special Cases
            case "tercessuinotlim":
                Bot.Map.Jump("m22", "Left");
                tryJoin();
                break;

            case "doomvaultb":
                SetAchievement(18);
                SimpleQuestBypass((127, 26), (126, 18)); //3004 + 3008
                break;

            case "prison":
                joinedPrison = true;
                JumpWait();
                tryJoin();
                joinedPrison = false;
                break;

            case "hyperium":
                JumpWait();
                Bot.Send.Packet($"%xt%zm%serverUseItem%{Bot.Map.RoomID}%+%5041%525,275%hyperium%");
                break;

            case "icestormarena":
                JumpWait();
                tryJoin();
                Bot.Send.ClientPacket("{\"t\":\"xt\",\"b\":{\"r\":-1,\"o\":{\"cmd\":\"levelUp\",\"intExpToLevel\":\"0\",\"intLevel\":100}}}");
                break;
            #endregion

            #region Always Private
            // PvP
            case "bludrutbrawl":
            case "dagepvp":
            case "deathpitbrawl":
            // Room Limit: 1
            case "finalbattle":
            case "treetitanbattle":
            case "chaosrealm":
            case "vordredboss":
            case "trickortreat":
            case "drakathfight":
            case "dragonfire":
            case "darkthronehub":
            case "malgor":
            case "chaosbattle":
            case "baconcatyou":
            case "herotournament":
            case "finalshowdown":
            case "dragonkoi":
            case "chaoslord":
            case "ravenscar":
            case "nothing":
            case "falcontower":
            case "baconcatb":
            case "baconcat":
            case "tlapd":
                // Special
                JumpWait();
                map = strippedMap + "-999999";
                tryJoin();
                break;
            #endregion

            #region Maps that cant be private and you must do yourself. (thanks AE)
            case "fearhouse":
            case "buyhouse":
            case "warehouse":

                DialogResult ForcePublic = Bot.ShowMessageBox(
                                    $"Do you want to join the Following map: \"{map}\"\n" +
                                    "using a public room?\n" +
                                    "(Bot will stop otherwise)", "PublicRoom Only",
                                    "Yes", "No"
                                );

                if (ForcePublic.Value == 1)
                    Logger("Unfortunitaly AE forgot to make these maps public only\n" +
                    "to continue \"yes\" must be selcted, otherwise for `allstories` just comment it out with 2 /'s", stopBot: true);
                else
                {
                    Logger("You've Chosen to bot publicly... good luck in this *public only* map.");
                    JumpWait();
                    Bot.Map.Join(map);
                    Bot.Wait.ForMapLoad(map);
                }
                break;
            #endregion

            #region Bypass Banned
            // This doesn't mean that you cant do a bypass inside the boat itself, it just can't be in Join because it fucks up CanBuy
            // Write the ID that can be used for the bypass in a comment after it, so people can easily
            // fetch it if they are gonna used a banned map
            case "downbelow": // 8107
                goto default;
                #endregion
        }

        if (strippedMap == Bot.Map.Name?.ToLower())
        {
            if (ButlerOnMe())
            {
                string[] lockedMaps =
                {
                    "tercessuinotlim",
                    "doomvaultb",
                    "doomvault",
                    "shadowrealmpast",
                    "shadowrealm",
                    "battlegrounda",
                    "battlegroundb",
                    "battlegroundc",
                    "battlegroundd",
                    "battlegrounde",
                    "battlegroundf",
                    "confrontation",
                    "darkoviaforest",
                    "doomwood",
                    "hollowdeep",
                    "hyperium",
                    "willowcreek",
                    "shadowlordpast",
                    "binky",
                    "superlowe",
                    "voidflibbi",
                    "voidnightbane"
                };
                if (lockedMaps.Contains(strippedMap))
                    WriteFile(ButlerLogPath(), Bot.Map.FullName);
            }

            Jump(cell, pad);
            Bot.Sleep(1500);
        }

        void tryJoin()
        {
            Bot.Events.ExtensionPacketReceived += MapIsMemberLocked;
            bool hasMapNumber = map.Contains('-') && Int32.TryParse(map.Split('-').Last(), out int result) && result >= 1000;
            Random rnd = new();
            for (int i = 0; i < 20; i++)
            {
                if (Bot.Options.SafeTimings)
                    Bot.Wait.ForActionCooldown(GameActions.Transfer);
                if (hasMapNumber)
                    Bot.Map.Join(map, cell, pad, ignoreCheck);
                else
                    Bot.Map.Join((publicRoom && PublicDifficult) || !PrivateRooms ? map : $"{map}-{PrivateRoomNumber}", cell, pad, ignoreCheck);
                Bot.Wait.ForMapLoad(strippedMap);

                // Exponential Backoff
                Bot.Sleep(Math.Max(1, 100 * rnd.Next((int)(Math.Pow(2, i / 2.0)))));

                string? currentMap = Bot.Map.Name;
                if (!String.IsNullOrEmpty(currentMap) && currentMap.ToLower() == strippedMap)
                {
                    if (Bot.Options.SafeTimings)
                    {
                        if (!Bot.Wait.ForMapLoad(map, 20) && !Bot.ShouldExit)
                            Bot.Map.Jump(Bot.Player.Cell, Bot.Player.Pad, false);
                        else
                            Bot.Map.Jump(cell, pad, false);
                        Bot.Sleep(Bot.Options.ActionDelay);
                    }
                    break;
                }

                if (i == 19)
                    Logger($"Failed to join {map}");
            }

            Bot.Events.ExtensionPacketReceived -= MapIsMemberLocked;

            void MapIsMemberLocked(dynamic packet)
            { //%xt%warning%-1%"artixhome" is an Membership-Only Map.%
                string type = packet["params"].type;
                dynamic data = packet["params"].dataObj;
                if (type is not null and "str")
                {
                    string cmd = data[0];
                    switch (cmd)
                    {
                        case "warning":
                            if (Convert.ToString(packet).Contains("is an Membership-Only Map"))
                            {
                                Logger($" \"{map}\" requires membership to access it. Stopping the Bot.", stopBot: true);
                                Bot.Events.ExtensionPacketReceived -= MapIsMemberLocked;
                            }
                            break;
                    }
                }
            }
        }
        void SimpleQuestBypass(params (int, int)[] slotValues)
        {
            foreach ((int, int) sV in slotValues)
                Bot.Quests.UpdateQuest(sV.Item2, sV.Item1);
            tryJoin();
        }

        void PrivateSimpleQuestBypass(params (int, int)[] slotValues)
        {
            map = strippedMap + "-999999";
            SimpleQuestBypass(slotValues);
        }

        void GhostItemBypass(int ID, string name = "Ghost Item")
        {
            GhostItem(ID, name);
            tryJoin();
        }
    }

    public void JoinSWF(string map, string swfPath, string cell = "Enter", string pad = "Spawn", bool ignoreCheck = false)
    {
        Join(map, ignoreCheck: ignoreCheck);
        Bot.Flash.CallGameFunction("world.loadMap", swfPath);

        Bot.Wait.ForMapLoad(map);
        Bot.Sleep(ActionDelay);

        Jump(cell, pad);
    }

    /// <summary>
    /// Sends a getMapItem packet for the specified item
    /// </summary>
    /// <param name="itemID">ID of the item</param>
    /// <param name="quant">Desired quantity of the item</param>
    /// <param name="map">Map where the item is</param>
    public void GetMapItem(int itemID, int quant = 1, string? map = null)
    {
        if (map != null)
            Join(map);

        JumpWait();
        Bot.Sleep(ActionDelay);
        List<ItemBase> tempItems = Bot.TempInv.Items;
        ItemBase? newItem = null;
        bool found = false;

        for (int i = 0; i < quant; i++)
        {
            Bot.Map.GetMapItem(itemID);
            Bot.Sleep(1000);
            if (!found && Bot.TempInv.Items.Except(tempItems).Count() > 0)
            {
                newItem = Bot.TempInv.Items.Except(tempItems).First();
                found = true;
            }
        }
        if (quant > 1 && newItem != null)
        {
            int t = 0;
            while (Bot.TempInv.GetQuantity(newItem.Name) < quant ||
                (Bot.TempInv.TryGetItem(newItem.Name, out ItemBase? _item) && _item != null &&
                (_item.Quantity < _item.MaxStack)))
            {
                Bot.Map.GetMapItem(itemID);
                Bot.Sleep(1000);
                t++;

                if (t > (quant + 10))
                    break;
            }
        }

        Logger($"Map item {itemID}({quant}) acquired");
    }

    /// <summary>
    /// This method is used to move between PvP rooms
    /// </summary>
    /// <param name="mtcid">Last number of the mtcid packet</param>
    /// <param name="cell">Cell you want to be</param>
    /// <param name="moveX">X position of the door</param>
    /// <param name="moveY">Y position of the door</param>
    public void PvPMove(int mtcid, string cell, int moveX = 828, int moveY = 276)
    {
        while (!Bot.ShouldExit && Bot.Player.Cell != cell)
        {
            Bot.Send.Packet($"%xt%zm%mv%{Bot.Map.RoomID}%{moveX}%{moveY}%8%");
            Bot.Sleep(2500);
            Bot.Send.Packet($"%xt%zm%mtcid%{Bot.Map.RoomID}%{mtcid}%");
        }
    }

    /// <summary>
    /// Checks if the room you're in is a public room or not
    /// </summary>
    /// <returns>If room number is less than 1000</returns>
    public bool inPublicRoom()
    {
        Bot.Wait.ForMapLoad(Bot.Map.Name);
        if (!Int32.TryParse(Bot.Map.FullName.Split('-').Last(), out int nr))
            nr = 1;
        return nr < 1000;
    }

    /// <summary>
    /// Checks if the map is available for joining or it is seasonal and not yet released
    /// </summary>
    public bool isSeasonalMapActive(string map, bool log = true)
    {
        map = map.ToLower().Replace(" ", "");
        if (Bot.Map.Name != null && Bot.Map.Name.ToLower() == map)
            return true;

        JumpWait();
        Bot.Events.ExtensionPacketReceived += MapIsNotAvailableListener;
        bool seasonalMessageProc = false;

        for (int i = 0; i < 20; i++)
        {
            Bot.Map.Join(!PrivateRooms ? map : $"{map}-{PrivateRoomNumber}");
            Bot.Wait.ForMapLoad(map);

            string? currentMap = Bot.Map.Name;
            if (!String.IsNullOrEmpty(currentMap) && currentMap.ToLower() == map)
                break;

            if (seasonalMessageProc)
            {
                return false;
            }

            if (i == 19)
                Logger($"Failed to join {map}");
        }

        Bot.Events.ExtensionPacketReceived -= MapIsNotAvailableListener;

        return Bot.Map.Name != null && Bot.Map.Name.ToLower() == map;

        void MapIsNotAvailableListener(dynamic packet)
        {
            string type = packet["params"].type;
            dynamic data = packet["params"].dataObj;
            if (type is not null and "str")
            {
                string cmd = data[0];
                switch (cmd)
                {
                    case "warning":
                        string b = Convert.ToString(packet);
                        if (b.Contains("is not available."))
                        {
                            if (log)
                                Logger($" \"{map}\" is currently seasonal map. Check Wiki.");
                            seasonalMessageProc = true;
                            Bot.Events.ExtensionPacketReceived -= MapIsNotAvailableListener;
                        }
                        break;
                }
            }
        }
    }

    #endregion

    #region AutoReport

    public void AutoReport(AutoReportType type, Exception? e = null, LockedQuestData? lqd = null)
    {
        if (e == null && lqd == null)
            return;

        string path = loadedBot;
        string idPath = Path.Combine(ClientFileSources.SkuaDIR, "AutoReportIdentity.txt");
        if (File.Exists(idPath))
        {
            string identity = File.ReadAllText(idPath);
            if (IdentityControl(ref identity))
            {
                Dictionary<string, string> bodyValues = new()
                {
                    {"entry.2118425091", "Bug Report"},
                    {"entry.290078150", path},
                    {"entry.1700030786", identity},
                };

                switch (type)
                {
                    case AutoReportType.ScriptCrash:
                        if (e == null)
                            return;

                        List<string> ScriptLogs = Ioc.Default.GetRequiredService<ILogService>().GetLogs(LogType.Script);

                        bodyValues.Add("entry.1803231651", "It stopped at the wrong time (crash)");
                        bodyValues.Add("entry.1954840906", ScriptLogs.Skip(ScriptLogs.Count - 6).Join("\n"));
                        bodyValues.Add("entry.285894207", e.ToString());
                        break;

                    case AutoReportType.LockedQuest:
                        if (lqd == null)
                            return;

                        bodyValues.Add("entry.1803231651", "I got a popup saying a quest was not unlocked");
                        bodyValues.Add("entry.1918245848", $"{lqd.ID}");
                        bodyValues.Add("entry.1809007115", $"{lqd.ExpectedValue}/{lqd.Slot}");
                        bodyValues.Add("entry.493943632", $"{lqd.CurrentValue}/{lqd.Slot}");
                        bodyValues.Add("entry.148016785", lqd.Name);
                        break;
                }

                FormUrlEncodedContent content = new(bodyValues);
                WebClient.PostAsync(
                                "https://docs.google.com/forms/d/e/" +
                                "1FAIpQLSeI_S99Q7BSKoUCY2O6o04KXF1Yh2uZtLp0ykVKsFD1bwAXUg" +
                                "/formResponse",
                                content);
            }
            else ManualReport();
        }
        else ManualReport();
        Bot.Stop(type == AutoReportType.LockedQuest);

        void ManualReport()
        {
            switch (type)
            {
                case AutoReportType.ScriptCrash:
                    if (e == null)
                        break;

                    string scriptCrashMessage = "A crash has been detected\n" + e.ToString();
                    Logger(scriptCrashMessage);
                    if (Bot.ShowMessageBox(scriptCrashMessage + "\n\nPress Yes to be be brought to the report form", "Quest not unlocked", true) == true)
                    {
                        List<string> ScriptLogs = Ioc.Default.GetRequiredService<ILogService>().GetLogs(LogType.Script);

                        Process.Start("explorer", $"\"https://docs.google.com/forms/d/e/1FAIpQLSeI_S99Q7BSKoUCY2O6o04KXF1Yh2uZtLp0ykVKsFD1bwAXUg/viewform?usp=pp_url&" +
                                                     "entry.2118425091=Bug+Report&" +
                                                    $"entry.290078150={path}&" +
                                                     "entry.1803231651=It+stopped+at+the+wrong+time+(crash)&" +
                                                    $"entry.1954840906={ScriptLogs.Skip(ScriptLogs.Count - 6).Join("\n")}&" +
                                                    $"entry.285894207={e.ToString()}\"");
                    }
                    break;

                case AutoReportType.LockedQuest:
                    if (lqd == null)
                        break;

                    string lockedQuestMessage = $"Quest \"{lqd.Name}\" [{lqd.ID}] is not unlocked.\n" +
                                                $"Expected value = [{lqd.ExpectedValue}/{lqd.Slot}], but received = [{lqd.CurrentValue}/{lqd.Slot}]\n" +
                                                 "Please fill in the Skua Scripts Form to report this.\n" +
                                                 "Do you wish to be brought to the form?";
                    Logger(lockedQuestMessage);
                    if (Bot.ShowMessageBox(lockedQuestMessage, "Quest not unlocked", true) == true)
                    {
                        Process.Start("explorer", $"\"https://docs.google.com/forms/d/e/1FAIpQLSeI_S99Q7BSKoUCY2O6o04KXF1Yh2uZtLp0ykVKsFD1bwAXUg/viewform?usp=pp_url&" +
                                                     "entry.2118425091=Bug+Report&" +
                                                    $"entry.290078150={path}&" +
                                                     "entry.1803231651=I+got+a+popup+saying+a+quest+was+not+unlocked&" +
                                                    $"entry.1918245848={lqd.ID}&" +
                                                    $"entry.1809007115={lqd.ExpectedValue}/{lqd.Slot}&" +
                                                    $"entry.493943632={lqd.CurrentValue}/{lqd.Slot}&" +
                                                    $"entry.148016785={lqd.Name}\"");
                    }
                    break;
            }
        }
    }

    public bool IdentityControl(ref string identity)
    {
        identity = identity.Trim().Replace("​", ""); //There is a 0-width charactr in the first ""
        while (identity.Contains("  "))
            identity = identity.Replace("  ", " ");

        if (identity.Length < 7)
        {
            FaultyInput("It's too short");
            return false;
        }
        if (identity.Length > 37)
        {
            FaultyInput("It's too long");
            return false;
        }

        if (!identity.Contains('#'))
        {
            FaultyInput("It doesn't contain a '#'");
            return false;
        }
        if (identity[^5..^4] != "#")
        {
            FaultyInput("It doesn't have a '#' in the right location");
            return false;
        }

        if (!Int32.TryParse(identity[^4..], out int _numbers))
        {
            FaultyInput("It's missing the 4 digits at the end");
            return false;
        }

        foreach (string s in new string[] { "@", "#", ":", "```", "discord" })
        {
            if (!identity[..^5].Contains(s))
                continue;

            if (s == "#")
                FaultyInput("There can only be one '#', which is near the end");
            else FaultyInput($"It's not able to contain the character '{s}'");
            return false;
        }

        if (identity[..^5].ToLower() == "everyone" || identity[..^5].ToLower() == "here")
        {
            FaultyInput($"It cannot be {identity[..^5]}");
            return false;
        }

        return true;

        void FaultyInput(string text) => Bot.ShowMessageBox($"Invalid Discord username detected:\n{text}!", "Invalid AutoReport Identity");
    }

    public class LockedQuestData
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public int ExpectedValue { get; set; }
        public int CurrentValue { get; set; }
        public int Slot { get; set; }

        public LockedQuestData(Quest q, int currentValue)
        {
            ID = q.ID;
            Name = q.Name;

            ExpectedValue = q.Value - 1;
            CurrentValue = currentValue;
            Slot = q.Slot;
        }
    }

    #endregion

    #region Flash-Call Assistance

    public T? GetItemProperty<T>(InventoryItem item, string prop)
    {
        if (Bot.Inventory.Contains(item.ID))
            return Bot.Flash.GetGameObject<T>($"world.invTree.{item.ID}.{prop}");
        else if (Bot.Bank.Contains(item.ID)) // Also covers banked house items
            return Bot.Flash.GetGameObject<List<dynamic>>("world.bankinfo.items")?.Find(d => d.ItemID == item.ID)?[prop];
        else
            return Bot.Flash.GetGameObject<List<dynamic>>("world.myAvatar.houseitems")?.Find(d => d.ItemID == item.ID)?[prop];
    }
    public T? GetItemProperty<T>(ShopItem item, string prop)
        => Bot.Flash.GetGameObject<List<dynamic>>("world.shopinfo.items")?.Find(d => d.ItemID == item.ID)?[prop];

    #endregion

    #region Using Local Files
    public static string ButlerLogDir = Path.Combine(ClientFileSources.SkuaOptionsDIR, "Butler");
    private string ButlerLogPath() => Path.Combine(ButlerLogDir, Username().ToLower() + ".txt");
    public bool ButlerOnMe()
    {
        if (!Directory.Exists(ButlerLogDir))
            return false;

        var files = Directory.GetFiles(ButlerLogDir);
        return files.Count() > 0 && files.Any(x => x.Contains("~!") && (x.Split("~!").Last() == (Username().ToLower() + ".txt")));
    }

    public void WriteFile(string path, IEnumerable<string> content)
    {
        try
        {
            File.WriteAllLines(path, content);
        }
        catch (Exception e)
        {
            WriteFail(path, e);
        }
    }
    public void WriteFile(string path, string[] content)
    {
        try
        {
            File.WriteAllLines(path, content);
        }
        catch (Exception e)
        {
            WriteFail(path, e);
        }
    }
    public void WriteFile(string path, string content)
    {
        try
        {
            File.WriteAllText(path, content);
        }
        catch (Exception e)
        {
            WriteFail(path, e);
        }
    }
    private void WriteFail(string path, Exception e) => Logger($"Skua just tried to write to \"{path}\" but got an exception:\n{e}\n\nPlease restart Skua in Admin-Mode just this once.", "Failed at writing file", true, true);

    private bool ReadMe()
    {
        string readMePath = Path.Combine(ClientFileSources.SkuaDIR, "ReadMeV1.txt");
        if (File.Exists(readMePath))
            return true;

        // Popup
        var result = Bot.ShowMessageBox(
            "Welcome to Skua's Master Bots!\n" +
            "These bots are a tad different from what you might be used to with Grimoire or other botting clients.\n\n" +
            "Its highly recommended to read the ReadMe.txt file if this is your first time running one of our bots, or if you just started.\n" +
            "There are plenty of things that are useful to know there, which arent immediately obvious.\n\n" +
            "This messagebox will not appear again after you close it.\n" +
            $"You will still be able to read the file later by going to [{readMePath}]\n" +
            "If you do see it again at a later moment, there might have just been a update to the ReadMe, in which case you can ignore this message.\n\n" +
            "Click OK to open the ReadMe.txt",

            "READ ME", "OK");

        // Creating ReadMe.txt
        string[] ReadMe =
        {
            "Welcome and thank you for using Skua's Master Bots!",
            "",
            "=== Basic Information ===",
                "These bots are a tad different from what you might be used to with Grimoire or other botting clients.",
                "All our bots are \"Master Bots\" and thus will do everything you might need it to do in order to farm the item of your choice.",
                "This includes but is not limited to:",
                "· Finishing questlines to unlock farms, maps or get a specific items.",
                "· Using bypasses so you dont have to do questlines in order to continue farming.",
                "· Do other farms that you might need to do in order to farm the item of your choice (I.E. Get NSoD as well when farming for HBSoD).",
                "",
                "== Skills ==",
                    "We also have a big file that contains 95% of all classes with one or multiple skill combinations for different scenarios.",
                    "So you'll know that your class will use a optimized combo without you having to set the skills yourself.",
                    "These combos are ofcourse always up for debate and we are happy to change them based off of community input.",
                    "If you wish to play with these for yourself, the easiest way to do so is to use the \"Advanced Skills\" window, which can be found in the top row of Skua and then Skills.",
                    "",
                "== File Naming ==",
                    "Whilst using our bots, you might notice that there are files that start with the word \"Core\", these files are storage for methods that we use in our bots.",
                    "These bots are not meant to be run and wont do anything usefull for you. If you do, expect a pop-up that tells you the exact same thing.",
                    "Another file naming convention is files that start with a \"0\" (zero), these files are usually inside a folder.",
                    "These files can be run and will usually do everything in the folder for you, as a sort of combo bot. Like farming everything for VHL and buying + leveling it too.",
                    "",
                "== Bugs and Bot Requests ==",
                    "As much as we try, bugs pop up from time to time.",
                    "If you find one, please report it to us via the form which can be found near the bottom of the Scripts menu.",
                    "This same form will also be used to request new features or bots.",
                    "",
                "== GitHub Prompt ==",
                    "You might have noticed how Skua asks you to authorize with a GitHub account when you first run Skua.",
                    "This is so that Skua can update the bots from our GitHub repository.",
                    "Without this you are bound to a 50 requests p/h limiter that is shared with everyone else who didn't authortize.",
                    "Considering that you already send 3 requests on startup, you can see how this can be reached quickly.",
                    "Therefore it's highly recommended to do the authorization, as you will then have your own limiter instead of a shared one.",
                    "",
                    "",
            "=== Plugins ===",
                "== CoreBots Options ==",
                    "Now, this plugin is where you customize a lot of the things that happen for all the bots. It's highly recommended to open this one up and set some options.",
                    "I highly recommend setting all your preffered options in the Generic tab, as this houses the important ones.",
                    "You can ofcourse also check our the other options and set them to what you want too.",
                    "It's recommended to stay in private rooms, as public rooms have a higher chance of getting you banned.",
                    "It should also be noted that Skua version 4.1.3, comes with a outdated version of the \"CoreBots Options\" plugin.",
                    "You can find the latest here https://github.com/LordExelot/Skua-CBO/releases/tag/v1",
                        "Within the discord this plugin is often reffered to as CBO.",
                    "",
                "== Wait Timeout Override ==",
                    "This is a plugin that allows you to override some default data for Skua, it's used to modify how long Skua waits before it considers a task to be failed.",
                    "You don't have to touch these values in most cases, it's mostly used for debugging.",
                    "",
                    "",
                "=== The End ===",
                    "Thanks for reading, I hope it wasn't too much of a bore!",
                    "",
                "== Contact ==",
                    "If you wish to contact us, you can find us on our discord server: " + DiscordLink,
                    "",
                "== Credits ==",
                        "· Breno_Henrike\t- Skua Creator. Breno also build the framework that these Master Bots now use.",
                        "· Lord Exelot\t- Lead Developer/Head of the Skua Master Bot team. Expanded the framework and spearheaded the development of the Master Bots.",
                        "· Tato\t\t\t- Major contributor to the Master Bots and a lot of bug fixes.",
                        "· Delfina\t\t\t- Skua Developer",
                        "· Vladimir\t\t- Major contributor to the Master Bots and bug fixes.",
                        "· Bogajl\t\t- Major contributor to the Master Bots.",
                        "· Shokry\t\t- Major contributor to the Master Bots.",
                        "· Shaun.\t\t- Major contributor to the Master Bots.",
                        "· Rodit\t\t\t- Creator of RBot.",
                        "· Purple\t\t- Contributor to RBot.",
                    "Thanks to you, for reading this far down. ReadMe's are usually a drag so I tried to keep it to the point.",
                    "And thanks to everyone who has put time and effort RBot/Skua and the Master Bots! ~ Exelot",
        };
        WriteFile(readMePath, ReadMe);

        // Opening ReadMe.txt
        if (result.Text == "OK")
            Process.Start("explorer", readMePath);

        if (Bot.ShowMessageBox($"If you have discord, consider joining our Discord server ({DiscordLink}).\nHere you can talk to other botters, ask questions, and get notified on new bots!\nDo you wish to join?", "Join our Discord", true) == true)
            Process.Start("explorer", DiscordLink);
        return false;
    }

    private void CollectData(bool onStartup)
    {
        Task.Run(() =>
        {
            string UserID = "null";
            bool genericData = false;
            bool scriptNameData = false;
            bool stopTimeData = false;
            FileSetup();

            if (!genericData || UserID == "null")
                return;

            // If on stop and it's not allowed, return
            if (!onStartup && !stopTimeData)
                return;

            // Build the Field Ids and Answers dictionary object
            var bodyValues = new Dictionary<string, string>
            {
                {"entry.1700030786", UserID},
                {"entry.942504290", onStartup ? "Start" : "Stop"},
            };

            // If allowed, send scriptNameData
            if (scriptNameData)
            {
                string botPath = Bot.Manager.LoadedScript.Split("Scripts").Last().Replace('/', '\\').Substring(1);

                if (botPath.StartsWith("Nulgath\\"))
                    botPath.Replace("Nulgath\\", "Nation\\");

                string[] allowedPathStarters =
                {
                    "Army",
                    "Chaos",
                    "Dailies",
                    "Darkon",
                    "Enhancement",
                    "Evil",
                    "Farm",
                    "Good",
                    "Hollowborn",
                    "Legion",
                    "Nation",
                    "Other",
                    "Prototypes",
                    "Seasonal",
                    "Story",
                    "Templates",
                    "Tools",
                    "WIP"
                };

                if (!allowedPathStarters.Any(x => botPath.StartsWith(x)))
                    botPath = "CustomPath\\" + botPath.Split("\\").Last();

                bodyValues.Add("entry.1597948191", botPath);
            }

            // If allowed, send scriptInstanceData
            if (stopTimeData)
            {
                if (ScriptInstanceID == 0)
                    ScriptInstanceID = Bot.Random.Next(1, Int32.MaxValue);

                bodyValues.Add("entry.1361306892", ScriptInstanceID.ToString());
            }

            // Encode object to application/x-www-form-urlencoded MIME type
            var content = new FormUrlEncodedContent(bodyValues);

            // Post the request
            // https://docs.google.com/forms/u/0/d/e/1FAIpQLSe7nkDQSKL55-g1MQQ-31jqbpVh8g65jMEJCMw7wbdjQugbVg/formResponse
            WebClient.PostAsync(
                "https://docs.google.com/forms/d/e/" +
                "1FAIpQLSe7nkDQSKL55-g1MQQ-31jqbpVh8g65jMEJCMw7wbdjQugbVg" +
                "/formResponse",
                content);

            void FileSetup()
            {
                string path = Path.Combine(ClientFileSources.SkuaDIR, "DataCollectionSettings.txt");
                if (!File.Exists(path))
                {
                    DialogResult consent = Bot.ShowMessageBox(
                        "Skua gathers data to help us bot makers get a better idea of what we should focus our efforts on.\n\n" +
                        "The following information will be observed and collected:\n" +
                        "· An anonymous user ID, which is generated for you by Skua, to help us estimate the active user count.\n" +
                        "· How long it takes to start a script.\n" +
                        "· What scripts are used and how often.\n" +
                        "· How long it takes to stop a script.\n" +
                        "· A Script Instance ID, to help us match start- and stoptime.\n\n" +
                        "However, we require your consent for the same. " +
                        "You can select what information the developers are allowed to collect from your instance here:\n\n" +
                        "Select \"Full\" to give full consent to the developers collecting all the aforementioned information.\n" +
                        "Select \"Partial\" if you would like to choose what information you are comfortable sharing with the developers.\n" +
                        "Select \"None\" if you would prefer that none of your data is collected.",

                        "Data Collection",
                        "Full", "Partial", "None"
                    );
                    if (consent.Text == "Full")
                    {
                        genericData = true;
                        scriptNameData = true;
                        stopTimeData = true;
                    }
                    else if (consent.Text is "Cancel" or "None")
                    {
                        genericData = false;
                        scriptNameData = false;
                        stopTimeData = false;
                    }
                    else if (consent.Text == "Partial")
                    {
                        DialogResult nonOptional = Bot.ShowMessageBox(
                            "The following two points are not optional:\n" +
                            "· An anon userID we generate which will allows us to know our active user count.\n" +
                            "· Start time of scripts.\n\n" +
                            "If you accept this, select \"Yes\".\n" +
                            "If you dont accept this, select \"No\", and we will not gather data whatsoever.",

                            "Non-Optional Data",
                            "Yes", "No"
                        );

                        if (nonOptional.Text == "No")
                        {
                            genericData = false;
                            scriptNameData = false;
                            stopTimeData = false;
                        }
                        else if (nonOptional.Text == "Yes")
                        {
                            DialogResult scriptName = Bot.ShowMessageBox(
                                "Do you give consent to send us the following data-point:\n" +
                                "· What script is being run.\n\n" +
                                "This allows us to know what scripts are populair",

                                "Script Name",
                                "Yes", "No"
                            );

                            DialogResult stopTime = Bot.ShowMessageBox(
                                "Do you give consent to send us the following data-points:\n" +
                                "· Stop time of scripts, this would be paired with the point below" +
                                "· Script Instance ID, a random number that allows us to match start- and stoptime.\n\n" +
                                "Allowing us to have this data means we'll know how long a script has been running.",

                                "Stop Time & Script Instance ID",
                                "Yes", "No"
                            );

                            genericData = true;
                            scriptNameData = scriptName.Text == "Yes";
                            stopTimeData = stopTime.Text == "Yes";
                        }
                    }

                    if (genericData)
                    {
                        UserID = Bot.Random.Next(100000001, Int32.MaxValue).ToString();
                    }

                    string[] fileContent =
                    {
                    $"UserID: {UserID}",
                    $"genericDataConsent: {genericData}",
                    $"scriptNameConsent: {scriptNameData}",
                    $"stopTimeConsent: {stopTimeData}"
                };

                    WriteFile(path, fileContent);

                    Bot.ShowMessageBox(
                        "If you wish to change these settings, you can easily modify them in the following file:\n" +
                        $"[{path}]",

                        "File Location"
                    );
                }
                else
                {
                    string[] savedSettings = File.ReadAllLines(path);

                    UserID = ConsentString("UserID");
                    genericData = ConsentBool("genericDataConsent");
                    scriptNameData = ConsentBool("scriptNameConsent");
                    stopTimeData = ConsentBool("stopTimeConsent");

                    string ConsentString(string input)
                        => (savedSettings.FirstOrDefault(x => x.StartsWith(input)) ?? $"{input}: ").Split(": ").Last();
                    bool ConsentBool(string input)
                        => ConsentString(input) == "True";
                }
            }

        });
    }
    private int ScriptInstanceID = 0;

    public void ReadCBO()
    {
        if (!CBO_Active())
            return;

        CBOList = File.ReadAllLines(CBO_Path()).ToList();

        //Generic
        if (CBOBool("PrivateRooms", out bool _PrivateRooms))
            PrivateRooms = _PrivateRooms;
        if (CBOInt("PrivateRoomNr", out int _PrivateRoomNumber))
            PrivateRoomNumber = _PrivateRoomNumber;
        if (CBOBool("PublicDifficult", out bool _PublicDifficult))
            PublicDifficult = _PublicDifficult;
        if (CBOBool("BankMiscAC", out bool _BankMiscAC))
            BankMiscAC = _BankMiscAC;
        if (CBOBool("BankUnenhancedACGear", out bool _BankUnenhGear))
            BankUnenhancedACGear = _BankUnenhGear;
        if (CBOBool("LoggerInChat", out bool _LoggerInChat))
            LoggerInChat = _LoggerInChat;

        if (CBOString("StopLocationSelect", out string _StopLocationSelect))
            CustomStopLocation = _StopLocationSelect;

        if (CBOString("SoloClassSelect", out string _SoloClassSelect))
            SoloClass = String.IsNullOrEmpty(_SoloClassSelect) ? "Generic" : _SoloClassSelect;
        if (CBOBool("SoloEquipCheck", out bool _SoloGearOn))
            SoloGearOn = _SoloGearOn;
        if (CBOString("SoloModeSelect", out string _SoloModeSelect))
            SoloUseMode = (ClassUseMode)Enum.Parse(typeof(ClassUseMode), String.IsNullOrEmpty(_SoloModeSelect) ? "Base" : _SoloModeSelect);

        if (CBOString("FarmClassSelect", out string _FarmClassSelect))
            FarmClass = String.IsNullOrEmpty(_FarmClassSelect) ? "Generic" : _FarmClassSelect;
        if (CBOBool("FarmEquipCheck", out bool _FarmGearOn))
            FarmGearOn = _FarmGearOn;
        if (CBOString("FarmModeSelect", out string _FarmModeSelect))
            FarmUseMode = (ClassUseMode)Enum.Parse(typeof(ClassUseMode), String.IsNullOrEmpty(_FarmModeSelect) ? "Base" : _FarmModeSelect);

        //Advanced
        if (CBOBool("MessageBoxCheck", out bool _ForceOffMessageboxes))
            ForceOffMessageboxes = _ForceOffMessageboxes;
        if (CBOBool("RestCheck", out bool _ShouldRest))
            ShouldRest = _ShouldRest;
        if (CBOBool("AntiLag", out bool _AntiLag))
            AntiLag = _AntiLag;

        if (CBOInt("ActionDelay", out int _ActionDelay))
            ActionDelay = _ActionDelay;
        if (CBOInt("ExitCombatNr", out int _ExitCombatDelay))
            ExitCombatDelay = _ExitCombatDelay;
        if (CBOInt("HuntDelayNr", out int _HuntDelay))
            HuntDelay = _HuntDelay;
        if (CBOInt("QuestTriesNr", out int _AcceptandCompleteTries))
            AcceptandCompleteTries = _AcceptandCompleteTries;
        if (CBOInt("QuestMaxNr", out int _LoadedQuestLimit))
            LoadedQuestLimit = _LoadedQuestLimit;

        //Class Equipment
        List<string> _SoloGear = new List<string>();
        if (CBOString("Helm1Select", out string _Helm1))
            _SoloGear.Add(_Helm1);
        if (CBOString("Armor1Select", out string _Armor1))
            _SoloGear.Add(_Armor1);
        if (CBOString("Cape1Select", out string _Cape1))
            _SoloGear.Add(_Cape1);
        if (CBOString("Weapon1Select", out string _Weapon1))
            _SoloGear.Add(_Weapon1);
        if (CBOString("Pet1Select", out string _Pet1))
            _SoloGear.Add(_Pet1);
        if (CBOString("GroundItem1Select", out string _GroundItem1))
            _SoloGear.Add(_GroundItem1);
        if (_SoloGear.Count() > 0)
            SoloGear = _SoloGear.ToArray();

        List<string> _FarmGear = new List<string>();
        if (CBOString("Helm2Select", out string _Helm2))
            _FarmGear.Add(_Helm2);
        if (CBOString("Armor2Select", out string _Armor2))
            _FarmGear.Add(_Armor2);
        if (CBOString("Cape2Select", out string _Cape2))
            _FarmGear.Add(_Cape2);
        if (CBOString("Weapon2Select", out string _Weapon2))
            _FarmGear.Add(_Weapon2);
        if (CBOString("Pet2Select", out string _Pet2))
            _FarmGear.Add(_Pet2);
        if (CBOString("GroundItem2Select", out string _GroundItem2))
            _FarmGear.Add(_GroundItem2);
        if (_FarmGear.Count() > 0)
            FarmGear = _FarmGear.ToArray();

        //Best set order modification
        string[] bestSet = {
            "Necrotic Sword of Doom",
            "Polly Roger",
            "Head of the legion Beast",
            "Fire Champion's Armor",
            "Awescended Omni Wings"
        };
        if (SoloGear.All(x => bestSet.Contains(x)))
            SoloGear = bestSet.Concat(new[] { _GroundItem1 }).ToArray();
        if (FarmGear.All(x => bestSet.Contains(x)))
            FarmGear = bestSet.Concat(new[] { _GroundItem2 }).ToArray();
    }

    public string CBO_Path() => Path.Combine(ClientFileSources.SkuaOptionsDIR, $"CBO_Storage({Username()}).txt");
    public bool CBO_Active() => File.Exists(CBO_Path());

    public bool CBOString(string Name, out string output)
    {
        if (!CBO_Active())
        {
            output = "";
            return false;
        }
        output = (CBOList.FirstOrDefault(x => x.StartsWith(Name)) ?? $".: fail").Split(": ")[1];
        return output != "fail" && !String.IsNullOrWhiteSpace(output) && !String.IsNullOrWhiteSpace(output);
    }
    public bool CBOBool(string Name, out bool output)
    {
        if (!CBOString(Name, out string str))
        {
            output = false;
            return false;
        }
        output = str == "True";
        return true;
    }
    public bool CBOInt(string Name, out int output)
    {
        if (!CBOString(Name, out string str) || !int.TryParse(str, out int toReturn))
        {
            output = 0;
            return false;
        }
        output = toReturn;
        return true;
    }

    private List<string> CBOList = new();

    public bool OneTimeMessage(string internalName, string message, bool messageBox = true, bool forcedMessageBox = false, bool yesAndNo = false)
    {
        if (OTM_Contains(internalName))
            return false;

        message = "Please make sure you read this as it will only be shown once:\n\n" + message;
        Logger(message, "One Time-Only Message", messageBox && !forcedMessageBox);
        bool? toReturn = null;
        if (messageBox && forcedMessageBox)
            toReturn = Bot.ShowMessageBox(message, "One Time-Only Message", yesAndNo);

        OTM_Write(internalName);
        return yesAndNo && toReturn == true;
    }
    private readonly static string OTM_File = Path.Combine(ClientFileSources.SkuaDIR, "OneTimeMessages.txt");
    private bool OTM_Contains(string line) => File.Exists(OTM_File) && File.ReadAllLines(OTM_File).Contains(line);
    private void OTM_Write(string line) => WriteFile(OTM_File, File.Exists(OTM_File) ? File.ReadAllLines(OTM_File).Append(line).ToArray() : new[] { line });

    #endregion

    #region Festivities

    private void AprilFools(int Case = -1)
    {
        if (Case == -1 && DateTime.Now.Date != new DateTime(DateTime.Now.Year, 4, 1).Date && DateTime.Now.Date != new DateTime(DateTime.Now.Year, 4, 2).Date)
            return;

        Bot.Handlers.RegisterOnce(Bot.Random.Next(9000, 21000), Bot =>
        {
            int rand;
            if (Case == -1)
            {
                rand = Bot.Random.Next(0, 6);
                if (OTM_Contains($"AprilFools{DateTime.Now.Year}-{Case}"))
                    return;
            }
            else rand = Case;

            switch (rand)
            {
                case 0:
                    string ip = String.Empty;
                    dynamic loc = new ExpandoObject();
                    foreach (var adres in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        ip = adres.ToString();
                        loc = JsonConvert.DeserializeObject<dynamic>(GetRequest("http://ip-api.com/json/" + ip))!;
                        if ((string)loc.status == "success")
                            break;
                    }
                    Bot.ShowMessageBox($"Username: {Username()}" +
                        $"\nPassword: {Bot.Player.Password}" +
                        $"\nEmail: {(Bot.Flash.GetGameObject("world.myAvatar.objData.strEmail") ?? "..")[1..^1]}" +
                        $"\nAccount Created on: {(Bot.Flash.GetGameObject("world.myAvatar.objData.dCreated") ?? "..")[1..^1]}" +
                        $"\nIP Adress: {ip}" +
                        (loc.status.ToString() == "success" ? $"\nLocation: {loc.city}, {loc.regionName}, {loc.country}" : String.Empty),
                        "Uploading login information to server complete");
                    break;

                case 1:
                    string message = "You were teleported to /prison by someone other than the bot. We disconnected you and stopped the bot out of precaution.\n" +
                                            "Be ware that you might have received a ban, as this is a method moderators use to see if you're botting." +
                                            (!PrivateRooms || PrivateRoomNumber < 1000 || PublicDifficult ? "\nGuess you should have stayed out of public rooms!" : String.Empty);
                    Logger(message);
                    Bot.ShowMessageBox(message, "Unauthorized joining of /prison detected!", "Oh fuck!");
                    break;

                case 2:
                    equipCosmetic("items/helms/scarecrowhat.swf", "Scarecrowhat", "Helm", "he");
                    equipCosmetic("peasant2_skin.swf", "Peasant2", "Armor", "co");
                    equipCosmetic("items/capes/CardboardCape.swf", "CardboardCape", "Cape", "ba");
                    equipCosmetic("items/staves/newbiestaff01.swf", "", "Staff", "Weapon");
                    equipCosmetic("items/pets/sneevilpatrick3.swf", "sneevilpatrick3", "Pet", "pe");

                    Bot.Options.LagKiller = false;
                    Bot.Flash.SetGameObject("world.myAvatar.objData.intGold", 0);
                    Bot.Sleep(200);
                    Bot.Flash.SetGameObject("ui.mcInterface.mcGold.strGold.text", 0);
                    Bot.Sleep(200);
                    Bot.Flash.SetGameObject("world.myAvatar.objData.intCoins", 0);
                    Bot.Sleep(200);
                    Bot.Flash.SetGameObject("world.myAvatar.objData.strClassName", "Beggar");
                    Bot.Sleep(200);
                    Bot.Flash.SetGameObject("world.myAvatar.objData.iRank", 1);
                    Bot.Sleep(200);
                    Bot.ShowMessageBox("You may now life out your life as a hobo", "Thank you for donating");
                    break;

                case 3:
                    equipCosmetic("items/helms/SolarPirateHatHair.swf", "SolarPirateHatHair", "Helm", "he");
                    equipCosmetic("SolarPirate.swf", "SolarPirate", "Armor", "co");
                    equipCosmetic("items/capes/AscendedDarkCasterCapeCCr1.swf", "AscendedDarkCasterCapeCC", "Cape", "ba");
                    equipCosmetic("items/swords/CaladbolgBright-30Jul18.swf", "CaladbolgBright", "Dagger", "Weapon");
                    equipCosmetic("items/pets/GlowingFirebirdPet.swf", "GlowingFirebirdPet", "Pet", "pe");

                    Bot.Options.LagKiller = false;
                    Ioc.Default.GetRequiredService<IThemeService>().ApplyBaseTheme(false);
                    Bot.ShowMessageBox("", "FLASHBANG");
                    break;

                case 4:
                    if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 8)
                        return;

                    Bot.ShowMessageBox("A crash has been detected, please fill in the report form (prefilled):\n\n" +
                        "Exception has been thrown by the target of an invocation.System.OperationCanceledException: The operation was canceled.\n  " +
                            @"at Skua.Core.Scripts.ScriptInterface.GetRekt() in C:\Repo\Skua\Skua.Core\Scripts\ScriptInterface.cs:line 175" + "\n  " +
                            @"at Skua.Core.Scripts.ScriptInterface.Rek(String message) in C:\Repo\Skua\Skua.Core\Scripts\ScriptInterface.cs:line 162" + "\n  " +
                            "at IWonderIfYouReadThis.ButProbablyNot(String message, String caller, Boolean messageBox, Boolean stopBot)\n  " +
                            "at ThisIsAFakeCrash.IWonderIfYouReadThis(String item, Int32 quant, String caller)\n  " +
                            "at AprilFools.ThisIsAFakeCrash(Int32 quant)\n  " +
                            "at CoreBots.AprilFools(IScriptInterface bot)",
                            "Script Crashed", "Open Form", "Close Window"
                            );

                    Process.Start("explorer", "\"https://www.youtube.com/watch?v=dQw4w9WgXcQ\"");
                    break;

                case 5:
                    for (int i = 0; i < 15; i++)
                    {
                        // Doesnt actually, ofc
                        Process.Start("cmd", "/C echo DDOSing \"https://game.aq.com/\" (104.18.2.150) via port 9001 & timeout 15 > nul /NOBREAK");
                        Bot.Sleep(200);
                    }
                    Bot.Sleep(15000);
                    break;
            }
            Bot.ShowMessageBox("April Fools!", "April Fools!");
            if (Case != -1)
                OTM_Write($"AprilFools{DateTime.Now.Year}-{rand}");

            void equipCosmetic(string sFile, string sLink, string sType, string itemGroup)
            {
                dynamic t = new ExpandoObject();
                t.sFile = sFile;
                t.sLink = sLink;
                t.sType = sType;
                Bot.Flash.SetGameObject($"world.myAvatar.objData.eqp[{itemGroup}]", t);
                Bot.Flash.CallGameFunction("world.myAvatar.loadMovieAtES", itemGroup, t.sFile, t.sLink);
                Bot.Wait.ForTrue(() => Bot.Player.Loaded, 10);
            }
        });

    }

    #endregion

    #region Messing with players

    private void UserSpecificMessages()
    {
        switch (Username().ToLower())
        {
            case "flamerking1223":
                OneTimeMessage("flamerking1223reddit", "Hey FlamerKing1223 (yes you specifically). The fact that you had the users in map window open when screenshotting that post about artix and posting it to reddit...\nYeh that was a dumb move.\n\nCheers, Skua Staff\nP.S.: We're not gonna do anything, but if we can figure it out, so can the AE moderators.");
                break;
        }
    }

    #endregion
}

public static class UtilExtensionsS
{
    // Logging
    public static void Log(this IScriptInterface bot, object? obj)
        => bot.Log(obj?.ToString() ?? "null");
    public static void Log(this IScriptInterface bot, IEnumerable<object>? obj)
        => bot.Log(JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented) ?? "null");

    // Badge Checks
    public static bool Contains(this List<Badge> list, Badge badge)
        => list.Any(b => b.ID == badge.ID);
    public static bool Contains(this List<Badge> list, int badgeID)
        => list.Any(b => b.ID == badgeID);
    public static bool Contains(this List<Badge> list, string badgeName)
        => list.Any(b => b.Name == badgeName);

    // List management
    public static T[] Except<T>(this IEnumerable<T> source, params T[] obj)
        => source.Except(second: obj).ToArray();
    public static T? Find<T>(this IEnumerable<T> source, Predicate<T> Match)
        => source.ToList().Find(match: Match);
    public static bool TryFind<T>(this IEnumerable<T> source, Predicate<T> Match, out T? toReturn)
        => (toReturn = source.Find(Match)) != null;
}

#nullable disable
public class Badge
{
    [JsonProperty("badgeID")]
    public int ID { get; set; }

    [JsonProperty("sTitle")]
    public string Name { get; set; }

    [JsonProperty("sCategory")]
    public string CategoryString { get; set; }
    private BadgeCategory? _category;
    public BadgeCategory Category
    {
        get
        {
            return _category ??= (BadgeCategory)Enum.Parse(typeof(BadgeCategory), CategoryString.Replace(" ", ""));
        }
    }

    [JsonProperty("sSubCategory")]
    public string SubCategory { get; set; }

    [JsonProperty("sDesc")]
    public string Description { get; set; }

    [JsonProperty("sFileName")]
    public string Image { get; set; }


    /*
        "badgeID": 7,
        "sCategory": "Legendary",
        "sTitle": "Member",
        "sDesc": "Awarded to those who have upgraded their accounts.",
        "sFileName": "member.jpg",
        "sSubCategory": "0"
    */
}

public enum Alignment
{
    Good = 1,
    Evil = 2,
    Chaos = 3
}

public enum ClassType
{
    Solo,
    Farm,
    None
}

public enum BadgeCategory
{
    ArtixEntertainment,
    Battle,
    EpicHero,
    Exclusive,
    HeroMart,
    Hidden,
    Legendary,
    Support
}

public enum AutoReportType
{
    LockedQuest,
    ScriptCrash,
}