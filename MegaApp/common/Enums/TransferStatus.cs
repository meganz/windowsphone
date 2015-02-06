namespace MegaApp.Enums
{
    public enum TransferStatus
    {
        NotStarted = 60,
        Connecting = 50,
        Downloading = 0,
        Uploading = 10,
        Pausing = 20,
        Paused = 30,
        Canceling = 40,
        Finished = 100,
        Canceled = 110,
        Error = 999
    }
}