using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Enteties;
using MFAWebApplication.Abstraction;
using MFAWebApplication.CommandsAndQueries.Users;
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

        private readonly IMediator _mediator;

        public UserController( IMediator mediator )
        {
            _mediator = mediator;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetUsersAsync()
        //{
        //    var users = await _userRepository.GetAll();
        //    if (users != null)
        //    {
        //        return Ok(users);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        [HttpGet]
        [Route("user-data")]
        [ActionName("GetUserById")]
        public async Task<IActionResult> GetUserData( CancellationToken cancellationToken )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if ( string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId) )
            {
                return Unauthorized("Invalid token.");
            }

            var query = new GetUserProfileQuery(userId);
            var result = await _mediator.Query<GetUserProfileQuery,User>(query,cancellationToken);

            if ( result.IsFailure )
            {
                return NotFound(result.Error);
            }

            return Ok(result.Value);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("sign-up")]
        [EnableRateLimiting("registerLimiter")]
        public async Task<IActionResult> AddUserAsync( [FromBody] UserDTO userDto, CancellationToken cancellationToken )
        {
            if ( userDto is null )
            {
                return BadRequest("User data is required.");
            }

            var result = await _mediator.Send(new CreateUserCommand(userDto), cancellationToken);

            if ( result.IsFailure )
            {
                return BadRequest(result.Error);
            }

            return Ok();

        }

        [HttpPut]
        [Route("user")]
        public async Task<IActionResult> UpdateUserAsync( [FromBody] UserDTO userDto, CancellationToken cancellationToken )
        {
            var result = await _mediator.Send(new UpdateUserCommand(userDto), cancellationToken);

            if ( result.IsFailure )
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpDelete]
        [Route("user/{userId:guid}")]
        public async Task<IActionResult> DeleteUserAsync( Guid userId, CancellationToken cancellationToken )
        {
            var result = await _mediator.Send(new DeleteUserCommand(userId), cancellationToken);

            if ( result.IsFailure )
                return NotFound(result.Error);

            return Ok();
        }


        [HttpPost]
        [Route("enable-mfa")]
        [ActionName("GenerateUserQRCode")]
        public async Task<IActionResult> EnableMfaAsync( CancellationToken cancellationToken )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if ( string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId) )
            {
                return Unauthorized("Invalid token.");
            }

            var command = new EnableMfaOfUserCommand(userId);
            var result = await _mediator.Send<EnableMfaOfUserCommand, byte[]>(command, cancellationToken);

            if ( result.IsFailure )
                return BadRequest(result.Error);

            return File(result.Value, "image/png");

        }

        [HttpPost]
        [Route("disable-mfa")]
        [ActionName("DisableUserMfa")]
        public async Task<IActionResult> MfaDisableAsync( CancellationToken cancellationToken )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if ( string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId) )
            {
                return Unauthorized("Invalid token.");
            }

            var command = new DisableMfaOfUserCommand(userId);
            var result = await _mediator.Send(command, cancellationToken);

            if ( result.IsFailure )
                return BadRequest(result.Error);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [EnableRateLimiting("loginLimiter")]
        public async Task<IActionResult> LoginAsync( [FromBody] UserLoginDTO userLoginDTO, CancellationToken cancellationToken )
        {
            var query = new LoginUserQuery(userLoginDTO);
            var result = await _mediator.Query<LoginUserQuery,LoginSecurityDTO>(query, cancellationToken);

            if ( result.IsFailure )
            {
                return Unauthorized("Invalid credentials.");
            }

            var loginResult = result.Value;

            if ( loginResult.RequiresMfa )
            {
                return Ok(new
                {
                    message = "MFA required. Please verify using the 6-digit code.",
                    challengeId = loginResult.ChallengeId
                });
            }

            return Ok(new { token = loginResult.Token });
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("verify-mfa")]
        [EnableRateLimiting("mfaLimiter")]
        public async Task<IActionResult> VerifyMfaAsync( [FromBody] MfaVerificationDTO verificationDto, CancellationToken cancellationToken)
        {
            var query = new VerifyMfaOfUserQuery(verificationDto);
            var result = await _mediator.Query<VerifyMfaOfUserQuery, string>(query, cancellationToken);

            if ( result.IsFailure )
            {
                return Unauthorized(result.Error);
            }

            return Ok(result);
        }

    }

}
