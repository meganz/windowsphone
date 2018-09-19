namespace MegaApp.Enums
{
    public enum ChangePasswordResult
    {
        Success,                    // Successful change password.
        MultiFactorAuthInvalidCode, // Invalid Multi-factor authentication code.
        Unknown                     // Unknown result, but not successful.
    }
}
