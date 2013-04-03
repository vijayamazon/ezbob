IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_NodeDataSign_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_NodeDataSign_Select]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_NodeDataSign_Select]
    @pSignedDocumentId bigint
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
