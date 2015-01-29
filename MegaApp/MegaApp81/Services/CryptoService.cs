using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Services
{
    static class CryptoService
    {
        public static string HashData(string value)
        {
            // Convert the value to a byte[].
            byte[] valueByte = Encoding.UTF8.GetBytes(value);

            var sha256Managed = new SHA256Managed();

            byte[] hashBytes = sha256Managed.ComputeHash(valueByte);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Encrypt a string value
        /// </summary>
        /// <param name="value">String value to encrypt</param>
        /// <returns>The orignal value encrypted and converted to base64 string</returns>
        public static string EncryptData(string value)
        {
            // Convert the value to a byte[].
            byte[] valueByte = Encoding.UTF8.GetBytes(value);

            // Extra entropy values
            var optionalEntropy = new byte[] { 5, 8, 9, 1, 2, 7, 5, 4 };

            // Encrypt the value by using the Protect() method and add extra entropy for some more security.
            byte[] protectedPinByte = ProtectedData.Protect(valueByte, optionalEntropy);

            // Convert the bytes to a string to store in isolated storage
            return Convert.ToBase64String(protectedPinByte);
        }

        /// <summary>
        /// Decrypt a base64 string value
        /// </summary>
        /// <param name="value">Base64 string value to decrypt</param>
        /// <returns>Decrypted original string</returns>
        public static string DecryptData(string value)
        {
            try
            {
                // Convert the value from base64 to a byte[].
                byte[] encryptedByte = Convert.FromBase64String(value);

                // Extra entropy values
                var optionalEntropy = new byte[] { 5, 8, 9, 1, 2, 7, 5, 4 };

                // Decrypt the value by using the Unprotect () method and the same extra entropy as in encryption
                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedByte, optionalEntropy);

                // Return the original string value
                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length);
            }
            catch
            {
                return null;
            }
        }
    }
}
