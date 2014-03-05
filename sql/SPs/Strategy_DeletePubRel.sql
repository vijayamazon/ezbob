IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_DeletePubRel]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_DeletePubRel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_DeletePubRel] 
	(@pPublicId bigint,
    @pStrategyId bigint,
    @pAction nvarchar(7),
    @pUserId INT,
    @pSignedDocument ntext,
    @pData ntext,
    @pAllData ntext)
AS
BEGIN
	delete strategy_publicrel where publicid = @pPublicId and strategyid = @pStrategyId;
     
     update strategy_strategy
        set state = 0
      where strategyid = @pStrategyId;
     
     if Not (@pSignedDocument is null) begin
        execute Strategy_PublicSignInsert @pPublicId, @pAction, @pUserId, @pSignedDocument, @pData, @pStrategyId, @pAllData
     end
END
GO
