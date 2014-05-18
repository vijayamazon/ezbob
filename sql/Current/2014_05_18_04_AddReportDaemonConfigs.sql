IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReportDaemonDropboxCredentials')
 INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ReportDaemonDropboxCredentials', 'ThisValueShouldBeSetPerMachine', 'Report Daemon credentials to deliver reports into Dropbox')
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReportDaemonDropboxRootPath')
 INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ReportDaemonDropboxRootPath', '/Reports/Dev', 'Report Daemon root path to deliver reports into Dropbox')
END
ELSE
BEGIN
	IF @Environment = 'QA' OR @Environment = 'UAT'
	BEGIN
		IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReportDaemonDropboxRootPath')
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ReportDaemonDropboxRootPath', '/Reports/QA', 'Report Daemon root path to deliver reports into Dropbox')
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReportDaemonDropboxRootPath')
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ReportDaemonDropboxRootPath', '/Reports/Production', 'Report Daemon root path to deliver reports into Dropbox')
	END
END
GO
