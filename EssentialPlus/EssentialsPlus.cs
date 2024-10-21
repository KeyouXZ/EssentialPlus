using EssentialsPlus.Db;
using EssentialsPlus.Extensions;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace EssentialsPlus;

[ApiVersion(2, 1)]
public class EssentialsPlus : TerrariaPlugin
{
    public static Config Config { get; private set; }
    public static IDbConnection Db { get; private set; }
    public static HomeManager Homes { get; private set; }
    public static MuteManager Mutes { get; private set; }

    public override string Author => "WhiteX and others, Average, Cjx, Liver Emperor Xi En (translation), Cai (updates)";

    public override string Description => "Enhanced version of Essentials";

    public override string Name => "EssentialsPlus";

    public override Version Version => new Version(1, 0, 2);

    public EssentialsPlus(Main game)
        : base(game)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= this.OnReload;
            PlayerHooks.PlayerCommand -= this.OnPlayerCommand;

            ServerApi.Hooks.GameInitialize.Deregister(this, this.OnInitialize);
            ServerApi.Hooks.GamePostInitialize.Deregister(this, this.OnPostInitialize);
            ServerApi.Hooks.NetGetData.Deregister(this, this.OnGetData);
            ServerApi.Hooks.ServerJoin.Deregister(this, this.OnJoin);
        }
        base.Dispose(disposing);
    }

    public override void Initialize()
    {
        GeneralHooks.ReloadEvent += this.OnReload;
        PlayerHooks.PlayerCommand += this.OnPlayerCommand;

        ServerApi.Hooks.GameInitialize.Register(this, this.OnInitialize);
        ServerApi.Hooks.GamePostInitialize.Register(this, this.OnPostInitialize);
        ServerApi.Hooks.NetGetData.Register(this, this.OnGetData);
        ServerApi.Hooks.ServerJoin.Register(this, this.OnJoin);
    }

    private void OnReload(ReloadEventArgs e)
    {
        var path = Path.Combine(TShock.SavePath, "essentials.json");
        Config = Config.Read(path);
        if (!File.Exists(path))
        {
            Config.Write(path);
        }
        Homes.Reload();
        e.Player.SendSuccessMessage("[EssentialsPlus] Reloaded configuration and homes!");
    }

    private readonly List<string> teleportCommands = new List<string>
    {
        "tp", "tppos", "tpnpc", "warp", "spawn", "home"
    };

    private void OnPlayerCommand(PlayerCommandEventArgs e)
    {
        if (e.Handled || e.Player == null)
        {
            return;
        }

        var command = e.CommandList.FirstOrDefault();
        if (command == null || (command.Permissions.Any() && !command.Permissions.Any(s => e.Player.Group.HasPermission(s))))
        {
            return;
        }

        if (e.Player.TPlayer.hostile &&
            command.Names.Select(s => s.ToLowerInvariant())
                .Intersect(Config.DisabledCommandsInPvp.Select(s => s.ToLowerInvariant()))
                .Any())
        {
            e.Player.SendErrorMessage("You cannot use this command in PvP!");
            e.Handled = true;
            return;
        }

        if (e.Player.Group.HasPermission(Permissions.LastCommand) && command.CommandDelegate != Commands.RepeatLast)
        {
            e.Player.GetPlayerInfo().LastCommand = e.CommandText;
        }

        if (this.teleportCommands.Contains(e.CommandName) && e.Player.Group.HasPermission(Permissions.TpBack))
        {
            e.Player.GetPlayerInfo().PushBackHistory(e.Player.TPlayer.position);
        }
    }

    private void OnInitialize(EventArgs e)
    {
        #region Config

        var path = Path.Combine(TShock.SavePath, "essentials.json");
        Config = Config.Read(path);
        if (!File.Exists(path))
        {
            Config.Write(path);
        }

        #endregion

        #region Database

        if (TShock.Config.Settings.StorageType.Equals("mysql", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(Config.MySqlHost) ||
                string.IsNullOrWhiteSpace(Config.MySqlDbName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Essentials+] MySQL is enabled, but Essentials+ MySQL configuration is not set.");
                Console.WriteLine("[Essentials+] Please configure your MySQL server information in essentials.json, then restart the server.");
                Console.WriteLine("[Essentials+] This plugin will now disable itself...");
                Console.ResetColor();

                GeneralHooks.ReloadEvent -= this.OnReload;
                PlayerHooks.PlayerCommand -= this.OnPlayerCommand;

                ServerApi.Hooks.GameInitialize.Deregister(this, this.OnInitialize);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, this.OnPostInitialize);
                ServerApi.Hooks.NetGetData.Deregister(this, this.OnGetData);
                ServerApi.Hooks.ServerJoin.Deregister(this, this.OnJoin);

                return;
            }

            var host = Config.MySqlHost.Split(':');
            Db = new MySqlConnection
            {
                ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                    host[0],
                    host.Length == 1 ? "3306" : host[1],
                    Config.MySqlDbName,
                    Config.MySqlUsername,
                    Config.MySqlPassword)
            };
        }
        else
        {
            Db = TShock.Config.Settings.StorageType.Equals("sqlite", StringComparison.OrdinalIgnoreCase)
                ? (IDbConnection) new SqliteConnection(
                            "Data Source=" + Path.Combine(TShock.SavePath, "essentials.sqlite"))
                : throw new InvalidOperationException("Invalid storage type!");
        }

        Mutes = new MuteManager(Db);

        #endregion

        #region Commands

        // Allow overriding of existing commands.
        Action<Command> Add = c =>
        {
            // Find any command that matches the name or alias of the new command and remove it.
            TShockAPI.Commands.ChatCommands.RemoveAll(c2 => c2.Names.Exists(s2 => c.Names.Contains(s2)));
            // Then add the new command.
            TShockAPI.Commands.ChatCommands.Add(c);
        };

        Add(new Command(Permissions.Find, Commands.Find, "find")
        {
            HelpText = "Find items and/or NPCs with the specified name."
        });

        Add(new Command(Permissions.FreezeTime, Commands.FreezeTime, "freezetime")
        {
            HelpText = "Toggle freezing time."
        });

        Add(new Command(Permissions.HomeDelete, Commands.DeleteHome, "delhome")
        {
            AllowServer = false,
            HelpText = "Delete one of your homes."
        });
        Add(new Command(Permissions.HomeSet, Commands.SetHome, "sethome")
        {
            AllowServer = false,
            HelpText = "Set one of your homes."
        });
        Add(new Command(Permissions.HomeTp, Commands.MyHome, "myhome")
        {
            AllowServer = false,
            HelpText = "Teleport to one of your homes."
        });

        Add(new Command(Permissions.KickAll, Commands.KickAll, "kickall")
        {
            HelpText = "Kick everyone on the server."
        });

        Add(new Command(Permissions.LastCommand, Commands.RepeatLast, "=")
        {
            HelpText = "Allows you to repeat the last command."
        });

        Add(new Command(Permissions.More, Commands.More, "more")
        {
            AllowServer = false,
            HelpText = "Maximize the stack of the item you're holding."
        });

        // This will override TShock's 'mute' command.
        Add(new Command(Permissions.Mute, Commands.Mute, "mute")
        {
            HelpText = "Manage mutes."
        });

        Add(new Command(Permissions.PvP, Commands.PvP, "pvpget2")
        {
            AllowServer = false,
            HelpText = "Toggle your PvP status."
        });

        Add(new Command(Permissions.Ruler, Commands.Ruler, "ruler")
        {
            AllowServer = false,
            HelpText = "Allows you to measure the distance between two tiles."
        });

        Add(new Command(Permissions.Send, Commands.Send, "send")
        {
            HelpText = "Broadcast a message with a custom color."
        });

        Add(new Command(Permissions.Sudo, Commands.Sudo, "sudo")
        {
            HelpText = "Allows you to execute commands as another user."
        });

        Add(new Command(Permissions.TimeCmd, Commands.TimeCmd, "timecmd")
        {
            HelpText = "Execute a command after a given time interval."
        });

        Add(new Command(Permissions.TpBack, Commands.Back, "eback", "b")
        {
            AllowServer = false,
            HelpText = "Teleport back to your previous location after dying or teleporting."
        });
        Add(new Command(Permissions.TpDown, Commands.Down, "down")
        {
            AllowServer = false,
            HelpText = "Teleport you down through a layer of blocks."
        });
        Add(new Command(Permissions.TpLeft, Commands.Left, "left")
        {
            AllowServer = false,
            HelpText = "Teleport you left through a layer of blocks."
        });
        Add(new Command(Permissions.TpRight, Commands.Right, "right")
        {
            AllowServer = false,
            HelpText = "Teleport you right through a layer of blocks."
        });
        Add(new Command(Permissions.TpUp, Commands.Up, "up")
        {
            AllowServer = false,
            HelpText = "Teleport you up through a layer of blocks."
        });

        #endregion
    }

    private void OnPostInitialize(EventArgs args)
    {
        Homes = new HomeManager(Db);
    }

    private async void OnJoin(JoinEventArgs e)
    {
        var player = TShock.Players[e.Who];
        if (player == null)
        {
            return;
        }

        var muteExpiration = Mutes.GetExpiration(player);

        if (DateTime.UtcNow < muteExpiration)
        {
            player.mute = true;
            try
            {
                await Task.Delay(muteExpiration - DateTime.UtcNow, player.GetPlayerInfo().MuteToken);
                player.mute = false;
                player.SendInfoMessage("You have been unbanned.");
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private void OnGetData(GetDataEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        var tsplayer = TShock.Players[e.Msg.whoAmI];
        if (tsplayer == null)
        {
            return;
        }

        switch (e.MsgID)
        {
            #region Packet 118 - PlayerDeathV2

            case PacketTypes.PlayerDeathV2:
                if (tsplayer.Group.HasPermission(Permissions.TpBack))
                {
                    tsplayer.GetPlayerInfo().PushBackHistory(tsplayer.TPlayer.position);
                }
                return;

            case PacketTypes.Teleport:
                {
                    if (tsplayer.Group.HasPermission(Permissions.TpBack))
                    {
                        using (var ms = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            BitsByte flags = (byte)ms.ReadByte();

                            var type = 0;
                            if (flags[1])
                            {
                                type = 2;
                            }

                            if (type == 0 && tsplayer.Group.HasPermission(TShockAPI.Permissions.rod))
                            {
                                tsplayer.GetPlayerInfo().PushBackHistory(tsplayer.TPlayer.position);
                            }
                            else if (type == 2 && tsplayer.Group.HasPermission(TShockAPI.Permissions.wormhole))
                            {
                                tsplayer.GetPlayerInfo().PushBackHistory(tsplayer.TPlayer.position);
                            }
                        }
                    }
                }
                return;

                #endregion
        }
    }
}