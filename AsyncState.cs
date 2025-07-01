using System;
using System.Collections.Generic;
using HarmonyLib;
using MU3.Data;
using MU3.Notes;
using MU3.User;
using MU3.Util;
using BepInEx.Logging;
namespace LiveStreamTool;

public class AsyncState
{
    internal ManualLogSource Logger;
    public int musicId;
    public int difficultyId;
    public int musicBPM;
    public int playCount;
    public int retryCount;
    public float musicLevel;
    public string musicTitle;
    public string musicArtist;
    public bool isInPlay;
    public bool isInResult;
    public bool isMusicSelected;
    public int platinumScoreMax;
    public int platinumScoreLost;
    public int[][] judgeCount;
    public UserFumen userFumen;
    public bool isCurrentVersion;
    public int bestRank;
    public int platinumRank;
    public string designer;

    public int b50_1000;
    public int n10_1000;
    public int p50_1000;
    public int b50_1000_delta;
    public int n10_1000_delta;
    public int n10_raw_1000_delta;
    public int p50_1000_delta;
    private bool isRatingSet;

    public AsyncState(ManualLogSource logger){
        Logger = logger;
        judgeCount = new int[][]{
            new int[3],
            new int[3],
            new int[3],
            new int[3],
        };
        ResetCounter();
        ClearMusic();
        // [0]:M [1]:H [2]:B [3]:CB
        // [0]:sum [1]:early [2]:late
    }

    public void SetRating(int b50_1000, int n10_1000, int p50_1000){
        if(isRatingSet){
            b50_1000_delta = b50_1000 - this.b50_1000;
            n10_1000_delta = n10_1000 - this.n10_1000;
            int n10_raw_1000 = n10_1000 / 5;
            int n10_raw_1000_old = this.n10_1000 / 5;
            n10_raw_1000_delta = n10_raw_1000 - n10_raw_1000_old;
            p50_1000_delta = p50_1000 - this.p50_1000;
        }
        this.b50_1000 = b50_1000;
        this.n10_1000 = n10_1000;
        this.p50_1000 = p50_1000;
        isRatingSet = true;
    }

    public void ResetRating(){
        b50_1000 = 0;
        n10_1000 = 0;
        p50_1000 = 0;
        b50_1000_delta = 0;
        n10_1000_delta = 0;
        n10_raw_1000_delta = 0;
        p50_1000_delta = 0;
        isRatingSet = false;
    }

    public void SetMusic(MU3.Data.MusicData musicData,
                         MU3.DataStudio.FumenDifficulty difficulty,
                         MU3.ViewData.MusicViewData viewData){
        musicId = musicData.id;
        difficultyId = (int)difficulty;
        musicTitle = musicData.name;
        musicArtist = musicData.artistName;
        isCurrentVersion = musicData.isCurrentVersion;
        isMusicSelected = true;
        designer = musicData.fumenData[difficultyId].notesDesignerName;
        if(designer == ""){
            designer = "N/A";
        }
        userFumen = viewData.userFumen[difficultyId];
        if(Singleton<UserManager>.instance.getUserFumen(musicId, difficulty, false) == null){
            playCount = 0;
        }
        else{
            playCount = userFumen.PlayCount;
        }
        FumenData[] fumenData = (FumenData[])AccessTools.Field(typeof(MU3.Data.MusicData), "_fumenData").GetValue(musicData);
        musicLevel = fumenData[difficultyId].fumenConst;
        musicBPM = fumenData[difficultyId].bpm;
        platinumScoreMax = fumenData[difficultyId].platinumScoreMax;

        bestRank = 0;
        platinumRank = 0;
        UserManager userMgr = Singleton<UserManager>.instance;
        List<UserFumenScore> ratingList;
        List<UserFumenScore> platinumList;
        if(isCurrentVersion){
            ratingList = userMgr.userNewRatingBaseBestNewList;
        }
        else{
            ratingList = userMgr.userNewRatingBaseBestList;
        }
        platinumList = userMgr.userNewRatingBasePScoreList;
        for(int i=0;i<ratingList.Count;i++){
            if(ratingList[i].musicId == musicId && ratingList[i].difficultId == difficulty){
                bestRank = i+1;
                break;
            }
        }
        for(int i=0;i<platinumList.Count;i++){
            if(platinumList[i].musicId == musicId && platinumList[i].difficultId == difficulty){
                platinumRank = i+1;
                break;
            }
        }
    }

    public void StartPlay() {
        isInPlay = true;
    }

    public void StartTry() {
        isInPlay = true;
        ++retryCount;
    }

    public void EnterResult() {
        isInResult = true;
    }

    public void EndPlay() {
        isInPlay = false;
        isInResult = false;
        retryCount = 0;
        ResetCounter();
    }

    public void addJudgement(Judge judge, Timing timing){
        ++judgeCount[(int)judge][0];
        switch(timing){
        case Timing.Fast:
            ++judgeCount[(int)judge][1];
            break;
        case Timing.Late:
            ++judgeCount[(int)judge][2];
            break;
        }
        if(judge == Judge.Perfect){
            if(timing == Timing.Fast || timing == Timing.Late){
                ++platinumScoreLost;
            }
        }
        else {
            platinumScoreLost += 2;
        }
    }

    public void addDamageOrLostBell() {
        platinumScoreLost += 2;
    }

    public void ClearMusic(){
        musicId = 0;
        difficultyId = -1;
        musicTitle = "N/A";
        musicArtist = "N/A";
        designer = "N/A";
        isMusicSelected = false;
        userFumen = null;
        playCount = 0;
        musicLevel = 0;
        musicBPM = 0;
        platinumScoreMax = 0;
        bestRank = 0;
        platinumRank = 0;
        isCurrentVersion = false;
        retryCount = 0;
    }

    public void ResetCounter() {
        for(int i=0;i<4;i++){
            for(int j=0;j<3;j++){
                judgeCount[i][j]=0;
            }
        }
        platinumScoreLost = 0;
    }
}
