using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using Tymeline.API.Daos;

namespace Tymeline.API
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
           
            // services.Configure<CustomAuthenticationOptions>(Configuration.GetSection("CustomAuthenticationOptions"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"]));

            services.AddSingleton<IAuthorizationMiddlewareResultHandler,
                          MyAuthorizationMiddlewareResultHandler>();

            
            services.AddAuthorization(options => {
                options.AddPolicy("CanAffectRoles", policy => policy.RequireClaim("CanAffectRoles"));
            });
            services.AddAuthentication(options =>  
            {  
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  
            })
            .AddJwtBearer(config =>  
            {   
                config.RequireHttpsMetadata = false;  
                config.SaveToken = true;  
                config.TokenValidationParameters = new TokenValidationParameters()  
                {  
                    IssuerSigningKey = signingKey,  
                    ValidateAudience = true,  
                    ValidAudience = Configuration["AppSettings:Hostname"],  
                    ValidateIssuer = true,  
                    ValidIssuer = Configuration["AppSettings:Hostname"],  
                    ValidateLifetime = true,  
                    ValidateIssuerSigningKey = true  
                };  
        
            });  

            
            services.AddTransient<MySqlConnection>(_ => new MySqlConnection(Configuration["AppSettings:SqlConnection:MySqlConnectionString"]));
            services.AddSingleton<IDataRolesService,DataRolesService>();
            services.AddSingleton<IJwtService,JwtService>();
            services.AddSingleton<UtilService>();
            services.AddSingleton<IAuthDao,AuthDao>();
            services.AddSingleton<ITymelineObjectDao,TymelineObjectDaoMySql>();
            services.AddScoped<ITymelineService, TymelineService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddControllers();
            
          
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tymeline.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tymeline.API v1"));
            }
            else if(env.IsProduction()){
            }
            // app.UseMiddleware<TestMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            // first Authentication then Authorization!

            

            app.UseAuthentication();
            app.UseAuthorization();
            
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
