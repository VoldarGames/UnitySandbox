using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GifCaptureManager : MonoBehaviour
{
    FileSystemWatcher watcher;
    CaptureJob currentCaptureJob;
    List<byte[]> pngs = new List<byte[]>();

    Texture2D colorBuffer;
    int lastDebounce;
    public CaptureJobsManager CaptureJobsManager;
    public bool Capturing { get; private set; }
    public RenderTexture MyRenderTexture;
    public int GifWidth = 256;
    public int GifHeight = 256;
    public AnimationController AnimationController;

    public void StartCapturing(CaptureJob captureJob)
    {
        Capturing = true;
        currentCaptureJob = captureJob;          
        pngs.Clear();

        AnimationController = AssetBundlesLoader.CaptureParents[currentCaptureJob.slotIndex].GetComponent<AnimationController>();
        AnimationController.AnimationFinished += HandleAnimationFinished;
        AnimationController.Init();        
        Logger.Log($"Start rendering job {currentCaptureJob.Guid} with settings {GifWidth}x{GifHeight} capture time {AnimationController.AnimationDuration}");
    }

    void HandleAnimationFinished()
    {
        StopCapturing();
        Encode();
        Logger.Log($"Render job {currentCaptureJob.Guid} finished!");
    }

    public void StopCapturing()
    {
        Capturing = false;
        AnimationController.AnimationFinished -= HandleAnimationFinished;
    }

    public void InitializeFileSystemWatcher()
    {
        watcher = new FileSystemWatcher();
        watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
        watcher.Path = ".";
        watcher.Filter = $"*.gif";        
        watcher.EnableRaisingEvents = true;
        watcher.Created += HandleGifFileChanged;
        watcher.Changed += HandleGifFileChanged;
    }

    void Start()
    {
        colorBuffer = new Texture2D(GifWidth, GifHeight, TextureFormat.RGB24, false);
        InitializeFileSystemWatcher();
    }

    void LateUpdate()
    {
        if (Capturing)
        {
            AnimationController.NextFrame();
            //Necessary for batchmode
            GetComponent<Camera>().Render();
            CaptureFrame();
        }
    }

    void CaptureFrame()
    {
        RenderTexture.active = MyRenderTexture;
        colorBuffer.ReadPixels(new Rect(0, 0, colorBuffer.width, colorBuffer.height), 0, 0, false);
        colorBuffer.Apply();
        pngs.Add(ImageConversion.EncodeToPNG(colorBuffer));
    }

    void OnDestroy()
    {
        if(watcher != null)
        {
            watcher.Created -= HandleGifFileChanged;
            watcher.Changed -= HandleGifFileChanged;
        }        
    }    

    void Encode()
    {
        Utils.CreateDirectory($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}");
        for (int i = 0; i < pngs.Count; i++)
        {
            File.WriteAllBytes($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}/{i}.png", pngs[i]);

            //Save static shield image
            
        }
        File.WriteAllBytes($"{currentCaptureJob.Guid}.png", pngs[pngs.Count - 1]);
        currentCaptureJob.CapturePngFilePath = $"{currentCaptureJob.Guid.ToString()}.png";

        System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
        ffmpeg.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
        ffmpeg.StartInfo.Arguments = "/c " + "cd " + Utils.ProjectPath + $" && ffmpeg\\ffmpeg.exe -i {Constants.Paths.Pngs}{currentCaptureJob.slotIndex}\\%d.png -f gif -y -loop -1 {currentCaptureJob.Guid.ToString()}.gif";
        Logger.Log($"Starting ffmpeg encoding for job {currentCaptureJob.Guid}");
        ffmpeg.Start();
    }

    void HandleGifFileChanged(object sender, FileSystemEventArgs args)
    {
        if (args.FullPath.EndsWith($"{currentCaptureJob.Guid.ToString()}.gif"))
        {
            Utils.Debounce(() =>
            {
                currentCaptureJob.CaptureGifFilePath = $"{currentCaptureJob.Guid.ToString()}.gif";
                currentCaptureJob.Status = CaptureJobStatus.Completed;
                CaptureJobsManager.FreeSlot(currentCaptureJob.slotIndex);
                Directory.Delete($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}", true);
                
            }, ref lastDebounce);
        }
    }
}
