using System;

public class CaptureJob
{
    public Guid Guid;
    public string UserId;
    public CaptureJobStatus Status;
    public int slotIndex;
    public Action<CaptureJob> JobAction;
    public string CaptureGifFilePath;
    public string CapturePngFilePath;
}
