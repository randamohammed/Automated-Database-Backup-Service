using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CsutemBackupDatabase
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                DatabaseBackup databasePackup = new DatabaseBackup();
                databasePackup.runInConseole();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new DatabaseBackup()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
