# EssentialsPlus

- Author: WhiteX et al., Average, Cjx, Gandi Xien adaptation and modification, Cai update, Keyou & Pace Kobo translator

- Source: [github](https://github.com/QuiCM/EssentialsPlus)

- Provide some management instructions

## Update log
```
1.0.2
Fix database error
1.0.1
Fix the bug that the muting can't be obtained after restart, rename some methods
1.0.2
Translate language to English
```

## Instructions ##

- **/find** or **/find** -> Contains multiple subcommands:
- **-command** or **-command** -> Search for a specific command based on the input, and return the matching command and its permissions.
- **-item** or **-item** -> Search for a specific item based on the input, and return the matching item and its ID.
- **-tile** or **-block** -> Search for a specific block based on the input, and return the matching block and its ID.
- **-wall** or **-wall** -> Searches for a specific wall based on the input, returns matching walls and their IDs.
- **/freezetime** or **/freezetime** -> Freezes or unfreezes time.
- **/delhome** or **/deletehome** <homename> -> Deletes one of your home points.
- **/sethome** or **/sethome** <homename> -> Sets one of your home points.
- **/myhome** or **/myhome** <homename> -> Teleports to one of your home points.
- **/kickall** or **/kickall** <reason> -> Kicks everyone on the server.
- **/=** or **/repeat** -> Repeats the last command you entered (excluding other iterations of /=).
- **/more** or **/stack** -> Maximizes the stacking of a handheld item. Subcommands:
- **-all** or **-all** -> Maximizes all stackable items in the player's backpack.
- **/mute** or **/MuteManager** -> Overrides TShock's /mute. Contains subcommands:
- **add** <name> <time> -> Adds a muted user named <name> for <time>.
- **delete** <name> -> Removes a muted user named <name>.
- **help** or **help** -> Outputs command information.
- **/pvpget2** or **/TogglePvPStatus** -> Toggles your PvP status.
- **/ruler** or **/MeasureTool** [1|2] -> Measure the distance between point 1 and point 2.
- **/send** or **/BroadcastMessage** -> Broadcasts a message in a custom color.
- **/sudo** or **/ExecuteOnDelegated** -> Attempts to have <player> execute <command>. Contains subcommands:
- **-force** -> Force command to run regardless of <player>'s permissions.
- **/timecmd** or **/timecmd** -> Execute command after a given time interval. Contains subcommands:
- **-repeat** -> Repeat <command> every <time>.
- **/eback** or **/b** or **/back** [number of steps] -> Brings you back to the previous position. If [number of steps] is given, attempts to bring you back [number of steps] steps ago.
- **/down** or **/down** [number of levels] -> Attempts to move your position down on the map. If [number of levels] is given, attempts to move down [number of levels] times.
- **/left** or **/left** [number of levels] -> Same as /down [number of levels], but moves left.
- **/right** or **/右** [level] -> Same as /down [level], but moves right.
- **/up** or **/上** [level] -> Same as /down [level], but moves up.

## Permissions ##

- essentials.find -> Allows the use of the /find command.
- essentials.freezetime -> Allows the use of the /freezetime command.
- essentials.home.delete -> Allows the use of the /delhome and /sethome commands.
- essentials.home.tp -> Allows the use of the /myhome command.
- essentials.kickall -> Allows the use of the /kickall command.
- essentials.lastcommand -> Allows the use of the /= command.
- essentials.more -> Allows the use of the /more command.
- essentials.mute -> Allows the use of the /mute command.
- essentials.pvp -> Allow the use of the /pvpget2 command.
- essentials.ruler -> Allow the use of the /ruler command.
- essentials.send -> Allow the use of the /send command.
- essentials.sudo -> Allow the use of the /sudo command.
- essentials.timecmd -> Allow the use of the /timecmd command.
- essentials.tp.eback -> Allow the use of the /eback command.
- essentials.tp.down -> Allow the use of the /down command.
- essentials.tp.left -> Allow the use of the /left command.
- essentials.tp.right -> Allow the use of the /right command.
- essentials.tp.up -> Allow the use of the /up command.

## Configuration
> Configuration file location: tshock/EssentialsPlus.json
```json
{
    "Pvp disable command": [
        "eback"
    ],
    "Backward location history": 10,
    "MySql host": "If you use Mysql, you need to configure complete information here",
    "MySql database name": "",
    "MySql user": "",
    "MySql password": ""
}
```