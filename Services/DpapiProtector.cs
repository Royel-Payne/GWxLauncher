using System;
using System.Security.Cryptography;
using System.Text;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Simple DPAPI wrapper for protecting small secrets at rest.
    /// Uses CurrentUser scope (decryptable only by the same Windows user).
    /// </summary>
    internal static class DpapiProtector
    {
        public static string ProtectToBase64(string plaintext)
        {
            if (plaintext == null)
                throw new ArgumentNullException(nameof(plaintext));

            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] protectedBytes = ProtectedData.Protect(bytes, optionalEntropy: null, scope: DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        public static string UnprotectFromBase64(string protectedBase64)
        {
            if (protectedBase64 == null)
                throw new ArgumentNullException(nameof(protectedBase64));

            if (string.IsNullOrWhiteSpace(protectedBase64))
                return string.Empty;

            byte[] protectedBytes = Convert.FromBase64String(protectedBase64);
            byte[] bytes = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, scope: DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
