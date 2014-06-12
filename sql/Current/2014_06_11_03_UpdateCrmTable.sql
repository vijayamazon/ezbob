IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'Incoming' and Object_ID = Object_ID(N'CustomerRelations'))
BEGIN 
	EXECUTE('
	ALTER TABLE CustomerRelations ALTER COLUMN Incoming NVARCHAR(20)
	UPDATE CustomerRelations SET Incoming=''In'' WHERE Incoming=''1''
	UPDATE CustomerRelations SET Incoming=''Out'' WHERE Incoming=''0''')
	EXEC sp_RENAME 'CustomerRelations.Incoming', 'Type', 'COLUMN'
END 
GO
