# MU3 Live Tools

A web overlay mod which exports some real-time gameplay information

Supported data:

 + Rating analysis
   + Best 50 average
   + New 10 average, both raw average and the contribution to final rating (which is 1/5 of the raw average for now)
   + PScore best 50 average
   + Flashes red with rating delta value in result scene when your rating raises
 + Overlay of currently selected music/fumen
   + Music title / artist name / notes designer name
   + Fumen difficulty name and fumen constant
   + Music BPM
 + Performance analysis of currently selected music/fumen
   + Current best tech score
   + Rating of current fumen
   + Platinum Score / Star / Rating of current fumen
   + Achieved badges
   + Rank of current fumen in your rating list
 + Real time performance analysis during gameplay
   + Current tech score in minus style
   + Rating coresponding to current tech score and badges in current gameplay
   + Platinum Score / Star / Rating in minus style
   + Achieved badges in current gameplay
   + Achieved badges in previous records
   + Counters of all note judgements with fast/late counter
   + Bell count in minus style
   + Current damage count
## Demo

https://github.com/user-attachments/assets/baf57d73-8b9d-4679-ae08-bb2e4f881200

https://github.com/user-attachments/assets/650c7f36-503a-48a1-be22-9209c5188635

## Usage

This mod requires [BepInEx](https://github.com/BepInEx/BepInEx). Make sure you have BepInEx installed for your MU3 game.

Extract the released archive to your `BepInEx/plugins` folder.

In OBS, add a Browser Source with URL of `http://localhost:9715/index.html`, width of 400px, height of 700px. Note that currently the web overlay is not designed for various width/height.

If you haven't started your game, start the game and refresh the browser source.

Install font `FOT-Seurat ProN` for a more familiar look of music title with original game.

## Supported Game Versions

Currently supports version `1.50` only. Not tested on version `1.51`. Version `1.45` and below are NOT supported.

## Build from source

Put game assembly `Assembly-CSharp.dll` to `lib/` and use `dotnet build`

## Known issues

 * This mod may crash the game if you have malformed option data installed with missing music jackets.
 * This mod relies on specific scene sequences to synchronize state, and scene-skipping mods may cause it to malfunction.
