IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SPNAME]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SPNAME]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SPNAME] 
	(@pSignedDocumentId bigint)
AS
BEGIN
	SELECT
        [signedData]
        ,[data]
        ,[outletName]
        ,[nodeId]
        ,[userName]
    FROM  [Application_NodeDataSign]
    WHERE id = @pSignedDocumentId;
END
GO
