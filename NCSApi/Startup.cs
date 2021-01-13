using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.DataContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AutoMapper;
using NCSApi.Config;
using NCSApi.Core;
using NCSApi.Service;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using DataLayer;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;

namespace NCSApi
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

            DutyConfig _dutyConfig = new DutyConfig();

           // Settings _settings = new Settings();

           // Configuration.Bind("ConnectonString", _settings);
            Configuration.Bind(nameof(DutyConfig), _dutyConfig);


            services.AddSingleton(_dutyConfig);
            services.AddSingleton(new CustomContext());           
            services.AddAutoMapper(typeof(Startup));
            services.AddHttpClient<ICustomDutyClient, CustomDutyClientService>();
            // services.AddSingleton(_settings);
            //services.AddSingleton(new CustomContext());

            //services.AddDbContext<CustomContext>(opt =>
            //{
            //    opt.UseSqlServer(Configuration["ConnetionString:DefaultConnection"]);
            //});

            //services.Configure<Settings>(Configuration.GetSection("ConnectonString"));

            //services.AddCors();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Custom Duty api", Version = "v1" });

                //var security = new Dictionary<string, IEnumerable<string>>
                //{
                //    {"Bearer", new string[0] }
                //};
                //c.AddSecurityDefinition(name: "Bearer", new ApiKeyScheme {
                //    Description = "JWT Authorization header using bearer scheme",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});
            });
            services.AddControllers().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(
                options => { options.RouteTemplate = "swagger/{documentName}/swagger.json"; });

            // Enable middleware to serve swagger - ui(HTML, JS, CSS, etc.),
            //   specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "My API V1");
            });

            //app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Assessment_Attachment")),
                RequestPath = new PathString("/Assessment_Attachment")
            });
            app.UseCors(builder =>
            builder.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader()
            );
         

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
