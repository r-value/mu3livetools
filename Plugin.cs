using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using MU3.User;
using MU3.DataStudio;
using MU3.Battle;
using MU3.Notes;
using WebSocketSharp.Server;
using Comio.BD15275;
using MU3;
using MU3.DB;
using UnityEngine;
using MU3.Util;
using System.Threading;
using MU3.TestMode;
namespace LiveStreamTool;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    static string listenAddr = "http://127.0.0.1:9715";

    static GameState stateExported;
    static GameState stateStashed;

    static AsyncState asyncState;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Start() {
        Harmony.CreateAndPatchAll(typeof(Plugin));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} Started!");


        stateExported = new GameState();
        stateStashed = new GameState();
        asyncState = new AsyncState(Logger);

        jacketService = new JacketsService(Logger);
        overlayService = new OverlayService(Logger);

        server = new HttpServer(listenAddr);
        server.OnGet += (sender, e) => {
            var req = e.Request;
            var res = e.Response;
            if(req.RawUrl.StartsWith("/jacket/")){
                jacketService.OnGet(req.RawUrl, res);
            }
            else{
                overlayService.OnGet(req.RawUrl, res);
            }
        };
        server.AddWebSocketService<StateService>("/state", ()=>{
            return new StateService(stateExported, Logger);
        });
        server.Start();
        Logger.LogInfo($"Started listening on {listenAddr}");
    }

    private void Stop() {
        server.Stop();
    }

    private void Update() {
        stateStashed.LoadFromAsyncState(asyncState);
        stateStashed.UpdateState(SingletonMonoBehaviour<GameEngine>.instance);
        if(!stateStashed.Equal(stateExported)){
            lock(stateExported){
                stateExported.CopyFrom(stateStashed);
                Monitor.Pulse(stateExported);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ResourceManager), "loadJacketImage", [typeof(int), typeof(ResourceManager.UIResourceSize)])]
    public static void LoadJacketImageHook(int musicId, ResourceManager.UIResourceSize size){
        // LAZY LOAD
        jacketService.LoadJacket(musicId);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TestModePage), "Awake")]
    public static void TestModePageAwakeHook(){
        asyncState.EndPlay();
        asyncState.ClearMusic();
        asyncState.ResetRating();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scene_39_Logout), "Awake")]
    public static void LogoutHook(){
        asyncState.ClearMusic();
        asyncState.ResetRating();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Battle.Counters), "addPlatinumScore")]
    public static void NoteJudgeHook(Judge judge, Timing timing){
        asyncState.addJudgement(judge, timing);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Battle.Counters), "addBellResult")]
    public static void BellResultHook(bool isHit){
        if(!isHit){
            asyncState.addDamageOrLostBell();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Battle.Counters), "addBulletDamage")]
    public static void BulletDamageHook(){
        asyncState.addDamageOrLostBell();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Battle.Counters), "reset")]
    public static void CounterResetHook(){
        asyncState.ResetCounter();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Notes.NotesManager), "startPlay")]
    public static void StartTryHook(){
        asyncState.StartTry();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.Battle.GameEngine), "playStart")]
    public static void StartPlayHook(){
        asyncState.StartPlay();
        Logger.LogInfo($"Play started");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameEngine), "applyResultToUserData")]
    public static void PlayFinishCheckpoint(){
        asyncState.EnterResult();
        Logger.LogInfo($"Music Play Finish Checkpoint");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MU3.Scene_38_End), "Init_Init")]
    public static void StopPlayHook(){
        asyncState.EndPlay();
        stateStashed.current = new CurrentScore();
        Logger.LogInfo($"Play finished");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MU3.AssetBundleDB), "initialize")]
    public static void AssetBundleInitializedHook(){
        jacketService.defaultLoads();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserNewRating), "calcRate")]
    public static void CalcRateHook(UserNewRating __instance){
        Logger.LogInfo($"CalcRateHook");
        var newList = (NewRatingList)AccessTools.Field(typeof(UserNewRating), "_bestNewList").GetValue(__instance);
        var bestList = (NewRatingList)AccessTools.Field(typeof(UserNewRating), "_bestOldList").GetValue(__instance);
        var platinumList = (NewRatingList)AccessTools.Field(typeof(UserNewRating), "_platinumScoreList").GetValue(__instance);
        int n10 = 0;
        foreach (NewRating newRating in newList)
        {
            n10 += newRating.rate1000;
        }
        int b50 = 0;
        foreach (NewRating newRating2 in bestList)
        {
            b50 += newRating2.rate1000;
        }
        int p50 = 0;
        foreach (NewRating newRating3 in platinumList)
        {
            p50 += newRating3.rate1000;
        }
        p50 /= 50;
        b50 /= 50;
        n10 /= 2;
        asyncState.SetRating(b50, n10, p50);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt), "onChangeMusicViewDataOrIsOnFocus")]
    [HarmonyPatch(typeof(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt), "updateDifficulty")]
    public static void MusicSelectHook(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt __instance){
        bool isOnFocus = (bool)AccessTools.Field(typeof(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt), "_isOnFocus").GetValue(__instance);
        if(!isOnFocus) return;
        if(__instance.musicViewData == null || __instance.isRandom){
            asyncState.ClearMusic();
            return;
        }

        MU3.SceneObject.MusicDataObject dataobj = AccessTools.Field(typeof(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt), "_musicDataObject").GetValue(__instance) as MU3.SceneObject.MusicDataObject;
        MU3.DataStudio.FumenDifficulty difficulty = (MU3.DataStudio.FumenDifficulty)AccessTools.Field(typeof(MU3.SceneObject.MusicSelect.ANM_SWH_MusicBt), "_difficulty").GetValue(__instance);
        MU3.Data.MusicData musicData = AccessTools.Field(typeof(MU3.SceneObject.MusicDataObject), "_musicData").GetValue(dataobj) as MU3.Data.MusicData;
        asyncState.SetMusic(musicData, difficulty, __instance.musicViewData);
    }

    static private JacketsService jacketService;
    static private OverlayService overlayService;
    private HttpServer server;
}
