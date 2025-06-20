# MU3直播工具
[English](./README.md)
[简体中文](./README_chs.md)

一个可实时显示某代号MU3游戏若干游玩信息的mod

支持的数据和功能包括：

 + RATING分析
   + 历史最佳50曲（b50）平均值
   + 新版本最佳10曲（r10）平均值，包括原始平均值和对最终RATING的贡献量（目前为原始平均值的 1/5）
   + 白金分最佳50曲（p50）平均值
   + 当玩家RATING将要提升时，结算时RATING改变量会以红色字体着重标记
 + 显示当前选中的乐曲和谱面
   + 乐曲标题、艺术家名称、谱师名称
   + 谱面难度名称、定数
   + 乐曲每分钟节拍数（BPM）
 + 显示当前选中谱面的成绩分析
   + 最佳技术分（TECHNICAL SCORE）
   + 最佳技术RATING[^1]
   + 最佳白金分（PLATINUM SCORE）、最佳星级、最佳白金RATING[^1]
   + 之前的总游玩次数
   + 已获得的标记（包括FC/AP标记、FB标记和技术分评级标记）
   + 在玩家b50/r10/p50中的排名
 + 实时显示游玩中谱面的成绩分析
   + 当前技术分（类型-[^2]），包含NOTE子分数和BELL子分数
   + 当前技术RATING，受当前技术分和当前可获得标记的影响
   + 当前白金分、星级、白金RATING（均为类型-）
   + 当前游玩次数序号、本游玩过程中重试次数[^3]
   + 当前可获得的标记
   + 之前已获得的标记
   + 各个判定的计数，包含FAST和LATE计数
   + 铃铛（BELL）计数（类型-）
   + 当前受伤次数

## 示例

https://github.com/user-attachments/assets/baf57d73-8b9d-4679-ae08-bb2e4f881200

https://github.com/user-attachments/assets/650c7f36-503a-48a1-be22-9209c5188635

## 使用方法

这个mod的正常运行需要[BepInEx](https://github.com/BepInEx/BepInEx)。请确保您的MU3游戏已正确安装此工具。

请在[此处](https://github.com/r-value/mu3livetools/releases/latest)下载最新版压缩包，并解压至`BepInEx/plugins`文件夹下。

在OBS等直播软件的场景中，加入一个浏览器源，设置url为`http://localhost:9715/default.html`，宽为400px，高为700px。注意注意！目前该网页的设计不支持可变宽度和高度。

请务必打开游戏之后，再配置或刷新浏览器源，以保证数据的正常显示。

如有需求，可安装字体“FOT-Seurat ProN”，这可以使mod显示的字体与游戏里的更像。

## 支持游戏版本

目前仅支持版本`1.50`。尚未在版本`1.51`上进行测试。不支持`1.45`及以下的版本。

## 从源代码构建

将游戏的`Assembly-CSharp.dll`放到`lib/`，然后运行`dotnet build`命令。

## 已知问题

 * 如果您在option中安装了格式有误的数据（具体为乐曲封面文件缺失），这个mod可能会导致游戏崩溃。
 * 这个mod依赖特定的场景顺序来同步数据和状态。如果您同时安装了跳过场景的mod，可能会导致这个mod失灵。

[^1]:为帮助区分两种不同种类的RATING，本文档加入了不同的前缀以方便分辨。

[^2]:类型-，即从最高分开始计算，一旦失误则进行相应扣分。

[^3]:重试功能由另外的mod提供。