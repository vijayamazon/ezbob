IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyFileMetadata]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyFileMetadata]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetCompanyFileMetadata
	@CompanyFileId INT
AS
	SELECT FilePath AS FilePath FROM MP_CompanyFilesMetaData WHERE Id=@CompanyFileId
GO