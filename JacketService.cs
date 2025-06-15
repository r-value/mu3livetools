using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using MU3;
using MU3.DB;
using UnityEngine;
using WebSocketSharp.Net;

namespace LiveStreamTool;

public class JacketsService
{
    public JacketsService(ManualLogSource logger){
        loadUIResource = AccessTools.Method(typeof(ResourceManager), "loadUIResource", [typeof(UiResourceKindID), typeof(int), typeof(ResourceManager.UIResourceSize)]);
        this.logger = logger;
        jacketsPNG = new Dictionary<int, byte[]>();
    }

    // NOTE: hook to a relatively early and rarely executed method
    public void defaultLoads() {
        if(defaultLoaded) return;
        {  // default jacket
            Texture2D texture;
            texture = (Texture2D)loadUIResource.Invoke(null, [UiResourceKindID.Jacket,0, ResourceManager.UIResourceSize.S]);
            jacketsPNG[0] = textureToPNG(texture);
            texture = (Texture2D)loadUIResource.Invoke(null, [UiResourceKindID.Jacket,1, ResourceManager.UIResourceSize.S]);
            jacketsPNG[1] = textureToPNG(texture);
            logger.LogInfo("Loaded default jacket");
        }
        defaultLoaded = true;
    }

    public void LoadJacket(int musicId){
        if(jacketsPNG.ContainsKey(musicId)) return;
        Texture2D texture = (Texture2D)loadUIResource.Invoke(null, [UiResourceKindID.Jacket, musicId, ResourceManager.UIResourceSize.S]);
        jacketsPNG[musicId] = textureToPNG(texture);
    }

    private byte[] textureToPNG(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText.EncodeToPNG();
    }

    public void OnGet(string resType, HttpListenerResponse res){
        try{
            byte[] png = [];
            if(resType.StartsWith("/jacket/")){
                int musicId = int.Parse(resType.Substring(8));
                if(!jacketsPNG.ContainsKey(musicId)){
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                png = jacketsPNG[musicId];
            }
            else{
                res.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            res.ContentType = "image/png";
            res.StatusCode = (int)HttpStatusCode.OK;
            res.ContentLength64 = png.Length;
            using (var writer = new BinaryWriter(res.OutputStream)) {
                writer.Write(png);
            }
        }
        catch (Exception e){
            res.StatusCode = (int)HttpStatusCode.InternalServerError;
            logger.LogError(e.Message);
        }
        finally{
            res.Close();
        }
    }

    MethodInfo loadUIResource;

    bool defaultLoaded = false;

    ManualLogSource logger;
    
    Dictionary<int, byte[]> jacketsPNG;
}
