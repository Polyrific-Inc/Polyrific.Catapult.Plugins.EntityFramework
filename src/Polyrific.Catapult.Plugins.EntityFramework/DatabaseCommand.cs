// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using Microsoft.Extensions.Logging;
using Polyrific.Catapult.Plugins.EntityFramework.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Polyrific.Catapult.Plugins.EntityFramework
{
    public class DatabaseCommand : IDatabaseCommand
    {
        private readonly ILogger _logger;

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public DatabaseCommand(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> Update(string dataProjectPath, string connectionString, string configuration = "Debug")
        {
            var error = await ExecuteMigrationScript(dataProjectPath, connectionString);

            if (!string.IsNullOrEmpty(error))
                return error;

            return "";
        }

        private async Task<string> ExecuteMigrationScript(string dataProjectPath, string connectionString, int attempts = 0)
        {
            var efMigrateFileLocation = Path.Combine(AssemblyDirectory, "Tools/migrate.exe");
            if (!File.Exists(efMigrateFileLocation))
            {
                return $"migrate.exe is not available in {AssemblyDirectory}/Tools/";
            }

            // Don't verbose so we can get exactly the error message
            var args = $"{Path.GetFileName(dataProjectPath)} /startUpDirectory:\"{Path.GetDirectoryName(dataProjectPath)}\" /startupConfigurationFile:\"{dataProjectPath}.config\" /connectionString=\"{connectionString}\" /connectionProviderName=\"System.Data.SqlClient\"";

            var result = await CommandHelper.Execute(efMigrateFileLocation, args, _logger);

            if (!string.IsNullOrEmpty(result.error))
                return result.error;

            if (result.output.Trim().StartsWith("ERROR") && result.output.Contains("transient failure") && attempts < 5)
            {
                _logger.LogError($"Transient error occured. Retrying in 30 seconds... (attempt no {attempts + 1})");
                System.Threading.Thread.Sleep(30000);
                return await ExecuteMigrationScript(dataProjectPath, connectionString, attempts + 1);
            }


            if (result.output.Trim().StartsWith("ERROR"))
            {
                return result.output;
            }

            return "";
        }
    }
}
