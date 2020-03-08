using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using TaskManager.Models;


namespace TaskManager
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
            services.AddScoped<Microsoft.EntityFrameworkCore.DbContext, TaskManager.Models.TaskManagerContext>();
            services.AddScoped<Models.ITaskManagerContext, Models.TaskManagerContext>();

            services.AddControllersWithViews();



            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);


            #region Enable Cookie Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.LoginPath = new PathString("/Users/Signin");
                            options.AccessDeniedPath = new PathString("/auth/Signin");
                        });//Simple auth method
            #endregion


            #region Enable JWT Authentication


            var tokenConfigurations = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                Configuration.GetSection("TokenConfigurations"))
                    .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);


            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;



            }).AddJwtBearer(bearerOptions =>
            {
                var validationParams = bearerOptions.TokenValidationParameters;
                validationParams.IssuerSigningKey = signingConfigurations.Key;
                validationParams.ValidAudience = tokenConfigurations.Audience;
                validationParams.ValidIssuer = tokenConfigurations.Issuer;




                //Validates the received token signature
                validationParams.ValidateIssuerSigningKey = true;

                //Checks whether token still valid
                validationParams.ValidateLifetime = true;

                //Tolerance time for token expiration
                validationParams.ClockSkew = TimeSpan.Zero;
            });

            //Enable JWT
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });




            #endregion

            #region Enable Views Edition while running
#if DEBUG
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
#endif

            #endregion


            #region Configure Swagger
            // Configure Swagger API Documentation Mechanism
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Task Manager App API Definition",
                    Version = "v1",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Email = "vitor.v.gomes@live.com",
                        Name = "Vitor Vinicius",
                        Url = new Uri("https://www.linkedin.com/in/vitorvgsilva/")
                    },
                    Description = "Use this documentation to integrate your system with the Task Manager application.",
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "Apache License Version 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0")
                    }
                });



                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT authorization header using the Carrier scheme. 
Type 'Bearer', a blank space and then 
Your accessToken obtained via the '/Users/Logon' route in the text box below.
e.g: 'Bearer LKJ2H4323llkjl23'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                        {
                          new OpenApiSecurityScheme
                          {
                            Reference = new OpenApiReference
                              {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                              },
                              Scheme = "oauth2",
                              Name = "Bearer",
                              In = ParameterLocation.Header,

                            },
                            new List<string>()
                          }
                 });


                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlFile);




            });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ITaskManagerContext>();
                ((TaskManagerContext)context).Database.EnsureCreated();
            }

            #region Enable Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Sample");
            });
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });




        }
    }
}
