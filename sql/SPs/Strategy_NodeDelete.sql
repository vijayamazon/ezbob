IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeDelete]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeDelete]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_NodeDelete] 
	(@pNodeName NVARCHAR(max)
      ,@pSignedDocument NVARCHAR(max)
	  ,@pUserId int)
AS
BEGIN
	update Strategy_Node set
    IsDeleted = NodeId,
    TerminationDate  = GETDATE(),
    SignedDocumentDelete = @pSignedDocument,
    DeleterUserId = @pUserId
  where upper([name]) = upper(@pNodeName)
END
GO
