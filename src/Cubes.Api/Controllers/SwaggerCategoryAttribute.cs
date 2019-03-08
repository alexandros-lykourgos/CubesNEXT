using System;

namespace Cubes.Api.Controllers
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class SwaggerCategoryAttribute : Attribute
    {
        public SwaggerCategoryAttribute(string category)
            => Category = category;

        public string Category { get; }
    }
}