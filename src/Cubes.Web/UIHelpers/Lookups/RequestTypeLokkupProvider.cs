using System;
using System.Collections.Generic;
using System.Linq;
using Cubes.Core.Commands;
using Cubes.Core.Utilities;

namespace Cubes.Web.UIHelpers.Lookups
{
    public class RequestTypeLokkupProvider : ILookupProvider
    {
        private static readonly string ProviderName = "RequestTypes";

        public string Name => ProviderName;

        public Lookup Get()
        {
            var types = GetRequestTypes();
            var typesList = types
                .Select(i =>
                {
                    var attribute = i.GetAttribute<DisplayAttribute>();
                    var display = attribute == null || String.IsNullOrEmpty(attribute.Name) ? i.Name : attribute.Name;

                    return new LookupItem
                    {
                        Value     = i.FullName,
                        Display   = display,
                        Group     = String.Empty,
                        OtherData = null
                    };
                })
                .ToList();
            return new Lookup
            {
                Name  = this.Name,
                Items = typesList
            };
        }

        private static IEnumerable<Type> GetRequestTypes()
        {
            var types = new List<Type>();
            var dlls = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var dll in dlls)
                types.AddRange(dll.GetTypes()
                    .Where(i => i.IsClass
                        && i.IsConcrete()
                        && i.BaseType != null
                        && i.BaseType.IsGenericType
                        && i.BaseType.GetGenericTypeDefinition() == typeof(Request<>))
                    .ToList()
                );

            return types;
        }
    }
}
