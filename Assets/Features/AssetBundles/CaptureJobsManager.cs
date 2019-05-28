using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEngine;

public class CaptureJobsManager : MonoBehaviour
{
    bool isRunning = false;
    int currentConcurrentJobs;
    ObservableCollection<CaptureJob> captureJobsList = new ObservableCollection<CaptureJob>();
    bool[] availableSlots;
    public GameObject GifCapturerCameraPrefab;
    public bool AutoRun;
    public int AutoDispatchSeconds = 5;
    public int MaxConcurrentJobs = 10;
    public float CamerasX = 3f;
    public float CamerasY = 1.5f;
    public float CamerasZ = -5f;
    public float SpacingPerCamera = 100f;

    public float RotateAroundTargetX = 3f;
    public float RotateAroundTargetY = 0f;
    public float RotateAroundTargetZ = -1f;

    public float SpawnX = 2f;
    public float SpawnY = 0f;
    public float SpawnZ = 0f;

    public AssetBundlesLoader AssetBundlesLoader;
    public GifCaptureManager[] GifCaptureManagers;

    public void Start()
    {
        Logger.Log($"Setting up Capture Jobs Manager with {MaxConcurrentJobs} max concurrent jobs");
        SetupGifCaptureManagers();
        SetupCaptureParents();
        if (AutoRun)
        {
            availableSlots = new bool[MaxConcurrentJobs];
            for (int i = 0; i < availableSlots.Length; i++)
            {
                availableSlots[i] = true;
            }
            Run();
        }
    }

    void SetupCaptureParents()
    {
        AssetBundlesLoader.CaptureParents = new Transform[MaxConcurrentJobs];
        for (int i = 0; i < MaxConcurrentJobs; i++)
        {
            AssetBundlesLoader.CaptureParents[i] = new GameObject($"CaptureParent{i}").transform;
        }
    }

    void SetupGifCaptureManagers()
    {
        GifCaptureManagers = new GifCaptureManager[MaxConcurrentJobs];
        for (int i = 0; i < MaxConcurrentJobs; i++)
        {
            GifCaptureManagers[i] = Instantiate(GifCapturerCameraPrefab, new Vector3(CamerasX, CamerasY + SpacingPerCamera * i, CamerasZ), Quaternion.identity).GetComponent<GifCaptureManager>();
            var renderTexture = new RenderTexture(GifCaptureManagers[i].GifWidth, GifCaptureManagers[i].GifHeight, 16, RenderTextureFormat.Default);
            GifCaptureManagers[i].MyRenderTexture = renderTexture;
            GifCaptureManagers[i].GetComponent<RotateAround>().Target = new Vector3(RotateAroundTargetX, RotateAroundTargetY + i * SpacingPerCamera, RotateAroundTargetZ);
            GifCaptureManagers[i].GetComponent<Camera>().targetTexture = GifCaptureManagers[i].MyRenderTexture;
            GifCaptureManagers[i].CaptureJobsManager = this;
        }
    }

    public void Run()
    {
        if (isRunning) {
            Logger.Log($"Capture jobs manager was already running", Logger.LogLevel.Warning);
            return;
        }

        Logger.Log($"Run capture jobs manager");
        isRunning = true;
        captureJobsList.CollectionChanged += HandleCaptureJobCollectionChanged;
        InvokeRepeating(nameof(DispatchQueueJobs), AutoDispatchSeconds, AutoDispatchSeconds);
    }

    public void CleanCompletedJobs()
    {
        Logger.Log($"Cleaning completed jobs");
        var completedJobs = captureJobsList.Where(c => c.Status == CaptureJobStatus.Completed);
        foreach (var job in completedJobs)
        {
            captureJobsList.Remove(job);
        }
    }

    public void FreeSlot(int slotIndex)
    {
        Logger.Log($"Free slot {slotIndex}", Logger.LogLevel.Debug);
        availableSlots[slotIndex] = true;
        AssetBundlesLoader.DeleteCaptureSelection(slotIndex);
    }

    public void Stop()
    {
        isRunning = false;
        foreach (var job in captureJobsList)
        {
            if(job.Status == CaptureJobStatus.InProgress)
            {
                job.Status = CaptureJobStatus.Enqueued;
            }
        }
        captureJobsList.CollectionChanged -= HandleCaptureJobCollectionChanged;
    }

    public Guid AddCaptureJob(string userId)
    {
        if (!captureJobsList.Any(c => c.UserId == userId))        
        {
            var guid = Guid.NewGuid();

            if (!captureJobsList.ToList().Exists(j => j.Guid == guid))
            {
                captureJobsList.Add(new CaptureJob
                {
                    Guid = guid,
                    UserId = userId,
                    JobAction = JobAction,
                    Status = CaptureJobStatus.Enqueued
                });
            }            
            return guid;
        }
        else
        {
            return captureJobsList.First(j => j.UserId == userId).Guid;
        }
    }

    void JobAction(CaptureJob job)
    {
        try
        {
            AssignSlot(job);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AssetBundlesLoader.GenerateRandomShield(new Vector3(SpawnX, SpawnY + job.slotIndex * SpacingPerCamera, SpawnZ), job.slotIndex);
                GifCaptureManagers[job.slotIndex].StartCapturing(job);
            });
        }
        catch (Exception)
        {
            job.Status = CaptureJobStatus.Error;
        }
    }

    void AssignSlot(CaptureJob job)
    {
        for (int i = 0; i < availableSlots.Length; i++)
        {
            if (availableSlots[i])
            {
                job.slotIndex = i;
                availableSlots[i] = false;
                return;
            }
        }
        throw new Exception("All slots are busy.");
    }

    public CaptureResult GetCaptureResult(Guid guid)
    {
        var job = captureJobsList.FirstOrDefault(j => j.Guid == guid);
        if (job == null) return new CaptureResult { Status = CaptureJobStatus.NotFound };
        if (job.Status != CaptureJobStatus.Completed) return new CaptureResult { Status = job.Status };

        var filePath = job.CaptureFilePath;
        if (filePath != null)
        {
            var result = File.ReadAllBytes(filePath);
            return new CaptureResult
            {
                Raw = result,
                Status = CaptureJobStatus.Completed
            };
        }
        else
        {
            throw new FileNotFoundException();
        }
    }

    void HandleCaptureJobCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        DispatchQueueJobs();
    }

    void DispatchQueueJobs()
    {
        currentConcurrentJobs = captureJobsList.Count(job => job.Status == CaptureJobStatus.InProgress);
        if (currentConcurrentJobs < MaxConcurrentJobs)
        {
            var toRunJobs = captureJobsList.Where(job => job.Status == CaptureJobStatus.Enqueued).Take(MaxConcurrentJobs - currentConcurrentJobs);
            foreach (var job in toRunJobs)
            {
                job.Status = CaptureJobStatus.InProgress;
                job.JobAction?.Invoke(job);
            }
        }
    }
}
