using System;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HeroGame.Controllers
{
    [Authorize]
    [Route( "api/[controller]" )]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly DataContext _context;

        public AccountController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appsettings,
            DataContext context) 
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appsettings.Value;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost( "Sign-In" )]
        public IActionResult Authenticate( [FromBody] AuthenticateModel model )
        {
            var user = _userService.Authenticate( model.Username, model.Password );

            if( user == null )
                return BadRequest( new { message = "Username or password is incorrect" } );

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes( _appSettings.Secret );
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity( new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.AccountId.ToString())
                } ),
                Expires = DateTime.UtcNow.AddDays( 7 ),
                SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
            };
            var token = tokenHandler.CreateToken( tokenDescriptor );
            var tokenString = tokenHandler.WriteToken( token );

            // return basic user info and authentication token
            return Ok( new {
                Id = user.AccountId,
                Username = user.UserName,
                Token = tokenString
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
        public async Task SignOut()
        {
            // await HttpContext.SignOut
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAll()
        {
            return await _context.Accounts.ToListAsync();
            //var users = _userService.GetAll();
            //var model = _mapper.Map<IList<UserModel>>( users );
            //return Ok( model );
        }

        [HttpGet( "{id}" )]
        public IActionResult GetById( int id )
        {
            var user = _userService.GetById( id );
            var model = _mapper.Map<UserModel>( user );
            return Ok( model );
        }

        [HttpPut( "{id}" )]
        public IActionResult Update( int id, [FromBody] UpdateModel model )
        {
            // map model to entity and set id
            var user = _mapper.Map<Account>( model );
            user.AccountId = id;

            try
            {
                // update user 
                _userService.Update( user, model.Password );
                return Ok();
            }
            catch( AppException ex )
            {
                // return error message if there was an exception
                return BadRequest( new { message = ex.Message } );
            }
        }

        [HttpDelete( "{id}" )]
        public IActionResult Delete( int id )
        {
            _userService.Delete( id );
            return Ok();
        }
    }
}
