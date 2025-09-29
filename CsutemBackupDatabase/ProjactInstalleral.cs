using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace CsutemBackupDatabase
{
    [RunInstaller(true)]
    public partial class ProjactInstalleral :Installer
    {
        ServiceProcessInstaller ServiceProcessInstaller = new ServiceProcessInstaller();
        ServiceInstaller ServiceInstaller = new ServiceInstaller();
        public ProjactInstalleral()
        {
            InitializeComponent();

            ServiceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            ServiceInstaller = new ServiceInstaller
            {
                ServiceName = "DatabaseBackupService",
                DisplayName = "Csutem Backup Database",
                Description = "Take every daye Packup for database",
                ServicesDependedOn = new string[] { "MSSQLSERVER" , "VSS" , "RpcSs" , "EventLog" },
                StartType = ServiceStartMode.Automatic

            };
            Installers.Add(ServiceInstaller);
            Installers.Add(ServiceProcessInstaller);
        }


       

    }
}
