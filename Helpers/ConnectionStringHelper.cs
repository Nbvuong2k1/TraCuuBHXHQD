using System;
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
}

