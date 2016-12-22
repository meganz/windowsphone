using System;
using System.Text.RegularExpressions;

namespace MegaApp.Services
{
    public static class ValidationService
    {
        /// <summary>
        /// Checks if a string is a valid email address.
        /// </summary>
        /// <param name="str">String to check.</param>
        /// <returns>TRUE if the string is a valid email address, FALSE in other case.</returns>
        public static bool IsValidEmail(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;            

            try
            {
                // Return true if str is in valid e-mail format.
                return Regex.IsMatch(str,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase);
            }
            catch (Exception) { return false; }
        }
    }
}
