ALTER TABLE dbo.[Customer] ALTER COLUMN [RefNumber] NVARCHAR(8)
update  dbo.[Customer] set [RefNumber] = '01'+[RefNumber];

GO