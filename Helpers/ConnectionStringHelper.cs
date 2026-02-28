using System;
using System.Security.Cryptography;
using System.Text;

namespace TraCuuBHXH_BHYT.Helpers
{
    public static class ConnectionStringHelper
    {
        /// <summary>
        /// Giải mã chuỗi Base64; nếu chuỗi không hợp lệ sẽ trả về chuỗi gốc.
        /// </summary>
        public static string DecodeBase64(string? encodedValue)
        {
            if (string.IsNullOrWhiteSpace(encodedValue))
            {
                return string.Empty;
            }

            try
            {
                // TryFromBase64String tránh ném exception khi chuỗi không đúng định dạng.
                Span<byte> buffer = new Span<byte>(new byte[encodedValue.Length]);
                if (!Convert.TryFromBase64String(encodedValue, buffer, out var bytesWritten))
                {
                    return encodedValue;
                }

                return Encoding.UTF8.GetString(buffer.Slice(0, bytesWritten));
            }
            catch
            {
                // Nếu có lỗi (ví dụ Encoding lỗi) thì vẫn dùng chuỗi gốc.
                return encodedValue;
            }
        }

    }
    public static class ConnectionStringCrypto
    {
        private const int KeySize = 32; // 256 bit
        private const int IvSize = 16;  // 128 bit
        private const int SaltSize = 16;
        private const int Iterations = 100_000;       
        public static string Decrypt(string encryptedText, string secretKey)
        {
            byte[] allBytes = Convert.FromBase64String(encryptedText);

            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            byte[] cipherText = new byte[allBytes.Length - SaltSize - IvSize];

            Buffer.BlockCopy(allBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(allBytes, SaltSize, iv, 0, IvSize);
            Buffer.BlockCopy(allBytes, SaltSize + IvSize, cipherText, 0, cipherText.Length);

            using var keyDerivation = new Rfc2898DeriveBytes(
                secretKey,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );

            byte[] key = keyDerivation.GetBytes(KeySize);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherText);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }
    }
    }

