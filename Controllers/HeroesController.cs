
namespace HeroGame.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
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

    [Authorize]
    [Route( "api/[controller]" )]
    [ApiController]
    public class HeroesController : ControllerBase
    {
        public HeroesController(
            IHeroesService heroesService,
            IMapper mapper,
            DataContext context )
        {
            _heroesService = heroesService;
            _mapper = mapper;
            _context = context;
        }


        [AllowAnonymous]
        [HttpPost( "Create" )]
        public IActionResult Register( [FromBody] RegisterHeroes model )
        {
            // map model to entity
            Hero hero = _mapper.Map<Hero>( model );
            var accountId = int.Parse( HttpContext.User.Claims.Single( c => c.Type == ClaimTypes.NameIdentifier ).Value);
            try
            {
                var heros = new Hero {
                    AccountId = accountId,
                    Name = "TestName",
                    Level = 1,
                    Experience = 0,
                    AttackPoints = 5,
                    HealthPoints = 20,
                    MaxHealthPoints = 20
                };
                //{
                //    // create user
                _heroesService.Create( heros );
            }
            //}
            catch( AppException ex )
            {
                // return error message if there was an exception
                return BadRequest( new { message = ex.Message } );
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hero>>> GetAllHeroes()
        {
            return await _context.Heroes.ToListAsync();
        }

        [AllowAnonymous]
        [HttpDelete( "{id}" )]
        public IActionResult Delete( int id )
        {
            _heroesService.Delete( id );
            return Ok();
        }

        private IHeroesService _heroesService;
        private IMapper _mapper;
        private readonly DataContext _context;
    }
}
