IF OBJECT_ID(N'[dbo].[SaveCompanyFileMetadata]') IS NULL
BEGIN
	 EXECUTE('CREATE PROCEDURE SaveCompanyFileMetadata AS SELECT 1')
END 
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveCompanyFileMetadata
	@CustomerId INT,
	@Created DATETIME,
	@FileName NVARCHAR(300),
	@FilePath NVARCHAR(MAX),
	@FileContentType NVARCHAR(300),
	@IsBankStatement BIT
AS
	INSERT INTO MP_CompanyFilesMetaData (CustomerId, Created, FileName, FilePath, FileContentType, IsBankStatement) 
	VALUES (@CustomerId, @Created, @FileName, @FilePath, @FileContentType, @IsBankStatement)

GO