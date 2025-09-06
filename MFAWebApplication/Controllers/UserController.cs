using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Repository;
using MFAWebApplication.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;


namespace AuthenticationWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = await _userRepository.GetUsersAsync();
            if (users != null)
            {
                return Ok(users);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("user-data")]
        [ActionName("GetUserById")]
        public async Task<IActionResult> GetUserData()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized("Invalid token.");
            }

            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("sign-up")]
        [EnableRateLimiting("registerLimiter")]
        public async Task<IActionResult> AddUserAsync([FromBody] UserDTO userDTO)
        {
            var isAdded = await _userRepository.AddUserAsync(userDTO);
            if (isAdded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserDTO userDTO)
        {
            var isUpdated = await _userRepository.UpdateUserAsync(userDTO);
            if (isUpdated)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        [Route("{userId:guid}")]
        [ActionName("DeleteById")]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            var isDeleted = await _userRepository.DeleteUserAsync(userId);
            if (isDeleted)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("enable-mfa")]
        [ActionName("GenerateUserQRCode")]
        public async Task<IActionResult> MfaGenerateAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized("Invalid token.");
            }

            var qrCodeBytes = await _userRepository.MfaGenerate(userId);
            if (qrCodeBytes != null)
            {
                return File(qrCodeBytes, "image/png");
            }
            return BadRequest("MFA setup failed: User not found or MFA already enabled.");
        }

        [HttpPost]
        [Route("disable-mfa")]
        [ActionName("DisableUserMfa")]
        public async Task<IActionResult> MfaDisableAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized("Invalid token.");
            }

            var isDisabled = await _userRepository.MfaDisable(userId);
            if (isDisabled)
            {
                return Ok();
            }
            return BadRequest("MFA disabling failed failed: User not found");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [EnableRateLimiting("loginLimiter")]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginDTO userLoginDTO)
        {
            var loginResult = await _userRepository.LoginAsync(userLoginDTO);

            if (loginResult.Token == null && !loginResult.RequiresMfa)
            {
                return Unauthorized("Invalid credentials.");
            }

            if (loginResult.RequiresMfa)
            {
                return Ok(new { message = "MFA required. Please verify using the 6-digit code.", challengeId = loginResult.ChallengeId });
            }

            return Ok(new { token = loginResult.Token });
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("verify-mfa")]
        [EnableRateLimiting("mfaLimiter")]
        public async Task<IActionResult> VerifyMfaAsync([FromBody] MfaVerificationDTO verificationDTO)
        {
            string? token = await _userRepository.VerifyMfaAsync(verificationDTO);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid MFA code.");
            }

            return Ok(new { token });
        }

    }

}
