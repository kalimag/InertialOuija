# InertialOuija

An unofficial mod for the game [InertialDrift] (Steam/Windows version only) to improve and expand ghost playback.

[InertialDrift]: https://store.steampowered.com/app/1184480/Inertial_Drift/

## Features

- Use any ghost you want in Time Attack
- Store an umlimited number of ghosts per track/car
- Store ghosts as individual, shareable files
- Ghosts for DLC car leaderboards
- No delayed ghosts for point-to-point races
- Use multiple ghosts at once

## Install

Download the [latest release] and extract into the game folder. `doorstop_config.ini`, `version.dll` and the
`InertialOuija` sub-folder must end up in the same folder as `InertialDrift.exe`.

If the mod is installed correctly, you will see the mod's version number in the bottom left corner of the main
menu.

This mod uses the [Doorstop] and [Harmony] libraries to inject code into the game without modifying game files.

[latest release]: https://github.com/kalimag/InertialOuija/releases
[Doorstop]: https://github.com/NeighTools/UnityDoorstop/
[Harmony]: https://github.com/pardeike/Harmony

## Usage

Press `F2` to open the ghost menu and configure which ghosts you want to use. Select *None* to use the game's
regular ghost behaviour.

> **Note** The mod chooses which ghosts to display at the start of the race. To use new ghosts or change the ghost
> options, you have to exit and restart the race.

After installing the mod, open the *Options* menu and select *Export Ghost Database* to make ghosts you previously
recorded available for use with the mod. You should only have to do this once.

If you have added or removed files in the ghost folder while the game is running, select *Refresh External Ghosts*.
