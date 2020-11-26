using HeroGame.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HeroGame.Helpers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeroGame.Services;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace HeroGame
{
    public class Startup
    {
        public IWebHostEnvironment _env { get; }
        
        public IConfiguration _configuration { get; }

        public Startup( IWebHostEnvironment env, IConfiguration configuration )
        {
            _env = env;
            _configuration = configuration; ;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration( mc =>
            {
                mc.AddProfile( new AutoMapperProfile() );
            } );

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton( mapper );

            services.AddMvc();

            if( _env.IsProduction() )
            {
                services.AddDbContext<DataContext>();
            }

            services.AddDbContext<DataContext>( options =>
                       options.UseSqlServer( _configuration.GetConnectionString( "Hero" ) ) );

            services.AddCors( options => options.AddPolicy( "ApiCORSPolicy", builder => 
            {
                builder.WithOrigins( "http://localhost:54683" )
                    .SetIsOriginAllowed( _ => true )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }));

            services.AddControllers();

            var appSettingsSection = _configuration.GetSection( "AppSettings" );
            services.Configure<AppSettings>( appSettingsSection );

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes( appSettings.Secret );

            services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
                .AddCookie( options => {
                    options.Cookie.Name = "STGH_Authentication";
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Events = new CookieAuthenticationEvents {
                        OnRedirectToLogin = redirectContext => {
                            redirectContext.HttpContext.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        }
                    };
                } );

            // configure DI for application services
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<IHeroesService, HeroesService>();

            services.AddSwaggerGen( c => {
                c.SwaggerDoc( "v1", new OpenApiInfo { Title = "HeroGame", Version = "v1" } );
            } );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env, DataContext dataContext )
        {
            // migrate any database changes on startup (includes initial db creation)
            dataContext.Database.Migrate();

            if( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI( c => c.SwaggerEndpoint( "/swagger/v1/swagger.json", "HeroGame v1" ) );
            }

           // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors( "ApiCORSPolicy" );
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints( endpoints => endpoints.MapControllers() );
        }
    }
}
