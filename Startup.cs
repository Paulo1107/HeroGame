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
                services.AddDbContext<DataContext>();

            services.AddCors();
            services.AddControllers();

            var appSettingsSection = _configuration.GetSection( "AppSettings" );
            services.Configure<AppSettings>( appSettingsSection );

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes( appSettings.Secret );
            services.AddAuthentication( x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            } )
            .AddJwtBearer( x => {
                x.Events = new JwtBearerEvents {
                    OnTokenValidated = context => {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                        var userId = int.Parse( context.Principal.Identity.Name );
                        var user = userService.GetById( userId );
                        if( user == null )
                        {
                            // return unauthorized if user no longer exists
                            context.Fail( "Unauthorized" );
                        }
                        return Task.CompletedTask;
                    }
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey( key ),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            } );

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();

            services.AddDbContext<DataContext>( options =>
                       options.UseSqlServer( _configuration.GetConnectionString("Hero") ) );


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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints( endpoints => endpoints.MapControllers() );
        }
    }
}
