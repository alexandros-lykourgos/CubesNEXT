using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cubes.Core.Base;
using Cubes.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Cubes.Web.Swager
{
    public static class SwaggerHelpers
    {
        public static void AddCubesSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                var cubesConfig = configuration.GetCubesConfiguration();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cubes API Documentation", Version = "v1" });
                c.OperationFilter<SwaggerCategoryAsTagFilter>();
                c.OrderActionsBy(api =>
                {
                    var attr = ((ControllerActionDescriptor)api.ActionDescriptor)
                        .ControllerTypeInfo
                        .Assembly
                        .GetCustomAttribute(typeof(SwaggerCategoryAttribute)) as SwaggerCategoryAttribute;
                    var categ = attr?.Category.IfNullOrEmpty("Undefined");
                    var order = categ == "Core" ? 0 : 1;
                    var ctrl  = $"{api.ActionDescriptor.RouteValues["controller"]}_{api.HttpMethod}";

                    return $"{order}_{categ}_{ctrl}";
                });

                var swaggerFiles = cubesConfig
                    .SwaggerFiles
                    .Where(File.Exists)
                    .ToList();
                foreach (var xmlfile in swaggerFiles)
                    c.IncludeXmlComments(xmlfile);
            });
        }

        public static IApplicationBuilder UseCubesSwagger(this IApplicationBuilder app)
        {
            var swaggerUrl = "/docs/v1/swagger.json";
            app.UseSwagger(c => c.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.InjectStylesheet("/ui/swagger-css");
                c.DocExpansion(DocExpansion.List);
                c.SwaggerEndpoint(swaggerUrl, "Cubes API V1");
                c.RoutePrefix = "docs/api";

                c.DisplayRequestDuration();
                c.DocumentTitle = "Cubes API";
            });
            app.UseReDoc(c =>
            {
                c.RoutePrefix = "docs/redocs";
                c.SpecUrl = swaggerUrl;
            });

            return app;
        }
    }

    public class SwaggerCategoryAsTagFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attr = context
                .MethodInfo
                .DeclaringType
                .Assembly
                .GetCustomAttribute(typeof(SwaggerCategoryAttribute)) as SwaggerCategoryAttribute;
            var tag = attr?.Category;
            if (!String.IsNullOrEmpty(tag))
                operation.Tags = new List<OpenApiTag> {new OpenApiTag { Name = tag }};
        }
    }
}
