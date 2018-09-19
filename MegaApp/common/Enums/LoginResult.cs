namespace MegaApp.Enums
{
    public enum LoginResult
    {
        Success,                            // Successful login.
        UnassociatedEmailOrWrongPassword,   // Email unassociated with a MEGA account or Wrong password.
        TooManyLoginAttempts,               // Too many failed login attempts. Wait one hour.
        AccountNotConfirmed,                // Account not confirmed.
        MultiFactorAuthRequired,            // Multi-factor authentication required
        MultiFactorAuthInvalidCode,         // Invalid Multi-factor authentication code.
        Unknown                             // Unknown result, but not successful.
    }
}