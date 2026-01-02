using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TraCuuBHXH_BHYT.Data;
using TraCuuBHXH_BHYT.Interface;
using TraCuuBHXH_BHYT.Response;

namespace TraCuuBHXH_BHYT.Service
{
    public class TokenValidationService : ITokenValidationService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        public TokenValidationService(AppDbContext db, IConfiguration config)
        {
            _db = db;
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

            // Kiểm tra token với DB (Key = "Token")
            try
            {
                // Lưu ý: ValidateBearerToken hiện tại là đồng bộ (synchronous),
                // nhưng truy cập DB nên dùng async. Tuy nhiên để giữ chữ ký hàm, 
                // ta dùng .GetAwaiter().GetResult() hoặc chuyển hàm sang async nếu interface cho phép.
                // Ở đây ta dùng cách đồng bộ để tránh sửa interface ngay lập tức, 
                // nhưng tốt nhất nên refactor interface sang async Task.
                
                var tokenParam = _db.DMParameter
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Key == "Token" && x.IsActive == true);

                if (tokenParam == null || string.IsNullOrWhiteSpace(tokenParam.Value))
                {
                    // Nếu chưa có token nào được sinh ra trong DB thì coi như token client gửi lên là không hợp lệ
                    return (false, "Hệ thống chưa có token hợp lệ để đối chiếu");
                }

                if (!string.Equals(token, tokenParam.Value.Trim(), StringComparison.Ordinal))
                {
                    return (false, "Token không khớp với hệ thống");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kiểm tra token trong DB: {ex.Message}");
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

        public async Task<TokenResult> GetTokenAsync_V2(string authorizationHeader)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    throw new UnauthorizedAccessException("Thiếu header Authorization");
                }

                var param = await _db.DMParameter
                    .FirstOrDefaultAsync(x => x.Key == "Base64" && x.IsActive == true);

                if (param == null || string.IsNullOrWhiteSpace(param.Value))
                {
                    throw new UnauthorizedAccessException("Không tìm thấy cấu hình Base64");
                }

                var incoming = authorizationHeader.Trim();
                var expected = param.Value.Trim();

                if (!string.Equals(incoming, expected, StringComparison.Ordinal))
                {
                    throw new UnauthorizedAccessException("Xác thực không hợp lệ");
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, "TraCuuBHXH_BHYT"),
                    new Claim("scope", "am_application_scope default")
                };

                var expires = DateTime.UtcNow.AddDays(1);
                var jwt = new JwtSecurityToken(
                    issuer: "TraCuuBHXH_BHYT",
                    audience: "TraCuuBHXH_BHYT",
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: expires
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);

                var tokenParam = await _db.DMParameter.FirstOrDefaultAsync(x => x.Key == "Token");
                if (tokenParam != null)
                {
                    tokenParam.Value = tokenString;
                    _db.DMParameter.Update(tokenParam);
                }
                else
                {
                    await _db.DMParameter.AddAsync(new TraCuuBHXH_BHYT.Entities.DMParameterEntity
                    {
                        Key = "Token",
                        Value = tokenString,
                        IsActive = true
                    });
                }
                await _db.SaveChangesAsync();

                return new TokenResult
                {
                    access_token = tokenString,
                    scope = "am_application_scope default",
                    token_type = "Bearer",
                    expires_in = 86400
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
