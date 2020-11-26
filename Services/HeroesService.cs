
namespace HeroGame.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HeroGame.Entities;
    using HeroGame.Helpers;

    public interface IHeroesService
    {
        Hero Create( Hero hero);
        void Delete( int id);
        IEnumerable<Hero> GetAllHeroes();
    }

    public class HeroesService : IHeroesService
    {
        private DataContext _context;

        public HeroesService( DataContext dataContext )
        {
            _context = dataContext;
        }

        public Hero Create( Hero hero )           
        {
            if( _context.Heroes.Any( x => x.Name == hero.Name ) )
            {
                throw new AppException( "Hero \"" + hero.Name + "\" is already taken" );
            }

            _context.Heroes.Add( hero );
            _context.SaveChanges();
            
            return hero;
        }

        public void Delete( int id )
        {
            Hero hero = _context.Heroes.Find( id );
            if( hero != null )
            {
                _context.Heroes.Remove( hero );
                _context.SaveChanges();
            }
        }

        public IEnumerable<Hero> GetAllHeroes()
        {
            return _context.Heroes;
        }
    }
}
