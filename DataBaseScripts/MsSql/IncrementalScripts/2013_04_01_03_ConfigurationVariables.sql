delete from ConfigurationVariables where name = 'CAISPath'
GO
INSERT INTO ConfigurationVariables (Name, [Value], [Description]) VALUES ('CAISPath', '\\ezbob\c$\CAIS\', 'Path to CAIS reports folder')
GO

delete from ConfigurationVariables where name = 'CAISPath2'
GO
INSERT INTO ConfigurationVariables (Name, [Value], [Description]) VALUES ('CAISPath2', '\\ezbob\c$\CAISBACKUP\', 'Path to Backup of CAIS reports folder')