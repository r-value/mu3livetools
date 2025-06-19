# MU3 Live Tools
[English](./README.md)
[简体中文](./README_chs.md)

A web overlay mod which exports some real-time gameplay information

Supported data and functions:

 + Rating analysis
   + Best 50 average
   + New 10 average, both raw average and the contribution to final Rating (which is 1/5 of the raw average for now)
   + Platinum Score best 50 average
   + Flashes red with Rating delta value in result scene when player's Rating raises
 + Overlay of currently selected music / chart
   + Music title, artist name, chart designer name
   + Chart difficulty name and chart constant
   + Music BPM
 + Performance analysis of currently selected music / chart
   + Best Technical Score (T-Score)
   + Best T-Score Rating[^1] of current chart
   + Best Platinum Score (P-Score), Stars and P-Score Rating[^2] of current chart
   + Total play count of previous sessions
   + Achieved Badges
   + Rank of current chart in your Rating list
 + Real time performance analysis during gameplay
   + Current T-Score in minus style[^3], including subscores for Notes and Bells
   + Current T-Score Rating, which coresponds to current T-Score and will-be-achieved Badges
   + Current P-Score, Stars and P-Score Rating in minus style
   + Play number and retry count of current session
   + Will-be-achieved Badges in current gameplay
   + Achieved Badges in previous records
   + Counters of all note judgements, including Fast and Late counters
   + Bell count in minus style
   + Current Damage count
   
## Demo

https://github.com/user-attachments/assets/baf57d73-8b9d-4679-ae08-bb2e4f881200

https://github.com/user-attachments/assets/650c7f36-503a-48a1-be22-9209c5188635

## Usage

This mod requires [BepInEx](https://github.com/BepInEx/BepInEx). Make sure you have BepInEx properly installed for your MU3 game.

Extract the released archive to your `BepInEx/plugins` folder. You can download the latest archive [here](https://github.com/r-value/mu3livetools/releases/latest).

In OBS or other live streaming applications, add a Browser Source with URL of `http://localhost:9715/default.html`, width of 400px, height of 700px. Note that currently the web overlay is not designed for various width/height.

If you haven't started your game, please start the game first and refresh the browser source after that, so that the mod can display information normally.

Install font `FOT-Seurat ProN` for a more familiar look of music title with original game.

## Supported Game Versions

Currently supports version `1.50` only. Not tested on version `1.51`. Version `1.45` and below are NOT supported.

## Build from source

Put game assembly `Assembly-CSharp.dll` to `lib/` and use `dotnet build`

## Known issues

 * This mod may crash the game if you have malformed option data installed with missing music jackets.
 * This mod relies on specific scene sequences to synchronize state, and scene-skipping mods may cause it to malfunction.

[^1]:Per-chart T-Score Rating is based on **Technical Score** and achieved **Badge bonuses**. Refer [here](https://silentblue.remywiki.com/ONGEKI:Rating#Score_rating_(Re:Fresh)) for further definitions.

[^2]:Per-chart P-Score Rating is based on **Stars count**. Refer [here](https://silentblue.remywiki.com/ONGEKI:Rating#Star_rating_(Re:Fresh)) for further definitions.

[^3]:Minus style is a scoring method that starts from the highest score and deducts corresponding points for mistakes.