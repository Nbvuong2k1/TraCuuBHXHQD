using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using TraCuuBHXH_BHYT.Data;
using TraCuuBHXH_BHYT.Interface;
using TraCuuBHXH_BHYT.Response;

namespace TraCuuBHXH_BHYT.Service
{
    public class TokenValidationService : ITokenValidationService
    {
        private readonly IConfiguration _config;
        public TokenValidationService( IConfiguration config)
        {
            _config = config;
        }
        public (bool IsValid, string ErrorMessage) ValidateBearerToken(string authorization)
        {
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return (false, "Thiếu token xác thực");
            }

            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Token không đúng định dạng. Vui lòng sử dụng Bearer token");
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Token không hợp lệ");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                {
                    return (false, "Token không hợp lệ hoặc không đúng định dạng");
                }

                var jsonToken = handler.ReadJwtToken(token);

                if (jsonToken.ValidTo != DateTime.MinValue &&
                    jsonToken.ValidTo < DateTime.UtcNow)
                {
                    return (false, "Token đã hết hạn");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Token không hợp lệ: {ex.Message}");
            }
        }

        public async Task<TokenResult> GetTokenAsync(string authorizationHeader)
        {
            string apiToken = _config["AppSettings:url_token"];
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                throw new UnauthorizedAccessException("Thiếu cấu hình url_token");
            }
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                throw new UnauthorizedAccessException("Thiếu header Authorization");
            }

            using (HttpClient client = new HttpClient())
            {
                // Forward trực tiếp header Authorization từ client: "Basic base64(consumer-key:consumer-secret)"
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                };

                try
                {
                    HttpResponseMessage response = await client.PostAsync(apiToken, new FormUrlEncodedContent(formData));
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new UnauthorizedAccessException("Không lấy được token");
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<TokenResult>(responseBody);
                    return res ?? new TokenResult();
                }
                catch (HttpRequestException ex)
                {
                    throw new UnauthorizedAccessException("Lỗi trong quá trình lấy Token");
                }
            }
        }
    }
}
