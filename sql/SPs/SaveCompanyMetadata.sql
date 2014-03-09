IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveCompanyFileMetadata]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveCompanyFileMetadata]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE SaveCompanyFileMetadata
	@CustomerId INT,
	@Created DATETIME,
	@FileName NVARCHAR(300),
	@FilePath NVARCHAR(MAX),
	@FileContentType NVARCHAR(300)
AS
	INSERT INTO MP_CompanyFilesMetaData (CustomerId, Created, FileName, FilePath, FileContentType) VALUES (@CustomerId, @Created, @FileName, @FilePath, @FileContentType)
GO
