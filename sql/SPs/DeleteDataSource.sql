IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteDataSource]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteDataSource]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteDataSource] 
	(@pDataSourceId int,
  @pSignedData [nvarchar](max))
AS
BEGIN
	UPDATE DataSource_Sources
  SET 
     IsDeleted = Id,
     SignedDocumentDelete = @pSignedData
  WHERE DisplayName = (SELECT DisplayName FROM
  DataSource_Sources WHERE  Id = @pDataSourceId)
END
GO
