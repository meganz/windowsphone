namespace MegaApp.Enums
{
    /// <summary>
    /// Possible results of a "fetch nodes" request made at one account (login) or a folder link (login to folder).
    /// </summary>
    public enum FetchNodesResult
    {
        Success,                            // Request was successfull.
        InvalidHandleOrDecryptionKey,       // Folder link handle length or Key length no valid.
        InvalidDecryptionKey,               // Folder link no valid decryption key.
        NoDecryptionKey,                    // Folder link has not decryption key.
        UnavailableLink,                    // Folder link taken down or not exists or has been deleted by user.
        AssociatedUserAccountTerminated,    // Folder link taken down and the link owner's account is blocked.
        Unknown                             // Unknown error.
    }
}
