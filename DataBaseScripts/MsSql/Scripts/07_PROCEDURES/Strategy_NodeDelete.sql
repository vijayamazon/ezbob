IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeDelete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeDelete]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Strategy_NodeDelete]
       @pNodeName NVARCHAR(max)
      ,@pSignedDocument NVARCHAR(max)
	  ,@pUserId int
AS
BEGIN
  update Strategy_Node set
    IsDeleted = NodeId,
    TerminationDate  = GETDATE(),
    SignedDocumentDelete = @pSignedDocument,
    DeleterUserId = @pUserId
  where upper([name]) = upper(@pNodeName);

END
GO
