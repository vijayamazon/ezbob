go
delete from [CaisReportsHistory]
go
exec sp_RENAME 'CaisReportsHistory.FilePath', 'DirName' , 'COLUMN'
go
alter table [CaisReportsHistory] add  FileData [nvarchar](max) NULL