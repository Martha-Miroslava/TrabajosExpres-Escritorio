using System;

namespace TrabajosExpres.PresentationLogicLayer.Utils
{
    /// <summary>
    /// This class is for passwords
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// Method that encrypts a password
        /// </summary>
        /// <param name="password">The password</param>
        /// <returns>Encrypted password</returns>
        public static string Encrypt(string password)
        {
            string passwordEncrypt = string.Empty;
            byte[] encryted = System.Text.Encoding.Unicode.GetBytes(password);
            passwordEncrypt = Convert.ToBase64String(encryted);
            return passwordEncrypt;
        }

        /// <summary>
        /// Method that decrypts a password
        /// </summary>
        /// <param name="password">The password</param>
        /// <returns>Decrypted password</returns>
        public static string Decrypt(string password)
        {
            string passwordDecrypt = string.Empty;
            byte[] decryted = Convert.FromBase64String(password);
            passwordDecrypt = System.Text.Encoding.Unicode.GetString(decryted);
            return passwordDecrypt;
        }
    }
}
