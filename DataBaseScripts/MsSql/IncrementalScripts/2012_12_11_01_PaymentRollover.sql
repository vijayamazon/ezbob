update PaymentRollover set Created = ISNULL(Created, GETDATE()) -- update nuulable to now()
go
alter table PaymentRollover alter column Created DateTime Not Null
go
alter table PaymentRollover add [Status] varchar(50)
go