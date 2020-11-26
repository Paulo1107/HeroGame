
namespace HeroGame.Helpers
{
    using HeroGame.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public partial class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            // connect to sql server database
            if( !optionsBuilder.IsConfigured )
            {
                optionsBuilder.UseSqlServer( Configuration.GetConnectionString( "Hero" ));
            }
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Hero> Heroes { get; set; }
    }
}