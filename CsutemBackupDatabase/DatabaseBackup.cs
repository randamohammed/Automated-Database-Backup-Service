using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using System.Linq;

namespace CsutemBackupDatabase
{
    public partial class DatabaseBackup : ServiceBase
    {
        private string BackupFolder;
        private string ConnectionString;
        private string LogFolder;
        private int backupIntervalMinutes ;
        private string SendFrom;
        private string SendTo;
        private string Subject;
        private string Body;
        private string username;
        private string Password;
        private string smtpClients;
        private Timer backupTimer;
        private int NumKeepBackFail;
        public DatabaseBackup()
        {
            InitializeComponent();

            BackupFolder = ConfigurationManager.AppSettings["BackupFolder"];
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
            LogFolder = ConfigurationManager.AppSettings["LogFolder"];
            backupIntervalMinutes =Convert.ToInt32(ConfigurationManager.AppSettings["backupIntervalMinutes"])  * 60  *  1000;
            SendFrom = ConfigurationManager.AppSettings["SendFrom"];
            SendTo = ConfigurationManager.AppSettings["SendTo"];
            Subject = ConfigurationManager.AppSettings["Subject"];
            username = ConfigurationManager.AppSettings["username"];
            Password = ConfigurationManager.AppSettings["Password"];
            smtpClients = ConfigurationManager.AppSettings["smtpClients"];
            Body = ConfigurationManager.AppSettings["Body"];
            NumKeepBackFail =Convert.ToInt32( ConfigurationManager.AppSettings["Body"]);

            if(string.IsNullOrWhiteSpace(backupIntervalMinutes.ToString()) || backupIntervalMinutes < 0 )
            {
                backupIntervalMinutes = 60 ;
                Log($"Error in Backup Interval Minutes most be nagativ number I set defulte number: {backupIntervalMinutes}");
            }

            if (string.IsNullOrWhiteSpace(NumKeepBackFail.ToString()) || NumKeepBackFail < 0)
            {
                backupIntervalMinutes = 6;
                Log($"Num Keep Back Faile Minutes most be nagativ number I set defulte number: {NumKeepBackFail}");
            }

            if (string.IsNullOrWhiteSpace(BackupFolder))
            {
                BackupFolder = @"D:\DatabaseBackups";
                Log($"Folder Database Backups is massing in App.config I Using defulte: {BackupFolder}"  );
            }

            if(string.IsNullOrWhiteSpace(LogFolder))
            {
                LogFolder = @"D:\Databasebackups\Log";
                Log($"Log Folder Database Backups is massing in App.config I Using defulte: {LogFolder}");
            }

            if(string.IsNullOrEmpty(ConnectionString.ToString()) )
            {
                ConnectionString = "Server=.;Database=master;Trusted_Connection=True;";
                Log($"Connection string is massing in App.config I Using defulte: {ConnectionString}");
            }

            if( string.IsNullOrEmpty(Subject))
            {
                Subject = "Error in Backup Service";
                Log($"Subject contuint is massing. I defulte write {Subject}");
            }

            if (string.IsNullOrEmpty(Body))
            {
                Body = "An error occurred during backup";
                Log($"body contuint is massing. I defulte write {Body}");
            }

            if (string.IsNullOrEmpty(smtpClients))
            {
                smtpClients = "smtp.sendgrid.net";
                Log($"Type of smtp is massing. I defulte write {smtpClients}");
            }

            if(string.IsNullOrEmpty(Password))
            {
                username = "apikey";
                Log("username is massing. II defulte Using "+username );
            }
            Directory.CreateDirectory(BackupFolder);
            Directory.CreateDirectory (LogFolder);
        }
       
       
        private void PerformBackup()
        {
            string backupFile = Path.Combine(BackupFolder, "backup_" + DateTime.Now.ToString("yyyy MM dd_HH mm ss") + ".bak");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string Query = $@"BACKUP DATABASE {connection.Database} TO DISK = '{backupFile}'   WITH INIT;";
              
                using (SqlCommand command = new SqlCommand(Query, connection))
                {
                    try
                    {        
                        command.ExecuteNonQuery();
                        Log($"Database backup successful: {backupFile}");
                    }
                    catch (Exception ex)
                    {
                        Log("Error during backup: " + ex.Message);
                    }
                  
                }
              
            }

        }
        
        private void SendErrorEmail(string errorMessage)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();


                mailMessage.From = new MailAddress(SendFrom); // نفس الإيميل اللي عملت له Verify
                mailMessage.To.Add(SendTo);
                mailMessage.Subject = Subject;
                mailMessage.Body = Body + errorMessage;

                SmtpClient smtpClient = new SmtpClient(smtpClients, 587);
                smtpClient.Credentials = new System.Net.NetworkCredential(username, Password);
                smtpClient.EnableSsl = true;
                smtpClient.Send(mailMessage);
                Log("Send message to email sucssfuly");
            }
            catch (Exception ex)
            {
                Log("Database backup failed: " + errorMessage);
            }
        }

        public void DoBackup(object state)
        {
            try
            {

                PerformBackup();
                CleanOldBackups(BackupFolder);


                throw new Exception("sumited the datatbase backup");
            }
            catch (Exception ex)
            {
                SendErrorEmail(ex.Message);
                Log("Error during backup: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private void Log(string message)
        {
            string logepath =Path.Combine(LogFolder, "Log.txt");
            string logMessage = $"[{DateTime.Now:yyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logepath, logMessage + Environment.NewLine);

            if(Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }
        }

        private void CleanOldBackups(string backupdi)
        {

            var files = new DirectoryInfo(backupdi).GetFiles("*.bak").OrderByDescending(f => f.CreationTime).ToList();

            if (files.Count > NumKeepBackFail)
            {
                foreach (var file in files.Skip(NumKeepBackFail))
                {
                    try
                    {
                        file.Delete();
                        Log($"Deleted old backup: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Error deleting old {file.Name} : {ex.Message}");
                    }
                }
            }
        }
        protected override void OnStart(string[] args)
        {
            Log("Service started...");

            backupTimer = new Timer(
                callback: DoBackup,
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMinutes(backupIntervalMinutes)

                );
           

            Log($"Backup schedule initiated: every {backupIntervalMinutes} minute(s).");

        }

        protected override void OnStop()
        {
           Log("Service Stop...");
           backupTimer.Dispose();
           
        }

     
     public void runInConseole()
     {
     if (Environment.UserInteractive)
     {
         Console.WriteLine("Service run in console");
         OnStart(null);
         Console.WriteLine("press Enter to stop service...");
         Console.ReadLine();
         OnStop();
     }
        }
    }
}
