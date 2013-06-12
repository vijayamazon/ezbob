go
delete from [CaisReportsHistory]
go
exec sp_RENAME 'CaisReportsHistory.FilePath', 'DirName' , 'COLUMN'
go
alter table [CaisReportsHistory] add  FileData [varbinary](max) NULL
go
update [ConfigurationVariables] set [Value] = 'c:\CAIS' where [Name] = 'CAISPath'
update [ConfigurationVariables] set [Value] = 'c:\CAISBACKUP' where [Name] = 'CAISPath2'