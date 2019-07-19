# PocketTrap
SCP:SL Smod Plugin

* When other SCPs step on the portal, they can enter the Pocket Dimension.
* When SCPs leaving, it will return to the original portal position.
* When a human steps on it, they are sent to the Pocket Dimension in the same way as SCP-106's attack.

# Installation

Put PocketTrap.dll to sm_plugins.

# Config

Config Option | Value Type | Default Value | Description
--- | :---: | :---: | ---
ptrap_damage | Bool | true | Damaged when teleport.
ptrap_damage_amount | Int | 40 | Damage amount.
ptrap_ignored_scp035 | Bool | false | Whether to ignore SCP-035
ptrap_ignored_teams | Int List | Empty | List of TeamIDs that'll be ignored for trap.
ptrap_ignored_roles | Int List | Empty | List of RoleIDs that'll be ignored for trap.
ptrap_range | Float | 2.5 | Range of trap.
ptrap_cooltime | Float | 10.0 | Time to trap again to same player.
ptrap_animation | Bool | False | Add the suck animation. if cause problem, please set false.
