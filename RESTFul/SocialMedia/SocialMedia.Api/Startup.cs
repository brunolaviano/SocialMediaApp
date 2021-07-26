using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialMedia.Core.CustomEntities;
using SocialMedia.Core.Interfaces;
using SocialMedia.Core.Services;
using SocialMedia.Infraestructure.Data;
using SocialMedia.Infraestructure.Extensions;
using SocialMedia.Infraestructure.Filters;
using SocialMedia.Infraestructure.Interfaces;
using SocialMedia.Infraestructure.Options;
using SocialMedia.Infraestructure.Repositories;
using SocialMedia.Infraestructure.Services;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace SocialMedia.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddControllers(options => 
            {
                options.Filters.Add<GlobalExceptionFilter>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //Evitar que envie Nulos en la Serializacion
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 
            })
            //Deshabilitar la validacion del [ApiController] para validarlo de otra forma 
            //Ejemplo a traves de los Filters Controles
            .ConfigureApiBehaviorOptions(options => {
                //options.SuppressModelStateInvalidFilter = true;  //Es para Anular la Validacion del [ApiController]
            });

            /*//Configurar los parametros Pagination por defecto
            services.Configure<PaginationOptions>(Configuration.GetSection("Pagination"));
            //Configurar las Opciones de la Password para Encriptacion
            services.Configure<PasswordOptions>(Configuration.GetSection("PasswordOptions"));*/
            services.AddOptions(Configuration); //Refactorizado en ServiceCollectionExtension (Infrastructure/Extenstions)


            //Defino Cadena Conexion a la BBDD
            /*services.AddDbContext<SocialMediaContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SocialMedia"))
            );*/
            services.AddDbContexts(Configuration); //Refactorizado en ServiceCollectionExtension (Infrastructure/Extenstions)

            //Resolver las Dependencias
            /*services.AddTransient<IPostService, PostService>();
            services.AddTransient<ISecurityService, SecurityService>();*/
            /*services.AddTransient<IPostRepository, PostRepository>(); //No Se Utiliza
            services.AddTransient<IUserRepository, UserRepository>();*/ //No Se Utiliza
            /*services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IPasswordService, PasswordService>();
            services.AddSingleton<IUriService>(provider =>
            {
                var accesor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accesor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
                return new UriServices(absoluteUri);
            });*/
            services.AddServices(); //Refactorizado en ServiceCollectionExtension (Infrastructure/Extenstions)


            //Configurar Swagger para documentar la APP
            /*services.AddSwaggerGen(doc =>
            {
                doc.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Media API", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                doc.IncludeXmlComments(xmlPath);
            });*/
            services.AddSwagger($"{Assembly.GetExecutingAssembly().GetName().Name}.xml"); //Refactorizado en ServiceCollectionExtension (Infrastructure/Extenstions)


            //Configurar el JWT Token
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Authentication:Issuer"],
                    ValidAudience = Configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]))
                };
            });


            //Agrego el ValidatorFilter de Infraestructura como si fuera un Middleware
            //para que aplique para toda nuestra API (Filtro de forma Global)
            services.AddMvc(options =>
            {
                options.Filters.Add<ValidationFilter>();
            })
            //Registro el FluentValidation registrado (PostValidators) en carpeta Infrastructure
            .AddFluentValidation(options => {
                options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(options => 
            {
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "Social Media API V1");
                //options.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            //Defino el Uso de la Authentication por Token JWT
            app.UseAuthentication();

            app.UseAuthorization();
                        

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
