IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExportSigningId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExportSigningId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExportSigningId] 
	(@pSignedDocumentId bigint,
   @pId int)
AS
BEGIN
	UPDATE Export_Results
SET SignedDocumentId = @pSignedDocumentId
WHERE Id = @pId
END
GO
