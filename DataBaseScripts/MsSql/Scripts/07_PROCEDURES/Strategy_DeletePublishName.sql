IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_DeletePublishName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_DeletePublishName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].Strategy_DeletePublishName
  @pId bigint
 ,@pSignedDocument NVARCHAR(max)
 ,@pUserId int
AS
BEGIN
   if @pId > 0 
   begin
      update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel where publicid = @pId);
     
     update strategy_publicname 
        set
            IsDeleted = @pId,
            TerminationDate  = GETDATE(),
            SignedDocumentDelete = @pSignedDocument,
            DeleterUserId = @pUserId
     where publicnameid = @pId;
   end; 

END
GO
