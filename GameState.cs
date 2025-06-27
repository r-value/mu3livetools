using System;
using System.CodeDom;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using MU3.Battle;
using MU3.DataStudio;
using MU3.DB;
using MU3.Notes;
using MU3.User;
using UnityEngine;

namespace LiveStreamTool;

public class Judgements
{
    public int count;
    public int early;
    public int late;

    public Judgements(){
        count = 0;
        early = 0;
        late = 0;
    }

    public void CopyFrom(Judgements rhs){
        count = rhs.count;
        early = rhs.early;
        late = rhs.late;
    }

    public bool Equal(Judgements rhs){
        return count == rhs.count &&
            early == rhs.early &&
            late == rhs.late;
    }
}

public class Score{
    public int techScore;
    public int platinumScore;
    public int platinumStar;
    public bool isAllBreakPlus;
    public bool isAllBreak;
    public bool isFullCombo;
    public bool isFullBell;
    public float rating;
    public int platinumRating;
    public int toNextPlatinum;
    public int fromLastPlatinum;
    public string technicalRank;

    public Score(){
        Reset();
    }

    public void Reset(){
        techScore = 0;
        platinumScore = 0;
        rating = 0;
        platinumStar = 0;
        toNextPlatinum = 0;
        fromLastPlatinum = 0;
        isAllBreakPlus = false;
        isAllBreak = false;
        isFullCombo = false;
        isFullBell = false;
        platinumRating = 0;
        technicalRank = "";
    }

    public bool Equal(Score rhs){
        return techScore == rhs.techScore &&
            platinumScore == rhs.platinumScore &&
            rating == rhs.rating &&
            platinumStar == rhs.platinumStar &&
            platinumRating == rhs.platinumRating &&
            isAllBreakPlus == rhs.isAllBreakPlus &&
            isAllBreak == rhs.isAllBreak &&
            isFullCombo == rhs.isFullCombo &&
            isFullBell == rhs.isFullBell &&
            toNextPlatinum == rhs.toNextPlatinum &&
            fromLastPlatinum == rhs.fromLastPlatinum &&
            technicalRank == rhs.technicalRank;
    }

    public void CopyFrom(Score rhs){
        techScore = rhs.techScore;
        platinumScore = rhs.platinumScore;
        rating = rhs.rating;
        platinumStar = rhs.platinumStar;
        platinumRating = rhs.platinumRating;
        isAllBreakPlus = rhs.isAllBreakPlus;
        isAllBreak = rhs.isAllBreak;
        isFullCombo = rhs.isFullCombo;
        isFullBell = rhs.isFullBell;
        toNextPlatinum = rhs.toNextPlatinum;
        fromLastPlatinum = rhs.fromLastPlatinum;
        technicalRank = rhs.technicalRank;
    }
    private int rating1000(int level1000, NewRating.Const.RateTbl rate, NewRating.Const.RateTbl nextRate){
        if(level1000 == 0) return 0;
        NewRating.ClearMarkType key = isAllBreakPlus ? NewRating.ClearMarkType.AllBreakPLUS : (isAllBreak ? NewRating.ClearMarkType.AllBreak : (isFullCombo ? NewRating.ClearMarkType.FullCombo : NewRating.ClearMarkType.None));
        NewRating.FullBellType key2 = isFullBell ? NewRating.FullBellType.FullBell : NewRating.FullBellType.None;
        int num = NewRating.Const.clearMarkRateTbl[key];
        int num2 = NewRating.Const.fullBellRateTbl[key2];
        int num3 = 0;
        foreach (TechnicalRankID technicalRankID in NewRating.Const.teckRankRateTbl.Keys)
        {
            if (techScore >= technicalRankID.getLower())
            {
                num3 = NewRating.Const.teckRankRateTbl[technicalRankID];
            }
        }
        int num4 = level1000 + rate.bonus1000;
        num4 += (nextRate.bonus1000 - rate.bonus1000) * (techScore - rate.score) / (nextRate.score - rate.score);
        return num4 + (num + num3 + num2);
    }

    public void CalcPlatinumDelta(int max){
        toNextPlatinum = ((94 + platinumStar) * max + 99) / 100 - platinumScore;
        if(platinumStar == 0) {
            fromLastPlatinum = 0;
        }
        else{
            fromLastPlatinum = platinumScore - ((93 + platinumStar) * max + 99) / 100;
        }
    }

    public void UpdateRating(double level) {
        platinumRating = (int)(Mathf.Clamp(platinumStar, 0, 5) * level * level);
        if(techScore <= 500000) {
            rating = 0;
            return;
        }
        int level1000 = (int)(level * 1000 + 0.5f);
        if (techScore <= NewRating.Const.rateTblMin.score)
        {
            rating = (level1000 + NewRating.Const.rateTblMin.bonus1000) * (techScore - 500000) / (NewRating.Const.rateTblMin.score - 500000) / 1000.0f;
        }
        else
        {
            for (int i = 1; i < 7; i++)
            {
                NewRating.Const.RateTbl rateTbl = NewRating.Const.rateTbl[i];
                if (techScore <= rateTbl.score)
                {
                    NewRating.Const.RateTbl rate = NewRating.Const.rateTbl[i - 1];
                    rating = rating1000(level1000, rate, rateTbl) / 1000.0f;
                    break;
                }
            }
        }
    }

    public void CalcTechnicalRank(){
        technicalRank = "";
        for(TechnicalRankID i = TechnicalRankID.SSS1; i >= TechnicalRankID.S; i--){
            if(techScore >= i.getLower()){
                technicalRank = i.getName();
                break;
            }
        }
    }
}

public class BestScore : Score {
    public int bestRank;
    public int platinumRank;
    public void LoadFrom(AsyncState state){
        if(state.userFumen == null) {
            Reset();
            bestRank = 0;
            platinumRank = 0;
            return;
        }
        bestRank = state.bestRank;
        platinumRank = state.platinumRank;
        techScore = state.userFumen.TechScoreMax;
        platinumScore = state.userFumen.PlatinumScoreMax;
        isAllBreakPlus = state.userFumen.TechScoreMax >= 1010000;
        isAllBreak = state.userFumen.IsAllBreak;
        isFullCombo = state.userFumen.IsFullCombo;
        isFullBell = state.userFumen.IsFullBell;
        platinumStar = (int)state.userFumen.PlatinumScoreRank;
    }
    public void CopyFrom(BestScore rhs){
        base.CopyFrom(rhs);
        bestRank = rhs.bestRank;
        platinumRank = rhs.platinumRank;
    }
    public bool Equal(BestScore rhs){
        return base.Equal(rhs) &&
            bestRank == rhs.bestRank &&
            platinumRank == rhs.platinumRank;
    }
}

public class CurrentScore : Score
{
    public long bellLost;
    public long totalBell;
    public long totalNote;
    public long scoreNote;
    public long scoreBell;
    public int damage;
    public void CalcPlatinumStar(int max){
        platinumStar = 0;
        for(int i = 1; i <= 6; i++){
            if(platinumScore >= ((93 + i) * max + 99) / 100){
                platinumStar = i;
            }
            else{
                break;
            }
        }
    }

    public void LoadFromGame(GameEngine gameEngine, GameState gameState){
        if(gameEngine == null) return;

        techScore = (int)(1010000L - (long)Reflections._techScoreLostField.GetValue(gameEngine.counters));
        totalBell = (long)Reflections._tsjBellTotalField.GetValue(gameEngine.counters);
        totalNote = (long)Reflections._tsjNoteTotalField.GetValue(gameEngine.counters) / 10L;
        scoreNote = 950000L - (long)Reflections._techScoreNoteLostField.GetValue(gameEngine.counters);
        scoreBell = 60000L - (long)Reflections._techScoreBellLostField.GetValue(gameEngine.counters);
        bellLost = (long)Reflections._tsjBellLostField.GetValue(gameEngine.counters);
        platinumScore = gameState.platinumScoreMax - gameState.platinumScoreLost;
        if(platinumScore < 0){
            platinumScore = 0;
        }
        int[] scoreCounters = (int[])Reflections._scoresField.GetValue(gameEngine.counters);
        damage = scoreCounters[(int)MU3.Battle.ScoreType.BulletHitCount];
        isFullBell = bellLost == 0;
        isFullCombo = scoreCounters[(int)MU3.Battle.ScoreType.Miss] == 0;
        isAllBreak = isFullCombo && scoreCounters[(int)MU3.Battle.ScoreType.Hit] == 0;
        isAllBreakPlus = techScore >= 1010000L;
    }

    public void CopyFrom(CurrentScore rhs){
        base.CopyFrom(rhs);
        bellLost = rhs.bellLost;
        totalBell = rhs.totalBell;
        totalNote = rhs.totalNote;
        scoreNote = rhs.scoreNote;
        scoreBell = rhs.scoreBell;
        damage = rhs.damage;
    }

    public bool Equal(CurrentScore rhs){
        return base.Equal(rhs) &&
            bellLost == rhs.bellLost &&
            totalBell == rhs.totalBell &&
            totalNote == rhs.totalNote &&
            scoreNote == rhs.scoreNote &&
            scoreBell == rhs.scoreBell &&
            damage == rhs.damage;
    }
}

public static class Reflections{
    public static readonly FieldInfo _techScoreLostField;
    public static readonly FieldInfo _tsjBellTotalField;
    public static readonly FieldInfo _tsjNoteTotalField;
    public static readonly FieldInfo _techScoreNoteLostField;
    public static readonly FieldInfo _techScoreBellLostField;
    public static readonly FieldInfo _tsjBellLostField;
    public static readonly FieldInfo _scoresField;

    static Reflections()
    {
        _techScoreLostField = AccessTools.Field(typeof(MU3.Battle.Counters), "_techScoreLost");
        _tsjBellTotalField = AccessTools.Field(typeof(MU3.Battle.Counters), "_tsjBellTotal");
        _tsjNoteTotalField = AccessTools.Field(typeof(MU3.Battle.Counters), "_tsjNoteTotal");
        _techScoreNoteLostField = AccessTools.Field(typeof(MU3.Battle.Counters), "_techScoreNoteLost");
        _techScoreBellLostField = AccessTools.Field(typeof(MU3.Battle.Counters), "_techScoreBellLost");
        _tsjBellLostField = AccessTools.Field(typeof(MU3.Battle.Counters), "_tsjBellLost");
        _scoresField = AccessTools.Field(typeof(MU3.Battle.Counters), "_scores");
    }
}

public class GameState
{
    // ============Async============
    public bool isInGame;
    public bool isMusicSelected;
    public bool isInResult;
    public int musicId;
    public bool isCurrentVersion;
    public string musicTitle;
    public double musicLevel;
    public int playCount;
    public int retryCount;
    public int musicBPM;
    public string designer;
    public string musicArtist;
    public string difficulty;
    public Judgements judgeCrBreak;
    public Judgements judgeBreak;
    public Judgements judgeHit;
    public int judgeMiss;
    public BestScore best;
    public int platinumScoreMax;
    public int platinumScoreLost;
    public float b50;
    public float n10;
    public float p50;
    public float n10_raw;
    public float b50_delta;
    public float n10_delta;
    public float p50_delta;
    public float n10_raw_delta;
    // ============Loaded============
    public CurrentScore current;

    public GameState() {
        Reset();
    }

    public void UpdateState(GameEngine gameEngine){
        if(isInGame){
            current.LoadFromGame(gameEngine, this);
        }
        Calculate();
    }

    private void Calculate(){
        if(isInGame){
            current.CalcPlatinumStar(platinumScoreMax);
            current.UpdateRating(musicLevel);
            current.CalcPlatinumDelta(platinumScoreMax);
            current.CalcTechnicalRank();
        }
        else{
            best.UpdateRating(musicLevel);
            best.CalcPlatinumDelta(platinumScoreMax);
            best.CalcTechnicalRank();
        }
    }

    public void LoadFromAsyncState(AsyncState state){
        musicId = state.musicId;
        musicTitle = state.musicTitle;
        musicArtist = state.musicArtist;
        designer = state.designer;
        isInGame = state.isInPlay;
        isMusicSelected = state.isMusicSelected;
        musicBPM = state.musicBPM;
        musicLevel = state.musicLevel;
        platinumScoreMax = state.platinumScoreMax;
        platinumScoreLost = state.platinumScoreLost;
        isCurrentVersion = state.isCurrentVersion;
        isInResult = state.isInResult;
        playCount = state.playCount;
        retryCount = state.retryCount < 1 ? 1 : state.retryCount;
        b50 = state.b50_1000 / 1000.0f;
        n10 = state.n10_1000 / 1000.0f;
        p50 = state.p50_1000 / 1000.0f;
        n10_raw = state.n10_1000 / 5 / 1000.0f;
        b50_delta = state.b50_1000_delta / 1000.0f;
        n10_delta = state.n10_1000_delta / 1000.0f;
        p50_delta = state.p50_1000_delta / 1000.0f;
        n10_raw_delta = state.n10_raw_1000_delta / 1000.0f;

        switch(state.difficultyId) {
            case 0:
                difficulty = "BASIC";
                break;
            case 1: 
                difficulty = "ADVANCED";
                break;
            case 2:
                difficulty = "EXPERT";
                break;
            case 3:
                difficulty = "MASTER";
                break;
            case 4:
                difficulty = "LUNATIC";
                break;
            default:
                difficulty = "NONE";
                break;
        }

        if (!isInGame) best.LoadFrom(state);

        judgeCrBreak.count = state.judgeCount[3][0];
        judgeCrBreak.early = state.judgeCount[3][1];
        judgeCrBreak.late = state.judgeCount[3][2];
        judgeBreak.count = state.judgeCount[2][0];
        judgeBreak.early = state.judgeCount[2][1];
        judgeBreak.late = state.judgeCount[2][2];
        judgeHit.count = state.judgeCount[1][0];
        judgeHit.early = state.judgeCount[1][1];
        judgeHit.late = state.judgeCount[1][2];
        judgeMiss = state.judgeCount[0][0];
    }

    public bool Equal(GameState rhs){
        return isInGame == rhs.isInGame &&
            isMusicSelected == rhs.isMusicSelected &&
            isInResult == rhs.isInResult &&
            musicId == rhs.musicId &&
            musicTitle == rhs.musicTitle &&
            musicArtist == rhs.musicArtist &&
            designer == rhs.designer &&
            musicLevel == rhs.musicLevel &&
            musicBPM == rhs.musicBPM &&
            difficulty == rhs.difficulty &&
            best.Equal(rhs.best) &&
            current.Equal(rhs.current) &&
            judgeCrBreak.Equal(rhs.judgeCrBreak) &&
            judgeBreak.Equal(rhs.judgeBreak) &&
            judgeHit.Equal(rhs.judgeHit) &&
            judgeMiss == rhs.judgeMiss &&
            platinumScoreMax == rhs.platinumScoreMax &&
            platinumScoreLost == rhs.platinumScoreLost &&
            playCount == rhs.playCount &&
            retryCount == rhs.retryCount &&
            b50 == rhs.b50 &&
            n10 == rhs.n10 &&
            p50 == rhs.p50 &&
            n10_raw == rhs.n10_raw &&
            b50_delta == rhs.b50_delta &&
            n10_delta == rhs.n10_delta &&
            p50_delta == rhs.p50_delta &&
            n10_raw_delta == rhs.n10_raw_delta &&
            isCurrentVersion == rhs.isCurrentVersion;
    }

    public void Reset() {
        isInGame = false;
        isMusicSelected = false;
        isInResult = false;
        musicId = 0;
        musicTitle = "N/A";
        musicArtist = "N/A";
        designer = "N/A";
        musicLevel = 0;
        musicBPM = 0;
        difficulty = "NONE";
        best = new BestScore();
        current = new CurrentScore();
        platinumScoreMax = 0;
        platinumScoreLost = 0;
        playCount = 0;
        retryCount = 0;
        judgeCrBreak = new Judgements();
        judgeBreak = new Judgements();
        judgeHit = new Judgements();
        judgeMiss = 0;
        isCurrentVersion = false;
        p50 = 0;
        n10 = 0;
        b50 = 0;
        n10_raw = 0;
        b50_delta = 0;
        n10_delta = 0;
        p50_delta = 0;
        n10_raw_delta = 0;
    }

    public void CopyFrom(GameState rhs) {
        isInGame = rhs.isInGame;
        isMusicSelected = rhs.isMusicSelected;
        isInResult = rhs.isInResult;
        musicId = rhs.musicId;
        musicTitle = rhs.musicTitle;
        musicLevel = rhs.musicLevel;
        musicBPM = rhs.musicBPM;
        musicArtist = rhs.musicArtist;
        designer = rhs.designer;
        difficulty = rhs.difficulty;
        best.CopyFrom(rhs.best);
        current.CopyFrom(rhs.current);
        platinumScoreMax = rhs.platinumScoreMax;
        platinumScoreLost = rhs.platinumScoreLost;
        playCount = rhs.playCount;
        judgeCrBreak.CopyFrom(rhs.judgeCrBreak);
        judgeBreak.CopyFrom(rhs.judgeBreak);
        judgeHit.CopyFrom(rhs.judgeHit);
        judgeMiss = rhs.judgeMiss;
        isCurrentVersion = rhs.isCurrentVersion;
        retryCount = rhs.retryCount;
        b50 = rhs.b50;
        n10 = rhs.n10;
        p50 = rhs.p50;
        n10_raw = rhs.n10_raw;
        b50_delta = rhs.b50_delta;
        n10_delta = rhs.n10_delta;
        p50_delta = rhs.p50_delta;
        n10_raw_delta = rhs.n10_raw_delta;
    }
}
