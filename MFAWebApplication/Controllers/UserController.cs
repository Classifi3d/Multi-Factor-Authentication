using AuthenticationWebApplication.DTOs;
using AuthenticationWebApplication.Repository;
using AutoMapper;
using MFAWebApplication.Abstraction;
using MFAWebApplication.CommandsAndQueries.Users;
using MFAWebApplication.DTOs;
using MFAWebApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Security.Claims;


namespace AuthenticationWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly Mapper _mapper;
        private readonly SecurityService _securityService;
        private readonly IMemoryCache _cache;
        public UserController(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            Mapper mapper,
            SecurityService securityService,
            IMemoryCache cache )
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _mapper = mapper;
            _securityService = securityService;
            _cache = cache;
        }

        //private readonly Mediator _mediator;

        //public UserController()
        //{
        //    _mediator = new Mediator(Assembly.GetExecutingAssembly());

        //}


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

            var result = await new GetUserProfileQueryHandler(
                _unitOfWork
                ).Handle(new GetUserProfileQuery(userId), cancellationToken);


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

            var result = await new CreateUserCommandHandler(
                _unitOfWork,
                _mapper,
                _securityService
                ).Handle(new CreateUserCommand(userDto), cancellationToken);

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
            var result = await new UpdateUserCommandHandler(
                _userRepository
                ).Handle(new UpdateUserCommand(userDto), cancellationToken);

            if ( result.IsFailure )
                return BadRequest(result.Error);

            return Ok();
        }

        [HttpDelete]
        [Route("user/{userId:guid}")]
        public async Task<IActionResult> DeleteUserAsync( Guid userId, CancellationToken cancellationToken )
        {
            var result = await new DeleteUserCommandHandler(
                _unitOfWork 
                ).Handle(new DeleteUserCommand(userId), cancellationToken);

            if ( result.IsFailure )
                return NotFound(result.Error);

            return Ok();
        }


        [HttpPost]
        [Route("enable-mfa")]
        [ActionName("GenerateUserQRCode")]
        public async Task<IActionResult> MfaGenerateAsync( CancellationToken cancellationToken )
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if ( string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId) )
            {
                return Unauthorized("Invalid token.");
            }

            var result = await new EnableMfaOfUserCommandHandler(
                _unitOfWork,
                _securityService).Handle(new EnableMfaOfUserCommand(userId), cancellationToken);

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

            var result = await new DisableMfaOfUserCommandHandler(
                _unitOfWork
                ).Handle(new DisableMfaOfUserCommand(userId), cancellationToken);


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

            var result = await new LoginUserQueryHandler(
                _userRepository,
                _securityService,
                _cache
                ).Handle(new LoginUserQuery(userLoginDTO), cancellationToken);


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
        public async Task<IActionResult> VerifyMfaAsync( [FromBody] MfaVerificationDTO verificationDTO, CancellationToken cancellationToken)
        {

            var result = await new VerifyMfaOfUserQueryHandler(
                _unitOfWork,
                _securityService,
                _cache
            ).Handle(new VerifyMfaOfUserQuery(verificationDTO), cancellationToken);


            if ( result.IsFailure )
            {
                return Unauthorized(result.Error);
            }

            return Ok(result);
        }

    }

}
