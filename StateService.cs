using System;
using System.Threading;
using BepInEx.Logging;
using MU3.DB;
using UnityEngine;
using WebSocketSharp.Server;
using WebSocketSharp;
using LitJson;
namespace LiveStreamTool;

public class StateService: WebSocketBehavior
{
    public StateService(GameState state, ManualLogSource logger){
        this.state = state;
        this.logger = logger;
        this.watcher = new Thread(() => { this.ServiceLoop(); });
        watcher.Start();
    }

    public void ServiceLoop(){
        while(true){
            string serialized;
            lock (state) {
                Monitor.Wait(state);
                serialized = JsonMapper.ToJson(state);
            }
            Sessions.Broadcast(serialized);
        }
    }

    protected override void OnMessage(MessageEventArgs e){
        if(e.Data == "tsumugi on air"){
            string serialized;
            lock (state) {
                serialized = JsonMapper.ToJson(state);
            }
            Send(serialized);
        }
        else{
            logger.LogError($"Unknown message: {e.Data}");
        }
    }

    GameState state;
    Thread watcher;
    ManualLogSource logger;
}
