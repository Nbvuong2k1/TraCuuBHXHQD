using TraCuuBHXH_BHYT.Response;

namespace TraCuuBHXH_BHYT.Interface
{
    public interface ITokenValidationService
    {
        (bool IsValid, string ErrorMessage) ValidateBearerToken(string authorization);
        Task<TokenResult> GetTokenAsync(string authorizationHeader);
        Task<TokenResult> GetTokenAsync_V2(string authorizationHeader);
    }
}
