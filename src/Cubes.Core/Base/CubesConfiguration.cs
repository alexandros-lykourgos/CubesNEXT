using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Cubes.Core.Base
{
    /// <summary>
    /// Exposes Cubes environment and settings information through <see cref="IConfiguration"/>.
    /// </summary>
    public class CubesConfiguration
    {
        public string Version                   { get; set; }
        public string RootFolder                { get; set; }
        public string AppsFolder                { get; set; }
        public string LogsFolder                { get; set; }
        public string SettingsFolder            { get; set; }
        public string StorageFolder             { get; set; }
        public string TempFolder                { get; set; }
        public string StaticContentFolder       { get; set; }
        public string BinariesFolder            { get; set; }

        public IEnumerable<string> SwaggerFiles { get; set; }
        public IEnumerable<string> AssembliesWithControllers { get; set; }
    }

    public static class CubesConfigurationExtensions
    {
        public static IConfigurationBuilder AddCubesConfigurationProvider(this IConfigurationBuilder configuration,
            ICubesEnvironment cubes)
        {
            var cubesConfig = new Dictionary<string, string>
                {
                    { $"{CubesConstants.Configuration_Section}:Version"            , cubes.GetEnvironmentInformation().FullVersion },
                    { $"{CubesConstants.Configuration_Section}:RootFolder"         , cubes.GetRootFolder() },
                    { $"{CubesConstants.Configuration_Section}:AppsFolder"         , cubes.GetAppsFolder() },
                    { $"{CubesConstants.Configuration_Section}:LogsFolder"         , cubes.GetFolder(CubesFolderKind.Logs) },
                    { $"{CubesConstants.Configuration_Section}:SettingsFolder"     , cubes.GetSettingsFolder() },
                    { $"{CubesConstants.Configuration_Section}:StorageFolder"      , cubes.GetStorageFolder() },
                    { $"{CubesConstants.Configuration_Section}:TempFolder"         , cubes.GetFolder(CubesFolderKind.Temp) },
                    { $"{CubesConstants.Configuration_Section}:StaticContentFolder", cubes.GetFolder(CubesFolderKind.Content) },
                    { $"{CubesConstants.Configuration_Section}:BinariesFolder"     , cubes.GetBinariesFolder() },
                };
            var swaggerFiles = cubes
                .GetSwaggerFiles()
                .Select((f, i) => new KeyValuePair<string, string>($"{CubesConstants.Configuration_Section}:SwaggerFiles:{i}", f))
                .ToList();
            foreach (var item in swaggerFiles)
                cubesConfig.Add(item.Key, item.Value);

            var allAssembliesWithControllers = cubes
                .GetActivatedApplications()
                .SelectMany(app => app.AssembliesWithControllers.ToList())
                .Select((f, i) => new KeyValuePair<string, string>($"{CubesConstants.Configuration_Section}:AssembliesWithControllers:{i}", f))
                .ToList();
            foreach (var item in allAssembliesWithControllers)
                cubesConfig.Add(item.Key, item.Value);

            configuration.AddInMemoryCollection(cubesConfig);
            return configuration;
        }

        public static CubesConfiguration GetCubesConfiguration(this IConfiguration configuration)
            => configuration.GetSection(CubesConstants.Configuration_Section).Get<CubesConfiguration>();
    }
}
