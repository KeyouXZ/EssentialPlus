using EssentialsPlus.Db;
using EssentialsPlus.Extensions;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using TShockAPI.DB;

namespace EssentialsPlus;

public static class Commands
{
    public static async void Find(CommandArgs e)
    {
        var regex = new Regex(@"^\w+ -(?<switch>\w+) (?<search>.+?) ?(?<page>\d*)$");
        var match = regex.Match(e.Message);
        if (!match.Success)
        {
            e.Player.SendErrorMessage("Incorrect format! The correct syntax is: {0}find <-type> <name...> [page]",
                TShock.Config.Settings.CommandSpecifier);
            e.Player.SendSuccessMessage("Valid {0}find types:", TShock.Config.Settings.CommandSpecifier);
            e.Player.SendInfoMessage("-command: Find command.");
            e.Player.SendInfoMessage("-item: Find item.");
            e.Player.SendInfoMessage("-npc: Find NPC.");
            e.Player.SendInfoMessage("-tile: Find block.");
            e.Player.SendInfoMessage("-wall: Find wall.");
            return;
        }

        var page = 1;
        if (!string.IsNullOrWhiteSpace(match.Groups["page"].Value) &&
            (!int.TryParse(match.Groups["page"].Value, out page) || page <= 0))
        {
            e.Player.SendErrorMessage("Invalid page number '{0}'!", match.Groups["page"].Value);
            return;
        }

        switch (match.Groups["switch"].Value.ToLowerInvariant())
        {
            #region Command

            case "command":
            case "c":
            case "命令":
            {
                var commands = new List<string>();

                await Task.Run(() =>
                {
                    foreach (
                        var command in
                            TShockAPI.Commands.ChatCommands.FindAll(c => c.Names.Any(s => s.ContainsInsensitive(match.Groups[2].Value))))
                    {
                        commands.Add(string.Format("{0} (Permission: {1})", command.Name, command.Permissions.FirstOrDefault()));
                    }
                });

                PaginationTools.SendPage(e.Player, page, commands,
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Found commands ({0}/{1}):",
                        FooterFormat = string.Format("Type /find -command {0} {{0}} to see more", match.Groups[2].Value),
                        NothingToDisplayString = "No commands found."
                    });
                return;
            }

            #endregion

            #region Item

            case "item":
            case "i":
            case "物品":
            {
                var items = new List<string>();

                await Task.Run(() =>
                {
                    for (var i = -48; i < 0; i++)
                    {
                        var item = new Item();
                        item.netDefaults(i);
                        if (item.HoverName.ContainsInsensitive(match.Groups[2].Value))
                        {
                            items.Add(string.Format("{0} (ID: {1})", item.HoverName, i));
                        }
                    }
                    for (var i = 0; i < ItemID.Count; i++)
                    {
                        if (Lang.GetItemNameValue(i).ContainsInsensitive(match.Groups[2].Value))
                        {
                            items.Add(string.Format("{0} (ID: {1})", Lang.GetItemNameValue(i), i));
                        }
                    }
                });

                PaginationTools.SendPage(e.Player, page, items,
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Found items ({0}/{1}):",
                        FooterFormat = string.Format("Type /find -item {0} {{0}} to see more", match.Groups[2].Value),
                        NothingToDisplayString = "No items found."
                    });
                return;
            }

            
			#endregion

            #region NPC

            case "npc":
            case "n":
            case "NPC":
            {
                var npcs = new List<string>();

                await Task.Run(() =>
                {
                    for (var i = -65; i < 0; i++)
                    {
                        var npc = new NPC();
                        npc.SetDefaults(i);
                        if (npc.FullName.ContainsInsensitive(match.Groups[2].Value))
                        {
                            npcs.Add(string.Format("{0} (ID: {1})", npc.FullName, i));
                        }
                    }
                    for (var i = 0; i < NPCID.Count; i++)
                    {
                        if (Lang.GetNPCNameValue(i).ContainsInsensitive(match.Groups[2].Value))
                        {
                            npcs.Add(string.Format("{0} (ID: {1})", Lang.GetNPCNameValue(i), i));
                        }
                    }
                });

               PaginationTools.SendPage(e.Player, page, npcs,
    new PaginationTools.Settings
    {
        HeaderFormat = "Found NPCs ({0}/{1}):",
        FooterFormat = string.Format("Type /find -npc {0} {{0}} to see more", match.Groups[2].Value),
        NothingToDisplayString = "No NPCs found.",
    });
return;
}

#endregion

#region Tile

case "tile":
case "t":
case "方块":
{
    var tiles = new List<string>();

    await Task.Run(() =>
    {
        foreach (var fi in typeof(TileID).GetFields())
        {
            var sb = new StringBuilder();
            for (var i = 0; i < fi.Name.Length; i++)
            {
                if (char.IsUpper(fi.Name[i]) && i > 0)
                {
                    sb.Append(" ").Append(fi.Name[i]);
                }
                else
                {
                    sb.Append(fi.Name[i]);
                }
            }

            var name = sb.ToString();
            if (name.ContainsInsensitive(match.Groups[2].Value))
            {
                tiles.Add(string.Format("{0} (ID: {1})", name, fi.GetValue(null)));
            }
        }
    });

    PaginationTools.SendPage(e.Player, page, tiles,
        new PaginationTools.Settings
        {
            HeaderFormat = "Found blocks ({0}/{1}):",
            FooterFormat = string.Format("Type /find -tile {0} {{0}} to see more", match.Groups[2].Value),
            NothingToDisplayString = "No blocks found.",
        });
    return;
}

#endregion

#region Wall

case "wall":
case "w":
case "墙壁":
{
    var walls = new List<string>();

    await Task.Run(() =>
    {
        foreach (var fi in typeof(WallID).GetFields())
        {
            var sb = new StringBuilder();
            for (var i = 0; i < fi.Name.Length; i++)
            {
                if (char.IsUpper(fi.Name[i]) && i > 0)
                {
                    sb.Append(" ").Append(fi.Name[i]);
                }
                else
                {
                    sb.Append(fi.Name[i]);
                }
            }

            var name = sb.ToString();
            if (name.ContainsInsensitive(match.Groups[2].Value))
            {
                walls.Add(string.Format("{0} (ID: {1})", name, fi.GetValue(null)));
            }
        }
    });

    PaginationTools.SendPage(e.Player, page, walls,
        new PaginationTools.Settings
        {
            HeaderFormat = "Found walls ({0}/{1}):",
            FooterFormat = string.Format("Type /find -wall {0} {{0}} to see more", match.Groups[2].Value),
            NothingToDisplayString = "No walls found.",
        });
    return;
}

#endregion

default:
{
    e.Player.SendSuccessMessage("Valid {0}find switches:", TShock.Config.Settings.CommandSpecifier);
    e.Player.SendInfoMessage("-command: Find command.");
    e.Player.SendInfoMessage("-item: Find item.");
    e.Player.SendInfoMessage("-npc: Find NPC.");
    e.Player.SendInfoMessage("-tile: Find block.");
    e.Player.SendInfoMessage("-wall: Find wall.");
    return;
}
}
}

public static System.Timers.Timer FreezeTimer = new System.Timers.Timer(1000);

public static void FreezeTime(CommandArgs e)
{
    if (FreezeTimer.Enabled)
    {
        FreezeTimer.Stop();
        TSPlayer.All.SendInfoMessage("{0} has unfrozen time.", e.Player.Name);
    }
    else
    {
        var dayTime = Main.dayTime;
        var time = Main.time;

        FreezeTimer.Dispose();
        FreezeTimer = new System.Timers.Timer(1000);
        FreezeTimer.Elapsed += (o, ee) =>
        {
            Main.dayTime = dayTime;
            Main.time = time;
            TSPlayer.All.SendData(PacketTypes.TimeSet);
        };
        FreezeTimer.Start();
        TSPlayer.All.SendInfoMessage("{0} has frozen time.", e.Player.Name);
    }
}

    public static void DeleteHome(CommandArgs e)
{
    if (e.Parameters.Count > 1)
    {
        e.Player.SendErrorMessage("Syntax error! Correct syntax is: {0}delhome <home name>", TShock.Config.Settings.CommandSpecifier);
        return;
    }

    var homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
    var home = EssentialsPlus.Homes.GetHome(e.Player, homeName);
    if (home != null)
    {
        if (EssentialsPlus.Homes.DeleteHome(e.Player, homeName))
        {
            e.Player.SendSuccessMessage("Successfully deleted your home '{0}'.", homeName);
        }
        else
        {
            e.Player.SendErrorMessage("Unable to delete the home, please check the logs for more details.");
        }
    }
    else
    {
        e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
    }
}

public static async void MyHome(CommandArgs e)
{
    if (e.Parameters.Count > 1)
    {
        e.Player.SendErrorMessage("Syntax error! Correct syntax is: {0}myhome <home name>", TShock.Config.Settings.CommandSpecifier);
        return;
    }

    if (Regex.Match(e.Message, @"^\w+ -l(?:ist)?$").Success)
    {
        var homes = EssentialsPlus.Homes.GetAllAsync(e.Player);
        e.Player.SendInfoMessage(homes.Count == 0 ? "You haven't set any homes." : "List of homes: {0}", string.Join(", ", homes.Select(h => h.Name)));
    }
    else
    {
        var homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
        var home = EssentialsPlus.Homes.GetHome(e.Player, homeName);
        if (home != null)
        {
            e.Player.Teleport(home.X, home.Y);
            e.Player.SendSuccessMessage("Teleported you to your home '{0}'.", homeName);
        }
        else
        {
            e.Player.SendErrorMessage("Invalid home '{0}'!", homeName);
        }
    }
}

public static async void SetHome(CommandArgs e)
{
    if (e.Parameters.Count > 1)
    {
        e.Player.SendErrorMessage("Syntax error! Correct syntax is: {0}sethome <home name>", TShock.Config.Settings.CommandSpecifier);
        return;
    }

    var homeName = e.Parameters.Count == 1 ? e.Parameters[0] : "home";
    if (EssentialsPlus.Homes.GetHome(e.Player, homeName) != null)
    {
        if (EssentialsPlus.Homes.UpdateHome(e.Player, homeName, e.Player.X, e.Player.Y))
        {
            e.Player.SendSuccessMessage("Updated your home '{0}'.", homeName);
        }
        else
        {
            e.Player.SendErrorMessage("Unable to update the home, please check the logs for more details.");
        }
        return;
    }

    if (EssentialsPlus.Homes.GetAllAsync(e.Player).Count >= e.Player.Group.GetDynamicPermission(Permissions.HomeSet))
    {
        e.Player.SendErrorMessage("You have reached the maximum limit of set homes!");
        return;
    }

    if (EssentialsPlus.Homes.AddHome(e.Player, homeName, e.Player.X, e.Player.Y))
    {
        e.Player.SendSuccessMessage("Set your home '{0}'.", homeName);
    }
    else
    {
        e.Player.SendErrorMessage("Unable to set the home, please check the logs for more details.");
    }
}

public static async void KickAll(CommandArgs e)
{
    var regex = new Regex(@"^\w+(?: -(\w+))* ?(.*)$");
    var match = regex.Match(e.Message);
    var noSave = false;
    foreach (Capture capture in match.Groups[1].Captures)
    {
        switch (capture.Value.ToLowerInvariant())
        {
            case "nosave":
                noSave = true;
                continue;
            default:
                e.Player.SendSuccessMessage("Valid {0}kickall switch:", TShock.Config.Settings.CommandSpecifier);
                e.Player.SendInfoMessage("-nosave: Does not save SSC data when kicked.");
                return;
        }
    }

    var kickLevel = e.Player.Group.GetDynamicPermission(Permissions.KickAll);
    var reason = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? "No reason." : match.Groups[2].Value;
    await Task.WhenAll(TShock.Players.Where(p => p != null && p.Group.GetDynamicPermission(Permissions.KickAll) < kickLevel).Select(p => Task.Run(() =>
    {
        if (!noSave && p.IsLoggedIn)
        {
            p.SaveServerCharacter();
        }
        p.Disconnect("Kick reason: " + reason);
    })));
    e.Player.SendSuccessMessage("Successfully kicked everyone. Reason: '{0}'.", reason);
}

public static async void RepeatLast(CommandArgs e)
{
    var lastCommand = e.Player.GetPlayerInfo().LastCommand;
    if (string.IsNullOrWhiteSpace(lastCommand))
    {
        e.Player.SendErrorMessage("You don't have a last command!");
        return;
    }

    e.Player.SendSuccessMessage("Repeated the last command '{0}{1}'!", TShock.Config.Settings.CommandSpecifier, lastCommand);
    await Task.Run(() => TShockAPI.Commands.HandleCommand(e.Player, TShock.Config.Settings.CommandSpecifier + lastCommand));
}

public static async void More(CommandArgs e)
{
    await Task.Run(() =>
    {
        if (e.Parameters.Count > 0 && e.Parameters[0].ToLower() == "all")
        {
            var full = true;
            foreach (var item in e.TPlayer.inventory)
            {
                if (item == null || item.stack == 0)
                {
                    continue;
                }

                var amtToAdd = item.maxStack - item.stack;
                if (amtToAdd > 0 && item.stack > 0 && !item.Name.ToLower().Contains("coin"))
                {
                    full = false;
                    e.Player.GiveItem(item.type, amtToAdd);
                }
            }
            if (!full)
            {
                e.Player.SendSuccessMessage("Filled up your inventory.");
            }
            else
            {
                e.Player.SendErrorMessage("Your inventory is already full.");
            }
        }
        else
        {
            var item = e.Player.TPlayer.inventory[e.TPlayer.selectedItem];
            var amtToAdd = item.maxStack - item.stack;
            if (amtToAdd == 0)
            {
                e.Player.SendErrorMessage("Your {0} is already full.", item.Name);
            }
            else if (amtToAdd > 0 && item.stack > 0)
            {
                e.Player.GiveItem(item.type, amtToAdd);
            }

            e.Player.SendSuccessMessage("Increased the stack size of your {0}.", item.Name);
        }
    });
}

public static async void Mute(CommandArgs e)
{
    var subCmd = e.Parameters.FirstOrDefault() ?? "help";
    switch (subCmd.ToLowerInvariant())
    {
        #region Add Mute

        case "add":
        {
            var regex = new Regex(@"^\w+ \w+ (?:""(.+?)""|([^\s]+?))(?: (.+))?$");
            var match = regex.Match(e.Message);
            if (!match.Success)
            {
                e.Player.SendErrorMessage("Invalid syntax! Correct syntax is: /mute add <name> [time]");
                return;
            }

            var seconds = int.MaxValue / 1000;
            if (!string.IsNullOrWhiteSpace(match.Groups[3].Value) &&
                (!TShock.Utils.TryParseTime(match.Groups[3].Value, out seconds) || seconds <= 0 ||
                 seconds > int.MaxValue / 1000))
            {
                e.Player.SendErrorMessage("Invalid time '{0}'!", match.Groups[3].Value);
                return;
            }

            var playerName = string.IsNullOrWhiteSpace(match.Groups[2].Value)
                ? match.Groups[1].Value
                : match.Groups[2].Value;
            var players = TShock.Players.FindPlayers(playerName);
            if (players.Count == 0)
            {
                var user = TShock.UserAccounts.GetUserAccountByName(playerName);
                if (user == null)
                {
                    e.Player.SendErrorMessage("Invalid player or account '{0}'!", playerName);
                }
                else
                {
                    if (TShock.Groups.GetGroupByName(user.Group).GetDynamicPermission(Permissions.Mute) >=
                        e.Player.Group.GetDynamicPermission(Permissions.Mute))
                    {
                        e.Player.SendErrorMessage("You cannot mute {0}!", user.Name);
                        return;
                    }

                    if (EssentialsPlus.Mutes.AddMute(user, DateTime.UtcNow.AddSeconds(seconds)))
                    {
                        TSPlayer.All.SendInfoMessage("{0} muted {1}.", e.Player.Name, user.Name);
                    }
                    else
                    {
                        e.Player.SendErrorMessage("Unable to mute, please check the logs for more details.");
                    }
                }
            }
            else if (players.Count > 1)
            {
                e.Player.SendErrorMessage("Matched multiple players: {0}", string.Join(", ", players.Select(p => p.Name)));
            }
            else
            {
                if (players[0].Group.GetDynamicPermission(Permissions.Mute) >=
                    e.Player.Group.GetDynamicPermission(Permissions.Mute))
                {
                    e.Player.SendErrorMessage("You cannot mute {0}!", players[0].Name);
                    return;
                }

                if (EssentialsPlus.Mutes.AddMute(players[0], DateTime.UtcNow.AddSeconds(seconds)))
                {
                    TSPlayer.All.SendInfoMessage("{0} muted {1}.", e.Player.Name, players[0].Name);

                    players[0].mute = true;
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(seconds), players[0].GetPlayerInfo().MuteToken);
                        players[0].mute = false;
                        players[0].SendInfoMessage("You have been unmuted.");
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
                else
                {
                    e.Player.SendErrorMessage("Unable to mute, please check the logs for more details.");
                }
            }
        }
        return;

        #endregion

        #region Remove Mute


            case "del":
            case "delete":
            {
                var regex = new Regex(@"^\w+ \w+ (?:""(.+?)""|([^\s]*?))$");
                var match = regex.Match(e.Message);
                if (!match.Success)
                {
                    e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: /mute del <name>");
                    return;
                }

                var playerName = string.IsNullOrWhiteSpace(match.Groups[2].Value)
                    ? match.Groups[1].Value
                    : match.Groups[2].Value;
                var players = TShock.Players.FindPlayers(playerName);
                if (players.Count == 0)
                {
                    var user = TShock.UserAccounts.GetUserAccountByName(playerName);
                    if (user == null)
                    {
                        e.Player.SendErrorMessage("Invalid Players or Accounts '{0}'！", playerName);
                    }
                    else
                    {
                        if (EssentialsPlus.Mutes.DeleteMute(user))
                        {
                            TSPlayer.All.SendInfoMessage("{0} Unblocked {1}.", e.Player.Name, user.Name);
                        }
                        else
                        {
                            e.Player.SendErrorMessage("Unable to unban, check the logs for more information.");
                        }
                    }
                }
                else if (players.Count > 1)
                {
                    e.Player.SendErrorMessage("Matching to multiple players：{0}", string.Join(", ", players.Select(p => p.Name)));
                }
                else
                {
                    if (EssentialsPlus.Mutes.DeleteMute(players[0]))
                    {
                        players[0].mute = false;
                        TSPlayer.All.SendInfoMessage("{0} Unblocked {1}", e.Player.Name, players[0].Name);
                    }
                    else
                    {
                        e.Player.SendErrorMessage("Unable to unban, check the logs for more information.");
                    }
                }
            }
            return;

            #endregion

            #region 帮助

            default:
                e.Player.SendSuccessMessage("Banning subcommands:");
                e.Player.SendInfoMessage("add <name> [time] - Bans a player or account.");
                e.Player.SendInfoMessage("del <name> - unban a player or account");
                return;

                #endregion
        }
    }


    public static void PvP(CommandArgs e)
    {
        e.TPlayer.hostile = !e.TPlayer.hostile;
        var hostile = Language.GetTextValue(e.TPlayer.hostile ? "LegacyMultiplayer.11" : "LegacyMultiplayer.12", e.Player.Name);
        TSPlayer.All.SendData(PacketTypes.TogglePvp, "", e.Player.Index);
        TSPlayer.All.SendMessage(hostile, Main.teamColor[e.Player.Team]);
    }

    public static void Ruler(CommandArgs e)
    {
        if (e.Parameters.Count == 0)
        {
            if (e.Player.TempPoints.Any(p => p == Point.Zero))
            {
                e.Player.SendErrorMessage("The ruler gauge is not set!");
                return;
            }

            var p1 = e.Player.TempPoints[0];
            var p2 = e.Player.TempPoints[1];
            var x = Math.Abs(p1.X - p2.X);
            var y = Math.Abs(p1.Y - p2.Y);
            var cartesian = Math.Sqrt((x * x) + (y * y));
            e.Player.SendInfoMessage("Distance: x-axis: {0}, y-axis: {1}, right-angle distance:{2:N3}", x, y, cartesian);
        }
        else if (e.Parameters.Count == 1)
        {
            if (e.Parameters[0] == "1")
            {
                e.Player.AwaitingTempPoint = 1;
                e.Player.SendInfoMessage("Modify a square to set the first ruler point.");
            }
            else if (e.Parameters[0] == "2")
            {
                e.Player.AwaitingTempPoint = 2;
                e.Player.SendInfoMessage("Modify a square to set a second ruler point.");
            }
            else
            {
                e.Player.SendErrorMessage("Invalid grammar! The correct syntax is：{0}ruler [1/2]", TShock.Config.Settings.CommandSpecifier);
            }
        }
        else
        {
            e.Player.SendErrorMessage("Invalid grammar! The correct syntax is：{0}ruler [1/2]", TShock.Config.Settings.CommandSpecifier);
        }
    }


    public static void Send(CommandArgs e)
    {
        var regex = new Regex(@"^\w+(?: (\d+),(\d+),(\d+))? (.+)$");
        var match = regex.Match(e.Message);
        if (!match.Success)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}send [r,g,b] <text... >", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var r = e.Player.Group.R;
        var g = e.Player.Group.G;
        var b = e.Player.Group.B;
        if (!string.IsNullOrWhiteSpace(match.Groups[1].Value) && !string.IsNullOrWhiteSpace(match.Groups[2].Value) && !string.IsNullOrWhiteSpace(match.Groups[3].Value) &&
            (!byte.TryParse(match.Groups[1].Value, out r) || !byte.TryParse(match.Groups[2].Value, out g) || !byte.TryParse(match.Groups[3].Value, out b)))
        {
            e.Player.SendErrorMessage("Invalid colours!");
            return;
        }
        TSPlayer.All.SendMessage(match.Groups[4].Value, new Color(r, g, b));
    }


    public static async void Sudo(CommandArgs e)
    {
        var regex = new Regex(string.Format(@"^\w+(?: -(\w+))* (?:""(.+?)""|([^\s]*?)) (?:{0})?(.+)$", TShock.Config.Settings.CommandSpecifier));
        var match = regex.Match(e.Message);
        if (!match.Success)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}sudo [-switches...] <player> <command... >", TShock.Config.Settings.CommandSpecifier);
            e.Player.SendSuccessMessage("Valid {0}sudo switches:", TShock.Config.Settings.CommandSpecifier);
            e.Player.SendInfoMessage("-f, -force: Force sudo and ignore permission restrictions.");
            return;
        }

        var force = false;
        foreach (Capture capture in match.Groups[1].Captures)
        {
            switch (capture.Value.ToLowerInvariant())
            {
                case "f":
                case "force":
                    if (!e.Player.Group.HasPermission(Permissions.SudoForce))
                    {
                        e.Player.SendErrorMessage("You do not have access to the ‘-{0}’ switch!", capture.Value);
                        return;
                    }
                    force = true;
                    continue;
                default:
                    e.Player.SendSuccessMessage("Valid {0}sudo switches：", TShock.Config.Settings.CommandSpecifier);
                    e.Player.SendInfoMessage("-f, -force: Force sudo and ignore permission restrictions.");
                    return;
            }
        }

        var playerName = string.IsNullOrWhiteSpace(match.Groups[3].Value) ? match.Groups[2].Value : match.Groups[3].Value;
        var command = match.Groups[4].Value;

        var players = TShock.Players.FindPlayers(playerName);
        if (players.Count == 0)
        {
            e.Player.SendErrorMessage("Invalid Players '{0}'！", playerName);
        }
        else if (players.Count > 1)
        {
            e.Player.SendErrorMessage("Matching to multiple players：{0}", string.Join(", ", players.Select(p => p.Name)));
        }
        else
        {
            if ((e.Player.Group.GetDynamicPermission(Permissions.Sudo) <= players[0].Group.GetDynamicPermission(Permissions.Sudo))
                && !e.Player.Group.HasPermission(Permissions.SudoSuper))
            {
                e.Player.SendErrorMessage("You cannot force {0} to execute {1}{2}！", players[0].Name, TShock.Config.Settings.CommandSpecifier, command);
                return;
            }

            e.Player.SendSuccessMessage("Force {0} Execute {1}{2}。", players[0].Name, TShock.Config.Settings.CommandSpecifier, command);
            if (!e.Player.Group.HasPermission(Permissions.SudoInvisible))
            {
                players[0].SendInfoMessage("{0} Enforcement {1}{2}。", e.Player.Name, TShock.Config.Settings.CommandSpecifier, command);
            }

            var fakePlayer = new TSPlayer(players[0].Index)
            {
                AwaitingName = players[0].AwaitingName,
                AwaitingNameParameters = players[0].AwaitingNameParameters,
                AwaitingTempPoint = players[0].AwaitingTempPoint,
                Group = force ? new SuperAdminGroup() : players[0].Group,
                TempPoints = players[0].TempPoints
            };
            await Task.Run(() => TShockAPI.Commands.HandleCommand(fakePlayer, TShock.Config.Settings.CommandSpecifier + command));

            players[0].AwaitingName = fakePlayer.AwaitingName;
            players[0].AwaitingNameParameters = fakePlayer.AwaitingNameParameters;
            players[0].AwaitingTempPoint = fakePlayer.AwaitingTempPoint;
            players[0].TempPoints = fakePlayer.TempPoints;
        }
    }


    public static async void TimeCmd(CommandArgs e)
    {
        var regex = new Regex(string.Format(@"^\w+(?: -(\w+))* (\w+) (?:{0})?(.+)$", TShock.Config.Settings.CommandSpecifier));
        var match = regex.Match(e.Message);
        if (!match.Success)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}timecmd [-switches...] <time> <commands... >", TShock.Config.Settings.CommandSpecifier);
            e.Player.SendSuccessMessage("Valid {0}timecmd switches.", TShock.Config.Settings.CommandSpecifier);
            e.Player.SendInfoMessage("-r, -repeat: Repeat the time command.");
            return;
        }

        var repeat = false;
        foreach (Capture capture in match.Groups[1].Captures)
        {
            switch (capture.Value.ToLowerInvariant())
            {
                case "r":
                case "repeat":
                    repeat = true;
                    break;
                default:
                    e.Player.SendSuccessMessage("Valid {0}timecmd switches:", TShock.Config.Settings.CommandSpecifier);
                    e.Player.SendInfoMessage("-r, -repeat: Repeat the time command.");
                    return;
            }
        }

        if (!TShock.Utils.TryParseTime(match.Groups[2].Value, out int seconds) || seconds <= 0 || seconds > int.MaxValue / 1000)
        {
            e.Player.SendErrorMessage("lapse '{0}'!", match.Groups[2].Value);
            return;
        }

        if (repeat)
        {
            e.Player.SendSuccessMessage("The command ‘{0}{1}’ has been queued for execution. Use /cancel to cancel!", TShock.Config.Settings.CommandSpecifier, match.Groups[3].Value);
        }
        else
        {
            e.Player.SendSuccessMessage("The command ‘{0}{1}’ has been queued for execution. Use /cancel to cancel!", TShock.Config.Settings.CommandSpecifier, match.Groups[3].Value);
        }

        e.Player.AddResponse("cancel", o =>
        {
            e.Player.GetPlayerInfo().CancelTimeCmd();
            e.Player.SendSuccessMessage("Cancels all time commands!");
        });

        var token = e.Player.GetPlayerInfo().TimeCmdToken;
        try
        {
            await Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                    TShockAPI.Commands.HandleCommand(e.Player, TShock.Config.Settings.CommandSpecifier + match.Groups[3].Value);
                }
                while (repeat);
            }, token);
        }
        catch (TaskCanceledException)
        {
        }
    }


    public static void Back(CommandArgs e)
    {
        if (e.Parameters.Count > 1)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}eback [number of steps]", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var steps = 1;
        if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out steps) || steps <= 0))
        {
            e.Player.SendErrorMessage("Invalid steps '{0}'！", e.Parameters[0]);
            return;
        }

        var info = e.Player.GetPlayerInfo();
        if (info.BackHistoryCount == 0)
        {
            e.Player.SendErrorMessage("Unable to teleport back to previous location!");
            return;
        }

        steps = Math.Min(steps, info.BackHistoryCount);
        e.Player.SendSuccessMessage("Send back {0} steps.", steps);
        var vector = info.PopBackHistory(steps);
        e.Player.Teleport(vector.X, vector.Y);
    }

    public static async void Down(CommandArgs e)
    {
        if (e.Parameters.Count > 1)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}down [number of layers]", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var levels = 1;
        if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
        {
            e.Player.SendErrorMessage("Number of invalid layers '{0}'！", levels);
            return;
        }

        var currentLevel = 0;
        var empty = false;
        var x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
        var y = Math.Max(0, Math.Min(e.Player.TileY + 3, Main.maxTilesY - 3));

        await Task.Run(() =>
        {
            for (var j = y; currentLevel < levels && j < Main.maxTilesY - 2; j++)
            {
                if (Main.tile[x, j].IsEmpty() && Main.tile[x + 1, j].IsEmpty() &&
                    Main.tile[x, j + 1].IsEmpty() && Main.tile[x + 1, j + 1].IsEmpty() &&
                    Main.tile[x, j + 2].IsEmpty() && Main.tile[x + 1, j + 2].IsEmpty())
                {
                    empty = true;
                }
                else if (empty)
                {
                    empty = false;
                    currentLevel++;
                    y = j;
                }
            }
        });

        if (currentLevel == 0)
        {
            e.Player.SendErrorMessage("Can't teleport down!");
        }
        else
        {
            if (e.Player.Group.HasPermission(Permissions.TpBack))
            {
                e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
            }

            e.Player.Teleport(16 * x, (16 * y) - 10);
            e.Player.SendSuccessMessage("Transport down to level {0}.", currentLevel);
        }
    }

    public static async void Left(CommandArgs e)
    {
        if (e.Parameters.Count > 1)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}left [number of layers]", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var levels = 1;
        if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
        {
            e.Player.SendErrorMessage("Number of invalid layers '{0}'！", levels);
            return;
        }

        var currentLevel = 0;
        var solid = false;
        var x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
        var y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

        await Task.Run(() =>
        {
            for (var i = x; currentLevel < levels && i >= 0; i--)
            {
                if (Main.tile[i, y].IsEmpty() && Main.tile[i + 1, y].IsEmpty() &&
                    Main.tile[i, y + 1].IsEmpty() && Main.tile[i + 1, y + 1].IsEmpty() &&
                    Main.tile[i, y + 2].IsEmpty() && Main.tile[i + 1, y + 2].IsEmpty())
                {
                    if (solid)
                    {
                        solid = false;
                        currentLevel++;
                        x = i;
                    }
                }
                else
                {
                    solid = true;
                }
            }
        });

        if (currentLevel == 0)
        {
            e.Player.SendErrorMessage("Unable to teleport left!");
        }
        else
        {
            if (e.Player.Group.HasPermission(Permissions.TpBack))
            {
                e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
            }

            e.Player.Teleport((16 * x) + 12, 16 * y);
            e.Player.SendSuccessMessage("Transmit left {0} level.", currentLevel);
        }
    }

    public static async void Right(CommandArgs e)
    {
        if (e.Parameters.Count > 1)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}right [number of layers]", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var levels = 1;
        if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
        {
            e.Player.SendErrorMessage("Number of invalid layers '{0}'！", levels);
            return;
        }

        var currentLevel = 0;
        var solid = false;
        var x = Math.Max(0, Math.Min(e.Player.TileX + 1, Main.maxTilesX - 2));
        var y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

        await Task.Run(() =>
        {
            for (var i = x; currentLevel < levels && i < Main.maxTilesX - 1; i++)
            {
                if (Main.tile[i, y].IsEmpty() && Main.tile[i + 1, y].IsEmpty() &&
                    Main.tile[i, y + 1].IsEmpty() && Main.tile[i + 1, y + 1].IsEmpty() &&
                    Main.tile[i, y + 2].IsEmpty() && Main.tile[i + 1, y + 2].IsEmpty())
                {
                    if (solid)
                    {
                        solid = false;
                        currentLevel++;
                        x = i;
                    }
                }
                else
                {
                    solid = true;
                }
            }
        });

        if (currentLevel == 0)
        {
            e.Player.SendErrorMessage("Unable to teleport right!");
        }
        else
        {
            if (e.Player.Group.HasPermission(Permissions.TpBack))
            {
                e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
            }

            e.Player.Teleport(16 * x, 16 * y);
            e.Player.SendSuccessMessage("Transmit right {0} level.", currentLevel);
        }
    }

    public static async void Up(CommandArgs e)
    {
        if (e.Parameters.Count > 1)
        {
            e.Player.SendErrorMessage("Invalid syntax! The correct syntax is: {0}up [number of layers]", TShock.Config.Settings.CommandSpecifier);
            return;
        }

        var levels = 1;
        if (e.Parameters.Count > 0 && (!int.TryParse(e.Parameters[0], out levels) || levels <= 0))
        {
            e.Player.SendErrorMessage("Number of invalid layers '{0}'！", levels);
            return;
        }

        var currentLevel = 0;
        var solid = false;
        var x = Math.Max(0, Math.Min(e.Player.TileX, Main.maxTilesX - 2));
        var y = Math.Max(0, Math.Min(e.Player.TileY, Main.maxTilesY - 3));

        await Task.Run(() =>
        {
            for (var j = y; currentLevel < levels && j >= 0; j--)
            {
                if (Main.tile[x, j].IsEmpty() && Main.tile[x + 1, j].IsEmpty() &&
                    Main.tile[x, j + 1].IsEmpty() && Main.tile[x + 1, j + 1].IsEmpty() &&
                    Main.tile[x, j + 2].IsEmpty() && Main.tile[x + 1, j + 2].IsEmpty())
                {
                    if (solid)
                    {
                        solid = false;
                        currentLevel++;
                        y = j;
                    }
                }
                else
                {
                    solid = true;
                }
            }
        });

        if (currentLevel == 0)
        {
            e.Player.SendErrorMessage("Unable to teleport above!");
        }
        else
        {
            if (e.Player.Group.HasPermission(Permissions.TpBack))
            {
                e.Player.GetPlayerInfo().PushBackHistory(e.TPlayer.position);
            }

            e.Player.Teleport(16 * x, (16 * y) + 6);
            e.Player.SendSuccessMessage("Transport to the {0} level.", currentLevel);
        }
    }

}