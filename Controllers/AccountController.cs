
namespace HeroGame.Controllers
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using HeroGame.Entities;
    using HeroGame.Helpers;
    using HeroGame.Models;
    using HeroGame.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    [Authorize]
    [Route( "api/[controller]" )]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController(
            IAccountService userService,
            IMapper mapper,
            DataContext context) 
        {
            _userService = userService;
            _mapper = mapper;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost( "Sign-In" )]
        public async Task<IActionResult> Authenticate( [FromBody] AuthenticateModel model )
        {
            Account user = _userService.Authenticate( model.Username, model.Password );

            if( user == null )
            {
                return BadRequest( new { message = "Username or password is incorrect" } );
            }

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity( new[]{
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString())
                    } 
                ) 
            );

            AuthenticationProperties properties = new AuthenticationProperties { AllowRefresh = true, IsPersistent = true };
            await Request.HttpContext.SignInAsync( CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, properties );

            // return basic user info and authentication token
            return Ok( new {
                Id = user.AccountId,
                Username = user.UserName,
            } );
        }

        [AllowAnonymous]
        [HttpPost( "Sign-Up" )]
        public IActionResult Register( [FromBody] RegisterModel model )
        {
            // map model to entity
            Account user = _mapper.Map<Account>( model );

            try
            {
                // create user
                _userService.Create( user, model.Password );
                return Ok();
            }
            catch( AppException ex )
            {
                // return error message if there was an exception
                return BadRequest( new { message = ex.Message } );
            }
        }

        [AllowAnonymous]
        [HttpPost( "Sign-Out" )]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync( CookieAuthenticationDefaults.AuthenticationScheme );
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        {
            return await _context.Accounts.ToListAsync();
        }

        //[AllowAnonymous]
        //[HttpGet( "{id}" )]
        //public IActionResult GetById( int id )
        //{
        //    Account account = _userService.GetById( id );
        //    UserModel model = _mapper.Map<UserModel>( account );
        //    return Ok( model );
        //}

        //[HttpPut( "{id}" )]
        //public IActionResult Update( int id, [FromBody] UpdateModel model )
        //{
        //    // map model to entity and set id
        //    Account account = _mapper.Map<Account>( model );
        //    account.AccountId = id;

        //    try
        //    {
        //        // update user 
        //        _userService.Update( account, model.Password );
        //        return Ok();
        //    }
        //    catch( AppException ex )
        //    {
        //        // return error message if there was an exception
        //        return BadRequest( new { message = ex.Message } );
        //    }
        //}

        //[HttpDelete( "{id}" )]
        //public IActionResult Delete( int id )
        //{
        //    _userService.Delete( id );
        //    return Ok();
        //}


        private IAccountService _userService;
        private IMapper _mapper;
        private readonly DataContext _context;
    }
}
