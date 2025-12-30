using Microsoft.AspNetCore.Mvc;
using TraCuuBHXH_BHYT.Interface;
using TraCuuBHXH_BHYT.Request;
using TraCuuBHXH_BHYT.Response;
using TraCuuBHXH_BHYT.Constant;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System;

namespace TraCuuBHXH_BHYT.Controllers
{
    [ApiController]
    [Route("")]
    public class TraCuuBHXHController : ControllerBase
    {
        private readonly ITraCuuBHXHService _serviceTraCuuBHXH;
        private readonly ITokenValidationService _tokenValidationService;
        public TraCuuBHXHController(ITraCuuBHXHService serviceTraCuuBHXH,
             ITokenValidationService tokenValidationService)
        {
            _serviceTraCuuBHXH = serviceTraCuuBHXH;
            _tokenValidationService = tokenValidationService;
        }

        [HttpPost("traCuuThongTin")]
        public async Task<IActionResult> TraCuuBHXHQD([FromBody] RequestTraCuuBHXHVN request, [FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization)) return Unauthorized("Xác thực không hợp lệ");
            if (request == null) return BadRequest("Request không hợp lệ");


            // Validate Bearer token
            var tokenValidationResult = _tokenValidationService.ValidateBearerToken(authorization);
            if (!tokenValidationResult.IsValid)
            {
                return Unauthorized(tokenValidationResult.ErrorMessage);
            }

            var result = await _serviceTraCuuBHXH.TraCuuBHXHQDAsync(request);

            if (result.maLoi == Constant.Constant.MA_LOI_THAT_BAI)
            {
                return BadRequest(new
                {
                    maLoi = result.maLoi,
                    moTaLoi = result.moTaLoi
                });
            }

            return Ok(result);
        }

        [HttpPost("themHoacCapNhat")]
        public async Task<IActionResult> ThemHoacCapNhatBHXH([FromBody] RequestTraCuuBHXHVN request, [FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization)) return Unauthorized("Xác thực không hợp lệ");
            if (request == null) return BadRequest("Request không hợp lệ");

            // Validate Bearer token
            var tokenValidationResult = _tokenValidationService.ValidateBearerToken(authorization);
            if (!tokenValidationResult.IsValid)
            {
                return Unauthorized(tokenValidationResult.ErrorMessage);
            }

            var result = await _serviceTraCuuBHXH.ThemHoacCapNhatAsync(request);

            if (result.maLoi == Constant.Constant.MA_LOI_THAT_BAI)
            {
                return BadRequest(new
                {
                    maLoi = result.maLoi,
                    moTaLoi = result.moTaLoi
                });
            }

            return Ok(result);
        }

        // API lấy token NGSP (tách riêng)
        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsyncV2([FromHeader(Name = "Authorization")] string authorization)
        {
            try
            {
                var token = await _tokenValidationService.GetTokenAsync(authorization);
                if (string.IsNullOrWhiteSpace(token.access_token))
                {
                    return Unauthorized("Không lấy được token");
                }
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Lỗi hệ thống");
            }
        }
    }
}
