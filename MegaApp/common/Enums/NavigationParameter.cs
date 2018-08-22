namespace MegaApp.Enums
{
    public enum NavigationParameter
    {
        Normal              = 0,
        Login               = 10,
        PasswordLogin       = 15,
        CreateAccount       = 20,
        UriLaunch           = 40,
        Browsing            = 50,
        BreadCrumb          = 60,
        InternalNodeLaunch  = 65,
        FileLinkLaunch      = 70,
        FolderLinkLaunch    = 75,
        ImportFolderLink    = 76,
        Uploads             = 80,
        Downloads           = 90,
        DisablePassword     = 100,
        AutoCameraUpload    = 110,
        SecuritySettings    = 120,
        MFA_Enabled         = 125,
        AccountUpdate       = 200,
        None                = 999,

        API_ESID            = -15,
        API_EBLOCKED        = -16
    }
}
