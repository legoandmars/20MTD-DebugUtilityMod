# Debug Utility Mod
A [BepInEx](https://github.com/BepInEx/BepInEx/releases) plugin for [20 Minutes Till Dawn](https://store.steampowered.com/app/1966900/20_Minutes_Till_Dawn/).

## Features

Provides support for modders with some quality of life configurable tweak that make debugging process easier.

Modders can change config values via the BepInEx config file : BepInEx/config/kaios.mod.debugutility

### Configurable feature
- XPPatch       : Fast level up and max level
- FastGame      : Reach endgame faster
- Invincibility : Avoid any damage
- GunPatch      : Infinite Ammo
- RerollPatch   : Every character has infinite reroll
- EnemyPatch    : Elite and bosses are weak

### No Pain, No Gain

To avoid currupting save file during test runs, this mod prevents
- Unlocking new darkness level
- Unlocking new achievements
- Gaining soul currency

## For modders

- Clone the project
- Open repo in VSCode
- Setup $GameDir variable in *DebugUtilityMod.csproj*
- ```dotnet build``` to build and deploy mod
- ```dotnet publish``` to publish a .zip file
