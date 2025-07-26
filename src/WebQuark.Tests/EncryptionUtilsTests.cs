// (c) 2025 Francesco Del Re <francesco.delre.87@gmail.com>
// This code is licensed under MIT license (see LICENSE.txt for details)
using WebQuark.Session.Utilities;

namespace WebQuark.Tests
{
    public class EncryptionUtilsTests
    {
        private const string TestKey = "ThisIsA32ByteLengthEncryptionKey!"; // 32 chars
        private const string ShortKey = "shortkey";
        private const string TestPlainText = "Hello, world! This is a test.";

        [Fact]
        public void Encrypt_And_Decrypt_ReturnsOriginalText()
        {
            // Act
            string encrypted = EncryptionUtils.Encrypt(TestPlainText, TestKey);
            string decrypted = EncryptionUtils.Decrypt(encrypted, TestKey);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void Encrypt_WithShortKey_PadsKeyCorrectly()
        {
            string encrypted = EncryptionUtils.Encrypt(TestPlainText, ShortKey);
            string decrypted = EncryptionUtils.Decrypt(encrypted, ShortKey);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Theory]
        [InlineData(null, TestKey)]
        [InlineData("", TestKey)]
        [InlineData(TestPlainText, null)]
        [InlineData(TestPlainText, "")]
        public void Encrypt_ThrowsArgumentNullException_OnInvalidInput(string plainText, string key)
        {
            Assert.Throws<ArgumentNullException>(() => EncryptionUtils.Encrypt(plainText, key));
        }

        [Theory]
        [InlineData(null, TestKey)]
        [InlineData("", TestKey)]
        [InlineData("someciphertext", null)]
        [InlineData("someciphertext", "")]
        public void Decrypt_ThrowsArgumentNullException_OnInvalidInput(string cipherText, string key)
        {
            Assert.Throws<ArgumentNullException>(() => EncryptionUtils.Decrypt(cipherText, key));
        }

        [Fact]
        public void Decrypt_ThrowsFormatException_ForInvalidBase64()
        {
            string invalidCipher = "NotBase64EncodedText!";
            Assert.Throws<FormatException>(() => EncryptionUtils.Decrypt(invalidCipher, TestKey));
        }
    }
}