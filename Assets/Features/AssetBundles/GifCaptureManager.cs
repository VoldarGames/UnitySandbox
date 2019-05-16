using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GifCaptureManager : MonoBehaviour
{
    FileSystemWatcher watcher;
    CaptureJob currentCaptureJob;
    List<byte[]> pngs = new List<byte[]>();
    float T = 0;
    float startTime = 0;
    float period;
    Texture2D colorBuffer;
    int lastDebounce;
    public CaptureJobsManager CaptureJobsManager;
    public bool Capturing { get; private set; }
    public float CaptureTime = 10;    
    public float FrameRate = 15;
    public RenderTexture MyRenderTexture;
    public int GifWidth = 256;
    public int GifHeight = 256;

    public void StartCapturing(CaptureJob captureJob)
    {
        Capturing = true;
        currentCaptureJob = captureJob;          
        pngs.Clear();
        RenderTexture.active = MyRenderTexture;
        colorBuffer.ReadPixels(new Rect(0, 0, colorBuffer.width, colorBuffer.height), 0, 0);
        colorBuffer.Apply();
        startTime = Time.time;
        Debug.Log("Start rendering camera GIF");
    }

    public void StopCapturing()
    {
        Capturing = false;        
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
        period = 1f / FrameRate;
        colorBuffer = new Texture2D(GifWidth, GifHeight, TextureFormat.RGB24, false);
        InitializeFileSystemWatcher();
    }

    void LateUpdate()
    {
        if (Capturing)
        {
            //Necessary for batchmode
            GetComponent<Camera>().Render();
        }
    }

    void OnDestroy()
    {
        if(watcher != null)
        {
            watcher.Created -= HandleGifFileChanged;
            watcher.Changed -= HandleGifFileChanged;
        }        
    }

    void OnPostRender()
    {
        if (Capturing)
        {
            T += Time.deltaTime;
            if (T >= period)
            {
                T = 0;
                RenderTexture.active = MyRenderTexture;
                colorBuffer.ReadPixels(new Rect(0, 0, colorBuffer.width, colorBuffer.height), 0, 0, false);
                colorBuffer.Apply();
                pngs.Add(ImageConversion.EncodeToPNG(colorBuffer));
            }
            if (Time.time > (startTime + CaptureTime))
            {
                StopCapturing();
                Encode();
                Debug.Log("Render Finished!");
            }
        }
    }

    void Encode()
    {
        Utils.CreateDirectory($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}");
        for (int i = 0; i < pngs.Count; i++)
        {
            File.WriteAllBytes($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}/{i}.png", pngs[i]);
        }

        System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
        ffmpeg.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
        ffmpeg.StartInfo.Arguments = "/c " + "cd " + Utils.ProjectPath + $" && del out.gif && ffmpeg\\ffmpeg.exe -i {Constants.Paths.Pngs}{currentCaptureJob.slotIndex}\\%d.png -f gif -y {currentCaptureJob.Guid.ToString()}.gif";
        ffmpeg.Start();
    }

    void HandleGifFileChanged(object sender, FileSystemEventArgs args)
    {
        if (args.FullPath.EndsWith($"{currentCaptureJob.Guid.ToString()}.gif"))
        {
            Utils.Debounce(() =>
            {
                currentCaptureJob.CaptureFilePath = $"{currentCaptureJob.Guid.ToString()}.gif";
                currentCaptureJob.Status = CaptureJobStatus.Completed;
                CaptureJobsManager.FreeSlot(currentCaptureJob.slotIndex);
                Directory.Delete($"{Constants.Paths.Pngs}{currentCaptureJob.slotIndex}", true);
                
            }, ref lastDebounce);
        }
    }
}
