Create database $(DATABASE_NAME); 
GO
BEGIN TRY
	USE $(DATABASE_NAME)
	IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = N'SssServer')
	begin
		CREATE LOGIN SssServer
			WITH PASSWORD = 'sss';
		
		CREATE USER SssServer FOR LOGIN SssServer 
			WITH DEFAULT_SCHEMA = dbo;
		EXEC sp_addrolemember N'db_owner', N'SssServer';
	end
	    
	else 
	begin
		
		CREATE USER SssServer FOR LOGIN SssServer 
		    WITH DEFAULT_SCHEMA = dbo;
		EXEC sp_addrolemember N'db_owner', N'SssServer';
	end
end try
BEGIN CATCH
	
END CATCH
GO