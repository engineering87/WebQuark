// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebQuark.Session.Utilities
{
    public static class EncryptionUtils
    {
        /// <summary>
        /// Converts the provided key string into a 32-byte array used for AES encryption.
        /// Pads or truncates the key to fit 32 bytes.
        /// </summary>
        /// <param name="key">The encryption key as a string.</param>
        /// <returns>A 32-byte key array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the key is null or empty.</exception>
        private static byte[] GetKeyBytes(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length == 32)
                return keyBytes;

            var paddedKey = new byte[32];
            Array.Copy(keyBytes, paddedKey, Math.Min(keyBytes.Length, 32));
            return paddedKey;
        }

        /// <summary>
        /// Encrypts the specified plain text using AES encryption with the given key.
        /// Generates a random IV which is prepended to the ciphertext for use during decryption.
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt.</param>
        /// <param name="key">The encryption key as a string.</param>
        /// <returns>The encrypted data as a Base64 encoded string, including the IV.</returns>
        /// <exception cref="ArgumentNullException">Thrown if plainText or key is null or empty.</exception>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var keyBytes = GetKeyBytes(key);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }

                    var encryptedBytes = ms.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// Decrypts the specified ciphertext (Base64 encoded string) using AES decryption with the given key.
        /// Expects the IV to be prepended to the ciphertext.
        /// </summary>
        /// <param name="cipherText">The Base64 encoded ciphertext including the IV.</param>
        /// <param name="key">The encryption key as a string.</param>
        /// <returns>The decrypted plaintext string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if cipherText or key is null or empty.</exception>
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var keyBytes = GetKeyBytes(key);
            var fullCipher = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV from the first 16 bytes
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                var cipher = new byte[fullCipher.Length - iv.Length];
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}