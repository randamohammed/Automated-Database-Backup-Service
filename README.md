# Database Backup Service

This project is an automated service for managing database backups, designed to ensure data safety and reliability. The service goes through several stages from configuration to deployment:

🔹 Features & Workflow

**Configuration**

Define the database connection string.
Specify the backup file name and target folder for storing backups.
Configure a log folder to record all activities.

**Backup Process**

Connects to the database and handles connection errors.
Creates a backup file with a timestamp in its name for easy sorting and management.

Records every step of the backup process in a dedicated log file.

**Backup Rotation & Cleanup**

Maintains only a specified number of backup files.

Automatically deletes older backups beyond the configured limit.

**Error Handling & Notifications**

Catches errors during backup or connection failures.

Sends an email notification with error details.

Email settings (SMTP, account, recipients) are stored in the configuration file.

**Deployment**

Built on .NET Framework and runs as a Windows Service.

Fully configurable through external config files without code changes.
