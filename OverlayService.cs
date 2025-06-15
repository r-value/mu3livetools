using System;
using System.IO;
using WebSocketSharp.Net;
using BepInEx.Logging;
using UnityEngine;
namespace LiveStreamTool;

public class OverlayService
{
    string overlayPath;
    ManualLogSource logger;
    public OverlayService(ManualLogSource logger){
        this.logger = logger;
        overlayPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "mu3live-overlay");
        if(!Directory.Exists(overlayPath)){
            logger.LogError($"Overlay directory not found: {overlayPath}");
            return;
        }
        logger.LogInfo($"Overlay service initialized at {overlayPath}");
    }

    public void OnGet(string rawUrl, HttpListenerResponse res){
        string path = Path.Combine(overlayPath, rawUrl.Replace("/", "\\").Substring(1));
        if(!File.Exists(path)){
            logger.LogError($"Overlay file not found: {path}");
            res.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        // TODO: send overlay file and content type, maybe html, js, css, etc.
        try {
            string extension = Path.GetExtension(path).ToLower();
            string contentType = extension switch {
                ".html" => "text/html",
                ".js" => "application/javascript", 
                ".css" => "text/css",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            byte[] fileBytes = File.ReadAllBytes(path);
            res.ContentType = contentType;
            res.ContentLength64 = fileBytes.Length;
            res.StatusCode = (int)HttpStatusCode.OK;
            
            using (var writer = new BinaryWriter(res.OutputStream)) {
                writer.Write(fileBytes);
            }
        }
        catch (Exception e) {
            res.StatusCode = (int)HttpStatusCode.InternalServerError;
            logger.LogError($"Error sending overlay file: {e.Message}");
        }
        finally {
            res.Close();
        }
    }
}
