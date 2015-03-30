namespace MegaApp.Enums
{
    public enum TransferStatus
    {
        Downloading = 0,
        Uploading   = 10,
        Pausing     = 20,
        Paused      = 30,
        Canceling   = 40,
        Queued      = 50,        
        NotStarted  = 60,
        Downloaded  = 100,
        Uploaded    = 110,
        Canceled    = 140,        
        Error       = 999
    }
}