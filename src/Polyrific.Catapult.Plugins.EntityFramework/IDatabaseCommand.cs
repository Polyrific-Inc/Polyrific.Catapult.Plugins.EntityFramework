// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using System.Threading.Tasks;

namespace Polyrific.Catapult.Plugins.EntityFramework
{
    public interface IDatabaseCommand
    {
        /// <summary>
        /// Run the ef migrate db command
        /// </summary>
        /// <param name="dataProjectPath">The dll location of the data project</param>
        /// <param name="connectionString">The connection string to be used</param>
        /// <param name="configuration">The --configuration option</param>
        /// <returns></returns>
        Task<string> Update(string dataProject, string connectionString, string configuration = "Debug");
    }
}
